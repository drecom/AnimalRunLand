using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using System.Text.RegularExpressions;
using BeefDefine.SchemaWrapper;
using BeefMain.Runtime;
using System.Linq;
using UnityEngine.UI;
using Tsl.UI;
using Shand;
using Util.Extension;
using Liver.Entity;
using Liver;

public class RaceArea : MonoBehaviour
{
    public Camera raceCamera;
    public GameObject gatePrefab;
    public GameObject BaloonPrefab;

    [HideInInspector, System.NonSerialized]
    public RenderTexture renderTexture;
    [HideInInspector, System.NonSerialized]

    public bool pause = false;

    string current;
    string next;
    public Dictionary<string, NodeMap.NodeData> nodes;
    public float StarRatio = 1;

    GameObject animalRoot;      // 移動オブジェクト
    GameObject animal;          // モデル
    AnimalController controller;

    // 開始マップのPATH
    string mapPath;
    // 緯度経度→World座標
    Vector3 enterPosition;
    // 開始時の緯度経度
    float latitude;
    float longitude;
    // エリア座標
    Vector2Int gridPos;

    Easing LaneChange = new Easing();
    Easing LaneTab = new Easing();

    enum Lane
    {
        Left = -1,
        Center = 0,
        Right = 1,
    }
    Lane lane = Lane.Center;
    List<string> currentBranchList = new List<string>();
    // 左右の分岐数
    int numBranchLeft  = 0;
    int numBranchRight = 0;

    float totalDistance = 0;
    int totalStar = 0;
    int totalPowerup = 0;

    Animal animalData;
    string branchNode = null;
    bool countdown = false;
    Dictionary<GameObject, float> PassPoiTimestamp = new Dictionary<GameObject, float>();

    float branchDistance = 0;
    bool swipeToLaneChange;
    float swipeLimit;

    Liver.RaceWindow _RaceWindow;
    Liver.RaceWindow RaceWindow
    {
        get
        {
            if (_RaceWindow == null)
            {
                var go = GameObject.Find("RaceWindow(Clone)");

                if (go != null) { _RaceWindow = go.GetComponent<Liver.RaceWindow>(); }
            }

            return _RaceWindow;
        }
    }

    float remainTime = 0;
    float RecoveryTime = 0;

    // 煽りBGM判定用
    int prevTime = 0;


    ItemEffect currentEffect;
    BeefMap beefmap;

    // スター倍化の為に覚えておく
    Map.GridArea itemGridArea;
    List<MeshSwitcher> normalStars = new List<MeshSwitcher>();

    // レース中
    bool activeRace = false;

    LoadingWindow Loading;


