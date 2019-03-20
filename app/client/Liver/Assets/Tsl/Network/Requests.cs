//#define USE_MYSERVER

using UnityEngine;
using System;
using System.Collections;
using System.Text;
using Network;
using Api.Net;

namespace Network
{
public class Auth
{
    public string PlayerId { get; set; }
    public string SecretKey { get; set; }
}
public class AuthOperate
{
    public Auth auth = new Auth();
    public string accessToken = "";
    public string filename;

    public AuthOperate(string fn = "")
    {
        this.filename = fn;
    }

    public string GetFilename()
    {
        if (this.filename == "")
        {
            this.filename = "auth_operate_" + Requests.setting.ServerType.ToString();
        }

        return this.filename;
    }

    public void LoadState()
    {
        Tsl.Entity.Storage.Load(GetFilename(), out auth);
    }

    public void SaveState()
    {
        Tsl.Entity.Storage.Save(GetFilename(), auth);
    }

    public void Delete()
    {
        Tsl.Entity.Storage.Delete(GetFilename());
    }

    public void UpdateDeviceId(string playerId, string secretKey)
    {
        this.auth.PlayerId = playerId;
        this.auth.SecretKey = secretKey;
        this.accessToken = "";
    }
}
}

namespace Api.Net
{
public class HTTPException : Exception
{
    public readonly HTTPRequest Request;
    public readonly HTTPResponse Response;

    public HTTPException(HTTPRequest req, HTTPResponse resp)
    {
        Request = req;
        Response = resp;
    }
}

public enum ServerType
{
    DevelopServer,
    MasterServer,
    CustomServer,
    StagingServer,
}

[Serializable]
public class ServerSetting
{
    public ServerType ServerType;
    public string UserServerUrl;

    public static void Load()
    {
        var data = new ServerSetting();

        if (!Tsl.Entity.Storage.Load("serversetting", out data))
        {
            data = ServerSettings.ServerSetting();
        }

        Requests.setting = data;
    }

    public static void Save()
    {
        Tsl.Entity.Storage.Save("serversetting", Requests.setting);
    }
}

public class Requests
{
    public static ServerSetting setting;
    public static int InNetAccess = 0; // 通信中フラグ
    public static int RetryCountRemain = 0; // リトライ残り回数
    public const int RetryCount = 10; // リトライ試行回数
    public static string ClientApiVersion = Masterdata.Constants.Api_version.ApiVersion.Version;
    public static int TotalRequestCount = 0;
    public static double TotalResponseTime = 0.0;

    public static string BASE_URL
    {
        get
        {
            if (setting == null) { ServerSetting.Load();}

            return setting.UserServerUrl;
        }
    }

    private static Requests singletoneInstance = null;

    public Action<string> OnCreateAuth = null; // 新規プレイヤーが作成されたときに呼ばれる。引数はPlayerID
    public AuthOperate AuthOperate;
    public string PlayerId {  get { return this.AuthOperate.auth.PlayerId; } }

    public Util.Future<byte[]> Send(string url, HTTPMethods method, byte[] postData, bool noAuth = false)
    {
        return new Util.Future<byte[]>(promise => InternalSend(url, method, postData, promise, noAuth));
    }

    private Util.Future<Util.Unit> CreateAuth()
    {
        var udid = SystemInfo.deviceUniqueIdentifier;
        this.AuthOperate.auth.SecretKey = Guid.NewGuid().ToString();
        var authF = Api.Net.Auth.Register.Post(this, new Api.Net.Auth.Register.Request(
                secret_key: this.AuthOperate.auth.SecretKey,
                udid: udid,
                player_id: "new"));
        return authF.Then(result =>
        {
            this.AuthOperate.auth.PlayerId = result.PlayerId;
            this.AuthOperate.SaveState();

            if (this.OnCreateAuth != null) { this.OnCreateAuth(result.PlayerId); }

            return Util.Unit.Value;
        });
    }

    // 認証情報のファイルを消す
    public void DeleteAuth()
    {
        if (this.AuthOperate == null)
        {
            this.AuthOperate = new Network.AuthOperate();
            this.AuthOperate.LoadState();
        }

        this.AuthOperate.Delete();
        this.AuthOperate = null;
    }
    // 認証情報を更新する。（機種変更）
    public void UpdateAuth(string playerId, string secretKey)
    {
        this.AuthOperate = new Network.AuthOperate();
        this.AuthOperate.UpdateDeviceId(playerId, secretKey);
        this.AuthOperate.SaveState();
    }

    static private string toString(ref byte[] data)
    {
        string s = "";

        foreach (var b in data)
        {
            s += (char)b;
        }

        return s;
    }

