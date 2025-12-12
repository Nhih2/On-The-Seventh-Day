using TMPro;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    #region SINGELTON
    public static PopupManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        HideMessage();
    }
    #endregion

    [SerializeField] private Transform Popup;
    [SerializeField] private TextMeshProUGUI nameTxt, desTxt;
    private Transform follow;
    private Vector3 offset;

    void Update()
    {
        if (follow)
        {
            Popup.transform.position = follow.position + offset;
            ClampPosition();
        }
    }

    public void ShowMessage(Vector3 position, string name, string description)
    {
        Popup.gameObject.SetActive(true);
        Popup.transform.position = position;
        nameTxt.text = name;
        desTxt.text = description;
        ClampPosition();
    }
    public void ShowMessage(Transform target, Vector3 offset, string name, string description)
    {
        Popup.gameObject.SetActive(true);
        Popup.transform.position = target.position + offset;
        follow = target;
        this.offset = offset;
        nameTxt.text = name;
        desTxt.text = description;
        ClampPosition();
    }
    public void HideMessage()
    {
        Popup.gameObject.SetActive(false);
        follow = null;
    }
    private void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = Popup.transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        Popup.transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }
}
