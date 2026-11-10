using System.Collections;
using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    [SerializeField] private Collider2D _bounds;
    [SerializeField, Min(0.1f)] private float _interval = 15f;
    [SerializeField, Min(0f)] private float _firstDelay = 1f;
    [SerializeField, Min(0.1f)] private float _speed = 2.6f;

    private Coroutine _routine;

    private void OnEnable()
    {
        _routine = StartCoroutine(SpawnRoutine());
    }

    private void OnDisable()
    {
        if (_routine == null)
            return;

        StopCoroutine(_routine);
        _routine = null;
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (!PlayHasStarted())
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(_firstDelay);
            while (PlayHasStarted())
            {
                Spawn();
                yield return new WaitForSeconds(StarInterval());
            }
        }
    }

    private static bool PlayHasStarted()
    {
        return GameManager.Instance != null && GameManager.Instance.PlayStarted;
    }

    private float StarInterval()
    {
        if (GameManager.Instance == null)
            return _interval;

        return Mathf.Max(0.1f, GameManager.Instance.PowerStarInterval);
    }

    private void Spawn()
    {
        Camera cam = Camera.main;
        Vector3 position;
        float removeX;

        if (cam != null && cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * Mathf.Max(cam.aspect, 1f);
            Vector3 view = cam.transform.position;
            float y = view.y + Random.Range(-halfHeight * 0.45f, halfHeight * 0.45f);
            float x = view.x + halfWidth - 0.8f;
            position = new Vector3(x, y, 0f);
            removeX = view.x - halfWidth - 1f;
        }
        else if (!TryMapSpawn(out position, out removeX))
        {
            return;
        }

        GameObject star = new GameObject("PowerStar");
        star.transform.position = position;
        StarPickup pickup = star.AddComponent<StarPickup>();
        pickup.Launch(_speed, removeX);
    }

    private bool TryMapSpawn(out Vector3 position, out float removeX)
    {
        position = default;
        removeX = 0f;

        if (_bounds == null)
        {
            GroundCameraConfiner confiner = FindFirstObjectByType<GroundCameraConfiner>();
            if (confiner != null)
                _bounds = confiner.GetComponent<Collider2D>();
        }

        if (_bounds == null)
            return false;

        Bounds area = _bounds.bounds;
        float y = Random.Range(
            Mathf.Lerp(area.min.y, area.max.y, 0.25f),
            Mathf.Lerp(area.min.y, area.max.y, 0.75f));
        position = new Vector3(area.max.x - 1.2f, y, 0f);
        removeX = area.min.x - 0.5f;
        return true;
    }
}
