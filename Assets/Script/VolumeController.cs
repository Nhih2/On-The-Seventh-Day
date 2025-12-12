using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{

    [SerializeField] private List<Sprite> volumeSprites;
    private Button myButton;
    private Image myImg;

    void Awake()
    {
        myImg = GetComponent<Image>();
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (GameSetting.MASTER_BG_VOLUME == 1)
        {
            GameSetting.MASTER_BG_VOLUME = 0.5f;
            GameSetting.MASTER_SFX_VOLUME = 0.5f;
            myImg.sprite = volumeSprites[1];
        }
        else if (GameSetting.MASTER_BG_VOLUME == 0.5f)
        {
            GameSetting.MASTER_BG_VOLUME = 0f;
            GameSetting.MASTER_SFX_VOLUME = 0f;
            myImg.sprite = volumeSprites[2];
        }
        else
        {
            GameSetting.MASTER_BG_VOLUME = 1f;
            GameSetting.MASTER_SFX_VOLUME = 1f;
            myImg.sprite = volumeSprites[0];
        }
    }
}
