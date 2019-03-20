using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BeefMain.Runtime;


namespace Map
{

public class ItemLayoutScene : MonoBehaviour
{
    public float ItemDensity = 1;

    // 配置用の乱数は内部で保持
    // NOTE いつも同じ結果が欲しい
    System.Random random = new System.Random(1234);


    //{
    //    texture.Apply();
    //    Sprite sprite = Sprite.Create(texture,
    //                                  new Rect(0, 0, texture.width, texture.height),
    //                                  new Vector2(0.5f, 0.5f));
    //    var comp = gameObject.GetComponent<SpriteRenderer>();
    //    comp.sprite = sprite;
    //}


    public IEnumerator Process(RoadInfo roadInfo, GridArea gridArea)
    {
        Debug.Log("ItemLayoutScene.Process");
        // Itemを置けそうな場所にオブジェクトを配置
        var builder = GetComponent<ItemBuilder>();
        int num = 0;
        var half = Map.LocationDeterminer.GridSizeKilometer * 0.5f;
        float goNextFrameTime = Time.realtimeSinceStartup + 0.03f;

        foreach (var road in roadInfo.Roads)
        {
            // 道の長さを計算
            float l = road.TotalLength;

            // 交差点の前後5mにはアイテムを置かない
            if (l < 36) { continue; }

            float d = 18;

            while (l > (d + 18))
            {
                // なるべくまっすぐな直線上に配置
                if (RoadInfo.CheckVertex(road, d, 20))
                {
                    if (this.random.NextDouble() < this.ItemDensity)
                    {
                        var p = RoadInfo.CalcPosition(road, d);
                        var v = RoadInfo.CalcVector(road, d);

                        // 3kmグリッド内か判定
                        if (Mathf.Abs(p.x) > half || Mathf.Abs(p.z) > half)
                        {
                            continue;
                        }

                        var obj = builder.EnterItem(this.random);
                        var item = gridArea.Add(obj, p, Quaternion.LookRotation(v));
                        item.GetComponent<ItemBase>().CopyForm(obj.GetComponent<ItemBase>());
                        item.SetActive(true);
                        num += 1;
                    }
                }

                d += 20;
            }

            // 一定時間ごとにyield
            if (Time.realtimeSinceStartup > goNextFrameTime)
            {
                yield return null;
                goNextFrameTime = Time.realtimeSinceStartup + 0.03f;
            }
        }

        Debug.Log($"Item num: {num}");
        // 用済みなので削除
        Destroy(builder);
    }
}

}
