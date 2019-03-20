// このコードは、YAMLのマスターデータ仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masterdata.Test
{
public class RelationDataJson
{
    public int id;
    public string name;
    public int value;
}

public partial class RelationData
{
    public RelationData() {}

    RelationDataJson json;
    RelationData(RelationDataJson json)
    {
        this.json = json;
    }

    public int Id { get { return json.id; } set { json.id = value; } }
    public string Name { get { return json.name; } set { json.name = value; } }
    public int Value { get { return json.value; } set { json.value = value; } }

    static RelationData[] staticArray;
    static Dictionary<int, RelationData> staticDict;

    public static void Load(string inputJson)
    {
        var json_types = LitJson.JsonMapper.ToObject<RelationDataJson[]>(inputJson);
        staticArray = json_types.Select(x => new RelationData(x)).ToArray();
        staticDict = staticArray.ToDictionary(x => x.Id);
    }

    public static RelationData Get(int id)
    {
        return RelationData.staticDict[id];
    }
    public static RelationData[] All()
    {
        return RelationData.staticArray;
    }

    public static RelationData FromJson(RelationDataJson json)
    {
        return new RelationData(json);
    }

    public RelationDataJson ToJson()
    {
        return this.json;
    }

    public override string ToString()
    {
        return LitJson.JsonMapper.ToJson(json);
    }

    public RelationData Clone()
    {
        return new RelationData(LitJson.JsonMapper.ToObject<RelationDataJson>(LitJson.JsonMapper.ToJson(this.json)));
    }
}
}
