using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DifficultyInfo : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    const string Body =
        "The graves have opened. Ghosts wander the cemetery, and the dead are up to drag them back. You are the guard. Gather the ghosts, keep the line behind you, and walk them into the vortex back to Hades.\n\n" +
        "HOW TO PLAY\n" +
        "On a keyboard, move with WASD or the arrow keys and swing with the mouse button or Enter. On a phone, use the joystick to move and the attack button to swing. On a gamepad, use the stick to move and the buttons to swing.\n\n" +
        "Touch a ghost and it joins the line. Walk into the vortex to send the whole line home. Returned ghosts add to your score. A life comes every 25 ghosts on Easy, and every 20 on Medium and Hard. You can hold up to 3.\n\n" +
        "You start with 3 lives and a full health bar. Enemy blows take health. When your health is gone you lose a life and, if one remains, you return to your post.\n\n" +
        "Your sword cuts down skeletons, slimes, and goblins. Defeating one adds a little to the score.\n\n" +
        "An enemy that catches you knocks one ghost off the back of the line. The extra enemies go after that back of the line themselves. Each one takes a single ghost, then has to finish its swing before it can take another.\n\n" +
        "A ghost left alone wanders off. Most ghosts are worth a little. Blue ghosts are worth more. Green ghosts are worth the most. A hostile ghost wanders with them and will not join the line. Step close and it strikes. Touch it and you take a hit. Your sword can cut it down.\n\n" +
        "DIFFICULTY\n" +
        "Easy. Two slow enemies, the next pair after both fall. They stay apart. The second hunts the line. Ghosts every 4 seconds. Light hits.\n\n" +
        "Medium. Four faster enemies, the next four after all fall. Three hunt the line. They stay apart. Ghosts every 3 seconds. Harder hits.\n\n" +
        "Hard. Four fast enemies, the next four after all fall. Three hunt the line. They stay apart. Ghosts every 2 seconds. The hardest hits.";

    [SerializeField] private Font _comedyFont;

    Font _uiFont;
    GameObject _overlay;
    Text _story;
    bool _built;

    void OnEnable()
    {
        Build();
    }

    void OnDisable()
    {
        IsOpen = false;
        if (_overlay != null)
            _overlay.SetActive(false);
    }

    void Build()
    {
        if (_built)
            return;

        RectTransform panel = transform.Find("LevelsPanel") as RectTransform;
        if (panel == null)
            return;

        _built = true;
        _uiFont = Resources.Load<Font>("ComicNeue-Bold");
        if (_uiFont == null)
            _uiFont = _comedyFont;
        if (_uiFont == null)
            _uiFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        Sprite round = MakeSprite(64, 22);
        Sprite circle = MakeSprite(64, 32);

        WireInfoButton(panel);
        WireExitButton(panel);
        CreateResetButton(panel);
        CreateOverlay(panel, round, circle);
    }

    void CreateResetButton(RectTransform panel)
    {
        if (panel.Find("ResetScoresButton") != null)
            return;

        RectTransform buttonRect = CreateRect("ResetScoresButton", panel);
        buttonRect.anchorMin = buttonRect.anchorMax = Vector2.zero;
        buttonRect.pivot = Vector2.zero;
        buttonRect.anchoredPosition = new Vector2(170f, 96f);
        buttonRect.sizeDelta = new Vector2(340f, 52f);
        buttonRect.SetAsLastSibling();

        TextMeshProUGUI label = buttonRect.gameObject.AddComponent<TextMeshProUGUI>();
        TMP_Text sample = panel.GetComponentInChildren<TMP_Text>(true);
        if (sample != null)
            label.font = sample.font;

        label.text = "Reset Scores";
        label.fontSize = 32;
        label.fontStyle = FontStyles.Underline;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        label.color = Color.white;
        label.raycastTarget = true;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;

        Button button = buttonRect.gameObject.AddComponent<Button>();
        button.targetGraphic = label;
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.185f, 0.802f, 0.194f, 1f);
        colors.highlightedColor = new Color(0.55f, 1f, 0.5f, 1f);
        colors.pressedColor = new Color(0.1f, 0.45f, 0.12f, 1f);
        colors.selectedColor = colors.normalColor;
        button.colors = colors;
        button.onClick.AddListener(() =>
        {
            GameOverUI.ResetAllHighScores();
            label.text = "Scores Cleared";
        });
    }

    void WireInfoButton(RectTransform panel)
    {
        Transform existing = panel.Find("InfoButton");
        if (existing == null || !existing.TryGetComponent(out Button button))
            return;

        button.onClick.AddListener(Show);
    }

    void WireExitButton(RectTransform panel)
    {
        Transform existing = panel.Find("ExitButton");
        if (existing == null || !existing.TryGetComponent(out Button button))
            return;

        button.onClick.AddListener(ExitToMenu);
    }

    void ExitToMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMenu();
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    void CreateOverlay(RectTransform panel, Sprite round, Sprite circle)
    {
        RectTransform root = CreateRect("InfoOverlay", panel);
        Stretch(root, 0f);
        root.gameObject.SetActive(false);
        _overlay = root.gameObject;

        RectTransform dim = CreateRect("Dim", root);
        Stretch(dim, 0f);
        Image dimImage = dim.gameObject.AddComponent<Image>();
        dimImage.color = new Color(0.08f, 0.04f, 0.1f, 0.72f);
        Button dimButton = dim.gameObject.AddComponent<Button>();
        dimButton.targetGraphic = dimImage;
        dimButton.onClick.AddListener(Hide);

        RectTransform shadow = CreateRect("CardShadow", root);
        shadow.anchorMin = shadow.anchorMax = new Vector2(0.5f, 0.5f);
        shadow.sizeDelta = new Vector2(1624f, 1024f);
        shadow.anchoredPosition = new Vector2(14f, -16f);
        Image shadowImage = shadow.gameObject.AddComponent<Image>();
        shadowImage.sprite = round;
        shadowImage.type = Image.Type.Sliced;
        shadowImage.color = new Color(0.25f, 0.08f, 0.22f, 0.55f);
        shadowImage.raycastTarget = false;

        RectTransform card = CreateRect("Card", root);
        card.anchorMin = card.anchorMax = new Vector2(0.5f, 0.5f);
        card.sizeDelta = new Vector2(1600f, 1000f);
        card.anchoredPosition = Vector2.zero;
        Image cardImage = card.gameObject.AddComponent<Image>();
        cardImage.sprite = round;
        cardImage.type = Image.Type.Sliced;
        cardImage.color = new Color(1f, 0.95f, 0.84f, 1f);

        RectTransform title = CreateRect("Title", card);
        title.anchorMin = new Vector2(0f, 1f);
        title.anchorMax = new Vector2(1f, 1f);
        title.pivot = new Vector2(0.5f, 1f);
        title.sizeDelta = new Vector2(-48f, 90f);
        title.anchoredPosition = new Vector2(0f, -12f);
        Text titleText = AddLabel(title, "Ghost Collector", 54, TextAnchor.MiddleCenter, new Color(0.45f, 0.12f, 0.22f, 1f));
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform close = CreateRect("Close", card);
        close.anchorMin = new Vector2(0.5f, 0f);
        close.anchorMax = new Vector2(0.5f, 0f);
        close.pivot = new Vector2(0.5f, 0f);
        close.sizeDelta = new Vector2(280f, 86f);
        close.anchoredPosition = new Vector2(0f, 22f);
        Image closeImage = close.gameObject.AddComponent<Image>();
        closeImage.sprite = circle;
        closeImage.type = Image.Type.Sliced;
        closeImage.color = new Color(0.96f, 0.76f, 0.28f, 1f);
        Button closeButton = close.gameObject.AddComponent<Button>();
        closeButton.targetGraphic = closeImage;
        closeButton.onClick.AddListener(Hide);

        RectTransform closeLabel = CreateRect("Label", close);
        Stretch(closeLabel, 0f);
        Text closeText = AddLabel(closeLabel, "Got it", 36, TextAnchor.MiddleCenter, new Color(0.28f, 0.1f, 0.38f, 1f));
        closeText.horizontalOverflow = HorizontalWrapMode.Overflow;
        closeText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform storyRect = CreateRect("Story", root);
        storyRect.anchorMin = storyRect.anchorMax = new Vector2(0.5f, 0.5f);
        storyRect.pivot = new Vector2(0.5f, 0.5f);
        storyRect.sizeDelta = new Vector2(1480f, 760f);
        storyRect.anchoredPosition = new Vector2(0f, -36f);
        _story = AddLabel(storyRect, Body, 24, TextAnchor.UpperLeft, new Color(0.1f, 0.05f, 0.05f, 1f));
        _story.horizontalOverflow = HorizontalWrapMode.Wrap;
        _story.verticalOverflow = VerticalWrapMode.Overflow;
        _story.alignByGeometry = false;

        close.SetParent(root, true);
        close.SetAsLastSibling();
        root.SetAsLastSibling();
    }

    void Show()
    {
        if (_overlay == null)
            return;

        _overlay.SetActive(true);
        _overlay.transform.SetAsLastSibling();
        IsOpen = true;
        if (_story != null)
        {
            _story.font = _uiFont;
            _story.text = Body;
        }
    }

    void Hide()
    {
        if (_overlay != null)
            _overlay.SetActive(false);

        IsOpen = false;
    }

    Text AddLabel(RectTransform rect, string message, int size, TextAnchor anchor, Color color)
    {
        Text label = rect.gameObject.AddComponent<Text>();
        label.font = _uiFont;
        label.fontSize = size;
        label.alignment = anchor;
        label.color = color;
        label.supportRichText = false;
        label.raycastTarget = false;
        label.text = message;
        return label;
    }

    static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = 5;
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    static void Stretch(RectTransform rect, float inset)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    static Sprite MakeSprite(int size, int radius)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[size * size];
        float limit = radius;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;
                float cx = Mathf.Clamp(px, limit, size - limit);
                float cy = Mathf.Clamp(py, limit, size - limit);
                float edge = limit - Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                pixels[y * size + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(edge));
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));
    }
}
