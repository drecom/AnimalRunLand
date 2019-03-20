using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liver
{
public class MainMenuWindow : Tsl.UI.Window
{
    public GameObject DebugButton;


    private void Awake()
    {
        var bgmMng = BgmManager.Instance;

        if (bgmMng.bgmKind != Sound.BgmKind.TOP)
        {
            bgmMng.Play(Sound.BgmKind.TOP, 0.0f, 0.0f, true, 1.0f);
        }

        if (Debug.isDebugBuild)
        {
            // デバッグ用のボタンを有効にする
            DebugButton.SetActive(true);
        }
    }
    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "StartButton":
                // レースボタンが押されたらメニューシーンに遷移
                Liver.RaceWindow.AnimalKind = (AnimalKind)Enum.Parse(typeof(AnimalKind), PlayerDataManager.PlayerData.LastSelectedAnimalId);
                this.BaseScene.ChangeScene("Race", 1.0f);
                break;

            case "AROW":
                // ブラウザ起動
                Application.OpenURL(Entity.AppSettings.Instance.ArowURL);
                break;

            case "Debug":
                // デバッグメニュー起動
                var setting = BaseScene.AddModalWindow<RaceDebugSetting>("RaceDebugSetting");
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }
}
}
