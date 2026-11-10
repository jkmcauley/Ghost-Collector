using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour

{

    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]

    [SerializeField] private AudioSource _musicSource;

    [SerializeField] private AudioSource _sfxSource;

    [Header("Background Music")]

    [SerializeField] private AudioClip _backgroundMusic;

    [Header("Sound Effects")]

    [SerializeField] private AudioClip _skeleton;

    [SerializeField] private AudioClip _slime;

    [SerializeField] private AudioClip _goblin;

    [SerializeField] private AudioClip _undead;

    [SerializeField] private AudioClip _bats;

    [SerializeField] private AudioClip _extra;
    [SerializeField] private AudioClip _ghostCollect;
    
    [SerializeField] private AudioClip _powerUp;
    [SerializeField] private AudioClip _sword;
    [SerializeField] private AudioClip _whoosh;
    [SerializeField] private AudioClip _taDa;
    [SerializeField] private AudioClip _gameover;
    [SerializeField] private AudioClip _playerHurt;

    private AudioSource _powerSource;
    private AudioSource _gameOverSource;
    private Coroutine _gameOverRoutine;

    private void Awake()

    {

        if (Instance != null && Instance != this)

        {

            Destroy(gameObject);

            return;

        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        _musicSource.playOnAwake = false;

        _musicSource.loop = true;

        _sfxSource.playOnAwake = false;

        _sfxSource.loop = false;

    }

    private void Start()

    {

        PlayMusic();

    }

    public void PlayMusic()

    {

        if (_backgroundMusic == null)

            return;

        _musicSource.clip = _backgroundMusic;

        _musicSource.Play();

    }

    public void StopMusic()

    {

        _musicSource.Stop();

    }

    public void PlaySkeleton() => PlaySound(_skeleton);

    public void PlaySlime() => PlaySound(_slime);

    public void PlayGoblin() => PlaySound(_goblin);

    public void PlayUndead() => PlaySound(_undead);

    public void PlayBats() => PlaySound(_bats);
    public void PlayGhostCollect() => PlaySound(_ghostCollect);
    public void PowerUpCollect() => PlayPowerUp();
    public void PlaySword() => PlaySound(_sword);
    public void PlayWhoosh() => PlaySound(_whoosh);
    public void PlayTaDa() => PlaySound(_taDa);
    public void PlayerHurt() => PlaySound(_playerHurt);

    public void PlayGameover()
    {
        if (_gameover == null)
            return;

        if (_gameOverRoutine != null)
            StopCoroutine(_gameOverRoutine);

        _gameOverRoutine = StartCoroutine(PlayGameOverRoutine());
    }

    public void PlayPowerUp()
    {
        if (_powerUp == null)
            return;

        EnsurePowerSource();
        _powerSource.mute = !SoundOn;
        _powerSource.clip = _powerUp;
        _powerSource.Play();
    }

    public void StopPowerUp()
    {
        if (_powerSource != null)
            _powerSource.Stop();
    }

    private void EnsurePowerSource()
    {
        if (_powerSource != null)
            return;

        _powerSource = gameObject.AddComponent<AudioSource>();
        _powerSource.playOnAwake = false;
        _powerSource.loop = false;
    }
     

    public void PlayExtra() => PlaySound(_extra);

    public bool SoundOn { get; private set; } = true;

    public void SetSoundOn(bool on)
    {
        SoundOn = on;
        if (_musicSource != null)
            _musicSource.mute = !on;
        if (_sfxSource != null)
            _sfxSource.mute = !on;
        if (_powerSource != null)
            _powerSource.mute = !on;
        if (_gameOverSource != null)
            _gameOverSource.mute = !on;
    }

    public void ToggleSound()
    {
        SetSoundOn(!SoundOn);
    }

    private IEnumerator PlayGameOverRoutine()
    {
        EnsureGameOverSource();
        _gameOverSource.mute = !SoundOn;
        _gameOverSource.loop = false;
        _gameOverSource.clip = _gameover;

        for (int i = 0; i < 3; i++)
        {
            _gameOverSource.Play();
            if (_gameover.length > 0f)
                yield return new WaitForSecondsRealtime(_gameover.length);
        }

        _gameOverRoutine = null;
    }

    private void EnsureGameOverSource()
    {
        if (_gameOverSource != null)
            return;

        _gameOverSource = gameObject.AddComponent<AudioSource>();
        _gameOverSource.playOnAwake = false;
        _gameOverSource.loop = false;
    }

    private void PlaySound(AudioClip clip)

    {

        if (clip != null)

            _sfxSource.PlayOneShot(clip);

    }

    private void OnDestroy()

    {

        if (Instance == this)

            Instance = null;

    }

}
