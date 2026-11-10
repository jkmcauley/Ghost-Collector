// -----------------------------------------------------------------------------
//  Tiny Questers - Top-Down RPG Character Bundle Vol.1
//  ToggleSpriteUI.cs  (runtime)
//
//  Two-state UI button. Clicking swaps the target Image between an ON and an
//  OFF sprite; while the pointer is held down, the matching "pressed" sprite is
//  shown instead. Used by the demo's Play / Auto buttons.
//
//  The pressed sprites are optional — leave one empty and that state simply
//  keeps showing its normal sprite.
// -----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TinyQuesters.Demo
{
    public class ToggleSpriteUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField]
        [Tooltip("Button that flips the state. Keep it on this same GameObject, " +
                 "otherwise the held-down state cannot be tracked.")]
        private Button button;

        [SerializeField]
        [Tooltip("Image whose sprite gets swapped.")]
        private Image targetImage;

        [SerializeField]
        [Tooltip("Shown while toggled ON and not held down.")]
        private Sprite toggledOnSprite;

        [SerializeField]
        [Tooltip("Shown while toggled ON and held down. Optional.")]
        private Sprite toggledOnPressedSprite;

        [SerializeField]
        [Tooltip("Shown while toggled OFF and not held down.")]
        private Sprite toggledOffSprite;

        [SerializeField]
        [Tooltip("Shown while toggled OFF and held down. Optional.")]
        private Sprite toggledOffPressedSprite;

        private bool isToggledOn;
        private bool isPressed;

        void Start()
        {
            if (targetImage == null)
            {
                Debug.LogError($"{nameof(ToggleSpriteUI)} on '{name}': targetImage is not " +
                               "assigned, so there is nothing to swap. Disabling.", this);
                enabled = false;
                return;
            }

            if (button == null)
            {
                Debug.LogWarning($"{nameof(ToggleSpriteUI)} on '{name}': button is not assigned, " +
                                 $"so clicks will not toggle. Call {nameof(ToggleSprite)}() instead.", this);
            }
            else
            {
                if (button.gameObject != gameObject)
                {
                    Debug.LogWarning($"{nameof(ToggleSpriteUI)} on '{name}': button sits on " +
                                     $"'{button.gameObject.name}', not on this GameObject, so the " +
                                     "pressed sprites will never show.", this);
                }
                button.onClick.AddListener(ToggleSprite);
            }

            ApplySprite();
        }

        void OnDestroy()
        {
            if (button != null) button.onClick.RemoveListener(ToggleSprite);
        }

        void OnDisable()
        {
            // A pointer-up can be missed while disabled — never stay stuck pressed.
            isPressed = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isPressed = true;
            ApplySprite();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            ApplySprite();
        }

        /// <summary>Flips the toggle state. Wired to the button's onClick.</summary>
        public void ToggleSprite()
        {
            isToggledOn = !isToggledOn;
            ApplySprite();
        }

        private void ApplySprite()
        {
            if (targetImage == null) return;

            Sprite pressed = isToggledOn ? toggledOnPressedSprite : toggledOffPressedSprite;
            Sprite normal = isToggledOn ? toggledOnSprite : toggledOffSprite;

            Sprite next = (isPressed && pressed != null) ? pressed : normal;
            if (next != null) targetImage.sprite = next;
        }
    }
}
