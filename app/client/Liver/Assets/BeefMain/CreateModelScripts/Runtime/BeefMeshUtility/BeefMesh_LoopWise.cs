using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BeefCreateModelScripts.Runtime.BeefMesh
{
public partial class BeefMeshUtility
{
    private const float TOLERANCE = 0.00001f;

    // 0 : 不定 -1 : 時計回り 1 : 反時計回り
    private static int CalcLoopwise(List<Vector2> aPoint)
    {
        int uiCountPoint = aPoint.Count;

        if (2 >= uiCountPoint)
        {
            return 0;
        }

        int ui = 0;
        // 左端（同じ左端ならば下側）の点を求める。
        int uiIndexLeft = 0;

        for (ui = 1; ui < uiCountPoint; ui++)
        {
            if (aPoint[uiIndexLeft].x > aPoint[ui].x
                    || (Mathf.Abs(aPoint[uiIndexLeft].x - aPoint[ui].x) < TOLERANCE && aPoint[uiIndexLeft].y > aPoint[ui].y))
            {
                uiIndexLeft = ui;
            }
        }

        // 左端点の前後の点を求める（座標値が左端点と異なる点まで進める）
        int uiIndexLeft_prev;

        if (0 == uiIndexLeft)
        {
            uiIndexLeft_prev = uiCountPoint - 1;
        }
        else
        {
            uiIndexLeft_prev = uiIndexLeft - 1;
        }

        for (ui = 0; ui < uiCountPoint; ui++)
        {
            if (aPoint[uiIndexLeft] != aPoint[uiIndexLeft_prev])
            {
                break;
            }

            if (0 == uiIndexLeft_prev)
            {
                uiIndexLeft_prev = uiCountPoint - 1;
            }
            else
            {
                uiIndexLeft_prev--;
            }
        }

        if (ui == uiCountPoint)
        {
            // 左端点と異なる前の点がない⇒すべて同じ座標値⇒多角形は点に縮退している。
            return 0;
        }

        int uiIndexLeft_next = (uiIndexLeft + 1) % uiCountPoint;

        for (ui = 0; ui < uiCountPoint; ui++)
        {
            if (aPoint[uiIndexLeft] != aPoint[uiIndexLeft_next])
            {
                break;
            }

            uiIndexLeft_next = (uiIndexLeft_next + 1) % uiCountPoint;
        }

        double dOuterProduct = CalcOuterProduct(
                                   aPoint[uiIndexLeft] - aPoint[uiIndexLeft_prev],
                                   aPoint[uiIndexLeft_next] - aPoint[uiIndexLeft]);

        if (0 > dOuterProduct)
        {
            // 時計回り
            return -1;
        }
        else if (0 < dOuterProduct)
        {
            // 反時計回り
            return 1;
        }
        else
        {
            return 0;
        }
    }

    private static float CalcOuterProduct(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

}
}
