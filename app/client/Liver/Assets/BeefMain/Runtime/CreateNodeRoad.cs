using System.Collections.Generic;
using BeefCreateModelScripts.Runtime;
using System.Linq;
using UnityEngine;
using System;
using BeefCreateModelScripts.Runtime.BeefMesh;

namespace BeefMain.Runtime
{
public class CreateNodeRoad
{
    public static float RoadWidth = 6.0f;

    public static void CreateRoadByNodeMap(NodeMap nodeMap)
    {
        CreateRoadByNodeMap(nodeMap, CreateMeshGameObject);
    }

    public static void CreateRoadByNodeMapNext(NodeMap nodeMap, Action<CreateRoadMeshScripts.MeshSet, string, string> createMeshGameObject)
    {
        var hash = new Dictionary<string, BeefMeshUtility.IPlaneMeshSet>();
        var roadMeshSets = new List<BeefMeshUtility.IPlaneMeshSet>();
        var curveMeshSets = new List<BeefMeshUtility.IPlaneMeshSet>();
        var areaMeshSets = new List<BeefMeshUtility.IPlaneMeshSet>();
        var groupDict = new NodeMap.GroupDict(nodeMap.nodeMap.Count);
        NodeMap.Group(nodeMap, out groupDict);

        foreach (var entry in groupDict)
        {
            // FIXME: 1つ以上のグループが存在する場合の暫定対処, 警告だけ出して処理続行する.
            if (entry.Value != 0)
            {
                Debug.LogWarningFormat("More than one Node groups exist.");
                continue;
            }

            CreateRoadMeshFromNodeLine(nodeMap.nodeMap, entry.Key, hash, roadMeshSets);
            var key1 = entry.Key;
            var centerNode = nodeMap.nodeMap[key1];

            if (1 < centerNode.Graph.Count)
            {
                var connectedMeshSets = centerNode.Graph.ConvertAll((string key2) => key1 + ":" + key2).ConvertAll((string input) => hash[input]).ToList();
                var meshSet = BeefMeshUtility.GetFrameMeshSet(connectedMeshSets);

                if (meshSet == null)
                {
                    continue;
                }

                if (centerNode.Graph.Count == 2)
                {
                    curveMeshSets.Add(meshSet);
                }
                else
                {
                    areaMeshSets.Add(meshSet);
                }
            }
        }

        var roadMeshSetsNeo = CreateRoadMeshScripts.GetCombinedMesh(roadMeshSets.ConvertAll((input) => input.GetMeshSet()));

        for (int i = 0; i < roadMeshSetsNeo.Count; i++)
        {
            createMeshGameObject(roadMeshSetsNeo[i], "Road" + i, "TileMaterial");
        }

        var curveMeshSetsNeo = CreateRoadMeshScripts.GetCombinedMesh(curveMeshSets.ConvertAll((input) => input.GetMeshSet()));

        for (int i = 0; i < curveMeshSetsNeo.Count; i++)
        {
            createMeshGameObject(curveMeshSetsNeo[i], "Curve" + i, "CurveTileMaterial");
        }

        var areaMeshSetsNeo = CreateRoadMeshScripts.GetCombinedMesh(areaMeshSets.ConvertAll((input) => input.GetMeshSet()));

        for (int i = 0; i < areaMeshSetsNeo.Count; i++)
        {
            createMeshGameObject(areaMeshSetsNeo[i], "CrossRoad" + i, "CrossTileMaterial");
        }
    }

    public static void CreateRoadMeshFromNodeLine(Dictionary<string, NodeMap.NodeData> nodeData, string key, Dictionary<string, BeefMeshUtility.IPlaneMeshSet> registerHashtag, List<BeefMeshUtility.IPlaneMeshSet> roadMeshCallback)
    {
        var key1 = key;
        var centerNode = nodeData[key1];
        var graph = centerNode.Graph;

        foreach (string key2 in graph)
        {
            var nextNode = nodeData[key2];
            var hashKey1 = key1 + ":" + key2;
            var hashKey2 = key2 + ":" + key1;

            // 同じノードをつないだメッシュを重複して生成しないようにハッシュで判定する。
            if (registerHashtag.ContainsKey(hashKey1) || registerHashtag.ContainsKey(hashKey2))
            {
            }
            else
            {
                var list = BeefMeshUtility.CreateStraightRoad(centerNode.Position, nextNode.Position, RoadWidth);
                registerHashtag[hashKey1] = list[0];
                registerHashtag[hashKey2] = list[list.Count - 1];

                for (int i = 1; i < list.Count - 1; i++)
                {
                    roadMeshCallback.Add(list[i]);
                }
            }
        }
    }


    public static void CreateRoadByNodeMap(NodeMap nodeMap, Action<CreateRoadMeshScripts.MeshSet, string, string> createMeshGameObject)
    {
        var hash = new HashSet<string>();
        var roadMeshSets = new List<CreateRoadMeshScripts.MeshSet>();
        var areaMeshSets = new List<CreateRoadMeshScripts.MeshSet>();
        var groupDict = new NodeMap.GroupDict();
        NodeMap.Group(nodeMap, out groupDict);

        foreach (var entry in groupDict)
        {
            // FIXME: 1つ以上のグループが存在する場合の暫定対処, 警告だけ出して処理続行する.
            if (entry.Value != 0)
            {
                Debug.LogWarningFormat("More than one Node groups exist.");
                continue;
            }

            CreateRoadMeshFromNode(nodeMap.nodeMap, entry.Key, hash,
                                   CreateRoadMeshScripts.CreateAddToMeshSetListCallback(roadMeshSets)
                                  );
            CreateCrossRoadMeshFromNode(nodeMap.nodeMap, entry.Key,
                                        CreateRoadMeshScripts.CreateAddToMeshSetListCallback(areaMeshSets)
                                       );
        }

        roadMeshSets = CreateRoadMeshScripts.GetCombinedMesh(roadMeshSets);

        for (int i = 0; i < roadMeshSets.Count; i++)
        {
            createMeshGameObject(roadMeshSets[i], "Road" + i, "TileMaterial");
        }

        areaMeshSets = CreateRoadMeshScripts.GetCombinedMesh(areaMeshSets.ConvertAll(ConvertUvSetting));

        for (int i = 0; i < areaMeshSets.Count; i++)
        {
            createMeshGameObject(areaMeshSets[i], "CrossRoad" + i, "CrossTileMaterial");
        }
    }

