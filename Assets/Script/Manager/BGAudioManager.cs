using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BGAudioManager : MonoBehaviour
{
    public static BGAudioManager Instance { get; private set; }

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    public float crossfadeDuration = 1.5f;

    [Header("Music Tracks (match enum order)")]
    public List<AudioClip> musicTracks;
    private float previousMasterVolume;

    public enum MusicType
    {
        GameStart,
        GameLoop
    }

    private AudioSource currentSource;
    private AudioSource nextSource;

    private bool isPlayingOnce = false;
    private MusicType nextQueuedTrack;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentSource = gameObject.AddComponent<AudioSource>();
        nextSource = gameObject.AddComponent<AudioSource>();

        currentSource.loop = true;
        nextSource.loop = true;

        currentSource.volume = 0;
        nextSource.volume = 0;

        PlayOnceThenTransition(MusicType.GameStart, MusicType.GameLoop);
        previousMasterVolume = GameSetting.MASTER_BG_VOLUME;
    }

    void Update()
    {
        if (isPlayingOnce && !currentSource.isPlaying)
        {
            isPlayingOnce = false;
            PlayMusic(nextQueuedTrack);
        }

        if (!Mathf.Approximately(previousMasterVolume, GameSetting.MASTER_BG_VOLUME))
        {
            previousMasterVolume = GameSetting.MASTER_BG_VOLUME;
            UpdateVolume();
        }
    }
    private void UpdateVolume()
    {
        currentSource.volume = musicVolume * GameSetting.MASTER_BG_VOLUME;
        nextSource.volume = musicVolume * GameSetting.MASTER_BG_VOLUME;
    }

    public void PlayMusic(MusicType type)
    {
        int index = (int)type;

        if (index >= 0 && index < musicTracks.Count && musicTracks[index] != null)
        {
            AudioClip newClip = musicTracks[index];

            if (currentSource.clip == newClip && currentSource.isPlaying)
                return;

            StartCoroutine(CrossfadeToClip(newClip));
        }
        else
        {
            Debug.LogWarning($"MusicManager: No music clip assigned for MusicType.{type}");
        }
    }

    private IEnumerator CrossfadeToClip(AudioClip newClip)
    {
        AudioSource oldSource = currentSource;
        AudioSource fadeInSource = nextSource;

        fadeInSource.clip = newClip;
        fadeInSource.volume = 0f;
        fadeInSource.loop = true;
        fadeInSource.Play();

        currentSource = fadeInSource;
        nextSource = oldSource;

        float timer = 0f;
        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / crossfadeDuration;

            fadeInSource.volume = Mathf.Lerp(0f, musicVolume * GameSetting.MASTER_BG_VOLUME, t);
            oldSource.volume = Mathf.Lerp(musicVolume * GameSetting.MASTER_BG_VOLUME, 0f, t);

            yield return null;
        }

        fadeInSource.volume = musicVolume * GameSetting.MASTER_BG_VOLUME;
        oldSource.Stop();
    }

    public void StopMusic()
    {
        currentSource.Stop();
        nextSource.Stop();
        isPlayingOnce = false;
    }

    public void PlayOnceThenTransition(MusicType playOnceType, MusicType nextType)
    {
        int index = (int)playOnceType;
        if (index >= 0 && index < musicTracks.Count && musicTracks[index] != null)
        {
            currentSource.Stop();
            nextSource.Stop();

            currentSource.clip = musicTracks[index];
            currentSource.loop = false;
            currentSource.volume = musicVolume * GameSetting.MASTER_BG_VOLUME;
            currentSource.Play();

            isPlayingOnce = true;
            nextQueuedTrack = nextType;
        }
        else
        {
            Debug.LogWarning($"MusicManager: No clip assigned for MusicType.{playOnceType}");
        }
    }
}
