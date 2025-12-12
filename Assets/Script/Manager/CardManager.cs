using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardManager : MonoBehaviour
{
    #region CARD VARIABLES
    private const int MAX_CARDS = 7, MAX_PLAY_CARDS = 5;

    //

    public List<PlayerCard> selectedCard { get; private set; } = new();

    //


    [SerializeField] private CardHolder cardHolder;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private Transform CardPrefab, VisualParent;

    //

    void Awake()
    {

    }

    private void Start()
    {
        GameManager.Instance.OnTurnStart += TurnStart;
    }

    private List<PlayerCard> handCard = new();
    #endregion


    //
    //// 
    //
    public void RedrawCard(int cardsRedrawn)
    {
        //int cardsRedrawn = selectedCard.Count;
        //inventoryManager.MoveToTrash(selectedCard.Select(x => x.card).ToList());
        //selectedCard.Clear();
        StartCoroutine(WaitAndDraw(cardsRedrawn));
    }

    public void DrawCard()
    {
        Debug.Log("[CardManager.WaitAndDraw] hand: " + handCard.Count + " / selected" + selectedCard.Count);
        if (handCard.Count + selectedCard.Count < MAX_CARDS)
        {
            Card card = cardHolder.AddCard(Instantiate(CardPrefab).GetComponent<Card>());
            handCard.Add(card.Init(VisualParent));
            handCard[^1].Initialize(this, inventoryManager.GetCard());
        }
    }

    #region CARDS SIGNAL
    /*public bool SelectCard(PlayerCard card)
    {
        if (selectedCard.Count >= MAX_PLAY_CARDS) return false;
        handCard.Remove(card);
        selectedCard.Add(card);
        Debug.Log("[CardManager.SelectCard] card: " + GameSetting.STATS_NAME[card.card.type1]);
        return true;
    }

    public void UnselectCard(PlayerCard card)
    {
        selectedCard.Remove(card);
        handCard.Add(card);
        Debug.Log("[CardManager.UnselectCard] card: " + GameSetting.STATS_NAME[card.card.type1]);
    }*/
    #endregion

    #region MANAGER SIGNAl
    public void MoveToTrash(PlayerCard card)
    {
        handCard.Remove(card);
        inventoryManager.MoveToTrash(new() { card.card });
    }
    private void TurnStart()
    {
        //inventoryManager.MoveToTrash(selectedCard.Select(x => x.card).ToList());
        //selectedCard.Clear();
        StartCoroutine(WaitAndDraw(4));
    }
    IEnumerator WaitAndDraw(int cards)
    {
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < cards; i++)
        {
            DrawCard();
            yield return new WaitForSeconds(0.5f);
        }
    }
    #endregion
}
