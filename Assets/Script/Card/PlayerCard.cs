using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    public PlayCard card { get; private set; } = new();

    private bool init = false;
    [SerializeField] private Vector3 PopupOffset;
    [SerializeField] private TextMeshProUGUI score;
    [SerializeField] private List<TextMeshProUGUI> cardNames;
    [SerializeField] private Image image, centerIcon, centerIcon2;
    [SerializeField] private List<Image> cardCornerIcons;
    [SerializeField] private Mask mask;
    [SerializeField] private GameObject duoObj;
    [SerializeField] private CardVisual cardVisual;

    private CardManager Manager;

    void Awake()
    {
        score.gameObject.SetActive(false);
        image.gameObject.SetActive(false);
    }

    //

    void Update()
    {
        Initialize();
        HandleText();
    }

    public void DestroyCard()
    {
        Manager.MoveToTrash(this);
    }

    public void ShowMessage()
    {
        if (card.type2 == -1)
        {
            PopupManager.Instance.ShowMessage(
                transform,
                PopupOffset,
                GameSetting.STATS_FULLNAME[card.type1],
                GameSetting.STATS_FULLDESCRIPTION[card.type1]);
        }
        else
        {
            if (card.type1 < GameSetting.STATS_THRESHOLD && card.type2 < GameSetting.STATS_THRESHOLD)
            {
                PopupManager.Instance.ShowMessage(
                    transform,
                    PopupOffset,
                    GameSetting.STATS_NAME[card.type1] + " / " + GameSetting.STATS_NAME[card.type2],
                    $"+10.\nCount toward {GameSetting.STATS_NAME[card.type1]} and {GameSetting.STATS_NAME[card.type2]}.");
            }
            else
            {
                PopupManager.Instance.ShowMessage(
                    transform,
                    PopupOffset,
                    GameSetting.STATS_NAME[card.type1] + " / " + GameSetting.STATS_NAME[card.type2],
                    $"{GameSetting.STATS_FULLDESCRIPTION[card.type1]}\n{GameSetting.STATS_FULLDESCRIPTION[card.type2]}");
            }
        }
    }

    public void HideMessage()
    {
        PopupManager.Instance.HideMessage();
    }

    private void Initialize()
    {
        if (init) return;
        if (!cardVisual.parentCard) return;
        init = true;
        cardVisual.parentCard.SelectEvent.AddListener(OnSelect);
    }

    private void OnSelect(Card card, bool selected)
    {
        //if (selected) Manager.SelectCard(this);
        //else Manager.UnselectCard(this);
    }

    private void HandleText()
    {
        if (card.type1 == -1)
        {
            Debug.LogError("Type-1" + card.type1 + " Type-2" + card.type2);
            return;
        }
        for (int i = 0; i < cardNames.Count; i++)
        {
            if (i % 2 == 0)
            {
                cardNames[i].text = GameSetting.STATS_NAME[card.type1];
                cardCornerIcons[i].sprite = GameManager.Instance.sprites[card.type1];
                centerIcon.sprite = GameManager.Instance.sprites[card.type1];
                if (card.type1 % 2 == 0)
                {
                    centerIcon.color = Color.red;
                    cardCornerIcons[i].color = Color.red;
                    cardNames[i].color = Color.red;
                }
                else
                {
                    centerIcon.color = Color.black;
                    cardCornerIcons[i].color = Color.black;
                    cardNames[i].color = Color.black;
                }
            }
            else
            {
                if (card.type2 != -1)
                {
                    DuoMode();
                    centerIcon2.sprite = GameManager.Instance.sprites[card.type2];
                    cardNames[i].text = GameSetting.STATS_NAME[card.type2];
                    cardNames[i].gameObject.SetActive(true);
                    cardCornerIcons[i].sprite = GameManager.Instance.sprites[card.type2];
                    cardCornerIcons[i].gameObject.SetActive(true);
                    if (card.type2 % 2 == 0)
                    {
                        centerIcon2.color = Color.red;
                        cardCornerIcons[i].color = Color.red;
                        cardNames[i].color = Color.red;
                    }
                    else
                    {
                        centerIcon2.color = Color.black;
                        cardCornerIcons[i].color = Color.black;
                        cardNames[i].color = Color.black;
                    }
                }
                else
                {
                    SingleMode();
                    cardNames[i].gameObject.SetActive(false);
                    cardCornerIcons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private void DuoMode()
    {
        duoObj.SetActive(true);
        mask.enabled = true;
    }

    private void SingleMode()
    {
        duoObj.SetActive(false);
        mask.enabled = false;
    }

    //

    public void Initialize(CardManager manager, PlayCard card)
    {
        this.card = card;
        Manager = manager;
    }
    //

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
    }
}