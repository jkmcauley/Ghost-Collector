using UnityEngine;

public class UndeadOrbit : MonoBehaviour
{
    [SerializeField] private Transform _vortex;
    [SerializeField, Min(0f)] private float _degreesPerSecond = 90f;
    [SerializeField, Min(0f)] private float _clearance = 0.75f;

    private SpriteRenderer _renderer;
    private Rigidbody2D _body;
    private Vector3 _center;
    private float _radius;
    private float _angle;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _body = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (_vortex == null)
        {
            Debug.LogWarning("Assign the Vortex in the Inspector.", this);
            enabled = false;
            return;
        }

        _center = OrbitCenter();

        Vector3 offset = transform.position - _center;
        offset.z = 0f;
        _radius = offset.magnitude;

        float minimum = MinimumRadius();
        if (_radius < minimum)
            _radius = minimum;

        _angle = offset.sqrMagnitude > 0.0001f
            ? Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg
            : 0f;
    }

    private void FixedUpdate()
    {
        _angle += _degreesPerSecond * Time.fixedDeltaTime;

        float radians = _angle * Mathf.Deg2Rad;
        Vector3 next = _center + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * _radius;
        Vector3 move = next - transform.position;

        if (_body != null)
            _body.MovePosition(next);
        else
            transform.position = next;

        Face(move);
    }

    private Vector3 OrbitCenter()
    {
        if (_vortex.TryGetComponent(out CircleCollider2D circle))
            return circle.bounds.center;

        return _vortex.position;
    }

    private float MinimumRadius()
    {
        if (_vortex.TryGetComponent(out CircleCollider2D circle))
            return circle.bounds.extents.x + _clearance;

        return _clearance;
    }

    private void Face(Vector3 move)
    {
        if (_renderer == null || Mathf.Abs(move.x) < 0.0001f)
            return;

        _renderer.flipX = move.x < 0f;
    }
}
