// このコードは、YAMLのAPI仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Network;

namespace Api.Net.Auth
{
public partial class Register
{
    public class Request
    {
        public class JsonType
        {
            public string secret_key;
            public string udid;
            public string player_id;
        }

        JsonType json = new JsonType();

        public string ToJson()
        {
            return LitJson.JsonMapper.ToJson(this.json);
        }

        public string SecretKey { get { return json.secret_key; } set { json.secret_key = value; } }
        public string Udid { get { return json.udid; } set { json.udid = value; } }
        public string PlayerId { get { return json.player_id; } set { json.player_id = value; } }

        public Request(string secret_key, string udid, string player_id)
        {
            this.SecretKey = secret_key;
            this.Udid = udid;
            this.PlayerId = player_id;
        }

    }

    public class Response
    {
        public class JsonType
        {
            public string player_id;
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

        public string PlayerId { get { return json.player_id; } set { json.player_id = value; } }
    }

    static partial void OnResponse(Response response);

    public static Util.Future<Response> Post(global::Api.Net.Requests requests, Request request)
    {
        string postStr = "{\"p\":" + request.ToJson() + "}";
        return requests.Send(global::Api.Net.Requests.BASE_URL + "/api/auth/register", HTTPMethods.Post, Encoding.UTF8.GetBytes(postStr)).Then(buf =>
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
