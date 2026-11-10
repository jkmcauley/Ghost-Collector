using UnityEngine;

public class BatChirp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerMovement _))
            return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayBats();
    }
}
