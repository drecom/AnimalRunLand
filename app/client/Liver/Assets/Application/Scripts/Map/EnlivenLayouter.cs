using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Tsl.Entity;
using Util.Extension;
using System.IO;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Map
{
public class EnlivenLayouter : MonoBehaviour
{
    //Color white = Color.white;

    [System.Serializable]
    public class EnlivenData
    {
        public GameObject prefab;
        public int size; // 半径
    }
    public EnlivenData[] enlivens;
    int maxSize;

#if false
    // 賑やかしオブジェクト
    public GameObject[] Prefab;
#endif

    // 密度
    public float Density = 0.1f;

    private void Awake()
    {
        System.Array.Sort(enlivens, (a, b) =>
        {
            return a.size.CompareTo(b.size);
        });

        if (enlivens.Length == 0)
        {
            maxSize = 0;
            return;
        }

        maxSize = enlivens.Last().size;
    }
    public int EmptyAreaSize(Liver.Entity.AreaMap areaMap, int x, int y, int step, int max)
    {
        int current = step;

        while (current < max)
        {
            // 四方向調べる
            // NOTE 建物が四角いので４端を調査
            if (!areaMap.Contain(x - current, y - current)) { return current - step; }

            if (!areaMap.Contain(x - current, y + current)) { return current - step; }

            if (!areaMap.Contain(x + current, y - current)) { return current - step; }

            if (!areaMap.Contain(x + current, y + current)) { return current - step; }

            var leftup = areaMap.GetPixel(x - current, y - current);

            if ((leftup.r > 0)) { return current - step; }

            //if (!ExtendedColor.FastEqual(ref up, ref white)) return current - step;
            var leftdown = areaMap.GetPixel(x - current, y + current);

            if ((leftdown.r > 0)) { return current - step; }

            //if (!ExtendedColor.FastEqual(ref down, ref white)) return current - step;
            var rightup = areaMap.GetPixel(x + current, y - current);

            if ((rightup.r > 0)) { return current - step; }

            //if (!ExtendedColor.FastEqual(ref left, ref white)) return current - step;
            var rightdown = areaMap.GetPixel(x + current, y + current);

            if ((rightdown.r > 0)) { return current - step; }

            //if (!ExtendedColor.FastEqual(ref right, ref white)) return current - step;
            current += step;
        }

        return max;
    }

    // 生成
    public IEnumerator Process(GridArea gridArea, Liver.Entity.AreaMap areaMap)
    {
        Debug.Log("EnlivenLayouter.Process");

        if (enlivens.Length == 0)
        {
            yield break;
        }

        // テキストから生成
        var enliven = Enliven.Load();
        yield return null;
        var num = (int)LocationDeterminer.GridSizeKilometer;
        var offset = new Vector3(num * -0.5f, 0, num * -0.5f);
        int x, y, size;
        float goNextFrameTime = Time.realtimeSinceStartup + 0.03f;

        for (int i = 0; i < enliven.Count; i++)
        {
            enliven.Unpack(i, out x, out y, out size);
            var color = areaMap.GetPixel(x, y);

            if (color.r > 0) { continue; }

            //if (!ExtendedColor.FastEqual(ref color, ref white)) continue;
            var emptySize = Mathf.Max(1, EmptyAreaSize(areaMap, x, y, 1, Mathf.Min(size, maxSize)));
#if false
            // 空きスペース以下のプレハブをランダムで抽選する
            var count = System.Array.FindLastIndex(enlivens, (e) =>
            {
                return e.size <= emptySize;
            });
            var prefab = enlivens[Random.Range(0, count + 1)].prefab;
#else
            GameObject prefab = null;
            // なるべく大きいオブジェクトを配置する
            // MEMO : サイズ1以上のものが複数がある場合、後ろのほうが抽選されます。（現時点対応必要ないため放置します。
            int count = enlivens.Length - 1;

            for (int j = enlivens.Length - 1; j >= 0; j--)
            {
                if (enlivens[j].size <= emptySize)
                {
                    count = j;
                    break;
                }
            }

            if (enlivens[count].size != 1)
            {
                prefab = enlivens[count].prefab;
            }
            else
            {
                prefab = enlivens[Random.Range(0, count + 1)].prefab;
            }

#endif
            var p = new Vector3(x + offset.x, 0 + offset.y, y + offset.z);
            var c = gridArea.Component<PoolEnliven>(p);
            c.Add(prefab, p);

            // 一定時間ごとにyield
            if (Time.realtimeSinceStartup > goNextFrameTime)
            {
                yield return null;
                goNextFrameTime = Time.realtimeSinceStartup + 0.03f;
            }
        }
    }

#if UNITY_EDITOR
    // 配置データを生成する
    [MenuItem("Enliven/Gen")]
    public static void CreateData()
    {
        // 設定された密度を取得
        var area = Resources.Load<GameObject>("Race/RaceArea");
        var density = area.GetComponent<EnlivenLayouter>().Density;
        var min = area.GetComponent<EnlivenLayouter>().enlivens.Where(e => e.size > 1).Min(e => e.size);   // サイズ１を除き、最小の賑やかしサイズを調べる
        // 座標だけ生成
        var enliven = new Enliven();
        var num = (int)LocationDeterminer.GridSizeKilometer;
        UnityEngine.Assertions.Assert.IsTrue(num < 0xFFF); /// ビットでデータ保持するため、x / y は 0xFFF 以下と制限します
        var body = new List<Vector2Int>();
        var size = new List<int>();

        for (int x = 0; x < num; ++x)
        {
            for (int z = 0; z < num; ++z)
            {
                if (Random.Range(0.0f, 1.0f) > density) { continue; }

                body.Add(new Vector2Int(x, z));
            }
        }

        for (int i = 0; i < body.Count; i++)
        {
            var s = float.MaxValue;

            if (EditorUtility.DisplayCancelableProgressBar("距離を計算中", $"{i} / {body.Count}", i / (float)body.Count))
            {
                EditorUtility.ClearProgressBar();
                Debug.Log("キャンセルされたため、賑やかしデータは更新されません");
                return;
            }

            for (int j = 0; j < body.Count; j++)
            {
                if (i == j) { continue; }

                s = Mathf.Min(s, (body[i] - body[j]).magnitude);

                if (s < min)
                {
                    s = 1;
                    break;   // これ以上調べません
                }
            }

            s = Mathf.Min(s, (float)0xFF); // サイズは 255 以上にならないようにします。
            size.Add((int)Mathf.Floor(s));
        }

        EditorUtility.ClearProgressBar();

        using (var ms = new MemoryStream())
        {
            using (var bw = new BinaryWriter(ms))
            {
                var count = body.Count;

                for (int i = 0; i < count; i++)
                {
                    bw.Write(Enliven.Pack(body[i].x, body[i].y, size[i]));
                }

                bw.Write(count);
            }

            enliven.data = ms.ToArray();
        }

        enliven.Save();
    }

#endif
}

}
