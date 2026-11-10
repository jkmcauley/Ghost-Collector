using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectRatio : MonoBehaviour
{
    [SerializeField] private float _aspect = 16f / 9f;

    private Camera _camera;

    private void OnEnable()
    {
        _camera = GetComponent<Camera>();
        Apply();
    }

    private void LateUpdate()
    {
        Apply();
    }

    private void Apply()
    {
        if (_camera == null)
            _camera = GetComponent<Camera>();

        if (_aspect < 0.01f || Screen.height <= 0)
            return;

        float windowAspect = (float)Screen.width / Screen.height;
        float scale = windowAspect / _aspect;
        Rect rect = new Rect(0f, 0f, 1f, 1f);

        if (scale < 1f)
        {
            rect.height = scale;
            rect.y = (1f - scale) * 0.5f;
        }
        else
        {
            float width = 1f / scale;
            rect.width = width;
            rect.x = (1f - width) * 0.5f;
        }

        if (_camera.rect != rect)
            _camera.rect = rect;
    }
}
