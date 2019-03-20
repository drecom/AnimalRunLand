using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireFlower : MonoBehaviour
{
    // 適当な数
    public int Num = 100;

    public GameObject Prefab;
    public Vector2 Height = new Vector2(50, 50);

    static int? layer;

    public Camera RaceCamera;


    private void Awake()
    {
        // Layer を設定する
        if (!layer.HasValue)
        {
            layer = LayerMask.NameToLayer("Race");

            if (this.Prefab.layer != layer)
            {
                this.Prefab.SetLayerAllChildren(layer.Value);
            }
        }
    }

    // NOTE エリアを画像化したあとで生成
    public void Setup()
    {
        // 賑やかしを配置する階層を生成
        var holder = new GameObject("BackGround").transform;
        holder.SetParent(transform, true);
        var half = Map.LocationDeterminer.GridSizeKilometer * 0.5;

        for (int i = 0; i < this.Num; ++i)
        {
            var pos = new Vector3();
            pos.x = Random.Range((float)(-half), (float)(half));
            pos.y = Random.Range(this.Height.x, this.Height.y);
            pos.z = Random.Range((float)(-half), (float)(half));
            var child = Instantiate(this.Prefab, holder, false);
            child.transform.localPosition += pos;
            var anim = child.GetComponentInChildren<Animator>();

            if (anim)
            {
                anim.Play(0, -1, Random.Range(0.0f, 1.0f));
            }

            var c = child.AddComponent<TowardCamera>();
            c.raceCamera = this.RaceCamera;
        }
    }
}
