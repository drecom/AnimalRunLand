////================================
//// Beefmap を読み込んでアイテム配置し、ファイルを書き出す
////
////================================
//using UnityEngine;
//using UnityEngine.Assertions;
//using System.IO;
//using BeefDefine.SchemaWrapper;
//using BeefMain.Runtime;
//using System.Text.RegularExpressions;
//using System.Collections.Generic;
//using System.Linq;

//#if UNITY_EDITOR
//using UnityEditor;
//#endif

//public class ProcessData
//{
//    public string[] poiNames;
//    public Vector3[] poiPositions;
//}

//#if UNITY_EDITOR
//public static class BeefmapProcess
//{
//    [MenuItem("Beef/TA/Process")]
//    public static void Exec()
//    {

//        /// MEMO : 将来はサーバから *.beefmap 一覧を取得してすべてのマップを処理する
//        foreach (var fn in Directory.EnumerateFiles(Application.streamingAssetsPath, "*.beefmap"))
//        {
//            Process(fn);
//        }
//    }

//    static void Process(string fn)
//    {
//        var streamingAssetsPath = Application.streamingAssetsPath;
//        var path = Path.Combine(streamingAssetsPath, fn);
//        byte[] bytes = File.ReadAllBytes(path);

//        var beefMapObject = BeefMapObjectModel.LoadByData(bytes);

//        var go = new GameObject("Process");
//        var holder = go.AddComponent<NodeMapHolder>();

//        // マップの中心位置はファイル名から推察
//        var gridPosition = GetGridFromFileName(fn);
//        var centerPosition = GetCenterPositionFromGridPosition(gridPosition);

//        holder.Initialize(beefMapObject.RoadDataModels, centerPosition, MapUtility.WorldScale);
//        var nodes = holder.GetNodeDataByGroup(0);

//        Debug.Log($"nodes count : {nodes.Count}");
//        var pos = new List<Vector3>();
//        foreach (var node in nodes)
//        {
//            pos.Add((node.Value.Position + Vector3.up) );
//        }

//        //var namesFilter = new[] { "学", "校", "交番", "駐", "署", "園", "駅", "新" };

//        var poi = new List<Vector3>();
//        var names = new List<string>();

//        //foreach (var building in beefMapObject.BuildingDataModels)
//        //{
//        //    string name;
//        //    if (!building.InfoList.FullInfoList.TryGetValue("name", out name)) continue;
//        //    if (!namesFilter.Any(n => name.Contains(n))) continue;

//        //    names.Add(string.IsNullOrEmpty(name) ? "noname" : name);

//        //    var positionList = building.ExteriorPositions.Select(v =>
//        //    {
//        //        return new Vector3(
//        //            (v.EastLon - centerPosition.x) * MapUtility.WorldScale.x,
//        //            2.0f,
//        //            (v.NorthLat - centerPosition.y) * MapUtility.WorldScale.y
//        //        );
//        //    }).ToList();

//        //    var x = positionList.Sum(p => p.x)/ positionList.Count;
//        //    var y = positionList.Sum(p => p.y)/ positionList.Count;
//        //    var z = positionList.Sum(p => p.z)/ positionList.Count;
//        //    poi.Add(new Vector3(x,y,z));
//        //}

//        var process = new ProcessData { poiPositions = poi.ToArray(), poiNames = names.ToArray() };

//        File.WriteAllText(Path.ChangeExtension(path, "process"), JsonUtility.ToJson(process));
//        AssetDatabase.Refresh();

//        GameObject.DestroyImmediate(go);
//    }

//    public static Vector2Int GetGridFromFileName(string filename)
//    {
//        var firstFilePathInfo = Path.GetFileNameWithoutExtension(filename).Split('_');
//        var gridX = int.Parse(firstFilePathInfo[1]);
//        var gridY = int.Parse(firstFilePathInfo[2]);
//        return new Vector2Int(gridX, gridY);
//    }

//    public static Vector2 GetCenterPositionFromGridPosition(Vector2Int gridPosition)
//    {
//        return MapUtility.NihonCenterPosition +
//               new Vector2(MapUtility.OneKilometerDegree.x * 5 * gridPosition.x,
//                           MapUtility.OneKilometerDegree.y * 5 * gridPosition.y);
//    }

//}
//#endif
