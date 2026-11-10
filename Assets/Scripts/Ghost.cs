using System.Collections;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    static readonly Vector3[] Directions =
    {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
    };

    [SerializeField] private Transform _player;
    [SerializeField] private float _idleTime = 5f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _moveDuration = 5f;
    [SerializeField, Min(0)] private int _points = 10;

    public int Points => _points;

    private bool _isCollected;
    private bool _remains;
    private Coroutine _routine;
    private SpriteRenderer _renderer;
    private Rigidbody2D _body;

    public void Initialize(Transform player)
    {
        _player = player;
    }

    public void Remain()
    {
        _remains = true;
        StopRoutine();
    }

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _body = GetComponent<Rigidbody2D>();
        if (_body == null)
            _body = gameObject.AddComponent<Rigidbody2D>();

        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.gravityScale = 0f;
        _body.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void OnEnable()
    {
        _isCollected = false;
        if (_renderer != null)
        {
            _renderer.flipX = false;
            _renderer.material.color = Color.white;
        }

        if (_remains)
            return;

        _routine = StartCoroutine(GhostRoutine());
    }

    private IEnumerator GhostRoutine()
    {
        yield return new WaitForSeconds(_idleTime);

        Vector3[] directions = ShuffledDirections();
        float legDuration = _moveDuration / directions.Length;

        foreach (Vector3 direction in directions)
        {
            Face(direction);

            float legTimer = 0f;
            while (legTimer < legDuration && !_isCollected)
            {
                transform.Translate(direction * _moveSpeed * Time.deltaTime, Space.World);
                legTimer += Time.deltaTime;
                yield return null;
            }
        }

        _routine = null;

        if (!_isCollected)
            gameObject.SetActive(false);
    }

    public void StopWandering()
    {
        StopRoutine();
    }

    public void Collect()
    {
        if (_isCollected)
            return;

        _isCollected = true;
        StopRoutine();

        if (_renderer != null)
            _renderer.material.color = Color.red;
    }

    private void OnDisable()
    {
        StopRoutine();
    }

    private void StopRoutine()
    {
        if (_routine == null)
            return;

        StopCoroutine(_routine);
        _routine = null;
    }

    private void Face(Vector3 direction)
    {
        if (_renderer == null || Mathf.Abs(direction.x) < 0.01f)
            return;

        _renderer.flipX = direction.x < 0f;
    }

    private static Vector3[] ShuffledDirections()
    {
        Vector3[] directions = (Vector3[])Directions.Clone();

        for (int i = directions.Length - 1; i > 0; i--)
        {
            int swap = Random.Range(0, i + 1);
            (directions[i], directions[swap]) = (directions[swap], directions[i]);
        }

        return directions;
    }
}
