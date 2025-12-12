using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;

public class CardBody : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector3 originalScale, anchorOriginalPos;

    [Header("Card")]
    public Transform anchor, body;
    private Vector3 rotationDelta;
    private int savedIndex;
    Vector3 movementDelta;

    [Header("References")]
    [SerializeField] private Transform shakeParent;
    [SerializeField] private Transform tiltParent;
    [SerializeField] private Vector3 selectOffset = new Vector2(0, 0.5f);

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
    [SerializeField] private bool isSelectble = true;
    [SerializeField] private float selectPunchAmount = 20;

    [Header("Hover Parameters")]
    [SerializeField] private float hoverPunchAngle = 5;
    [SerializeField] private float hoverTransition = .15f;


    [Header("Curve")]
    [SerializeField] private CurveParameters curve;
    public bool isHovering = false, isSelect = false;

    private void Start()
    {
        originalScale = body.transform.localScale;
        anchorOriginalPos = anchor.transform.position;
        savedIndex = Random.Range(0, 10);
    }

    void Update()
    {
        SmoothFollow();
        FollowRotation();
        CardTilt();

    }

    private void SmoothFollow()
    {
        body.transform.position = Vector3.Lerp(body.transform.position, anchor.position, followSpeed * Time.deltaTime);
    }

    private void FollowRotation()
    {
        Vector3 movement = (body.transform.position - anchor.position);
        movementDelta = Vector3.Lerp(movementDelta, movement, 25 * Time.deltaTime);
        Vector3 movementRotation = movement * rotationAmount;
        rotationDelta = Vector3.Lerp(rotationDelta, movementRotation, rotationSpeed * Time.deltaTime);
        body.transform.eulerAngles = new Vector3(body.transform.eulerAngles.x, body.transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));
    }

    private void CardTilt()
    {
        float sine = Mathf.Sin(Time.time + savedIndex) * (isHovering ? .2f : 1);
        float cosine = Mathf.Cos(Time.time + savedIndex) * (isHovering ? .2f : 1);

        Vector3 offset = body.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float tiltX = isHovering ? ((offset.y * -1) * manualTiltAmount) : 0;
        float tiltY = isHovering ? ((offset.x) * manualTiltAmount) : 0;
        float tiltZ = 0;

        float lerpX = Mathf.LerpAngle(tiltParent.eulerAngles.x, tiltX + (sine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpY = Mathf.LerpAngle(tiltParent.eulerAngles.y, tiltY + (cosine * autoTiltAmount), tiltSpeed * Time.deltaTime);
        float lerpZ = Mathf.LerpAngle(tiltParent.eulerAngles.z, tiltZ, tiltSpeed / 2 * Time.deltaTime);

        tiltParent.eulerAngles = new Vector3(lerpX, lerpY, lerpZ);
    }

    private void Select()
    {
        isSelect = !isSelect;
        if (!isSelectble)
        {
            SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard);
            return;
        }
        if (isSelect)
            anchor.transform.position = anchorOriginalPos + selectOffset;
        else anchor.transform.position = anchorOriginalPos;
        if (isSelect) SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard);
        else SfxManager.Instance.PlaySfx(SfxManager.SfxType.SelectCard, 0.05f, 0.6f);
        DOTween.Kill(2, true);
        float dir = isSelect ? 0.02f : 0;
        shakeParent.DOPunchPosition(shakeParent.up * selectPunchAmount * dir, scaleTransition, 10, 1);
        shakeParent.DOPunchRotation(Vector3.forward * (hoverPunchAngle / 2), hoverTransition, 20, 1).SetId(2);

        if (scaleAnimations)
            body.transform.DOScale(scaleOnHover * originalScale, scaleTransition).SetEase(scaleEase);
    }

    public void Select(bool chosenSelect)
    {
        if (isSelect == chosenSelect) return;
        isSelect = !chosenSelect;
        Select();
    }

    public void ShakeCard(float magnitude = 1f)
    {
        DOTween.Kill(2, true);

        if (scaleAnimations)
            body.transform.DOScale(scaleOnHover * originalScale * magnitude, scaleTransition).SetEase(Ease.InBack).OnComplete(
                () =>
                {
                    shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle / 2, hoverTransition, 20, 1).SetId(2);
                    body.transform.DOScale(originalScale, scaleTransition).SetEase(Ease.OutBack);
                }
            );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleAnimations)
            body.transform.DOScale(scaleOnHover * originalScale, scaleTransition).SetEase(scaleEase);

        isHovering = true;
        DOTween.Kill(2, true);
        shakeParent.DOPunchRotation(Vector3.forward * hoverPunchAngle, hoverTransition, 20, 1).SetId(2);
        SfxManager.Instance.PlaySfx(SfxManager.SfxType.EnterCard, 0.005f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        if (scaleAnimations)
            body.transform.DOScale(originalScale, scaleTransition).SetEase(scaleEase);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (scaleAnimations)
            body.transform.DOScale(scaleOnSelect * originalScale, scaleTransition).SetEase(scaleEase);

        Select();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (scaleAnimations)
            body.transform.DOScale(scaleOnHover * originalScale, scaleTransition).SetEase(scaleEase);
    }
}
