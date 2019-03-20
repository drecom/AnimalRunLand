using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FindGameObjects
{
    static void FindByName(Transform parent, string name, ref List<Transform> objs)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                objs.Add(child);
            }

            // 再帰的に検索
            if (child.childCount != 0)
            {
                FindByName(child, name, ref objs);
            }
        }
    }


    // 指定名のGameObjectを検索
    // NOTE rootは検索対象に含めない
    public static List<Transform> Process(GameObject root, string name)
    {
        List<Transform> objs = new List<Transform>();
        FindByName(root.transform, name, ref objs);
        return objs;
    }
}
