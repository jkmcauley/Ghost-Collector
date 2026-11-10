using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [Header("Canvas Panels")]
    [SerializeField] private GameObject _hud;
    [SerializeField] private GameObject _levels;
    [SerializeField] private GameObject _touchScreen;
    [SerializeField] private GameObject _gameOverHUD;

    [Header("Game Over Text")]
    [SerializeField] private TMP_Text _difficultyText;
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _highScoreText;

    [Header("Confetti - Scene Instance")]
    [SerializeField] private ParticleSystem _confetti;

    private static readonly Color[] ConfettiColors =
    {
        new Color(0.95f, 0.28f, 0.32f),
        new Color(1f, 0.82f, 0.22f),
        new Color(0.25f, 0.82f, 0.38f),
        new Color(0.28f, 0.62f, 1f),
        new Color(0.78f, 0.38f, 0.95f),
        new Color(1f, 0.55f, 0.2f)
    };

    private readonly List<ConfettiBit> _bits = new List<ConfettiBit>();
    private Sprite _confettiSprite;
    private bool _isGameOver;

    private void Awake()
    {
        _gameOverHUD.SetActive(false);

        if (_confetti != null)
            _confetti.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.Instance.RegisterGameOverUI(this);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterGameOverUI(this);
    }

    public static string HighScoreKey(DifficultyLevel difficulty)
    {
        return $"HighScore_{difficulty}";
    }

    public static void ResetAllHighScores()
    {
        foreach (DifficultyLevel difficulty in System.Enum.GetValues(typeof(DifficultyLevel)))
            PlayerPrefs.DeleteKey(HighScoreKey(difficulty));

        PlayerPrefs.Save();
    }

    public void ShowGameOver(int finalScore, DifficultyLevel difficulty)
    {
        if (_isGameOver)
            return;

        _isGameOver = true;

        string key = HighScoreKey(difficulty);
        int highScore = PlayerPrefs.GetInt(key, 0);
        bool isNewHighScore = finalScore > highScore;

        if (isNewHighScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt(key, highScore);
            PlayerPrefs.Save();
        }

        _hud.SetActive(false);
        _levels.SetActive(false);
        _touchScreen.SetActive(false);

        _difficultyText.text = $"{difficulty} Mode";
        _scoreText.text = $"Score: {finalScore:N0}";
        _highScoreText.text = $"High Score: {highScore:N0}";

        _gameOverHUD.SetActive(true);

        if (isNewHighScore)
            PlayConfetti();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopMusic();
            SoundManager.Instance.PlayGameover();
        }

        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (_bits.Count == 0)
            return;

        float step = Time.unscaledDeltaTime;
        for (int i = _bits.Count - 1; i >= 0; i--)
        {
            ConfettiBit bit = _bits[i];
            bit.Life -= step;
            if (bit.Life <= 0f)
            {
                Destroy(bit.Rect.gameObject);
                _bits.RemoveAt(i);
                continue;
            }

            bit.Velocity.y -= 320f * step;
            bit.Rect.anchoredPosition += bit.Velocity * step;
            bit.Rect.Rotate(0f, 0f, bit.Spin * step);

            Color color = bit.Image.color;
            color.a = Mathf.Clamp01(bit.Life / 0.6f);
            bit.Image.color = color;
            _bits[i] = bit;
        }
    }

    private void PlayConfetti()
    {
        if (_confetti != null)
            _confetti.gameObject.SetActive(false);

        RectTransform hud = _gameOverHUD.transform as RectTransform;
        if (hud == null)
            return;

        RectTransform trophy = FindTrophy(hud);
        Vector2 origin = new Vector2(0f, 40f);
        if (trophy != null)
        {
            origin = trophy.anchoredPosition;
            origin.y += trophy.sizeDelta.y * (1f - trophy.pivot.y) + 8f;
        }

        Sprite sprite = ConfettiSprite();
        for (int i = 0; i < 56; i++)
        {
            RectTransform rect = new GameObject("Confetti", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)).GetComponent<RectTransform>();
            rect.SetParent(hud, false);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(Random.Range(10f, 18f), Random.Range(22f, 36f));
            rect.anchoredPosition = origin + new Vector2(Random.Range(-70f, 70f), Random.Range(0f, 24f));
            rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            rect.SetAsLastSibling();

            Image image = rect.GetComponent<Image>();
            image.sprite = sprite;
            image.color = ConfettiColors[i % ConfettiColors.Length];
            image.raycastTarget = false;

            _bits.Add(new ConfettiBit
            {
                Rect = rect,
                Image = image,
                Velocity = new Vector2(Random.Range(-260f, 260f), Random.Range(520f, 860f)),
                Spin = Random.Range(-280f, 280f),
                Life = Random.Range(2.2f, 3.4f)
            });
        }
    }

    private RectTransform FindTrophy(RectTransform hud)
    {
        if (_confetti != null && _confetti.transform.parent is RectTransform parent)
            return parent;

        Transform named = hud.Find("TrophyIMG");
        return named as RectTransform;
    }

    private Sprite ConfettiSprite()
    {
        if (_confettiSprite != null)
            return _confettiSprite;

        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        texture.Apply();
        _confettiSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 100f);
        return _confettiSprite;
    }

    private struct ConfettiBit
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Velocity;
        public float Spin;
        public float Life;
    }
}
