using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Baloon : MonoBehaviour
{
    // 幟に記入する文字列
    public string Text { set; private get; }


    void Start()
    {
        // 幟オブジェクト
        var t = transform.Find("bg_balloon_002/curtain");
        var go = t.gameObject;
        // 文字をレンダリングするプレハブをくっつける
        var rotateText = Instantiate(Resources.Load("models/Background/RotateText"), t) as GameObject;
        rotateText.GetComponent<UnityEngine.UI.Text>().text = Text;
    }
}