    void Awake()
    {
        this.remainTime = RaceSettings.Instance.TimeLimit;
        this.prevTime = (int)this.remainTime;
        swipeToLaneChange = PlayerPrefs.GetInt("SwipeToLaneChange", 1) != 0;
        swipeLimit = PlayerPrefs.GetInt("OnSwipeDistanceLimit", 5);
        this.Loading = RaceWindow.BaseScene.AddModalWindow<LoadingWindow>("LoadingWindow");
        LoadingProgress("Loading Start");
        renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Default);
        raceCamera.targetTexture = renderTexture;
        RaceWindow.raceImage.texture = renderTexture;
        // 動物をロードする
        animalRoot = new GameObject("AnimalRoot");
        var shadow = Instantiate<GameObject>(Resources.Load<GameObject>("Models/Animal/cha_shadow"), animalRoot.transform);
        shadow.transform.localPosition += new Vector3(0, 0.1f, 0);
        animal = Instantiate<GameObject>(Resources.Load<GameObject>(string.Format("Models/Animal/{0}", AnimalSettings.Instance[RaceWindow.AnimalKind].model)), animalRoot.transform);
        // 当たり判定の準備
        var collider = Instantiate<GameObject>(Resources.Load<GameObject>("Models/Animal/AnimalCollider"), animal.transform);
        animalRoot.SetLayerAllChildren(LayerMask.NameToLayer("Race"));
        controller = animal.AddComponent<AnimalController>();
        controller.SetShadow(shadow);
        // エフェクト追加
        controller.AutoEffect(AnimationType.Run, ParticleGen.Instance.Play("Effects/ef_007", animal.transform));
        controller.AutoEffect(AnimationType.Dash, ParticleGen.Instance.Play("Effects/ef_008", animal.transform));
        controller.AutoEffect(AnimationType.Damage, ParticleGen.Instance.Play("Effects/ef_009", animal.transform));
        controller.AutoEffect(AnimationType.TurnL, ParticleGen.Instance.Play("Effects/ef_013", animal.transform));
        controller.AutoEffect(AnimationType.TurnR, ParticleGen.Instance.Play("Effects/ef_012", animal.transform));
        controller.OnGetItem += (obj) =>
        {
            var match = Regex.Match(obj.name, @"(Star|Powerup)(\d+)");

            switch (match.Groups[1].Value)
            {
                case "Star":
                    var switcher = obj.GetComponentInChildren<MeshSwitcher>();

                    if (switcher != null)
                    {
                        GetStar(switcher.Current + 1);
                    }
                    else
                    {
                        GetStar(int.Parse(match.Groups[2].Value));
                    }

                    ParticleGen.Instance.PlayOnce("Effects/ef_006", animal.transform);
                    break;

                case "Powerup":
                    var id = int.Parse(match.Groups[2].Value);
                    GetPowerup((PowerUpItem)(id - 1));
                    ParticleGen.Instance.PlayOnce("Effects/ef_005", animal.transform);

                    // 演出削除
                    foreach (Transform child in obj.transform)
                    {
                        Destroy(child.gameObject);
                    }

                    break;
            }
        };
        controller.OnObstacle += OnObstacle;
        controller.OnWall += (go) => UTurn();   // 壁なのでUターンする
        controller.OnPoi += (obj) =>
        {
            if (PassPoiTimestamp.ContainsKey(obj))
            {
                return;
            }

            // 効果は一度きり(値は適当)
            PassPoiTimestamp[obj] = 0.0f;
            // 時間回復
            this.remainTime += this.RecoveryTime;
            // 回復時間表示を消す
            var timeObj = obj.transform.Find("time").gameObject;
            timeObj.SetActive(false);
            // 各種演出
            ParticleGen.Instance.PlayOnce("Effects/ef_014", animal.transform);
            RaceWindow.ShowPassGateText(obj.name);
            SeManager.Instance.Play(Liver.Sound.SeKind.Time_Recovery.ToString(), 0.0f, false, 1.0f);
        };
        // アニマルカーのデータを生成する
        this.animalData = AnimalFactory.Create(Liver.RaceWindow.AnimalKind);
        controller.animal = this.animalData;
        {
            // 各アニマルカー向けに調整
            var child = collider.transform.Find("ItemNormal");
            var co = child.GetComponent<BoxCollider>();
            co.size = this.animalData.ItemNormal;
            Debug.Log($"ItemNormal: {co.size}");
        }
        {
            // 各アニマルカー向けに調整
            var child = collider.transform.Find("ItemBoost");
            var co = child.GetComponent<BoxCollider>();
            co.size = this.animalData.ItemBoost;
            Debug.Log($"ItemBoost: {co.size}");
        }
        LaneChange.SetOnUpdate((x) =>
        {
            animal.transform.localPosition = new Vector3(x, animal.transform.localPosition.y, 0);
            shadow.transform.localPosition = new Vector3(x, shadow.transform.localPosition.y, 0);
        });
        RaceTime.Init();
    }

    // ゲートの時間表示を再開する
    IEnumerator RedisplayGateTime(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.SetActive(true);
    }


    IEnumerator Start()
    {
        enabled = false;
        Debug.Log($"Force location: {Liver.RaceWindow.ForceLocation}");
        LoadingProgress("Location check...");
        // 位置情報からマップを特定する
        var componemt = gameObject.AddComponent<Map.LocationDeterminer>();

        // NOTE リプレイ＆動作検証
        if (Liver.RaceWindow.ForceLocation)
        {
            Debug.Log("Force location.");
            componemt.ForceLocation(Liver.RaceWindow.Longitude, Liver.RaceWindow.Latitude);
        }

        yield return componemt.Wait();

        if (componemt.Disabled || componemt.TimeOut)
        {
            if (componemt.Disabled)
            {
                // GPSが使えない
                BeginErrorDialog(Liver.Entity.Dialog.Kind.NoGps);
            }
            else if (componemt.TimeOut)
            {
                // GPSが使えない
                BeginErrorDialog(Liver.Entity.Dialog.Kind.TimeOut);
            }

            // 位置の引き継ぎ解除
            Liver.RaceWindow.ForceLocation = false;
            yield break;
        }

        // マップ情報を収集
        this.mapPath = componemt.MapPath;
        this.enterPosition = componemt.EnterPosition;
        this.gridPos = Map.LocationDeterminer.LatLngToGrid(componemt.Latitude, componemt.Longitude);
        this.latitude = (float)componemt.Latitude;
        this.longitude = (float)componemt.Longitude;
        // NOTE 使い終わったら破棄
        Destroy(componemt);
        byte[] data = null;
        yield return LoadBeefmap((bytes) =>
        {
            data = bytes;
        });

        if (data != null)
        {
            yield return CreateBeefmap(data);
        }

        if (data == null || this.nodes.Count == 0)
        {
            // データ読み込み失敗 or ノード無し だとレースを始められない
            BeginErrorDialog(Liver.Entity.Dialog.Kind.NoMap);
            // 位置の引き継ぎ解除
            Liver.RaceWindow.ForceLocation = false;
            yield break;
        }

        // Loading画面の破棄
        Destroy(this.Loading.gameObject);
        this.Loading = null;
        // アニマルカー別BGM再生
        PlayRaceBgm();
        // カメラ演出の完了を待つ
        var cam = raceCamera.GetComponent<Tracking>();
        cam.StartDemo();

        while (cam.IsDemo())
        {
            yield return null;
        }

        // レース開始演 Ready -> Go!!
        yield return CountDownReady();
        var gesture = RaceWindow.gameObject.GetComponentInChildren<Gesture>();
        gesture.OnClick += OnClick;
        gesture.OnFlick += OnFlick;
        // 設定を変更
        gesture.threshold = RaceSettings.Instance.FlickAccThreshold;
        var threshold = RaceSettings.Instance.FlickMoveThreshold;
        gesture.moveThreshold = threshold * threshold;
        // 位置の引き継ぎ解除
        Liver.RaceWindow.ForceLocation = false;
        enabled = true;
    }


    // エラーダイアログ表示
    void BeginErrorDialog(Liver.Entity.Dialog.Kind type)
    {
        var df = Liver.Entity.Dialog.Load().bodies[(int)type];
        var dialog = RaceWindow.BaseScene.AddModalWindow<Liver.DialogWindow>("DialogWindow");
        dialog.SetDialog(df.Title, df.SubTitle, df.Message,
                         () =>
        {
            RaceWindow.BaseScene.ChangeScene("Menu", 1.0f);
            dialog.onClose();
        });
        dialog.EnableCloseButton(df.CloseButton);
    }


    IEnumerator LoadBeefmap(System.Action<byte[]> cb)
    {
        string folder = AppSettings.Instance.Folfer;
        string filePath = this.mapPath + ".beefmap";
        var fn = Path.Combine(Path.Combine(Application.temporaryCachePath, folder), Path.GetFileName(filePath));
        Debug.Log(fn);
        byte[] bytes = null;

        if (!Directory.Exists(Path.GetDirectoryName(fn)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fn));
        }

        if (File.Exists(fn))
        {
            /// MEMO : ローカルからロードする
            LoadingProgress("Loading map from cache...");
            Debug.Log("Load from cache.");
            bytes = File.ReadAllBytes(fn);
        }
        else
        {
            // MEMO サーバーから読み込み
            LoadingProgress("Loading map from server...");
            Debug.Log("Load from server.");
            string DefaultServerUrl = AppSettings.Instance.MapUrl();
            var path = Path.Combine(DefaultServerUrl, filePath);
            var www = new WWW(path);
            yield return www;

            if (string.IsNullOrEmpty(www.error))
            {
                bytes = www.bytes;
                File.WriteAllBytes(fn, bytes);
            }
        }

        cb(bytes);
    }


    IEnumerator CreateMiniMap(Transform worldTransform, List<Vector3> poiList)
    {
        // エリア全体をレンダリング
        var areaImage = Instantiate(Resources.Load("Race/MiniMap"), worldTransform) as GameObject;
        var co = areaImage.GetComponent<AreaImage>();
        co.ImageSize     = RaceSettings.Instance.MinimapSize;
        var minimapScale = 3000f / RaceSettings.Instance.MinimapSize;
        // 各種色設定
        Minimap.ApplyColors(ref co.Material);
        // レンダリングまち
        yield return new WaitForEndOfFrame();
        // 画像を入手
        var texture = co.RenderImage;
        var image = new Texture2D(texture.width, texture.height, TextureFormat.ARGB32, false)
        {
            wrapMode = TextureWrapMode.Clamp
        };
        RenderTexture.active = texture;
        image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        // POI位置を書き込み
        Minimap.ApplyPoi(poiList, ref image, 1.0f / minimapScale);
        Minimap.FillOuterFrame(ref image);
        image.Apply();
        RenderTexture.active = null;
        // RaceWindowに反映
        this.RaceWindow.MiniMap.texture = image;
#if UNITY_EDITOR
        // PNGにして書き出す
        var pngImage = image.EncodeToPNG();
        File.WriteAllBytes("./savedata/miniMap.png", pngImage);
#endif
        // 使用済みなので削除
        Destroy(areaImage);
    }

    // Minimap更新
    void ForceMinimap(float angle)
    {
        this.RaceWindow.ForceMiniMap(angle);
        this.RaceWindow.UpdateMiniMap(this.animal.transform.position, angle);
    }
    void UpdateMinimap(float angle)
    {
        this.RaceWindow.UpdateMiniMap(this.animal.transform.position, angle);
    }


    IEnumerator CreateAreaImage(Transform worldTransform, System.Action<AreaMap> cb)
    {
        // エリア全体をレンダリング
        var areaImage = Instantiate(Resources.Load("Race/AreaImage"), worldTransform) as GameObject;
        // レンダリングまち
        yield return new WaitForEndOfFrame();
        // 画像を入手
        var texture = areaImage.GetComponent<AreaImage>().RenderImage;
        var image = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        RenderTexture.active = texture;
        image.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        image.Apply();
        RenderTexture.active = null;
        // 画像を取り出す
        var areaMap = new AreaMap(image);
#if UNITY_EDITOR
        // PNGにして書き出す
        var pngImage = image.EncodeToPNG();
        File.WriteAllBytes("./savedata/areaImage.png", pngImage);
#endif
        // 使用済みなので削除
        Destroy(areaImage);
        cb(areaMap);
    }

