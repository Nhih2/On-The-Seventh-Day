
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;

public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    private bool freeze = false;

    [SerializeField] private Canvas canvas;
    [SerializeField] private Image imageComponent;
    [SerializeField] private bool instantiateVisual = true;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public float longPressDelay = 0.25f;
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardVisual cardVisual;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;

    public PlayerCard Init(Transform parent)
    {
        canvas = parent.GetComponent<Canvas>();
        if (!instantiateVisual)
            return null;

        cardVisual = Instantiate(cardVisualPrefab, parent).GetComponent<CardVisual>();
        cardVisual.Initialize(this);

        StartCoroutine(Frame());
        SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard, 0.2f, 0.7f);

        IEnumerator Frame()
        {
            yield return new WaitForSeconds(.2f);
            BeginDragEvent.Invoke(this);
            yield return new WaitForSeconds(.1f);
            EndDragEvent.Invoke(this);
        }
        return cardVisual.GetComponent<PlayerCard>();

    }

    void Update()
    {
        ClampPosition();

        if (isDragging)
        {
            if (freeze) return;
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    void ClampPosition()
    {
        if (freeze) return;
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (freeze) return;
        BeginDragEvent.Invoke(this);
        cardVisual.playerCard.HideMessage();
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        wasDragged = true;
        //Debug.Log("[CARD.OnBeginDrag] wasDrag: " + wasDragged);
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (freeze) return;
        EndDragEvent.Invoke(this);
        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
            //Debug.Log("[CARD.OnEndDrag] wasDrag: " + wasDragged);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (freeze) return;
        //Debug.Log("[OnPointerEnter] Pressed: " + eventData.button);
        PointerEnterEvent.Invoke(this);
        isHovering = true;
        cardVisual.playerCard.ShowMessage();
        SfxManager.Instance.PlaySfx(SfxManager.SfxType.EnterCard, 0.005f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (freeze) return;
        //Debug.Log("[OnPointerExit] Pressed: " + eventData.button);
        PointerExitEvent.Invoke(this);
        isHovering = false;
        cardVisual.playerCard.HideMessage();
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (freeze) return;
        //Debug.Log("[OnPointerDown] Pressed: " + eventData.button);
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (freeze) return;
        //Debug.Log("[OnPointerUp] Pressed: " + eventData.button);
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > longPressDelay);

        //Debug.Log("[OnPointerUp] Up: " + pointerUpTime + " /Down: " + pointerDownTime + " /delay: " + longPressDelay);
        if (pointerUpTime - pointerDownTime > longPressDelay)
            return;

        //Debug.Log("[OnPointerUp] wasDragged: " + wasDragged);
        if (wasDragged)
            return;

        selected = !selected;
        SelectEvent.Invoke(this, selected);
        if (selected) SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard);
        else SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard, 0.05f, 0.6f);

        Debug.Log("[OnPointerUP] Selected: " + selected);
        if (selected)
            transform.localPosition += cardVisual.transform.up * selectionOffset;
        else
            transform.localPosition = Vector3.zero;
    }

    public void Deselect()
    {
        if (freeze) return;
        //Debug.Log("[Deselect] Selected: " + selected);
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += (cardVisual.transform.up * selectionOffset);
            else
                transform.localPosition = Vector3.zero;
        }
    }


    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {
        if (transform.parent.CompareTag("Slot"))
        {
            return (float)((float)ParentIndex()).Remap(0, (float)(transform.parent.parent.childCount - 1), 0, 1);
        }
        else
        {
            return 0;
        }
    }

    private void OnDestroy()
    {
        if (cardVisual != null)
            Destroy(cardVisual.gameObject);
    }

    public void Freeze()
    {
        freeze = true;
        EndDragEvent.Invoke(this);
    }

    public void ResetPosition()
    {
        EndDragEvent.Invoke(this);
    }
}
