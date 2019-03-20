using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeefCreateModelScripts.Runtime
{
/// <summary>
/// Unity座標情報から 道の Mesh を作成する.
/// </summary>
public class CreateRoadMeshScripts
{
    /// <summary>
    /// メッシュ情報を 最大頂点数が 65535 を超えないようにまとめる.
    /// </summary>
    /// <returns>The combined mesh.</returns>
    /// <param name="meshSetList">Mesh set list.</param>
    public static List<MeshSet> GetCombinedMesh(List<MeshSet> meshSetList, int maxVerticesCount = 60000)
    {
        var returnSet = new List<MeshSet>();
        var uniMeshs = new MeshSet();

        for (int i = 0; i < meshSetList.Count; i++)
        {
            if (uniMeshs.VerticesList.Count > maxVerticesCount)
            {
                returnSet.Add(uniMeshs);
                uniMeshs = new MeshSet();
            }

            int prev = uniMeshs.VerticesList.Count;
            MeshSet meshSet = meshSetList[i];
            uniMeshs.VerticesList.AddRange(meshSet.VerticesList);
            uniMeshs.UvList.AddRange(meshSet.UvList);
            var addTriangleList = meshSet.TrianglesList.Select((int index) =>
            {
                return index + prev;
            });
            uniMeshs.TrianglesList.AddRange(addTriangleList);
        }

        returnSet.Add(uniMeshs);
        return returnSet;
    }




    /// <summary>
    /// 受け取った MeshSet をリストに追加するコールバックを作成する。
    /// </summary>
    /// <returns>受け取った MeshSet をリストに追加するコールバック.</returns>
    /// <param name="meshSetList">追加先のリスト.</param>
    public static Action<MeshSet> CreateAddToMeshSetListCallback(List<MeshSet> meshSetList)
    {
        return (MeshSet meshset) =>
        {
            meshSetList.Add(meshset);
        };
    }

    public static Vector2 GetVector2(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }

    /// <summary>
    /// Ises the cross up.
    /// </summary>
    /// <returns><c>true</c>, if cross up was ised, <c>false</c> otherwise.</returns>
    /// <param name="sv">メッシュ開始頂点.</param>
    /// <param name="v1">頂点1.</param>
    /// <param name="v2">頂点2.</param>
    public static bool IsCrossUp(Vector3 sv, Vector3 v1, Vector3 v2, Vector3 direction)
    {
        var crossDirection = Vector3.Cross(v1 - sv, v2 - sv).normalized;
        var crossAngle = Vector3.Angle(crossDirection, direction);
        return crossAngle < 90.0f;
    }

    /// <summary>
    /// 上方向がテクスチャになるように頂点順番を設定する。
    /// </summary>
    /// <param name="verticesList">Vertices list.</param>
    /// <param name="trianglesList">Triangles list.</param>
    /// <param name="p0">P0.</param>
    /// <param name="p1">P1.</param>
    /// <param name="p2">P2.</param>
    public static void AddTrianglesList(List<Vector3> verticesList, List<int> trianglesList, int p0, int p1, int p2, Vector3 direction)
    {
        var crossUp = IsCrossUp(verticesList[p0], verticesList[p1], verticesList[p2], direction);

        if (crossUp)
        {
            trianglesList.Add(p0);
            trianglesList.Add(p1);
            trianglesList.Add(p2);
        }
        else
        {
            trianglesList.Add(p0);
            trianglesList.Add(p2);
            trianglesList.Add(p1);
        }
    }

    public class PrevNext
    {
        public Vector3 Prev;
        public Vector3 Next;
    }

    /// <summary>
    /// メッシュ生成に利用できる情報
    /// </summary>
    public class MeshSet
    {
        public List<Vector3> VerticesList = new List<Vector3>();
        public List<int> TrianglesList = new List<int>();
        public List<Vector2> UvList = new List<Vector2>();

        public static Mesh CreateMesh(MeshSet meshSet)
        {
            var mesh = new Mesh();
            mesh.SetVertices(meshSet.VerticesList);
            mesh.SetTriangles(meshSet.TrianglesList, 0);
            mesh.SetUVs(0, meshSet.UvList);
            mesh.RecalculateNormals();
            return mesh;
        }

        public Mesh CreateMesh()
        {
            return CreateMesh(this);
        }
    }

    public static float CurveMeter = 6f;
    public static float CurveLeap = 0.3f;

    public static Vector3 Vector3LeapWithMeter(Vector3 v1, Vector3 v2, float meter)
    {
        if (Vector3.Distance(v1, v2) < meter)
        {
            return v1;
        }

        var v = v2 - v1;
        return v1 + v.normalized * meter;
    }

    /// <summary>
    /// Creates the curve.
    /// </summary>
    /// <param name="positionList">Position list.</param>
    /// <param name="action">メッシュ生成に必要な情報を受け取るコールバック.</param>
    public static void CreateArea(List<Vector3> positionList, Action<MeshSet> action)
    {
        var positionSetList = new Utility.PositionSetList();

        for (int i = 0; i < positionList.Count; i++)
        {
            int prevIndex = i - 1;
            int nextIndex = i + 1;

            if (i == 0)
            {
                prevIndex = positionList.Count - 1;
            }

            if (i == positionList.Count - 1)
            {
                nextIndex = 0;
            }

            positionSetList.Add(new Utility.PositionSet
            {
                position = positionList[i],
                Index = i,
                PrevIndex = prevIndex,
                NextIndex = nextIndex,
            });
        }

        Utility.SeparateAreaSet(positionSetList, action);
    }

    /// <summary>
    /// Creates the curve.
    /// </summary>
    /// <param name="positionList">Position list.</param>
    /// <param name="width">道の太さ.</param>
    /// <param name="action">メッシュ生成に必要な情報を受け取るコールバック.</param>
    public static void CreateCurve(List<Vector3> positionList, float width, Action<MeshSet> action)
    {
        var prevNextList = new List<PrevNext>();

        for (int i = 0; i < positionList.Count; i++)
        {
            PrevNext prevNext = new PrevNext();

            if (i != 0)
            {
                prevNext.Prev = Vector3LeapWithMeter(positionList[i], positionList[i - 1], CurveMeter);
            }

            if (i != positionList.Count - 1)
            {
                prevNext.Next = Vector3LeapWithMeter(positionList[i], positionList[i + 1], CurveMeter);
            }

            if (i == 0)
            {
                prevNext.Prev = positionList[i];
                prevNext.Next = positionList[i];
            }

            if (i == positionList.Count - 1)
            {
                prevNext.Prev = positionList[i];
                prevNext.Next = positionList[i];
            }

            prevNextList.Add(prevNext);
        }

        for (int i = 0; i < positionList.Count; i++)
        {
            if (i != 0)
            {
                CreateStraightRoad(prevNextList[i - 1].Next, prevNextList[i].Prev, width, action);
                CreateCurveRoad(positionList[i], prevNextList[i].Next, prevNextList[i].Prev, width, action);
            }
        }
    }

    public static void CreateCurveRoad(Vector3 sv, Vector3 v1, Vector3 v2, float width, Action<MeshSet> action)
    {
        v1 = v1 + Vector3.up * 0.01f;
        v2 = v2 + Vector3.up * 0.01f;
        var verticesList = new List<Vector3>();
        var trianglesList = new List<int>();
        var v1roadWidthVector = (GetVector2(v1 - sv)).normalized * width * 0.5f;
        var v2roadWidthVector = (GetVector2(v2 - sv)).normalized * width * 0.5f;
        {
            var first = new Vector3(
                v1.x - v1roadWidthVector.y,
                v1.y,
                v1.z + v1roadWidthVector.x
            );
            var second = new Vector3(
                v1.x + v1roadWidthVector.y,
                v1.y,
                v1.z - v1roadWidthVector.x
            );
            verticesList.Add(first);
            verticesList.Add(second);
        }
        {
            var first = new Vector3(
                v2.x - v2roadWidthVector.y,
                v2.y,
                v2.z + v2roadWidthVector.x
            );
            var second = new Vector3(
                v2.x + v2roadWidthVector.y,
                v2.y,
                v2.z - v2roadWidthVector.x
            );
            verticesList.Add(first);
            verticesList.Add(second);
        }
        AddTrianglesList(verticesList, trianglesList, 0, 1, 2, Vector3.up);
        AddTrianglesList(verticesList, trianglesList, 2, 3, 0, Vector3.up);
        var uvList = new List<Vector2>();
        uvList.Add(new Vector2(0, 0));
        uvList.Add(new Vector2(1, 0));
        uvList.Add(new Vector2(1, 1));
        uvList.Add(new Vector2(0, 1));
        CreateMeshSet(verticesList, trianglesList, uvList, action);
    }

    public static int TileMeter = 6;

    public static void CreateStraightRoad(Vector3 v1, Vector3 v2, float width, Action<MeshSet> action)
    {
        var verticesList = new List<Vector3>();
        var trianglesList = new List<int>();
        var uvList = new List<Vector2>();
        var roadDirectionVector = (v2 - v1).normalized;
        var roadWidthVector = GetVector2(roadDirectionVector).normalized * width * 0.5f;
        var distance = Vector3.Distance(v2, v1);
        int max = ((int)distance / TileMeter);
        max = Math.Max(max, 1);
        var leftdown = new Vector2(1f / 8, 0f);
        var rightup = new Vector2(1f - 1f / 8, 1f);

        for (int i = 0; i < max; i++)
        {
            int index = verticesList.Count;
            {
                {
                    var leap = (float)i / max;
                    var v0 = Vector3.Lerp(v1, v2, leap);
                    var first = new Vector3(
                        v0.x - roadWidthVector.y,
                        v0.y,
                        v0.z + roadWidthVector.x
                    );
                    var second = new Vector3(
                        v0.x + roadWidthVector.y,
                        v0.y,
                        v0.z - roadWidthVector.x
                    );
                    verticesList.Add(first);
                    verticesList.Add(second);
                }
                {
                    var leap = (float)(i + 1) / max;
                    var v0 = Vector3.Lerp(v1, v2, leap);
                    var first = new Vector3(
                        v0.x - roadWidthVector.y,
                        v0.y,
                        v0.z + roadWidthVector.x
                    );
                    var second = new Vector3(
                        v0.x + roadWidthVector.y,
                        v0.y,
                        v0.z - roadWidthVector.x
                    );
                    verticesList.Add(first);
                    verticesList.Add(second);
                }
                AddTrianglesList(verticesList, trianglesList, index + 0, index + 1, index + 2, Vector3.up);
                AddTrianglesList(verticesList, trianglesList, index + 1, index + 2, index + 3, Vector3.up);
                uvList.Add(new Vector2(leftdown.x, leftdown.y));
                uvList.Add(new Vector2(rightup.x, leftdown.y));
                uvList.Add(new Vector2(leftdown.x, rightup.y));
                uvList.Add(new Vector2(rightup.x, rightup.y));
            }
            {
                {
                    verticesList.Add(verticesList[index + 0]);
                    verticesList.Add(verticesList[index + 2]);
                    verticesList.Add(verticesList[index + 0] + Vector3.down);
                    verticesList.Add(verticesList[index + 2] + Vector3.down);
                    uvList.Add(new Vector2(0, leftdown.y));
                    uvList.Add(new Vector2(0, rightup.y));
                    uvList.Add(new Vector2(leftdown.x, leftdown.y));
                    uvList.Add(new Vector2(leftdown.y, rightup.y));
                    var nextIndex = index + 4;
                    var left = Quaternion.AngleAxis(-90, Vector3.up) * roadDirectionVector;
                    AddTrianglesList(verticesList, trianglesList, nextIndex + 0, nextIndex + 1, nextIndex + 2, left);
                    AddTrianglesList(verticesList, trianglesList, nextIndex + 1, nextIndex + 2, nextIndex + 3, left);
                }
                {
                    verticesList.Add(verticesList[index + 1]);
                    verticesList.Add(verticesList[index + 3]);
                    verticesList.Add(verticesList[index + 1] + Vector3.down);
                    verticesList.Add(verticesList[index + 3] + Vector3.down);
                    uvList.Add(new Vector2(rightup.x, leftdown.y));
                    uvList.Add(new Vector2(rightup.x, rightup.y));
                    uvList.Add(new Vector2(1, leftdown.y));
                    uvList.Add(new Vector2(1, rightup.y));
                    var nextIndex = index + 4 + 4;
                    var right = Quaternion.AngleAxis(90, Vector3.up) * roadDirectionVector;
                    AddTrianglesList(verticesList, trianglesList, nextIndex + 0, nextIndex + 1, nextIndex + 2, right);
                    AddTrianglesList(verticesList, trianglesList, nextIndex + 1, nextIndex + 2, nextIndex + 3, right);
                }
            }
        }

        CreateMeshSet(verticesList, trianglesList, uvList, action);
    }



    public static void CreateMeshSet(List<Vector3> verticesList, List<int> trianglesList, List<Vector2> uvList, Action<MeshSet> action)
    {
        action(new MeshSet
        {
            VerticesList = verticesList,
            TrianglesList = trianglesList,
            UvList = uvList
        });
    }
}
}
