using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class SoundSettingWindow : Tsl.UI.Window
{
    public Toggle bgmTgl;
    public Toggle seTgl;

    private void Awake()
    {
        var bgmMng = BgmManager.Instance;
        var seMng = SeManager.Instance;
        bgmTgl.isOn = bgmMng.MasterVolume > 0.0f ? true : false;
        seTgl.isOn = seMng.MasterVolume > 0.0f ? true : false;
        bgmTgl.onValueChanged.RemoveAllListeners();
        seTgl.onValueChanged.RemoveAllListeners();
        bgmTgl.onValueChanged.AddListener(isOn =>
        {
            if (isOn) { bgmMng.MasterVolume = 1.0f; }
            else { bgmMng.MasterVolume = 0.0f; }
        });
        seTgl.onValueChanged.AddListener(isOn =>
        {
            if (isOn) { seMng.MasterVolume = 1.0f; }
            else { seMng.MasterVolume = 0.0f; }
        });
    }

    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "ButtonClose":
                SaveSoundData();
                base.OnClickButtonEvent(button);
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }

    private void SaveSoundData()
    {
        PlayerDataManager.PlayerData.EnableBgm(bgmTgl.isOn);
        PlayerDataManager.PlayerData.EnableSe(seTgl.isOn);
    }
}
}