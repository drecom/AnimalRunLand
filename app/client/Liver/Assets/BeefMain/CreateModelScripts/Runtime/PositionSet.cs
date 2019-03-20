using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace BeefCreateModelScripts.Runtime
{
public class Utility
{
    public class PositionSet
    {
        public Vector3 position;
        public int Index;
        public int PrevIndex;
        public int NextIndex;
    }

    public class PositionSetList : List<PositionSet>
    {
        private List<PositionSet> copyList = null;


        public List<PositionSet> GetSortedList()
        {
            if (copyList == null)
            {
                copyList = new List<PositionSet>(this);
                copyList.Sort((PositionSet x, PositionSet y) =>
                {
                    if (x.position.x < y.position.x)
                    {
                        return 1;
                    }

                    if (x.position.x == y.position.x)
                    {
                        return 0;
                    }

                    return -1;
                });
            }

            return copyList;
        }

        public PositionSet FindByIndex(int index)
        {
            return this.Find((PositionSet obj) =>
            {
                return obj.Index == index;
            });
        }

        public void RemoveSortedListFirst()
        {
            this.Remove(this.FindByIndex(copyList[0].Index));
            this.FindByIndex(copyList[0].NextIndex).PrevIndex = copyList[0].PrevIndex;
            this.FindByIndex(copyList[0].PrevIndex).NextIndex = copyList[0].NextIndex;
            copyList.Remove(copyList[0]);
        }

        public bool IsRight(int index)
        {
            var centerPosition = this.FindByIndex(index).position;
            var nextPosition = this.FindByIndex(this.FindByIndex(index).NextIndex).position;
            var prevPosition = this.FindByIndex(this.FindByIndex(index).PrevIndex).position;
            var isRight =
                ((nextPosition.x - centerPosition.x) < 0) &&
                ((prevPosition.x - centerPosition.x) < 0);
            return isRight;
        }
    }

    public static bool LineInnerIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        var result = LineLineIntersectionForPlane(out intersection, linePoint1, lineVec1, linePoint2, lineVec2);

        if (result)
        {
            if (
                Vector3.Angle(intersection - linePoint1, lineVec1) < 0.1f &&
                (intersection - linePoint1).magnitude < lineVec1.magnitude)
            {
                return result;
            }
            else
            {
                intersection = Vector3.zero;
                return false;
            }
        }

        return false;
    }

    public static bool LineLineIntersectionForPlane(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        linePoint1.y = 0.0f;
        linePoint2.y = 0.0f;
        lineVec1.y = 0.0f;
        lineVec2.y = 0.0f;
        return LineLineIntersection(out intersection, linePoint1, lineVec1, linePoint2, lineVec2);
    }

    //Calculate the intersection point of two lines. Returns true if lines intersect, otherwise false.
    //Note that in 3d, two lines do not intersect most of the time. So if the two lines are not in the
    //same plane, use ClosestPointsOnTwoLines() instead.
    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);
        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }

    /// <summary>
    /// Creates the curve.
    /// </summary>
    /// <param name="positionList">Position list.</param>
    /// <param name="action">メッシュ生成に必要な情報を受け取るコールバック.</param>
    public static void CreateArea(List<Vector3> positionList, Action<CreateRoadMeshScripts.MeshSet> action)
    {
        var positionSetList = new PositionSetList();

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

            positionSetList.Add(new PositionSet
            {
                position = positionList[i],
                Index = i,
                PrevIndex = prevIndex,
                NextIndex = nextIndex,
            });
        }

        SeparateAreaSet(positionSetList, action);
    }

    private static Vector2Int CreateSet(int x, int y)
    {
        if (x < y)
        {
            return new Vector2Int(x, y);
        }

        return new Vector2Int(y, x);
    }

    public static void SeparateAreaSet(PositionSetList areaSet, Action<CreateRoadMeshScripts.MeshSet> action)
    {
        // どちらも同じ側にあったとき、
        // 今までに見つかっている頂点・辺との交点を探す
        // 探して見つからなければそいつは凸の頂点
        // 探して見つかればそいつは凹の頂点。
        // 交点で2つに分割する。
        // 未知2つを再帰で繰り返していく。
        var sortedList = areaSet.GetSortedList();

        for (int i = 0; i < sortedList.Count; i++)
        {
            var index = sortedList[i].Index;

            if (!areaSet.IsRight(index))
            {
                continue;
            }

            // 内凸を調べる。
            var sideList = new List<Vector2Int>();

            for (int j = 0; j < i; j++)
            {
                var nextSide = CreateSet(sortedList[j].Index, sortedList[j].NextIndex);
                var prevSide = CreateSet(sortedList[j].Index, sortedList[j].PrevIndex);
                sideList.Add(nextSide);
                sideList.Add(prevSide);
            }

            sideList = sideList.Distinct().ToList();
            var collisionCount = 0;
            var collisionNearlyPoint = Vector3.zero;
            var collisionNearlyPointIndex = Vector2Int.zero;

            foreach (var vector2Int in sideList)
            {
                Vector3 collisionPoint;
                var point1 = areaSet[vector2Int.x].position;
                var vec1 = areaSet[vector2Int.y].position - point1;
                var point2 = areaSet[index].position;
                var vec2 = point2 - areaSet[areaSet[index].NextIndex].position;
                bool isCollision = LineInnerIntersection(out collisionPoint, point1, vec1, point2, vec2);

                if (isCollision)
                {
                    collisionCount++;

                    if (Vector3.Distance(collisionPoint, point2) < Vector3.Distance(collisionNearlyPoint, point2))
                    {
                        collisionNearlyPoint = collisionPoint;
                        collisionNearlyPointIndex = vector2Int;
                    }
                }
            }

            if (collisionCount % 2 == 1)
            {
                // 分割する.
                var newList1 = new List<Vector3>();
                var newList2 = new List<Vector3>();

                if (areaSet[collisionNearlyPointIndex.x].NextIndex != collisionNearlyPointIndex.y)
                {
                    var t = collisionNearlyPointIndex.x;
                    collisionNearlyPointIndex.x = collisionNearlyPointIndex.y;
                    collisionNearlyPointIndex.y = t;
                }

                {
                    {
                        newList1.Add(areaSet[collisionNearlyPointIndex.x].position);
                        newList1.Add(collisionNearlyPoint);
                        newList1.Add(areaSet[index].position);
                        var nextIndex = areaSet[index].NextIndex;

                        while (nextIndex != collisionNearlyPointIndex.x)
                        {
                            newList1.Add(areaSet[nextIndex].position);
                            nextIndex = areaSet[nextIndex].NextIndex;
                        }
                    }
                    {
                        newList2.Add(areaSet[collisionNearlyPointIndex.y].position);
                        newList2.Add(collisionNearlyPoint);
                        newList2.Add(areaSet[index].position);
                        var prevIndex = areaSet[index].PrevIndex;

                        while (prevIndex != collisionNearlyPointIndex.y)
                        {
                            newList2.Add(areaSet[prevIndex].position);
                            prevIndex = areaSet[prevIndex].NextIndex;
                        }
                    }
                }

                CreateArea(newList1, action);
                CreateArea(newList2, action);
                return;
            }
        }

        CreateAreaMesh(areaSet, action);
    }
    public static void CreateAreaMesh(Utility.PositionSetList areaSet, Action<CreateRoadMeshScripts.MeshSet> action)
    {
        var verticesList = new List<Vector3>();
        var trianglesList = new List<int>();
        var uvList = new List<Vector2>();

        while (areaSet.Count != 2)
        {
            var index = verticesList.Count;
            // 最東を選ぶ
            var tri = areaSet.GetSortedList()[0];
            var next = areaSet.FindByIndex(tri.NextIndex);
            var prev = areaSet.FindByIndex(tri.PrevIndex);
            // 最東から内側のメッシュ生成する
            verticesList.Add(tri.position);
            verticesList.Add(next.position);
            verticesList.Add(prev.position);
            CreateRoadMeshScripts.AddTrianglesList(verticesList, trianglesList, index + 0, index + 1, index + 2, Vector3.up);
            uvList.Add(new Vector2(0, 0));
            uvList.Add(new Vector2(1, 0));
            uvList.Add(new Vector2(1, 1));
            // 選んだ頂点を減らす。
            areaSet.RemoveSortedListFirst();
        }

        CreateRoadMeshScripts.CreateMeshSet(verticesList, trianglesList, uvList, action);
    }


}
}
