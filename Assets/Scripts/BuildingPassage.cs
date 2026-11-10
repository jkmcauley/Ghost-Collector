using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BuildingPassage : MonoBehaviour
{
    const float MinWall = 0.08f;
    const float FootprintFraction = 0.34f;

    [SerializeField] private float _passageWidth = 1.35f;

    static PhysicsMaterial2D _slide;

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer.sprite == null)
            return;

        Bounds bounds = spriteRenderer.sprite.bounds;
        float widestOpening = bounds.size.x - MinWall * 2f;
        if (widestOpening < 0.4f)
            return;

        float passage = Mathf.Max(_passageWidth, bounds.size.x * 0.7f);
        passage = Mathf.Min(passage, widestOpening);
        float wallWidth = (bounds.size.x - passage) * 0.5f;
        float footprint = Mathf.Max(bounds.size.y * FootprintFraction, 0.5f);
        float y = bounds.min.y + footprint * 0.5f;

        AddWall(bounds.min.x + wallWidth * 0.5f, y, wallWidth, footprint);
        AddWall(bounds.max.x - wallWidth * 0.5f, y, wallWidth, footprint);
    }

    private void AddWall(float x, float y, float width, float height)
    {
        BoxCollider2D wall = gameObject.AddComponent<BoxCollider2D>();
        wall.isTrigger = false;
        wall.sharedMaterial = SlideMaterial();
        wall.offset = new Vector2(x, y);
        wall.size = new Vector2(width, height);
    }

    static PhysicsMaterial2D SlideMaterial()
    {
        if (_slide != null)
            return _slide;

        _slide = new PhysicsMaterial2D("BuildingSlide")
        {
            friction = 0f,
            bounciness = 0f
        };
        return _slide;
    }
}
