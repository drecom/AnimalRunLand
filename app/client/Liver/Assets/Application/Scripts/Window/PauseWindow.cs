using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class PauseWindow : Tsl.UI.Window
{
    public Action onClose;

    public Text Countdown;
    public Text AreaName;
    public Text DistanceTxt;
    public Text StarTxt;

    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "ReStartButton":
                DrawCount();
                break;

            case "TopButton":
                var dialog = BaseScene.AddModalWindow<DialogWindow>("DialogWindow");
                dialog.CloseBtn.gameObject.SetActive(false);
                dialog.BtnPanel.gameObject.SetActive(true);
                dialog.SetDialog("TOPへ戻る", "", "スコアがリセットされます。\nよろしいですか？", null,
                () => { this.BaseScene.ChangeScene("Menu", 1.0f); }, () => { dialog.onClose(); });
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }

    public void SetStatus(string areaName, string distance, string star, string mission)
    {
        Map.AreaName.Get(areaName, (text) => { this.AreaName.text = text; });
        this.DistanceTxt.text = $"走行距離：{distance}";
        this.StarTxt.text     = $"獲得スター数：{star}個";
    }

    private void DrawCount()
    {
        this.GetPanel().gameObject.SetActive(false);
        this.transform.Find("ReStartPanel").gameObject.SetActive(true);
        StartCoroutine(ShowCountdownTime(3));
    }

    public IEnumerator ShowCountdownTime(int count, bool extraSound = false, float speed = 1.0f)
    {
        this.Countdown.gameObject.SetActive(true);
        var easing = new Shand.Easing();

        while (count > 0)
        {
            this.Countdown.text = count.ToString();
            easing.Start(Shand.EasingPattern.BounceOut, 0.5f * speed, 5.0f, 1.0f);

            while (!easing.Ended)
            {
                var s = easing.UpdateValue(Time.deltaTime) * 1.0f;
                this.Countdown.transform.localScale = new Vector3(s, s, 1.0f);
                yield return null;
            }

            SeManager.Instance.Play(Sound.SeKind.Countdown.ToString(), 0.0f, false, 1.0f);
            yield return new WaitForSeconds(0.5f * speed);
            --count;
        }

        this.Countdown.gameObject.SetActive(false);
        Destroy(GameObject.Find("PauseWindow(Clone)"));

        if (onClose != null) { onClose(); }
    }
}
}
