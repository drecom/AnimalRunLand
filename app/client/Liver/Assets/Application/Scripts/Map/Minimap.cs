using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Minimap
{
    // シェーダーに各種色を適用
    static public void ApplyColors(ref Material material)
    {
        material.SetColor("_GroundColor", Liver.Entity.RaceSettings.Instance.MinimapBaseColor);
        material.SetColor("_PathColor", Liver.Entity.RaceSettings.Instance.MinimapPathColor);
    }

    // POIを適用
    static public void ApplyPoi(List<Vector3> pos, ref Texture2D texture, float scale)
    {
        var halfWidth  = (int)(texture.width * 0.5f);
        var halfHeight = (int)(texture.height * 0.5f);
        int[,] bitmap =
        {
            { 0, 0, 1, 1, 1, 0, 0 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 0 },
            { 0, 0, 1, 1, 1, 0, 0 },
        };
        var color = Liver.Entity.RaceSettings.Instance.MinimapPoiColor;

        foreach (var p in pos)
        {
            var x = (int)(p.x * scale);
            var y = (int)(p.z * scale);

            // マップ外のPOIはスルー
            if (Mathf.Abs(x) > halfWidth
                    || Mathf.Abs(y) > halfHeight) { continue; }

            x = x + halfWidth - 3;
            y = y + halfHeight - 3;

            for (int ox = 0; ox < 7; ++ox)
            {
                for (int oy = 0; oy < 7; ++oy)
                {
                    if (bitmap[oy, ox] != 0)
                    {
                        texture.SetPixel(x + ox, y + oy, color);
                    }
                }
            }
        }

        // 他所でやるので気にしない
        //texture.Apply();
    }

    // 最外周を塗りつぶす
    static public void FillOuterFrame(ref Texture2D texture)
    {
        var w = texture.width;
        var h = texture.height;
        var color = Liver.Entity.RaceSettings.Instance.MinimapBaseColor;

        for (int x = 0; x < w; ++x)
        {
            texture.SetPixel(x,     0, color);
            texture.SetPixel(x, h - 1, color);
        }

        for (int y = 0; y < h; ++y)
        {
            texture.SetPixel(0, y, color);
            texture.SetPixel(w - 1, y, color);
        }
    }

}
