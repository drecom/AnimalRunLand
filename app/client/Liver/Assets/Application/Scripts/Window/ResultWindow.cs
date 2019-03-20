using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using System.Linq;

namespace Liver
{
public class ResultWindow : Tsl.UI.Window
{
    public Text Distance;
    public Text Star;
    public Text Score;
    public Text Ranking;
    public Text AreaName;


    protected override void OnStart()
    {
        var arg = this.GetArgments();
        var distance       = float.Parse(arg.args[1]);
        this.Distance.text = $"走行距離：{RaceArea.FormatDistance(distance)}";
        this.Star.text     = $"獲得スター数：{float.Parse(arg.args[2])}個";
        this.Score.text    = $"{arg.args[4]}pt";
        Map.AreaName.Get(arg.args[5], (text) => { this.AreaName.text = text; });
        StartCoroutine(setScoreResultCoroutine());
        BgmManager.Instance.Play(Sound.BgmKind.result, 0.0f, 0.0f, false, 1.0f);
    }

    private IEnumerator setScoreResultCoroutine()
    {
        // データが来るまで待つ
        while (Tsl.Network.GameServer.Instance.busy()) { yield return null; }

        var result = Tsl.Network.GameServer.Instance.SendScoreResult;

        if (result != null && result.Accepted)
        {
            // 結果が取得できた
            Debug.Log($"あなたのハイスコアは{result.MyHighScore}点です。");
            Debug.Log($"あなたの順位は{result.OrderInStage}位です。");
            this.Ranking.text = $"エリアランキング：{result.OrderInStage}位";
        }
        else
        {
            // エラー
            Debug.LogError("スコアの送信に失敗しました");
        }
    }

    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "BackButton":
                // BACKボタンが押されたらメニューシーンに遷移
                this.BaseScene.ChangeScene("Menu", 1.0f);
                break;

            case "RestartButton":
                // 同じ位置で始める
                RaceWindow.ForceLocation = true;
                // シーン遷移でレース再開
                this.BaseScene.ChangeScene("Race", 1.0f);
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }
}
}
