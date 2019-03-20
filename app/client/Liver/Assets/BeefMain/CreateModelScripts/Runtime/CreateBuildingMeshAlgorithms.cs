using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace BeefCreateModelScripts.Runtime
{
/// <summary>
/// 座標情報から 建物の Mesh を作成する.
/// </summary>
public class CreateBuildingMeshAlgorithms
{
    /// <summary>
    /// 9 slice テクスチャを利用した建物メッシュの作成.
    /// </summary>
    public static void CreateBuildingMesh_WithNineSlice(List<Vector2> positionList, float height, Action<Mesh> callback)
    {
        positionList = PositionSep(positionList);
        positionList = SetCenter(positionList);
        Vector2 leftDown = new Vector2(0.31f, 0.22f);
        Vector2 rightUp = new Vector2(1 - leftDown.x, 1 - leftDown.y);
        const float levelHeight = 4.0f;
        var maxLevel = Mathf.CeilToInt(height / levelHeight);
        var verticesList = new List<Vector3>();

        for (int level = 0; level < maxLevel; level++)
        {
            for (int i = 0; i < positionList.Count; i++)
            {
                var pos = positionList[i];
                verticesList.Add(
                    new Vector3(pos.x, level * levelHeight, pos.y)
                );
                verticesList.Add(
                    new Vector3(pos.x, (level + 1) * levelHeight, pos.y)
                );
            }
        }

        positionList = SetCenter(positionList);
        int loop = CalcLoopwise(positionList);
        var trianglesList = new List<int>();
        var triangleSet = new Action<bool, int, int, List<int>>(delegate(bool clock, int inNowIndex, int inNextIndex, List<int> targetTrianglesList)
        {
            if (clock)
            {
                targetTrianglesList.Add(inNowIndex);
                targetTrianglesList.Add(inNextIndex);
                targetTrianglesList.Add(inNextIndex + 1);
                targetTrianglesList.Add(inNextIndex + 1);
                targetTrianglesList.Add(inNowIndex + 1);
                targetTrianglesList.Add(inNowIndex);
            }
            else
            {
                targetTrianglesList.Add(inNowIndex);
                targetTrianglesList.Add(inNextIndex + 1);
                targetTrianglesList.Add(inNextIndex);
                targetTrianglesList.Add(inNextIndex + 1);
                targetTrianglesList.Add(inNowIndex);
                targetTrianglesList.Add(inNowIndex + 1);
            }
        });

        for (int level = 0; level < maxLevel; level++)
        {
            for (int i = 0; i  < positionList.Count; i++)
            {
                int nowIndex = level * positionList.Count * 2 + i * 2;
                int next = ((i + 1) != positionList.Count) ? (i * 2 + 2) : 0;
                int nextIndex = level * positionList.Count * 2 + next;
                triangleSet(loop == -1, nowIndex, nextIndex, trianglesList);
            }
        }

        var uvList = new List<Vector2>();

        for (int level = 0; level < maxLevel; level++)
        {
            float uvDown = 0;
            float uvUp = 1;

            if (level == 0)
            {
                uvUp = leftDown.y;
            }
            else if (level == maxLevel - 1)
            {
                uvDown = rightUp.y;
            }
            else
            {
                uvDown = leftDown.y;
                uvUp = rightUp.y;
            }

            for (int i = 0; i < positionList.Count; i++)
            {
                uvList.Add(new Vector2(i % 2 == 0 ? leftDown.x : rightUp.x, uvDown));
                uvList.Add(new Vector2(i % 2 == 0 ? leftDown.x : rightUp.x, uvUp));
            }
        }

        Mesh mesh = new Mesh();
        {
            mesh.vertices = verticesList.ToArray();
            mesh.uv = uvList.ToArray();
        }
        mesh.subMeshCount = 1;
        string print = "";
        print += "vettices: " + Environment.NewLine + string.Join("," + Environment.NewLine, verticesList.Select(v => v.x + ":" + v.y + ":" + v.z).ToArray()) + Environment.NewLine;
        print += "triangle: " + Environment.NewLine + string.Join("," + Environment.NewLine, trianglesList.Select(i => i.ToString()).ToArray()) + Environment.NewLine;
        print += "uv: " + Environment.NewLine + string.Join(","  + Environment.NewLine, uvList.Select(v => v.x + ":" + v.y).ToArray()) + Environment.NewLine;
        mesh.SetTriangles(trianglesList.ToArray(), 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (callback != null)
        {
            callback(mesh);
        }
    }

    /// <summary>
    /// 複数枚テクスチャを利用した建物メッシュの作成.
    /// </summary>
    public static void CreateBuildingMesh(List<Vector2> positionList, float height, Action<Mesh> callback)
    {
        positionList = PositionSep(positionList);
        positionList = SetCenter(positionList);
        const float levelHeight = 5.0f;
        var maxLevel = Mathf.CeilToInt(height / levelHeight);
        var verticesList = new List<Vector3>();

        for (int level = 0; level < maxLevel; level++)
        {
            for (int i = 0; i < positionList.Count; i++)
            {
                var pos = positionList[i];
                verticesList.Add(
                    new Vector3(pos.x, level * levelHeight, pos.y)
                );
                verticesList.Add(
                    new Vector3(pos.x, (level + 1) * levelHeight, pos.y)
                );
            }
        }

        int loop = CalcLoopwise(positionList);
        var trianglesList = new List<int>();
        var subTrianglesList = new List<int>();
        var triangleSet = new Action<bool, int, int, List<int>>(delegate(bool clock, int inNowIndex, int inNextIndex, List<int> targetTrianglesList)
        {
            if (clock)
            {
                targetTrianglesList.Add(inNowIndex);
                targetTrianglesList.Add(inNextIndex);
                targetTrianglesList.Add(inNextIndex + 1);
                targetTrianglesList.Add(inNextIndex + 1);
                targetTrianglesList.Add(inNowIndex + 1);
                targetTrianglesList.Add(inNowIndex);
            }
            else
            {
                targetTrianglesList.Add(inNowIndex);
                targetTrianglesList.Add(inNextIndex + 1);
                targetTrianglesList.Add(inNextIndex);
                targetTrianglesList.Add(inNextIndex + 1);
                targetTrianglesList.Add(inNowIndex);
                targetTrianglesList.Add(inNowIndex + 1);
            }
        });

        for (int level = 0; level < maxLevel; level++)
        {
            for (int i = 0; i  < positionList.Count; i++)
            {
                int nowIndex = level * positionList.Count * 2 + i * 2;
                int next = ((i + 1) != positionList.Count) ? (i * 2 + 2) : 0;
                int nextIndex = level * positionList.Count * 2 + next;

                if (level == 0)
                {
                    triangleSet(loop == -1, nowIndex, nextIndex, trianglesList);
                }
                else
                {
                    triangleSet(loop == -1, nowIndex, nextIndex, subTrianglesList);
                }
            }
        }

        var uvList = new List<Vector2>();

        for (int level = 0; level < maxLevel; level++)
        {
            float uvFloor = 0;
            float uvCeil = 1;

            for (int i = 0; i < positionList.Count; i++)
            {
                uvList.Add(new Vector2(i % 2 == 0 ? 0 : 1, uvFloor));
                uvList.Add(new Vector2(i % 2 == 0 ? 0 : 1, uvCeil));
            }
        }

        Mesh mesh = new Mesh();
        {
            mesh.vertices = verticesList.ToArray();
            mesh.uv = uvList.ToArray();
        }
        mesh.subMeshCount = 2;
        mesh.SetTriangles(trianglesList.ToArray(), 0);
        mesh.SetTriangles(subTrianglesList.ToArray(), 1);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (callback != null)
        {
            callback(mesh);
        }
    }

    private static float TOLERANCE = 0.00001f;

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
                    || (Math.Abs(aPoint[uiIndexLeft].x - aPoint[ui].x) < TOLERANCE && aPoint[uiIndexLeft].y > aPoint[ui].y))
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

    public static void CreateYaneMesh(List<Vector2> positionList, float height, Action<Mesh> callback)
    {
        positionList = PositionSep(positionList);
        positionList = SetCenter(positionList);
        Mesh yaneMesh = Triangulator.CreateInfluencePolygon(positionList.ToArray(), height);
        callback(yaneMesh);
    }

    private static float CalcOuterProduct(Vector2 a, Vector2 b)
    {
        return a.x * b.y - a.y * b.x;
    }

    /// <summary>
    /// 建物の座標データを細かくする.
    /// FIXME: 利用者からわかりやすくする.
    /// </summary>
    private static List<Vector2> PositionSep(List<Vector2> positionList)
    {
        var retPositionList = new List<Vector2>();

        for (int i = 1; i < positionList.Count; i++)
        {
            const float distanceSeparater = 4.0f;
            float distance = Vector2.Distance(positionList[i - 1], positionList[i]);
            int sep = Mathf.Max(1, Mathf.FloorToInt(distance / distanceSeparater));

            for (int t = 0; t < sep; t++)
            {
                var pos = Vector2.Lerp(positionList[i - 1], positionList[i], (t * 1.0f / sep));
                retPositionList.Add(
                    new Vector2(pos.x, pos.y)
                );
            }
        }

        {
            var pos = positionList[positionList.Count - 1];
            retPositionList.Add(
                new Vector2(pos.x, pos.y)
            );
        }

        return retPositionList;
    }


    private static List<Vector2> SetCenter(List<Vector2> positionList)
    {
        var retPositionList = new List<Vector2>();
        Vector2 center = new Vector2(
            positionList.Average((Vector2 arg) => arg.x),
            positionList.Average((Vector2 arg) => arg.y)
        );
        retPositionList = positionList.Select((arg) =>
        {
            return new Vector2(
                       arg.x - center.x,
                       arg.y - center.y
                   );
        }).ToList<Vector2>();
        return retPositionList;
    }

}
}
