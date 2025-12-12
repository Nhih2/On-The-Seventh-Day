using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class RewardManager : MonoBehaviour
{
    public List<RewardCard> cards { get; private set; } = new();

    private const int MAX_REWARD_CARDS = 3;

    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform background, cardPrefab;
    [SerializeField] private List<Transform> CardsPosition;
    [SerializeField] private Button cancelButton, confirmbutton;

    private BoolWrapper isSceneStart = new(false), willNext = new(false);
    private class BoolWrapper
    {
        public bool value;
        public BoolWrapper(bool val)
        {
            value = val;
        }
    }
    private RewardCard selectedCard;

    void Awake()
    {
        cancelButton.onClick.AddListener(() =>
        {
            if (isSceneStart.value) return;
            SfxManager.Instance.ButtonPressSfx();
            GameManager.Instance.TriggerRewardEnd(isSceneStart.value, willNext.value);
        });
        confirmbutton.onClick.AddListener(() =>
        {
            SfxManager.Instance.ButtonPressSfx();
            if (selectedCard)
            {
                if (selectedCard.reward.emotion != null)
                    EmotionManager.Instance.AddEmotion(selectedCard.reward.emotion);
                if (selectedCard.reward.playCard != null)
                    inventoryManager.AddCard(selectedCard.reward.playCard);
                if (selectedCard.reward.scenery != null)
                {
                    GameManager.Instance.curretScene = selectedCard.reward.scenery.sce;
                    GameManager.Instance.SceneCount++;
                    GameManager.Instance.RoundCount = 5;
                }
                GameManager.Instance.TriggerRewardEnd(isSceneStart.value, willNext.value);
            }
        });
        cancelButton.gameObject.SetActive(false);
        confirmbutton.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
    }


    #region EVENT HANDLER
    void Start()
    {
        GameManager.Instance.OnRewardEnd += RewardEnd;
        GameManager.Instance.OnRewardStart += RewardStart;
    }

    private void RewardEnd(bool isSceneStart, bool willNext)
    {
        foreach (RewardCard card in cards) Destroy(card.gameObject);
        cards.Clear();
        cancelButton.gameObject.SetActive(false);
        confirmbutton.gameObject.SetActive(false);
        background.gameObject.SetActive(false);
    }

    private void RewardStart(bool isSceneStart, bool willNext)
    {
        for (int i = 0; i < MAX_REWARD_CARDS; i++) DrawCard(isSceneStart);
        cancelButton.gameObject.SetActive(true);
        confirmbutton.gameObject.SetActive(true);
        background.gameObject.SetActive(true);
        this.isSceneStart.value = isSceneStart;
        this.willNext.value = willNext;
        if (isSceneStart) cancelButton.interactable = false;
        else cancelButton.interactable = true;
    }
    #endregion

    public void DrawCard(bool isSceneStart)
    {
        if (cards.Count < MAX_REWARD_CARDS)
        {
            cards.Add(Instantiate(cardPrefab).GetComponent<RewardCard>());
            cards[^1].Initialize(this, GetRandomReward(isSceneStart), GetLatestPosition());
        }
        if (isSceneStart && GameManager.Instance.FullScoreText != "")
        {
            DialogueManager.Instance.ShowScore(GameManager.Instance.FullScoreText);
        }
    }

    private Reward GetRandomReward(bool isSceneStart)
    {
        if (isSceneStart)
        {
            if (GameManager.Instance.SceneCount % 3 == 0) return new() { scenery = GameSetting.scenes[0] };
            if (GameManager.Instance.SceneCount % 3 == 2) return new() { scenery = GameSetting.scenes[2] };
            if (GameSetting.hasJob)
            {
                if (GameManager.Instance.SceneCount % 3 == 1 && cards.Count == 1) return new() { scenery = GameSetting.scenes[1] };
                int index = Random.Range(0, GameSetting.scenes.Count);
                while (index == 3)
                    index = Random.Range(0, GameSetting.scenes.Count);
                return new()
                {
                    scenery = GameSetting.scenes[index]
                };
            }
            else
            {
                if (GameManager.Instance.SceneCount % 3 == 1 && cards.Count == 1) return new() { scenery = GameSetting.scenes[3] };
                int index = Random.Range(0, GameSetting.scenes.Count);
                while (index == 1)
                    index = Random.Range(0, GameSetting.scenes.Count);
                return new()
                {
                    scenery = GameSetting.scenes[index]
                };
            }
        }
        else
        {
            if (Random.Range(0, 4) == 2)
                return new() { emotion = GameSetting.emotions[Random.Range(0, GameSetting.emotions.Count)] };
            else if (Random.Range(0, 3) == 1)
            {
                Reward rtnReward = new()
                {
                    playCard = new()
                    {
                        type1 = Random.Range(0, GameSetting.STATS_THRESHOLD),
                        type2 = Random.Range(0, GameSetting.STATS_THRESHOLD),
                    }
                };
                while (rtnReward.playCard.type1 == rtnReward.playCard.type2) rtnReward = new()
                {
                    playCard = new()
                    {
                        type1 = Random.Range(0, GameSetting.STATS_THRESHOLD),
                        type2 = Random.Range(0, GameSetting.STATS_THRESHOLD),
                    }
                };
                if (rtnReward.playCard.type1 < rtnReward.playCard.type2)
                    (rtnReward.playCard.type1, rtnReward.playCard.type2) = (rtnReward.playCard.type2, rtnReward.playCard.type1);
                return rtnReward;
            }
            else if (Random.Range(0, 3) == 1)
            {
                Reward rtnReward = new()
                {
                    playCard = new()
                    {
                        type1 = Random.Range(0, GameSetting.STATS_NAME.Count),
                        type2 = Random.Range(0, GameSetting.STATS_NAME.Count),
                    }
                };
                while (rtnReward.playCard.type1 == rtnReward.playCard.type2) rtnReward = new()
                {
                    playCard = new()
                    {
                        type1 = Random.Range(0, GameSetting.STATS_NAME.Count),
                        type2 = Random.Range(0, GameSetting.STATS_NAME.Count),
                    }
                };
                if (rtnReward.playCard.type1 < rtnReward.playCard.type2)
                    (rtnReward.playCard.type1, rtnReward.playCard.type2) = (rtnReward.playCard.type2, rtnReward.playCard.type1);
                return rtnReward;
            }
            else if (Random.Range(0, 2) == 1)
            {
                return new()
                {
                    playCard = new()
                    {
                        type1 = Random.Range(GameSetting.STATS_THRESHOLD, GameSetting.STATS_NAME.Count),
                    }
                };
            }
            else
                return new()
                {
                    playCard = new()
                    {
                        type1 = Random.Range(0, GameSetting.STATS_NAME.Count),
                    }
                };
        }
    }

    //

    #region CARD INTERFACE
    public void SelectCard(RewardCard selectedCard)
    {
        foreach (RewardCard card in cards) card.Unselect();
        selectedCard.Select();
        this.selectedCard = selectedCard;
    }
    #endregion

    //

    #region POSITION HANDLER
    private Vector3 GetLatestPosition()
    {
        return CardsPosition[cards.Count - 1].position;
    }
    #endregion
}