#if false
    IEnumerator LoadRaceAreaImage(System.Action<byte[]> cb)
    {
        string folder = AppSettings.Instance.Folfer;
        var fn = Path.Combine(Path.Combine(Application.temporaryCachePath, folder), this.mapPath + ".png");
        byte[] bytes = null;

        if (!Directory.Exists(Path.GetDirectoryName(fn)))
        {
            // ディレクトリを生成
            Directory.CreateDirectory(Path.GetDirectoryName(fn));
        }

        if (File.Exists(fn))
        {
            /// MEMO : ローカルからロードする
            bytes = File.ReadAllBytes(fn);
            Debug.Log("Load from cache.");
        }
        else
        {
            string DefaultServerUrl = AppSettings.Instance.MapUrl();
            var path = Path.Combine(DefaultServerUrl, Path.GetFileName(fn));
            var www = new WWW(path);
            yield return www;
            Debug.Log("Load from server.");

            if (string.IsNullOrEmpty(www.error))
            {
                // キャッシュに書き出しとく
                bytes = www.bytes;
                File.WriteAllBytes(fn, bytes);
            }
        }

        cb(bytes);
    }
#endif


    IEnumerator CreateBeefmap(byte[] bytes)
    {
        // NOTE マップの中心位置はファイル名から推察
        var gridPosition = GetGridFromFileName(this.mapPath);
        var centerPosition = GetCenterPositionFromGridPosition(gridPosition);
        LoadingProgress("Create beef map...");
        // バイナリから初期化
        beefmap = new BeefMap();
        yield return beefmap.LoadAsync(bytes, centerPosition, MapUtility.WorldScale);
        // ノード一覧を保持
        nodes = beefmap.nodeMap.nodeMap;
        // メッシュ情報を配置するための親を作成する
        var world = new GameObject("World");
        world.transform.SetParent(transform, true);
        // グリッド座標でオブジェクトを管理
        // POI
        var gridAreaLarge = gameObject.AddComponent<Map.GridArea>();
        gridAreaLarge.GridSize = RaceSettings.Instance.POIGridSize;
        gridAreaLarge.Distance = this.animalData.POIGridDistance;
        // POI以外のオブジェクト
        var gridAreaSmall = gameObject.AddComponent<Map.GridArea>();
        gridAreaSmall.GridSize = RaceSettings.Instance.MapGridSize;
        gridAreaSmall.Distance = RaceSettings.Instance.MapGridDistance;
        this.itemGridArea = gameObject.AddComponent<Map.GridArea>();
        this.itemGridArea.GridSize = RaceSettings.Instance.MapGridSize;
        this.itemGridArea.Distance = RaceSettings.Instance.MapGridDistance;
        LoadingProgress("Create path model...");
        yield return new WaitForEndOfFrame();
        // マップデータ→道のモデルを生成
        // NOTE Mesh生成用クラスを作ってBeekSDKに渡している
        var builder = new Map.RoadModelBuilder(world.transform);
        beefmap.CreateRoadByNodeMap(builder.CreateMeshGameObject);
        // NOTE APIの仕様が変わってlayerは自前で設定
        world.SetLayerAllChildren(LayerMask.NameToLayer("Race"));

        // ノードが存在しない
        if (nodes.Count == 0)
        {
            yield break;
        }

        // 開始ノード
        current = Map.LocationDeterminer.SearchStartPosition(this.enterPosition, nodes);
        var currentNode = nodes[current];
        animalRoot.transform.localPosition = currentNode.Position;
        /// ランダムで次のノードを取得する
        var index = RaceWindow.ForceLocation ? RaceWindow.NextNodeIndex
                    : Random.Range(0, currentNode.Graph.Count);
        next = currentNode.Graph[index];
        // リトライのために保存
        RaceWindow.NextNodeIndex = index;
        // レース開始位置を緯度経度に戻しておく
        // NOTE エリア履歴で見た時にどこからレースを始めたか記録しておくため
        var latlng = Map.LocationDeterminer.PositionToLatLng(currentNode.Position, this.gridPos);
        this.latitude  = latlng.x;
        this.longitude = latlng.y;
        AnimalUpdate(0);
        raceCamera.GetComponent<Tracking>().Init(animalRoot.transform);
        List<Vector3> poiList = new List<Vector3>();
        var registerd_poi = CreatePOI(beefmap, world.transform, centerPosition, MapUtility.WorldScale, gridAreaLarge, ref poiList);
        var poi_with_gate = CreateGate(beefmap, world.transform, centerPosition, MapUtility.WorldScale, gridAreaSmall, registerd_poi);
        //CreateBallon(beefmap, world.transform, centerPosition, MapUtility.WorldScale);
        {
            // マップ上の賑やかし物配置のための準備
            LoadingProgress("Render Path...");
            yield return new WaitForEndOfFrame();
            // POIを全て表示
            gridAreaLarge.SetActive(true);
            // エリアをレンダリングして画像を得る
            AreaMap areaMap = null;
            yield return CreateAreaImage(world.transform, (data) => { areaMap = data; });
            // POIは全部非表示
            gridAreaLarge.SetActive(false);
            // マップ上に色々配置
            LoadingProgress("Layout objects...");
            yield return new WaitForEndOfFrame();
            var roadInfo = new Map.RoadInfo(this.nodes);
            yield return LayoutMapObjects(roadInfo, gridAreaSmall, areaMap);
            yield return LayoutItems(roadInfo, this.itemGridArea);
        }
        {
            // MiniMap
            LoadingProgress("Render Minimap...");
            yield return new WaitForEndOfFrame();
            var poiWithGate = CreatePoiWithGate(poi_with_gate, centerPosition, MapUtility.WorldScale);
            yield return CreateMiniMap(world.transform, poiWithGate);
            ForceMinimap(this.animal.transform.eulerAngles.y);
        }
        {
            // 花火
            LoadingProgress("Create Fireflower...");
            yield return new WaitForEndOfFrame();
            var fireFlower = GetComponent<FireFlower>();
            fireFlower.Setup();
        }
        // 地面
        Instantiate<GameObject>(Resources.Load<GameObject>("Models/Background/ground"), world.transform);
        // 外壁
        Instantiate(Resources.Load("Models/Background/Wall"), world.transform);
        // NOTE 時間差処理が終わってから!!
        gridAreaLarge.AnimalTransform     = this.animal.transform;
        gridAreaSmall.AnimalTransform     = this.animal.transform;
        this.itemGridArea.AnimalTransform = this.animal.transform;
    }

    // アイテム配置
    IEnumerator LayoutItems(Map.RoadInfo roadInfo, Map.GridArea gridArea)
    {
        var layouter = GetComponent<Map.ItemLayoutScene>();
        ItemBase.OnMeshSwitcher = AddMeshSwitcher;
        yield return layouter.Process(roadInfo, gridArea);
        // 用済み
        Destroy(layouter);
    }

    void AddMeshSwitcher(MeshSwitcher c)
    {
        this.normalStars.Add(c);

        // アイテム倍化効果有効中の場合
        if (currentEffect is Ratio)
        {
            BeginNormalStarDoubler(c);
        }
    }

    // 賑やかし配置
    IEnumerator LayoutMapObjects(Map.RoadInfo roadInfo, Map.GridArea gridArea, AreaMap areaMap)
    {
        {
            var comp = gameObject.GetComponent<Map.FenceLayouter>();
            yield return comp.Process(roadInfo, gridArea, areaMap);
            // 使い終わったら削除
            Destroy(comp);
            yield return null;
        }
        {
            var comp = gameObject.GetComponent<Map.EnlivenLayouter>();
            yield return comp.Process(gridArea, areaMap);
            //comp.CreateData();
            // 使い終わったら削除
            Destroy(comp);
            yield return null;
        }
    }


    HashSet<string> CreatePOI(BeefMap map, Transform parent, Vector2 worldCenter, Vector2 worldScale, Map.GridArea gridArea, ref List<Vector3> poiList)
    {
        // 登録したPOI
        HashSet<string> registerd_poi = new HashSet<string>();
        var poiSettings = Resources.Load("Configs/PoiSettings") as PoiSettings;

        foreach (var poi in beefmap.poiList)
        {
            // 3kmグリッド内か判定
            var pos = poi.WorldPostiton(worldCenter, worldScale);

            if (Mathf.Abs(pos.x) > Map.LocationDeterminer.GridSizeKilometer * 0.5f
                    || Mathf.Abs(pos.z) > Map.LocationDeterminer.GridSizeKilometer * 0.5f)
            {
                //Debug.Log($"exclude poi {poi.UniqueId()}");
                continue;
            }

            // 対応表からモデルを決める
            var poiType = poi.Type();
            var prefab = poiSettings.Get(poiType);

            if (prefab == null)
            {
                Debug.LogWarning($"Error!! {poiType}");
            }
            else
            {
                var p = poi.WorldPostiton(worldCenter, worldScale);
                var go = gridArea.Add(prefab, p);
                go.name = poi.Name();
                poiList.Add(p);
            }

            registerd_poi.Add(poi.UniqueId());
        }

        return registerd_poi;
    }

    // ゲートのあるPOIとそれ以外を判別するために
    // ゲート付きのPOIを戻す
    HashSet<string> CreateGate(BeefMap map, Transform parent, Vector2 worldCenter, Vector2 worldScale, Map.GridArea gridArea, HashSet<string> regist_id)
    {
        var layer = LayerMask.NameToLayer("Race");
        // gate
        int num = 0;
        var poi_with_gate = new HashSet<string>();
        // NOTE 加算時間は配置後に決まる
        List<TextMesh> meshComponents = new List<TextMesh>();

        foreach (var gate in beefmap.gateList)
        {
            if (!regist_id.Contains(gate.UniqueId()) && (string.CompareOrdinal(gate.Type(), "Station") != 0))
            {
                // POIとセットで登録する。駅だけはPOIの存在がなくても登録する。
                //Debug.Log($"exclude gate {gate.UniqueId()}");
                continue;
            }

            poi_with_gate.Add(gate.UniqueId());
            var p = new Vector3(0f, 1.5f, 0f) + gate.WorldPostiton(worldCenter, worldScale);
            var go = gridArea.Add(gatePrefab, p, gate.Rotation());
            go.name = gate.Name();
            {
                // 常にカメラの方を向く
                var timeObj = go.transform.Find("time").gameObject;
                var comp = timeObj.GetComponent<TowardCamera>();
                comp.raceCamera = this.raceCamera;
                var textObj = timeObj.transform.Find("text").gameObject;
                meshComponents.Add(textObj.GetComponent<TextMesh>());
            }
            num += 1;
        }

        Debug.Log($"POI: {regist_id.Count}  GATE: {num}  STATION: {beefmap.stationWorldPositions.Count}");
        // １ゲートあたりの回復量を決める
        // FIXME 最低1秒でいいの？
        RecoveryTime = Mathf.Max(1, RaceSettings.Instance.TotalRecoveryTime / Mathf.Max(num, 1));

        foreach (var comp in meshComponents)
        {
            comp.text = $"+{(int)RecoveryTime}s";
        }

        return poi_with_gate;
    }

    // ゲート付きPOIのリストを作成
    List<Vector3> CreatePoiWithGate(HashSet<string> poi_with_gate, Vector2 worldCenter, Vector2 worldScale)
    {
        var poiList = new List<Vector3>();

        foreach (var poi in beefmap.poiList)
        {
            if (!poi_with_gate.Contains(poi.UniqueId())) { continue; }

            var pos = poi.WorldPostiton(worldCenter, worldScale);
            poiList.Add(pos);
        }

        return poiList;
    }

    //// バルーン生成
    //void CreateBallon(BeefMap map, Transform parent, Vector2 worldCenter, Vector2 worldScale)
    //{
    //    var layer = LayerMask.NameToLayer("Race");
    //    foreach (var node in map.stationList)
    //    {
    //        // 3kmグリッド内か判定
    //        var pos = node.WorldPostiton(worldCenter, worldScale);
    //        if (Mathf.Abs(pos.x) > Map.LocationDeterminer.GridSizeKilometer * 0.5f
    //            || Mathf.Abs(pos.z) > Map.LocationDeterminer.GridSizeKilometer * 0.5f)
    //        {
    //            map.stationWorldPositions.Remove(node.Name());
    //            continue;
    //        }

    //        var name = node.Name();
    //        GameObject obj = Object.Instantiate(this.BaloonPrefab, pos, Quaternion.identity, parent.transform);
    //        obj.name = name + " baloon";
    //        {
    //            var c = obj.AddComponent<Baloon>();
    //            c.Text = name;
    //        }
    //        {
    //            var c = obj.AddComponent<TowardCamera>();
    //            c.raceCamera = this.raceCamera;
    //        }
    //        obj.SetLayerAllChildren(layer);
    //    }
    //}

    /// <summary>
    /// スター取得
    /// </summary>
    /// <param name="id"></param>
    void GetStar(int id)
    {
        totalStar += Mathf.CeilToInt(id * StarRatio);    // 仮で ID 分増加
        RaceWindow.TotalStar.text = totalStar.ToString();
        // スター取得時
        SeManager.Instance.Play(Liver.Sound.SeKind.got_star.ToString(), 0.0f, false, 1.0f, this.animal.transform);
    }

    /// <summary>
    /// パワーアップアイテム取得
    /// </summary>
    /// <param name="id"></param>
    void GetPowerup(PowerUpItem? type)
    {
        this.totalPowerup += 1;

        if (currentEffect != null)
        {
            currentEffect.End();
            currentEffect = null;
        }

        ItemData dat = default(ItemData);
        int lv = default(int);
        float endTime = default(float);

        if (type.HasValue)
        {
            dat = ItemData.Get(type.Value);
            lv = PlayerDataManager.PlayerData.GetPoweUpItemLevel(type.Value);
            endTime = RaceTime.realtimeSinceStartup + dat.Time[lv];
        }

        switch (type)
        {
            case PowerUpItem.Magnet:
                {
                    currentEffect = new Magnet(animal, endTime);
                }
                break;

            case PowerUpItem.Barrier:
                currentEffect = new Barrier(endTime);
                break;

            case PowerUpItem.Boost:
                currentEffect = new Boost(animalData, 1.2f, endTime);
                controller.PlayAnimation(AnimationType.Dash);
                currentEffect.OnEnd += () =>
                {
                    controller.PlayAnimation(AnimationType.Run);
                };
                break;

            case PowerUpItem.Ratio:
                currentEffect = new Ratio(this, 3, endTime);
                break;

            case PowerUpItem.EnergyDrink:
                currentEffect = new RedBull(animalData, endTime);
                break;

            case PowerUpItem.Tire:
                currentEffect = new Tire(endTime);
                break;
        }

        if (currentEffect != null)
        {
            RaceWindow.SetItem(type.Value);
            currentEffect.gauge = RaceWindow.Gauge;
            currentEffect.Start();
            // パワーアプアイテム取得時
            SeManager.Instance.Play(Liver.Sound.SeKind.got_powerups.ToString(), 0.0f, false, 1.0f, this.animal.transform);
            // パワーアップアイテム発動中のエフェクト音
            var handle = SeManager.Instance.Play(Liver.Sound.SeKind.SE_Powerup_Items.ToString(), 0.0f, true, 1.0f);
            // 効果エフェクト表示
            var particle = ParticleGen.Instance.Play("Effects/ef_001", animal.transform);
            currentEffect.OnEnd += () =>
            {
                ParticleGen.Instance.Push(particle);
                RaceWindow.SetItem(null);

                if (handle.HasValue) { SeManager.Instance.Stop(handle.Value, 0); }
            };
        }
    }

    // スター倍化
    // FIXME 必要ならコルーチンで分散処理
    public void BeginNormalStarDoubler()
    {
        foreach (var obj in this.normalStars)
        {
            // FIXME この判定は安全なのか？
            if (obj == null) { continue; }

            BeginNormalStarDoubler(obj);
        }
    }

    void BeginNormalStarDoubler(MeshSwitcher c)
    {
        c.Set(Random.Range(0f, 1f) < this.animalData.StarDoublerRate ? 1 : 2);
    }

    // スター倍化戻す
    // FIXME 必要ならコルーチンで分散処理
    public void EndNormalStarDoubler()
    {
        foreach (var obj in this.normalStars)
        {
            // FIXME この判定は安全なのか？
            if (obj == null) { continue; }

            obj.Set(0);
        }
    }


    IEnumerator CountDownReady()
    {
        yield return new WaitForSecondsRealtime(RaceCameraSettings.Instance.ReadyDelayTime);
        countdown = true;
        var window = RaceWindow;
        window.ReadyRace();
        yield return new WaitForSecondsRealtime(RaceCameraSettings.Instance.GoDelayTime);
        countdown = false;
        // 走行アニメーション開始
        controller.PlayAnimation(AnimationType.Run);
        // Go!!演出(移動開始と演出が被るので別コルーチンとして動かす)
        StartCoroutine(CountDownGo());
        yield return new WaitForSecondsRealtime(RaceCameraSettings.Instance.StartDelayTime);
        // スタートダッシュ!!
        animalData.Dash();
    }
    IEnumerator CountDownGo()
    {
        var window = this.RaceWindow;
        window.GoRace();
        yield return new WaitForSecondsRealtime(RaceCameraSettings.Instance.GoDispTime);
        window.BeginRace();
        UpdateRoadName();
        this.activeRace = true;
    }

    void PlayRaceBgm()
    {
        BgmManager.Instance.Play(Sound.AnimalBgm(RaceWindow.AnimalKind), 0.0f, 0.0f, true, 1.0f);
        Debug.Log("Play RaceBgm");
    }

    void Update()
    {
        RaceTime.Update();

        if (currentEffect != null)
        {
            if (currentEffect.Update() == false)
            {
                GetPowerup(null);
            }
        }

        if (!LaneChange.Ended)
        {
            LaneChange.Update(Time.deltaTime);
        }

        if (!LaneTab.Ended)
        {
            LaneTab.Update(Time.deltaTime);
        }

        if (Debug.isDebugBuild)
        {
            DevelopInputHack();

            if (Input.GetKeyDown(KeyCode.B))
            {
                OnObstacle(null);
            }
        }

        AnimalUpdate(Time.deltaTime);
        UpdateBranch();

        if (this.activeRace)
        {
            checkDisplayBranchButton();
            UpdateStation();
        }

        // MiniMap更新
        UpdateMinimap(this.raceCamera.transform.eulerAngles.y);
    }

    // 交差点を曲がるボタンを表示する判定
    void checkDisplayBranchButton()
    {
        this.branchDistance = NodeMapUtil.BranchDistance(beefmap.nodeMap, current, next, animalRoot.transform);
        var branchDisp = this.branchDistance < RaceSettings.Instance.BranchButtonDistance;
        this.RaceWindow.BranchLeft.SetActive(branchDisp);
        this.RaceWindow.BranchRight.SetActive(branchDisp);

        if (Debug.isDebugBuild)
        {
            this.RaceWindow.BranchDistance.text = string.Format("{0:0.00}m", this.branchDistance);
        }
    }


    private void OnObstacle(GameObject obj)
    {
        if (currentEffect is Barrier)
        {
            /// MEMO : バリア効果により障害物無視!!
            return;
        }

        // 障害物にぶつかり時間減る
        SeManager.Instance.Play(Liver.Sound.SeKind.hit_wall.ToString(), 0.0f, false, 1.0f, this.animal.transform);
        remainTime -= this.animalData.Penalty;
        animalData.Break();
        controller.PlayAnimation(AnimationType.Damage, () =>
        {
            controller.PlayAnimation((currentEffect is Boost) ? AnimationType.Dash : AnimationType.Run);
        });

        if (obj != null)
        {
        }
    }

    void AnimalUpdate(float deltaTime)
    {
        animalData.Updata(deltaTime);
        // 移動する
        var distance = animalData.Speed * deltaTime;

        while ((distance = NodeMapUtil.Move(beefmap.nodeMap, next, animalRoot.transform, distance)) > 0)
        {
            string front = "";
            var node = beefmap.nodeMap.nodeMap[next];

            if (node.Graph.Contains(branchNode))
            {
                front = branchNode;
                branchNode = null;
            }
            else
            {
                front = NodeMapUtil.FrontNode(beefmap.nodeMap, current, next);
            }

            var forward = (nodes[next].Position - nodes[current].Position).normalized;
            var diff = nodes[front].Position - nodes[next].Position;
            var rotate = CalcRotate(forward, diff);

            if (rotate > animalData.TurnAngle)
            {
                controller.PlayAnimation(AnimationType.TurnR, () =>
                {
                    controller.PlayAnimation((currentEffect is Boost) ? AnimationType.Dash : AnimationType.Run);
                });

                if (!(currentEffect is Tire)) { animalData.Deceleration(); }
            }
            else if (rotate < -animalData.TurnAngle)
            {
                controller.PlayAnimation(AnimationType.TurnL, () =>
                {
                    controller.PlayAnimation((currentEffect is Boost) ? AnimationType.Dash : AnimationType.Run);
                });

                if (!(currentEffect is Tire)) { animalData.Deceleration(); }
            }

            current = next;
            next = front;
            UpdateRoadName();
        };

        // 距離算出＆設定
        this.totalDistance += ((animalData.Speed * 1000) / 3600) * deltaTime;

        RaceWindow.TotalDistance.text = RaceArea.FormatDistance(this.totalDistance);

        // 残り時間
        remainTime -= deltaTime;

        RaceWindow.RemainTime.text = ((int)Mathf.Round(Mathf.Max(remainTime, 0))).ToString();

        this.RaceWindow.CurrentRemainTime = (int)Mathf.Round(remainTime);

        // 煽りBGM判定
        var curTime = (int)remainTime;

        if (curTime != this.prevTime)
        {
            int hurryTime = RaceSettings.Instance.HurryUpBgmTime;

            if (curTime <= hurryTime
                    && this.prevTime > hurryTime)
            {
                // 煽り開始
                BgmManager.Instance.Play(Sound.BgmKind.Hurryup, 0.0f, 0.0f, true, 1.0f);
                Debug.Log("Play Hurryup");
            }
            else if (this.prevTime <= hurryTime
                     && curTime > hurryTime)
            {
                // 通常BGMに戻す(Gate通過で時間が増えた)
                PlayRaceBgm();
            }

            this.prevTime = curTime;
        }

        // 動物の向き計算
        var dir = nodes[next].Position - nodes[current].Position;
        var rad = Mathf.Atan2(dir.x, dir.z);
        animalRoot.transform.localRotation = Quaternion.Euler(new Vector3(0, rad * Mathf.Rad2Deg, 0));

        if (remainTime < 0)
        {
            // 待機モーションにする
            controller.PlayAnimation(AnimationType.Idle);

            // パワーアップ効果消滅
            if (currentEffect != null)
            {
                currentEffect.End();
                currentEffect = null;
            }

            enabled = false;    // 更新停止する
            // スコア計算
            var score = Liver.Score.calc(this.totalDistance, this.totalStar, this.totalPowerup);
            // プレイ結果記録
            Liver.PlayerDataManager.PlayerData.SetResult(Liver.RaceWindow.AreaName,
                    this.latitude,
                    this.longitude,
                    Liver.RaceWindow.AnimalKind,
                    this.totalStar,
                    this.totalDistance,
                    score);
            Liver.Mission.RacePlayed(this.totalDistance, this.totalStar, this.totalPowerup);
            // リトライ用に位置を覚えておく
            RaceWindow.Latitude  = this.latitude;
            RaceWindow.Longitude = this.longitude;
            // サーバーに結果を送る
            string stageId = Liver.RaceWindow.AreaName;
            Tsl.Network.GameServer.Instance.SendScore(stageId, score, (int)Liver.RaceWindow.AnimalKind,
                    r =>
            {
                // responseデータは、GameServerクラスに保存されるので後から取得可能
                Debug.Log($"スコア送信完了 {r.Accepted}");
            });
            // 結果画面準備
            List<string> args = new List<string>();
            args.Add(((int)Liver.RaceWindow.AnimalKind).ToString());    // アニマルカー
            args.Add(this.totalDistance.ToString());                    // 走行距離
            args.Add(this.totalStar.ToString());                        // 取得スター
            args.Add(this.totalPowerup.ToString());                     // パワーアップ回数
            args.Add(score.ToString());                                 // スコア
            args.Add(stageId);
            // NOTE 終了演出は時間差で
            StartCoroutine(BeginResult(args));
        }
    }

    // 結果画面開始
    IEnumerator BeginResult(List<string> args)
    {
        this.RaceWindow.Timeup();
        yield return new WaitForSeconds(3.5f);
        this.RaceWindow.DispResult(args);
    }



    void UpdateBranchColor()
    {
        var window = this.RaceWindow;
        var index = currentBranchList.IndexOf(branchNode);
        var green = new Color(0.176f, 0.651f, 0.486f);
        var red   = new Color(1.000f, 0.533f, 0.604f);

        foreach (Transform t in window.BranchLeft.transform)
        {
            t.GetChild(0).GetComponent<Image>().color = green;
            t.GetChild(1).gameObject.SetActive(false);
        }

        foreach (Transform t in window.BranchRight.transform)
        {
            t.GetChild(0).GetComponent<Image>().color = green;
            t.GetChild(1).gameObject.SetActive(false);
        }

        if (index < this.numBranchLeft)
        {
            var t = window.BranchLeft.transform.GetChild(index);
            t.GetChild(0).GetComponent<Image>().color = red;
            t.GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            var t = window.BranchRight.transform.GetChild(index - this.numBranchLeft);
            t.GetChild(0).GetComponent<Image>().color = red;
            t.GetChild(1).gameObject.SetActive(true);
        }
    }

    void UpdateBranch()
    {
        string start;
        string end;
        var branchs = NodeMapUtil.BranchList(nodes, current, next, out start, out end);

        if (currentBranchList.Count != branchs.Count || !branchs.All(n => currentBranchList.Contains(n)))
        {
            // 角度の昇順に並べる
            branchs.Sort((a, b) =>
            {
                var forward = (nodes[end].Position - nodes[start].Position).normalized;
                var aDiff = nodes[a].Position - nodes[end].Position;
                var bDiff = nodes[b].Position - nodes[end].Position;
                return CalcRotate(forward, aDiff).CompareTo(CalcRotate(forward, bDiff));
            });
            var rot = branchs.Select((name) =>
            {
                var forward = (nodes[end].Position - nodes[start].Position).normalized;
                var diff = nodes[name].Position - nodes[end].Position;
                return CalcRotate(forward, diff);
            }).ToList();
            var window = this.RaceWindow;
            // UI更新
            {
                // 一旦全部消す
                foreach (Transform t in window.BranchLeft.transform)
                {
                    t.gameObject.SetActive(false);
                }

                foreach (Transform t in window.BranchRight.transform)
                {
                    t.gameObject.SetActive(false);
                }

                // 左方向へ曲がる
                var cnt = window.BranchLeft.transform.childCount;
                int index = 0;

                for (int i = index; i < rot.Count; ++i)
                {
                    if (rot[i] > 0.0f) { break; }

                    var t = window.BranchLeft.transform.GetChild(i);
                    t.gameObject.SetActive(true);
                    t.GetChild(0).localEulerAngles = new Vector3(0, 0, -rot[i]);
                    index += 1;
                }

                // 右方向へ曲がる
                cnt = window.BranchRight.transform.childCount;

                for (int i = index; i < rot.Count; ++i)
                {
                    var t = window.BranchRight.transform.GetChild(i - index);
                    t.gameObject.SetActive(true);
                    t.GetChild(0).localEulerAngles = new Vector3(0, 0, -rot[i]);
                }

                // 左右の分岐数
                this.numBranchLeft  = index;
                this.numBranchRight = rot.Count - index;
            }
            currentBranchList = branchs;

            if (branchs.Count > 0)
            {
                branchNode = branchs[rot.IndexOf(rot.Nearest(0))];
                UpdateBranchColor();
            }
        }
    }


    Dictionary<string, float> stationRotate = new Dictionary<string, float>();
    List<string> sortStation;// = new List<string>();
    /// <summary>
    /// 駅を表示する
    /// </summary>
    void UpdateStation()
    {
        if (sortStation == null)
        {
            sortStation = beefmap.stationWorldPositions.Keys.ToList();
        }

        // 進行方向の１８０以内の駅をリストアップ
        foreach (var data in beefmap.stationWorldPositions)
        {
            var forward = (nodes[next].Position - animalRoot.transform.position).normalized;
            var diff = data.Value - animalRoot.transform.position;
            stationRotate[data.Key] = CalcRotate(forward, diff);
        }

        var sorted = sortStation.Where((n) =>
        {
            return Mathf.Abs(stationRotate[n]) < (RaceSettings.Instance.ViewingAngle / 2);
        }).ToList();
        sorted.Sort((a, b) =>
        {
            var da = (animalRoot.transform.position - beefmap.stationWorldPositions[a]).sqrMagnitude;
            var db = (animalRoot.transform.position - beefmap.stationWorldPositions[b]).sqrMagnitude;
            return da.CompareTo(db);
        });
        sorted = sorted.Take(3).ToList();
        sorted.Sort((a, b) =>
        {
            return stationRotate[a].CompareTo(stationRotate[b]);
        });
        RaceWindow.SetStations(sorted, beefmap);
    }


    /// <summary>
    /// 道の名前を更新する
    /// </summary>
    void UpdateRoadName()
    {
        string name = "";

        foreach (var r in beefmap.namedRoad)
        {
            if (r.Value.Contains(current) && r.Value.Contains(next))
            {
                name = r.Key;
                break;
            }
        };

        RaceWindow.Road.text = name;
    }

    /// <summary>
    /// 角度計算
    /// </summary>
    /// <param name="forward"></param>
    /// <param name="diff"></param>
    /// <returns></returns>
    float CalcRotate(Vector3 forward, Vector3 diff)
    {
        var axis = Vector3.Cross(forward, diff);
        return Vector3.Angle(forward, diff) * (axis.y < 0 ? -1 : 1);
    }

    void DevelopInputHack()
    {
#if UNITY_EDITOR

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            OnFlick(Gesture.Flick.Up);
        }

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            OnFlick(Gesture.Flick.Left);
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            OnFlick(Gesture.Flick.Right);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            OnFlick(Gesture.Flick.Down);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeLane(Lane.Left);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeLane(Lane.Center);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeLane(Lane.Right);
        }

        // 残り時間いじる
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (remainTime >= 15) { remainTime = 15; }
            else { remainTime = 0; }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            this.remainTime += 15;
        }

        // アイテム効果発動
        if (Input.GetKeyDown(KeyCode.F1)) { GetPowerup(PowerUpItem.Magnet); }

        if (Input.GetKeyDown(KeyCode.F2)) { GetPowerup(PowerUpItem.Barrier); }

        if (Input.GetKeyDown(KeyCode.F3)) { GetPowerup(PowerUpItem.Boost); }

        if (Input.GetKeyDown(KeyCode.F4)) { GetPowerup(PowerUpItem.Ratio); }

        if (Input.GetKeyDown(KeyCode.F5)) { GetPowerup(PowerUpItem.EnergyDrink); }

        if (Input.GetKeyDown(KeyCode.F6)) { GetPowerup(PowerUpItem.Tire); }

        // ゲート通過
        if (Input.GetKeyDown(KeyCode.G)) { this.RaceWindow.ShowPassGateText("ほげふが"); }

