using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Extension;

namespace Map
{

public class FenceLayouter : MonoBehaviour
{
    // 柵のプレハブ
    public GameObject FenceAll;     // 両方
    public GameObject FenceLeft;    // 左側のみ
    public GameObject FenceRight;   // 右側のみ

    public IEnumerator Process(RoadInfo roadInfo, GridArea gridArea, Liver.Entity.AreaMap areaMap)
    {
        if (FenceAll == null || FenceLeft == null || FenceRight == null)
        {
            yield break;
        }

        Debug.Log("FenceLayouter.Process");
        // ピクセルを配列でゲット
        var num = (int)LocationDeterminer.GridSizeKilometer;
        var offset = new Vector3(num * -0.5f, 0, num * -0.5f);
        var dummy = Instantiate<GameObject>(this.FenceAll).transform;
        var left = dummy.Find("left");
        var right = dummy.Find("right");
        var vFront = new Vector3(0, 0, -2);
        var vBack  = new Vector3(0, 0, 2);
        var half = Map.LocationDeterminer.GridSizeKilometer * 0.5f;
        float goNextFrameTime = Time.realtimeSinceStartup + 0.03f;

        foreach (var road in roadInfo.Roads)
        {
            // 道の長さを計算
            float l = road.TotalLength;

            // 交差点の前後5mには置かない
            if (l < (3 + 6 + 3) * 2) { continue; }

            float d = 3 + 6 + 3;

            while (l > (d + 3 + 6 + 3))
            {
                if (Random.Range(0.0f, 1.0f) < 0.5f)
                {
                    var p = RoadInfo.CalcPosition(road, d);
                    var v = RoadInfo.CalcVector(road, d);

                    // 3kmグリッド内か判定
                    if (Mathf.Abs(p.x) > half || Mathf.Abs(p.z) > half)
                    {
                        d += 8.0f;
                        continue;
                    }

                    if (RoadInfo.CheckVertex(road, d, 16, 0.99f))
                    {
                        var q = Quaternion.LookRotation(v); // 向きを計算する
                        // 計算のため、ダミーの座標と向きを設定する
                        dummy.localPosition = p;
                        dummy.localRotation = q;
                        byte bit = 0;

                        // テクスチャ画像上での色を調査
                        if (IsEmptyField(left.position, areaMap, offset)
                                && IsEmptyField(left.position + q * vFront, areaMap, offset)
                                && IsEmptyField(left.position + q * vBack, areaMap, offset)) { bit |= 1 << 0; } // 左

                        if (IsEmptyField(right.position, areaMap, offset)
                                && IsEmptyField(right.position + q * vFront, areaMap, offset)
                                && IsEmptyField(right.position + q * vBack, areaMap, offset)) { bit |= 1 << 1; } // 右

                        switch (bit)
                        {
                            // 左のみ
                            case 1:
                                gridArea.Component<PoolEnliven>(p).Add(this.FenceLeft, p, q);
                                break;

                            // 右のみ
                            case 2:
                                gridArea.Component<PoolEnliven>(p).Add(this.FenceRight, p, q);
                                break;

                            // 両方
                            case 3:
                                gridArea.Component<PoolEnliven>(p).Add(this.FenceAll, p, q);
                                break;
                        }
                    }
                }

                d += 8.0f;
            }

            // 一定時間ごとにyield
            if (Time.realtimeSinceStartup > goNextFrameTime)
            {
                yield return null;
                goNextFrameTime = Time.realtimeSinceStartup + 0.03f;
            }
        }

        Destroy(dummy.gameObject);
        //texture.SetPixels32(colors);
    }

    //static Color white = Color.white;
    // その場所が空き地かどうか調べる
    static public bool IsEmptyField(Vector3 pos, Liver.Entity.AreaMap areaMap, Vector3 offset)
    {
        // 画像上の位置
        var tpos = pos - offset;
        //Debug.Log($"{pos.x}, {pos.z} -> {tpos.x}, {tpos.z}");
        int x = (int)tpos.x;
        int y = (int)tpos.z;

        if (!areaMap.Contain(x, y)) { return false; }

        var color = areaMap.GetPixel(x, y);
        return !(color.g > 0);
        //return ExtendedColor.FastEqual(ref color, ref white);
    }
}
}

