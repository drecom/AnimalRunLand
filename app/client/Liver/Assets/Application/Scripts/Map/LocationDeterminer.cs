//
// GPS情報からメッシュのマップを特定する
//
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BeefMain.Runtime;


namespace Map
{
public class LocationDeterminer : MonoBehaviour
{
    GPS.LocationManager locationManager;
    IEnumerator coroutineMethod;

    // PC実行時のダミー位置
    // Lon 139.777952  Lat 35.698683   テクニカルアーツ所在地
    // Lon 139.713141  Lat 35.631854   ドリコム所在地
    // Lon 139.703460  Lat 35.659010   渋谷ヒカリエ
    // Lon 139.757748  Lat 35.642158   芝浦ふ頭
    // Lon 139.810660  Lat 35.710075   東京スカイツリー
    // TODO 一覧から選択
    public double DummyLongitude = 139.777952;
    public double DummyLatitude = 35.698683;

    // グリッドサイズ
    static readonly public double GridSize = 3;
    static readonly public double GridSizeKilometer = GridSize * 1000;
    // 緯度経度1度あたりの距離
    static readonly double DistanceByDegrees = 90000;

    // グリッドマップの中心位置
    static readonly public double CenterLongitude = 139.741;
    static readonly public double CenterLatitude = 35.658;

    static readonly private int TimeOutSeconds = 10;

    // 外部から利用する値
    public string MapPath { get; private set; }
    public Vector3 EnterPosition { get; private set; }

    // 開始位置
    public double Latitude;
    public double Longitude;

    // GPS使用を許可されていない
    public bool Disabled { get; private set; }
    // GPS取得に時間がかかっている
    public bool TimeOut { get; private set; }

    bool forceDummy = false;


    // リモート接続かどうか取得
    bool IsRemoteConnected()
    {
#if UNITY_EDITOR
        return UnityEditor.EditorApplication.isRemoteConnected;
#else
        return false;
#endif
    }


    void Awake()
    {
#if false
        // グリッド座標→緯度経度
        Vector2[] tbl =
        {
            new Vector2(-1, -1),
            new Vector2(-1,  0),
            new Vector2(-1,  1),
            new Vector2(0, -1),
            new Vector2(0,  0),
            new Vector2(0,  1),
            new Vector2(1, -1),
            new Vector2(1,  0),
            new Vector2(1,  1),
            new Vector2(5,  8),
            new Vector2(5,  9),
            new Vector2(6, 10),
            new Vector2(6,  8),
            new Vector2(6,  9),
            new Vector2(7, 10),
            new Vector2(7,  7),
            new Vector2(7,  8),
            new Vector2(7,  9),
            new Vector2(8,  7),
            new Vector2(8,  8),
        };

        foreach (var v in tbl)
        {
            GridToLatLng(v.x, v.y);
        }

#endif
        // TIPS Componentは自分で生成可能
        this.locationManager = gameObject.AddComponent<GPS.LocationManager>();
        this.Disabled = false;
        this.TimeOut = false;
    }

    void OnDestroy()
    {
        // 明示的に破棄
        Destroy(this.locationManager);
    }


    // 強制的に指定位置で始める
    public void ForceLocation(double longitude, double latitude)
    {
        this.forceDummy     = true;
        this.DummyLongitude = longitude;
        this.DummyLatitude  = latitude;
    }


    // 初期化完了待ち
    public IEnumerator Wait()
    {
#if UNITY_EDITOR

        // エディタ上で動作している場合はUnitRemoteの接続待ち
        if (!this.forceDummy && Application.isEditor && !IsRemoteConnected())
        {
            yield return new WaitForSeconds(3);
        }

#endif

        if (!this.forceDummy && (!Application.isEditor || IsRemoteConnected()))
        {
            // 実機動作かRemote動作の時はGPSを待つ
            yield return WaitLocation();
        }
        else
        {
            // PC動作では適当な位置
            SetLocation(this.DummyLongitude, this.DummyLatitude);
            Debug.Log("Dummy location.");
        }
    }


    IEnumerator WaitLocation()
    {
        int timeCounter = 0;

        while (!this.locationManager.Started && !this.locationManager.Disabled)
        {
            // GPS情報取得をのんびり待つ
            yield return new WaitForSeconds(1);
            timeCounter++;

            if (TimeOutSeconds < timeCounter)
            {
                Debug.Log("Timeout GPS Update.");
                // 位置が取得できなかった
                this.TimeOut = true;
                break;
            }
        }

        if (this.locationManager.Started)
        {
            Debug.Log("Get location.");
            SetLocation(this.locationManager.Longitude, this.locationManager.Latitude);
        }
        else if (!this.TimeOut)
        {
            Debug.Log("Disabled location.");
            // 位置が取得できなかった
            this.Disabled = true;
        }
    }


