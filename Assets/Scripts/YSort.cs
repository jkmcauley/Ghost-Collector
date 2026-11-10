using UnityEngine;

// Same order the cemetery props were baked with: higher on the map draws behind.
public class YSort : MonoBehaviour
{
    const int BaseOrder = 800;
    const float UnitsToOrder = 40f;

    [SerializeField] private float _sortOffset;

    SpriteRenderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (_renderer == null)
            return;

        float feetY = _renderer.bounds.min.y + _sortOffset;
        _renderer.sortingOrder = BaseOrder + Mathf.RoundToInt(-feetY * UnitsToOrder);
    }
}
