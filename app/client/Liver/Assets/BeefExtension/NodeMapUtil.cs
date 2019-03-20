using System.Collections.Generic;
using UnityEngine;
using BeefMain.Runtime;
using System.Linq;
using static BeefMain.Runtime.NodeMap;

public static class NodeMapUtil
{
    /// <summary>
    /// 正面のノードを計算する
    /// </summary>
    /// <returns></returns>
    public static string FrontNode(NodeMap nodeMap, string current, string next)
    {
        var nodes = nodeMap.nodeMap;
        // current node を除く
        var n = nodes[next].Graph.Where(name => name != current).ToList();

        if (n.Count > 0)
        {
            n.Sort((a, b) =>
            {
                var cpos = nodes[current].Position;
                var npos = nodes[next].Position;
                var apos = nodes[a].Position;
                var bpos = nodes[b].Position;
                var ad = Vector3.Dot((npos - cpos).normalized, (apos - npos).normalized);
                var bd = Vector3.Dot((npos - cpos).normalized, (bpos - npos).normalized);
                return bd.CompareTo(ad);
            });
            return n[0];
        }
        else
        {
            return current;
        }
    }

    /// <summary>
    /// 移動する
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="current"></param>
    /// <param name="next"></param>
    /// <param name="transform"></param>
    /// <param name="distance"></param>
    public static float Move(NodeMap nodeMap, string next, Transform transform, float distance = 0.1f)
    {
        var nodes = nodeMap.nodeMap;
        var nextNode = nodes[next];
        var dir = nextNode.Position - transform.localPosition;

        if (dir.magnitude > distance)
        {
            transform.localPosition += dir.normalized * distance;
            return 0f;
        }
        else
        {
            transform.localPosition = nextNode.Position;
            return distance - dir.magnitude;
        }
    }

    /// <summary>
    /// 次の分岐ノード一覧を取得
    /// </summary>
    public static List<string> BranchList(Dictionary<string, NodeMap.NodeData> nodes, string current, string next, out string branchStart, out string branchEnd)
    {
        branchStart = current;
        branchEnd = next;
        var n = nodes[next].Graph.Where(name => name != current);

        switch (nodes[next].Graph.Count)
        {
            case 1:
                return new List<string>();

            case 2:
                return BranchList(nodes, next, n.First(), out branchStart, out branchEnd);

            default:
                return n.ToList();
        }
    }

    /// <summary>
    /// 分岐までの距離を取得
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="pos"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    public static float BranchDistance(NodeMap nodeMap, string current, string next, Transform transform)
    {
        var nodes = nodeMap.nodeMap;
        var nextNode = nodes[next];
        var res = (nextNode.Position - transform.localPosition).magnitude;

        if (nextNode.Graph.Count == 2)
        {
            return res + BranchDistance(nodeMap, nextNode.Graph.Find(n => n != current), next);
        }
        else
        {
            return res;
        }
    }

    static float BranchDistance(NodeMap nodeMap, string next, string current)
    {
        var nodes = nodeMap.nodeMap;
        var node = nodes[current];
        var nextNode = nodes[next];
        var res = (nextNode.Position - node.Position).magnitude;

        if (nextNode.Graph.Count == 2)
        {
            return res + BranchDistance(nodeMap, nextNode.Graph.Find(n => n != current), next);
        }
        else
        {
            return res;
        }
    }
}
