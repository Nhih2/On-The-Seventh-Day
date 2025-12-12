using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    #region SINGELTON
    public static DialogueManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        HideMessage();
        continueButton.onClick.AddListener(NextMessage);
        skipButton.onClick.AddListener(SkipMessage);
        NextMessage();
    }
    #endregion

    void Update()
    {
        currentLocation = Popup.transform.position;
    }

    [SerializeField] private Transform Popup, BG;
    [SerializeField] private TextMeshProUGUI desTxt;
    [SerializeField] private Button continueButton, skipButton;
    public Vector3 currentLocation;
    private List<string> tutorial = new()
    {
       "A tutorial, huh? Alright then.\n(Click on Continue or Skip to proceed — they’re pretty self-explanatory.)",

"First, select a <color=#00008B>Scene</color> card. Don’t worry about your choice — they all share the same <color=#00008B>Scene</color>.",

"Then, click Confirm to begin your first chapter.",

"Now, you’ve received 4 <color=blue>Attribute</color> cards — or <color=blue>Stats</color>, if you prefer to call them that.",

"See the cards at the top? Those are the <color=red>Challenge</color> cards. The name of each challenge is shown at the top.",

"The yellow box displays the requirements needed to succeed at the <color=red>Challenge</color>.",

"When you succeed, the green box shows what you will gain — usually positive multiplier or Energy or Heart.",

"If you fail, the red box shows what you will lose — multiplier or Energy or Heart, same as the green box but in reverse.",

"That’s enough explanation — let’s see how much Score you can rack up. Click on your cards to build your hand.",

"Missing a required <color=blue>Attribute</color>? Select the cards you don’t want and click Redraw. This will cose you Energy.",

"Each Redraw increases the Energy cost by 1, but this count resets with every new Scene.\nOnce you're satisfied, click Play.",

"Each <color=blue>Attribute</color> card earns you 10 points. Playing duplicates earns bonus points — 10 extra each time.",

"Try to beat the Threshold! If your Score surpasses it, you’ll gain a new Card or Emotion.",

"Emotions have lasting effects for the rest of the game — but only 5 can be active at once.",

"You can click an Emotion in the top-right corner to remove it at any time.",

"Your Score resets after each Scene. You have 5 Rounds to earn as many points as humanly possible.",

"Oh — and try not to let your Heart run out.\nThat’s all! Have fun, and good luck!",

"",
"",

    };
    private List<int> keyId = new()
    {
        2, 7, 10, 15
    };
    public bool isLocked = false, isShowingScore = false;
    private int currentId = 0;

    private void SkipMessage()
    {
        foreach (int i in keyId)
            if (i > currentId)
            {
                currentId = i;
                NextMessage();
                return;
            }
        HideMessage();
    }

    private bool isSeven = false, isSevenDone = false, hasConversed = false;
    private int seven = 0;
    private List<string> sevenDialogues = new()
    {
        "So you have reached the seventh day, I'm impressed by your tenacity.",
        "So, what do you think of the game? Fun? Boring? Repetitive? Varying?",
        "The events happening here. They are like real life in a way, only in a more rushed and micro manner.",
        "Losing your job, becoming a parent, a zombie outbreak happen. Whatever happens, we as humanity always adapt to them.",
        "So even the most earth-shattering news turns out dull, and so we begin the cycle of sleeping and waking.",
        "It's like an endless loop, and the only loop breaker is your own death. Yet an bigger loop keep spinning round and round.",
        "Having played this game, what is your opinion on this matter? What is your meaning of life in this endless cycles?"
    };
    private void NextMessage()
    {
        if (isRotated)
        {
            desTxt.transform.Rotate(0, 0, -5);
            isRotated = false;
        }
        if (isSeven)
        {
            if (seven >= 6)
            {
                isSeven = false;
            }
            else
            {
                ShakeText(desTxt, () =>
                {
                    desTxt.text = sevenDialogues[seven];
                    seven++;
                });
                return;
            }
        }
        if (isShowingScore)
        {
            if (GameManager.Instance.SceneCount % 3 == 2 && GameSetting.hasJob && GameSetting.ScenariosSuccess[46] < 1)
            {
                isShown = false;
                ShakeText(desTxt, () =>
                {
                    desTxt.text = "Your boss has fired you for";
                    if (GameManager.Instance.curretScene == Sce.Office) desTxt.text += " not delivering work on deadline.";
                    if (GameManager.Instance.curretScene != Sce.Office) desTxt.text += " not going to work.";
                    GameSetting.hasJob = false;
                });
            }
            if (GameManager.Instance.SceneCount % 3 == 2 && !GameSetting.hasJob && GameSetting.ScenariosSuccess[46] > 0)
            {
                isShown = false;
                ShakeText(desTxt, () =>
                {
                    desTxt.text = "You have been hired";
                });
                GameSetting.hasJob = true;
            }
            if (GameManager.Instance.SceneCount % 3 == 0 && !hasConversed)
            {
                isShown = false;
                if (GameManager.Instance.SceneCount / 3 + 1 == 3 && !isSevenDone)
                {
                    isSevenDone = true;
                    isSeven = true;
                    ShakeText(desTxt, () =>
                    {
                        desTxt.text = "Day 7";
                    });
                }
                else
                {
                    desTxt.text = "Day " + (GameManager.Instance.SceneCount / 3 + 1);
                }
                hasConversed = true;
            }
            if (isShown)
                HideMessage();
            isShown = true;
            return;
        }
        if (SfxManager.Instance) SfxManager.Instance.ButtonPressSfx();
        if (currentId == 15) { HideMessage(); return; }
        if (isLocked) return;
        Popup.transform.position = currentLocation;
        ShowMessage(tutorial[currentId]);
        currentId++;
    }

    public void ShowMessage(string description)
    {
        Popup.gameObject.SetActive(true);
        ShakeText(desTxt, () =>
        {
            desTxt.text = description;
            if (currentId == 14) EmotionManager.Instance.Showcase();
        });
    }
    public void HideMessage()
    {
        Popup.gameObject.SetActive(false);
        BG.gameObject.SetActive(false);
    }

    private bool isShown = false, isRotated = false;
    public void ShowScore(string message)
    {
        if (!isShowingScore)
        {
            isShowingScore = true;
            skipButton.interactable = false;
            Popup.transform.position = new Vector3(0, 0, 0);
            Popup.transform.localScale = new Vector3(3, 6, 0);
        }
        desTxt.transform.localRotation = new(0, 0, 0, 0);
        desTxt.transform.Rotate(0, 0, 5);
        isRotated = true;
        desTxt.text = message;
        isShown = true;
        Popup.gameObject.SetActive(true);
        BG.gameObject.SetActive(true);
        hasConversed = false;
    }

    //
    private Vector3 originalScale = new(1f, 0.5f, 1);
    private float scaleTransition = 0.25f, hoverTransition = 0.05f;
    private float hoverPunchAngle = 5;
    private void ShakeText(TextMeshProUGUI text, Action onShake, float magnitude = 1.35f)
    {
        DOTween.Kill(2, true);
        isLocked = true;
        text.transform.DOScale(originalScale * magnitude, scaleTransition).SetEase(Ease.InBack).OnComplete(
            () =>
            {
                onShake();
                text.transform.DOPunchRotation(Vector3.forward * hoverPunchAngle / 2, hoverTransition, 20, 1).SetId(2);
                text.transform.DOScale(originalScale, scaleTransition).SetEase(Ease.OutBack).OnComplete(() =>
                {
                    isLocked = false;
                });
            }
        );
    }
}
