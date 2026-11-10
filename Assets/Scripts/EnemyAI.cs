using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private Transform _player;
    [SerializeField] private string _clipPrefix = "TQ_Skeleton_";
    [SerializeField] private string _attackState;
    [SerializeField] private string _walkState;
    [SerializeField] private bool _chase = true;
    [SerializeField, Min(0f)] private float _attackRange = 0.3f;
    [SerializeField, Min(0f)] private float _moveSpeed = 2.5f;
    [SerializeField, Min(0f)] private float _attackRecovery = 0.8f;
    [SerializeField] private int _damage = 15;
    [SerializeField] private float _hitStart = 0.45f;
    [SerializeField] private float _hitEnd = 0.9f;

    private EnemyController _enemy;
    private UndeadOrbit _orbit;
    private Animator _animator;
    private SpriteRenderer _renderer;
    private Rigidbody2D _body;
    private Collider2D _bodyCollider;
    private BoxCollider2D _hitbox;
    private EnemyAttackHitbox _sword;
    private bool _attacking;
    private bool _hitActive;
    private bool _landedHit;
    private float _attackStart;
    private float _nextAttackTime;
    private string _attackClip;
    private string _currentClip;
    private Vector2 _swingDirection;
    private Vector2 _move;
    private bool _huntsGhost;
    private int _ghostSlot;
    private GhostCollector _collector;
    private const float _ghostReach = 0.65f;
    private const float _separateDistance = 0.75f;
    private const float _laneDistance = 2f;
    private static readonly List<EnemyAI> _enemies = new();
    private Vector2 _laneDirection = Vector2.down;

    public void Initialize(Transform player)
    {
        float angle = Mathf.Abs(GetInstanceID() % 8) * Mathf.PI * 2f / 8f;
        Initialize(player, false, 0, new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
    }

    public void Initialize(Transform player, bool huntsGhost, int ghostSlot)
    {
        Initialize(player, huntsGhost, ghostSlot, _laneDirection);
    }

    public void Initialize(Transform player, bool huntsGhost, int ghostSlot, Vector2 approach)
    {
        _player = player;
        _huntsGhost = huntsGhost;
        _ghostSlot = ghostSlot;
        _collector = player != null ? player.GetComponent<GhostCollector>() : null;
        if (approach.sqrMagnitude > 0.0001f)
            _laneDirection = approach.normalized;
    }

    public void SetCombat(float moveSpeed, int damage)
    {
        _moveSpeed = Mathf.Max(0f, moveSpeed);
        _damage = Mathf.Max(0, damage);
    }

    public bool HuntsGhost => _huntsGhost;
    public bool CanStealGhost => !_attacking && Time.time >= _nextAttackTime;

    public void Swing()
    {
        if (_attacking)
            return;

        Transform prey = Prey();
        if (prey == null)
            return;

        Vector2 toPrey = prey.position - transform.position;
        Vector2 facing = toPrey.sqrMagnitude > 0.0001f
            ? DominantDirection(toPrey)
            : Vector2.down;
        BeginAttack(facing);
    }

    private Transform Prey()
    {
        if (_huntsGhost && _collector != null)
        {
            GhostFollower ghost = _collector.GhostFromTail(_ghostSlot);
            if (ghost != null)
                return ghost.transform;
        }

        return _player;
    }

    public void Hit(PlayerHealth player)
    {
        if (_landedHit || player == null)
            return;

        _landedHit = true;
        int damage = _enemy != null && _enemy.AttackDamage > 0 ? _enemy.AttackDamage : _damage;
        player.TakeDamage(damage);
    }

    private void Awake()
    {
        _enemy = GetComponent<EnemyController>();
        _bodyCollider = GetComponent<Collider2D>();
        _orbit = GetComponent<UndeadOrbit>();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        _body = GetComponent<Rigidbody2D>();

        GameObject hitObject = new GameObject("SwordHitbox");
        hitObject.transform.SetParent(transform, false);
        _hitbox = hitObject.AddComponent<BoxCollider2D>();
        _hitbox.isTrigger = true;
        _hitbox.enabled = false;
        _sword = hitObject.AddComponent<EnemyAttackHitbox>();
        _sword.SetOwner(this);
    }

    private void OnEnable()
    {
        _attacking = false;
        _landedHit = false;
        _currentClip = null;
        _move = Vector2.zero;
        if (_animator != null)
            _animator.speed = 1f;
        _huntsGhost = false;
        _ghostSlot = 0;
        _nextAttackTime = 0f;
        EndHit();
        if (!_enemies.Contains(this))
            _enemies.Add(this);
    }

    private void OnDisable()
    {
        _enemies.Remove(this);
        EndHit();
    }

    private void Update()
    {
        if (_enemy != null && _enemy.IsDead)
        {
            _attacking = false;
            _move = Vector2.zero;
            EndHit();
            if (_animator != null)
                _animator.speed = 1f;
            return;
        }

        if (_attacking && !PlayerPower.IsActive)
        {
            _move = Vector2.zero;
            UpdateSwing();
            return;
        }

        if (_attacking)
            EndSwing();

        if (_player == null || _animator == null)
        {
            _move = Vector2.zero;
            return;
        }

        Transform prey = Prey();
        if (prey == null)
        {
            _move = Vector2.zero;
            return;
        }

        Vector2 toPrey = prey.position - transform.position;
        bool huntingLine = prey != _player;
        float reach = huntingLine ? _ghostReach : _attackRange;
        bool inRange = toPrey.sqrMagnitude <= reach * reach;
        if (huntingLine && !inRange && prey.TryGetComponent(out GhostFollower nearGhost) && Touching(nearGhost))
            inRange = true;

        Vector2 destination = prey.position;
        if (!huntingLine && !IsClosestChaser())
            destination = (Vector2)_player.position + _laneDirection * _laneDistance;
        else if (huntingLine && toPrey.magnitude > reach + 0.6f)
            destination = (Vector2)prey.position + TailOffset(toPrey);

        Vector2 toDestination = destination - (Vector2)transform.position;
        Vector2 facing = DominantDirection(toPrey);

        if (_chase && _orbit != null)
            _orbit.enabled = false;

        if (huntingLine && inRange && !PlayerPower.IsActive)
        {
            _move = Vector2.zero;
            if (CanStealGhost
                && prey.TryGetComponent(out GhostFollower ghost)
                && _collector != null
                && _collector.Steal(ghost))
                Swing();
            return;
        }

        Vector2 push = Separation();
        bool crowded = push.sqrMagnitude > 0.0001f;

        if (_chase && (!inRange || crowded))
        {
            Vector2 direction = inRange || toDestination.sqrMagnitude < 0.0001f
                ? push
                : toDestination.normalized + push;
            if (direction.sqrMagnitude < 0.0001f)
            {
                _move = Vector2.zero;
                FaceAndWalk(facing);
                return;
            }

            _move = direction.normalized * _moveSpeed;
            FaceAndWalk(facing);
            return;
        }

        if (PlayerPower.IsActive)
        {
            if (!_chase)
            {
                _move = Vector2.zero;
                return;
            }

            Vector2 direction = toDestination.sqrMagnitude > 0.0001f ? toDestination.normalized + push : push;
            if (direction.sqrMagnitude < 0.0001f)
                direction = toDestination.sqrMagnitude > 0.0001f ? toDestination : Vector2.down;

            _move = direction.normalized * _moveSpeed;
            FaceAndWalk(facing);
            return;
        }

        _move = Vector2.zero;
        if (!inRange || Time.time < _nextAttackTime)
            return;

        BeginAttack(facing);
    }

    private void FixedUpdate()
    {
        if (_body == null || _move.sqrMagnitude <= 0f)
            return;

        _body.MovePosition(_body.position + _move * Time.fixedDeltaTime);
    }

    private bool IsClosestChaser()
    {
        if (_player == null)
            return true;

        Vector2 player = _player.position;
        float myDistance = (player - (Vector2)transform.position).sqrMagnitude;
        int myId = GetInstanceID();

        for (int i = 0; i < _enemies.Count; i++)
        {
            EnemyAI other = _enemies[i];
            if (other == null || other == this || !other.isActiveAndEnabled)
                continue;

            if (other._enemy != null && other._enemy.IsDead)
                continue;

            if (other.Prey() != _player)
                continue;

            float distance = (player - (Vector2)other.transform.position).sqrMagnitude;
            if (distance < myDistance)
                return false;

            if (Mathf.Abs(distance - myDistance) < 0.0001f && other.GetInstanceID() < myId)
                return false;
        }

        return true;
    }

    private Vector2 TailOffset(Vector2 toPrey)
    {
        Vector2 side = new Vector2(-toPrey.y, toPrey.x);
        if (side.sqrMagnitude < 0.0001f)
            return Vector2.zero;

        side.Normalize();
        if ((_ghostSlot & 1) == 1)
            side = -side;

        return side * 0.75f;
    }

    private Vector2 Separation()
    {
        Vector2 push = Vector2.zero;
        AccumulateSeparation(transform.position, GetInstanceID(), _separateDistance, ref push);
        EnemyGhost.AccumulateSeparation(transform.position, GetInstanceID(), _separateDistance, ref push);
        return push;
    }

    public static void AccumulateSeparation(Vector2 position, int selfId, float separateDistance, ref Vector2 push)
    {
        for (int i = 0; i < _enemies.Count; i++)
        {
            EnemyAI other = _enemies[i];
            if (other == null || other.GetInstanceID() == selfId || !other.isActiveAndEnabled)
                continue;

            if (other._enemy != null && other._enemy.IsDead)
                continue;

            AddPush(position, other.transform.position, selfId, other.GetInstanceID(), separateDistance, ref push);
        }
    }

    public static void AddPush(Vector2 position, Vector2 otherPosition, int selfId, int otherId, float separateDistance, ref Vector2 push)
    {
        Vector2 away = position - otherPosition;
        float distance = away.magnitude;
        if (distance >= separateDistance)
            return;

        if (distance < 0.001f)
            away = new Vector2(otherId > selfId ? 1f : -1f, 0f);
        else
            away /= distance;

        push += away * ((separateDistance - Mathf.Max(distance, 0.001f)) / separateDistance);
    }

    private bool Touching(GhostFollower ghost)
    {
        if (_bodyCollider == null || ghost == null)
            return false;

        Collider2D other = ghost.GetComponent<Collider2D>();
        return other != null && _bodyCollider.IsTouching(other);
    }

    private void FaceAndWalk(Vector2 facing)
    {
        if (_renderer != null)
        {
            if (!string.IsNullOrEmpty(_walkState) && Mathf.Abs(facing.x) > 0.01f)
                _renderer.flipX = facing.x < 0f;
            else
                _renderer.flipX = false;
        }

        string walk = string.IsNullOrEmpty(_walkState)
            ? _clipPrefix + "Walk_" + DirectionName(facing)
            : _walkState;
        PlayIfChanged(walk);
    }

    private void BeginAttack(Vector2 direction)
    {
        _swingDirection = direction;
        _attackClip = string.IsNullOrEmpty(_attackState)
            ? _clipPrefix + "Attack_" + DirectionName(direction)
            : _attackState;

        if (_renderer != null)
        {
            if (!string.IsNullOrEmpty(_attackState) && Mathf.Abs(direction.x) > 0.01f)
                _renderer.flipX = direction.x < 0f;
            else
                _renderer.flipX = false;
        }

        if (_orbit != null)
            _orbit.enabled = false;

        _animator.Play(_attackClip, 0, 0f);
        _currentClip = _attackClip;
        _attacking = true;
        if (_enemy != null)
            _enemy.Growl();
        _attackStart = Time.time;
        _landedHit = false;
    }

    private void UpdateSwing()
    {
        float elapsed = Time.time - _attackStart;
        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        bool clipFinished = elapsed > 0.05f
            && info.IsName(_attackClip)
            && (info.normalizedTime >= 1f || (info.length > 0f && elapsed >= info.length));
        if (clipFinished || elapsed >= 3f)
        {
            EndSwing();
            return;
        }

        if (elapsed < _hitStart || elapsed >= _hitEnd)
        {
            EndHit();
            return;
        }

        PlaceHitbox(_swingDirection);
        if (_hitActive)
            return;

        _hitActive = true;
        _sword.Arm();
        _hitbox.enabled = true;
        CatchPlayerAlreadyInside();
    }

    private void CatchPlayerAlreadyInside()
    {
        Vector2 center = transform.TransformPoint(_hitbox.offset);
        Vector2 size = Vector2.Scale(_hitbox.size, transform.lossyScale);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null || hit == _hitbox || hit.transform.IsChildOf(transform))
                continue;

            if (!hit.TryGetComponent(out PlayerHealth player))
                continue;

            Hit(player);
            break;
        }
    }

    private void EndSwing()
    {
        _attacking = false;
        _currentClip = null;
        _nextAttackTime = Time.time + _attackRecovery;
        EndHit();

        if (_enemy != null && _enemy.IsDead)
            return;

        if (_orbit != null && !_chase)
            _orbit.enabled = true;

        if (_enemy != null)
            _enemy.PlayLive();
    }

    private void PlayIfChanged(string clip)
    {
        if (clip == _currentClip || _animator == null)
            return;

        _animator.Play(clip);
        _currentClip = clip;
    }

    private void EndHit()
    {
        if (_sword != null)
            _sword.Disarm();

        _hitActive = false;
        if (_hitbox != null)
            _hitbox.enabled = false;
    }

    private void PlaceHitbox(Vector2 direction)
    {
        _hitbox.offset = direction * (_attackRange * 0.5f);
        _hitbox.size = new Vector2(
            Mathf.Abs(direction.x) > 0.01f ? _attackRange : 0.5f,
            Mathf.Abs(direction.y) > 0.01f ? _attackRange : 0.5f);
    }

    private static Vector2 DominantDirection(Vector2 move)
    {
        if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            return move.x < 0f ? Vector2.left : Vector2.right;

        return move.y < 0f ? Vector2.down : Vector2.up;
    }

    private static string DirectionName(Vector2 direction)
    {
        if (direction == Vector2.up)
            return "Up";
        if (direction == Vector2.left)
            return "Left";
        if (direction == Vector2.right)
            return "Right";

        return "Down";
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.45f, 0.2f, 0.9f);
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        if (_hitbox == null || !_hitbox.enabled)
            return;

        Gizmos.color = new Color(1f, 0.25f, 0.2f, 0.9f);
        Vector3 center = transform.TransformPoint(_hitbox.offset);
        Vector3 size = Vector3.Scale(_hitbox.size, transform.lossyScale);
        Gizmos.DrawWireCube(center, size);
    }
}
