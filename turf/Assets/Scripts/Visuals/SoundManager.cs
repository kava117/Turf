using UnityEngine;

// MonoBehaviour singleton. Plays AudioClips in response to game events.
// All clip fields are placeholder — assign real audio assets in the Inspector.
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip   clickClip;
    [SerializeField] private AudioClip   captureClip;
    [SerializeField] private AudioClip   gameOverClip;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        EventManager.OnTileCaptured += HandleTileCaptured;
        EventManager.OnGameOver     += HandleGameOver;
    }

    void OnDisable()
    {
        EventManager.OnTileCaptured -= HandleTileCaptured;
        EventManager.OnGameOver     -= HandleGameOver;
    }

    public void PlayClick()          => Play(clickClip);
    public void PlayCapture()        => Play(captureClip);
    public void PlayGameOver()       => Play(gameOverClip);

    private void HandleTileCaptured(Vector3Int cell, BaseMatchProfile owner) => PlayCapture();
    private void HandleGameOver(BaseMatchProfile winner)                      => PlayGameOver();

    private void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;
        audioSource.PlayOneShot(clip);
    }
}