    void SetLocation(double longitude, double latitude)
    {
        this.Latitude  = latitude;
        this.Longitude = longitude;
        // GPSの緯度経度→グリッド座標
        var grid = LatLngToGrid(latitude, longitude);
        // グリッド座標→グリッド中心の緯度経度
        var gridCenterLatLng = GridToLatLng(grid.x, grid.y);
        var subFolder = $"map_{Mathf.Floor(gridCenterLatLng.y)}_{Mathf.Floor(gridCenterLatLng.x)}/";
        // PATH(拡張子抜き)
        Liver.RaceWindow.AreaName = $"{grid.x}_{grid.y}";
        this.MapPath = subFolder + $"MAP_{Liver.RaceWindow.AreaName}";
        Debug.Log("Path: " + this.MapPath);
        // マップ内座標(距離)
        // FIXME 南の方は若干グリッド間に隙間があるので
        //       必ずマップ内に収まるように補正をかける
        var gridDegrees = (1 / DistanceByDegrees) * LocationDeterminer.GridSizeKilometer;
        var map_x = (longitude - CenterLongitude - grid.x * gridDegrees) * MapUtility.WorldScale.x;
        var map_y = (latitude - CenterLatitude - grid.y * gridDegrees) * MapUtility.WorldScale.y;
        Debug.Log("Start: " + map_x + " ," + map_y);
        this.EnterPosition = new Vector3((float)map_x, 0, (float)map_y);
    }

    // 全ノードを走査して一番近いノードを探す
    public static string SearchStartPosition(Vector3 startPosition, Dictionary<string, NodeMap.NodeData> node)
    {
        // FIXME LINQでの実装
        var sl = float.MaxValue;
        string node_name = string.Empty;

        foreach (var n in node)
        {
            // 3kmグリッド外は無視
            var pos = n.Value.Position;

            if (Mathf.Abs(pos.x) > (LocationDeterminer.GridSizeKilometer * 0.5f - 25)
                    || Mathf.Abs(pos.z) > (LocationDeterminer.GridSizeKilometer * 0.5f - 25))
            {
                continue;
            }

            var d = n.Value.Position - startPosition;
            var l = d.sqrMagnitude;

            if (l < sl)
            {
                sl = l;
                node_name = n.Key;
            }
        }

        Debug.Log("Enter node:" + node_name);
        return node_name;
    }


    // 緯度経度→グリッド位置
    static public Vector2Int LatLngToGrid(double latitude, double longitude)
    {
        var gridDegrees = (1 / DistanceByDegrees) * LocationDeterminer.GridSizeKilometer;
        var grid_x = Mathd.Floor((longitude - CenterLongitude + gridDegrees * 0.5) / gridDegrees);
        var grid_y = Mathd.Floor((latitude  - CenterLatitude  + gridDegrees * 0.5) / gridDegrees);
        return new Vector2Int((int)grid_x, (int)grid_y);
    }

    // グリッド位置→緯度経度
    static public Vector2 GridToLatLng(int x, int y)
    {
        var gridDegrees = (1 / DistanceByDegrees) * LocationDeterminer.GridSizeKilometer;
        var longitude = x * gridDegrees + CenterLongitude;
        var latitude  = y * gridDegrees + CenterLatitude;
        Debug.Log($"{x}, {y} -> {latitude}, {longitude}");
        return new Vector2((float)latitude, (float)longitude);
    }

    // 座標→緯度経度
    static public Vector2 PositionToLatLng(Vector3 pos, Vector2Int grid)
    {
        var gridDegrees = (1 / DistanceByDegrees) * LocationDeterminer.GridSizeKilometer;
        var longitude = pos.x / MapUtility.WorldScale.x + grid.x * gridDegrees + CenterLongitude;
        var latitude  = pos.z / MapUtility.WorldScale.y + grid.y * gridDegrees + CenterLatitude;
        return new Vector2((float)latitude, (float)longitude);
    }
}
}

/*
-1, -1 -> 35.6246666666667, 139.707666666667
-1, 0 -> 35.658, 139.707666666667
-1, 1 -> 35.6913333333333, 139.707666666667
0, -1 -> 35.6246666666667, 139.741
0, 0 -> 35.658, 139.741
0, 1 -> 35.6913333333333, 139.741
1, -1 -> 35.6246666666667, 139.774333333333
1, 0 -> 35.658, 139.774333333333
1, 1 -> 35.6913333333333, 139.774333333333
5, 8 -> 35.9246666666667, 139.907666666667
5, 9 -> 35.958, 139.907666666667
6, 10 -> 35.9913333333333, 139.941
6, 8 -> 35.9246666666667, 139.941
6, 9 -> 35.958, 139.941
7, 10 -> 35.9913333333333, 139.974333333333
7, 7 -> 35.8913333333333, 139.974333333333
7, 8 -> 35.9246666666667, 139.974333333333
7, 9 -> 35.958, 139.974333333333
8, 7 -> 35.8913333333333, 140.007666666667
8, 8 -> 35.9246666666667, 140.007666666667

*/