#endif
    }


    // AudioLisner付け替え
    void EnableAudioListener(Camera camera, bool value)
    {
        if (camera == null) { return; }

        var comp = camera.GetComponent<AudioListener>();

        if (comp == null) { return; }

        comp.enabled = value;
    }


    void OnEnable()
    {
        // 3D効果を得るためraceCameraのListenerを使う
        EnableAudioListener(Camera.main,     false);
        EnableAudioListener(this.raceCamera, true);
    }

    void OnDisable()
    {
        // レース以外はMainCameraを使う
        EnableAudioListener(Camera.main,     true);
        EnableAudioListener(this.raceCamera, false);
    }


    private void OnDestroy()
    {
        if (renderTexture)
        {
            RenderTexture.ReleaseTemporary(renderTexture);
        }

        if (animal)
        {
            GameObject.Destroy(animal);
        }

        if (animalRoot)
        {
            GameObject.Destroy(animalRoot);
        }

        SeManager.Instance.Stop(0);
    }

    void ChangeLane(Lane lane, float animation = 0.3f)
    {
        if (this.lane == lane) { return; }

        this.lane = lane;
        float start = animal.transform.localPosition.x;
        float end = 0f;

        switch (lane)
        {
            case Lane.Left:
                end = -1.8f;
                break;

            case Lane.Right:
                end = +1.8f;
                break;
        }

        if (animation > 0)
        {
            LaneChange.Start(EasingPattern.Linear, animation, start, end);
        }
        else
        {
            LaneChange.Start(EasingPattern.Linear, 0, start, end);
            LaneChange.ForceEnd();
        }
    }

    void OnClick(Vector2 pos)
    {
        /// MEMO : スワイプでレーン変更なので、タップ処理無効にします
        if (swipeToLaneChange) { return; }

        var window = RaceWindow;
        var gesture = window.GetComponentInChildren<Gesture>();

        foreach (RectTransform rt in gesture.transform)
        {
            if (RectTransformToScreenSpace(rt).Contains(pos))
            {
                switch (rt.name)
                {
                    case "Lane1":
                        ChangeLane(Lane.Left);
                        break;

                    case "Lane2":
                        ChangeLane(Lane.Center);
                        break;

                    case "Lane3":
                        ChangeLane(Lane.Right);
                        break;
                }

                //　未完了のがあれば先に完了させます
                if (!LaneTab.Ended) { LaneTab.ForceEnd(); }

                var image = rt.GetComponent<Image>();
                LaneTab.SetOnUpdate((alpha) =>
                {
                    var c = image.color;
                    c.a = alpha;
                    image.color = c;
                });
                LaneTab.SetFinishEvent(() =>
                {
                    image.color = new Color(1, 1, 1, 0);
                });
                LaneTab.Start(EasingPattern.ExponentialOut, 1, 0.25f, 0);
            }
        }
    }
    public Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }

    void UTurn()
    {
        var temp = current;
        current = next;
        next = temp;

        switch (lane)
        {
            case Lane.Left:
                ChangeLane(Lane.Right, 0);
                break;

            case Lane.Right:
                ChangeLane(Lane.Left, 0);
                break;
        }

        // Uターン時
        SeManager.Instance.Play(Liver.Sound.SeKind.turn_around.ToString(), 0.0f, false, 1.0f, this.animal.transform);
    }

    /// <summary>
    /// 分岐方向を変更します
    /// 現在の分岐から +- 1 で左右切り替え
    /// </summary>
    /// <param name="offset"></param>
    public void BranchListChange(int offset)
    {
        if (currentBranchList.Count <= 0) { return; }

        var currentIndex = currentBranchList.IndexOf(branchNode);
        branchNode = currentBranchList[Mathf.Clamp(currentIndex + offset, 0, currentBranchList.Count - 1)];
        UpdateBranchColor();
    }

    // 強制変更
    public void BranchListForceChange(int index)
    {
        if (currentBranchList.Count <= 0) { return; }

        // ボタン番号→index
        if (index >= 4)
        {
            index = index - 4 + this.numBranchLeft;
        }

        this.branchNode = currentBranchList[Mathf.Clamp(index, 0, this.currentBranchList.Count - 1)];
        UpdateBranchColor();
    }


    void OnFlick(Gesture.Flick dir)
    {
        if (countdown)
        {
            if (dir == Gesture.Flick.Left || dir == Gesture.Flick.Right)
            {
                var temp = current;
                current = next;
                next = temp;
                AnimalUpdate(0);
            }
        }
        else
        {
            if (dir == Gesture.Flick.Down)
            {
                controller.PlayAnimation(AnimationType.TurnU, () =>
                {
                    UTurn();
                    controller.PlayAnimation((currentEffect is Boost) ? AnimationType.Dash : AnimationType.Run);
                });
            }
            else if (dir == Gesture.Flick.Up)
            {
                controller.Jump(() =>
                {
                    controller.PlayAnimation((currentEffect is Boost) ? AnimationType.Dash : AnimationType.Run);
                });
            }
            else if (dir == Gesture.Flick.Left)
            {
                if (swipeToLaneChange)
                {
                    ChangeLane((Lane)Mathf.Max((int)(lane - 1), (int)Lane.Left));
                }
            }
            else if (dir == Gesture.Flick.Right)
            {
                if (swipeToLaneChange)
                {
                    ChangeLane((Lane)Mathf.Min((int)(lane + 1), (int)Lane.Right));
                }
            }
        }
    }

    public static Vector2Int GetGridFromFileName(string filename)
    {
        var firstFilePathInfo = Path.GetFileNameWithoutExtension(filename).Split('_');
        var gridX = int.Parse(firstFilePathInfo[1]);
        var gridY = int.Parse(firstFilePathInfo[2]);
        return new Vector2Int(gridX, gridY);
    }

    public static Vector2 GetCenterPositionFromGridPosition(Vector2Int gridPosition)
    {
        return MapUtility.NihonCenterPosition +
               new Vector2((float)(MapUtility.OneKilometerDegree.x * Map.LocationDeterminer.GridSize * gridPosition.x),
                           (float)(MapUtility.OneKilometerDegree.y * Map.LocationDeterminer.GridSize * gridPosition.y));
    }

    // 走行距離をテキストに変換
    public static string FormatDistance(float distance)
    {
        return $"{distance:F1}m";
    }

    // Loading画面にテキストを出力(テスト用)
    void LoadingProgress(string text)
    {
        if (this.Loading) { this.Loading.Progress.text = text; }
    }
}

public static class GameObjectExtension
{
    public static void SetLayerAllChildren(this GameObject gameObject, int layerNo)
    {
        gameObject.layer = layerNo;

        foreach (Transform childTransform in gameObject.transform)
        {
            SetLayerAllChildren(childTransform.gameObject, layerNo);
        }
    }
}
