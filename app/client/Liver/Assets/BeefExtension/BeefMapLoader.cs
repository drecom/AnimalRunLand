using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeefMain.Runtime;
using BeefDefine.SchemaWrapper;
using System;
using BeefCreateModelScripts.Runtime;
using System.Threading;

public class BeefMap
{
    public static BeefMap Current;
    public delegate void CreateRoadByNodeMapCallback(CreateRoadMeshScripts.MeshSet meshset, string objName, string materialName);

    public NodeMap nodeMap { get; private set; }
    public BeefMapObjectModel beefMapObjectModel { get; private set;}

    /// <summary>
    /// 加工データ
    /// </summary>
    public List<NodeDataModel> poiList = new List<NodeDataModel>();
    public List<NodeDataModel> gateList = new List<NodeDataModel>();
    public List<NodeDataModel> stationList = new List<NodeDataModel>();
    public Dictionary<string, HashSet<string>> namedRoad = new Dictionary<string, HashSet<string>>();   // 名前を設定された道一覧

    public Dictionary<string, Vector3> stationWorldPositions = new Dictionary<string, Vector3>();

    private List<RoadData> roadList = new List<RoadData>();

    class Parameter
    {
        public BeefMap beefmap;
        public byte[] bytes;
        public Vector2 worldCenter;
        public Vector2 worldScale;
    }

    class RoadData
    {
        public CreateRoadMeshScripts.MeshSet meshset;
        public string objName;
        public string materialName;
    }

    /// <summary>
    /// バイナリからロードする
    /// </summary>
    /// <param name="bytes"></param>
    public IEnumerator LoadAsync(byte[] bytes, Vector2 worldCenter, Vector2 worldScale)
    {
        var thread = new Thread(new ParameterizedThreadStart(LoadAsyncInternal));
        thread.Start(new Parameter { beefmap = this, bytes = bytes, worldCenter = worldCenter, worldScale = worldScale });

        while (thread.ThreadState == ThreadState.Running) { yield return null; }

        Current = this;
    }

    /// <summary>
    /// スレッド
    /// </summary>
    /// <param name="o"></param>
    private static void LoadAsyncInternal(object o)
    {
        var data = o as Parameter;
        // バイナリ -> BeefMapObjectModel
        data.beefmap.beefMapObjectModel = BeefMapObjectModel.LoadByData(data.bytes);
        // BeefMapObjectModel -> NodeMap
        data.beefmap.nodeMap = NodeMap.CreateNodeMap(data.beefmap.beefMapObjectModel.RoadDataModels, data.worldCenter, data.worldScale).FilterByGroup(0);

        // 加工
        foreach (var node in data.beefmap.beefMapObjectModel.NodeDataModels)
        {
            if (node.IsPOI()) { data.beefmap.poiList.Add(node); }

            if (node.IsGate()) { data.beefmap.gateList.Add(node); }

            if (node.IsStation())
            {
                // 3kmグリッド内なら追加
                var pos = node.WorldPostiton(data.worldCenter, data.worldScale);

                if (Mathf.Abs(pos.x) < Map.LocationDeterminer.GridSizeKilometer * 0.5f
                        && Mathf.Abs(pos.z) < Map.LocationDeterminer.GridSizeKilometer * 0.5f)
                {
                    // 駅名が正常に取れなかったらスキップする。
                    if (string.Equals(node.Id, node.Name()))
                    {
                        continue;
                    }

                    data.beefmap.stationWorldPositions[node.Name()] = pos;
                }
            }
        }

        foreach (var item in data.beefmap.beefMapObjectModel.RoadDataModels)
        {
            string name;

            if (item.InfoList.TryGetValue("name", out name))
            {
                if (!data.beefmap.namedRoad.ContainsKey(name))
                {
                    data.beefmap.namedRoad.Add(name, new HashSet<string>());
                }

                foreach (var p in item.Positions)
                {
                    data.beefmap.namedRoad[name].Add(p.Id);
                }
            }
        }

        // 道メッシュを計算する
        CreateNodeRoad.CreateRoadByNodeMapNext(data.beefmap.nodeMap, (meshSet, name, material) =>
        {
            var road = new RoadData { meshset = meshSet, objName = name, materialName = material };
            data.beefmap.roadList.Add(road);
        });
    }

    public void CreateRoadByNodeMap(CreateRoadByNodeMapCallback cb, bool clear = true)
    {
        foreach (var road in roadList)
        {
            cb(road.meshset, road.objName, road.materialName);
        }

        if (clear) { roadList.Clear(); }
    }
}
