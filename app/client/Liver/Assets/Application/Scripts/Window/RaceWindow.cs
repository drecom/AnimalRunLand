using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tsl.UI;
using Shand;

namespace Liver
{
public class RaceWindow : Tsl.UI.Window
{
    public RawImage raceImage;

    // 交差点の分岐をボタンで表示
    public GameObject BranchLeft;
    public GameObject BranchRight;

    public Text TotalDistance;
    public Text TotalStar;
    public Text RemainTime;
    public Text PassGateText;
    Vector3 gateOriginPosition;
    public Image item;
    public Image Gauge;
    public GameObject GaugeObject;
    private GameObject RaceArea;
    public Text BranchDistance;
    public RectTransform[] Stations;
    private Camera raceCamera;
    public Camera uiCamera;
    public BalloonTail[] Tails;
    public Text Road;

    // レース開始演出
    public GameObject ReadyObj;
    public GameObject GoObj;

    // 残り時間カウントダウン
    public Text TimeRemainText;
    [System.NonSerialized]
    public int CurrentRemainTime = 0;
    int prevRemainTime           = 0;

    // 時間切れ
    public GameObject TimeUpImage;
    // NOTE 時間切れ後はPauseボタンが効かない
    public Button PauseButton;

    // 本編内で共有するフィールド
    static public AnimalKind AnimalKind;
    static public string AreaName;

    // プレイした位置(リトライ用途)
    static public bool ForceLocation;
    static public double Latitude;
    static public double Longitude;
    // 開始時の向き
    static public int NextNodeIndex;

    // 道のマテリアル指定
    static public int RoadMaterial = 0;

    // MiniMap
    public RawImage MiniMap;
    public Image MiniMapBase;
    Material minimapMaterial;
    bool minimapAutoDirection = true;
    float minimapYaw = 0;


    Easing gate;// = new Easing();


    protected override void OnStart()
    {
        // 方向指示ボタンはレース開始までOff
        this.BranchLeft.SetActive(false);
        this.BranchRight.SetActive(false);
        // RaceArea 動的生成する
        RaceArea = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("Race/RaceArea"));
        raceCamera = RaceArea.GetComponent<RaceArea>().raceCamera;
        // アニマルカー別BGM再生
        //BgmManager.Instance.Play(Sound.AnimalBgm(RaceWindow.AnimalKind), 0.0f, 0.0f, true, 1.0f);
        // NOTE レース開始までPause禁止
        this.PauseButton.interactable = false;
        gateOriginPosition = PassGateText.transform.parent.position;
        // 通りの名前を初期化
        this.Road.text = "";
        // Imageのマテリアルは自分で複製する
        this.minimapMaterial  = Instantiate(this.MiniMap.material);
        this.MiniMap.material = this.minimapMaterial;

        if (Debug.isDebugBuild)
        {
            this.BranchDistance.gameObject.SetActive(true);
        }
    }

    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "StopButton":
                if (GameObject.Find("PauseWindow(Clone)") == null) { BaseScene.AddModalWindow<PauseWindow>("PauseWindow"); }
                else { GameObject.Find("PauseWindow(Clone)").transform.Find("Panel").gameObject.SetActive(true); }

                this.RaceArea.SetActive(false);
                var pause = GameObject.Find("PauseWindow(Clone)").GetComponent<PauseWindow>();
                pause.onClose = () =>
                {
                    this.RaceArea.SetActive(true);
                };
                pause.SetStatus(RaceWindow.AreaName, this.TotalDistance.text, this.TotalStar.text, "");
                break;

            case "Left":
                RaceArea.GetComponent<RaceArea>().BranchListChange(-1);
                break;

            case "Right":
                RaceArea.GetComponent<RaceArea>().BranchListChange(1);
                break;

