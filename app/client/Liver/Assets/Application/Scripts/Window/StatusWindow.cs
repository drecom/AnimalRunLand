using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class StatusWindow : Tsl.UI.Window
{
    public Text UserNameTxt;
    public Text StarNumTxt;
    public Text AreaNumTxt;
    public Text TotalDistanceTxt;


    protected override void OnStart()
    {
        SetData();
    }
    private void SetData()
    {
        this.UserNameTxt.text      = PlayerDataManager.PlayerData.PlayerName;
        this.StarNumTxt.text       = PlayerDataManager.PlayerData.StarNum.ToString();
        this.AreaNumTxt.text       = $"{PlayerDataManager.PlayerData.AreaHistory.Count}こ";
        this.TotalDistanceTxt.text = $"{PlayerDataManager.PlayerData.TotalDistance / 1000:F2}km";
    }
    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "NameChangeButton":
                var win = BaseScene.AddModalWindow<NameInputWindow, Argments>("NameInputWindow", new Argments(""));
                win.onEnd = SetData;
                break;

            case "AreaHistoryButton":
                BaseScene.AddModalWindow<AreaHistoryWindow>("AreaHistoryWindow");
                break;

            //case "HistoryBackButton":
            //    foreach (Transform tran in this.Content)
            //    {
            //        GameObject.Destroy(tran.gameObject);
            //    }
            //    this.History.gameObject.SetActive(false);
            //    break;
            case "ButtonClose":
                this.GetPanel().gameObject.SetActive(false);
                this.Close();
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }

}
}
