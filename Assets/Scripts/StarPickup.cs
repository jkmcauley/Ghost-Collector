using UnityEngine;

public class StarPickup : MonoBehaviour
{
    const float CoinSize = 0.8f;

    private static Sprite _sprite;
    private static Texture2D _texture;

    private float _speed;
    private float _removeX;
    private bool _moving;
    private bool _collected;

    private void Awake()
    {
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        SpriteRenderer sprite = gameObject.AddComponent<SpriteRenderer>();
        sprite.sprite = CoinSprite();
        sprite.sortingOrder = 5000;
        sprite.color = Color.white;

        CircleCollider2D circle = gameObject.AddComponent<CircleCollider2D>();
        circle.isTrigger = true;
        circle.radius = CoinSize * 0.45f;
    }

    public void Launch(float speed, float removeX)
    {
        _speed = speed;
        _removeX = removeX;
        _moving = true;
    }

    private void Update()
    {
        if (!_moving || _collected)
            return;

        Vector3 position = transform.position;
        position.x -= _speed * Time.deltaTime;
        position.z = 0f;
        transform.position = position;

        if (position.x < _removeX)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected)
            return;

        PlayerPower power = other.GetComponentInParent<PlayerPower>();
        if (power == null)
            return;

        if (Vector2.Distance(transform.position, power.transform.position) > 1.4f)
            return;

        if (power.TryGetComponent(out PlayerHealth health) && health.IsDead)
            return;

        _collected = true;
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayPowerUp();

        power.Activate();
        Destroy(gameObject);
    }

    private static Sprite CoinSprite()
    {
        if (_sprite != null)
            return _sprite;

        const int size = 48;
        _texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        _texture.filterMode = FilterMode.Point;
        _texture.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[size * size];
        float center = (size - 1) * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance > 0.92f)
                    pixels[y * size + x] = Color.clear;
                else if (distance > 0.72f)
                    pixels[y * size + x] = new Color(0.45f, 0.22f, 0.02f, 1f);
                else
                    pixels[y * size + x] = new Color(1f, 0.82f, 0.1f, 1f);
            }
        }

        _texture.SetPixels(pixels);
        _texture.Apply(false, false);
        float pixelsPerUnit = size / CoinSize;
        _sprite = Sprite.Create(_texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
        return _sprite;
    }
}
