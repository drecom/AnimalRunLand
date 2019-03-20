using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tsl.Entity
{
public class MapTestData
{
    public string PointName { get; set; }  // テスト用ジャンプポイント箇所名
    public double Latitude { get; set; }    // 緯度
    public double Longitude { get; set; }   // 経度
}

public class MapTestDatarOperate
{
    public List<MapTestData> userData = new List<MapTestData>();

    public List<MapTestData> data
    {
        get
        {
            return this.userData;
        }
    }

    private string GetFilename()
    {
        return "map_test_data";
    }

    public void LoadState(Action onEnd = null)
    {
        var textAsset = Resources.Load("Race/" + GetFilename()) as TextAsset;
        Serializer.Deserialize(textAsset.text, out this.userData);

        if (onEnd != null) { onEnd(); }
    }
}
}
