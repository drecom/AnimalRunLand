using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ParticleGen : MonoBehaviour
{
    static ParticleGen _Instance;
    public static ParticleGen Instance
    {
        get
        {
            if (_Instance == null)
            {
                var go = new GameObject(typeof(ParticleGen).Name);
                GameObject.DontDestroyOnLoad(go);
                _Instance = go.AddComponent<ParticleGen>();
            }

            return _Instance;
        }
    }

    /// <summary>
    /// Resources.Load(...) からロードされたオブジェクトのキャッシュ
    /// </summary>
    Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    /// <summary>
    /// 再生完了したエフェクトを保持し使いまわす!
    /// </summary>
    Dictionary<string, Stack<GameObject>> pools = new Dictionary<string, Stack<GameObject>>();

    /// <summary>
    /// 再生中エフェクト一覧
    /// </summary>
    List<Data> effects = new List<Data>();

    class Data
    {
        public GameObject go;
        ParticleSystem[] ps;

        public Data(GameObject go)
        {
            this.go = go;
            ps = go.GetComponentsInChildren<ParticleSystem>();
        }

        /// <summary>
        /// 再生中かどうか
        /// </summary>
        /// <returns></returns>
        public bool IsPlaying()
        {
            if (go == null) { return false; }

            return !ps.All(d => d.isStopped);
        }
    }

    /// <summary>
    /// 1回のみ再生、再生完了したら自動的プールに返します
    /// </summary>
    /// <param name="fn"></param>
    /// <param name="parent"></param>
    public void PlayOnce(string fn, Transform parent)
    {
        var go = Pop(fn);
        go.transform.SetParent(parent, false);
        effects.Add(new Data(go));
    }

    public void PlayOnce(string fn, Transform parent, Vector3 pos)
    {
        var go = Pop(fn);
        go.transform.position = pos;
        go.transform.SetParent(parent, false);
        effects.Add(new Data(go));
    }

    /// <summary>
    /// エフェクトを再生する
    /// いらなくなったら、Push() に介してプールに返すと使いまわせる
    /// </summary>
    /// <param name="fn"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    public GameObject Play(string fn, Transform parent)
    {
        var go = Pop(fn);
        go.transform.SetParent(parent, false);
        return go;
    }

    /// <summary>
    /// Poolから取得する、なければ生成します
    /// </summary>
    /// <param name="fn"></param>
    /// <returns></returns>
    public GameObject Pop(string fn)
    {
        GameObject res = null;
        Stack<GameObject> stack;

        if (pools.TryGetValue(fn, out stack))
        {
            while (stack.Count > 0)
            {
                res = stack.Pop();

                if (res != null) { break; }
            }
        }

        if (res == null) { res = Instantiate(fn); }

        res.SetActive(true);
        res.name = fn;
        return res;
    }

    /// <summary>
    /// Pool に追加
    /// </summary>
    /// <param name="go"></param>
    public void Push(GameObject go)
    {
        if (go == null) { return; } // nullの場合無視します!!

        if (!pools.ContainsKey(go.name))
        {
            pools.Add(go.name, new Stack<GameObject>());
        }

        go.transform.SetParent(this.transform, false);
        go.SetActive(false);
        pools[go.name].Push(go);
    }

    public void Clear(bool clearCache = false)
    {
        // とりあえずすべてプールに返す
        foreach (var e in effects) { Push(e.go); }

        effects.Clear();

        if (clearCache)
        {
            foreach (var stack in pools)
            {
                while (stack.Value.Count > 0)
                {
                    GameObject.Destroy(stack.Value.Pop());
                }
            }

            pools.Clear();
            prefabs.Clear();
        }
    }
    GameObject Instantiate(string fn)
    {
        GameObject prefab;

        if (!prefabs.TryGetValue(fn, out prefab))
        {
            prefab = Resources.Load<GameObject>(fn);
            prefabs.Add(fn, prefab);
        }

        return Instantiate(prefab);
    }

    private void Update()
    {
        for (int i = effects.Count - 1; i >= 0; i--)
        {
            if (!effects[i].IsPlaying())
            {
                Push(effects[i].go);
                effects.RemoveAt(i);
            }
        }
    }
}
