// -----------------------------------------------------------------------------
//  Tiny Questers - Top-Down RPG Character Bundle Vol.1
//  BlinkAnimator.cs  (runtime)
//
//  Toggles the visibility of a UI Text component at a fixed interval to create a blinking effect.
// -----------------------------------------------------------------------------
using UnityEngine;
using UnityEngine.UI;

namespace TinyQuesters.Demo
{
    public class BlinkAnimator : MonoBehaviour
    {
        [SerializeField]
        private float blinkDuration = 0.55f;

        [SerializeField]
        private Text blinkText;

        private float blinkTimer;

        private void Update()
        {
            if (blinkText == null) return;
            
            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkDuration)
            {
                blinkTimer = 0f;
                blinkText.enabled = !blinkText.enabled;
            }
        }
    }
}