    private IEnumerator InternalSend(string url,
                                     HTTPMethods method,
                                     byte[] postData,
                                     Util.Promise<byte[]> promise,
                                     bool noAuth = false)
    {
        byte[] result = null;
        Exception exception = null;

        // register for authentication
        if (this.AuthOperate == null)
        {
            this.AuthOperate = new Network.AuthOperate();
            this.AuthOperate.LoadState();
        }

        if (string.IsNullOrEmpty(this.AuthOperate.auth.SecretKey))
        {
            var authF = this.CreateAuth();

            foreach (var v in authF) { yield return v; }

            authF.ThrowIfException();
        }

        int retryCount = 0;
        Requests.RetryCountRemain = 0;
        HTTPResponse responce = null;
        ++Requests.InNetAccess;

        while (retryCount < Requests.RetryCount)
        {
            responce = null;
            var sendNow = DateTime.Now;
            var request = new HTTPRequest(url, method, (req, resp) =>
            {
                Requests.TotalRequestCount += 1;
                Requests.TotalResponseTime += (DateTime.Now - sendNow).TotalSeconds;
                responce = resp;

                switch (req.State)
                {
                    case HTTPRequestStates.Finished:
                        if (resp.IsSuccess)
                        {
                            result = resp.Data;
                        }
                        else
                        {
                            exception = new HTTPException(req, resp);
                            Debug.Log(string.Format("Finished status is failed. url:{0} resp:{1} req:{2}, data:\n{3}", url, resp.StatusCode, req, toString(ref postData)));
                        }

                        break;

                    case HTTPRequestStates.Error:
                        if (resp == null)
                        {
                            exception = req.Exception;
                        }
                        else
                        {
                            exception = new HTTPException(req, resp);
                        }

                        Debug.LogError(string.Format("url:{0}\nstate:{1}\nreq:{2}\nresp:{3}\nexception:{4}", url, req.State, req, resp, req.Exception));
                        break;
                }
            });

            if (postData != null)
            {
                request.RawData = postData;
                request.SetHeader("Content-Type", "application/json; charset=UTF-8");
            }

#if UNITY_IOS
            var platform = "ios"; // TODO: ビルドセッティングでプラットフォームを切り替える
#else
            var platform = "android"; // TODO: ビルドセッティングでプラットフォームを切り替える
#endif
            request.SetHeader("X-Techarts-Platform", platform);

            if (!string.IsNullOrEmpty(AuthOperate.accessToken))
            {
                request.SetHeader("Authorization", "techarts " + AuthOperate.accessToken + " " + ClientApiVersion);
            }

            Debug.Log(string.Format("{0} {1}: {2}", HTTPMethods.Post.ToString(), url, Encoding.UTF8.GetString(postData)));
            yield return request.Send();

            if (request.State == HTTPRequestStates.Finished)
            {
                break;
            }

            // タイムアウトした場合は、5秒後にリトライ
            Requests.RetryCountRemain = Requests.RetryCount - retryCount;
            ++retryCount;
            Debug.LogError(string.Format("Request Time Out: retry:{0} exception:{1}",
                                         retryCount,
                                         exception == null ? "no exception" : exception.Message));
            yield return new WaitForSeconds(5.0f);
        }

        --Requests.InNetAccess;

        if (exception != null
                && exception is HTTPException
                && ((HTTPException)exception).Response != null)
        {
            // User not found, because cleared database.
            // If happened the error, please initialize save data.
            if (((HTTPException)exception).Response.StatusCode == 410)
            {
                // new ArgumentException("サーバーに指定されたユーザーが見つかりません。データベースがクリアされた可能性ああります。セーブデータを初期化して、ゲームを再起動してください");
                // サーバーにプレイヤーデータがない場合
                var authF = this.CreateAuth();

                foreach (var v in authF) { yield return v; }

                authF.ThrowIfException();
                var accessTokenF = Api.Net.Auth.AccessToken.Post(this, new Api.Net.Auth.AccessToken.Request(
                                       player_id: this.AuthOperate.auth.PlayerId,
                                       secret_key: this.AuthOperate.auth.SecretKey));

                foreach (var v in accessTokenF) { yield return v; }

                Debug.LogWarning("Recreate Authentication: PlayerId=" + this.AuthOperate.auth.PlayerId);
                this.AuthOperate.accessToken = accessTokenF.Result.AccessToken;
                exception = null;
                // 本体を送信
                var resendF = Send(url, method, postData);

                foreach (var v in resendF) { yield return v; }

                resendF.ThrowIfException();
                result = resendF.Result;
            }
            // get accesstoken if status code is 401.
            // and then re-send the url.
            else if (((HTTPException)exception).Response.StatusCode == 401 && !noAuth)
            {
                var accessTokenF = Api.Net.Auth.AccessToken.Post(this, new Api.Net.Auth.AccessToken.Request(
                                       player_id: AuthOperate.auth.PlayerId,
                                       secret_key: AuthOperate.auth.SecretKey));

                foreach (var v in accessTokenF)
                {
                    yield return v;
                }

                //if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor)
                {
                    if (accessTokenF.HasException)
                    {
                        var authF = this.CreateAuth();

                        foreach (var v in authF)
                        {
                            yield return v;
                        }

                        authF.ThrowIfException();
                        accessTokenF = Api.Net.Auth.AccessToken.Post(this, new Api.Net.Auth.AccessToken.Request(
                                           player_id: AuthOperate.auth.PlayerId,
                                           secret_key: AuthOperate.auth.SecretKey));

                        foreach (var v in accessTokenF)
                        {
                            yield return v;
                        }
                    }

                    Debug.LogWarning("Recreate Authentication: PlayerId=" + AuthOperate.auth.PlayerId);
                }
                this.AuthOperate.accessToken = accessTokenF.Result.AccessToken;
                exception = null;
                var resendF = Send(url, method, postData);

                foreach (var v in resendF)
                {
                    yield return v;
                }

                resendF.ThrowIfException();
                result = resendF.Result;
            }
        }

        if (result == null)
        {
            if (responce != null)
            {
                Debug.LogError(string.Format("URL:{0} RESPONCE:{1} \nDATA:{2}", url, responce.StatusCode, Encoding.UTF8.GetString(postData)));
            }
            else
            {
                Debug.LogError(string.Format("URL:{0} RESPONCE:null", url));
            }

            promise.Exception = exception;
        }
        else
        {
            promise.Result = result;
        }

        yield break;
    }
}
}
