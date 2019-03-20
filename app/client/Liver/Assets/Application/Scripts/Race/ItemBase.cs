using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;

public class ItemBase : MonoBehaviour
{
    public static Action<MeshSwitcher> OnMeshSwitcher;

    public struct Info
    {
        public Vector3 position;
        public GameObject effect;
    }

    Dictionary<GameObject, List<Info>> infos = new Dictionary<GameObject, List<Info>>();      // 配置情報
    Dictionary<GameObject, List<GameObject>> objs = new Dictionary<GameObject, List<GameObject>>(); // Pool から取得されたオブジェクト一覧

    private void OnEnable()
    {
        foreach (var obj in objs)
        {
            Assert.AreEqual(obj.Value.Count, 0);
        }

        foreach (var info in infos)
        {
            foreach (var i in info.Value)
            {
                var go = PrefabPool.Pop(info.Key, (o) =>
                {
                    // エフェクトを追加する
                    if (i.effect != null) { Instantiate(i.effect, o.transform); }

                    foreach (var c in o.GetComponentsInChildren<MeshSwitcher>())
                    {
                        OnMeshSwitcher(c);
                    }
                });
                // 座標をリセット
                go.transform.SetParent(null);
                go.transform.position = info.Key.transform.position;
                go.transform.rotation = info.Key.transform.rotation;
                // 実際の表示情報を設定する
                go.transform.SetParent(transform, false);
                go.transform.localPosition += i.position;
                // 影
                var shadowInstance = go.transform.Find("Shadow");

                if (shadowInstance != null)
                {
                    var p = go.transform.localPosition.y;
                    shadowInstance.transform.localPosition = new Vector3(0, -p + 0.1f, 0);           // NOTE z-fighting防止策
                }

                objs[info.Key].Add(go);
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

    public void Add(GameObject prefab, Vector3 position, GameObject effect = null)
    {
        List<Info> list;

        if (!infos.TryGetValue(prefab, out list))
        {
            list = new List<Info>();
            infos[prefab] = list;
        }

        list.Add(new Info { position = position, effect = effect });
    }

    public void CopyForm(ItemBase obj)
    {
        this.infos = new Dictionary<GameObject, List<Info>>();

        foreach (var info in obj.infos)
        {
            // コピーする
            this.infos[info.Key] = new List<Info>(info.Value);
            // インスタンスを保持用リストを作成しておく
            this.objs[info.Key] = new List<GameObject>(info.Value.Count);
        }
    }

    public void WillDestroy(GameObject go)
    {
        foreach (var obj in objs)
        {
            var i = obj.Value.IndexOf(go);

            if (i == -1) { continue; }

            infos[obj.Key].RemoveAt(i);
            obj.Value.RemoveAt(i);
        }

        //go.transform;
    }
}
