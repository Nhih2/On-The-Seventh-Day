using TMPro;
using UnityEngine;

public class ItemCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    public PlayCard card;

    public void ShowCard(PlayCard playCard)
    {
        card = playCard;
        text.text = GameSetting.STATS_NAME[card.type1];
        if (card.type2 != -1)
            text.text += "/" + GameSetting.STATS_NAME[card.type2];
    }
}
