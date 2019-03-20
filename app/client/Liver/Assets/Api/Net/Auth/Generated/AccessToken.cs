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
public partial class AccessToken
{
    public class Request
    {
        public class JsonType
        {
            public string player_id;
            public string secret_key;
        }

        JsonType json = new JsonType();

        public string ToJson()
        {
            return LitJson.JsonMapper.ToJson(this.json);
        }

        public string PlayerId { get { return json.player_id; } set { json.player_id = value; } }
        public string SecretKey { get { return json.secret_key; } set { json.secret_key = value; } }

        public Request(string player_id, string secret_key)
        {
            this.PlayerId = player_id;
            this.SecretKey = secret_key;
        }

    }

    public class Response
    {
        public class JsonType
        {
            public string access_token;
            public int expires_in;
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

        public string AccessToken { get { return json.access_token; } set { json.access_token = value; } }
        public int ExpiresIn { get { return json.expires_in; } set { json.expires_in = value; } }
    }

    static partial void OnResponse(Response response);

    public static Util.Future<Response> Post(global::Api.Net.Requests requests, Request request)
    {
        string postStr = "{\"p\":" + request.ToJson() + "}";
        return requests.Send(global::Api.Net.Requests.BASE_URL + "/api/auth/accesstoken", HTTPMethods.Post, Encoding.UTF8.GetBytes(postStr)).Then(buf =>
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
