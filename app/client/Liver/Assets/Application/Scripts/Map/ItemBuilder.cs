using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ItemBuilder : MonoBehaviour
{
    // 配置するオブジェクトの一覧
    // テキストファイルではABCDE...と対応する
    public GameObject[] ItemObject;

    // アイテムを並べる長さ
    public float Length = 20;

    // レーンの高さ
    public float Height = 2;

    // 雛形
    public GameObject TemplatePattern;

    // 生成したパーツ群
    class Pattern
    {
        public GameObject Object;
        public int AppearanceRate;
    }
    List<Pattern> itemPatterns;
    int TotalRate;

    // アイテム出現テキストと出現率
    public ItemPatternInfo PatternInfo;

    static int? layer;

    static GameObject itemEffectPrefab;

    void Awake()
    {
        if (itemEffectPrefab == null)
        {
            itemEffectPrefab = Resources.Load<GameObject>("Effects/ef_010");
        }

        // Layer を設定する : 最初の一回のみ実行されます
        if (!layer.HasValue)
        {
            layer = LayerMask.NameToLayer("Race");

            foreach (var item in this.ItemObject)
            {
                if (item.layer != layer)
                {
                    item.SetLayerAllChildren(layer.Value);
                }
            }

            if (itemEffectPrefab.layer != layer)
            {
                itemEffectPrefab.SetLayerAllChildren(layer.Value);
            }
        }

        this.itemPatterns = new List<Pattern>();
        this.TotalRate = 0;
        int i = 0;

        foreach (var p in this.PatternInfo.Patterns)
        {
            var obj = Build(p.Pattern);
            obj.name = $"Item{i:00}";
            ++i;
            this.TotalRate += GetRate(p, Liver.RaceWindow.AnimalKind);
            var temp = new Pattern { Object = obj, AppearanceRate = this.TotalRate };
            this.itemPatterns.Add(temp);
        }
    }


    // アイテム出現率を取得
    int GetRate(ItemPatternInfo.Body body, AnimalKind kind)
    {
        // アニマルカー別に拡張された確率があるか探す
        foreach (var extra in body.ExtraRate)
        {
            if (extra.Kind == kind)
            {
                return extra.Rate;
            }
        }

        return body.Rate;
    }


    // 出現率に応じたアイテムを選択
    public GameObject EnterItem(System.Random random)
    {
        var rate = random.Next(0, this.TotalRate);

        foreach (var pattern in this.itemPatterns)
        {
            if (rate < pattern.AppearanceRate)
            {
                return pattern.Object;
            }
        }

        // NOTE まずここには到達しない
        return this.itemPatterns[this.itemPatterns.Count - 1].Object;
    }


    void OnDestroy()
    {
        // 雛形を削除
        foreach (var p in this.itemPatterns)
        {
            Destroy(p.Object);
        }
    }


    // テキストファイルを読み込んでアイテムを配置する
    GameObject Build(TextAsset textAsset)
    {
        var root = Instantiate(TemplatePattern, transform).transform;
        root.gameObject.SetActive(false);
        var itembase = root.gameObject.GetComponent<ItemBase>();
        // 空白は無視しつつ改行で分割
        var text  = textAsset.text.Replace(" ", "");
        var lines = text.Split('\n');
        Vector3[] tbl =
        {
            new Vector3(-1.8f, 0, 0),
            new Vector3(0.0f, 0, 0),
            new Vector3(1.8f, 0, 0),

            new Vector3(-1.8f, this.Height, 0),
            new Vector3(0.0f, this.Height, 0),
            new Vector3(1.8f, this.Height, 0),

            new Vector3(-1.8f, this.Height * 2, 0),
            new Vector3(0.0f, this.Height * 2, 0),
            new Vector3(1.8f, this.Height * 2, 0),
        };
        int num = Mathf.Min(lines.Length, 12);

        for (int i = 0; i < num; ++i)
        {
            // NOTE \r\n に対処
            var t = lines[i].TrimEnd();
            var pos = tbl[i];
            pos.z = -this.Length / 2.0f;
            var space = this.Length / (t.Length + 1);

            foreach (var kind in t)
            {
                pos.z += space;

                if (kind == '.') { continue; }

                //Debug.Log(kind);
                var index = kind.CompareTo('A');
                // スターや障害物の使いまわし対応
                itembase.Add(this.ItemObject[index], pos, index >= 9 ? itemEffectPrefab : null);
            }
        }

        return root.gameObject;
    }
}
