using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liver
{
public class RaceStartWindow : Tsl.UI.Window
{
    public override void OnClickButtonEvent(Transform button)
    {
        if (button.name == "BackButton")
        {
            // BACKボタンが押されたらメニューシーンに遷移
            this.BaseScene.ChangeScene("Menu", 1.0f);
        }
        else
        {
            base.OnClickButtonEvent(button);
        }
    }

}
}
