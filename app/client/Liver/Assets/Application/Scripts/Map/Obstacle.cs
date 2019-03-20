using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Obstacle : MonoBehaviour
{
    // 配置予定プレハブ
    public GameObject[] Obstacles;

    static int? layer;

    void Awake()
    {
        if (!layer.HasValue)
        {
            layer = LayerMask.NameToLayer("Race");

            foreach (var o in Obstacles)
            {
                if (o.layer != layer)
                {
                    o.SetLayerAllChildren(layer.Value);
                }
            }
        }

        // 指定オブジェクトから１つを選んで子供とする
        var index = Random.Range(0, this.Obstacles.Length);
        Instantiate(this.Obstacles[index], transform, false);
    }
}
