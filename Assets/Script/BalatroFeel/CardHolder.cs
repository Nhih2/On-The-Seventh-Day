using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class CardHolder : MonoBehaviour
{
    [SerializeField] private Vector3 cardDisposal;
    [SerializeField] private Card selectedCard;
    [SerializeReference] private Card hoveredCard;

    private RectTransform rect;

    [Header("Spawn Settings")]
    public List<Card> cards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;

    [SerializeField] private List<Transform> Selected;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private CardManager cardManager;
    [SerializeField] private Button redrawButton;
    [SerializeField] private GameObject redraw, play;
    [SerializeField] private float offsetY = -1f;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        cards = new();

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }
        redrawButton.onClick.AddListener(OnRedraw);
        GameManager.Instance.OnTurnStart += TurnStart;
    }

    private bool hasMoveDown = false;
    private void MoveDown()
    {
        hasMoveDown = true;
        transform.DOMoveY(transform.position.y + offsetY, preMoveBuffer - 0.05f);
        redraw.transform.DOMoveY(redraw.transform.position.y + offsetY, preMoveBuffer - 0.05f);
        play.transform.DOMoveY(play.transform.position.y + offsetY, preMoveBuffer - 0.05f);
    }

    private void MoveUp()
    {
        if (!hasMoveDown) return;
        hasMoveDown = false;
        transform.DOMoveY(transform.position.y - offsetY, preMoveBuffer - 0.05f);
        redraw.transform.DOMoveY(redraw.transform.position.y - offsetY, preMoveBuffer - 0.05f);
        play.transform.DOMoveY(play.transform.position.y - offsetY, preMoveBuffer - 0.05f);
    }

    private void OnRedraw()
    {
        SfxManager.Instance.ButtonPressSfx();
        if (scoreManager.Redraw())
        {
            StartCoroutine(WaitToRedraw());
        }
    }

    private void TurnStart()
    {
        MoveUp();
        StartCoroutine(WaitToDelete());
    }

    private IEnumerator WaitToRedraw()
    {
        isInAnimation[2] = true;
        selectedCards = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.childCount > 0)
            {
                Card card = child.GetChild(0).GetComponent<Card>();
                if (card.selected)
                {
                    card.transform.SetParent(transform.parent);
                    selectedCards.Add(card);
                    this.cards.Remove(card);
                    card.Freeze();
                    card.cardVisual.playerCard.DestroyCard();
                }
            }
        }
        UpdatePosition();
        List<Card> deletedCards = new(selectedCards);
        selectedCards.Clear();
        yield return new WaitForEndOfFrame();
        foreach (Card card in deletedCards)
        {
            StartCoroutine(MoveBeforeDelete(card));
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);
        cardManager.RedrawCard(deletedCards.Count);
        yield return new WaitForSeconds(0.5f);
        isInAnimation[2] = false;
    }

    private IEnumerator WaitToDelete()
    {
        List<Card> deletedCards = new(selectedCards);
        selectedCards.Clear();
        yield return new WaitForEndOfFrame();
        foreach (Card card in deletedCards)
        {
            StartCoroutine(MoveBeforeDelete(card));
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator MoveBeforeDelete(Card card)
    {
        card.gameObject.transform.position = cardDisposal;
        SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard, 0.4f, 1.1f);
        yield return new WaitForSeconds(1f);
        Destroy(card.transform.gameObject);
    }

    private List<Card> selectedCards = new();
    private float moveDelay = 0.25f, moveBuffer = 0.5f, preMoveBuffer = 0.3f;
    public float MoveSelected()
    {
        MoveDown();
        selectedCards = new();
        /*foreach (Card card in cards)
            if (card.selected) selectedCards.Add(card); */
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.childCount > 0)
            {
                Card card = child.GetChild(0).GetComponent<Card>();
                if (card.selected)
                {
                    selectedCards.Add(card);
                    this.cards.Remove(card);
                    card.Freeze();
                    card.cardVisual.playerCard.DestroyCard();
                }
            }
        }
        StartCoroutine(WaitAndMove(selectedCards));
        return moveDelay * selectedCards.Count + moveBuffer + preMoveBuffer;
    }

    private IEnumerator WaitAndMove(List<Card> cards)
    {
        isInAnimation[1] = true;
        int index = 0;
        yield return new WaitForSeconds(preMoveBuffer);
        foreach (Card card in cards)
        {
            card.transform.SetParent(Selected[index]);
            card.transform.localPosition = Vector3.zero;
            index++;
            SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard, 0.2f, 1.2f);
            yield return new WaitForSeconds(moveDelay);
        }
        scoreManager.UpdateSelected(cards);
        UpdatePosition();
        isInAnimation[1] = false;
    }

    private float addTimer = 0;
    public Card AddCard(Card card)
    {
        cards.Add(card);
        card.transform.parent = transform.GetChild(cards.Count - 1);
        card.PointerEnterEvent.AddListener(CardPointerEnter);
        card.PointerExitEvent.AddListener(CardPointerExit);
        card.BeginDragEvent.AddListener(BeginDrag);
        card.EndDragEvent.AddListener(EndDrag);
        card.name = (cards.Count - 1).ToString();
        if (!isInAnimation[0]) StartCoroutine(AddCardCooldown());
        else addTimer = 0;
        return card;
    }

    private IEnumerator AddCardCooldown()
    {
        addTimer = 0;
        isInAnimation[0] = true;
        while (addTimer < 0.6f)
        {
            addTimer += Time.deltaTime;
            yield return null;
        }
        isInAnimation[0] = false;
        UpdatePosition();
    }

    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }


    void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }*/

        UpdateSelected();

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }

        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        for (int i = 0; i < cards.Count; i++)
        {

            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    private void UpdatePosition()
    {
        int index = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.childCount > 0) continue;
            index = i;
            while (transform.GetChild(i).childCount == 0)
            {
                i++;
                if (i >= transform.childCount) return;
            }
            Card card = transform.GetChild(i).GetChild(0).GetComponent<Card>();
            Transform indexParent = transform.GetChild(index);
            card.transform.SetParent(indexParent);
            card.transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
            i = index;
        }
    }

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (Card card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
        }
    }

    private List<bool> isInAnimation = new() { false, false, false };
    private void UpdateSelected()
    {
        int count = 0;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.childCount > 0)
            {
                Card card = child.GetChild(0).GetComponent<Card>();
                if (card.selected) count++;
            }
        }
        bool isClickable = true;
        foreach (bool check in isInAnimation)
            if (check == true)
            {
                isClickable = false;
                break;
            }
        if (count == 0 || !isClickable || GameManager.Instance.isTurnEnd)
        {
            GameManager.Instance.turnEndable = false;
            redrawButton.interactable = false;
        }
        else
        {
            GameManager.Instance.turnEndable = true;
            if (!scoreManager.CheckRedrawable()) redrawButton.interactable = false;
            else redrawButton.interactable = true;
        }
    }
}
