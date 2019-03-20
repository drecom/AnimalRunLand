using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeefMain.Runtime;
using System.Text;
using System;
namespace Map
{

public class RoadInfo
{
    // 交差点を含まない道
    public class Road
    {
        public List<Vector3> Positions = new List<Vector3>();

        public float TotalLength
        {
            get
            {
                float l = 0f;

                foreach (var m in Magnitudes) { l += m; }

                return l;
            }
        }

        float[] magnitudes;
        public float[] Magnitudes
        {
            get
            {
                if (magnitudes == null)
                {
                    magnitudes = new float[Positions.Count - 1];

                    for (int i = 1; i < Positions.Count; ++i)
                    {
                        magnitudes[i - 1] = (Positions[i] - Positions[i - 1]).magnitude;
                    }
                }

                return magnitudes;
            }
        }
    }
    List<Road> roads = new List<Road>();


    public List<Road> Roads
    {
        get
        {
            return this.roads;
        }
    }

    public RoadInfo(Dictionary<string, NodeMap.NodeData> nodes)
    {
        var hash = new HashSet<Tuple<string, string>>();

        foreach (var node in nodes)
        {
            var value = node.Value;

            // 接続数２→交差点ではないのでスルー
            if (value.Graph.Count == 2) { continue; }

            foreach (var n in value.Graph)
            {
                var key = GetUniqueHash(node.Key, n);

                // 調査済みなら次へ
                if (!hash.Contains(key))
                {
                    hash.Add(key);
                    // 行き止まり or 交差点まで検索
                    Road road = new Road();
                    road.Positions.Add(value.Position);
                    SearchOneway(n, hash, road, nodes);
                    this.roads.Add(road);
                }
            }
        }

        Debug.Log(this.roads.Count);
#if UNITY_EDITOR
        // 道の総延長を計算
        double length = 0;

        foreach (var road in this.roads)
        {
            for (int i = 0; i < road.Positions.Count - 1; ++i)
            {
                var v = road.Positions[i] - road.Positions[i + 1];
                length += v.magnitude;
            }
        }

        Debug.Log("Road length: " + length);
#endif
    }


    // 通路を延々探して繋ぐ
    void SearchOneway(string node_name, HashSet<Tuple<string, string>> hash, Road road, Dictionary<string, NodeMap.NodeData> nodes)
    {
        var node = nodes[node_name];
        road.Positions.Add(node.Position);

        // 行き止まり or 交差点ならここで調査終了
        if (node.Graph.Count != 2) { return; }

        // ハッシュに登録して次へ
        foreach (var n in node.Graph)
        {
            var key = GetUniqueHash(node_name, n);

            if (!hash.Contains(key))
            {
                hash.Add(key);
                SearchOneway(n, hash, road, nodes);
            }
        }
    }

    // 道の名前を生成
    static Tuple<string, string> GetUniqueHash(string key1, string key2)
    {
        return (string.CompareOrdinal(key1, key2) < 0) ?
               Tuple.Create(key1, key2) : Tuple.Create(key2, key1);
    }

    // 連結線分上の座標を始点からの距離で計算
    public static Vector3 CalcPosition(Road road, float distance)
    {
        var positions = road.Positions;
        var magnitudes = road.Magnitudes;

        for (int i = 0; i < magnitudes.Length; i++)
        {
            distance -= magnitudes[i];

            if (distance < 0)
            {
                var v = (positions[i + 1] - positions[i]);
                var p = positions[i + 1] + v.normalized * distance;
                return p;
            }
        }

        // 距離の方が長かった場合は最後尾を返す
        return positions[positions.Count - 1];
    }

    // 連結線分上の単位ベクトルを始点からの距離で計算
    public static Vector3 CalcVector(Road road, float distance)
    {
        var positions = road.Positions;
        var magnitudes = road.Magnitudes;

        for (int i = 0; i < magnitudes.Length; i++)
        {
            distance -= magnitudes[i];

            if (distance < 0)
            {
                var v = (positions[i + 1] - positions[i]);
                return v.normalized;
            }
        }

        // 距離の方が長かった場合は最後尾を返す
        return (positions[positions.Count - 1] - positions[positions.Count - 2]).normalized;
    }

    // 連結線分上が曲がっているか調べる
    public static bool CheckVertex(Road road, float distance, float range, float cosTheta = 0.98f)
    {
        // 現在位置、視点、終点の向きベクトルを調べる
        var center = CalcVector(road, distance);
        var start  = CalcVector(road, distance - range * 0.5f);
        var end    = CalcVector(road, distance + range * 0.5f);
        // 内積の絶対値が1に近い→直線に近い
        var d1 = Vector3.Dot(center, start);
        var d2 = Vector3.Dot(center, end);
        // cosθと比較している
        return (Mathf.Abs(d1) > cosTheta) && (Mathf.Abs(d2) > cosTheta);
    }

}

}
