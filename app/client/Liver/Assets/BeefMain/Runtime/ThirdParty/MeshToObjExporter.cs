using System.IO;
using System.Text;
using UnityEngine;

namespace BeefMain.MeshToObjExporter
{
public class MeshToObjExporter
{
    public static string MeshToString(Mesh mesh, string name)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("g ").Append(name).Append("\n");

        foreach (Vector3 v in mesh.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }

        sb.Append("\n");

        foreach (Vector3 v in mesh.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }

        sb.Append("\n");

        foreach (Vector3 v in mesh.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }

        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                    triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
        }

        return sb.ToString();
    }

    public static void MeshToFile(Mesh mesh, string filename)
    {
        string name = Path.GetFileNameWithoutExtension(filename);

        using (StreamWriter sw = new StreamWriter(filename))
        {
            sw.Write(MeshToString(mesh, name));
        }
    }
}
}
