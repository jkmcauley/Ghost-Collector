using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(YSort))]
public class PlayerMovement : MonoBehaviour
{
    const string ClipPrefix = "TQ_Warrior_";

    [SerializeField] private PlayerInputReader _input;
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Animator _animator;
    private Vector2 _facing = Vector2.down;
    private string _currentClip;
    private string _attackClip;
    private bool _attacking;
    private bool _hurting;
    private bool _dead;
    private float _attackStart;
    private float _hurtStart;
    private float _powerMultiplier = 1f;

    public bool IsAttacking => _attacking;
    public float AttackElapsed => Time.time - _attackStart;

    public void SetPowerMultiplier(float multiplier)
    {
        _powerMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetAlive(bool alive)
    {
        _dead = !alive;
        _attacking = false;
        _hurting = false;
        _currentClip = null;
        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }

    public void Warp(Vector3 position)
    {
        if (_rb != null)
        {
            _rb.position = position;
            _rb.linearVelocity = Vector2.zero;
        }

        transform.position = position;
    }
    public Vector2 SwingDirection { get; private set; }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        if (GetComponent<YSort>() == null)
            gameObject.AddComponent<YSort>();
    }

    private void Update()
    {
        if (_dead)
            return;

        if (_hurting)
        {
            float elapsed = Time.time - _hurtStart;
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            bool finished = elapsed > 0.05f
                && info.IsName(_currentClip)
                && info.normalizedTime >= 1f;
            if (finished || elapsed >= 0.45f)
            {
                _hurting = false;
                _currentClip = null;
            }

            return;
        }

        Vector2 move = _input.MoveInput;
        if (move.sqrMagnitude > 0.01f)
            _facing = DominantDirection(move);

        if (_attacking)
        {
            AnimatorStateInfo info = _animator.GetCurrentAnimatorStateInfo(0);
            if (!info.IsName(_attackClip) || info.normalizedTime < 1f)
                return;

            _attacking = false;
            _currentClip = null;
        }

        string next = move.sqrMagnitude > 0.01f
            ? ClipName("Walk", _facing)
            : ClipName("Idle", _facing);

        PlayIfChanged(next);
    }

    private void FixedUpdate()
    {
        if (_dead || _hurting)
        {
            _rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 direction = Vector2.ClampMagnitude(_input.MoveInput, 1f);
        _rb.linearVelocity = direction * _moveSpeed * _powerMultiplier;
    }

    private void OnDisable()
    {
        _rb.linearVelocity = Vector2.zero;
    }

    public bool PlayAttack()
    {
        if (_dead || _attacking || _hurting)
            return false;

        _attackClip = ClipName("Attack", _facing);
        _animator.Play(_attackClip, 0, 0f);
        _currentClip = _attackClip;
        _attacking = true;
        _attackStart = Time.time;
        SwingDirection = DirectionFromClip(_attackClip);
        return true;
    }

    public void PlayHurt()
    {
        if (_dead || _animator == null)
            return;

        _attacking = false;
        _hurting = true;
        _hurtStart = Time.time;
        _currentClip = ClipPrefix + "Hit";
        _animator.Play(_currentClip, 0, 0f);
        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;
    }

    private static Vector2 DirectionFromClip(string clip)
    {
        if (clip.EndsWith("_Left"))
            return Vector2.left;
        if (clip.EndsWith("_Right"))
            return Vector2.right;
        if (clip.EndsWith("_Up"))
            return Vector2.up;

        return Vector2.down;
    }

    private void PlayIfChanged(string clip)
    {
        if (clip == _currentClip)
            return;

        _animator.Play(clip);
        _currentClip = clip;
    }

    private static Vector2 DominantDirection(Vector2 move)
    {
        if (Mathf.Abs(move.x) > Mathf.Abs(move.y))
            return move.x < 0f ? Vector2.left : Vector2.right;

        return move.y < 0f ? Vector2.down : Vector2.up;
    }

    private static string ClipName(string action, Vector2 facing)
    {
        string direction;
        if (facing == Vector2.up)
            direction = "Up";
        else if (facing == Vector2.left && action != "Idle")
            direction = "Left";
        else if (facing == Vector2.right && action != "Idle")
            direction = "Right";
        else
            direction = "Down";

        // The side attack sheets swing opposite their clip names.
        if (action == "Attack" && direction == "Left")
            direction = "Right";
        else if (action == "Attack" && direction == "Right")
            direction = "Left";

        return ClipPrefix + action + "_" + direction;
    }
}
