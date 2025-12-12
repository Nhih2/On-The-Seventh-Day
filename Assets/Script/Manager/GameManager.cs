using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region SINGELTON
    public static GameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        TurnEndButton.onClick.AddListener(EndTurn);
        OnRewardEnd += OnSceneEnd;
    }
    #endregion

    #region GAME SPRITES
    public List<Sprite> sprites, emoticons, sceneries;
    #endregion


    #region GAME VARIABLES
    public int Heart = 3;
    public int Energy = 5;
    public void GameReset()
    {
        Debug.Log("GameReset");
        Heart = GameSetting.BASE_HEART;
        Energy = GameSetting.BASE_ENERGY;
        DeathCount++;
    }
    #endregion


    #region GAME EVENTS
    [SerializeField] private Button TurnEndButton;
    [SerializeField] private SpriteRenderer trueBG;
    public event Action OnTurnStart, OnTurnEnd;
    private float turnEndDelay = 0.5f;
    private int TurnEndCount = 0;
    public bool isTurnEnd = false, turnEndable = false, isInReward;
    public string FullScoreText = "";
    public int DeathCount = 0;
    //
    public Sce curretScene;
    public int RoundCount = 0, SceneCount = 0;
    //
    public event Action<bool, bool> OnRewardStart, OnRewardEnd;
    #endregion

    #region EVENTS HANDLER
    public void EndTurn()
    {
        if (isTurnEnd) return;
        SfxManager.Instance.ButtonPressSfx();
        TurnEndCount = OnTurnEnd.GetInvocationList().Length;
        OnTurnEnd?.Invoke();
        isTurnEnd = true;
    }
    public void StartTurn()
    {
        StartCoroutine(WaitForTurnStart());
    }
    private IEnumerator WaitForTurnStart()
    {
        yield return new WaitForSeconds(turnEndDelay);
        OnTurnStart?.Invoke();
        isTurnEnd = false;
    }
    private void SetButton()
    {
        TurnEndButton.interactable = turnEndable;
    }
    //
    private IEnumerator WaitForSceneStart()
    {
        yield return new WaitForSeconds(turnEndDelay);
        OnRewardStart?.Invoke(true, true);
        isTurnEnd = false;
    }
    private void OnSceneEnd(bool isSceneStart, bool willNext)
    {
        if (!isSceneStart) return;
        trueBG.sprite = curretScene.GetSprite();
    }
    //
    public void TriggerRewardEnd(bool isSceneStart, bool willNext)
    {
        isInReward = false;
        OnRewardEnd.Invoke(isSceneStart, willNext);
        if (willNext) StartTurn();
    }
    public void TriggerRewardStart(bool isSceneStart, bool willNext)
    {
        isInReward = true;
        OnRewardStart.Invoke(isSceneStart, willNext);
    }
    #endregion

    void Start()
    {
        StartCoroutine(WaitForSceneStart());
    }

    void Update()
    {
        SetButton();
    }
}
