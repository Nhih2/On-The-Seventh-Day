using UnityEngine;
using UnityEngine.EventSystems;

public class BewareSign : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Vector3 PopupPosition;
    private ScenarioCard card;
    private string warning;
    public void ShowBeware(ScenarioCard scenarioCard, string message)
    {
        card = scenarioCard;
        warning = message;
        card.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SfxManager.Instance.PlaySfx(SfxManager.SfxType.EnterCard, 0.005f);
        card.gameObject.SetActive(true);
        PopupManager.Instance.ShowMessage(
                                PopupPosition,
                                "<color=red>!!! WARNING !!!</color>",
                                warning);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        card.gameObject.SetActive(false);
        PopupManager.Instance.HideMessage();
    }
}
