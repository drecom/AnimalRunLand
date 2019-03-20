
﻿#if UNUSED_CreateRoadScripts
using System.Collections;

using System.Collections.Generic;
using eWolfRoadBuilder;

namespace BeefCreateModelScripts.Runtime
{
using System;
using UnityEngine;
using System.Linq;
using UnityEngine;

public static class CreateRoadScripts
{
public static IEnumerator CreateRoadAsync(List<RoadData> roadDataList, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        var result = parent.transform.Find("RoadRoot");
        GameObject root;

        if (result == null)
        {
            root = new GameObject("RoadRoot");
        }
        else
        {
            root = result.gameObject;
        }

        root.transform.parent = parent.transform;
        yield return new WaitForEndOfFrame();
        CreateRoadPrefab2(roadDataList, root, worldCenter, worldScale);
    }

    class RoadBuilderNode
    {
        public string Id = string.Empty;
        public Vector2 Position = Vector2.zero;
        public List<int> Links = new List<int>();
        public int Ref = -1;
    }

    public static void CreateRoad(List<RoadData> roadDataList, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        var result = parent.transform.Find("RoadRoot");
        GameObject root;

        if (result == null)
        {
            root = new GameObject("RoadRoot");
        }
        else
        {
            root = result.gameObject;
        }

        root.transform.parent = parent.transform;
        //        foreach (var roadData in buildingDataSet.RoadDatas)
        //        {
        //            CreateRoadPrefab(roadData, root, worldCenter, worldScale);
        //            //            CreateRoadMesh(roadData, root, worldCenter, worldScale);
        //        }
        CreateRoadPrefab3(roadDataList, root, worldCenter, worldScale);
    }

    private static void CreateRoadPrefab3(List<RoadData> roadDataList, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        var mainPrefab = Resources.Load<GameObject>("RoadNetwork_Grass_pf");
        string objectId = "RoadEdge" + Guid.NewGuid().ToString();
        var mainObject = UnityEngine.Object.Instantiate(mainPrefab, Vector3.zero, Quaternion.identity);
        mainObject.name = objectId;
        var roadBuilderNodes = new List<RoadBuilderNode>();

        foreach (var roadData in roadDataList)
        {
            if (roadData.Info.ContainsKey("natural"))
            {
                continue;
            }

            if (roadData.Info.ContainsKey("waterway"))
            {
                continue;
            }

            if (roadData.Info.ContainsKey("highway"))
            {
                if (roadData.Info["highway"].Equals("footway"))
                {
                    if (!roadData.Info.ContainsKey("width"))
                    {
                        // 歩道で 幅設定がない場合は無視.
                        continue;
                    }
                }
            }
            else
            {
                continue;
            }

            var positions = roadData.ExteriorPositions.Select(pos =>
            {
                var lat = (pos.NorthLat - worldCenter.y) * worldScale.y;
                var lon = (pos.EastLon - worldCenter.x) * worldScale.x;
                return new Vector2((float)lon, (float)lat);
            }).ToArray();

            for (int i = 0; i < positions.Length; i++)
            {
                var roadBuilderNode = new RoadBuilderNode()
                {
                    Id = string.Format("RoadNode_{0}_{1}_{2}", roadData.Id, i, Guid.NewGuid().ToString()),
                    Position = positions[i]
                };

                if (i != 0)
                {
                    roadBuilderNode.Links.Add(roadBuilderNodes.Count - 1);
                }

                if (i != positions.Length - 1)
                {
                    roadBuilderNode.Links.Add(roadBuilderNodes.Count + 1);
                }

                roadBuilderNodes.Add(roadBuilderNode);
            }
        }

        for (int i = 0; i < roadBuilderNodes.Count; i++)
        {
            var target = roadBuilderNodes[i];
            var findIndex = roadBuilderNodes.FindIndex(o =>
            {
                if (o.Ref != -1)
                {
                    return false;
                }

                if (o.Position == target.Position)
                {
                    return true;
                }

                return false;
            });

            if (findIndex < i)
            {
                roadBuilderNodes[findIndex].Links.AddRange(target.Links);
                target.Ref = findIndex;
            }
        }

        // Add
        var roadNetworkNodes = new Dictionary<string, RoadNetworkNode>();
        var data = roadBuilderNodes.Where(o => o.Ref == -1).ToList();

        for (int i = 0; i < data.Count; i++)
        {
            GameObject child = new GameObject(data[i].Id);
            child.transform.position = new Vector3(
                data[i].Position.x,
                data[i].Position.x * 0.00001f +
                data[i].Position.y * 0.00001f +
                0.1f,
                data[i].Position.y
            );
            child.transform.parent = mainObject.transform;
            var roadNetworkNode = child.AddComponent<RoadNetworkNode>();
            roadNetworkNodes.Add(data[i].Id, roadNetworkNode);
        }

        // セット
        foreach (var key in roadNetworkNodes.Keys)
        {
            var roadNetworkNode = roadNetworkNodes[key];
            var d = data.Find(o => o.Id.Equals(key));
            roadNetworkNode.Details.Roads = new List<RoadNetworkNode>();

            foreach (var o in d.Links)
            {
                var a = roadBuilderNodes[o];

                if (a.Ref == -1)
                {
                    roadNetworkNode.Details.Roads.Add(roadNetworkNodes[a.Id]);
                }
                else
                {
                    var b = roadBuilderNodes[a.Ref];
                    roadNetworkNode.Details.Roads.Add(roadNetworkNodes[b.Id]);
                }
            }
        }

        foreach (var key in roadNetworkNodes.Keys)
        {
            var roadNetworkNode = roadNetworkNodes[key];
            var d = data.Find(o => o.Id.Equals(key));
            roadNetworkNode.Details.Roads = new List<RoadNetworkNode>();

            foreach (var o in d.Links)
            {
                var a = roadBuilderNodes[o];

                if (a.Ref == -1)
                {
                    roadNetworkNode.Details.Roads.Add(roadNetworkNodes[a.Id]);
                }
                else
                {
                    var b = roadBuilderNodes[a.Ref];
                    roadNetworkNode.Details.Roads.Add(roadNetworkNodes[b.Id]);
                }
            }
        }

        {
            var roadBuilder = mainObject.GetComponent<RoadBuilder>();
            roadBuilder.CrossSectionDetails.RoadWidth = 8.0f;
            roadBuilder.CrossSectionDetails.WithCurb = false;
            roadBuilder.CreateCollision = false;
            roadBuilder.MeshPerNode = true;
            roadBuilder.DropToGround = false;
        }

        mainObject.transform.position += Vector3.up * 1.0f;
        mainObject.transform.parent = parent.transform;
    }

