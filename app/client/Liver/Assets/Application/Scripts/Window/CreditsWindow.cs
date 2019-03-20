using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Liver
{
public class CreditsWindow : Tsl.UI.Window
{
    public Text CreditsText;
    public ScrollRect ScrollRect;

    public float Speed = 0.005f;

    Coroutine coroutine;


    void Awake()
    {
        // テキストを読み込んで流し込む
        var textAsset = Resources.Load("Configs/Credits") as TextAsset;
        this.CreditsText.text = textAsset.text;
        this.coroutine = StartCoroutine(ScrollCredits());
    }

    //public override void OnClickButtonEvent(Transform button)
    //{
    //    base.OnClickButtonEvent(button);
    //}


    void Update()
    {
    }


    IEnumerator ScrollCredits()
    {
        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            yield return 0;
            this.ScrollRect.verticalNormalizedPosition -= this.Speed;

            if (this.ScrollRect.verticalNormalizedPosition < 0.0f) { break; }
        }
    }
}
}
