// -----------------------------------------------------------------------------
//  Tiny Questers - Top-Down RPG Character Bundle Vol.1
//  ShowcaseController.cs  (runtime)
//
//  Drives the TQ_Showcase scene:
//    - On start, every character is hidden and a centered
//      "PLAY TO SHOW OVERVIEW" prompt is shown.
//    - On the first click (or Enter), the prompt disappears, all characters
//      appear and each one begins cycling through its animations, and the
//      bottom hint box (controls + description) fades in.
//
//  Added by TinyQuestersBuilder.
// -----------------------------------------------------------------------------
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace TinyQuesters.Demo
{
    public class ShowcaseController : MonoBehaviour
    {
        [Tooltip("Per-character containers (character + its name/motion labels), toggled as one unit.")]
        public GameObject[] characterRoots;

        [Tooltip("Centered 'PLAY TO SHOW OVERVIEW' prompt, hidden once started.")]
        public GameObject playOverlay;

        [Tooltip("Bottom round box with the controls / description, shown once started.")]
        public GameObject hintBox;

        bool started;

        void Awake()
        {
            // Hide the characters (and their labels) and the bottom hint until the
            // user starts. (Awake, not Start, so nothing flashes on the first frame.)
            if (characterRoots != null)
            {
                foreach (var r in characterRoots)
                    if (r != null) r.SetActive(false);
            }
            if (hintBox != null)     hintBox.SetActive(false);
            if (playOverlay != null) playOverlay.SetActive(true);
        }

        void Update()
        {
            if (started) return;
            if (ClickedThisFrame()) StartShow();
        }

        static bool ClickedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            var m = Mouse.current;
            if (m != null && m.leftButton.wasPressedThisFrame) return true;
            var k = Keyboard.current;
            if (k != null && k.enterKey.wasPressedThisFrame) return true;
            return false;
#else
            return Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return);
#endif
        }

        public void StartShow()
        {
            if (started) return;
            started = true;

            if (playOverlay != null) playOverlay.SetActive(false);
            if (hintBox != null)     hintBox.SetActive(true);

            if (characterRoots != null)
            {
                foreach (var r in characterRoots)
                {
                    if (r == null) continue;
                    r.SetActive(true);
                    var sc = r.GetComponentInChildren<ShowcaseCharacter>(true);
                    if (sc != null) sc.Begin();
                }
            }
        }
    }
}
