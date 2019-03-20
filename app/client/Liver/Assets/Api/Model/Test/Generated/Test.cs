// このコードは、YAMLのモデルデータ仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api.Model.Test
{
public class TestJson
{
    public string name;
    public string description;
}

public partial class Test
{
    public Test() {}

    TestJson json;
    Test(TestJson json)
    {
        this.json = json;
    }

    public string Name { get { return json.name; } set { json.name = value; } }
    public string Description { get { return json.description; } set { json.description = value; } }

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
