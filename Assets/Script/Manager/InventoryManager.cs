using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private Button OpenInvenButton, OpenTrashButton, CancelButton;
    [SerializeField] private GameObject background, inventoryUI;
    [SerializeField] private TextMeshProUGUI header;
    [SerializeField] private List<ItemCard> cardList;

    private List<PlayCard> inventory, trash, middle;

    void Awake()
    {
        OpenInvenButton.onClick.AddListener(() => { OnOpen(true); });
        OpenTrashButton.onClick.AddListener(() => { OnOpen(false); });
        CancelButton.onClick.AddListener(() => { OnCancel(); });
        inventory = new() { };
        for (int i = 0; i < 4; i++)
            for (int j = 0; j < 4; j++) inventory.Add(new() { type1 = i });
        inventory.Shuffle();
        trash = new();
        middle = new();
        OnCancel();
    }

    void Start()
    {
        GameManager.Instance.OnTurnEnd += TurnEnd;
    }

    private void TurnEnd()
    {
        OnCancel();
    }

    public void MoveToTrash(List<PlayCard> cards)
    {
        trash.AddRange(cards);
        foreach (PlayCard card in cards) middle.Remove(card);
    }
    public void AddCard(PlayCard card)
    {
        inventory.Add(card);
        //(inventory[0], inventory[inventory.Count - 1]) = (inventory[inventory.Count - 1], inventory[0]);
    }

    //

    public PlayCard GetCard()
    {
        if (inventory.Count < 1)
            Reload();
        PlayCard rtnCard = inventory[0];
        middle.Add(rtnCard);
        inventory.RemoveAt(0);
        return rtnCard;
    }

    private void Reload()
    {
        inventory.AddRange(trash);
        trash.Clear();
        inventory.Shuffle();
    }

    //

    private void OnCancel()
    {
        if (GameManager.Instance)
            if (GameManager.Instance.isTurnEnd) return;
        SfxManager.Instance.ButtonPressSfx();
        background.SetActive(false);
        inventoryUI.SetActive(false);
        CancelButton.gameObject.SetActive(false);
        Hide();
    }

    private void OnOpen(bool isInven)
    {
        if (GameManager.Instance)
            if (GameManager.Instance.isTurnEnd) return;
        SfxManager.Instance.ButtonPressSfx();
        background.SetActive(true);
        inventoryUI.SetActive(true);
        CancelButton.gameObject.SetActive(true);
        Show(isInven ? inventory : trash);
        header.text = isInven ? "Inventory" : "Trash";
    }

    private void Show(List<PlayCard> cards)
    {
        int i = 0;
        Hide();
        foreach (PlayCard card in cards)
        {
            cardList[i].gameObject.SetActive(true);
            cardList[i].ShowCard(card);
            i++;
        }
    }

    private void Hide()
    {
        foreach (ItemCard card in cardList) card.gameObject.SetActive(false);
    }
}
