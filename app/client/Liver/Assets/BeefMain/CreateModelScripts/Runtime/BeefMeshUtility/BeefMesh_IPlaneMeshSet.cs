using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeefCreateModelScripts.Runtime.BeefMesh
{
public partial class BeefMeshUtility
{
    static System.Random random = new System.Random();
    public static float GetYoffset(int no)
    {
        var yOffset =
            (float)(random.NextDouble() * 0.005f) +
            //UnityEngine.Random.Range(0.0f, 0.005f) +
            0.005f * no;
        return yOffset;
    }

    public static FrameMeshSet GetFrameMeshSet(List<IPlaneMeshSet> planeMeshSets)
    {
        var positionList = new List<Vector3>();

        foreach (var obj in planeMeshSets)
        {
            positionList.AddRange(obj.GetPositionList());
        }

        var framePointsList = GetFrameScript.GetFramePoints(positionList.ConvertAll(GetVector2)).ToList();
        var yBuf = GetYoffset(positionList.Count);
        positionList = framePointsList.ConvertAll((input) => new Vector3(input.x, positionList[0].y + yBuf, input.y));

        if (positionList.Count < 3)
        {
            return null;
        }

        return new FrameMeshSet(positionList);
    }


    public interface IPlaneMeshSet
    {
        List<Vector3> GetPositionList();
        CreateRoadMeshScripts.MeshSet GetMeshSet();
    }

    public class RoadRectMeshSet : IPlaneMeshSet
    {
        public List<Vector3> positionList = new List<Vector3>();
        public Vector3 direction;
        public float num = 1;

        public List<Vector3> GetPositionList()
        {
            return positionList;
        }

        public CreateRoadMeshScripts.MeshSet GetMeshSet()
        {
            var yBuf = GetYoffset(0);
            var verticesList = new List<Vector3>();
            verticesList.Add(positionList[0] + Vector3.up * yBuf);
            verticesList.Add(positionList[1] + Vector3.up * yBuf);
            verticesList.Add(positionList[2] + Vector3.up * yBuf);
            verticesList.Add(positionList[3] + Vector3.up * yBuf);
            var trianglesList = new List<int>();
            var index = 0;
            CreateRoadMeshScripts.AddTrianglesList(verticesList, trianglesList, index + 0, index + 1, index + 2, Vector3.up);
            CreateRoadMeshScripts.AddTrianglesList(verticesList, trianglesList, index + 1, index + 2, index + 3, Vector3.up);
            var uvList = new List<Vector2>();
            var leftdown = new Vector2(1f / 8, 0f);
            var rightup = new Vector2(1f - 1f / 8, 1f * num);
            uvList.Add(new Vector2(leftdown.x, leftdown.y));
            uvList.Add(new Vector2(rightup.x, leftdown.y));
            uvList.Add(new Vector2(leftdown.x, rightup.y));
            uvList.Add(new Vector2(rightup.x, rightup.y));
            var meshSet = new CreateRoadMeshScripts.MeshSet();
            meshSet.VerticesList = verticesList;
            meshSet.TrianglesList = trianglesList;
            meshSet.UvList = uvList;
            {
                {
                    verticesList.Add(verticesList[index + 0] + Vector3.down);
                    verticesList.Add(verticesList[index + 2] + Vector3.down);
                    uvList.Add(new Vector2(0, leftdown.y));
                    uvList.Add(new Vector2(0, rightup.y));
                    var nextIndex = index + 4;
                    var left = Quaternion.AngleAxis(-90, Vector3.up) * direction;
                    CreateRoadMeshScripts.AddTrianglesList(verticesList, trianglesList, index + 0, index + 2, nextIndex + 0, left);
                    CreateRoadMeshScripts.AddTrianglesList(verticesList, trianglesList, index + 2, nextIndex + 0, nextIndex + 1, left);
                }
                {
                    verticesList.Add(verticesList[index + 1] + Vector3.down);
                    verticesList.Add(verticesList[index + 3] + Vector3.down);
                    uvList.Add(new Vector2(1, leftdown.y));
                    uvList.Add(new Vector2(1, rightup.y));
                    var nextIndex = index + 4 + 2;
                    var right = Quaternion.AngleAxis(90, Vector3.up) * direction;
                    CreateRoadMeshScripts.AddTrianglesList(verticesList, trianglesList, index + 1, index + 3, nextIndex + 0, right);
                    CreateRoadMeshScripts.AddTrianglesList(verticesList, trianglesList, index + 3, nextIndex + 0, nextIndex + 1, right);
                }
            }
            return meshSet;
        }
    }

    public class FrameMeshSet : IPlaneMeshSet
    {
        public List<Vector3> positionList = new List<Vector3>();

        public FrameMeshSet(List<Vector3> positionList)
        {
            this.positionList = positionList;
        }

        public List<Vector3> GetPositionList()
        {
            return positionList;
        }

        public CreateRoadMeshScripts.MeshSet GetMeshSet()
        {
            var meshSet = new List<CreateRoadMeshScripts.MeshSet>();
            CreateRoadMeshScripts.CreateArea(positionList, (obj) => meshSet.Add(obj));
            var unitMeshSet = CreateRoadMeshScripts.GetCombinedMesh(meshSet)[0];
            unitMeshSet = BeefMain.Runtime.CreateNodeRoad.ConvertUvSetting(unitMeshSet);
            var addDesign = new List<CreateRoadMeshScripts.MeshSet>();
            addDesign.Add(unitMeshSet);
            addDesign.Add(CreateDesign(positionList));
            return CreateRoadMeshScripts.GetCombinedMesh(addDesign)[0];
        }

        private static CreateRoadMeshScripts.MeshSet CreateDesign(List<Vector3> positionList)
        {
            int loop = CalcLoopwise(positionList.ConvertAll(GetVector2));
            var verticesList = new List<Vector3>();
            var trianglesList = new List<int>();
            var uvList = new List<Vector2>();
            var leftdown = new Vector2(1f / 8, 0f);
            var rightup = new Vector2(1f - 1f / 8, 1f);
            var triangleSet = new Action<bool, int, int, List<int>>(delegate(bool clock, int inNowIndex, int inNextIndex, List<int> targetTrianglesList)
            {
                if (!clock)
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

            for (int i = 0; i < positionList.Count; i++)
            {
                {
                    verticesList.Add(positionList[i]);
                    verticesList.Add(positionList[i] + Vector3.down);
                }
                {
                    int nowIndex = i * 2;
                    int next = ((i + 1) != positionList.Count) ? (i * 2 + 2) : 0;
                    int nextIndex = next;
                    triangleSet(loop == -1, nowIndex, nextIndex, trianglesList);
                }

                if (i % 2 == 0)
                {
                    uvList.Add(new Vector2(leftdown.x, leftdown.y));
                    uvList.Add(new Vector2(leftdown.x, 0));
                }
                else
                {
                    uvList.Add(new Vector2(rightup.x, leftdown.y));
                    uvList.Add(new Vector2(rightup.x, 0));
                }
            }

            return new CreateRoadMeshScripts.MeshSet
            {
                VerticesList = verticesList,
                TrianglesList = trianglesList,
                UvList = uvList
            };
        }
    }

    public static Vector2 GetVector2(Vector3 vector3)
    {
        return new Vector2(vector3.x, vector3.z);
    }

    public static int TileMeter = 6;

    public static List<IPlaneMeshSet> CreateStraightRoad(Vector3 v1, Vector3 v2, float width)
    {
        var planeMeshSets = new List<IPlaneMeshSet>();
        var roadDirectionVector = (v2 - v1).normalized;
        var roadWidthVector = GetVector2(roadDirectionVector).normalized * width * 0.5f;
        var distance = Vector3.Distance(v2, v1);
        int max = ((int)distance / TileMeter);
        max = Math.Max(max, 1);
        var leftdown = new Vector2(1f / 8, 0f);
        var rightup = new Vector2(1f - 1f / 8, 1f);

        for (int i = 0; i < max; i++)
        {
            var planeMesh = new RoadRectMeshSet();
            planeMesh.direction = roadDirectionVector;
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
                planeMesh.positionList.Add(first);
                planeMesh.positionList.Add(second);
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
                planeMesh.positionList.Add(first);
                planeMesh.positionList.Add(second);
            }
            planeMeshSets.Add(planeMesh);
        }

        if (max >= 4)
        {
            var ps = planeMeshSets[0] as RoadRectMeshSet;
            var pe = planeMeshSets[max - 1] as RoadRectMeshSet;
            var p1 = planeMeshSets[1] as RoadRectMeshSet;
            var pn = planeMeshSets[max - 2] as RoadRectMeshSet;
            p1.positionList[2] = pn.positionList[2];
            p1.positionList[3] = pn.positionList[3];
            p1.num = max - 2;
            planeMeshSets.Clear();
            planeMeshSets.Add(ps);
            planeMeshSets.Add(p1);
            planeMeshSets.Add(pe);
        }

        return planeMeshSets;
    }
}

}
