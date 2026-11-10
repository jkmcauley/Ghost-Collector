using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int _health = 100;
    [SerializeField] private int _lives = 3;
    [SerializeField] private string _deathState = "TQ_Warrior_Die";
    [SerializeField] private float _respawnDelay = 3f;

    private int _currentHealth;
    private int _livesRemaining;
    private bool _dead;
    private Vector3 _spawnPosition;
    private Animator _animator;
    private Collider2D _collider;
    private PlayerMovement _movement;
    private PlayerCombat _combat;
    [SerializeField] private UIManager _uiManager;

    public int CurrentHealth => _currentHealth;
    public int Lives => _livesRemaining;
    public bool IsDead => _dead;

    public void AddLives(int amount)
    {
        if (amount <= 0)
            return;

        int before = _livesRemaining;
        _livesRemaining = Mathf.Min(_livesRemaining + amount, 3);
        PushLives();

        if (_livesRemaining > before && SoundManager.Instance != null)
            SoundManager.Instance.PlayTaDa();
    }

    private void Awake()
    {
        _spawnPosition = transform.position;
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
        _movement = GetComponent<PlayerMovement>();
        _combat = GetComponent<PlayerCombat>();
        _livesRemaining = _lives;
        _currentHealth = _health;
    }

    private void Start()
    {
        PushHealth();
        PushLives();
    }

    private void OnEnable()
    {
        if (_dead)
            return;

        _currentHealth = _health;
        PushHealth();
    }

    public void TakeDamage(int damageAmount)
    {
        if (_dead || damageAmount <= 0 || PlayerPower.IsActive)
            return;

        _currentHealth -= damageAmount;
        if (_currentHealth < 0)
            _currentHealth = 0;

        PushHealth();

        if (_currentHealth <= 0)
            Die();
        else
        {
            if (_movement != null)
                _movement.PlayHurt();

            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayerHurt();
        }
    }

    private void Die()
    {
        _dead = true;
        if (TryGetComponent(out PlayerPower power))
            power.Stop();

        _currentHealth = 0;
        _livesRemaining = Mathf.Max(0, _livesRemaining - 1);
        PushHealth();
        PushLives();

        if (_movement != null)
            _movement.SetAlive(false);

        if (_combat != null)
            _combat.SetAlive(false);

        if (_collider != null)
            _collider.enabled = false;

        if (_animator != null && !string.IsNullOrEmpty(_deathState))
            _animator.Play(_deathState, 0, 0f);

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayerHurt();

        if (_livesRemaining > 0)
            StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        yield return null;

        float deathLength = 1.5f;
        if (_animator != null)
        {
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            if (info.length > 0f)
                deathLength = info.length;
        }

        yield return new WaitForSeconds(deathLength);
        yield return new WaitForSeconds(_respawnDelay);
        Revive();
    }

    private void Revive()
    {
        _currentHealth = _health;
        _dead = false;
        PushHealth();

        if (_movement != null)
            _movement.Warp(_spawnPosition);

        if (_collider != null)
            _collider.enabled = true;

        if (_movement != null)
            _movement.SetAlive(true);

        if (_combat != null)
            _combat.SetAlive(true);
    }

    private void PushHealth()
    {
        UIManager ui = _uiManager != null ? _uiManager : UIManager.Instance;
        if (ui != null)
            ui.UpdateHealth(_currentHealth, _health);
    }

    private void PushLives()
    {
        UIManager ui = _uiManager != null ? _uiManager : UIManager.Instance;
        if (ui != null)
            ui.UpdateLives(_livesRemaining);
        if (_livesRemaining <= 0)
        {
            GameManager.Instance.EndGame();
        }
    }
}
