using UnityEngine;

public class SoundToggle : MonoBehaviour
{
    [SerializeField] private GameObject _soundOn;
    [SerializeField] private GameObject _soundOff;

    private bool _on = true;

    private void OnEnable()
    {
        if (SoundManager.Instance != null)
            _on = SoundManager.Instance.SoundOn;

        Apply();
    }

    public void Toggle()
    {
        _on = !_on;
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSoundOn(_on);

        Apply();
    }

    private void Apply()
    {
        if (_soundOn != null)
            _soundOn.SetActive(_on);
        if (_soundOff != null)
            _soundOff.SetActive(!_on);
    }
}
