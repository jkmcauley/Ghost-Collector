using System.Collections.Generic;
using UnityEngine;

public class EnemyGhost : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 1f;
    [SerializeField] private float _patrolDistance = 0.75f;
    [SerializeField] private float _attackRange = 1f;
    [SerializeField] private string _idleTrigger = "EnemyGhostIdle";
    [SerializeField] private string _walkTrigger = "EnemyGhostWalk";
    [SerializeField] private string _attackTrigger = "EnemyGhostAttack";
    [SerializeField] private string _hitTrigger = "EnemyGhostHit";
    [SerializeField] private string _deadTrigger = "EnemyGhostDead";
    [SerializeField] private float _hitLock = 0.4f;

    private EnemyController _enemy;
    private Ghost _ghost;
    private Animator _animator;
    private SpriteRenderer _renderer;
    private BoxCollider2D _sword;
    private Transform _player;
    private Vector3 _spawnPoint;
    private Vector3 _patrolDirection = Vector3.right;
    private int _patrolSign = 1;
    private Vector3 _lastPosition;
    private string _currentTrigger;
    private float _hitUntil;
    private int _armedWindow = -1;
    private bool _swordHit;
    private static readonly List<EnemyGhost> _ghosts = new();

    private void Awake()
    {
        _enemy = GetComponent<EnemyController>();
        _ghost = GetComponent<Ghost>();
        if (_ghost != null)
            _ghost.Remain();
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();

        GameObject hitObject = new GameObject("SwordHitbox");
        hitObject.transform.SetParent(transform, false);
        _sword = hitObject.AddComponent<BoxCollider2D>();
        _sword.isTrigger = true;
        _sword.enabled = false;
        EnemyGhostHitbox sensor = hitObject.AddComponent<EnemyGhostHitbox>();
        sensor.SetOwner(this);
    }

    private void OnEnable()
    {
        _currentTrigger = null;
        _hitUntil = 0f;
        _armedWindow = -1;
        _swordHit = false;
        _lastPosition = transform.position;
        _spawnPoint = transform.position;
        _patrolDirection = Random.value < 0.5f ? Vector3.right : Vector3.up;
        _patrolSign = 1;
        _player = null;
        DisarmSword();
        ResetTriggers();
        if (!_ghosts.Contains(this))
            _ghosts.Add(this);

        if (_enemy != null)
        {
            _enemy.Hurt += OnHurt;
            _enemy.Killed += OnKilled;
        }
    }

    private void OnDisable()
    {
        _ghosts.Remove(this);

        if (_enemy == null)
            return;

        _enemy.Hurt -= OnHurt;
        _enemy.Killed -= OnKilled;
    }

    private void LateUpdate()
    {
        if (_enemy != null && _enemy.IsDead)
        {
            DisarmSword();
            return;
        }

        if (_player == null)
            ResolvePlayer();

        if (Time.time < _hitUntil)
        {
            DisarmSword();
            FacePlayer();
            return;
        }

        if (PlayerInRange())
        {
            FaceSword();
            Play(_attackTrigger);
            _lastPosition = transform.position;
            UpdateSword();
            return;
        }

        DisarmSword();
        Patrol();
        FacePlayer();
        bool moving = ((Vector2)transform.position - (Vector2)_lastPosition).sqrMagnitude > 0.000001f;
        _lastPosition = transform.position;
        if (moving)
            FacePlayer();
        Play(moving ? _walkTrigger : _idleTrigger);
    }

    private void Patrol()
    {
        Vector3 target = _spawnPoint + _patrolDirection * (_patrolDistance * _patrolSign);
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            _moveSpeed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude > 0.0004f)
            return;

        _patrolSign = -_patrolSign;
    }

    public static void AccumulateSeparation(Vector2 position, int selfId, float separateDistance, ref Vector2 push)
    {
        for (int i = 0; i < _ghosts.Count; i++)
        {
            EnemyGhost other = _ghosts[i];
            if (other == null || other.GetInstanceID() == selfId || !other.isActiveAndEnabled)
                continue;

            if (other._enemy != null && other._enemy.IsDead)
                continue;

            EnemyAI.AddPush(position, other.transform.position, selfId, other.GetInstanceID(), separateDistance, ref push);
        }
    }

    private void FacePlayer()
    {
        if (_renderer == null || _player == null)
            return;

        float dx = _player.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f)
            return;

        _renderer.flipX = dx < 0f;
    }

    public void Strike(Collider2D other)
    {
        if (_sword == null || !_sword.enabled || _swordHit || _enemy == null)
            return;

        if (other == null || other.transform.IsChildOf(transform))
            return;

        if (!other.TryGetComponent(out PlayerHealth player))
            return;

        _enemy.TryDamage(player);
        _swordHit = true;
    }

    private bool PlayerInRange()
    {
        if (_player == null)
            return false;

        Vector2 offset = (Vector2)_player.position - (Vector2)transform.position;
        return offset.sqrMagnitude <= _attackRange * _attackRange;
    }

    private void FaceSword()
    {
        if (_renderer == null || _player == null)
            return;

        float dx = _player.position.x - transform.position.x;
        if (Mathf.Abs(dx) < 0.01f)
            return;

        // The attack art holds the blade on the left, so flip it when you are on the right.
        _renderer.flipX = dx > 0f;
    }

    private void UpdateSword()
    {
        if (_sword == null || _animator == null || _player == null)
            return;

        AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
        if (!info.IsName(_attackTrigger))
        {
            DisarmSword();
            return;
        }

        int window = StrikeWindow(info.normalizedTime % 1f);
        if (window < 0)
        {
            DisarmSword();
            return;
        }

        PlaceSword();
        if (_armedWindow != window)
        {
            _armedWindow = window;
            _swordHit = false;
            _sword.enabled = true;
        }

        if (!_swordHit)
            CatchPlayer();
    }

    // Attack clip is 2 seconds. The blade is out on frames 2-4 and 7-9.
    private static int StrikeWindow(float normalizedTime)
    {
        if (normalizedTime >= 0.2f && normalizedTime < 0.5f)
            return 0;

        if (normalizedTime >= 0.7f && normalizedTime < 0.95f)
            return 1;

        return -1;
    }

    private void PlaceSword()
    {
        Vector2 toPlayer = (Vector2)_player.position - (Vector2)transform.position;
        Vector2 facing = DominantDirection(toPlayer);
        Vector3 scale = transform.lossyScale;
        float sx = Mathf.Max(0.001f, Mathf.Abs(scale.x));
        float sy = Mathf.Max(0.001f, Mathf.Abs(scale.y));
        Vector2 worldOffset = facing * (_attackRange * 0.45f);
        _sword.offset = new Vector2(worldOffset.x / sx, worldOffset.y / sy);

        const float thickness = 0.55f;
        Vector2 worldSize = new Vector2(
            Mathf.Abs(facing.x) > 0.01f ? _attackRange : thickness,
            Mathf.Abs(facing.y) > 0.01f ? _attackRange : thickness);
        _sword.size = new Vector2(worldSize.x / sx, worldSize.y / sy);
    }

    private void CatchPlayer()
    {
        Vector2 center = transform.TransformPoint(_sword.offset);
        Vector2 size = Vector2.Scale(_sword.size, transform.lossyScale);
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, 0f);

        for (int i = 0; i < hits.Length; i++)
            Strike(hits[i]);
    }

    private void DisarmSword()
    {
        _armedWindow = -1;
        _swordHit = false;
        if (_sword != null)
            _sword.enabled = false;
    }

    private static Vector2 DominantDirection(Vector2 move)
    {
        if (move.sqrMagnitude < 0.0001f)
            return Vector2.down;

        if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            return move.x < 0f ? Vector2.left : Vector2.right;

        return move.y < 0f ? Vector2.down : Vector2.up;
    }

    private void ResolvePlayer()
    {
        PlayerHealth health = FindAnyObjectByType<PlayerHealth>();
        if (health != null)
            _player = health.transform;
    }

    private void OnHurt()
    {
        if (_enemy != null && _enemy.IsDead)
            return;

        _hitUntil = Time.time + _hitLock;
        Play(_hitTrigger);
    }

    private void OnKilled()
    {
        _hitUntil = float.PositiveInfinity;
        DisarmSword();
        if (_ghost != null)
            _ghost.StopWandering();

        _currentTrigger = _deadTrigger;
        ResetTriggers();
    }

    private void Play(string trigger)
    {
        if (_animator == null || string.IsNullOrEmpty(trigger) || trigger == _currentTrigger)
            return;

        ResetTriggers();
        _animator.SetTrigger(trigger);
        _currentTrigger = trigger;
    }

    private void ResetTriggers()
    {
        if (_animator == null)
            return;

        _animator.ResetTrigger(_idleTrigger);
        _animator.ResetTrigger(_walkTrigger);
        _animator.ResetTrigger(_attackTrigger);
        _animator.ResetTrigger(_hitTrigger);
        _animator.ResetTrigger(_deadTrigger);
    }

    private void OnDrawGizmosSelected()
    {
        if (_sword == null || !_sword.enabled)
            return;

        Gizmos.color = new Color(1f, 0.25f, 0.2f, 0.9f);
        Vector3 center = transform.TransformPoint(_sword.offset);
        Vector3 size = Vector3.Scale(_sword.size, transform.lossyScale);
        Gizmos.DrawWireCube(center, size);
    }
}

public class EnemyGhostHitbox : MonoBehaviour
{
    private EnemyGhost _owner;

    public void SetOwner(EnemyGhost owner)
    {
        _owner = owner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_owner != null)
            _owner.Strike(other);
    }
}
