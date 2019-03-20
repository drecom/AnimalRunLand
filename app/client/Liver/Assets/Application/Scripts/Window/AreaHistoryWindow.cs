using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class AreaHistoryWindow : Tsl.UI.Window
{
    public Text NonHistoryTxt;
    public Transform Content;

    protected override void OnStart()
    {
        MakeHistoryList();
    }

    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "MapButton":
                var bar = button.GetComponentInParent<HistoryBar>();
                var url = $"https://maps.apple.com/maps?q={bar.LatTxt.text},{bar.LngTxt.text}";
                Application.OpenURL(url);
                Debug.Log($"View Map: {url}");
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }

    private List<Entity.PlayerData.Result> history = new List<Entity.PlayerData.Result>();
    private int historyCnt = 0;
    private void MakeHistoryList()
    {
        this.history = PlayerDataManager.PlayerData.ResultHistory;
        this.historyCnt = history.Count - 1;

        if (history.Count == 0)
        {
            this.NonHistoryTxt.gameObject.SetActive(true);
            return;
        }

        MakeHistoryBar();
    }
    private void MakeHistoryBar()
    {
        var template = Resources.Load("UI/HistoryBar");
        Map.AreaName.Get(history[historyCnt].AreaName, name =>
        {
            Debug.Log($"Area: {this.history[this.historyCnt].AreaName}");
            var historyBar = Instantiate(template) as GameObject;
            historyBar.transform.SetParent(this.Content);
            historyBar.transform.localPosition = Vector3.zero;
            historyBar.transform.localScale    = Vector3.one;
            var bar = historyBar.GetComponent<HistoryBar>();
            //bar.AreaTxt.text = history[historyCnt].AreaName;
            bar.AreaTxt.text = name;
            bar.LatTxt.text = this.history[this.historyCnt].Latitude.ToString();
            bar.LngTxt.text = this.history[this.historyCnt].Longitude.ToString();
            --this.historyCnt;

            if (this.historyCnt >= 0) { MakeHistoryBar(); }
        });
    }
}
}
