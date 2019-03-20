using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class GpsWindow : Tsl.UI.Window
{
    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "OkButton":
                Debug.Log("OK");
                // 続いて名前入力画面
                BaseScene.AddModalWindow<NameInputWindow, Argments>("NameInputWindow", new Argments("new"));
                this.Close();
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }
}
}
