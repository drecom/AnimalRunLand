namespace BeefCreateModelScripts.Runtime
{
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using BeefDefine.SchemaWrapper;

/// <summary>
/// 道の情報を LineRenderer で表現する.
/// </summary>
public class CreateNodeMapFromRoadDataScripts
{
    /// <summary>
    /// 道の表現
    /// </summary>
    public enum RoadType
    {
        LINE,
        MESH
    };

    /// <summary>
    /// 道データから道を表現する。
    /// </summary>
    /// <param name="roadDatas">Road datas.</param>
    /// <param name="parent">親オブジェクト</param>
    /// <param name="worldCenter">Unity の中心座標.</param>
    /// <param name="worldScale">Unity の表現スケール.</param>
    /// <param name="roadType">道の表現(RoadType.LINE or RoadType.MESH).</param>
    public static void CreateNodeMapFromListRoadData(List<RoadDataModel> roadDatas, GameObject parent, Vector2 worldCenter, Vector2 worldScale, RoadType roadType = RoadType.LINE)
    {
        var transformFindResult = parent.transform.Find("nodeMapRoot");
        GameObject root;

        if (transformFindResult == null)
        {
            root = new GameObject("nodeMapRoot");
        }
        else
        {
            root = transformFindResult.gameObject;
        }

        root.transform.parent = parent.transform;

        if (roadType == RoadType.LINE)
        {
            for (int i = 0; i < roadDatas.Count; i++)
            {
                var roadData = roadDatas[i];
                CreateHighwayLine(roadData, root, worldCenter, worldScale);
#if UNITY_EDITOR

                if (i % 100 == 0)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("CreateRoadLineFromListRoadData",
                            string.Format("CreateRoadLineFromListRoadData {0} / {1}", i, roadDatas.Count),
                            (i * 1.0f) / roadDatas.Count);
                }

#endif
            }
        }
        else
        {
            var meshSets = new List<CreateRoadMeshScripts.MeshSet>();

            for (int i = 0; i < roadDatas.Count; i++)
            {
                var roadData = roadDatas[i];

                if (!roadData.InfoList.FullInfoList.ContainsKey("highway"))
                {
                    continue;
                }

                var positions = roadData.Positions.Select(pos =>
                {
                    var height = 2.0f;
                    return new Vector3(
                               (pos.Position.EastLon - worldCenter.x) * worldScale.x,
                               height,
                               (pos.Position.NorthLat - worldCenter.y) * worldScale.y
                           );
                }).ToList();
                CreateRoadMeshScripts.CreateCurve(positions, 6.0f, CreateRoadMeshScripts.CreateAddToMeshSetListCallback(meshSets));
#if UNITY_EDITOR

                if (i % 100 == 0)
                {
                    UnityEditor.EditorUtility.DisplayProgressBar("CreateRoadLineFromListRoadData",
                            string.Format("CreateRoadLineFromListRoadData {0} / {1}", i, roadDatas.Count),
                            (i * 1.0f) / roadDatas.Count);
                }

#endif
            }

            var combinedMesh = CreateRoadMeshScripts.GetCombinedMesh(meshSets);

            for (int i = 0; i < combinedMesh.Count; i++)
            {
                var meshSet = combinedMesh[i];
                Mesh mesh = meshSet.CreateMesh();
                var obj = new GameObject("RoadMesh");
                var meshFilter = obj.AddComponent<MeshFilter>();
                var meshRenderer = obj.AddComponent<MeshRenderer>();
                meshFilter.sharedMesh = mesh;
                // TODO: マテリアルの外部設定.
                meshRenderer.sharedMaterial = Resources.Load<Material>("TileMaterial");
                obj.transform.SetParent(root.transform);
            }
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.ClearProgressBar();
#endif
    }

    public static void CreateHighwayLine(RoadDataModel roadData, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        if (!roadData.InfoList.FullInfoList.ContainsKey("highway"))
        {
            return;
        }

        var positions = roadData.Positions.Select(pos =>
        {
            var height = 2.0f;
            return new Vector3(
                       (pos.Position.EastLon - worldCenter.x) * worldScale.x,
                       height,
                       (pos.Position.NorthLat - worldCenter.y) * worldScale.y
                   );
        }).ToList();
        var obj = new GameObject("RoadEdge" + roadData.Id);
        var lineRenderer = obj.AddComponent<LineRenderer>();
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;
        lineRenderer.material = Resources.Load<Material>("Buildings/Materials/Black");
        obj.transform.parent = parent.transform;
        obj.isStatic = true;
    }

}
}
