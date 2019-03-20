using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GPS
{
// GPSによる位置取得
//   コンポーネントEnableでOn
//   コンポーネントDisableでOff
//
//   適当なGameObjectのコンポーネントとして使う
//
public class LocationManager : MonoBehaviour
{
    // 要求精度(Unity的には5~10mがオススメ)
    public float DesiredAccuracyInMeters = 10;
    // 更新精度(Unity的には5~10mがオススメ)
    public float UpdateDistanceInMeters = 10;
    // GPSの更新頻度
    public float IntervalSeconds = 1;

    // true: 位置情報が有効
    public bool Started { get; private set; }
    // true: ユーザーが許可していない
    public bool Disabled { get; private set; }

    IEnumerator coroutineMethod;

    // GPSから情報を取得した時刻
    public double CurrentTimeStamp = 0;
    // longitude 経度(東西)
    // latitude  緯度(南北)
    public float Longitude { get; private set; }
    public float Latitude  { get; private set; }

#if UNITY_IOS
    [System.Runtime.InteropServices.DllImport("__Internal")]
    static extern string LocationAuthorizationStatusCheck();
#endif


    void Awake()
    {
        this.Started  = false;
        this.Disabled = false;
        this.Logging  = false;
        this.Playback = false;
    }


    public void OnEnable()
    {
        Debug.Log("Location:OnEnable");
        startLocationService();
    }

    public void OnDisable()
    {
        Debug.Log("Location:OnDisable");
        // 初期化処理を停止
        stopLocationService();
    }


    IEnumerator serviceLocation()
    {
        // GPSデータ取得は無限ループで
        while (true)
        {
            if (Input.location.isEnabledByUser)
            {
                var status = Input.location.status;

                switch (status)
                {
                    case LocationServiceStatus.Stopped:
                        Input.location.Start(this.DesiredAccuracyInMeters, this.UpdateDistanceInMeters);
                        Debug.Log("Location start");
                        break;

                    case LocationServiceStatus.Initializing:
                        Debug.Log("Location Initializing");
                        break;

                    case LocationServiceStatus.Running:
                        {
                            var location = Input.location.lastData;

                            if (location.timestamp > this.CurrentTimeStamp)
                            {
                                // タイムスタンプが違う場合のみ処理する
                                this.CurrentTimeStamp = location.timestamp;
                                this.Longitude = location.longitude;
                                this.Latitude  = location.latitude;
                                this.Started = true;

                                // 必要ならログに記録
                                if (this.Logging)
                                {
                                    updateLog();
                                }
                            }
                        }
                        break;

                    case LocationServiceStatus.Failed:
#if UNITY_IOS
                        string locationAuthorizationStatus = LocationAuthorizationStatusCheck();

                        // kCLAuthorizationStatusAuthorized (許可)　<=(deprecated)
                        // kCLAuthorizationStatusAuthorizedAlways （常に許可）
                        // kCLAuthorizationStatusAuthorizedWhenInUse （使用中のみ許可）
                        if (locationAuthorizationStatus.Contains("kCLAuthorizationStatusAuthorized"))
                        {
                            break;
                        }

#endif
                        // GPS機能の利用をユーザーが許可していない
                        this.Disabled = true;
                        Debug.Log("Location failed");
                        break;
                }
            }
            else
            {
                // ユーザーが有効にする必要がある
                this.Disabled = true;
                Debug.Log("Disabled by user");
            }

            yield return new WaitForSeconds(this.IntervalSeconds);
        }
    }


    //
    // 以下動作検証用
    //

    // 開始と停止
    public void startLocationService()
    {
        // NOTE すでに開始しているなら何もしない
        if (this.coroutineMethod != null) { return; }

        if (this.Playback) { return; }

        this.Started = false;
        this.coroutineMethod = serviceLocation();
        StartCoroutine(this.coroutineMethod);
        Debug.Log("Location started.");
    }

    public void stopLocationService()
    {
        if (this.coroutineMethod == null) { return; }

        StopCoroutine(this.coroutineMethod);
        this.coroutineMethod = null;
        Input.location.Stop();
        this.Started = false;
        Debug.Log("Location stopped.");
    }


    // ログ機能
    class Log
    {
        [System.Serializable]
        public class Data
        {
            public double TimeStamp;
            // NOTE
            public double Longitude;
            public double Latitude;
        }

        public List<Data> actualData = new List<Data>();
    }
    Log log = new Log();

    public bool Logging { get; private set; }
    public bool Playback { get; private set; }

    bool playbackRemaining;


    // Playback用のデータが残っているか？
    public bool isPlaybackRemaining()
    {
        return this.Playback && this.playbackRemaining;
    }


    // ログ開始
    //   NOTE 次に位置情報取得を始めた時から有効になる
    public void startLogging()
    {
        stopPlayback();

        if (!this.Logging)
        {
            this.log.actualData.Clear();
            this.Logging = true;
            Debug.Log("Logging started.");
        }
    }

    // ログ停止
    public void stopLogging()
    {
        if (!this.Logging) { return; }

        writeLog();
        this.Logging = false;
        Debug.Log("Logging stopped.");
    }

    // ログを読み込んで再現
    public void startPlayback(string path)
    {
        stopPlayback();
        stopLocationService();
        stopLogging();
        loadLog(path);
        this.Started = true;
        this.Playback = true;
        this.playbackRemaining = true;
        // 再生開始
        this.playbackMethod = playback();
        StartCoroutine(this.playbackMethod);
        Debug.Log("Playback started.");
    }

    // 再現中止
    public void stopPlayback()
    {
        if (!this.Playback) { return; }

        StopCoroutine(this.playbackMethod);
        this.Playback = false;
        this.playbackRemaining = false;
        startLocationService();
        Debug.Log("Playback stopped.");
    }


    IEnumerator playbackMethod;

    // プレイバック用コルーチン
    IEnumerator playback()
    {
        while (log.actualData.Count > 0)
        {
            // 配列の先頭から１つずつ取り出して割り当てる
            var data = log.actualData[0];
            var t = data.TimeStamp;
            this.CurrentTimeStamp = t;
            this.Longitude = (float)data.Longitude;
            this.Latitude  = (float)data.Latitude;
            this.log.actualData.RemoveAt(0);

            if (log.actualData.Count == 0) { break; }

            yield return new WaitForSeconds((float)(log.actualData[0].TimeStamp - t));
        }

        Debug.Log("Playback finished.");
        this.playbackRemaining = false;
    }


    void updateLog()
    {
        // １つ加える
        var data = new Log.Data { TimeStamp = this.CurrentTimeStamp, Longitude = this.Longitude, Latitude = this.Latitude };
        this.log.actualData.Add(data);
    }

    void writeLog()
    {
        var text = JsonUtility.ToJson(this.log);
        //Debug.Log(text);
        var path = "gpsLog-" + System.DateTime.Now.ToString("yyyy-MMdd-HHmmss") + ".json";
        // FIXME UI実装が必要なので固定パス
        //var path = "log.json";
        Stream.Write(path, text, false);
        this.log.actualData.Clear();
    }

    void loadLog(string path)
    {
        // FIXME UI実装が必要なので固定パス
        //var path = "log.json";
        //var path = "gpsLog-2018-0614-172637.json";
        var text = Stream.Read(path);

        if (text != null)
        {
            JsonUtility.FromJsonOverwrite(text, this.log);
            Debug.Log("Log loaded");
        }
    }
}
}
