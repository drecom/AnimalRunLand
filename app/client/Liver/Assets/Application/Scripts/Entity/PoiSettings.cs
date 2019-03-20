using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Liver.Entity
{

[CreateAssetMenu(menuName = "Configs/PoiSettings")]
public class PoiSettings : ScriptableObject
{
    // 編集用の定義
    [System.Serializable]
    public class Pair
    {
        public string Name;
        public GameObject[] Prefab;
    }
    [Tooltip("POIタイプに紐付けるプレハブ")]
    public Pair[] CorrespondenceTable;

    [Tooltip("POIが上の一覧に属していなかった場合に紐付けるプレハブ")]
    public GameObject Unknown;


    // 検索用の定義
    Dictionary<string, GameObject[]> table = new Dictionary<string, GameObject[]>();


    void Awake()
    {
        // 検索しやすい形式にする
        foreach (var pair in this.CorrespondenceTable)
        {
            this.table.Add(pair.Name, pair.Prefab);
        }
    }


    // 識別子からGameObjectを取得
    public GameObject Get(string key)
    {
        GameObject[] obj;

        if (this.table.TryGetValue(key, out obj))
        {
            return obj[Random.Range(0, obj.Length)];
        }

        return this.Unknown;
    }
}

}
