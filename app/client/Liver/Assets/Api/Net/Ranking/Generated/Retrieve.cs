// このコードは、YAMLのAPI仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Network;

namespace Api.Net.Ranking
{
public partial class Retrieve
{
    public class Request
    {
        public class JsonType
        {
            public int ranking_type;
            public string stage_id;
            public int kind1;
            public int kind2;
            public int term;
            public int count;
        }

        JsonType json = new JsonType();

        public string ToJson()
        {
            return LitJson.JsonMapper.ToJson(this.json);
        }

        public int RankingType { get { return json.ranking_type; } set { json.ranking_type = value; } }
        public string StageId { get { return json.stage_id; } set { json.stage_id = value; } }
        public int Kind1 { get { return json.kind1; } set { json.kind1 = value; } }
        public int Kind2 { get { return json.kind2; } set { json.kind2 = value; } }
        public int Term { get { return json.term; } set { json.term = value; } }
        public int Count { get { return json.count; } set { json.count = value; } }

        public Request(int ranking_type, string stage_id, int kind1, int kind2, int term, int count)
        {
            this.RankingType = ranking_type;
            this.StageId = stage_id;
            this.Kind1 = kind1;
            this.Kind2 = kind2;
            this.Term = term;
            this.Count = count;
        }

    }

    public class Response
    {
        public class JsonType
        {
            public bool success;
            public int order;
            public int total_count;
            public Api.Model.Ranking.RankingDataJson[] rankings;
        }

        JsonType json = new JsonType();

        public Response(JsonType fromJson)
        {
            this.json = fromJson;
        }
        public static Response FromJson(string fromJson)
        {
            return new Response(LitJson.JsonMapper.ToObject<JsonType>(fromJson));
        }

        public bool Success { get { return json.success; } set { json.success = value; } }
        public int Order { get { return json.order; } set { json.order = value; } }
        public int TotalCount { get { return json.total_count; } set { json.total_count = value; } }
        public Api.Model.Ranking.RankingData[] Rankings { get { return json.rankings.Select(j => Api.Model.Ranking.RankingData.FromJson(j)).ToArray(); } set { json.rankings = value.Select(j => j.ToJson()).ToArray(); } }
    }

    static partial void OnResponse(Response response);

    public static Util.Future<Response> Post(global::Api.Net.Requests requests, Request request)
    {
        string postStr = "{\"p\":" + request.ToJson() + "}";
        return requests.Send(global::Api.Net.Requests.BASE_URL + "/api/ranking/retrieve", HTTPMethods.Post, Encoding.UTF8.GetBytes(postStr)).Then(buf =>
        {
#if false
            var resultStr = Crypto.Decode(Encoding.UTF8.GetString(buf));
#else
            var resultStr = Encoding.UTF8.GetString(buf);
#endif
            // Debug.Log(string.Format("Response: {0}", resultStr));
            var res = Response.FromJson(resultStr);
            OnResponse(res);
            return res;
        });
    }
}
}