    /// <summary>
    /// 接続している道それぞれのメッシュを生成する。
    /// </summary>
    /// <param name="nodeData">Node data.</param>
    /// <param name="key">Key.</param>
    /// <param name="registerHashtag">Hash.</param>
    /// <param name="roadMeshCallback">Road mesh callback.</param>
    public static void CreateRoadMeshFromNode(Dictionary<string, NodeMap.NodeData> nodeData, string key, HashSet<string> registerHashtag, Action<CreateRoadMeshScripts.MeshSet> roadMeshCallback)
    {
        var key1 = key;
        var centerNode = nodeData[key1];

        foreach (var key2 in centerNode.Graph)
        {
            var nextNode = nodeData[key2];
            var hashKey = GetUniqueHash(key1, key2);

            // 同じノードをつないだメッシュを重複して生成しないようにハッシュで判定する。
            if (!registerHashtag.Contains(hashKey))
            {
                CreateRoadMeshScripts.CreateStraightRoad(centerNode.Position, nextNode.Position, RoadWidth, roadMeshCallback);
                registerHashtag.Add(hashKey);
            }
        }
    }

    /// <summary>
    /// 交差点のメッシュを生成する。
    /// </summary>
    /// <param name="nodeData">Node data.</param>
    /// <param name="key">Key.</param>
    /// <param name="areaMeshCallback">Area mesh callback.</param>
    public static void CreateCrossRoadMeshFromNode(Dictionary<string, NodeMap.NodeData> nodeData, string key, Action<CreateRoadMeshScripts.MeshSet> areaMeshCallback)
    {
        var key1 = key;
        var centerNode = nodeData[key1];
        var connectCount = centerNode.Graph.Count;

        if (connectCount < 3)
        {
            return;
        }

        var positionList = new List<Vector3>();

        foreach (var key2 in centerNode.Graph)
        {
            var nextNode = nodeData[key2];
            var vector = (nextNode.Position - centerNode.Position);
            var left = centerNode.Position + vector.normalized * RoadWidth + Quaternion.AngleAxis(-90, Vector3.up) * vector.normalized * RoadWidth * 0.5f;
            var right = centerNode.Position + vector.normalized * RoadWidth + Quaternion.AngleAxis(90, Vector3.up) * vector.normalized * RoadWidth * 0.5f;
            positionList.Add(left);
            positionList.Add(right);
        }

        try
        {
            var framePointsList = GetFrameScript.GetFramePoints(positionList.ConvertAll((input) => new Vector2(input.x, input.z))).ToList();
            // Zファイティングを防ぐため 道路より上に配置している。
            var yBuf = (1f / 16) * connectCount;
            positionList = framePointsList.ConvertAll((input) => new Vector3(input.x, centerNode.Position.y + yBuf, input.y));
            CreateRoadMeshScripts.CreateArea(positionList, areaMeshCallback);
        }
        catch (Exception e)
        {
            // Debug.LogException(e);
        }
    }



    /// <summary>
    /// UV 設定を 左下 (0,0) 右上 (1,1) となるように変更する。
    /// </summary>
    /// <returns>The uv setting.</returns>
    /// <param name="input">Input.</param>
    public static CreateRoadMeshScripts.MeshSet ConvertUvSetting(CreateRoadMeshScripts.MeshSet input)
    {
        var leftDown = new Vector2(
            input.VerticesList.Min((point) => point.x),
            input.VerticesList.Min((point) => point.z)
        );
        var rightUp = new Vector2(
            input.VerticesList.Max((point) => point.x),
            input.VerticesList.Max((point) => point.z)
        );

        for (int i = 0; i < input.VerticesList.Count; i++)
        {
            var point = input.VerticesList[i];
            var x = (point.x - leftDown.x) / (rightUp.x - leftDown.x);
            var y = (point.z - leftDown.y) / (rightUp.y - leftDown.y);
            input.UvList[i] = new Vector2(x, y);
        }

        return input;
    }

    /// <summary>
    /// メッシュを持つゲームオブジェクトを生成する。
    /// </summary>
    /// <param name="meshset">Meshset.</param>
    /// <param name="objName">Object name.</param>
    /// <param name="materialName">Material name.</param>
    private static void CreateMeshGameObject(CreateRoadMeshScripts.MeshSet meshset, string objName, string materialName)
    {
        Mesh mesh = meshset.CreateMesh();
        var obj = new GameObject(objName);
        var meshFilter = obj.AddComponent<MeshFilter>();
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = Resources.Load<Material>(materialName);
    }

    private static string GetUniqueHash(string key1, string key2)
    {
        return (string.CompareOrdinal(key1, key2) < 0) ?
               string.Concat(key1, ":", key2) :
               string.Concat(key2, ":", key1);
    }
}
}