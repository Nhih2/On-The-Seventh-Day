using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmotionManager : MonoBehaviour
{
    #region SINGELTON
    public static EmotionManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    private const int MAX_EMOTION = 5;

    [SerializeField] private List<EmotionUI> emotionUIs;

    private List<Emotion> emotions = new();

    private void UpdateEmotions()
    {
        int i = 0;
        foreach (EmotionUI ui in emotionUIs) ui.emotion = null;
        foreach (Emotion emotionValue in emotions)
        {
            emotionUIs[i].emotion = emotionValue;
            i++;
        }
    }

    //

    public void AddEmotion(Emotion emotion)
    {
        if (emotions.Count >= MAX_EMOTION) return;
        emotions.Add(emotion);
        UpdateEmotions();
    }

    public void RemoveEmotion(Emotion emotion)
    {
        emotions.Remove(emotion);
        UpdateEmotions();
    }

    public bool CheckEmotion(Emo emo)
    {
        foreach (Emotion emotion in emotions)
        {
            if (emo == emotion.emotion) return true;
        }
        return false;
    }

    public Emo GetEmotion(int index)
    {
        return emotions[index].emotion;
    }

    public float ShowScore(string message, Emo emo, bool isMul)
    {
        int index = 0;
        foreach (Emotion emotion in emotions)
        {
            if (emo == emotion.emotion)
            {
                return emotionUIs[index].ShowScore(message, isMul);
            }
            index++;
        }
        return 0;
    }

    public void Showcase()
    {
        StartCoroutine(ShowcaseDelay());
    }

    private IEnumerator ShowcaseDelay()
    {
        int index = 1;
        foreach (EmotionUI ui in emotionUIs)
        {
            ui.ShowScore(index + "", true);
            index++;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
