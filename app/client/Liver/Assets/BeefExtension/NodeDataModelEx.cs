using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

/// <summary>
/// NodeDataModelの拡張メソッド
/// </summary>
namespace BeefDefine.SchemaWrapper
{
public static class NodeDataModelEx
{
    /// <summary>
    /// POIの種類を取得する
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static string Type(this NodeDataModel @this)
    {
        string id;

        if (@this.InfoList.TryGetValue("keyword", out id))
        {
            return id;
        }
        else
        {
            return "Unknown";
        }
    }
    /// <summary>
    /// POI
    /// </summary>
    public static bool IsPOI(this NodeDataModel @this)
    {
        var Id = @this.Id;
        var start = Id.IndexOf('_');

        if (start == -1) { return false; }

        var end = Id.IndexOf('_', start + 1);

        if (end == -1) { return false; }

        return String.CompareOrdinal(Id, start, "_position_", 0, "_position_".Length) == 0;
    }

    /// <summary>
    /// Gate
    /// </summary>
    public static bool IsGate(this NodeDataModel @this)
    {
        var Id = @this.Id;
        var start = Id.IndexOf('_');

        if (start == -1) { return false; }

        var end = Id.IndexOf('_', start + 1);

        if (end == -1) { return false; }

        return String.CompareOrdinal(Id, start, "_nearRoad_", 0, "_nearRoad_".Length) == 0;
    }

    /// <summary>
    /// Station
    /// </summary>
    public static bool IsStation(this NodeDataModel @this)
    {
        // public_transport=station か railway=station のどちらかが設定という判定で鉄道駅
        string transport, railway;

        if (@this.InfoList.TryGetValue("railway", out railway))
        {
            return (string.CompareOrdinal(railway, "station") == 0);
        }

        if (@this.InfoList.TryGetValue("public_transport", out transport))
        {
            return (string.CompareOrdinal(transport, "station") == 0);
        }

        return false;
    }

    /// <summary>
    /// 名前取得
    /// </summary>
    public static string Name(this NodeDataModel @this)
    {
        string name;

        if (@this.InfoList.TryGetValue("name", out name))
        {
            return name;
        }
        else
        {
            var index = @this.Id.IndexOf('_');

            if (index != -1) { return @this.Id.Substring(0, index); }
            else { return @this.Id; }
        }
    }

    static StringBuilder uniqueIdBuilder = new StringBuilder();
    // Name + Hash値
    public static string UniqueId(this NodeDataModel @this)
    {
        uniqueIdBuilder.Clear();
        var id = @this.Id;
        var s = id.IndexOf('_');
        var e = id.LastIndexOf('_');
        uniqueIdBuilder.Append(id, 0, s);
        uniqueIdBuilder.Append(id, e, id.Length - e);
        return uniqueIdBuilder.ToString();
    }

    /// <summary>
    /// 向きを取得 : 道に対して正面の情報を取得する
    /// </summary>
    /// <param name="this"></param>
    /// <returns></returns>
    public static Quaternion Rotation(this NodeDataModel @this)
    {
        float x, z;
        float.TryParse(@this.InfoList.GetValue("direction_x"), out x);
        float.TryParse(@this.InfoList.GetValue("direction_z"), out z);
        return Quaternion.LookRotation(new Vector3(x, 0, z));
    }

    /// <summary>
    /// 実際のワールド座標
    /// </summary>
    /// <param name="this"></param>
    /// <param name="worldCenter"></param>
    /// <param name="worldScale"></param>
    /// <returns></returns>
    public static Vector3 WorldPostiton(this NodeDataModel @this, Vector2 worldCenter, Vector2 worldScale)
    {
        PositionModel positionModel = @this.Position;
        var position = new Vector3(
            (positionModel.EastLon - worldCenter.x) * worldScale.x,
            0,
            (positionModel.NorthLat - worldCenter.y) * worldScale.y
        );
        return position;
    }
}

public static class InfoListModelEx
{
    public static bool TryGetValue(this InfoListModel @this, string key, out string value)
    {
        var kv = @this.dictionary.KeyValueListByKey(key);

        if (kv.HasValue)
        {
            value = kv.Value.StringValue;
            return true;
        }
        else
        {
            value = default(string);
            return false;
        }
    }
}
}