    private static void CreateRoadPrefab2(List<RoadData> roadDataList, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        var roadBuilderNodes = new List<RoadBuilderNode>();

        foreach (var roadData in roadDataList)
        {
            if (roadData.Info.ContainsKey("natural"))
            {
                continue;
            }

            if (roadData.Info.ContainsKey("waterway"))
            {
                continue;
            }

            if (roadData.Info.ContainsKey("highway"))
            {
                if (roadData.Info["highway"].Equals("footway"))
                {
                    if (!roadData.Info.ContainsKey("width"))
                    {
                        // 歩道で 幅設定がない場合は無視.
                        continue;
                    }
                }
            }
            else
            {
                continue;
            }

            var positions = roadData.ExteriorPositions.Select(pos =>
            {
                var lat = (pos.NorthLat - worldCenter.y) * worldScale.y;
                var lon = (pos.EastLon - worldCenter.x) * worldScale.x;
                return new Vector2((float)lon, (float)lat);
            }).ToArray();

            for (int i = 0; i < positions.Length; i++)
            {
                var roadBuilderNode = new RoadBuilderNode()
                {
                    Id = string.Format("RoadNode_{0}_{1}", roadData.Id, i),
                    Position = positions[i]
                };

                if (i != 0)
                {
                    roadBuilderNode.Links.Add(roadBuilderNodes.Count - 1);
                }

                if (i != positions.Length - 1)
                {
                    roadBuilderNode.Links.Add(roadBuilderNodes.Count + 1);
                }

                roadBuilderNodes.Add(roadBuilderNode);
            }
        }

        for (int i = 0; i < roadBuilderNodes.Count; i++)
        {
            var target = roadBuilderNodes[i];
            //            if (2 < target.Links.Count)
            //            {
            //                continue;
            //            }
            var findIndex = roadBuilderNodes.FindIndex(o =>
            {
                if (o.Ref != -1)
                {
                    return false;
                }

                if (o.Position == target.Position)
                {
                    return true;
                }

                //                if (o.Links.Count < 3)
                //                {
                //                    return Vector2.Distance(o.Position, target.Position) < 5.0f;
                //                }
                return false;
            });

            if (findIndex < i)
            {
                roadBuilderNodes[findIndex].Links.AddRange(target.Links);
                target.Ref = findIndex;
            }
        }

        string objectId = "RoadEdge";
        var mainObject = new GameObject();
        mainObject.name = objectId;
        // Add
        var roadNetworkNodes = new Dictionary<string, RoadNetworkNode>();
        var data = roadBuilderNodes.Where(o => o.Ref == -1).ToList();

        for (int i = 0; i < data.Count; i++)
        {
            GameObject child = new GameObject(data[i].Id);
            child.transform.position = new Vector3(
                data[i].Position.x,
                data[i].Position.x * 0.00001f +
                data[i].Position.y * 0.00001f +
                0.1f,
                data[i].Position.y
            );
            child.transform.parent = mainObject.transform;
            var roadNetworkNode = child.AddComponent<RoadNetworkNode>();
            roadNetworkNodes.Add(data[i].Id, roadNetworkNode);
        }

        // セット
        foreach (var key in roadNetworkNodes.Keys)
        {
            var roadNetworkNode = roadNetworkNodes[key];
            var d = data.Find(o => o.Id.Equals(key));
            roadNetworkNode.Details.Roads = new List<RoadNetworkNode>();

            foreach (var o in d.Links)
            {
                var a = roadBuilderNodes[o];

                if (a.Ref == -1)
                {
                    roadNetworkNode.Details.Roads.Add(roadNetworkNodes[a.Id]);
                }
                else
                {
                    var b = roadBuilderNodes[a.Ref];
                    roadNetworkNode.Details.Roads.Add(roadNetworkNodes[b.Id]);
                }
            }
        }

        var mainPrefab = Resources.Load<GameObject>("RoadNetwork_Grass_pf");

        foreach (var key in roadNetworkNodes.Keys)
        {
            var node = roadNetworkNodes[key];

            if (2 < node.Details.Roads.Count)
            {
                var pfObject = UnityEngine.Object.Instantiate(mainPrefab, Vector3.zero, Quaternion.identity);
                pfObject.transform.parent = mainObject.transform;
                var roadBuilder = pfObject.GetComponent<RoadBuilder>();
                roadBuilder.CrossSectionDetails.RoadWidth = 8.0f;
                var roadData = roadDataList.Find(r =>
                {
                    return node.transform.name.Contains(r.Id);
                });
                roadBuilder.CrossSectionDetails.RoadWidth = GetWidth(roadData);
                roadBuilder.CrossSectionDetails.WithCurb = false;
                roadBuilder.CreateCollision = false;
                roadBuilder.MeshPerNode = false;
                roadBuilder.DropToGround = false;
                var centerNode = new GameObject();
                centerNode.name = node.transform.name;
                centerNode.transform.position = node.transform.position + Vector3.up * 0.01f;
                centerNode.transform.parent = pfObject.transform;
                var centerNodeRoadNetworkNode = centerNode.AddComponent<RoadNetworkNode>();

                for (int r = 0; r < node.Details.Roads.Count; r++)
                {
                    RoadNetworkNode innerRoad = node.Details.Roads[r];
                    var copyNode = new GameObject();
                    copyNode.name = innerRoad.name;
                    copyNode.transform.position = innerRoad.transform.position + Vector3.up * 0.01f;
                    copyNode.transform.parent = pfObject.transform;
                    var copyNodeRoadNetworkNode = copyNode.AddComponent<RoadNetworkNode>();
                    centerNodeRoadNetworkNode.Details.Roads.Add(copyNodeRoadNetworkNode);
                    copyNodeRoadNetworkNode.Details.Roads.Add(centerNodeRoadNetworkNode);
                }

                node.RemoveLinkFrom();
            }
        }

        foreach (var key in roadNetworkNodes.Keys)
        {
            var node = roadNetworkNodes[key];

            if (node.Details.Roads.Count == 0)
            {
                node.DeleteNode();
            }
            else
            {
                var originalCurveList = new List<RoadNetworkNode>();
                originalCurveList.Add(node);
                {
                    var listCount = -1;

                    while (listCount != originalCurveList.Count)
                    {
                        listCount = originalCurveList.Count;

                        foreach (var road in originalCurveList[0].Details.Roads)
                        {
                            if (originalCurveList.IndexOf(road) == -1)
                            {
                                originalCurveList.Insert(0, road);
                                break;
                            }
                        }

                        foreach (var road in originalCurveList[originalCurveList.Count - 1].Details.Roads)
                        {
                            if (originalCurveList.IndexOf(road) == -1)
                            {
                                originalCurveList.Add(road);
                                break;
                            }
                        }
                    }
                }
                var pfObject = UnityEngine.Object.Instantiate(mainPrefab, Vector3.zero, Quaternion.identity);
                pfObject.transform.parent = mainObject.transform;
                var roadBuilder = pfObject.GetComponent<RoadBuilder>();
                roadBuilder.CrossSectionDetails.RoadWidth = 10.0f;
                var roadData = roadDataList.Find(r =>
                {
                    return node.transform.name.Contains(r.Id);
                });
                roadBuilder.CrossSectionDetails.RoadWidth = GetWidth(roadData);
                roadBuilder.CrossSectionDetails.WithCurb = false;
                roadBuilder.CreateCollision = false;
                roadBuilder.MeshPerNode = false;
                roadBuilder.DropToGround = false;
                var copyCurveList = new List<RoadNetworkNode>();

                for (int i = 0; i < originalCurveList.Count; i++)
                {
                    var copyNode = new GameObject();
                    copyNode.name = originalCurveList[i].transform.name;
                    copyNode.transform.position = originalCurveList[i].transform.position;
                    copyNode.transform.parent = pfObject.transform;
                    var copyNodeRoadNetworkNode = copyNode.AddComponent<RoadNetworkNode>();
                    copyCurveList.Add(copyNodeRoadNetworkNode);

                    if (0 < i)
                    {
                        copyCurveList[i - 1].Details.Roads.Add(copyNodeRoadNetworkNode);
                        copyNodeRoadNetworkNode.Details.Roads.Add(copyCurveList[i - 1]);
                    }
                }

                for (int i = 0; i < originalCurveList.Count; i++)
                {
                    originalCurveList[i].RemoveLinkFrom();
                }
            }
        }

        mainObject.transform.position += Vector3.up * 1.0f;
        mainObject.transform.parent = parent.transform;
    }

