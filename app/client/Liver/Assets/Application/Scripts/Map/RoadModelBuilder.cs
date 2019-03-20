using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeefCreateModelScripts.Runtime;


namespace Map
{
// 道のMeshを生成して階層に追加する用途
public class RoadModelBuilder
{
    Transform parentTransform;


    public RoadModelBuilder(Transform parent)
    {
        this.parentTransform = parent;
    }


    public void CreateMeshGameObject(CreateRoadMeshScripts.MeshSet meshset, string objName, string materialName)
    {
        var materials = new Dictionary<string, string>()
        {
            { "TileMaterial",      "Models/Background/Materials/RoadMaterial" },
            { "CurveTileMaterial", "Models/Background/Materials/CrossRoadMaterial" },
            { "CrossTileMaterial", "Models/Background/Materials/CrossRoadMaterial" },
        };
        var obj = new GameObject(objName);
        obj.transform.parent = this.parentTransform;
        var meshFilter = obj.AddComponent<MeshFilter>();
        var mesh = meshset.CreateMesh();
        meshFilter.sharedMesh = mesh;
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        meshRenderer.lightProbeUsage            = UnityEngine.Rendering.LightProbeUsage.Off;
        meshRenderer.reflectionProbeUsage       = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        meshRenderer.shadowCastingMode          = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows             = false;
        meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        var id = materials[materialName] + Liver.RaceWindow.RoadMaterial.ToString();
        meshRenderer.sharedMaterial = Resources.Load<Material>(id);
    }
}
}
