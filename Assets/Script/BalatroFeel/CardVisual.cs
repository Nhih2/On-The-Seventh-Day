using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardVisual : MonoBehaviour
{
    private bool initalize = false;
    private Vector3 originalScale;

    [Header("Card")]
    public PlayerCard playerCard;
    public Card parentCard;
    private Transform cardTransform;
    private Vector3 rotationDelta;
    private int savedIndex;
    Vector3 movementDelta;

    [Header("References")]
    [SerializeField] private Canvas canvas;
    public Transform visualShadow;
    private Vector2 shadowDistance;
    private Canvas shadowCanvas;
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;
    [SerializeField] private Image cardImage;
    [Header("Shadow Parameters")]
    [SerializeField] private Vector2 shadowOffset = new Vector2(-0.1f, 0.4f);

    [Header("Follow Parameters")]
    [SerializeField] private float followSpeed = 30;

    [Header("Rotation Parameters")]
    [SerializeField] private float rotationAmount = 20;
    [SerializeField] private float rotationSpeed = 20;
    [SerializeField] private float autoTiltAmount = 30;
    [SerializeField] private float manualTiltAmount = 20;
    [SerializeField] private float tiltSpeed = 20;

    [Header("Scale Parameters")]
    [SerializeField] private bool scaleAnimations = true;
    [SerializeField] private float scaleOnHover = 1.15f;
    [SerializeField] private float scaleOnSelect = 1.25f;
    [SerializeField] private float scaleTransition = .15f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;

    [Header("Select Parameters")]
    [SerializeField] private float selectPunchAmount = 20;

    [Header("Hober Parameters")]
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;

    [Header("Swap Parameters")]
    [SerializeField] private bool swapAnimations = true;
    [SerializeField] private float swapRotationAngle = 30;
    [SerializeField] private float swapTransition = .15f;
    [SerializeField] private int swapVibrato = 5;

    [Header("Curve")]
    [SerializeField] private CurveParameters curve;

    private float curveYOffset;
    private float curveRotationOffset;
    private Coroutine pressCoroutine;

    private void Start()
    {
        shadowDistance = visualShadow.localPosition;
        originalScale = transform.localScale;
    }

    public void Initialize(Card target, int index = 0)
    {
        //Declarations
        parentCard = target;
        cardTransform = target.transform;
        shadowCanvas = visualShadow.GetComponent<Canvas>();

        //Event Listening
        parentCard.PointerEnterEvent.AddListener(PointerEnter);
        parentCard.PointerExitEvent.AddListener(PointerExit);
        parentCard.BeginDragEvent.AddListener(BeginDrag);
        parentCard.EndDragEvent.AddListener(EndDrag);
        parentCard.PointerDownEvent.AddListener(PointerDown);
        parentCard.PointerUpEvent.AddListener(PointerUp);
        parentCard.SelectEvent.AddListener(Select);

        //Initialization
        initalize = true;

        //Set Shadow
        canvas.overrideSorting = true;
        canvas.sortingLayerName = GameSetting.SORTING_LAYER[2];
        canvas.sortingOrder = 5;

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.sortingLayerName = GameSetting.SORTING_LAYER[2];
        shadowCanvas.sortingOrder = 4;
    }

    public void UpdateIndex(int length)
    {
        transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }

    void Update()
    {
        if (!initalize || parentCard == null) return;

        HandPositioning();
        SmoothFollow();
        FollowRotation();
        CardTilt();

    }

    private void HandPositioning()
    {
        curveYOffset = (curve.positioning.Evaluate(parentCard.NormalizedPosition()) * curve.positioningInfluence) * parentCard.SiblingAmount();
        curveYOffset = parentCard.SiblingAmount() < 5 ? 0 : curveYOffset;
        curveRotationOffset = curve.rotation.Evaluate(parentCard.NormalizedPosition());
    }

    private void SmoothFollow()
    {
        Vector3 verticalOffset = (Vector3.up * (parentCard.isDragging ? 0 : curveYOffset));
        transform.position = Vector3.Lerp(transform.position, cardTransform.position + verticalOffset, followSpeed * Time.deltaTime);
    }

    private void FollowRotation()
    {
        Vector3 movement = (transform.position - cardTransform.position);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = (parentCard.isDragging ? movementDelta : movement) * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
    }

    private void CardTilt()
    {
        savedIndex = parentCard.isDragging ? savedIndex : parentCard.ParentIndex();
        float sine = Mathf.Sin(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + savedIndex) * (parentCard.isHovering ? .2f : 1);

        Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float tiltX = parentCard.isHovering ? ((offset.y * -1) * manualTiltAmount) : 0;
        float tiltY = parentCard.isHovering ? ((offset.x) * manualTiltAmount) : 0;
        float tiltZ = parentCard.isDragging ? tiltParent.eulerAngles.z : (curveRotationOffset * (curve.rotationInfluence * parentCard.SiblingAmount()));

        float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    private void Select(Card card, bool state)
    {
        DOTween.Kill(2, true);
        float dir = state ? 1.25f : 0;
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);
        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);

        if (scaleAnimations)
            transform.DOScale(scaleOnHover * originalScale, scaleTransition).SetEase(scaleEase);

    }

    public void Swap(float dir = 1)
    {
        if (!swapAnimations)
            return;

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation((Vector3.forward * swapRotationAngle) * dir, swapTransition, swapVibrato, 1).SetId(3);
    }

    private void BeginDrag(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect * originalScale, scaleTransition).SetEase(scaleEase);

        canvas.overrideSorting = true;
        canvas.sortingLayerName = GameSetting.SORTING_LAYER[4];
        canvas.sortingOrder = 50;
        visualShadow.localPosition += -(Vector3)shadowOffset;
        shadowCanvas.sortingLayerName = GameSetting.SORTING_LAYER[4];
        shadowCanvas.sortingOrder = 49;
    }

    private void EndDrag(Card card)
    {
        canvas.overrideSorting = true;
        canvas.sortingLayerName = GameSetting.SORTING_LAYER[2];
        canvas.sortingOrder = 5;

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.sortingLayerName = GameSetting.SORTING_LAYER[2];
        shadowCanvas.sortingOrder = 4;
        transform.DOScale(originalScale, scaleTransition).SetEase(scaleEase);
    }

    private void PointerEnter(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnHover * originalScale, scaleTransition).SetEase(scaleEase);

        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
    }

    private void PointerExit(Card card)
    {
        if (!parentCard.wasDragged)
            transform.DOScale(originalScale, scaleTransition).SetEase(scaleEase);
    }

    private void PointerUp(Card card, bool longPress)
    {
        if (scaleAnimations)
            transform.DOScale((longPress ? scaleOnHover : scaleOnSelect) * originalScale, scaleTransition).SetEase(scaleEase);
        canvas.sortingLayerName = GameSetting.SORTING_LAYER[2];
        canvas.sortingOrder = 5;

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.sortingLayerName = GameSetting.SORTING_LAYER[2];
        shadowCanvas.sortingOrder = 4;
    }

    private void PointerDown(Card card)
    {
        if (scaleAnimations)
            transform.DOScale(scaleOnSelect * originalScale, scaleTransition).SetEase(scaleEase);

        visualShadow.localPosition = shadowDistance;
        shadowCanvas.sortingLayerName = GameSetting.SORTING_LAYER[2];
        shadowCanvas.sortingOrder = 4;
    }

    public void ShakeCard(float magnitude = 1f)
    {
        DOTween.Kill(2, true);

        if (scaleAnimations)
            transform.DOScale(scaleOnHover * originalScale * magnitude, scaleTransition).SetEase(Ease.InBack).OnComplete(
                () =>
                {
                    shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle / 2, hoverTransition, 20, 1).SetId(2);
                    transform.DOScale(originalScale, scaleTransition).SetEase(Ease.OutBack);
                }
            );
    }
}
