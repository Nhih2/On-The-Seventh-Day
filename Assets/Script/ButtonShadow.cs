using UnityEngine;
using UnityEngine.UI;

public class ButtonShadow : MonoBehaviour
{
    private Button myButton;
    [SerializeField] private Image shadow;
    [SerializeField] private Color normal, shaded;
    void Start()
    {
        myButton = GetComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        if (myButton.interactable)
        {
            shadow.color = normal;
        }
        else
            shadow.color = shaded;
    }
}
