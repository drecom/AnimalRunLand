using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PoolEnliven : MonoBehaviour
{
    class Info
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    Dictionary<GameObject, List<Info>> infos = new Dictionary<GameObject, List<Info>>();   // 表示座標一覧
    Dictionary<GameObject, List<GameObject>> objs = new Dictionary<GameObject, List<GameObject>>();  // 取得したオブジェクト一覧

    private void OnEnable()
    {
        foreach (var info in infos)
        {
            List<GameObject> list;

            if (!objs.TryGetValue(info.Key, out list))
            {
                list = new List<GameObject>();
            }

            foreach (var i in info.Value)
            {
                var obj = PrefabPool.Pop(info.Key);
                var t = obj.transform;
                t.SetParent(this.transform);
                t.position = i.position;
                t.rotation = i.rotation;
                list.Add(obj);
            }
        }
    }

    private void OnDisable()
    {
        foreach (var obj in objs)
        {
            foreach (var o in obj.Value)
            {
                PrefabPool.Push(obj.Key, o);
            }

            obj.Value.Clear();
        }
    }
    public void Add(GameObject prefab, Vector3 position)
    {
        Add(prefab, position, Quaternion.identity);
    }
    public void Add(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        List<Info> list;

        if (!infos.TryGetValue(prefab, out list))
        {
            list = new List<Info>();
            infos[prefab] = list;
        }

        list.Add(new Info { position = position, rotation = rotation });
    }
}
