using System;
using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Core Stats")]
    [SerializeField] private EnemyData _enemyData;
    [SerializeField] private int _health = 100;
    [SerializeField] private string _liveState = "undead walk";
    [SerializeField] private string _deathState = "undead death";
    [SerializeField] private bool _returnToPool;
    [SerializeField] private float _respawnDelay = 3f;
    [SerializeField] private bool _returnToPoolAfterDeath;
    [SerializeField] private int _killPoints = 25;
    [SerializeField] private bool _damagesOnContact;
    [SerializeField] private float _contactCooldown = 0.6f;

    private int _currentHealth;
    private float _currentSpeed;
    private int _attackDamage;
    private int _ghostsLost = 1;
    private bool _dead;
    private float _nextGrowlTime;
    private float _nextContactTime;

    public event Action Hurt;
    public event Action Killed;

    public bool IsDead => _dead;
    public float MoveSpeed => _currentSpeed;
    public int AttackDamage => _attackDamage;
    public int GhostsLostPerContact => _ghostsLost;
    private Animator _animator;
    private SpriteRenderer _renderer;
    private Collider2D _collider;
    private UndeadOrbit _orbit;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
        _collider = GetComponent<Collider2D>();
        _orbit = GetComponent<UndeadOrbit>();
    }

    private void OnEnable()
    {
        StopAllCoroutines();
        Revive();
    }

    public void PlayLive()
    {
        if (_animator != null && !string.IsNullOrEmpty(_liveState))
            _animator.Play(_liveState, 0, 0f);
    }

    private void Revive()
    {
        _currentHealth = _enemyData != null ? _enemyData.MaxHealth : _health;
        ApplyCombat();

        _dead = false;
        _nextContactTime = 0f;

        if (_renderer != null)
            _renderer.enabled = true;

        if (_collider != null)
            _collider.enabled = true;

        PlayLive();

        if (_orbit != null)
            _orbit.enabled = true;
    }

    public void ApplyCombat()
    {
        float speed = _enemyData != null ? _enemyData.MoveSpeed : 1f;
        int damage = _enemyData != null ? _enemyData.AttackDamage : 0;
        int ghosts = 1;

        if (GameManager.Instance != null)
        {
            speed = GameManager.Instance.EnemyMoveSpeed;
            damage = GameManager.Instance.EnemyDamage;
            ghosts = GameManager.Instance.GhostsLostPerContact;
        }

        _currentSpeed = speed;
        _attackDamage = damage;
        _ghostsLost = Mathf.Max(1, ghosts);

        if (TryGetComponent(out EnemyAI ai))
            ai.SetCombat(_currentSpeed, _attackDamage);
    }

    public void TakeDamage(int damageAmount)
    {
        if (_dead || damageAmount <= 0)
            return;

        _currentHealth -= damageAmount;
        if (_currentHealth <= 0)
            Die();
        else
        {
            PlayHurt();
            Hurt?.Invoke();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (PlayerPower.IsActive)
        {
            CrushIfPowered(other);
            return;
        }

        if (_dead)
            return;

        if (_damagesOnContact)
        {
            DamagePlayer(other);
            return;
        }

        if (!other.TryGetComponent(out GhostCollector collector) || collector.Count == 0)
            return;

        if (TryGetComponent(out EnemyAI ai))
        {
            if (ai.HuntsGhost || !ai.CanStealGhost)
                return;

            collector.LoseGhost();
            ai.Swing();
            Growl();
            return;
        }

        collector.LoseGhost();
        Growl();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (CrushIfPowered(other))
            return;

        if (_dead || !_damagesOnContact)
            return;

        DamagePlayer(other);
    }

    private void DamagePlayer(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerHealth player))
            return;

        TryDamage(player);
    }

    public void TryDamage(PlayerHealth player)
    {
        if (_dead || player == null || PlayerPower.IsActive)
            return;

        if (Time.time < _nextContactTime)
            return;

        _nextContactTime = Time.time + _contactCooldown;
        player.TakeDamage(_attackDamage);
    }

    private bool CrushIfPowered(Collider2D other)
    {
        if (_dead || !PlayerPower.IsActive)
            return false;

        if (other.GetComponentInParent<PlayerHealth>() == null)
            return false;

        if (_animator != null)
            _animator.speed = 1f;

        TakeDamage(100000);
        return true;
    }

    public void Growl()
    {
        if (_dead || Time.time < _nextGrowlTime)
            return;

        if (_enemyData == null || _enemyData.EnemyName != "Undead")
            return;

        if (SoundManager.Instance == null)
            return;

        _nextGrowlTime = Time.time + 0.6f;
        SoundManager.Instance.PlayUndead();
    }

    private void Die()
    {
        _dead = true;
        Killed?.Invoke();

        if (_animator != null)
            _animator.speed = 1f;

        if (GameManager.Instance != null)
            GameManager.Instance.AddPoints(_killPoints);

        if (ScorePopup.Instance != null)
        {
            Vector3 popupPosition = _renderer != null
                ? _renderer.bounds.center
                : transform.position;
            ScorePopup.Instance.Show(_killPoints, popupPosition);
        }

        if (_collider != null)
            _collider.enabled = false;

        if (_orbit != null)
            _orbit.enabled = false;

        if (_animator != null)
            _animator.Play(_deathState, 0, 0f);

        PlayHurt();
        StartCoroutine(DeathRoutine());
    }

    private void PlayHurt()
    {
        if (SoundManager.Instance == null || string.IsNullOrEmpty(_deathState))
            return;

        if (_deathState.StartsWith("TQ_Skeleton"))
            SoundManager.Instance.PlaySkeleton();
        else if (_deathState.StartsWith("TQ_Slime"))
            SoundManager.Instance.PlaySlime();
        else if (_deathState.StartsWith("TQ_Goblin"))
            SoundManager.Instance.PlayGoblin();
    }

    private IEnumerator DeathRoutine()
    {
        yield return null;

        float deathLength = 1.2f;
        if (_animator != null)
        {
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.length > 0f)
                deathLength = info.length;
        }

        yield return new WaitForSeconds(deathLength);

        if (_returnToPool)
        {
            gameObject.SetActive(false);
            yield break;
        }

        if (_renderer != null)
            _renderer.enabled = false;

        yield return new WaitForSeconds(_respawnDelay);
        Revive();
    }
}
