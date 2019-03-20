// このコードは、YAMLのAPI仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Network;

namespace Api.Net.Boot
{
public partial class Boot
{
    public class Request
    {
        public class JsonType
        {
            public string client_api_version;
        }

        JsonType json = new JsonType();

        public string ToJson()
        {
            return LitJson.JsonMapper.ToJson(this.json);
        }

        public string ClientApiVersion { get { return json.client_api_version; } set { json.client_api_version = value; } }

        public Request(string client_api_version)
        {
            this.ClientApiVersion = client_api_version;
        }

    }

    public class Response
    {
        public class JsonType
        {
            public string masterdata_json;
            public string api_url;
            public string api_version;
            public string asset_bundle_url;
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

        public string MasterdataJson { get { return json.masterdata_json; } set { json.masterdata_json = value; } }
        public string ApiUrl { get { return json.api_url; } set { json.api_url = value; } }
        public string ApiVersion { get { return json.api_version; } set { json.api_version = value; } }
        public string AssetBundleUrl { get { return json.asset_bundle_url; } set { json.asset_bundle_url = value; } }
    }

    static partial void OnResponse(Response response);

    public static Util.Future<Response> Post(global::Api.Net.Requests requests, Request request)
    {
        string postStr = "{\"p\":" + request.ToJson() + "}";
        return requests.Send(global::Api.Net.Requests.BASE_URL + "/api/boot/boot", HTTPMethods.Post, Encoding.UTF8.GetBytes(postStr)).Then(buf =>
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
