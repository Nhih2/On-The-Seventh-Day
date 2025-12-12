using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [Header("Audio Settings")]
    public AudioSource audioSourcePrefab;

    [Header("Sound Clips")]
    public List<AudioClip> sfxClips;

    public enum SfxType
    {
        SelectCard,
        EnterCard,
        Score,
        CashOut,
        ButtonPress,
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    public void PlaySfx(SfxType type, float volume = 0.5f, float pitch = 1)
    {
        int index = (int)type;
        if (index >= 0 && index < sfxClips.Count && sfxClips[index] != null)
        {
            AudioSource source = Instantiate(audioSourcePrefab, transform);
            source.volume = volume * GameSetting.MASTER_SFX_VOLUME;
            source.pitch = Random.Range(0.95f, 1.05f) * pitch;
            source.PlayOneShot(sfxClips[index]);
            Destroy(source.gameObject, sfxClips[index].length + 0.1f);
        }
        else
        {
            Debug.LogWarning($"SfxManager: No clip assigned for SfxType.{type}");
        }
    }
    public void PlaySfxForDuration(SfxType type, float duration, float volume = 0.5f, float pitch = 1)
    {
        int index = (int)type;
        if (index >= 0 && index < sfxClips.Count && sfxClips[index] != null)
        {
            StartCoroutine(PlayAndStopAfterTime(sfxClips[index], duration, volume, Random.Range(0.95f, 1.05f) * pitch));
        }
        else
        {
            Debug.LogWarning($"SfxManager: No clip assigned for SfxType.{type}");
        }
    }

    private IEnumerator PlayAndStopAfterTime(AudioClip clip, float duration, float volume, float pitch)
    {
        GameObject tempGO = new GameObject("SFX_Timed_" + clip.name);
        tempGO.transform.SetParent(transform);

        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume * GameSetting.MASTER_SFX_VOLUME;
        source.pitch = pitch;
        source.Play();

        yield return new WaitForSeconds(duration);

        source.Stop();
        Destroy(tempGO);
    }
    public void ButtonPressSfx()
    {
        PlaySfx(SfxType.SelectCard, 0.15f, 1.5f);
    }
}
