// このコードは、YAMLのモデルデータ仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Api.Model.Ranking
{
public class RankingDataJson
{
    public string player_id;
    public string player_name;
    public string score;
    public string order;
}

public partial class RankingData
{
    public RankingData() {}

    RankingDataJson json;
    RankingData(RankingDataJson json)
    {
        this.json = json;
    }

    public string PlayerId { get { return json.player_id; } set { json.player_id = value; } }
    public string PlayerName { get { return json.player_name; } set { json.player_name = value; } }
    public string Score { get { return json.score; } set { json.score = value; } }
    public string Order { get { return json.order; } set { json.order = value; } }

    public static RankingData FromJson(RankingDataJson json)
    {
        return new RankingData(json);
    }

    public RankingDataJson ToJson()
    {
        return this.json;
    }

    public override string ToString()
    {
        return LitJson.JsonMapper.ToJson(json);
    }

    public RankingData Clone()
    {
        return new RankingData(LitJson.JsonMapper.ToObject<RankingDataJson>(LitJson.JsonMapper.ToJson(this.json)));
    }
}
}
