using System.Collections;
using UnityEngine;

public class GhostFollower : MonoBehaviour
{
    [SerializeField] private float _followSpeed = 7f;
    [SerializeField] private Sprite[] _explosionFrames;
    [SerializeField] private float _explosionFrameTime = 0.083333336f;

    public bool IsCollected { get; private set; }

    private bool _exploding;
    private Animator _animator;
    private SpriteRenderer _renderer;
    private Coroutine _explosion;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        IsCollected = false;
        _exploding = false;

        if (_animator != null)
            _animator.enabled = true;
    }

    private void OnDisable()
    {
        if (_explosion == null)
            return;

        StopCoroutine(_explosion);
        _explosion = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected || _exploding)
            return;

        if (other.TryGetComponent<GhostCollector>(out var collector))
            collector.Collect(this);
    }

    public void JoinLine()
    {
        IsCollected = true;

        if (TryGetComponent(out Ghost ghost))
            ghost.Collect();
    }

    public void Follow(Vector3 destination)
    {
        if (_exploding)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            _followSpeed * Time.deltaTime
        );
    }

    public void Remove()
    {
        gameObject.SetActive(false);
    }

    public void Explode()
    {
        if (_exploding)
            return;

        _exploding = true;
        _explosion = StartCoroutine(ExplosionRoutine());
    }

    private IEnumerator ExplosionRoutine()
    {
        if (_animator != null)
            _animator.enabled = false;

        if (_renderer != null)
        {
            _renderer.color = Color.white;
            if (_renderer.material != null)
                _renderer.material.color = Color.white;
        }

        if (_explosionFrames != null && _explosionFrames.Length > 0 && _renderer != null)
        {
            for (int i = 0; i < _explosionFrames.Length; i++)
            {
                _renderer.sprite = _explosionFrames[i];
                yield return new WaitForSeconds(_explosionFrameTime);
            }
        }

        _explosion = null;
        gameObject.SetActive(false);
    }
}
