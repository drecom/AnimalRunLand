using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;


public class SetCanvasBounds : MonoBehaviour
{
    RectTransform canvas;

    // セーフエリアを考慮する階層
    public RectTransform Panel;

    Rect lastSafeArea = new Rect(0, 0, 0, 0);

    // Use this for initialization
    void Awake()
    {
        this.canvas = GetComponent<RectTransform>();
        this.lastSafeArea = SafeArea();
        ApplySafeArea(this.lastSafeArea);
    }

    void ApplySafeArea(Rect area)
    {
        var display = Display.displays[0];
        var screenSize = new Vector2Int(display.systemWidth, display.systemHeight);
        var anchorMin = area.position;
        var anchorMax = area.position + area.size;
        anchorMin.x /= screenSize.x;
        anchorMin.y /= screenSize.y;
        anchorMax.x /= screenSize.x;
        anchorMax.y /= screenSize.y;
        this.Panel.anchorMin = anchorMin;
        this.Panel.anchorMax = anchorMax;
    }

    void Update()
    {
        Rect safeArea = SafeArea();

        // FIXME 画面の回転などを考慮して、毎フレーム計算している
        if (safeArea != lastSafeArea)
        {
            this.lastSafeArea = safeArea;
            ApplySafeArea(safeArea);
        }
    }

    Rect SafeArea()
    {
        var area = Screen.safeArea;
#if UNITY_EDITOR

        if (Screen.width == 1125 && Screen.height == 2436 || Screen.width == 2436 && Screen.height == 1125)
        {
            Vector2 positionOffset;
            Vector2 sizeOffset;

            //縦持ち
            if (Screen.width < Screen.height)
            {
                positionOffset = new Vector2(0f, area.size.y * 34f / 812f);
                sizeOffset     = new Vector2(0f, area.size.y * 44f / 812f);
            }
            //横持ち
            else
            {
                positionOffset = new Vector2(area.size.x * 44f / 812f, area.size.y * 21f / 375f);
                sizeOffset     = new Vector2(area.size.x * 44f / 812f, 0);
            }

            area.position = area.position + positionOffset;
            area.size     = area.size - positionOffset - sizeOffset;
        }

#endif
        return area;
    }
}
