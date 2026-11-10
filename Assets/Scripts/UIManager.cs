using TMPro;
using UIHealthAlchemy;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField] private TMP_Text _scoreText;

    [Header("Returned ghosts")]
    [SerializeField] private TMP_Text _ghostsReturnedText;

    [Header("Lives — assign left to right")]
    [SerializeField] private Image[] _lifeImages = new Image[3];
    [SerializeField, Range(0f, 1f)]
    private float _lostLifeAlpha = 0.2f;

    [Header("Health")]
    [SerializeField] private HealthBarLogic _healthBar;
    [SerializeField] private CardioStrip _cardioStrip;

    [Header("Difficulty")]
    [SerializeField] private TMP_Dropdown _difficultyDropdown;
    private GameObject _levelsRoot;
    private bool _difficultyBound;
    private bool _choosingDifficulty;
    private bool _dropdownWasOpen;
    private bool _levelChosen;
    private bool _levelAccepted;
    private bool _dismissing;
    private bool _waitingForFirstInput;
    private bool _startArmed;
    private float _fadeTimer = -1f;
    private CanvasGroup _levelsGroup;
    private const float MenuFadeSeconds = 0.45f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveGhostText();
    }

    private void OnEnable()
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.ScoreChanged += UpdateScore;
        GameManager.Instance.GhostsReturnedChanged += UpdateGhostsReturned;
        BindDifficultyDropdown();
    }

    private void Start()
    {
        ResolveGhostText();
        BindDifficultyDropdown();
        ShowDifficultyFirst();

        if (GameManager.Instance == null)
            return;

        UpdateScore(GameManager.Instance.Score);
        UpdateGhostsReturned(GameManager.Instance.GhostsReturned);
    }

    private void Update()
    {
        if (!_levelChosen)
            ShowModeLabel();

        if (!_choosingDifficulty || _difficultyDropdown == null)
            return;

        bool open = _difficultyDropdown.IsExpanded;
        if (_dropdownWasOpen && !open)
        {
            ShowSelectedLevel();
            _levelAccepted = true;
        }

        _dropdownWasOpen = open;

        if (_waitingForFirstInput)
        {
            TickFirstButton();
            return;
        }

        if (_dismissing)
        {
            TickMenuFade();
            return;
        }

        if (!_levelAccepted || DifficultyInfo.IsOpen)
            return;

        BeginMenuFade();
    }

    private void OnDisable()
    {
        UnbindDifficultyDropdown();

        if (GameManager.Instance == null)
            return;

        GameManager.Instance.ScoreChanged -= UpdateScore;
        GameManager.Instance.GhostsReturnedChanged -= UpdateGhostsReturned;
    }

    private void OnDestroy()
    {
        if (_choosingDifficulty)
            Time.timeScale = 1f;

        if (Instance == this)
            Instance = null;
    }

    private void ShowDifficultyFirst()
    {
        if (GameManager.Instance == null || GameManager.Instance.PlayStarted)
            return;

        _levelsRoot = FindNamedObject("Levels");
        if (_levelsRoot != null)
            _levelsRoot.SetActive(true);

        if (_difficultyDropdown == null)
            _difficultyDropdown = FindDifficultyDropdown();

        if (_difficultyDropdown == null)
        {
            GameManager.Instance.BeginPlay();
            return;
        }

        _choosingDifficulty = true;
        _dropdownWasOpen = false;
        _levelAccepted = false;
        _dismissing = false;
        _waitingForFirstInput = false;
        _fadeTimer = -1f;
        Time.timeScale = 0f;
    }

    private void BeginMenuFade()
    {
        _dismissing = true;
        _fadeTimer = MenuFadeSeconds;

        if (_levelsRoot == null)
            return;

        _levelsGroup = _levelsRoot.GetComponent<CanvasGroup>();
        if (_levelsGroup == null)
            _levelsGroup = _levelsRoot.AddComponent<CanvasGroup>();

        _levelsGroup.alpha = 1f;
        _levelsGroup.interactable = true;
        _levelsGroup.blocksRaycasts = true;
    }

    private void TickMenuFade()
    {
        _fadeTimer -= Time.unscaledDeltaTime;
        float shown = Mathf.Clamp01(_fadeTimer / MenuFadeSeconds);
        if (_levelsGroup != null)
            _levelsGroup.alpha = shown;

        if (_fadeTimer > 0f)
            return;

        _dismissing = false;
        _waitingForFirstInput = true;
        _startArmed = !ControlHeld();

        if (_levelsRoot != null)
            _levelsRoot.SetActive(false);
    }

    private void TickFirstButton()
    {
        if (!_startArmed)
        {
            if (!ControlHeld())
                _startArmed = true;
            return;
        }

        if (ControlPressedThisFrame())
            BeginPlay();
    }

    private static bool ControlHeld()
    {
        if (Pointer.current != null && Pointer.current.press.isPressed)
            return true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return true;

        if (Keyboard.current != null && Keyboard.current.anyKey.isPressed)
            return true;

        Gamepad pad = Gamepad.current;
        if (pad == null)
            return false;

        return pad.buttonSouth.isPressed
            || pad.buttonEast.isPressed
            || pad.buttonWest.isPressed
            || pad.buttonNorth.isPressed
            || pad.leftStick.ReadValue().sqrMagnitude > 0.25f
            || pad.dpad.ReadValue().sqrMagnitude > 0.25f;
    }

    private static bool ControlPressedThisFrame()
    {
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
            return true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            return true;

        Gamepad pad = Gamepad.current;
        if (pad == null)
            return false;

        return pad.buttonSouth.wasPressedThisFrame
            || pad.buttonEast.wasPressedThisFrame
            || pad.buttonWest.wasPressedThisFrame
            || pad.buttonNorth.wasPressedThisFrame
            || pad.leftStick.ReadValue().sqrMagnitude > 0.25f
            || pad.dpad.ReadValue().sqrMagnitude > 0.25f;
    }

    private void BeginPlay()
    {
        if (!_choosingDifficulty)
            return;

        _choosingDifficulty = false;
        Time.timeScale = 1f;

        if (_levelsRoot != null)
            _levelsRoot.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.BeginPlay();
    }

    private static GameObject FindNamedObject(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform transform = transforms[i];
            if (transform != null && transform.name == objectName)
                return transform.gameObject;
        }

        return null;
    }

    private void BindDifficultyDropdown()
    {
        if (_difficultyBound || GameManager.Instance == null)
            return;

        if (_difficultyDropdown == null)
            _difficultyDropdown = FindDifficultyDropdown();

        if (_difficultyDropdown == null)
            return;

        _difficultyDropdown.onValueChanged.AddListener(OnDifficultyPicked);
        _difficultyBound = true;
        if (GameManager.Instance != null)
            GameManager.Instance.SetDifficultyLevel(_difficultyDropdown.value);
        ShowModeLabel();
    }

    private void UnbindDifficultyDropdown()
    {
        if (!_difficultyBound || _difficultyDropdown == null)
        {
            _difficultyBound = false;
            return;
        }

        _difficultyDropdown.onValueChanged.RemoveListener(OnDifficultyPicked);
        _difficultyBound = false;
    }

    private void OnDifficultyPicked(int index)
    {
        if (GameManager.Instance == null)
            return;

        GameManager.Instance.SetDifficultyLevel(index);
        ShowSelectedLevel();
    }

    private void ShowModeLabel()
    {
        if (_difficultyDropdown == null || _difficultyDropdown.captionText == null)
            return;

        if (_difficultyDropdown.captionText.text == "Mode")
            return;

        _difficultyDropdown.captionText.text = "Mode";
    }

    private void ShowSelectedLevel()
    {
        _levelChosen = true;

        if (_difficultyDropdown == null || _difficultyDropdown.captionText == null)
            return;

        int index = _difficultyDropdown.value;
        if (index < 0 || index >= _difficultyDropdown.options.Count)
            return;

        _difficultyDropdown.captionText.text = _difficultyDropdown.options[index].text;
    }

    private static TMP_Dropdown FindDifficultyDropdown()
    {
        TMP_Dropdown[] dropdowns = FindObjectsByType<TMP_Dropdown>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < dropdowns.Length; i++)
        {
            TMP_Dropdown dropdown = dropdowns[i];
            if (dropdown != null
                && dropdown.gameObject.name.IndexOf("difficulty", System.StringComparison.OrdinalIgnoreCase) >= 0)
                return dropdown;
        }

        return null;
    }

    public void UpdateScore(int score)
    {
        if (_scoreText == null)
            return;

        _scoreText.text = $"Score: {score}";
    }

    public void UpdateGhostsReturned(int ghostsReturned)
    {
        if (_ghostsReturnedText == null)
            ResolveGhostText();

        if (_ghostsReturnedText == null)
            return;

        _ghostsReturnedText.text = $"Returned: {ghostsReturned}";
    }

    private void ResolveGhostText()
    {
        if (_ghostsReturnedText != null)
            return;

        TMP_Text[] texts = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || text == _scoreText)
                continue;

            string name = text.gameObject.name;
            string body = text.text;
            if (MentionsReturnedGhost(name) || MentionsReturnedGhost(body))
            {
                _ghostsReturnedText = text;
                return;
            }
        }
    }

    private static bool MentionsReturnedGhost(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        return value.IndexOf("ghost", System.StringComparison.OrdinalIgnoreCase) >= 0
            || value.IndexOf("return", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public void UpdateLives(int livesRemaining)
    {
        if (_lifeImages == null)
            return;

        livesRemaining = Mathf.Clamp(livesRemaining, 0, _lifeImages.Length);

        for (int i = 0; i < _lifeImages.Length; i++)
        {
            if (_lifeImages[i] == null)
                continue;

            Color color = _lifeImages[i].color;
            color.a = i < livesRemaining ? 1f : _lostLifeAlpha;
            _lifeImages[i].color = color;
        }
    }

    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (_healthBar == null)
            return;

        _healthBar.Value = maxHealth > 0f
            ? Mathf.Clamp01(currentHealth / maxHealth)
            : 0f;

        if (_cardioStrip == null)
            _cardioStrip = _healthBar.GetComponentInChildren<CardioStrip>(true);

        if (_cardioStrip != null)
            _cardioStrip.SetPlaying(currentHealth > 0f);
    }
}
