using UnityEngine;

public class PlayerPower : MonoBehaviour
{
    public static PlayerPower Instance { get; private set; }

    public static bool IsActive => Instance != null && Instance.IsPoweredUp;

    [SerializeField, Min(0.1f)] private float _duration = 8f;
    [SerializeField, Min(1f)] private float _scaleMultiplier = 1.5f;
    [SerializeField, Min(1f)] private float _speedMultiplier = 1.5f;
    [SerializeField] private float _flashSpeed = 16f;

    private PlayerMovement _movement;
    private PlayerHealth _health;
    private Collider2D _body;
    private SpriteRenderer[] _renderers;
    private Color[] _baseColors;
    private Vector3 _baseScale;
    private float _endTime;
    private bool _usingPassableObstacles;

    public bool IsPoweredUp { get; private set; }

    private void Awake()
    {
        _movement = GetComponent<PlayerMovement>();
        _health = GetComponent<PlayerHealth>();
        _body = GetComponent<Collider2D>();
        _renderers = GetComponentsInChildren<SpriteRenderer>(true);
        _baseColors = new Color[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++)
            _baseColors[i] = _renderers[i].color;

        _baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        Instance = this;
    }

    private void OnDisable()
    {
        if (IsPoweredUp)
        {
            EndEffects();
            if (SoundManager.Instance != null)
                SoundManager.Instance.StopPowerUp();
        }

        IsPoweredUp = false;
        if (Instance == this)
            Instance = null;
    }

    public void Activate()
    {
        if (_health != null && _health.IsDead)
            return;

        _endTime = Time.time + _duration;
        if (IsPoweredUp)
            return;

        IsPoweredUp = true;
        BeginEffects();
    }

    public void Stop()
    {
        if (!IsPoweredUp)
            return;

        IsPoweredUp = false;
        EndEffects();
        if (SoundManager.Instance != null)
            SoundManager.Instance.StopPowerUp();
    }

    private void Update()
    {
        if (!IsPoweredUp)
            return;

        Flash();
        if (Time.time < _endTime)
            return;

        IsPoweredUp = false;
        EndEffects();
    }

    private void BeginEffects()
    {
        transform.localScale = _baseScale * _scaleMultiplier;
        if (_movement != null)
            _movement.SetPowerMultiplier(_speedMultiplier);

        if (_body != null)
        {
            TreeBlockers.IgnoreFor(_body, true);
            _usingPassableObstacles = true;
        }
    }

    private void EndEffects()
    {
        transform.localScale = _baseScale;
        if (_movement != null)
            _movement.SetPowerMultiplier(1f);

        RestoreColor();

        if (_usingPassableObstacles && _body != null)
            TreeBlockers.IgnoreFor(_body, false);

        _usingPassableObstacles = false;
    }

    private void Flash()
    {
        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * _flashSpeed);
        Color color = Color.Lerp(
            new Color(0.15f, 1f, 0.25f, 1f),
            new Color(0.85f, 1f, 0.8f, 1f),
            pulse);

        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
                _renderers[i].color = color;
        }
    }

    private void RestoreColor()
    {
        for (int i = 0; i < _renderers.Length; i++)
        {
            if (_renderers[i] != null)
                _renderers[i].color = _baseColors[i];
        }
    }
}
