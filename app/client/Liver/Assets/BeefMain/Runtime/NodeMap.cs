using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BeefMain.Runtime
{
/// <summary>
/// Node map.
/// </summary>
public class NodeMap
{
    public class NodeData
    {
        public Vector3 Position;
        public List<string> Graph;
    }

    /// <summary>
    /// 各ノードの情報.
    /// </summary>
    public Dictionary<string, NodeData> nodeMap = new Dictionary<string, NodeData>();


    public class CostData
    {
        /// 実コスト
        public float cost;
        /// ヒューリスティック・コスト
        public float heuristicCost;
        /// 親ノード
        public string parent;
    }

    public class CostDict : Dictionary<string, CostData>
    {
        public CostDict()
        {
        }

        public CostDict(int capacity) : base(capacity)
        {
        }
    }

    public class GroupDict : Dictionary<string, int>
    {
        public GroupDict()
        {
        }

        public GroupDict(int capacity) : base(capacity)
        {
        }

        public void Dump()
        {
            int groupCount = this.Max((obj) =>
            {
                return obj.Value;
            });
            Debug.Log(string.Format("グループ数 : {0}", groupCount));

            for (int i = 0; i < groupCount; i++)
            {
                var count = this.Count<KeyValuePair<string, int>>((obj) =>
                {
                    return (obj.Value == i);
                });
                Debug.Log(string.Format("{0} グループ内のノード数 : {1}", i, count));
            }
        }
    }

    public static NodeMap CreateNodeMap(List<BeefDefine.SchemaWrapper.RoadDataModel> roadDataModels, Vector2 worldCenter, Vector2 worldScale)
    {
        var _nodeMap = new NodeMap();

        foreach (var roadData in roadDataModels)
        {
            if (!roadData.InfoList.FullInfoList.ContainsKey("highway"))
            {
                continue;
            }

            var positions = roadData.Positions;

            for (int i = 0; i < positions.Count; i++)
            {
                var position = positions[i];
                var nodeName = position.Id;
                var linkedNode = new List<string>();

                if (i != 0)
                {
                    linkedNode.Add(positions[i - 1].Id);
                }

                if ((i + 1) != positions.Count)
                {
                    linkedNode.Add(positions[i + 1].Id);
                }

                if (_nodeMap.nodeMap.ContainsKey(nodeName))
                {
                    var nodeData = _nodeMap.nodeMap[nodeName];
                    nodeData.Graph.AddRange(linkedNode);
                }
                else
                {
                    var vector3position = new Vector3(
                        (position.Position.EastLon - worldCenter.x) * worldScale.x,
                        1.5f,
                        (position.Position.NorthLat - worldCenter.y) * worldScale.y
                    );
                    var data = new NodeMap.NodeData
                    {
                        Graph = new List<string>(linkedNode),
                        Position = vector3position
                    };
                    _nodeMap.nodeMap.Add(nodeName, data);
                }
            }
        }

        return _nodeMap;
    }

    /// <summary>
    /// 道がつながっているかどうかグループ分けする.
    /// </summary>
    /// <returns>The group.</returns>
    /// <param name="nodeMap">Node map.</param>
    /// <param name="group">Group.</param>
    public static int Group(NodeMap nodeMap, out GroupDict group)
    {
        var groupId = 0;
        var keys = nodeMap.nodeMap.Keys.ToArray();
        group = new GroupDict(keys.Length);

        for (int i = 0; i < keys.Length; i++)
        {
            var key = keys[i];

            if (!group.ContainsKey(key))
            {
                Paint(nodeMap, group, key, groupId);
                groupId++;
            }
        }

        return groupId;
    }

    private static void Paint(NodeMap nodeMap, GroupDict costDict, string startKey, int paintId)
    {
        costDict[startKey] = paintId;
        var stack = new List<string>();
        stack.Add(startKey);

        while (stack.Count != 0)
        {
            var key = stack[0];
            var graph = nodeMap.nodeMap[key].Graph;

            foreach (var obj in graph)
            {
                if (!costDict.ContainsKey(obj))
                {
                    costDict[obj] = paintId;
                    stack.Add(obj);
                }
            }

            stack.RemoveAt(0);
        }
    }

    private struct CostDataForStack
    {
        public float cost;
        public float score;
        public string nodeName;

        public class myReverserClass : IComparer<CostDataForStack>
        {
            int IComparer<CostDataForStack>.Compare(CostDataForStack x, CostDataForStack y)
            {
                if (x.score == y.score)
                {
                    if (x.cost < y.cost)
                    {
                        return -1;
                    }

                    if (x.cost == y.cost)
                    {
                        return 0;
                    }

                    return 1;
                }

                if (x.score < y.score)
                {
                    return -1;
                }

                return 1;
            }
        }

        public static void Dump(CostDataForStack costDataForStack)
        {
            // Debug.Log(string.Format("Name: {0}, Cost: {1}, score:{2}", costDataForStack.nodeName, costDataForStack.cost, costDataForStack.score));
        }
    };

    /// <summary>
    /// 最短経路を探す.
    /// </summary>
    /// <returns>The saitan.</returns>
    /// <param name="nodeMap">Node map.</param>
    /// <param name="startKey">Start key.</param>
    /// <param name="goalKey">Goal key.</param>
    /// <param name="costDict">Cost dict.</param>
    /// <param name="route">Route.</param>
    public static int GetShortestRoute(NodeMap nodeMap, string startKey, string goalKey, out CostDict costDict, out LinkedList<string> route)
    {
        return GetShortestRoute(nodeMap, startKey, goalKey, 10000, out costDict, out route);
    }

    /// <summary>
    /// 最短経路を探す.
    /// </summary>
    /// <returns>The saitan.</returns>
    /// <param name="nodeMap">Node map.</param>
    /// <param name="startKey">Start key.</param>
    /// <param name="goalKey">Goal key.</param>
    /// <param name="loopout">計算を諦めるしきい値</param>
    /// <param name="costDict">Cost dict.</param>
    /// <param name="route">Route.</param>
    public static int GetShortestRoute(NodeMap nodeMap, string startKey, string goalKey, int loopout, out CostDict costDict, out LinkedList<string> route)
    {
        costDict = new CostDict(nodeMap.nodeMap.Count);
        var stack = new SortedSet<CostDataForStack>(new CostDataForStack.myReverserClass());
        var startNode = nodeMap.nodeMap[startKey];
        var goalNode = nodeMap.nodeMap[goalKey];
        {
            var openNodeCostData = new CostData
            {
                cost = 0.0f,
                heuristicCost = Vector3.Distance(startNode.Position, goalNode.Position),
                parent = null
            };
            costDict[startKey] = openNodeCostData;
            CostDataForStack costDataForStack = new CostDataForStack
            {
                cost = openNodeCostData.cost,
                score = openNodeCostData.cost + openNodeCostData.heuristicCost,
                nodeName = startKey
            };
            stack.Add(costDataForStack);
        }

        while (stack.Count != 0 && 0 < loopout)
        {
            loopout--;
            var parent = stack.First();
            stack.Remove(parent);
            var parentName = parent.nodeName;
            var parentNode = nodeMap.nodeMap[parentName];
            var parentCost = costDict[parentName];

            foreach (var openNodeName in parentNode.Graph)
            {
                if (costDict.ContainsKey(openNodeName))
                {
                    continue;
                }

                var openNode = nodeMap.nodeMap[openNodeName];
                var openNodeCost = parentCost.cost + Vector3.Distance(openNode.Position, parentNode.Position);
                var hCost = Vector3.Distance(openNode.Position, goalNode.Position);
                var openNodeCostData = new CostData
                {
                    cost = openNodeCost,
                    heuristicCost = hCost,
                    parent = parentName
                };
                var costDataForStack = new CostDataForStack
                {
                    cost = openNodeCostData.cost,
                    score = openNodeCostData.cost + openNodeCostData.heuristicCost,
                    nodeName = openNodeName
                };
                costDict[openNodeName] = openNodeCostData;
                stack.Add(costDataForStack);
                CostDataForStack.Dump(costDataForStack);
            }

            if (costDict.ContainsKey(goalKey))
            {
                break;
            }
        }

        if (costDict.ContainsKey(goalKey))
        {
            route = new LinkedList<string>();
            var node = costDict[goalKey];

            while (node.parent != null)
            {
                route.AddFirst(node.parent);
                node = costDict[node.parent];
            }

            return route.Count;
        }
        else
        {
            route = null;
            return 0;
        }
    }
}
}
