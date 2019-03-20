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
public partial class SavePlayerdata
{
    public class Request
    {
        public class JsonType
        {
            public string player_id;
            public string player_name;
            public string savedata;
            public string passcode;
            public int kind;
        }

        JsonType json = new JsonType();

        public string ToJson()
        {
            return LitJson.JsonMapper.ToJson(this.json);
        }

        public string PlayerId { get { return json.player_id; } set { json.player_id = value; } }
        public string PlayerName { get { return json.player_name; } set { json.player_name = value; } }
        public string Savedata { get { return json.savedata; } set { json.savedata = value; } }
        public string Passcode { get { return json.passcode; } set { json.passcode = value; } }
        public int Kind { get { return json.kind; } set { json.kind = value; } }

        public Request(string player_id, string player_name, string savedata, string passcode, int kind)
        {
            this.PlayerId = player_id;
            this.PlayerName = player_name;
            this.Savedata = savedata;
            this.Passcode = passcode;
            this.Kind = kind;
        }

    }

    public class Response
    {
        public class JsonType
        {
            public bool accepted;
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
    }

    static partial void OnResponse(Response response);

    public static Util.Future<Response> Post(global::Api.Net.Requests requests, Request request)
    {
        string postStr = "{\"p\":" + request.ToJson() + "}";
        return requests.Send(global::Api.Net.Requests.BASE_URL + "/api/player/save_playerdata", HTTPMethods.Post, Encoding.UTF8.GetBytes(postStr)).Then(buf =>
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
