using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private GameOverUI _gameOverUI;

    public DifficultyLevel DifficultyLevel = DifficultyLevel.Easy;

    public int Score { get; private set; }
    public int GhostsReturned { get; private set; }
    public float EnemyMoveSpeed { get; private set; } = 1f;
    public int EnemyDamage { get; private set; } = 10;
    public int GhostsLostPerContact { get; private set; } = 1;
    public int EnemiesAtOnce { get; private set; } = 2;
    public float EnemySpawnGap { get; private set; }
    public float GhostSpawnInterval { get; private set; } = 4f;
    public float PowerStarInterval { get; private set; } = 37f;
    public bool PlayStarted { get; private set; }

    public event Action<int> ScoreChanged;
    public event Action<int> GhostsReturnedChanged;

    [SerializeField] private int _ghostsPerLife = 25;
    private int _ghostsTowardNextLife;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SetDifficultyLevel(DifficultyLevel);
    }

    public void AddPoints(int amount)
    {
        if (amount <= 0)
            return;

        Score += amount;
        ScoreChanged?.Invoke(Score);
    }

    public void SetDifficultyLevel(int index)
    {
        if (index < 0 || index > (int)DifficultyLevel.Hard)
            index = 0;

        SetDifficultyLevel((DifficultyLevel)index);
    }

    public void SetDifficultyLevel(DifficultyLevel difficultyLevel)
    {
        DifficultyLevel = difficultyLevel;

        switch (difficultyLevel)
        {
            case DifficultyLevel.Easy:
                EnemyMoveSpeed = 1f;
                EnemyDamage = 10;
                GhostsLostPerContact = 1;
                EnemiesAtOnce = 2;
                EnemySpawnGap = 0f;
                GhostSpawnInterval = 4f;
                PowerStarInterval = 37f;
                _ghostsPerLife = 25;
                break;
            case DifficultyLevel.Medium:
                EnemyMoveSpeed = 2f;
                EnemyDamage = 15;
                GhostsLostPerContact = 1;
                EnemiesAtOnce = 4;
                EnemySpawnGap = 3f;
                GhostSpawnInterval = 3f;
                PowerStarInterval = 30f;
                _ghostsPerLife = 20;
                break;
            case DifficultyLevel.Hard:
                EnemyMoveSpeed = 3f;
                EnemyDamage = 20;
                GhostsLostPerContact = 1;
                EnemiesAtOnce = 4;
                EnemySpawnGap = 3f;
                GhostSpawnInterval = 2f;
                PowerStarInterval = 23f;
                _ghostsPerLife = 20;
                break;
        }

        EnemyController[] enemies = FindObjectsByType<EnemyController>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);

        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
                enemies[i].ApplyCombat();
        }
    }

    public void AddReturnedGhosts(int count)
    {
        if (count <= 0)
            return;

        GhostsReturned += count;
        _ghostsTowardNextLife += count;
        GhostsReturnedChanged?.Invoke(GhostsReturned);

        int livesEarned = 0;
        int ghostsPerLife = Mathf.Max(1, _ghostsPerLife);
        if (_ghostsTowardNextLife >= ghostsPerLife)
        {
            livesEarned = _ghostsTowardNextLife / ghostsPerLife;
            _ghostsTowardNextLife %= ghostsPerLife;
        }

        if (livesEarned <= 0)
            return;

        PlayerHealth player = FindFirstObjectByType<PlayerHealth>();
        if (player != null)
            player.AddLives(livesEarned);
    }

    public void ResetScore()
    {
        Score = 0;
        ScoreChanged?.Invoke(Score);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void BeginPlay()
    {
        PlayStarted = true;
    }

    public void StartGame()
    {
        PlayStarted = false;
        Score = 0;
        GhostsReturned = 0;
        _ghostsTowardNextLife = 0;
        SceneManager.LoadScene("Level_1");
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        PlayStarted = false;
        Score = 0;
        GhostsReturned = 0;
        _ghostsTowardNextLife = 0;

        if (Instance == this)
            Instance = null;

        Destroy(gameObject);
        SceneManager.LoadScene("MainMenuScene");
    }

    public void EndGame()
    {
        _gameOverUI.ShowGameOver(Score, DifficultyLevel);
        Time.timeScale = 0;
        
    }
    public void RegisterGameOverUI(GameOverUI gameOverUI)

    {

        _gameOverUI = gameOverUI;

    }

    public void UnregisterGameOverUI(GameOverUI gameOverUI)

    {

        if (_gameOverUI == gameOverUI)

            _gameOverUI = null;

    }
}
