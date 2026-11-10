using UnityEngine;
using UnityEngine.UI;

namespace UIHealthAlchemy
{
    // Unity UI does not redraw a static Image, so a shader that scrolls with
    // _Time stays frozen. This pushes a new scroll value every frame.
    [RequireComponent(typeof(Image))]
    public class CardioStrip : MonoBehaviour
    {
        [SerializeField] float speed = 0.45f;

        Image _image;
        Material _shared;
        Material _runtime;
        float _scroll;
        float _timeOffset;
        bool _playing = true;

        void Awake()
        {
            _image = GetComponent<Image>();
            _shared = _image.material;
            if (_shared == null)
                return;

            _runtime = new Material(_shared);
            _image.material = _runtime;
        }

        void Update()
        {
            if (!_playing || _runtime == null || _image == null)
                return;

            _scroll = Time.unscaledTime * speed + _timeOffset;
            Apply();
        }

        public void SetPlaying(bool playing)
        {
            if (_playing == playing)
                return;

            _playing = playing;
            if (playing)
                _timeOffset = _scroll - Time.unscaledTime * speed;
            else
                Apply();
        }

        void Apply()
        {
            if (_runtime == null || _image == null)
                return;

            _runtime.SetFloat("_Scroll", _scroll);
            _image.SetMaterialDirty();
            _image.SetVerticesDirty();
        }

        void OnDestroy()
        {
            if (_image != null && _shared != null)
                _image.material = _shared;
            if (_runtime != null)
                Destroy(_runtime);
        }
    }
}
