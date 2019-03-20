// このコードは、YAMLのマスターデータ仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Masterdata.Test
{
public class TestJson
{
    public int id;
    public string name;
    public int value;
    public int relation;
    public int[] arr;
}

public partial class Test
{
    public Test() {}

    TestJson json;
    Test(TestJson json)
    {
        this.json = json;
    }

    public int Id { get { return json.id; } set { json.id = value; } }
    public string Name { get { return json.name; } set { json.name = value; } }
    public int Value { get { return json.value; } set { json.value = value; } }
    public Masterdata.Test.RelationData Relation { get { return Masterdata.Test.RelationData.Get(json.relation); } }
    public int[] Arr { get { return json.arr; } set { json.arr = value; } }

    static Test[] staticArray;
    static Dictionary<int, Test> staticDict;

    public static void Load(string inputJson)
    {
        var json_types = LitJson.JsonMapper.ToObject<TestJson[]>(inputJson);
        staticArray = json_types.Select(x => new Test(x)).ToArray();
        staticDict = staticArray.ToDictionary(x => x.Id);
    }

    public static Test Get(int id)
    {
        return Test.staticDict[id];
    }
    public static Test[] All()
    {
        return Test.staticArray;
    }

    public static Test FromJson(TestJson json)
    {
        return new Test(json);
    }

    public TestJson ToJson()
    {
        return this.json;
    }

    public override string ToString()
    {
        return LitJson.JsonMapper.ToJson(json);
    }

    public Test Clone()
    {
        return new Test(LitJson.JsonMapper.ToObject<TestJson>(LitJson.JsonMapper.ToJson(this.json)));
    }
}
}
