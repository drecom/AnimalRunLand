using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BeefCreateModelScripts.Runtime
{
/// <summary>
/// 建物の情報から 建物のモデルを作成する.
/// </summary>
public class CreateModelMeshScripts
{
    public static void CreateBuilding(List<BeefDefine.SchemaWrapper.BuildingDataModel> buildingDataList, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        var root = new GameObject("BuildingRoot");
        root.transform.parent = parent.transform;

        for (int i = 0; i < buildingDataList.Count; i++)
        {
            var buildingData = buildingDataList[i];
            CreateBuildingMesh(buildingData, root, worldCenter, worldScale);
#if UNITY_EDITOR

            if (i % 100 == 0)
            {
                UnityEditor.EditorUtility.DisplayProgressBar("CreateBuildingAsset",
                        string.Format("CreateAsset {0} / {1}", i, buildingDataList.Count),
                        (i * 1.0f) / buildingDataList.Count);
            }

#endif
        }

#if UNITY_EDITOR
        UnityEditor.EditorUtility.ClearProgressBar();
#endif
    }

    private static void CreateBuildingMesh(BeefDefine.SchemaWrapper.BuildingDataModel buildingData, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        var height = GetHeight(buildingData);

        if (buildingData.InfoList.FullInfoList.ContainsKey("railway"))
        {
            return;
        }

        if (buildingData.InfoList.FullInfoList.ContainsKey("highway"))
        {
            return;
        }

        if (buildingData.InfoList.FullInfoList.ContainsKey("route"))
        {
            return;
        }

        if (!buildingData.InfoList.FullInfoList.ContainsKey("building"))
        {
            return;
        }

        var positionList = buildingData.ExteriorPositions.Select(pos =>
        {
            return new Vector2(
                       (pos.EastLon - worldCenter.x) * worldScale.x,
                       (pos.NorthLat - worldCenter.y) * worldScale.y
                   );
        }).ToList();
        Action<List<Vector2>, float, Action<Mesh>> createBuildingMeshAction = CreateBuildingMeshAlgorithms.CreateBuildingMesh_WithNineSlice;
        // Action<List<Vector2>, float, Action<Mesh>> createBuildingMeshAction = CreateBuildingMeshAlgorithms.CreateBuildingMesh;
        createBuildingMeshAction(positionList, height, delegate(Mesh buildingMesh)
        {
            // 建物として大きすぎる場合はハズレとする。
            if (300.0f < buildingMesh.bounds.size.magnitude)
            {
                return;
            }

            var groundHeight = -1.0f;
            RaycastHit raycastHit;

            if (Physics.Raycast(new Vector3(
                                    buildingMesh.bounds.center.x,
                                    100,
                                    buildingMesh.bounds.center.z
                                ),
                                Vector3.down,
                                out raycastHit
                               ))
            {
                groundHeight = raycastHit.point.y + 0.1f;
            }

            GameObject obj = new GameObject(buildingData.Id);
            {
                var meshFilter = obj.AddComponent<MeshFilter>();
                var meshRenderer = obj.AddComponent<MeshRenderer>();
                //var meshCollider = obj.AddComponent<MeshCollider>();
                //meshCollider.sharedMesh = buildingMesh;
                meshFilter.sharedMesh = buildingMesh;
                meshRenderer.sharedMaterials = new Material[]
                {
                    Resources.Load<Material>("NineSlice/wide_mat"),
                };
                obj.transform.parent = parent.transform;
                Vector2 center = new Vector2(
                    positionList.Average((Vector2 arg) => arg.x),
                    positionList.Average((Vector2 arg) => arg.y)
                );
                obj.transform.position = new Vector3(
                    center.x,
                    groundHeight,
                    center.y
                );
            }
            const float levelHeight = 4.0f;
            var maxLevel = Mathf.CeilToInt(height / levelHeight);
            CreateBuildingMeshAlgorithms.CreateYaneMesh(positionList, maxLevel * levelHeight, delegate(Mesh yaneMesh)
            {
                yaneMesh.uv = yaneMesh.vertices.Select((position => new Vector2
                {
                    x = (position.x - yaneMesh.bounds.min.x) / (yaneMesh.bounds.max.x - yaneMesh.bounds.min.x),
                    y = (position.z - yaneMesh.bounds.min.z) / (yaneMesh.bounds.max.z - yaneMesh.bounds.min.z),
                })).ToArray();
                var yaneObj = new GameObject();
                yaneObj.name = buildingData.Id + "_yane";
                var meshFilter = yaneObj.AddComponent<MeshFilter>();
                var meshRenderer = yaneObj.AddComponent<MeshRenderer>();
                meshFilter.sharedMesh = yaneMesh;
                int one = new System.Random().Next(1, 10);
                var path = string.Format("FromCityEngine/Roofs/FlatMaterials/flatRoof_{0}", one);

                if (100.0f < yaneMesh.bounds.size.magnitude)
                {
                    path = "FromCityEngine/Roofs/FlatMaterials/big_flat";
                }

                meshRenderer.sharedMaterial = Resources.Load<Material>(path);
                yaneObj.transform.parent = obj.transform;
                yaneObj.transform.localPosition = Vector3.zero;
            });
        });
    }

    /// <summary>
    /// 建物データから高さを取得する.
    /// FIXME: 利用者からわかりやすくする.
    /// </summary>
    private static float GetHeight(BeefDefine.SchemaWrapper.BuildingDataModel buildingData)
    {
        try
        {
            if (buildingData.InfoList.FullInfoList.ContainsKey("height"))
            {
                var heightText = buildingData.InfoList.FullInfoList["height"];

                // ['] が入っている場合は フィート
                if (heightText.Contains('\''))
                {
                    // 最初にマッチした数字列のみを利用する.
                    var mc = System.Text.RegularExpressions.Regex.Match(heightText, "\\d+");

                    if (mc.Success)
                    {
                        var feet = float.Parse(mc.Value);
                        return feet * 3.3f;
                    }
                }

                // [m] が入っている場合は メートル
                if (heightText.Contains('m'))
                {
                    // 最初にマッチした数字列のみを利用する.
                    var mc = System.Text.RegularExpressions.Regex.Match(heightText, "\\d+");

                    if (mc.Success)
                    {
                        return float.Parse(mc.Value);
                    }
                }

                // デフォルトは メートル
                {
                    // 最初にマッチした数字列のみを利用する.
                    var mc = System.Text.RegularExpressions.Regex.Match(heightText, "\\d+");

                    if (mc.Success)
                    {
                        return float.Parse(mc.Value);
                    }
                }
            }

            if (buildingData.InfoList.FullInfoList.ContainsKey("building:levels"))
            {
                var levelText = buildingData.InfoList.FullInfoList["building:levels"];
                {
                    // 最初にマッチした数字列のみを利用する.
                    var mc = System.Text.RegularExpressions.Regex.Match(levelText, "\\d+");

                    if (mc.Success)
                    {
                        var level = int.Parse(mc.Value);
                        return level * 3.0f;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return (float)5.0;
    }

}
}