            case "MinimapBase":
                // 北固定の切り替え
                this.minimapAutoDirection = !this.minimapAutoDirection;
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }

    // 分岐を変更する
    public void OnClickButtonBranch(int index)
    {
        RaceArea.GetComponent<RaceArea>().BranchListForceChange(index);
    }


    private void OnDestroy()
    {
        if (RaceArea != null)
        {
            GameObject.Destroy(RaceArea);
        }
    }

    public void SetItem(PowerUpItem? type)
    {
        if (type.HasValue)
        {
            GaugeObject.SetActive(true);
            item.sprite = Resources.LoadAll<Sprite>("Textures/UiParts/Item")[(int)type.Value];
        }
        else
        {
            GaugeObject.SetActive(false);
        }
    }

    public void SetStations(List<string> names, BeefMap beefmap)
    {
        List<Vector3> pos = new List<Vector3>();

        for (int i = 0; i < Stations.Length; i++)
        {
            if (i < names.Count)
            {
                Stations[i].gameObject.SetActive(true);
                Tails[i].gameObject.SetActive(true);
                Stations[i].GetComponentInChildren<Text>().text = names[i];
                var start = raceCamera.WorldToScreenPoint(beefmap.stationWorldPositions[names[i]]);
                Vector2 p;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(Stations[i].transform.parent.GetComponent<RectTransform>(), start, uiCamera, out p);
                var local = Stations[i].transform.localPosition;
                var parent = Stations[i].transform.parent.GetComponent<RectTransform>();
                float min = parent.rect.xMin + (Stations[i].sizeDelta.x / 2 + 10);
                local.x = Mathf.Max(p.x, min);// 左画面外対応
                Stations[i].transform.localPosition = local;
                Tails[i].focusPosition = start;
            }
            else
            {
                Stations[i].gameObject.SetActive(false);
                Tails[i].gameObject.SetActive(false);
            }
        }

        // 重ねないように一つずつ右へずらす
        for (int i = 1; i < names.Count; i++)
        {
            var lhs = Stations[i - 1].transform.localPosition;
            var width = Stations[i - 1].sizeDelta.x + 10;// / 2 + 10;
            var rhs = Stations[i].transform.localPosition;
            rhs.x = Mathf.Max(rhs.x, lhs.x + width);
            Stations[i].transform.localPosition = rhs;
        }

        // 右へ画面外になる場合、左側へ
        float? max = null;

        for (int i = names.Count - 1; i >= 0; i--)
        {
            var p = Stations[i].transform.parent.GetComponent<RectTransform>();

            if (!max.HasValue)
            {
                // 初回
                max = p.rect.xMax - (Stations[i].sizeDelta.x / 2 + 10);
            }
            else
            {
                max = max.Value - (Stations[i].sizeDelta.x + 10);
            }

            var local = Stations[i].transform.localPosition;
            local.x = Mathf.Min(local.x, max.Value);
            Stations[i].transform.localPosition = local;
        }
    }

    // レース前演出
    public void ReadyRace()
    {
        this.ReadyObj.SetActive(true);
    }

    public void GoRace()
    {
        this.GoObj.SetActive(true);
    }

    // レース開始
    public void BeginRace()
    {
        this.ReadyObj.SetActive(false);
        this.GoObj.SetActive(false);
        this.PauseButton.interactable = true;
    }

    // 時間切れでゲーム終了
    public void Timeup()
    {
        this.CurrentRemainTime = 0;
        this.prevRemainTime    = 0;
        this.TimeRemainText.gameObject.SetActive(false);
        this.TimeUpImage.SetActive(true);
        this.PauseButton.interactable = false;
        this.BranchLeft.SetActive(false);
        this.BranchRight.SetActive(false);

        // ボタン入力を禁止
        foreach (Transform t in this.BranchLeft.transform)
        {
            t.gameObject.GetComponent<Button>().interactable = false;
        }

        foreach (Transform t in this.BranchRight.transform)
        {
            t.gameObject.GetComponent<Button>().interactable = false;
        }

        BgmManager.Instance.Stop(0.1f);
        SeManager.Instance.Play(Liver.Sound.SeKind.timeup.ToString(), 0.0f, false, 1.0f);
    }

    // 結果表示
    public void DispResult(List<string> args)
    {
        BaseScene.AddModalWindow<ResultWindow, Window.Argments>("ResultWindow", new Window.Argments(args));
        this.RaceArea.SetActive(false);
    }



    void Update()
    {
        CountDownTimeRemain();

        if (gate != null && !gate.Ended)
        {
            gate.Update(Time.deltaTime);
        }
    }


    // 残り時間のカウントダウン(10 ... 1)
    void CountDownTimeRemain()
    {
        if (this.prevRemainTime != this.CurrentRemainTime)
        {
            // 一旦非アクティブにしてAnimationをリセット
            this.TimeRemainText.gameObject.SetActive(false);

            // NOTE カウントダウン中に時間が増えることもあるよ
            if (this.CurrentRemainTime <= 10)
            {
                this.TimeRemainText.gameObject.SetActive(true);
                this.TimeRemainText.text = (this.CurrentRemainTime).ToString();
                SeManager.Instance.Play(Sound.SeKind.Countdown.ToString(), 0.0f, false, 1.0f);
            }

            this.prevRemainTime = this.CurrentRemainTime;
        }
    }

    // Gate通過の表示
    public void ShowPassGateText(string name)
    {
        gate = new Easing();
        var bg = PassGateText.transform.parent;
        var rect = transform.GetComponent<RectTransform>().rect;
        PassGateText.text = name;
        gate.Start(EasingPattern.QuinticOut, 0.5f, -rect.width, gateOriginPosition.x);
        gate.SetOnUpdate((x) =>
        {
            var pos = bg.transform.position;
            pos.x = x;
            bg.transform.position = pos;
        });
        // 一回更新してから表示
        gate.Update(0);
        bg.gameObject.SetActive(true);
        gate.SetFinishEvent(() =>
        {
            gate.Start(EasingPattern.Linear, 2.5f, 0, 1);
            gate.SetOnUpdate((x) => { });
            gate.SetFinishEvent(() => bg.gameObject.SetActive(false));
        });
    }

    // MiniMap更新
    public void ForceMiniMap(float yaw)
    {
        this.minimapYaw = yaw;
    }

    public void UpdateMiniMap(Vector3 center, float yaw)
    {
        // center→正規化座標
        // FIXME エリアサイズが固定値
        var uv = new Vector2((center.x + 1500) / 3000.0f, (center.z + 1500) / 3000.0f);
        // 表示エリアに合わせて位置をずらす
        var s = Entity.RaceSettings.Instance.MinimapDiameter / 3000.0f;
        uv.x -= s * 0.5f;
        uv.y -= s * 0.5f;
        this.MiniMap.uvRect = new Rect(uv.x, uv.y, s, s);

        if (!this.minimapAutoDirection)
        {
            // 北固定モード
            yaw = 0;
        }

        // 贅沢にQuaternionで計算
        var from = Quaternion.AngleAxis(this.minimapYaw, Vector3.forward);
        var to   = Quaternion.AngleAxis(yaw, Vector3.forward);
        var q    = Quaternion.Lerp(from, to, 0.2f);
        this.minimapYaw = q.eulerAngles.z;
        var rv = new Vector3(0, 0, this.minimapYaw);
        this.MiniMap.rectTransform.localEulerAngles     = rv;
        this.MiniMapBase.rectTransform.localEulerAngles = rv;
        // UV0がマップとマスクの兼用
        this.minimapMaterial.SetFloat("_OffsetX", uv.x);
        this.minimapMaterial.SetFloat("_OffsetY", uv.y);
        this.minimapMaterial.SetFloat("_Scale", 1.0f / s);
    }
}
}
