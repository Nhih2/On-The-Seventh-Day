using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioManager : MonoBehaviour
{
    public List<ScenarioCard> cards { get; private set; } = new();

    private const int MAX_SCENARIO_CARDS = 3;

    [SerializeField] private BewareSign CarefulSign;
    [SerializeField] private Transform cardPrefab;
    [SerializeField] private Vector3 warningCardPosition;
    [SerializeField] private List<Transform> CardsPosition;

    ScenarioCard sceneCard;
    private List<int> selectedActions = new() { -1, -1, -1 };


    void Start()
    {
        GameManager.Instance.OnTurnStart += TurnStart;
        GameManager.Instance.OnRewardEnd += ScenarioStart;
        for (int i = 0; i < GameSetting.Scenarios.Count; i++)
        {
            GameSetting.ScenariosSuccess.Add(0);
            GameSetting.ScenariosFailure.Add(0);
        }
        sceneCard = Instantiate(cardPrefab).GetComponent<ScenarioCard>();
        sceneCard.Initialize(this, 0, warningCardPosition);
        sceneCard.gameObject.SetActive(false);
        foreach (SpriteRenderer renderer in sceneCard.GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.sortingLayerName = GameSetting.SORTING_LAYER[3];
            renderer.sortingOrder += 50;
        }
        foreach (Canvas canvas in sceneCard.GetComponentsInChildren<Canvas>())
        {
            canvas.sortingLayerName = GameSetting.SORTING_LAYER[3];
            canvas.sortingOrder += 50;
        }
    }

    private void ScenarioStart(bool isSceneStart, bool willNext)
    {
        if (isSceneStart)
        {
            ResetActions();
        }
    }

    private void ResetActions()
    {
        for (int i = 0; i < GameSetting.Scenarios.Count; i++)
        {
            GameSetting.ScenariosSuccess[i] = 0;
            GameSetting.ScenariosFailure[i] = 0;
        }
    }

    private void TurnStart()
    {
        foreach (ScenarioCard card in cards) Destroy(card.gameObject);
        cards.Clear();
        selectedActions[0] = -1;
        selectedActions[1] = -1;
        selectedActions[2] = -1;
        for (int i = 0; i < MAX_SCENARIO_CARDS; i++) DrawCard();
        int round = 6 - GameManager.Instance.RoundCount;
        Sce scene = GameManager.Instance.curretScene;
        CarefulSign.gameObject.SetActive(false);
        sceneCard.gameObject.SetActive(false);
        if (scene == Sce.House_Morning)
        {
            if (round == 2)
                ActivateSign(9, 1);
            if (round == 4)
                ActivateSign(15, 1);
        }
        if (scene == Sce.Office)
        {
            if (round == 1)
                ActivateSign(34, 2);
            if (round == 2)
                ActivateSign(34, 1);
            if (round == 3)
                ActivateSign(46, 2);
            if (round == 4)
                ActivateSign(46, 1);
        }
        if (scene == Sce.House_Night)
        {
            if (round == 1)
                ActivateSign(66, 2);
            if (round == 2)
                ActivateSign(66, 1);
            if (round == 3)
                ActivateSign(67, 2);
            if (round == 4)
                ActivateSign(67, 1);
        }
    }

    private void ActivateSign(int ActionId, int Turn)
    {
        sceneCard.Initialize(this, ActionId, warningCardPosition);
        CarefulSign.gameObject.SetActive(true);
        CarefulSign.ShowBeware(sceneCard, $"A dangerous <color=red>Challenge</color> will show up in {Turn} turn.");
    }

    public void DrawCard()
    {
        if (cards.Count < MAX_SCENARIO_CARDS)
        {
            int action = GetAction(cards.Count);
            selectedActions[cards.Count] = action;
            cards.Add(Instantiate(cardPrefab).GetComponent<ScenarioCard>());
            cards[^1].Initialize(this, action, GetLatestPosition());
        }
    }

    /*private int GetAction(int index)
    {
        int actionId = -1;
        Scenery scene = GameManager.Instance.curretScene.GetScenery();
        int round = 6 - GameManager.Instance.RoundCount;
        int repeatedTimes = 0;
        if (GameManager.Instance.curretScene == Sce.House_Morning)
        {
            while (true)
            {
                //Fixed Boss Actions
                if (round == 3 && index == 1) { actionId = 9; break; }
                if (round == 5 && index == 1) { actionId = 15; break; }
                actionId = Random.Range(scene.actionsStart, scene.actionsEnd);
                repeatedTimes++;
                if (repeatedTimes > 200)
                {
                    if (actionId == 15 && round != 5 && index != 1) continue;
                    if (actionId == 9 && round != 3 && index != 1) continue;
                    break;
                }
                //Actions with linking
                if (index == 2 || (index == 1 && Random.Range(0, 2) == 0) || (index == 0 && Random.Range(0, 4) == 0))
                {
                    if (round == 0 && Random.Range(0, 3) == 0) actionId = 5;
                    if (round > 0 && GameSetting.ScenariosSuccess[5] == 0) actionId = 5;
                    if (GameSetting.ScenariosSuccess[5] > 0)
                    {
                        if (round == 2 && Random.Range(0, 3) == 0) actionId = 6;
                        if (round > 2 && GameSetting.ScenariosSuccess[6] == 0) actionId = 6;
                        if (GameSetting.ScenariosSuccess[6] > 0)
                        {
                            if (round == 3 && Random.Range(0, 3) == 0) actionId = 7;
                            if (round == 4 && Random.Range(0, 2) == 0) actionId = 7;
                            if (round == 5 && GameSetting.ScenariosSuccess[7] == 0) actionId = 7;
                        }
                    }
                    if (Random.Range(0, 2) == 0)
                    {
                        if (GameSetting.ScenariosSuccess[10] > 0 && actionId != 11 && Random.Range(0, 2) == 0) actionId = 11;
                        if (GameSetting.ScenariosSuccess[20] > 0 && actionId != 21 && Random.Range(0, 2) == 0) actionId = 21;
                    }
                }

                //Preventing repeated action
                if (GameSetting.ScenariosSuccess[actionId] > 0) continue;
                if (actionId == selectedActions[0] || actionId == selectedActions[1] || actionId == selectedActions[2]) continue;
                //Preventing Linking action to trigger
                if (actionId == 6 && GameSetting.ScenariosSuccess[5] == 0) continue;
                if (actionId == 7 && GameSetting.ScenariosSuccess[6] == 0) continue;
                if (actionId == 11 && GameSetting.ScenariosSuccess[10] == 0) continue;
                if (actionId == 21 && GameSetting.ScenariosSuccess[20] == 0) continue;
                if (actionId == 15 && (round != 3 || index != 1)) continue;
                if (actionId == 9 && (round != 5 || index != 1)) continue;
                break;
            }
        }
        if (GameManager.Instance.curretScene == Sce.Office)
        {
            while (true)
            {
                //Fixed Boss Actions
                if (round == 3 && index == 1) { actionId = 34; break; }
                if (round == 5 && index == 1) { actionId = 46; break; }
                actionId = Random.Range(scene.actionsStart, scene.actionsEnd);
                repeatedTimes++;
                if (repeatedTimes > 200)
                {
                    if (actionId == 15 && round != 5 && index != 1) continue;
                    if (actionId == 9 && round != 3 && index != 1) continue;
                    break;
                }
                //Actions with linking
                if (index == 2 || (index == 1 && Random.Range(0, 2) == 0) || (index == 0 && Random.Range(0, 4) == 0))
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        if (round == 0 && Random.Range(0, 3) == 0) actionId = 35;
                        if (round > 0 && GameSetting.ScenariosSuccess[35] == 0) actionId = 35;
                        if (GameSetting.ScenariosSuccess[35] > 0)
                        {
                            if (round == 2 && Random.Range(0, 3) == 0) actionId = 36;
                            if (round > 2 && GameSetting.ScenariosSuccess[6] == 0) actionId = 36;
                            if (GameSetting.ScenariosSuccess[36] > 0)
                            {
                                if (round == 3 && Random.Range(0, 3) == 0) actionId = 37;
                                if (round == 4 && Random.Range(0, 2) == 0) actionId = 37;
                                if (round == 5 && GameSetting.ScenariosSuccess[37] == 0) actionId = 37;
                            }
                        }
                    }
                    else
                    {
                        if (round == 0 && Random.Range(0, 3) == 0) actionId = 42;
                        if (round > 0 && GameSetting.ScenariosSuccess[35] == 0) actionId = 42;
                        if (GameSetting.ScenariosSuccess[42] > 0)
                        {
                            if (round == 2 && Random.Range(0, 3) == 0) actionId = 43;
                            if (round > 2 && GameSetting.ScenariosSuccess[6] == 0) actionId = 343;
                            if (GameSetting.ScenariosSuccess[43] > 0)
                            {
                                if (round == 3 && Random.Range(0, 3) == 0) actionId = 44;
                                if (round == 4 && Random.Range(0, 2) == 0) actionId = 44;
                                if (round == 5 && GameSetting.ScenariosSuccess[44] == 0) actionId = 44;
                            }
                        }
                    }
                    if (Random.Range(0, 2) == 0)
                    {
                        if (GameSetting.ScenariosSuccess[33] > 0 && actionId != 47 && Random.Range(0, 2) == 0) actionId = 47;
                    }
                }

                //Preventing repeated action
                if (GameSetting.ScenariosSuccess[actionId] > 0 && actionId > 32) continue;
                if (actionId == selectedActions[0] || actionId == selectedActions[1] || actionId == selectedActions[2]) continue;
                //Preventing Linking action to trigger
                if (actionId == 47 && GameSetting.ScenariosSuccess[33] == 0) continue;
                if (actionId == 36 && GameSetting.ScenariosSuccess[35] == 0) continue;
                if (actionId == 37 && GameSetting.ScenariosSuccess[36] == 0) continue;
                if (actionId == 43 && GameSetting.ScenariosSuccess[42] == 0) continue;
                if (actionId == 44 && GameSetting.ScenariosSuccess[43] == 0) continue;
                if (!(actionId == 34 && round == 3 && index == 1)) continue;
                if (!(actionId == 46 && round == 5 && index == 1)) continue;
                break;
            }
        }
        return actionId;
    }*/

    private int GetAction(int index)
    {
        int actionId = -1;
        Scenery scene = GameManager.Instance.curretScene.GetScenery();
        int round = 6 - GameManager.Instance.RoundCount;
        int repeatedTimes = 0;

        if (GameManager.Instance.curretScene == Sce.House_Morning)
        {
            while (true)
            {
                if (round == 3 && index == 1) { actionId = 9; Debug.Log("House_Morning: Round 3, middle enemy → actionId = 9 (fixed boss action)."); break; }
                if (round == 5 && index == 1) { actionId = 15; Debug.Log("House_Morning: Round 5, middle enemy → actionId = 15 (fixed boss action)."); break; }

                actionId = Random.Range(scene.actionsStart, scene.actionsEnd);
                Debug.Log($"House_Morning: Randomly selected actionId = {actionId}");
                repeatedTimes++;

                if (repeatedTimes > 200)
                {
                    Debug.LogWarning("House_Morning: Repeat limit reached.");
                    if ((actionId == 15 && (round != 5 || index != 1)) ||
                        (actionId == 9 && (round != 3 || index != 1)))
                        continue;
                    break;
                }

                if (index == 2 || (index == 1 && Random.Range(0, 2) == 0) || (index == 0 && Random.Range(0, 4) == 0))
                {
                    if (round == 1 && Random.Range(0, 3) == 0)
                    {
                        actionId = 5;
                        Debug.Log("House_Morning: Start chain → actionId = 5");
                    }
                    else if (round > 1 && GameSetting.ScenariosSuccess[5] == 0)
                    {
                        actionId = 5;
                        Debug.Log("House_Morning: Retry chain 5 → actionId = 5");
                    }

                    if (GameSetting.ScenariosSuccess[5] > 0)
                    {
                        if (round == 3 && Random.Range(0, 3) == 0) { actionId = 6; Debug.Log("House_Morning: Chain progress → actionId = 6"); }
                        if (round > 3 && GameSetting.ScenariosSuccess[6] == 0) { actionId = 6; Debug.Log("House_Morning: Retry chain 6"); }

                        if (GameSetting.ScenariosSuccess[6] > 0)
                        {
                            if (round == 4 && Random.Range(0, 2) == 0) { actionId = 7; Debug.Log("House_Morning: Continue 7 at round 4"); }
                            if (round == 5 && GameSetting.ScenariosSuccess[7] == 0) { actionId = 7; Debug.Log("House_Morning: Retry 7 at round 5"); }
                        }
                    }

                    if (Random.Range(0, 2) == 0)
                    {
                        if (GameSetting.ScenariosSuccess[10] > 0 && actionId != 11 && Random.Range(0, 2) == 0)
                        {
                            actionId = 11;
                            Debug.Log("House_Morning: Unlock follow-up → actionId = 11");
                        }

                        if (GameSetting.ScenariosSuccess[20] > 0 && actionId != 21 && Random.Range(0, 2) == 0)
                        {
                            actionId = 21;
                            Debug.Log("House_Morning: Unlock follow-up → actionId = 21");
                        }
                    }
                }

                if (GameSetting.ScenariosSuccess[actionId] > 0)
                {
                    Debug.Log($"House_Morning: Skipped actionId = {actionId} (already completed)");
                    continue;
                }

                if (selectedActions.Contains(actionId))
                {
                    Debug.Log($"House_Morning: Skipped actionId = {actionId} (already selected this round)");
                    continue;
                }

                if ((actionId == 6 && GameSetting.ScenariosSuccess[5] == 0) ||
                    (actionId == 7 && GameSetting.ScenariosSuccess[6] == 0) ||
                    (actionId == 11 && GameSetting.ScenariosSuccess[10] == 0) ||
                    (actionId == 21 && GameSetting.ScenariosSuccess[20] == 0))
                {
                    Debug.Log($"House_Morning: Skipped actionId = {actionId} (linking condition not met)");
                    continue;
                }

                if ((actionId == 15 && (round != 5 || index != 1)) ||
                    (actionId == 9 && (round != 3 || index != 1)))
                {
                    Debug.Log($"House_Morning: Skipped fixed actionId = {actionId} (not correct round or index)");
                    continue;
                }

                break;
            }
        }
        else if (GameManager.Instance.curretScene == Sce.Office)
        {
            while (true)
            {
                if (round == 3 && index == 1) { actionId = 34; Debug.Log("Office: Round 3, middle enemy → actionId = 34 (fixed boss action)."); break; }
                if (round == 5 && index == 1) { actionId = 46; Debug.Log("Office: Round 5, middle enemy → actionId = 46 (fixed boss action)."); break; }

                actionId = Random.Range(scene.actionsStart, scene.actionsEnd);
                Debug.Log($"Office: Randomly selected actionId = {actionId}");
                repeatedTimes++;

                if (repeatedTimes > 200)
                {
                    Debug.LogWarning("Office: Repeat limit reached.");
                    if ((actionId == 46 && (round != 5 || index != 1)) ||
                        (actionId == 34 && (round != 3 || index != 1)))
                        continue;
                    break;
                }
                if (round == 2 || round == 4)
                {
                    if (Random.Range(0, 3) > 1) actionId = Random.Range(scene.actionsStart, 33);
                }
                if (index == 2 || (index == 1 && Random.Range(0, 2) == 0) || (index == 0 && Random.Range(0, 4) == 0))
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        if (round == 1 && Random.Range(0, 3) == 0) { actionId = 35; Debug.Log("Office: Start chain → actionId = 35"); }
                        else if (round > 1 && GameSetting.ScenariosSuccess[35] == 0) { actionId = 35; Debug.Log("Office: Retry 35"); }

                        if (GameSetting.ScenariosSuccess[35] > 0)
                        {
                            if (round == 3 && Random.Range(0, 3) == 0) { actionId = 36; Debug.Log("Office: Chain progress → actionId = 36"); }
                            if (round > 3 && GameSetting.ScenariosSuccess[36] == 0) { actionId = 36; Debug.Log("Office: Retry 36"); }

                            if (GameSetting.ScenariosSuccess[36] > 0)
                            {
                                if (round == 4 && Random.Range(0, 2) == 0) { actionId = 37; Debug.Log("Office: Continue 37 at round 4"); }
                                if (round == 5 && GameSetting.ScenariosSuccess[37] == 0) { actionId = 37; Debug.Log("Office: Retry 37 at round 5"); }
                            }
                        }
                    }
                    else
                    {
                        if (round == 1 && Random.Range(0, 3) == 0) { actionId = 42; Debug.Log("Office: Start alt chain → actionId = 42"); }
                        else if (round > 1 && GameSetting.ScenariosSuccess[35] == 0) { actionId = 42; Debug.Log("Office: Retry 42"); }

                        if (GameSetting.ScenariosSuccess[42] > 0)
                        {
                            if (round == 3 && Random.Range(0, 3) == 0) { actionId = 43; Debug.Log("Office: Chain progress → actionId = 43"); }
                            if (round > 3 && GameSetting.ScenariosSuccess[43] == 0) { actionId = 43; Debug.Log("Office: Retry chain → actionId = 43"); }

                            if (GameSetting.ScenariosSuccess[43] > 0)
                            {
                                if (round == 4 && Random.Range(0, 2) == 0) { actionId = 44; Debug.Log("Office: Continue 44 at round 4"); }
                                if (round == 5 && GameSetting.ScenariosSuccess[44] == 0) { actionId = 44; Debug.Log("Office: Retry 44 at round 5"); }
                            }
                        }
                    }

                    if (Random.Range(0, 2) == 0 && GameSetting.ScenariosSuccess[33] > 0 && actionId != 47 && Random.Range(0, 2) == 0)
                    {
                        actionId = 47;
                        Debug.Log("Office: Unlock follow-up → actionId = 47");
                    }
                }

                if (GameSetting.ScenariosSuccess[actionId] > 0 && actionId > 32)
                {
                    Debug.Log($"Office: Skipped actionId = {actionId} (already completed)");
                    continue;
                }

                if (selectedActions.Contains(actionId))
                {
                    Debug.Log($"Office: Skipped actionId = {actionId} (already selected this round)");
                    continue;
                }

                if ((actionId == 47 && GameSetting.ScenariosSuccess[33] == 0) ||
                    (actionId == 36 && GameSetting.ScenariosSuccess[35] == 0) ||
                    (actionId == 37 && GameSetting.ScenariosSuccess[36] == 0) ||
                    (actionId == 43 && GameSetting.ScenariosSuccess[42] == 0) ||
                    (actionId == 44 && GameSetting.ScenariosSuccess[43] == 0))
                {
                    Debug.Log($"Office: Skipped actionId = {actionId} (linking condition not met)");
                    continue;
                }

                if ((actionId == 34 && !(round == 3 && index == 1)) ||
                     (actionId == 46 && !(round == 5 && index == 1)))
                {
                    Debug.Log($"Office: Skipped fixed boss actionId = {actionId} (invalid round or index)");
                    continue;
                }


                break;
            }
        }
        else if (GameManager.Instance.curretScene == Sce.House_Night)
        {
            while (true)
            {
                if (round == 3 && index == 1) { actionId = 66; Debug.Log("Home_Night: Round 3, middle enemy → actionId = 66 (fixed boss action)."); break; }
                if (round == 5 && index == 1) { actionId = 67; Debug.Log("Home_Night: Round 5, middle enemy → actionId = 67 (fixed boss action)."); break; }

                actionId = Random.Range(scene.actionsStart, scene.actionsEnd);
                Debug.Log($"Home_Night: Randomly selected actionId = {actionId}");
                repeatedTimes++;

                if (repeatedTimes > 200)
                {
                    Debug.LogWarning("Home_Night: Repeat limit reached.");
                    if ((actionId == 66 && (round != 5 || index != 1)) ||
                        (actionId == 67 && (round != 3 || index != 1)))
                        continue;
                    break;
                }

                if (index == 2 || (index == 1 && Random.Range(0, 2) == 0) || (index == 0 && Random.Range(0, 4) == 0))
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        if (round == 1 && Random.Range(0, 3) == 0) { actionId = 60; Debug.Log("Home_Night: Start chain → actionId = 60"); }
                        else if (round > 1 && GameSetting.ScenariosSuccess[60] == 0) { actionId = 35; Debug.Log("Home_Night: Retry 60"); }

                        if (GameSetting.ScenariosSuccess[60] > 0)
                        {
                            if (round == 3 && Random.Range(0, 3) == 0) { actionId = 61; Debug.Log("Home_Night: Chain progress → actionId = 61"); }
                            if (round > 3 && GameSetting.ScenariosSuccess[61] == 0) { actionId = 61; Debug.Log("Home_Night: Retry 61"); }

                            if (GameSetting.ScenariosSuccess[61] > 0)
                            {
                                if (round == 4 && Random.Range(0, 2) == 0) { actionId = 62; Debug.Log("Home_Night: Continue 62 at round 4"); }
                                if (round == 5 && GameSetting.ScenariosSuccess[62] == 0) { actionId = 62; Debug.Log("Home_Night: Retry 62 at round 5"); }
                            }
                        }
                    }
                    else
                    {
                        if (round == 1 && Random.Range(0, 3) == 0) { actionId = 63; Debug.Log("Home_Night: Start alt chain → actionId = 63"); }
                        else if (round > 1 && GameSetting.ScenariosSuccess[63] == 0) { actionId = 63; Debug.Log("Home_Night: Retry 63"); }

                        if (GameSetting.ScenariosSuccess[63] > 0)
                        {
                            if (round == 3 && Random.Range(0, 3) == 0) { actionId = 64; Debug.Log("Home_Night: Chain progress → actionId = 64"); }
                            if (round > 3 && GameSetting.ScenariosSuccess[64] == 0) { actionId = 64; Debug.Log("Home_Night: Retry chain → actionId = 64"); }

                            if (GameSetting.ScenariosSuccess[64] > 0)
                            {
                                if (round == 4 && Random.Range(0, 2) == 0) { actionId = 65; Debug.Log("Home_Night: Continue 65 at round 4"); }
                                if (round == 5 && GameSetting.ScenariosSuccess[65] == 0) { actionId = 65; Debug.Log("Home_Night: Retry 65 at round 5"); }
                            }
                        }
                    }
                }

                if (GameSetting.ScenariosSuccess[actionId] > 0)
                {
                    Debug.Log($"Home_Night: Skipped actionId = {actionId} (already completed)");
                    continue;
                }

                if (selectedActions.Contains(actionId))
                {
                    Debug.Log($"Home_Night: Skipped actionId = {actionId} (already selected this round)");
                    continue;
                }

                if ((actionId == 61 && GameSetting.ScenariosSuccess[60] == 0) ||
                    (actionId == 62 && GameSetting.ScenariosSuccess[61] == 0) ||
                    (actionId == 64 && GameSetting.ScenariosSuccess[63] == 0) ||
                    (actionId == 65 && GameSetting.ScenariosSuccess[64] == 0))
                {
                    Debug.Log($"Home_Night: Skipped actionId = {actionId} (linking condition not met)");
                    continue;
                }

                if ((actionId == 66 && !(round == 3 && index == 1)) ||
    (actionId == 67 && !(round == 5 && index == 1)))
                {
                    Debug.Log($"Home_Night: Skipped fixed boss actionId = {actionId} (invalid round or index)");
                    continue;
                }

                break;
            }

        }
        else if (GameManager.Instance.curretScene == Sce.Job_Hunting)
        {
            while (true)
            {
                if (round == 3 && index == 1) { actionId = 90; Debug.Log("Home_Night: Round 3, middle enemy → actionId = 66 (fixed boss action)."); break; }
                if (round == 5 && index == 1) { actionId = 91; Debug.Log("Home_Night: Round 5, middle enemy → actionId = 67 (fixed boss action)."); break; }

                actionId = Random.Range(scene.actionsStart, scene.actionsEnd);
                Debug.Log($"Home_Night: Randomly selected actionId = {actionId}");
                repeatedTimes++;

                if (repeatedTimes > 200)
                {
                    Debug.LogWarning("Home_Night: Repeat limit reached.");
                    if ((actionId == 90 && (round != 5 || index != 1)) ||
                        (actionId == 91 && (round != 3 || index != 1)))
                        continue;
                    break;
                }

                if (index == 2 || (index == 1 && Random.Range(0, 2) == 0) || (index == 0 && Random.Range(0, 4) == 0))
                {
                    if (Random.Range(0, 2) == 0)
                    {
                        if (round == 1 && Random.Range(0, 3) == 0) { actionId = 84; Debug.Log("Home_Night: Start chain → actionId = 60"); }
                        else if (round > 1 && GameSetting.ScenariosSuccess[84] == 0) { actionId = 84; Debug.Log("Home_Night: Retry 60"); }

                        if (GameSetting.ScenariosSuccess[84] > 0)
                        {
                            if (round == 3 && Random.Range(0, 3) == 0) { actionId = 85; Debug.Log("Home_Night: Chain progress → actionId = 61"); }
                            if (round > 3 && GameSetting.ScenariosSuccess[85] == 0) { actionId = 85; Debug.Log("Home_Night: Retry 61"); }

                            if (GameSetting.ScenariosSuccess[85] > 0)
                            {
                                if (round == 4 && Random.Range(0, 2) == 0) { actionId = 86; Debug.Log("Home_Night: Continue 62 at round 4"); }
                                if (round == 5 && GameSetting.ScenariosSuccess[86] == 0) { actionId = 86; Debug.Log("Home_Night: Retry 62 at round 5"); }
                            }
                        }
                    }
                    else
                    {
                        if (round == 1 && Random.Range(0, 3) == 0) { actionId = 87; Debug.Log("Home_Night: Start alt chain → actionId = 63"); }
                        else if (round > 1 && GameSetting.ScenariosSuccess[87] == 0) { actionId = 87; Debug.Log("Home_Night: Retry 63"); }

                        if (GameSetting.ScenariosSuccess[87] > 0)
                        {
                            if (round == 3 && Random.Range(0, 3) == 0) { actionId = 88; Debug.Log("Home_Night: Chain progress → actionId = 64"); }
                            if (round > 3 && GameSetting.ScenariosSuccess[88] == 0) { actionId = 88; Debug.Log("Home_Night: Retry chain → actionId = 64"); }

                            if (GameSetting.ScenariosSuccess[88] > 0)
                            {
                                if (round == 4 && Random.Range(0, 2) == 0) { actionId = 89; Debug.Log("Home_Night: Continue 65 at round 4"); }
                                if (round == 5 && GameSetting.ScenariosSuccess[89] == 0) { actionId = 89; Debug.Log("Home_Night: Retry 65 at round 5"); }
                            }
                        }
                    }
                }

                if (GameSetting.ScenariosSuccess[actionId] > 0)
                {
                    Debug.Log($"Home_Night: Skipped actionId = {actionId} (already completed)");
                    continue;
                }

                if (selectedActions.Contains(actionId))
                {
                    Debug.Log($"Home_Night: Skipped actionId = {actionId} (already selected this round)");
                    continue;
                }

                if ((actionId == 85 && GameSetting.ScenariosSuccess[84] == 0) ||
                    (actionId == 86 && GameSetting.ScenariosSuccess[85] == 0) ||
                    (actionId == 88 && GameSetting.ScenariosSuccess[87] == 0) ||
                    (actionId == 89 && GameSetting.ScenariosSuccess[88] == 0))
                {
                    Debug.Log($"Home_Night: Skipped actionId = {actionId} (linking condition not met)");
                    continue;
                }

                if ((actionId == 90 && !(round == 3 && index == 1)) ||
    (actionId == 91 && !(round == 5 && index == 1)))
                {
                    Debug.Log($"Home_Night: Skipped fixed boss actionId = {actionId} (invalid round or index)");
                    continue;
                }
                break;
            }
        }
        Debug.Log($"Final selected actionId: {actionId}");
        return actionId;
    }


    private int GetFinishCount(int index)
    {
        return GameSetting.ScenariosSuccess[index] + GameSetting.ScenariosFailure[index];
    }

    #region POSITION HANDLER
    private Vector3 GetLatestPosition()
    {
        return CardsPosition[cards.Count - 1].position;
    }
    #endregion
}
