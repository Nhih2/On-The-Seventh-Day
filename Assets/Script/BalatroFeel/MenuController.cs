using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject game, menu;
    [SerializeField] private Button playButton;
    void Start()
    {
        playButton.onClick.AddListener(() =>
        {
            menu.gameObject.SetActive(false);
            game.gameObject.SetActive(true);
        });
    }
}
