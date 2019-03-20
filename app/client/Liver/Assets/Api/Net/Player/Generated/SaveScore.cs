// このコードは、YAMLのAPI仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Network;

namespace Api.Net.Player
{
public partial class SaveScore
{
    public class Request
    {
        public class JsonType
        {
            public string stage_id;
            public int score;
            public int kind1;
            public int kind2;
            public int kind3;
        }

        JsonType json = new JsonType();

        public string ToJson()
        {
            return LitJson.JsonMapper.ToJson(this.json);
        }

        public string StageId { get { return json.stage_id; } set { json.stage_id = value; } }
        public int Score { get { return json.score; } set { json.score = value; } }
        public int Kind1 { get { return json.kind1; } set { json.kind1 = value; } }
        public int Kind2 { get { return json.kind2; } set { json.kind2 = value; } }
        public int Kind3 { get { return json.kind3; } set { json.kind3 = value; } }

        public Request(string stage_id, int score, int kind1, int kind2, int kind3)
        {
            this.StageId = stage_id;
            this.Score = score;
            this.Kind1 = kind1;
            this.Kind2 = kind2;
            this.Kind3 = kind3;
        }

    }

    public class Response
    {
        public class JsonType
        {
            public bool accepted;
            public int order_in_stage;
            public int my_high_score;
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

        public bool Accepted { get { return json.accepted; } set { json.accepted = value; } }
        public int OrderInStage { get { return json.order_in_stage; } set { json.order_in_stage = value; } }
        public int MyHighScore { get { return json.my_high_score; } set { json.my_high_score = value; } }
    }

    static partial void OnResponse(Response response);

    public static Util.Future<Response> Post(global::Api.Net.Requests requests, Request request)
    {
        string postStr = "{\"p\":" + request.ToJson() + "}";
        return requests.Send(global::Api.Net.Requests.BASE_URL + "/api/player/save_score", HTTPMethods.Post, Encoding.UTF8.GetBytes(postStr)).Then(buf =>
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
