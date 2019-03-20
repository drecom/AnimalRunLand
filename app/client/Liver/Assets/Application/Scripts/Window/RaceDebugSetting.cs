using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class RaceDebugSetting : Tsl.UI.Window
{
    public Transform Content;
    public Transform PointPanel;
    public Transform InputPanel;
    public Toggle PointTgl;
    public Toggle InputTgl;
    public Text SearchTxt;
    public InputField LatTxt;
    public InputField LngTxt;
    public Toggle swipeFlag;
    public InputField DistanceLimit;

    // 一回での手入力でレースを始めた
    static bool manualLocation = false;
    static double manualLatitude;
    static double manualLongitude;


    private void Awake()
    {
        var mapTestData = new Tsl.Entity.MapTestDatarOperate();
        mapTestData.LoadState(() =>
        {
            foreach (var dat in mapTestData.userData)
            {
                var btn = GetPrefab("UI/PointButton", this.Content).GetComponent<PointButton>();
                btn.PointNameTxt.text = dat.PointName;
                btn.LatTxt.text = dat.Latitude.ToString();
                btn.LngTxt.text = dat.Longitude.ToString();
                btn.GetComponent<Toggle>().group = this.Content.GetComponent<ToggleGroup>();
                btn.GetComponent<Toggle>().onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        RaceWindow.Latitude  = double.Parse(btn.LatTxt.text);
                        RaceWindow.Longitude = double.Parse(btn.LngTxt.text);
                        RaceWindow.ForceLocation = true;
                        // リトライでレースを始める仕組みを利用しているので、
                        // 開始時の向きを固定にする必要がある(この時点で取得できない)
                        RaceWindow.NextNodeIndex = 0;
                        Debug.Log($"{btn.PointNameTxt.text} lat:{RaceWindow.Latitude}  lng:{RaceWindow.Longitude}");
                    }
                });
            }

            // GPS位置を利用
            {
                var btn = GetPrefab("UI/PointButton", this.Content).GetComponent<PointButton>();
                btn.PointNameTxt.text = "GPS";
                btn.GetComponent<Toggle>().group = this.Content.GetComponent<ToggleGroup>();
                btn.GetComponent<Toggle>().onValueChanged.AddListener(isOn =>
                {
                    if (isOn)
                    {
                        RaceWindow.ForceLocation = false;
                        Debug.Log("Use GPS location.");
                    }
                });
            }
        });
        this.PointPanel.gameObject.SetActive(true);
        this.InputPanel.gameObject.SetActive(false);
        PointTgl.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                this.PointPanel.gameObject.SetActive(true);
                this.InputPanel.gameObject.SetActive(false);
            }
        });
        InputTgl.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                this.InputPanel.gameObject.SetActive(true);
                this.PointPanel.gameObject.SetActive(false);
            }
        });
        swipeFlag.isOn     = PlayerPrefs.GetInt("SwipeToLaneChange", 1) != 0;
        DistanceLimit.text = PlayerPrefs.GetInt("OnSwipeDistanceLimit", 5).ToString();

        // 過去のレース位置を書き込んでおく
        if (manualLocation)
        {
            Debug.Log("!!!");
            this.LatTxt.text = manualLatitude.ToString();
            this.LngTxt.text = manualLongitude.ToString();
        }

        RaceWindow.ForceLocation = false;
    }


    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "OkButton":
                Destroy(this.gameObject);
                break;

            case "CacheClear":
                CacheClear();
                break;

            case "StarPlus":
                StarPlus();
                break;

            case "StarMinus":
                StarMinus();
                break;

            case "Search":
                Search();
                break;

            case "NameOk":
                Validate(GameObject.Find("UserName"));
                break;

            case "InitPlayData":
                InitPlayData();
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }

    /// <summary>
    /// キャッシュクリア
    /// 現在は beefmap を削除のみ
    /// </summary>
    private void CacheClear()
    {
        var folder = Liver.Entity.AppSettings.Instance.Folfer;
        // NOTE 最後の一文字を置き換えて似た名前のフォルダを全て対象に
        folder = folder.Remove(folder.Length - 1) + '*';

        foreach (var dir in Directory.GetDirectories(Application.temporaryCachePath, folder))
        {
            Directory.Delete(dir, true);
        }
    }

    // スター増減
    private void StarPlus()
    {
        var num = Liver.PlayerDataManager.PlayerData.StarNum + 1000;
        Liver.PlayerDataManager.PlayerData.SetStarNum(num);
        SeManager.Instance.Play(Liver.Sound.SeKind.got_star.ToString(), 0.0f, false, 1.0f);
    }
    private void StarMinus()
    {
        var num = Mathf.Max(Liver.PlayerDataManager.PlayerData.StarNum - 1000, 0);
        Liver.PlayerDataManager.PlayerData.SetStarNum(num);
        SeManager.Instance.Play(Liver.Sound.SeKind.jump_short.ToString(), 0.0f, false, 1.0f);
    }


    private GameObject GetPrefab(string fileName, Transform content)
    {
        var prefab = Instantiate(Resources.Load(fileName)) as GameObject;
        prefab.transform.SetParent(content);
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localScale = Vector3.one;
        return prefab;
    }

    // 緯度経度→住所
    void Search()
    {
        var latitude  = double.Parse(this.LatTxt.text);
        var longitude = double.Parse(this.LngTxt.text);
        // 緯度経度→エリア名
        var grid = Map.LocationDeterminer.LatLngToGrid(latitude, longitude);
        var areaName = $"{grid.x}_{grid.y}";
        Map.AreaName.Get(areaName, (name) => { this.SearchTxt.text = name + "周辺"; });
        // その場所でレースを開始する
        manualLocation  = true;
        manualLatitude  = latitude;
        manualLongitude = longitude;
        RaceWindow.Latitude      = latitude;
        RaceWindow.Longitude     = longitude;
        RaceWindow.ForceLocation = true;
        RaceWindow.NextNodeIndex = 0;
    }

    // 名前入力
    void Validate(GameObject obj)
    {
        var com = obj.GetComponent<InputField>();
        Debug.Log(com.text);
        // 名前を検証
        var userName = new Liver.UserName();

        if (!userName.Validate(com.text))
        {
            Debug.Log("No use this id.");
        }
    }

    // セーブデータ初期化
    void InitPlayData()
    {
        // 問答無用で初期化
        // NOTE あえて即座にセーブしていない
        PlayerDataManager.Clear();
        SeManager.Instance.Play(Liver.Sound.SeKind.got_star.ToString(), 0.0f, false, 1.0f);
        Debug.Log("Init PlayerData.");
    }


    /// <summary>
    /// スワイプでレーンチェンジ On/Off
    /// </summary>
    /// <param name="toggle"></param>
    public void OnSwipeChange(Toggle toggle)
    {
        PlayerPrefs.SetInt("SwipeToLaneChange", toggle.isOn ? 1 : 0);
    }

    /// <summary>
    /// スワイプ制限距離
    /// </summary>
    /// <param name="toggle"></param>
    public void OnSwipeDistanceLimit(InputField input)
    {
        PlayerPrefs.SetInt("OnSwipeDistanceLimit", int.Parse(input.text));
    }
}
}
