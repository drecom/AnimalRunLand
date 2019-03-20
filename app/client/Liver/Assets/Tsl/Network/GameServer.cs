using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tsl.Entity;
using Util.Extension;
using Boot = Api.Net.Boot;
using Player = Api.Net.Player;

namespace Api.Net.Stage
{
public partial class OpendStages
{
    public enum StageKind
    {
        SingleStage = 0,
        CampaignStage = 1,
        AllStage = 2
    }
}
}

namespace Tsl.Network
{
// ゲームサーバーへのアクセスを集約
//
// サポートAPIの追加手順
// 1.GameServerクラスに以下のインスタンスを追加
//      ApiStatus<hoge.Response> hogeApi
// 2.GameServerのコンストラクタにhogeApiを追加
//      this.allApi = new ApiStatusBase[] {
//          homeApi,
//          ...,
//          hogeApi };
// 3.GameServerに処理メソッドを追加
//      public IEnumerator RequestHogeCoroutine(Action<hoge.Response> onComplete = null)
//
public partial class GameServer : MonoBehaviour
{
    // 各APIの状態とデータキャッシュ
    // API追加した場合はコンストラクタでallApiに追加すること
    private ApiStatus<Boot.Boot.Response> bootApi = new ApiStatus<Boot.Boot.Response>();

    private void AwakeCommon()
    {
        this.allApi = new ApiStatusBase[]
        {
            bootApi,
        };
        DontDestroyOnLoad(this.gameObject);
    }


    #region ApiStatusクラス定義
    private class ApiStatusBase
    {
        public DateTime LastCallTime = DateTime.FromFileTimeUtc(0); // 1970/1/1
        public bool InRetrieving = false; // 取得中
        public void Reset()
        {
            this.InRetrieving = false;
            this.LastCallTime = DateTime.FromFileTimeUtc(0);
        }
    }

    // APIコールの状態を管理するクラス
    private class ApiStatus<T> : ApiStatusBase // where T : Api.Net.BaseResponse
    {
        public T CacheData = default(T); // null; // 読み込んだデータ


        public IEnumerator Request(Func<Util.Future<T>> post, Action<T> onComplete = null)
        {
            // 多重呼出しのチェック
            yield return busyWait(onComplete);

            if (!InRetrieving) { yield break; } // 多重呼出しで多重先が完了した

            // API コール
            var result = post();
            result.SetCallback(f => CheckAndComplite(f, onComplete));

            foreach (var v in result) { yield return v; }
        }


        // 指定時間(分)以下だったらキャッシュデータを使う
        public bool CacheCheck(float minutes, Action<T> onComplete)
        {
            var pasttime = (DateTime.Now - this.LastCallTime).TotalMinutes;

            if (pasttime < minutes && this.CacheData != null)
            {
                // 5分以内にリクエストを実行していたらキャッシュを返す
                this.InRetrieving = false;

                if (onComplete != null) { onComplete(this.CacheData); }

                return true;
            }

            return false;
        }

        // レスポンスの処理
        public void CheckAndComplite(Util.Future<T> result, Action<T> onComplete = null)
        {
            if (Check(result))
            {
                Complete(result.Result, onComplete);
            }
        }

        // エラーチェック
        public bool Check(Util.Future<T> result)
        {
            this.InRetrieving = false;

            if (result.HasException)
            {
                var httpexception = result.Exception as Api.Net.HTTPException;
                int statuscode = 0;

                if (httpexception != null && httpexception.Response != null)
                {
                    statuscode = httpexception.Response.StatusCode;
                }

                string msg = "";

                switch (statuscode)
                {
                    case 404:
                        msg = "通信エラーです。";
                        break;

                    case 426:
                        // msg = "APIバージョンが違います。";
                        msg = "新しいバージョンがあります。\nアプリをアップデートしてください。";
                        break;

                    case 502:
                    case 503:
                        msg = "サーバーメンテナンス中です。\nしばらくしてからアクセスしてください。";
                        break;

                    case 500:
                        msg = "Internal Server Error\n" + result.Exception.Message;
                        break;

                    case 0: // マスターデータに相違がある場合、bootのレスポンスの処理で例外がthrowされる
                        msg = "ゲームサーバーに接続できません。\n" + result.Exception.Message; // "マスターデータが違います。";
                        break;

                    default:
                        msg = "ゲームサーバーに接続できません。" + statuscode.ToString();
                        break;
                }

                if (GameServer.OnFatalError != null)
                {
                    GameServer.OnFatalError(msg, statuscode);
                }
                else
                {
                    Debug.LogError(string.Format("Network Error {0}: {1}", statuscode, msg));
                }

                return false;
            }

            return true;
        }

        // busyチェック
        public IEnumerator busyWait(Action<T> onEnd = null)
        {
            //Debug.LogWarning("API busyWait " + typeof(T).ToString());
            if (this.InRetrieving)
            {
                // 取得中の場合、終わるまで待ってキャッシュデータをセットする
                //Debug.LogWarning("API 取得中なので待つ");
                float timelimit = 60.0f; // 1分でタイムアウト

                while (this.InRetrieving && timelimit > 0.0f)
                {
                    timelimit -= Time.deltaTime;
                    yield return null;
                }

                if (this.InRetrieving)
                {
                    Debug.LogError("GameServer.busyWait: タイムアウトしました");
                }
                else
                {
                    // 取得終了
                    if (onEnd != null) { onEnd(this.CacheData); }
                }
            }
            else
            {
                // 取得中でない場合は取得開始
                //Debug.LogWarning("API 取得開始");
                this.InRetrieving = true;
            }
        }
        // 取得したデータをセットする
        public void Complete(T data, Action<T> onEnd = null)
        {
            //Debug.LogWarning("API データ取得完了");
            this.CacheData = data;
            this.InRetrieving = false;
            this.LastCallTime = DateTime.Now;

            if (onEnd != null) { onEnd(data); }
        }
    }
    #endregion
    public static Action<string, int> OnFatalError = null;
    public Api.Net.Requests Requests = new Api.Net.Requests();
    private ApiStatusBase[] allApi;
    // waiter
    private Util.Waiter waiter;

