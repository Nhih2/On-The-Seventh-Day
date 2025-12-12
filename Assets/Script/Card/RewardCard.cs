using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RewardCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Reward reward { get; private set; }

    [SerializeField] private Vector3 PopupOffset;
    [SerializeField] private Image emotionIcon, sceneIcon;
    [SerializeField] private SpriteRenderer background, selectedBG;
    [SerializeField] private Image centerIcon, centerIcon2;
    [SerializeField] private List<TextMeshProUGUI> cardNames;
    [SerializeField] private List<Image> cardCornerIcons;
    [SerializeField] private GameObject cardObj, emoObj;
    [SerializeField] private Mask mask;
    [SerializeField] private GameObject duoObj;
    private CardBody cardBody;

    private RewardManager Manager;

    public void Initialize(RewardManager manager, Reward reward, Vector3 ogPos)
    {
        this.reward = reward;
        Manager = manager;
        transform.position = ogPos;
        selectedBG.color = selectedBG.color.ChangeTransparency(0f);
        background.color = background.color.ChangeTransparency(0f);
        cardBody = GetComponent<CardBody>();
    }

    //

    void Update()
    {
        HandleNameAndDes();
    }

    private void HandleNameAndDes()
    {
        if (reward.emotion != null)
        {
            cardObj.SetActive(false);
            emoObj.SetActive(true);
            emotionIcon.sprite = reward.emotion.emotion.GetSprite();
            sceneIcon.gameObject.SetActive(false);
            emotionIcon.gameObject.SetActive(true);
        }
        else if (reward.scenery != null)
        {
            cardObj.SetActive(false);
            emoObj.SetActive(true);
            sceneIcon.sprite = reward.scenery.sce.GetSprite();
            sceneIcon.gameObject.SetActive(true);
            emotionIcon.gameObject.SetActive(false);
        }
        else if (reward.playCard != null)
        {
            for (int i = 0; i < cardNames.Count; i++)
            {
                cardObj.SetActive(true);
                emoObj.SetActive(false);
                if (i % 2 == 0)
                {
                    cardNames[i].text = GameSetting.STATS_NAME[reward.playCard.type1];
                    cardCornerIcons[i].sprite = GameManager.Instance.sprites[reward.playCard.type1];
                    centerIcon.sprite = GameManager.Instance.sprites[reward.playCard.type1];
                    if (reward.playCard.type1 % 2 == 0)
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
                    if (reward.playCard.type2 != -1)
                    {
                        DuoMode();
                        cardNames[i].text = GameSetting.STATS_NAME[reward.playCard.type2];
                        cardNames[i].gameObject.SetActive(true);
                        centerIcon2.sprite = GameManager.Instance.sprites[reward.playCard.type2];
                        cardCornerIcons[i].sprite = GameManager.Instance.sprites[reward.playCard.type2];
                        cardCornerIcons[i].gameObject.SetActive(true);
                        if (reward.playCard.type2 % 2 == 0)
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
        else
        {
            cardObj.SetActive(false);
            emoObj.SetActive(false);
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

    public void Unselect()
    {
        //selectedBG.color = selectedBG.color.ChangeTransparency(0f);
        if (cardBody.isSelect)
            cardBody.Select(false);
    }

    public void Select()
    {
        //selectedBG.color = selectedBG.color.ChangeTransparency(1f);
        if (!cardBody.isSelect)
            cardBody.Select(true);
    }

    //

    public void OnPointerEnter(PointerEventData eventData)
    {
        //background.color = background.color.ChangeTransparency(0.4f);
        ShowMessage();
        //SfxManager.Instance.PlaySfx(SfxManager.SfxType.EnterCard, 0.005f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //background.color = background.color.ChangeTransparency(0f);
        HideMessage();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard);
        Manager.SelectCard(this);
    }

    public void ShowMessage()
    {
        if (reward.playCard == null)
        {
            if (reward.emotion != null)
            {
                PopupManager.Instance.ShowMessage(
                        transform,
                        PopupOffset,
                        reward.emotion.name,
                        reward.emotion.des);
            }
            else if (reward.scenery != null)
            {
                PopupManager.Instance.ShowMessage(
                        transform,
                        PopupOffset,
                        reward.scenery.name,
                        reward.scenery.des);
            }
        }
        else
        {
            if (reward.playCard.type1 == -1) return;
            if (reward.playCard.type2 == -1)
            {
                PopupManager.Instance.ShowMessage(
                    transform,
                    PopupOffset,
                    GameSetting.STATS_FULLNAME[reward.playCard.type1],
                    GameSetting.STATS_FULLDESCRIPTION[reward.playCard.type1]);
            }
            else
            {
                if (reward.playCard.type1 < GameSetting.STATS_THRESHOLD && reward.playCard.type2 < GameSetting.STATS_THRESHOLD)
                {
                    PopupManager.Instance.ShowMessage(
                        transform,
                        PopupOffset,
                        GameSetting.STATS_NAME[reward.playCard.type1] + " / " + GameSetting.STATS_NAME[reward.playCard.type2],
                        $"+10.\nCount toward {GameSetting.STATS_NAME[reward.playCard.type1]} and {GameSetting.STATS_NAME[reward.playCard.type2]}.");
                }
                else
                {
                    PopupManager.Instance.ShowMessage(
                        transform,
                        PopupOffset,
                        GameSetting.STATS_NAME[reward.playCard.type1] + " / " + GameSetting.STATS_NAME[reward.playCard.type2],
                        $"{GameSetting.STATS_FULLDESCRIPTION[reward.playCard.type1]}\n{GameSetting.STATS_FULLDESCRIPTION[reward.playCard.type2]}");
                }
            }
        }
    }

    public void HideMessage()
    {
        PopupManager.Instance.HideMessage();
    }
}
