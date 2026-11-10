using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private float _attackCooldown = 0.4f;
    [SerializeField] private int _damage = 25;
    [SerializeField] private float _hitStart = 0.45f;
    [SerializeField] private float _hitEnd = 0.75f;
    [SerializeField] private float _swingSoundGap = 0.5f;

    private PlayerMovement _movement;
    private BoxCollider2D _hitbox;
    private SwordHitbox _sword;
    private bool _hitActive;
    private bool _dead;
    private float _nextAttackTime;
    private Coroutine _swingSounds;

    public void SetAlive(bool alive)
    {
        _dead = !alive;
        if (!alive)
        {
            EndHit();
            StopSwingSounds();
        }
    }

    public void Hit(EnemyController enemy)
    {
        enemy.TakeDamage(_damage);
    }

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();

        GameObject hitObject = new GameObject("SwordHitbox");
        hitObject.transform.SetParent(transform, false);
        _hitbox = hitObject.AddComponent<BoxCollider2D>();
        _hitbox.isTrigger = true;
        _hitbox.enabled = false;
        _sword = hitObject.AddComponent<SwordHitbox>();
        _sword.SetCombat(this);
    }

    private void OnEnable()
    {
        _input.AttackPressed += Attack;
    }

    private void OnDisable()
    {
        _input.AttackPressed -= Attack;
        EndHit();
        StopSwingSounds();
    }

    private void Attack()
    {
        if (_dead || Time.time < _nextAttackTime)
            return;

        if (!_movement.PlayAttack())
            return;

        PlaySwingSounds();
        _nextAttackTime = Time.time + _attackCooldown;
    }

    private void PlaySwingSounds()
    {
        StopSwingSounds();
        _swingSounds = StartCoroutine(SwingSounds());
    }

    private IEnumerator SwingSounds()
    {
        for (int i = 0; i < 3; i++)
        {
            if (i > 0)
                yield return new WaitForSeconds(_swingSoundGap);

            if (_dead)
                break;

            PlaySword();
        }

        _swingSounds = null;
    }

    private void StopSwingSounds()
    {
        if (_swingSounds == null)
            return;

        StopCoroutine(_swingSounds);
        _swingSounds = null;
    }

    private static void PlaySword()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySword();
    }

    private void Update()
    {
        if (_dead || _movement == null || !_movement.IsAttacking)
        {
            EndHit();
            return;
        }

        float elapsed = _movement.AttackElapsed;
        if (elapsed < _hitStart || elapsed >= _hitEnd)
        {
            EndHit();
            return;
        }

        PlaceHitbox(_movement.SwingDirection);

        if (_hitActive)
            return;

        _hitActive = true;
        _sword.Arm();
        _hitbox.enabled = true;
    }

    private void PlaceHitbox(Vector2 direction)
    {
        if (direction == Vector2.up)
        {
            _hitbox.offset = new Vector2(-0.055f, 0.281f);
            _hitbox.size = new Vector2(0.656f, 0.609f);
        }
        else if (direction == Vector2.left)
        {
            _hitbox.offset = new Vector2(-0.141f, 0.109f);
            _hitbox.size = new Vector2(0.953f, 0.234f);
        }
        else if (direction == Vector2.right)
        {
            _hitbox.offset = new Vector2(0.125f, 0.109f);
            _hitbox.size = new Vector2(0.953f, 0.234f);
        }
        else
        {
            _hitbox.offset = new Vector2(-0.047f, -0.047f);
            _hitbox.size = new Vector2(0.641f, 0.609f);
        }
    }

    private void EndHit()
    {
        if (_sword != null)
            _sword.Disarm();

        _hitActive = false;
        if (_hitbox != null)
            _hitbox.enabled = false;
    }

    private void OnDrawGizmos()
    {
        if (_hitbox == null || !_hitbox.enabled)
            return;

        Gizmos.color = new Color(1f, 0.25f, 0.2f, 0.9f);
        Vector3 center = transform.TransformPoint(_hitbox.offset);
        Vector3 size = Vector3.Scale(_hitbox.size, transform.lossyScale);
        Gizmos.DrawWireCube(center, size);
    }
}
