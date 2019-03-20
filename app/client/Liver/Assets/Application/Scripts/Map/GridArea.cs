using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Map
{

// マップ上のオブジェクトをグリッド分割して配置する
public class GridArea : MonoBehaviour
{
    Transform holder;
    int layer;

    // グリッドサイズと距離は可変に
    public int _GridSize = 300;
    public int GridSize
    {

        private get
        {
            return this._GridSize;
        }

        set
        {
            this._GridSize = value;
            this.holder.name = $"GridArea{value}";
        }
    }

    struct Vector2Short
    {
        public short x;
        public short y;

        public Vector2Short(short x, short y)
        {
            this.x = x;
            this.y = y;
        }
        public static Vector2Short operator -(Vector2Short a, Vector2Short b)
        {
            return new Vector2Short((short)(a.x - b.x), (short)(a.y - b.y));
        }
        public static bool operator ==(Vector2Short a, Vector2Short b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Vector2Short a, Vector2Short b)
        {
            return !(a == b);
        }
        // Returns the length of this vector (RO).
        public float magnitude { get { return Mathf.Sqrt(x * x + y * y); } }
        // Returns the squared length of this vector (RO).
        public float sqrMagnitude { get { return x * x + y * y; } }

        public override string ToString() { return string.Format("({0}, {1})", x, y); }
    }

    public int _Distance = 2 * 2;
    public int Distance
    {

        private get
        {
            return this._Distance;
        }

        set
        {
            this._Distance = value * value;
        }
    }
    class Vector2EqualityComparer : IEqualityComparer<Vector2Short>
    {
        public bool Equals(Vector2Short a, Vector2Short b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public int GetHashCode(Vector2Short obj)
        {
            return (((int)obj.x) << 16) | (0xffff & obj.y);
        }
    }
    Dictionary<Vector2Short, Transform> grid = new Dictionary<Vector2Short, Transform>(new Vector2EqualityComparer());

    Vector2Short current = new Vector2Short(short.MaxValue, short.MinValue);

    public Transform AnimalTransform { private get; set; }


    void Awake()
    {
        // 親の階層を生成
        this.holder = new GameObject("GridArea").transform;
        this.holder.transform.SetParent(transform);
        // レイヤー
        this.layer = LayerMask.NameToLayer("Race");
    }

    void Update()
    {
        if (AnimalTransform == null) { return; }

        // 範囲外のオブジェクトの表示、非表示を切り替え
        var pos = AnimalTransform.position;
        var key = new Vector2Short((short)(pos.x / GridSize), (short)(pos.z / GridSize));

        if (current == key) { return; }

        foreach (var g in grid)
        {
            var flag = (g.Key - key).sqrMagnitude < this.Distance;

            if (g.Value.gameObject.activeSelf == flag) { continue; }

            g.Value.gameObject.SetActive(flag);
            return; // 負荷分散したいので、SetActive()は 1フレーム1回のみ実行する。
        }

        current = key;
    }

    // 追加
    public GameObject Add(GameObject prefab, Vector3 pos, Quaternion quaternion)
    {
        if (prefab.layer != this.layer)
        {
            prefab.SetLayerAllChildren(this.layer);
        }

        return Instantiate<GameObject>(prefab, pos, quaternion, GetGridParent(CalcGrid(pos), this.holder));
    }
    // 追加
    public GameObject Add(GameObject prefab, Vector3 pos)
    {
        return Add(prefab, pos, Quaternion.identity);
    }

    public T Component<T>(Vector3 pos) where T : Component
    {
        var p = GetGridParent(CalcGrid(pos), this.holder);
        var res = p.gameObject.GetComponent<T>();

        if (res == null) { res = p.gameObject.AddComponent<T>(); }

        return res;
    }

    // グリッド座標計算
    Vector2Short CalcGrid(Vector3 v)
    {
        return new Vector2Short((short)(v.x / this.GridSize), (short)(v.z / this.GridSize));
    }

    GameObject prefab;
    GameObject Prefab
    {
        get
        {
            if (prefab == null)
            {
                prefab = new GameObject("Grid", new[] { typeof(PoolEnliven) });
                prefab.SetActive(false);
            }

            return prefab;
        }
    }

    // グリッド配置用親オブジェクトを生成
    Transform GetGridParent(Vector2Short v, Transform parent)
    {
        Transform res;

        if (!grid.TryGetValue(v, out res))
        {
            var go = Instantiate(Prefab);
#if UNITY_EDITOR
            go.name = v.ToString();
#endif
            res = go.transform;
            res.SetParent(parent);
            grid.Add(v, res);
        }

        return res;
    }

    public void SetActive(bool active)
    {
        foreach (Transform t in this.holder.transform)
        {
            t.gameObject.SetActive(active);
        }
    }

}

}
