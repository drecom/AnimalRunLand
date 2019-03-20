// このコードは、YAMLのAPI仕様から自動生成されたものです。
// 直接編集はしないでください。

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Network;

namespace Api.Net.Hello
{
public partial class Hello
{
    public class Request
    {
        public class JsonType
        {
            public string key;
            public Masterdata.Test.TestEnum enum_value;
        }

        JsonType json = new JsonType();

        public string ToJson()
        {
            return LitJson.JsonMapper.ToJson(this.json);
        }

        public string Key { get { return json.key; } set { json.key = value; } }
        public Masterdata.Test.TestEnum EnumValue { get { return json.enum_value; } set { json.enum_value = value; } }

        public Request(string key, Masterdata.Test.TestEnum enum_value)
        {
            this.Key = key;
            this.EnumValue = enum_value;
        }

    }

    public class Response
    {
        public class JsonType
        {
            public string key;
            public bool boolean_value;
            public int[] intar;
            public Masterdata.Test.TestJson test_master;
            public Api.Model.Test.TestJson test_model;
            public Masterdata.Test.TestEnum enum_value;
            public int? nullable_value;
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

        public string Key { get { return json.key; } set { json.key = value; } }
        public bool BooleanValue { get { return json.boolean_value; } set { json.boolean_value = value; } }
        public int[] Intar { get { return json.intar; } set { json.intar = value; } }
        public Masterdata.Test.Test TestMaster { get { return Masterdata.Test.Test.FromJson(json.test_master); } set { json.test_master = value.ToJson(); } }
        public Api.Model.Test.Test TestModel { get { return Api.Model.Test.Test.FromJson(json.test_model); } set { json.test_model = value.ToJson(); } }
        public Masterdata.Test.TestEnum EnumValue { get { return json.enum_value; } set { json.enum_value = value; } }
        public int? NullableValue { get { return json.nullable_value; } set { json.nullable_value = value; } }
    }

    static partial void OnResponse(Response response);

    public static Util.Future<Response> Post(global::Api.Net.Requests requests, Request request)
    {
        string postStr = "{\"p\":" + request.ToJson() + "}";
        return requests.Send(global::Api.Net.Requests.BASE_URL + "/api/hello/hello", HTTPMethods.Post, Encoding.UTF8.GetBytes(postStr)).Then(buf =>
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
