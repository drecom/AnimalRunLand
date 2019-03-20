using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liver
{
public class TitleWindow : Tsl.UI.Window
{
    private void Awake()
    {
        // FIXME データを読み込むタイミング
        var result = PlayerDataManager.Load();

        if (!result)
        {
            // セーブデータが存在しない
            Debug.Log("New game!");
            // GPS使用の確認
            BaseScene.AddModalWindow<GpsWindow>("GpsWindow");
        }
        else
        {
            this.BaseScene.ChangeScene("Menu", 1.0f);
        }

        var data = PlayerDataManager.PlayerData;
        var bgmMng = BgmManager.Instance;
        bgmMng.MasterVolume = data.BgmOn ? 1.0f : 0f;
        var seMng = SeManager.Instance;
        seMng.MasterVolume = data.SeOn ? 1.0f : 0f;
        bgmMng.Play(Sound.BgmKind.TOP, 0.0f, 0.0f, true, 1.0f);
    }
    public override void OnClickButtonEvent(Transform button)
    {
        if (button.name == "StartButton")
        {
            // スタートボタンが押されたらメニューシーンに遷移
            this.BaseScene.ChangeScene("Menu", 1.0f);
        }
        else
        {
            base.OnClickButtonEvent(button);
        }

        //BgmManager.Instance.Stop(1.0f);
    }


}
}
