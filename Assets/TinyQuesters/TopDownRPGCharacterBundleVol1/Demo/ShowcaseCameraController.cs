// -----------------------------------------------------------------------------
//  Tiny Questers - Top-Down RPG Character Bundle Vol.1
//  ShowcaseCameraController.cs  (runtime)
//
//  Pan & zoom controller for the TQ_Showcase scene so you can inspect every
//  character's animation up close.
//
//    Mouse scroll        : zoom in / out (toward the cursor)
//    Left / Middle drag   : pan the camera
//    Space (or R) key     : reset to the initial framing
//
//  Added automatically to the showcase Main Camera by TinyQuestersBuilder.
// -----------------------------------------------------------------------------
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TinyQuesters.Demo
{
    [RequireComponent(typeof(Camera))]
    public class ShowcaseCameraController : MonoBehaviour
    {
        public float minSize = 0.8f;
        public float maxSize = 40f;
        public float zoomStep = 0.12f;

        Camera cam;
        Vector3 homePos;
        float homeSize;
        Vector3 dragOrigin;
        bool dragging;

        void Awake()
        {
            cam = GetComponent<Camera>();
            if (!cam.orthographic) cam.orthographic = true;
        }

        void Start()
        {
            homePos = transform.position;
            homeSize = cam.orthographicSize;
        }

        void Update()
        {
#if ENABLE_INPUT_SYSTEM
            var m = Mouse.current;
            if (m == null) return;
            Vector2 mp = m.position.ReadValue();

            // ---- Zoom toward the cursor (scroll wheel) ----
            float scroll = m.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 before = cam.ScreenToWorldPoint(mp);
                float factor = Mathf.Exp(-Mathf.Sign(scroll) * zoomStep);
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize * factor, minSize, maxSize);
                Vector3 after = cam.ScreenToWorldPoint(mp);
                Vector3 d = before - after; d.z = 0f;
                transform.position += d;
            }

            // ---- Drag to pan (left or middle button) ----
            bool held = m.leftButton.isPressed || m.middleButton.isPressed;
            if (m.leftButton.wasPressedThisFrame || m.middleButton.wasPressedThisFrame)
            {
                dragOrigin = cam.ScreenToWorldPoint(mp);
                dragging = true;
            }
            else if (held && dragging)
            {
                Vector3 now = cam.ScreenToWorldPoint(mp);
                Vector3 d = dragOrigin - now; d.z = 0f;   // keep the grabbed world point under the cursor
                transform.position += d;
            }
            else if (!held)
            {
                dragging = false;
            }

            // ---- Reset framing (Space, or R) ----
            var kb = Keyboard.current;
            if (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.rKey.wasPressedThisFrame))
            {
                transform.position = homePos;
                cam.orthographicSize = homeSize;
            }
#else
            Vector3 mp = Input.mousePosition;

            // ---- Zoom toward the cursor (scroll wheel) ----
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                Vector3 before = cam.ScreenToWorldPoint(mp);
                float factor = Mathf.Exp(-Mathf.Sign(scroll) * zoomStep);
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize * factor, minSize, maxSize);
                Vector3 after = cam.ScreenToWorldPoint(mp);
                Vector3 d = before - after; d.z = 0f;
                transform.position += d;
            }

            // ---- Drag to pan (left or middle button) ----
            bool held = Input.GetMouseButton(0) || Input.GetMouseButton(2);
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2))
            {
                dragOrigin = cam.ScreenToWorldPoint(mp);
                dragging = true;
            }
            else if (held && dragging)
            {
                Vector3 now = cam.ScreenToWorldPoint(mp);
                Vector3 d = dragOrigin - now; d.z = 0f;   // keep the grabbed world point under the cursor
                transform.position += d;
            }
            else if (!held)
            {
                dragging = false;
            }

            // ---- Reset framing (Space, or R) ----
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.R))
            {
                transform.position = homePos;
                cam.orthographicSize = homeSize;
            }
#endif
        }
    }
}
