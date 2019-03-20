using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeefDefine.SchemaWrapper;
using System.Linq;

namespace BeefMain.Runtime
{
/// <summary>
/// GameObject に NodeMap を持たせる.
/// </summary>
public class NodeMapHolder : MonoBehaviour
{
    /// <summary>
    /// The node map.
    /// </summary>
    public NodeMap nodeMap = null;

    private Transform _targetTransForm;
    private string nextNode;
    private LinkedList<string> currentRoute;

    private void UpdatePosition(Transform _transform)
    {
        if (currentRoute == null)
        {
            return;
        }

        if (nextNode == null)
        {
            nextNode = currentRoute.First.Value;
            _transform.position = nodeMap.nodeMap[nextNode].Position;
            return;
        }

        var targetPosition = nodeMap.nodeMap[nextNode].Position;
        var targetDir = targetPosition - _transform.position;

        if (0.5f < targetDir.magnitude)
        {
            _transform.position = _transform.position + targetDir.normalized * 0.2f;
            return;
        }

        var nextNodeKouho = currentRoute.Find(nextNode).Next;

        if (nextNodeKouho != null)
        {
            nextNode = nextNodeKouho.Value;
        }
    }

    private void Update()
    {
        if (_targetTransForm != null)
        {
            UpdatePosition(_targetTransForm);
        }
    }

    /// <summary>
    /// _transform にランダムな道を歩かせる.
    /// </summary>
    /// <param name="_transform">Transform.</param>
    public void Test(Transform _transform)
    {
        nextNode = null;
        _targetTransForm = _transform;
        var groupDict = new NodeMap.GroupDict();
        var groupCount = NodeMap.Group(nodeMap, out groupDict);
        groupDict.Dump();
        var sameGroup = groupDict.Where((obj) =>
        {
            return obj.Value == 0;
        }).ToList();
        int start = Random.Range(0, sameGroup.Count);
        int goal = Random.Range(0, sameGroup.Count);
        var nodeNames = sameGroup.Select((arg) => arg.Key).ToArray();
        Debug.Log(sameGroup.Count);
        NodeMap.CostDict keyValuePairs;
        LinkedList<string> route;
        var pathCount = NodeMap.GetShortestRoute(nodeMap, nodeNames[start], nodeNames[goal], 10000, out keyValuePairs, out route);
        Debug.Log(string.Format("道を探します : start : {0} {1}", nodeNames[start], nodeNames[goal]));

        if (route == null)
        {
            Debug.Log("道が遠すぎたため停止します");
            currentRoute = null;
            return;
        }

        foreach (var nodeName in route)
        {
            Debug.Log("route : " + nodeName + " : " + nodeMap.nodeMap[nodeName].Position);
        }

        currentRoute = route;
    }

    /// <summary>
    /// 道の情報を NodeMap に変換する.
    /// </summary>
    /// <param name="roadDataModels">Road data models.</param>
    /// <param name="worldCenter">World center.</param>
    /// <param name="worldScale">World scale.</param>
    public void Initialize(List<RoadDataModel> roadDataModels, Vector2 worldCenter, Vector2 worldScale)
    {
        nodeMap = NodeMap.CreateNodeMap(roadDataModels, worldCenter, worldScale);
    }
}
}
