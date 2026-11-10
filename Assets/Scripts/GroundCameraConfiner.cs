using Unity.Cinemachine;
using UnityEngine;

public class GroundCameraConfiner : MonoBehaviour
{
    [SerializeField] private Collider2D _bounds;

    private void Awake()
    {
        if (_bounds == null)
            _bounds = GetComponent<Collider2D>();

        if (_bounds == null)
            return;

        CinemachineCamera[] cameras = FindObjectsByType<CinemachineCamera>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        foreach (CinemachineCamera camera in cameras)
        {
            CinemachineConfiner2D confiner = camera.GetComponent<CinemachineConfiner2D>();
            if (confiner == null)
                confiner = camera.gameObject.AddComponent<CinemachineConfiner2D>();

            confiner.BoundingShape2D = _bounds;
            confiner.Damping = 0f;
            confiner.SlowingDistance = 0f;
        }
    }
}