    private static float GetWidth(RoadData roadData)
    {
        try
        {
            if (roadData.Info.ContainsKey("width"))
            {
                var widthText = roadData.Info["width"];

                // ['] が入っている場合は フィート
                if (widthText.Contains('\''))
                {
                    // 最初にマッチした数字列のみを利用する.
                    var mc = System.Text.RegularExpressions.Regex.Match(widthText, "\\d+");

                    if (mc.Success)
                    {
                        var feet = float.Parse(mc.Value);
                        return feet * 3.3f;
                    }
                }

                // [m] が入っている場合は メートル
                if (widthText.Contains('m'))
                {
                    // 最初にマッチした数字列のみを利用する.
                    var mc = System.Text.RegularExpressions.Regex.Match(widthText, "\\d+");

                    if (mc.Success)
                    {
                        return float.Parse(mc.Value);
                    }
                }

                // デフォルトは メートル
                {
                    // 最初にマッチした数字列のみを利用する.
                    var mc = System.Text.RegularExpressions.Regex.Match(widthText, "\\d+");

                    if (mc.Success)
                    {
                        return float.Parse(mc.Value);
                    }
                }
            }

            if (roadData.Info.ContainsKey("lanes"))
            {
                var lanesText = roadData.Info["lanes"];
                {
                    // 最初にマッチした数字列のみを利用する.
                    var mc = System.Text.RegularExpressions.Regex.Match(lanesText, "\\d+");

                    if (mc.Success)
                    {
                        var level = int.Parse(mc.Value);
                        return level * 8.0f;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return 8.0f;
    }

    private static void CreateRoadPrefab(RoadData roadData, GameObject parent)
    {
    }


    //    private static void CreateRoadPrefab(RoadData roadData, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    //    {
    //        string objectId = "RoadEdge" + roadData.Id;
    //        float height = 0.01f + Random.Range(0, 100) * 0.0001f;
    //
    //        if (roadData.Info.ContainsKey("natural"))
    //        {
    //            return;
    //        }
    //
    //        if (roadData.Info.ContainsKey("waterway"))
    //        {
    //            return;
    //        }
    //
    //        var mainPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/eWolfRoadBuilder/Prefabs/RoadNetwork_Main_pf.prefab");
    //        var mainPfObject = GameObject.Instantiate(mainPrefab);
    //        mainPfObject.name = objectId;
    //        var positions = roadData.ExteriorPositions.Select(pos =>
    //        {
    //            var lat = (pos.NorthLat - worldCenter.y) * worldScale.y;
    //            var lon = (pos.EastLon - worldCenter.x) * worldScale.x;
    //            return new Vector2((float)lon, (float)lat);
    //        }).ToArray();
    //        // Add
    //        var roadNetworkNodes = new List<RoadNetworkNode>();
    //
    //        for (int i = 0; i < positions.Length; i++)
    //        {
    //            GameObject child = new GameObject(string.Format("child_{0}", i));
    //            child.transform.position = new Vector3(
    //                positions[i].x,
    //                height,
    //                positions[i].y
    //            );
    //            child.transform.parent = mainPfObject.transform;
    //            var roadNetworkNode = child.AddComponent<RoadNetworkNode>();
    //            roadNetworkNodes.Add(roadNetworkNode);
    //        }
    //
    //        // セット
    //        {
    //            int i = 0;
    //            roadNetworkNodes[i].Details.Union = RoadNetworkNode.UNION_TYPE.END;
    //            roadNetworkNodes[i].Details.Roads = new List<RoadNetworkNode>()
    //            {
    //                roadNetworkNodes[i + 1]
    //            };
    //        }
    //
    //        for (int i = 1; i < positions.Length - 1; i++)
    //        {
    //            roadNetworkNodes[i].Details.Union = RoadNetworkNode.UNION_TYPE.CORNER;
    //            roadNetworkNodes[i].Details.Roads = new List<RoadNetworkNode>()
    //            {
    //                roadNetworkNodes[i + 1],
    //                                 roadNetworkNodes[i - 1]
    //            };
    //        }
    //
    //        {
    //            int i = positions.Length - 1;
    //            roadNetworkNodes[i].Details.Union = RoadNetworkNode.UNION_TYPE.END;
    //            roadNetworkNodes[i].Details.Roads = new List<RoadNetworkNode>()
    //            {
    //                roadNetworkNodes[i - 1]
    //            };
    //        }
    //
    //        var roadBuilder = mainPfObject.GetComponent<RoadBuilder>();
    //        roadBuilder.CrossSectionDetails.RoadWidth = 13.0f;
    //        roadBuilder.CreateCollision = false;
    //        roadBuilder.MeshPerNode = false;
    //        //        roadBuilder.DropToGround = false;
    //        mainPfObject.transform.parent = parent.transform;
    //    }

    public static void CreateRoadMesh(RoadData roadData, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        string objectId = "RoadEdge" + roadData.Id;
        string materialId = "Black";
        float height = 0.01f + Random.Range(0, 100) * 0.0001f;
        float width = 10.0f;

        if (roadData.Info.ContainsKey("natural"))
        {
            //            if (!roadData.Info["natural"].Equals("coastline"))
            //            {
            //                return;
            //            }
            //
            //            objectId = "Coastline" + roadData.Id;
            //            materialId = "WaterMaterial";
            //            height = 0.03f + Random.Range(0, 100) * 0.0001f;
            //            width = 10.0f;
            return;
        }

        if (roadData.Info.ContainsKey("waterway"))
        {
            objectId = "Waterway" + roadData.Id;
            materialId = "WaterMaterial";
            height = 0.00f + Random.Range(0, 100) * 0.0001f;
            width = 20.0f;
        }

        var obj = new GameObject(objectId);
        var positions = roadData.ExteriorPositions.Select(pos =>
        {
            var lat = (pos.NorthLat - worldCenter.y) * worldScale.y;
            var lon = (pos.EastLon - worldCenter.x) * worldScale.x;
            return new Vector2((float)lon, (float)lat);
        }).ToArray();
        var meshFilter = obj.AddComponent<MeshFilter>();
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        var meshCollider = obj.AddComponent<MeshCollider>();
        meshFilter.sharedMesh = CreateMesh(positions, width);
        meshRenderer.material = Resources.Load<Material>(materialId);
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        obj.transform.localPosition = Vector3.up * height;
        obj.transform.parent = parent.transform;
        obj.isStatic = true;
    }

    // LineRenderer で表現
    //    public static void CreateLine(RoadData roadData, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    //    {
    //        var obj = new GameObject("RoadEdge" + roadData.Id);
    //        var lineRenderer = obj.AddComponent<LineRenderer>();
    //        var positions = new List<Vector3>();
    //
    //        for (var i = 0; i < roadData.ExteriorPositions.Count; i++)
    //        {
    //            var pos = roadData.ExteriorPositions[i];
    //            var lat = (pos.NorthLat - worldCenter.y) * worldScale.y;
    //            var lon = (pos.EastLon - worldCenter.x) * worldScale.x;
    //            var height = 1.0f;
    //
    //            //			RaycastHit raycastHit;
    //            //			if (Physics.Raycast(new Vector3(
    //            //					lon,
    //            //					1000,
    //            //					lat
    //            //				),
    //            //				Vector3.down,
    //            //				out raycastHit
    //            //			))
    //            //			{
    //            //				height = raycastHit.point.y + 0.3f;
    //            //			}
    //            if (roadData.Info.ContainsKey("waterway"))
    //            {
    //                height = 5.0f;
    //            }
    //
    //            positions.Add(
    //                new Vector3(lon, height, lat)
    //            );
    //        }
    //
    //        lineRenderer.positionCount = positions.Count;
    //        lineRenderer.SetPositions(positions.ToArray());
    //        lineRenderer.startWidth = 1.0f;
    //        lineRenderer.endWidth = 1.0f;
    //        lineRenderer.material = Resources.Load<Material>("Buildings/Materials/Black");
    //
    //        if (roadData.Info.ContainsKey("waterway"))
    //        {
    //            lineRenderer.startWidth = 25.0f;
    //            lineRenderer.endWidth = 25.0f;
    //            lineRenderer.material = Resources.Load<Material>("Buildings/Materials/Blue");
    //        }
    //
    //        obj.transform.parent = parent.transform;
    //        obj.isStatic = true;
    //    }

    private static Mesh CreateMesh(Vector2[] points, float width)
    {
        var verts = CreateLineVerticies(points, width);
        var triangles = CreateTriangles(points.Length);
        var uvs = CreateUv(points.Length);
        var mesh = new Mesh();
        mesh.vertices = ConvertVector2ToVector3(verts);
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        return mesh;
    }

    private static Vector3[] ConvertVector2ToVector3(Vector2[] points)
    {
        return points.Select(it =>
        {
            var height = -1.0f;
            RaycastHit raycastHit;

            if (Physics.Raycast(new Vector3(
                                    it.x,
                                    100,
                                    it.y
                                ),
                                Vector3.down,
                                out raycastHit
                               ))
            {
                height = raycastHit.point.y + 0.1f;
            }

            return new Vector3(it.x, height, it.y);
        }).ToArray();
    }

    private static Vector2[] CreateLineVerticies(Vector2[] points, float width)
    {
        if (points.Length < 2)
        {
            return null;
        }

        var vertsLength = points.Length << 1;
        var result = new Vector2[vertsLength];
        Vector2[] p1Updown, p2Updown;
        float theta, sinTheta, cosTheta;
        float thetaPrevious, sinThetaPrevious, cosThetaPrevious;
        // 始点を求める
        {
            thetaPrevious = Mathf.Atan2(points[1].y - points[0].y, points[1].x - points[0].x);
            sinThetaPrevious = Mathf.Sin(thetaPrevious);
            cosThetaPrevious = Mathf.Cos(thetaPrevious);
            p1Updown = CalculateUpDownPoints(points[0], sinThetaPrevious, cosThetaPrevious, width);
            result[0] = p1Updown[0];
            result[1] = p1Updown[1];
        }

        // 中点を求める
        for (var i = 1; i < points.Length - 1; i++)
        {
            theta = Mathf.Atan2(points[i + 1].y - points[i].y, points[i + 1].x - points[i].x);
            sinTheta = Mathf.Sin(theta);
            cosTheta = Mathf.Cos(theta);
            p2Updown = CalculateUpDownPoints(points[i], sinTheta, cosTheta, width);
            var mediumPoints = CalculateMidiumPoint(
                                   p1Updown,
                                   p2Updown,
                                   sinThetaPrevious,
                                   cosThetaPrevious,
                                   sinTheta,
                                   cosTheta);
            var i2 = i << 1; // i*2
            result[i2] = mediumPoints[0];
            result[i2 + 1] = mediumPoints[1];
            sinThetaPrevious = sinTheta;
            cosThetaPrevious = cosTheta;
            p1Updown = p2Updown;
        }

        // 終点を求める
        {
            var length = points.Length;
            theta = Mathf.Atan2(points[length - 1].y - points[length - 2].y, points[length - 1].x - points[length - 2].x);
            sinTheta = Mathf.Sin(theta);
            cosTheta = Mathf.Cos(theta);
            p2Updown = CalculateUpDownPoints(points[length - 1], sinTheta, cosTheta, width);
            result[vertsLength - 2] = p2Updown[0];
            result[vertsLength - 1] = p2Updown[1];
        }
        return result;
    }

    private static int[] CreateTriangles(int pointCount)
    {
        var triangles = new int[(pointCount - 1) * 6];

        for (var i = 0; i < pointCount - 1; i++)
        {
            var triangleIndex = i * 6;
            var vertIndex = i << 1;
            triangles[triangleIndex] = vertIndex;
            triangles[triangleIndex + 1] = vertIndex + 1;
            triangles[triangleIndex + 2] = vertIndex + 2;
            triangles[triangleIndex + 3] = vertIndex + 2;
            triangles[triangleIndex + 4] = vertIndex + 1;
            triangles[triangleIndex + 5] = vertIndex + 3;
        }

        return triangles;
    }

    private static Vector2[] CreateUv(int pointCount)
    {
        var vertsLength = pointCount << 1;
        var result = new Vector2[vertsLength];

        for (var i = 0; i < vertsLength; i++)
        {
            var u = (i % 2) == 0 ? 0.0f : 1.0f;
            var v = ((i % 4) / 2) == 0 ? 0.0f : 1.0f;
            result[i] = new Vector2(u, v);
        }

        return result;
    }


    private static Vector2[] CalculateUpDownPoints(Vector2 p, float sinTheta, float cosTheta, float width)
    {
        var widthCosTheta = width * cosTheta;
        var widthSinTheta = width * sinTheta;
        var pUp = new Vector2(p.x + widthSinTheta, p.y - widthCosTheta);
        var pDown = new Vector2(p.x - widthSinTheta, p.y + widthCosTheta);
        return new[] {pUp, pDown};
    }

    // Input: p1, p2, p12_theta, p23_theta
    // Output: p2', p2''
    private static Vector2[] CalculateMidiumPoint(
        Vector2[] p1Updown,
        Vector2[] p2Updown,
        float sinTheta1, float cosTheta1, float sinTheta2, float cosTheta2)
    {
        var a1B2MinusA2B1 = -sinTheta1 * cosTheta2 + sinTheta2 * cosTheta1;
        double TOLERANCE = 10e-5;

        if (Math.Abs(a1B2MinusA2B1) < TOLERANCE)
        {
            return new Vector2[]
            {
                (p1Updown[0] + p2Updown[0]) * 0.5f,
                (p1Updown[1] + p2Updown[1]) * 0.5f,
            };
        }

        var p1Up = p1Updown[0];
        var p1Down = p1Updown[1];
        var p2Up = p2Updown[0];
        var p2Down = p2Updown[1];
        var c1B2C2B1Up = (p1Up.x * sinTheta1 - p1Up.y * cosTheta1) * cosTheta2 -
                         (p2Up.x * sinTheta2 - p2Up.y * cosTheta2) * cosTheta1;
        var c1A2C2A1Up = (p1Up.x * sinTheta1 - p1Up.y * cosTheta1) * -sinTheta2 +
                         (p2Up.x * sinTheta2 - p2Up.y * cosTheta2) * sinTheta1;
        var c1B2C2B1Down = (p1Down.x * sinTheta1 - p1Down.y * cosTheta1) * cosTheta2 -
                           (p2Down.x * sinTheta2 - p2Down.y * cosTheta2) * cosTheta1;
        var c1A2C2A1Down = (p1Down.x * sinTheta1 - p1Down.y * cosTheta1) * -sinTheta2 +
                           (p2Down.x * sinTheta2 - p2Down.y * cosTheta2) * sinTheta1;
        return new[]
        {
            new Vector2(c1B2C2B1Up / -a1B2MinusA2B1, c1A2C2A1Up / a1B2MinusA2B1),
            new Vector2(c1B2C2B1Down / -a1B2MinusA2B1, c1A2C2A1Down / a1B2MinusA2B1),
        };
    }
}
}
#endif
