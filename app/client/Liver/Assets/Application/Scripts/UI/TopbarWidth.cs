using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class TopbarWidth : MonoBehaviour
{
    public RectTransform parentTransform;


    // iPhoneX対応のため、親階層の幅に併せる
    void Start()
    {
        if (this.parentTransform == null)
        {
            // ２つ上の階層→全画面パネル
            var go = transform.parent.parent.gameObject;
            this.parentTransform = go.GetComponent<RectTransform>();
        }

        var parentRect = this.parentTransform.rect;
        var myTransform = GetComponent<RectTransform>();
        var rect = myTransform.rect;
        var d = (parentRect.width - rect.width) * 0.5f;
        var ofs = myTransform.offsetMin;
        ofs.x = -d;
        myTransform.offsetMin = ofs;
        ofs = myTransform.offsetMax;
        ofs.x = d;
        myTransform.offsetMax = ofs;
    }
}
