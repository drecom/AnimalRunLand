using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using BeefCreateModelScripts.Runtime;
using UnityEngine;

namespace BeefCreateModelScripts.Runtime
{

/// <summary>
/// 標高情報.
/// </summary>
public class GsiDemData
{
    public string FileID;
    public List<float> DemLists = new List<float>();
    public Vector2 LowerCorner;
    public Vector2 UpperCorner;
    public int XLen;
    public int YLen;
}

/// <summary>
/// 国土地理院の標高情報データから 標高情報を取得する.
/// </summary>
public static class GsiDemParser
{

    public static GsiDemData Parse(string path)
    {
        GsiDemData gsiDemData = new GsiDemData();
        Debug.Log(path);

        using (XmlReader reader = XmlReader.Create(path))
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.Name.Equals("fid"))
                        {
                            string fid = reader.ReadElementContentAsString();
                            string[] sepSplit = fid.Split('-');
                            gsiDemData.FileID = sepSplit[sepSplit.Length - 1];
                        }

                        if (reader.Name.Equals("gml:lowerCorner"))
                        {
                            gsiDemData.LowerCorner = ReadPos(reader);
                        }

                        if (reader.Name.Equals("gml:upperCorner"))
                        {
                            gsiDemData.UpperCorner = ReadPos(reader);
                        }

                        if (reader.Name.Equals("gml:high"))
                        {
                            ReadLimits(gsiDemData, reader);
                        }

                        if (reader.Name.Equals("gml:tupleList"))
                        {
                            ReadTupleList(gsiDemData, reader);
                        }

                        break;
                }
            }
        }

        Debug.LogFormat("X {0} Y {1} / D {2}", gsiDemData.XLen, gsiDemData.YLen, gsiDemData.DemLists.Count);
        return gsiDemData;
    }

    public static Vector2 ReadPos(XmlReader reader)
    {
        string posLine = reader.ReadElementContentAsString();
        string[] sep = posLine.Split(' ');
        return new Vector2(float.Parse(sep[1]), float.Parse(sep[0]));
    }

    private static void ReadLimits(GsiDemData gsiDemData, XmlReader reader)
    {
        string limitLine = reader.ReadElementContentAsString();
        string[] sep = limitLine.Split(' ');
        gsiDemData.XLen = int.Parse(sep[0]) + 1;
        gsiDemData.YLen = int.Parse(sep[1]) + 1;
    }

    private static void ReadTupleList(GsiDemData gsiDemData, XmlReader reader)
    {
        string[] posListLines = reader.ReadElementContentAsString().Split('\n');

        for (int i = 1; i < posListLines.Length - 1; i++)
        {
            string[] sep = posListLines[i].Split(',');
            float dem = float.Parse(sep[1]);

            if (dem < 0)
            {
                dem = 0.0f;
                // dem = gsiDemData.DemLists[gsiDemData.DemLists.Count - 1];
                // Debug.Log(string.Format("{0} {1}", i, posListLines[i]));
            }

            gsiDemData.DemLists.Add(dem);
        }
    }
}

/// <summary>
/// 標高情報を表現する.
/// </summary>
public static class CreateGroundScripts
{
    public static void CreateGroundMesh(string directoryPath, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        GameObject root = new GameObject("GroundRoot");
        root.transform.parent = parent.transform;
        string[] files = Directory.GetFiles(Path.Combine(Application.streamingAssetsPath, directoryPath))
                         .Where(s => !s.EndsWith(".meta"))
                         .ToArray();

        foreach (var file in files)
        {
            string gsiDemMapName = Path.Combine(directoryPath, file);
            string target = Path.Combine(Application.streamingAssetsPath, gsiDemMapName);
            GsiDemData gsiDemData = GsiDemParser.Parse(target);
            CreateGroundMeshByGsi(gsiDemData, root, worldCenter, worldScale);
        }
    }


    private static void CreateGroundMeshByGsi(GsiDemData gsiDemData, GameObject parent, Vector2 worldCenter, Vector2 worldScale)
    {
        float FileMeterDegree = ((gsiDemData.UpperCorner.y - gsiDemData.LowerCorner.y) / (gsiDemData.YLen - 1));
        Debug.Log(FileMeterDegree);
        List<Vector3> verticesList = new List<Vector3>();

        for (int y = 0; y < gsiDemData.YLen; y++)
        {
            for (int x = 0; x < gsiDemData.XLen; x++)
            {
                Vector2 pos = new Vector2
                {
                    y = gsiDemData.LowerCorner.y + FileMeterDegree * (gsiDemData.YLen - y),
                    x = gsiDemData.LowerCorner.x + FileMeterDegree * x,
                };
                float northLat = (pos.y - worldCenter.y) * worldScale.y;
                float eastLon = (pos.x - worldCenter.x) * worldScale.x;
                int index = y * gsiDemData.XLen + x;
                var height = -1.0f;

                if (index < gsiDemData.DemLists.Count)
                {
                    height = gsiDemData.DemLists[index];
                }

                verticesList.Add(
                    new Vector3(
                        eastLon,
                        height,
                        northLat)
                );
            }
        }

        List<int> trianglesList = new List<int>();

        for (int i = 0; i < verticesList.Count; i++)
        {
            if ((i % gsiDemData.XLen) == (gsiDemData.XLen - 1))
            {
                continue;
            }

            if (i + gsiDemData.XLen < verticesList.Count)
            {
                trianglesList.Add(i);
                trianglesList.Add(i + 1);
                trianglesList.Add(i + gsiDemData.XLen);
                trianglesList.Add(i + 1);
                trianglesList.Add(i + 1 + gsiDemData.XLen);
                trianglesList.Add(i + gsiDemData.XLen);
            }
        }

        GameObject obj = new GameObject("Ground" + gsiDemData.FileID);
        var meshFilter = obj.AddComponent<MeshFilter>();
        var meshRenderer = obj.AddComponent<MeshRenderer>();
        var meshCollider = obj.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        {
            mesh.vertices = verticesList.ToArray();
            mesh.triangles = trianglesList.ToArray();
            mesh.RecalculateNormals();
        }
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        meshRenderer.sharedMaterial = Resources.Load<Material>("Buildings/Materials/StandardOrange");
        obj.transform.parent = parent.transform;
    }
}
}
