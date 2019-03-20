using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollStretch : MonoBehaviour
{
    public RectTransform panel;

    // iPhoneX対応のため、スクロールの表示を画面の下いっぱいまで表示させる
    // panelは必須。window直下のPanelを設定
    void Start()
    {
        var parentRect = this.panel.rect;
        var myTransform = GetComponent<RectTransform>();
        var rect = myTransform.rect;
        var a = parentRect.height - System.Math.Abs(myTransform.sizeDelta.y) - rect.height;
        var ofs = myTransform.offsetMin;
        ofs.y = -a;
        myTransform.offsetMin = ofs;
    }
}
