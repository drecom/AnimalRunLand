using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liver.Entity
{
[CreateAssetMenu(menuName = "Configs/RaceSettings")]
public class RaceSettings : ScriptableObject
{
    [Tooltip("レースの制限時間")]
    public float TimeLimit = 80;

    [Tooltip("フィールド全体の総回復時間（秒）：ゲート通過時回復時間 = TotalRecoveryTime / ゲート数")]
    public float TotalRecoveryTime = 3600;

    [Tooltip("POIを配置するグリッドの大きさ(単位:m)")]
    public int POIGridSize = 300;

    [Tooltip("POI以外のオブジェクトを配置するグリッドの大きさ(単位:m)")]
    public int MapGridSize = 100;
    [Tooltip("プレイヤーから見えるグリッド範囲(単位:グリッド数)")]
    public int MapGridDistance = 2;

    [Tooltip("煽りBGMに切り替わる残り秒数")]
    public int HurryUpBgmTime = 30;

    [Tooltip("駅表示の視野角度")]
    public int ViewingAngle = 50;

    [Tooltip("Minimapの直径(メートル)")]
    public float MinimapDiameter = 400.0f;
    [Tooltip("Minimapの画像サイズ(ピクセル)")]
    public int MinimapSize = 3000;

    [Tooltip("Minimapの地面の色")]
    public Color MinimapBaseColor;
    [Tooltip("Minimapの道の色")]
    public Color MinimapPathColor;
    [Tooltip("MinimapのPOIの色")]
    public Color MinimapPoiColor;

    [Tooltip("交差点までの距離がこの値を下回ると、交差点を曲がる操作ボタンが出現する(単位:m)")]
    public float BranchButtonDistance = 60;

    [Tooltip("フリック判定加速度(m/s) 超えたらフリックとみなす")]
    public float FlickAccThreshold = 50;
    [Tooltip("フリック判定移動量(pixel) 超えたらフリックとみなす")]
    public float FlickMoveThreshold = 100;

    static RaceSettings _Instance;
    public static RaceSettings Instance
    {
        get
        {
            if (_Instance == null) { _Instance = Resources.Load<RaceSettings>("Configs/RaceSettings"); }

            return _Instance;
        }
    }
}
}