    // singleton
    private static GameServer rawInstance = null;

    public static GameServer Instance
    {
        get
        {
            if (GameServer.rawInstance == null)
            {
                var obj = new GameObject("GameServer");
                GameServer.rawInstance = obj.AddComponent<GameServer>();
                DontDestroyOnLoad(GameServer.rawInstance.gameObject);
            }

            return GameServer.rawInstance;
        }
    }


    void Update()
    {
        if (this.waiter != null)
        {
            this.waiter.Update();
        }
    }

    public void StartWait(Func<bool> waitF, Action onEnd)
    {
        if (this.waiter != null)
        {
            Debug.LogError("GameServer::waiter busy");
            onEnd();
        }
        else
        {
            this.waiter = new Util.Waiter();
            this.waiter.StartWait(waitF, () =>
            {
                this.waiter = null;
                onEnd();
            });
        }
    }


    // なにかしらリクエスト中の場合
    public bool busy()
    {
        return this.allApi.Any(api => api.InRetrieving);
    }

    // 強制的にフラグを消す
    public void forceClear()
    {
        foreach (var api in this.allApi)
        {
            api.Reset();
        }

        lastGetServerLocalTime = DateTime.FromFileTimeUtc(0);
    }
    // Boot処理
    public IEnumerator Boot(Action<Boot.Boot.Response> onComplete = null)
    {
        yield return this.bootApi.Request(() =>
                                          Api.Net.Boot.Boot.Post(this.Requests, new Api.Net.Boot.Boot.Request(Masterdata.Constants.Api_version.ApiVersion.Version)),
                                          onComplete);
    }
#if false
    // ログインチェック
    public void LoginCheck(Action<Player.LoginCheck.Response> onComplete = null)
    {
        StartCoroutine(this.loginCheckApi.Request(() =>
                       Api.Net.Player.LoginCheck.Post(this.Requests, new Api.Net.Player.LoginCheck.Request()),
                       onComplete));
    }

    // プレイヤー情報の取得
    public Api.Net.Player.PlayerInfo.Response PlayerInfo
    {
        get
        {
            return this.playerInfoApi.CacheData;
        }
    }

    public void RetrievePlayerInfo(Action<Player.PlayerInfo.Response> onComplete = null)
    {
        StartCoroutine(RetrievePlayerInfoCoroutine(onComplete));
    }

    public IEnumerator RetrievePlayerInfoCoroutine(Action<Player.PlayerInfo.Response> onComplete = null)
    {
        yield return this.playerInfoApi.Request(() =>
                                                Api.Net.Player.PlayerInfo.Post(this.Requests, new Api.Net.Player.PlayerInfo.Request()),
                                                onComplete);
    }


    // プレイヤー情報の設定
    public void SendPlayerInfo(Action<Player.SetPlayerInfo.Response> onComplete = null)
    {
        StartCoroutine(SendPlayerInfoCoroutine(onComplete));
    }
    public IEnumerator SendPlayerInfoCoroutine(Action<Player.SetPlayerInfo.Response> onComplete = null)
    {
        yield return this.setPlayerApi.Request(() =>
                                               Api.Net.Player.SetPlayerInfo.Post(this.Requests,
                                                       new Api.Net.Player.SetPlayerInfo.Request(this.PlayerInfo.PlayerName,
                                                               this.PlayerInfo.CurrentNation,
                                                               this.PlayerInfo.CurrentMap,
                                                               this.PlayerInfo.CurrentStage,
                                                               this.PlayerInfo.CurrentCampaignType,
                                                               this.PlayerInfo.TotalPlayTime
                                                                                               )),
                                               onComplete);
    }
#endif


    // サーバー時刻を取得する
    private static DateTime lastGetServerLocalTime = DateTime.FromFileTimeUtc(0);
#if false
    public void GetServerTime(Action<DateTime> now)
    {
        StartCoroutine(GetServerTimeCoroutine(now));
    }
    public static IEnumerator GetServerTimeCoroutine(Action<DateTime> now)
    {
        var api = Instance.timeoryApi;
        // 最後に取得してから1h以内の場合はAPIコールしない
        var pasttime = (DateTime.Now - lastGetServerLocalTime);

        if (pasttime.TotalMinutes < 60.0f)
        {
            if (now != null) { now(api.LastCallTime + pasttime); }
        }
        else
        {
            var resultF = TimeApi.Post(new TimeApi.Request());
            resultF.SetCallback(f =>
            {
                if (api.Check(f))
                {
                    //OK
                    lastGetServerLocalTime = DateTime.Now;
                    api.LastCallTime = f.Result.ServerNow;
                    api.InRetrieving = false;
                    now(f.Result.ServerNow);
                }
            });

            foreach (var v in resultF) { yield return v; }
        }
    }
#endif

    // GameServerzのインスタンスでCoroutineを動かす
    public static void Coroutine(Func<Action, IEnumerator> act, Action onEnd)
    {
        GameServer.Instance.StartCoroutine(act(onEnd));
    }
    public static void Coroutine(Func<IEnumerator> act)
    {
        GameServer.Instance.StartCoroutine(act());
    }

}

}




