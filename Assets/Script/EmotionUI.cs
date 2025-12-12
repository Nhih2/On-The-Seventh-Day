using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EmotionUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Emotion emotion;

    [SerializeField] private Vector3 PopupOffset;
    [SerializeField] private Image emo;
    [SerializeField] private SpriteRenderer InnerBG;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private Image image;

    private bool isHover = false;

    void Start()
    {
        HideScore();
    }

    void Update()
    {
        NonHoverHandle();
    }

    private void NonHoverHandle()
    {
        if (!isHover)
        {
            if (emotion != null)
            {
                emo.sprite = emotion.emotion.GetSprite();
                emo.color = emo.color.ChangeTransparency(1f);
                InnerBG.color = InnerBG.color.ChangeTransparency(1f);
            }
            else
            {
                emo.color = emo.color.ChangeTransparency(0f);
                InnerBG.color = InnerBG.color.ChangeTransparency(0.2f);
            }
        }
        else if (emotion == null) isHover = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (emotion == null) return;
        SfxManager.Instance.ButtonPressSfx();
        EmotionManager.Instance.RemoveEmotion(emotion);
        isHover = false;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (emotion == null) return;
        SfxManager.Instance.PlaySfx(SfxManager.SfxType.EnterCard, 0.005f);
        isHover = true;
        InnerBG.color = InnerBG.color.ChangeTransparency(0.8f);
        ShowMessage();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (emotion == null) return;
        isHover = false;
        HideMessage();
    }

    public void ShowMessage()
    {
        if (emotion != null)
        {
            PopupManager.Instance.ShowMessage(
                    transform,
                    PopupOffset,
                    emotion.name,
                    emotion.des);
        }
    }

    public void HideMessage()
    {
        PopupManager.Instance.HideMessage();
    }

    public float ShowScore(string score, bool isMult)
    {
        this.score.gameObject.SetActive(true);
        image.gameObject.SetActive(true);
        this.score.text = score;
        return ShowAnimation(isMult);
    }

    private float ShowAnimation(bool isMult)
    {
        Vector3 targetScale = new(1, 1, 1), baseScale = new(0, 0, 0), imgScale = new(0.75f, 0.75f, 0.75f);
        Vector3 targetRotate = new(0, 0, UnityEngine.Random.Range(45, 90)), baseRotation = new(0, 0, 0);
        Ease tweenEaseIn = Ease.OutBack, tweenEaseOut = Ease.InBack;
        float fadeTime = 0.5f, delayTime = 0.15f;

        score.transform.localScale = baseScale;
        image.color = GameSetting.MULT_COLOR;
        if (!isMult)
            image.color = GameSetting.SCORE_COLOR;
        image.color = image.color.ChangeTransparency(1);
        image.transform.localScale = baseScale;
        image.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 45));

        Sequence seq = DOTween.Sequence();
        seq.Append(score.transform.DOScale(targetScale, delayTime).SetEase(tweenEaseIn));
        seq.AppendInterval(delayTime);
        seq.Append(score.transform.DOScale(baseScale, delayTime).SetEase(tweenEaseOut));

        Sequence seqImg = DOTween.Sequence();
        seqImg.Append(image.transform.DOScale(imgScale, fadeTime - delayTime).SetEase(tweenEaseIn));
        seqImg.Join(image.GetComponent<RectTransform>()
            .DORotate(new Vector3(0, 0, targetRotate.z), fadeTime - delayTime, RotateMode.FastBeyond360)
            .SetEase(tweenEaseIn));
        seqImg.Join(image.DOFade(0, fadeTime).SetEase(tweenEaseIn));
        seqImg.AppendCallback(() =>
        {
            image.gameObject.SetActive(false);
            score.gameObject.SetActive(false);
        });

        return fadeTime;
    }

    public void HideScore()
    {
        score.gameObject.SetActive(false);
        image.gameObject.SetActive(false);
    }
}
