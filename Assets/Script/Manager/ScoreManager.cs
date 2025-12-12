using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private const float SHOW_CARD_BUFFER = 0.05f, BEFORE_SHOW_MUL_DELAY = 0.25f, SHOW_MUL_DELAY = 1f, SHOW_SCORE_DELAY = 2f;
    private const float TRANSFER_TIME = 1f, CALC_DELAY = 1f;

    [SerializeField] private ScenarioManager scenarioManager;
    [SerializeField] private TextMeshProUGUI nameTxt, highscoreTxt, roundScoreTxt, scoreDesTxt, scoreTxt, mulTxt, sceneTxt, roundTxt;
    [SerializeField] private GameObject heartHolder, energyHolder, redrawHolder;
    [SerializeField] private CardHolder cardHolder;

    private int threshold = 0, roundScore = 0, thresholdPassed, previousRoundScore = 0;
    private float currentScore, currentMultiplier = 1;
    private int redrawCost = 0;

    private List<int> thresholdScale = new()
    {
        0, 150, 300, 500, 750, 1000, 1500, 2000, 2500, 3000, 3500, 4000, 4500, 5000, 5500, 6000, 6500, 7500
    };

    void Start()
    {
        GameManager.Instance.OnTurnEnd += TurnEnd;
        GameManager.Instance.OnTurnStart += TurnStart;
        GameManager.Instance.OnRewardEnd += OnScenarioStart;
        UpdateBasetext();
        nameTxt.text = "Testing Site";
        highscoreTxt.text = "Highscore:\n0";
        roundScoreTxt.text = "0";
        scoreDesTxt.text = "";
        scoreTxt.text = $"0";
        mulTxt.text = "0";
        mulTxt.text = "0";
        sceneTxt.text = "0";
        roundTxt.text = "0";
    }

    private void OnScenarioStart(bool isSceneStart, bool willNext)
    {
        if (!isSceneStart) return;
        thresholdPassed = 0;
        previousRoundScore = roundScore;
        threshold = thresholdScale[GameManager.Instance.SceneCount] + previousRoundScore;
        redrawCost = 0;
        ResetRoundScore();
    }

    void Update()
    {
        UpdateBasetext();
        UpdateHeartAndEnergy();
    }

    private void UpdateBasetext()
    {
        nameTxt.text = GameManager.Instance.curretScene.GetScenery().name;
        highscoreTxt.text = $"Threshold:\n{threshold}";
        roundScoreTxt.text = $"{roundScore}";
        scoreTxt.text = $"{currentScore}";
        mulTxt.text = $"{currentMultiplier}";
        sceneTxt.text = $"{GameManager.Instance.SceneCount}";
        roundTxt.text = $"{GameManager.Instance.RoundCount}";
    }

    private void UpdateHeartAndEnergy()
    {
        if (GameManager.Instance.Energy < 0) GameManager.Instance.Energy = 0;
        for (int i = 0; i < GameSetting.MAX_HEART; i++)
        {
            if (i < GameManager.Instance.Heart)
                heartHolder.transform.GetChild(i).gameObject.SetActive(true);
            else
                heartHolder.transform.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < GameSetting.MAX_ENERGY; i++)
        {
            if (i < GameManager.Instance.Energy)
                energyHolder.transform.GetChild(i).gameObject.SetActive(true);
            else
                energyHolder.transform.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < GameSetting.MAX_ENERGY; i++)
        {
            if (i < redrawCost)
                redrawHolder.transform.GetChild(i).gameObject.SetActive(true);
            else
                redrawHolder.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void TurnStart()
    {

    }

    private void TurnEnd()
    {
        StartCoroutine(TurnEndAnimation());
    }

    private List<PlayerCard> selectedCards;
    private List<CardVisual> selectedVisuals;
    public void UpdateSelected(List<Card> cards)
    {
        selectedVisuals = cards.Select(x => x.cardVisual).ToList();
        selectedCards = cards.Select(x => x.cardVisual.GetComponent<PlayerCard>()).ToList();
    }

    private IEnumerator TurnEndAnimation()
    {
        #region Setting up
        scoreDesTxt.text = "";
        UpdateBasetext();
        yield return new WaitForSeconds(cardHolder.MoveSelected());
        while (selectedCards == null) yield return null;
        currentScore = 0; currentMultiplier = 1;
        int baseScore = 10, incScore = 10;
        Dictionary<int, int> stats = new()
        {
            { 0, 0 },
            { 1, 0 },
            { 2, 0 },
            { 3, 0 }
        };
        Dictionary<int, int> stack = new()
        {
            { 0, 0 },
            { 1, 0 },
            { 2, 0 },
            { 3, 0 }
        };
        List<(int, int)> types = selectedCards.Select(x =>
        {
            return (x.card.type1, x.card.type2);
        }).ToList();
        List<int> fullTypes = selectedCards.Select(x =>
        {
            if (x.card.type2 != -1)
            {
                return x.card.type1 * 10 + x.card.type2;
            }
            else return x.card.type1;
        }).ToList();
        int id = 0;
        UpdateBasetext();
        #endregion

        //

        #region Card Score
        bool isDouble = false;
        foreach (PlayerCard card in selectedCards)
        {
            if (!stats.ContainsKey(types[id].Item1)) stats.Add(types[id].Item1, 0);
            if (!stats.ContainsKey(types[id].Item2)) stats.Add(types[id].Item2, 0);
            if (!stack.ContainsKey(fullTypes[id])) stack.Add(fullTypes[id], 0);
            int type1 = types[id].Item1, type2 = types[id].Item2;
            if (type1 < GameSetting.STATS_THRESHOLD || (type2 < GameSetting.STATS_THRESHOLD && type2 != -1))
            {

                int cardScore = baseScore + incScore * stack[fullTypes[id]];
                string shownScore = "+" + (baseScore + incScore * stack[fullTypes[id]]);
                ShakeText(scoreTxt, () =>
                {
                    currentScore += cardScore;
                });
                selectedVisuals[id].ShakeCard(1f);
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[fullTypes[id]]);
                yield return new WaitForSeconds(card.ShowScore(shownScore, false) + SHOW_CARD_BUFFER);
                if (isDouble)
                {
                    ShakeText(scoreTxt, () =>
                    {
                        currentScore += cardScore;
                    });
                    selectedVisuals[id - 1].ShakeCard(1f);
                    SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[fullTypes[id]] + 0.1f);
                    yield return new WaitForSeconds(card.ShowScore(shownScore, false) + SHOW_CARD_BUFFER);
                    ShakeText(scoreTxt, () =>
                    {
                        currentScore += cardScore;
                    });
                    selectedVisuals[id - 1].ShakeCard(1f);
                    SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[fullTypes[id]] + 0.1f);
                    yield return new WaitForSeconds(card.ShowScore(shownScore, false) + SHOW_CARD_BUFFER);
                    isDouble = false;
                }
            }
            if (type1 == 4 || type2 == 4)
            {
                float cardMul = GameSetting.DON_MUL;
                string shownScore = "+" + cardMul.FloatDisplay() + " Mult";
                ShakeText(mulTxt, () =>
                {
                    currentMultiplier += cardMul;
                });
                selectedVisuals[id].ShakeCard(1f);
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1.15f);
                yield return new WaitForSeconds(card.ShowScore(shownScore, true) + SHOW_CARD_BUFFER);
            }
            if (type1 == 5 || type2 == 5)
            {
                isDouble = true;
            }

            stats[type1]++;
            if (type2 != -1)
                stats[type2]++;

            stack[fullTypes[id]]++;

            //selectedVisuals[id].ShakeCard(1f);
            //SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[types[id]]);
            //yield return new WaitForSeconds(card.ShowScore(shownScore, false) + SHOW_CARD_BUFFER);
            id++;
        }
        // Log stats
        foreach (var kvp in stats)
        {
            Debug.Log($"[ScoreManager] stats[{kvp.Key}] = {kvp.Value}");
        }

        // Log stack
        foreach (var kvp in stack)
        {
            Debug.Log($"[ScoreManager] stack[{kvp.Key}] = {kvp.Value}");
        }

        // Log types
        for (int i = 0; i < types.Count; i++)
        {
            Debug.Log($"[ScoreManager] types[{i}] = ({types[i].Item1}, {types[i].Item2})");
        }

        // Log fullTypes
        for (int i = 0; i < fullTypes.Count; i++)
        {
            Debug.Log($"[ScoreManager] fullTypes[{i}] = {fullTypes[i]}");
        }
        #endregion

        #region Scenario Score
        foreach (ScenarioCard card in scenarioManager.cards)
        {
            bool willWait = false;
            float multi = 1;
            #region Scenario Failure
            if (card.scenario.stat_str > stats[0] ||
                card.scenario.GetDex() > stats[1] ||
                card.scenario.stat_int > stats[2] ||
                card.scenario.stat_sym > stats[3] ||
                -card.scenario.required.energy > GameManager.Instance.Energy ||
                -card.scenario.required.heart > GameManager.Instance.Heart)
            {
                multi = card.scenario.failure.multiplier;
                card.FlashFailure();
                GameSetting.ScenariosFailure[card.scenario.id]++;
                if (card.scenario.failure.energy != 0)
                    ShakeImage(energyHolder, () =>
                    {
                        GameManager.Instance.Energy += card.scenario.failure.energy;
                        if (GameManager.Instance.Energy > GameSetting.MAX_ENERGY) GameManager.Instance.Energy = GameSetting.MAX_ENERGY;
                        Debug.Log("[ScoreManager.TurnEndAnimation] Heart: " + GameManager.Instance.Heart + " /Energy: " + GameManager.Instance.Energy);
                        UpdateHeartAndEnergy();
                    });
                if (card.scenario.failure.heart != 0)
                    ShakeImage(heartHolder, () =>
                    {
                        GameManager.Instance.Heart += card.scenario.failure.heart;
                        if (GameManager.Instance.Heart > GameSetting.MAX_HEART) GameManager.Instance.Heart = GameSetting.MAX_HEART;
                        Debug.Log("[ScoreManager.TurnEndAnimation] Heart: " + GameManager.Instance.Heart + " /Energy: " + GameManager.Instance.Energy);
                        UpdateHeartAndEnergy();
                    });
                if (card.scenario.failure.energy != 0 || card.scenario.failure.heart != 0) willWait = true;
                yield return new WaitForSeconds(BEFORE_SHOW_MUL_DELAY);
            }
            #endregion
            else
            {
                #region Scenario Requirements
                if (card.scenario.required.energy != 0 || card.scenario.required.heart != 0)
                {
                    card.FlashRequirement();
                    if (card.scenario.required.energy != 0)
                        ShakeImage(energyHolder, () =>
                        {
                            GameManager.Instance.Energy += card.scenario.required.energy;
                            if (GameManager.Instance.Energy > GameSetting.MAX_ENERGY) GameManager.Instance.Energy = GameSetting.MAX_ENERGY;
                            Debug.Log("[ScoreManager.TurnEndAnimation] Heart: " + GameManager.Instance.Heart + " /Energy: " + GameManager.Instance.Energy);
                            UpdateHeartAndEnergy();
                        });
                    if (card.scenario.required.heart != 0)
                        ShakeImage(heartHolder, () =>
                        {
                            GameManager.Instance.Heart += card.scenario.required.heart;
                            if (GameManager.Instance.Heart > GameSetting.MAX_HEART) GameManager.Instance.Heart = GameSetting.MAX_HEART;
                            Debug.Log("[ScoreManager.TurnEndAnimation] Heart: " + GameManager.Instance.Heart + " /Energy: " + GameManager.Instance.Energy);
                            UpdateHeartAndEnergy();
                        });

                    yield return new WaitForSeconds(BEFORE_SHOW_MUL_DELAY);
                }
                #endregion
                #region Scenario Success
                multi = card.scenario.success.multiplier;
                card.FlashSuccess();
                GameSetting.ScenariosSuccess[card.scenario.id]++;
                if (card.scenario.success.energy != 0)
                    ShakeImage(energyHolder, () =>
                    {
                        GameManager.Instance.Energy += card.scenario.success.energy;
                        if (GameManager.Instance.Energy > GameSetting.MAX_ENERGY) GameManager.Instance.Energy = GameSetting.MAX_ENERGY;
                        Debug.Log("[ScoreManager.TurnEndAnimation] Heart: " + GameManager.Instance.Heart + " /Energy: " + GameManager.Instance.Energy);
                        UpdateHeartAndEnergy();
                    });
                if (card.scenario.success.heart != 0)
                    ShakeImage(heartHolder, () =>
                    {
                        GameManager.Instance.Heart += card.scenario.success.heart;
                        if (GameManager.Instance.Heart > GameSetting.MAX_HEART) GameManager.Instance.Heart = GameSetting.MAX_HEART;
                        Debug.Log("[ScoreManager.TurnEndAnimation] Heart: " + GameManager.Instance.Heart + " /Energy: " + GameManager.Instance.Energy);
                        UpdateHeartAndEnergy();
                    });
                if (card.scenario.success.energy != 0 || card.scenario.success.heart != 0) willWait = true;

                yield return new WaitForSeconds(BEFORE_SHOW_MUL_DELAY);
                #endregion
            }
            if (multi != 0)
            {
                ShakeText(mulTxt, () =>
                {
                    currentMultiplier += multi;
                    if (currentMultiplier < 0) currentMultiplier = 0;
                });
                UpdateBasetext();
                willWait = true;
            }
            UpdateBasetext();
            if (willWait)
            {
                if (multi < 0)
                    SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 0.75f);
                else if (multi < 1)
                    SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f);
                else if (multi < 2)
                    SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1.15f);
                else
                    SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1.3f);
                yield return new WaitForSeconds(card.ShowMultiplier((multi < 0 ? "" : "+") + multi + " Mult", true));
            }
        }
        #endregion

        #region Emotion Score
        stats = new()
        {
            { 0, 0 },
            { 1, 0 },
            { 2, 0 },
            { 3, 0 }
        };
        stack = new()
        {
            { 0, 0 },
            { 1, 0 },
            { 2, 0 },
            { 3, 0 }
        };
        id = 0;
        if (EmotionManager.Instance.CheckEmotion(Emo.Angry))
        {
            string bonusScore = $" +{GameSetting.ANG_MUL} Mult";
            //selectedVisuals[id].ShakeCard(1f);
            //SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[types[id]]);
            //yield return new WaitForSeconds(card.ShowScore(bonusScore, false) + SHOW_CARD_BUFFER);
            ShakeText(mulTxt, () =>
            {
                currentMultiplier += GameSetting.ANG_MUL;
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f);
            });
            yield return new WaitForSeconds(EmotionManager.Instance.ShowScore(bonusScore, Emo.Angry, true) + SHOW_CARD_BUFFER);
        }
        foreach (PlayerCard card in selectedCards)
        {
            if (!stats.ContainsKey(types[id].Item1)) stats.Add(types[id].Item1, 0);
            if (!stats.ContainsKey(types[id].Item2)) stats.Add(types[id].Item2, 0);
            if (!stack.ContainsKey(fullTypes[id])) stack.Add(fullTypes[id], 0);
            int type1 = types[id].Item1, type2 = types[id].Item2;

            string bonusScore = "";
            if (EmotionManager.Instance.CheckEmotion(Emo.Sad) && fullTypes.IsFirstOccurrence(id))
            {
                bonusScore += $" +{GameSetting.SAD_INC}";
                currentScore += GameSetting.SAD_INC;
                selectedVisuals[id].ShakeCard(1f);
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[fullTypes[id]]);
                yield return new WaitForSeconds(EmotionManager.Instance.ShowScore(bonusScore, Emo.Sad, false) + SHOW_CARD_BUFFER);
                //yield return new WaitForSeconds(card.ShowScore(bonusScore, false) + SHOW_CARD_BUFFER);
            }
            if (EmotionManager.Instance.CheckEmotion(Emo.Happy) && stack[fullTypes[id]] > 0)
            {
                bonusScore = $" +{GameSetting.HAP_MUL + 1} Mult";
                currentMultiplier += GameSetting.HAP_MUL;
                selectedVisuals[id].ShakeCard(1f);
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[fullTypes[id]]);
                yield return new WaitForSeconds(EmotionManager.Instance.ShowScore(bonusScore, Emo.Happy, true) + SHOW_CARD_BUFFER);
                //yield return new WaitForSeconds(card.ShowScore(bonusScore, true) + SHOW_CARD_BUFFER);
            }
            if (EmotionManager.Instance.CheckEmotion(Emo.Cute) && (type1 >= GameSetting.STATS_THRESHOLD || type2 >= GameSetting.STATS_THRESHOLD))
            {
                bonusScore = $" +{GameSetting.CUT_MUL + 1} Mult";
                currentMultiplier += GameSetting.CUT_MUL;
                selectedVisuals[id].ShakeCard(1f);
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f);
                yield return new WaitForSeconds(EmotionManager.Instance.ShowScore(bonusScore, Emo.Cute, true) + SHOW_CARD_BUFFER);
                //yield return new WaitForSeconds(card.ShowScore(bonusScore, true) + SHOW_CARD_BUFFER);
            }

            UpdateBasetext();

            stats[type1]++;
            if (type2 != -1)
                stats[type2]++;

            stack[fullTypes[id]]++;

            id++;
        }
        if (EmotionManager.Instance.CheckEmotion(Emo.Sastisfied) && stack[0] > 0 && stack[1] > 0)
        {
            string bonusScore = $" +{GameSetting.SAS_MUL} Mult";
            //selectedVisuals[id].ShakeCard(1f);
            //SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[types[id]]);
            //yield return new WaitForSeconds(card.ShowScore(bonusScore, false) + SHOW_CARD_BUFFER);
            ShakeText(mulTxt, () =>
            {
                currentMultiplier += GameSetting.SAS_MUL;
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f);
            });
            yield return new WaitForSeconds(EmotionManager.Instance.ShowScore(bonusScore, Emo.Sastisfied, true) + SHOW_CARD_BUFFER);
        }
        if (EmotionManager.Instance.CheckEmotion(Emo.Excited) && stack[2] > 0 && stack[3] > 0)
        {
            string bonusScore = $" +{GameSetting.EXC_MUL} Mult";
            //selectedVisuals[id].ShakeCard(1f);
            //SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[types[id]]);
            //yield return new WaitForSeconds(card.ShowScore(bonusScore, false) + SHOW_CARD_BUFFER);
            ShakeText(mulTxt, () =>
            {
                currentMultiplier += GameSetting.EXC_MUL;
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f);
            });
            yield return new WaitForSeconds(EmotionManager.Instance.ShowScore(bonusScore, Emo.Excited, true) + SHOW_CARD_BUFFER);
        }
        if (EmotionManager.Instance.CheckEmotion(Emo.Cool) && selectedCards.Count > 4)
        {
            string bonusScore = $" +{GameSetting.EXC_MUL} Mult";
            //selectedVisuals[id].ShakeCard(1f);
            //SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f + 0.075f * stack[types[id]]);
            //yield return new WaitForSeconds(card.ShowScore(bonusScore, false) + SHOW_CARD_BUFFER);
            ShakeText(mulTxt, () =>
            {
                currentMultiplier += GameSetting.EXC_MUL;
                SfxManager.Instance.PlaySfx(SfxManager.SfxType.Score, 0.1f, 1f);
            });
            yield return new WaitForSeconds(EmotionManager.Instance.ShowScore(bonusScore, Emo.Cool, true) + SHOW_CARD_BUFFER);
        }
        #endregion

        #region Convert Score
        float totalScore = Mathf.Floor(currentScore * currentMultiplier);
        ShakeText(scoreDesTxt, () =>
        {
            scoreDesTxt.text = (int)totalScore + "";
        });
        ShakeText(mulTxt, () =>
        {
            currentMultiplier = 1;
        });
        ShakeText(scoreTxt, () =>
        {
            currentScore = 0;
        });
        yield return new WaitForSeconds(CALC_DELAY);
        //
        int loop = 100, step = (int)(totalScore / 100);
        if (totalScore < loop)
        {
            loop = (int)totalScore;
            step = 1;
        }
        int realRoundScore = roundScore + (int)totalScore;
        SfxManager.Instance.PlaySfxForDuration(SfxManager.SfxType.CashOut, loop * TRANSFER_TIME / 90f);
        for (int i = 0; i < loop; i++)
        {
            roundScore += step;
            totalScore -= step;
            scoreDesTxt.text = (int)totalScore + "";
            yield return new WaitForSeconds(TRANSFER_TIME / 100);
        }
        roundScore = realRoundScore;
        scoreDesTxt.text = "0";
        yield return new WaitForSeconds(TRANSFER_TIME / 100);
        yield return new WaitForSeconds(CALC_DELAY);
        UpdateBasetext();
        #endregion

        #region Post Processing
        if (GameManager.Instance.Heart <= 0)
        {
            ShakeText(scoreDesTxt, () =>
            {
                scoreDesTxt.text = "GAME OVER!";
            });
            GameManager.Instance.GameReset();
            UpdateBasetext();
            yield return new WaitForSeconds(SHOW_SCORE_DELAY / 2);
            ShakeText(scoreDesTxt, () =>
            {
                scoreDesTxt.text = "No worries.";
            });
            UpdateBasetext();
            yield return new WaitForSeconds(SHOW_SCORE_DELAY / 2);
            ShakeText(scoreDesTxt, () =>
            {
                scoreDesTxt.text = "You can continue!";
            });
            UpdateBasetext();
            yield return new WaitForSeconds(SHOW_SCORE_DELAY / 2);
        }
        foreach (PlayerCard card in selectedCards) card.HideScore();
        foreach (ScenarioCard card in scenarioManager.cards) card.HideMultiplier();
        SetScoreText();
        while (roundScore > threshold)
        {
            ShakeText(highscoreTxt, () =>
            {
                thresholdPassed++;
                int scale = thresholdScale[GameManager.Instance.SceneCount];
                int passed = (int)(scale * 0.5f);
                for (int i = 0; i < thresholdPassed; i++) passed *= 2;
                threshold = scale + passed + previousRoundScore;
                GameManager.Instance.TriggerRewardStart(false, false);
            });
            yield return new WaitForSeconds(1f);
            while (GameManager.Instance.isInReward) yield return null;
        }
        ShakeText(roundTxt, () =>
        {
            GameManager.Instance.RoundCount--;
        });
        yield return new WaitForSeconds(1f);
        if (GameManager.Instance.RoundCount > 0) GameManager.Instance.StartTurn();
        else
        {
            GameManager.Instance.TriggerRewardStart(true, true);
        }
        selectedCards.Clear();
        currentScore = 0; currentMultiplier = 1;
        UpdateBasetext();
        scoreDesTxt.text = "";
        #endregion
    }

    private Vector3 originalScale = new(0.2f, 1, 1), holderScale = new(1, 1, 1);
    private float scaleTransition = 0.25f, hoverTransition = 0.05f;
    private float hoverPunchAngle = 5;
    private void ShakeText(TextMeshProUGUI text, Action onShake, float magnitude = 1.35f)
    {
        DOTween.Kill(2, true);

        text.transform.DOScale(originalScale * magnitude, scaleTransition).SetEase(Ease.InBack).OnComplete(
            () =>
            {
                onShake();
                text.transform.DOPunchRotation(Vector3.forward * hoverPunchAngle / 2, hoverTransition, 20, 1).SetId(2);
                text.transform.DOScale(originalScale, scaleTransition).SetEase(Ease.OutBack);
            }
        );
    }

    private void ShakeImage(GameObject image, Action onShake, float magnitude = 1.35f)
    {
        DOTween.Kill(2, true);
        image.transform.DOScale(holderScale * magnitude, scaleTransition).SetEase(Ease.InBack).OnComplete(
            () =>
            {
                onShake();
                image.transform.DOPunchRotation(Vector3.forward * hoverPunchAngle / 2, hoverTransition, 20, 1).SetId(2);
                image.transform.DOScale(holderScale, scaleTransition).SetEase(Ease.OutBack);
            }
        );
    }

    private void ResetRoundScore()
    {
        //roundScore = 0;
    }

    public bool Redraw()
    {
        if (GameManager.Instance.Energy >= redrawCost)
        {
            ShakeImage(energyHolder, () =>
            {

            });
            ShakeImage(redrawHolder, () =>
            {
                GameManager.Instance.Energy -= redrawCost;
                if (redrawCost < GameSetting.MAX_REDRAW) redrawCost++;
            });
            return true;
        }
        else
        {
            ShakeImage(energyHolder, () => { }, 1.5f);
            return false;
        }
    }

    public bool CheckRedrawable() => GameManager.Instance.Energy >= redrawCost;

    private void SetScoreText()
    {
        GameManager.Instance.FullScoreText =
        "Scene: " + GameManager.Instance.curretScene.GetScenery().name + "\n" +
        "Scene Count: " + GameManager.Instance.SceneCount + "\nDeath Count: " + GameManager.Instance.DeathCount + "\n" +
        "Total Score: " + roundScore;
    }
}
