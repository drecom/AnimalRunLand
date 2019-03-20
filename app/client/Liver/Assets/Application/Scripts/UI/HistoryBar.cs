using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class HistoryBar : MonoBehaviour
{
    public Text AreaTxt;
    public Text LatTxt;
    public Text LngTxt;

    public void OnClickViewMap()
    {
        var url = $"https://maps.apple.com/maps?q={this.LatTxt.text},{this.LngTxt.text}";
        Application.OpenURL(url);
        Debug.Log($"View Map: {url}");
    }
}
