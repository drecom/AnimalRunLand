using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace Map
{

// グリッド座標からいい感じのエリア名を導出する
public class AreaName
{
    static Dictionary<string, string> AreaInfo;


    // エリア名から住所っぽい文字列を取得
    public static void Get(double latitude, double longitude, System.Action<string> cb)
    {
        if (AreaInfo == null)
        {
            Debug.Log("Load geo data");
            AreaInfo = new Dictionary<string, string>();
            var lists = CsvReader.Read("Configs/geohash5", ',');

            foreach (var l in lists)
            {
                var key = l[0];
                var address = l[3] + l[4] + l[5] + l[6];
                AreaInfo.Add(key, address);
            }
        }

        // NOTE 住所の定まらない場合を先に決めとく
        var name = $"{latitude:#.###}, {longitude:#.###}";
        var hash = NGeoHash.GeoHash.Encode(latitude, longitude, 5);
        Debug.Log($"{latitude},{longitude} => {hash}");

        if (AreaInfo.ContainsKey(hash))
        {
            name = AreaInfo[hash];
        }

        // 取得完了時の処理
        if (cb != null) { cb(name); }
    }

    public static void Get(string areaName, System.Action<string> cb)
    {
        var t = areaName.Split('_');
        var loc = Map.LocationDeterminer.GridToLatLng(int.Parse(t[0]), int.Parse(t[1]));
        Get((double)loc.x, (double)loc.y, cb);
    }
}

}
