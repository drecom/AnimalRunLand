using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Shand;

namespace Liver.Entity
{
[CreateAssetMenu(menuName = "Configs/RaceCameraSettings")]
public class RaceCameraSettings : ScriptableObject
{
    [Tooltip("カメラ演出開始時の高さ(単位:m)")]
    public float StartHeight = 6000;

    [Tooltip("演出が始まるまでの待ち時間(秒)")]
    public float CameraDelayTime = 1;

    [Tooltip("レース時の高さへ移行する時間(秒)")]
    public float PositionTime = 3.5f;
    [Tooltip("滑らか移動アニメーションのパターン")]
    public EasingPattern PositionPattern = EasingPattern.QuarticOut;

    [Tooltip("スピン開始を始めるまでの猶予時間(秒)")]
    public float RotationDelay = 2.8f;
    [Tooltip("スピンしてレース時の向きに合わせる時間(秒)")]
    public float RotationTime = 2.8f;
    [Tooltip("スピンアニメーションのパターン")]
    public EasingPattern RotationPattern = EasingPattern.ExponentialIn;

    [Tooltip("スピンが終わった後に前方を向く時間(秒)")]
    public float PitchTime = 0.7f;
    [Tooltip("滑らかに前方を向くアニメーションのパターン")]
    public EasingPattern PitchPattern = EasingPattern.QuinticInOut;

    [Tooltip("演出が終わったのちにReadyを表示するまでの待ち時間(秒)")]
    public float ReadyDelayTime = 1;
    [Tooltip("Ready表示が終わったのちGoを表示するまでの待ち時間(秒)")]
    public float GoDelayTime = 1;
    [Tooltip("ReadyGo表示後レースが始まるまでの待ち時間(秒)")]
    public float StartDelayTime = 0.5f;
    [Tooltip("Goの表示時間(秒)")]
    public float GoDispTime = 1;


    static RaceCameraSettings _Instance;
    public static RaceCameraSettings Instance
    {
        get
        {
            if (_Instance == null) { _Instance = Resources.Load<RaceCameraSettings>("Configs/RaceCameraSettings"); }

            return _Instance;
        }
    }
}
}
