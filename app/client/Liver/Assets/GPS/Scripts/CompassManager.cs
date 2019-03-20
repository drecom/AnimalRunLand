using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GPS
{

// コンパス機能を有効にし、向きを計算する
//   コンポーネントEnableでOn
//   コンポーネントDisableでOff
//
//   適当なGameObjectのコンポーネントとして使う
//   LocationServiceが有効な場合は真北を取得できる
//   (通常は磁北を取得)
//
public class CompassManager : MonoBehaviour
{
    // 滑らかに向きを変更する割合(1.0なら補間無し)
    public float InterporateRate = 0.3f;

    // true: コンパス機能が有効
    public bool Started { get; private set; }
    // 端末が向いている向き(Yawでの回転のみ)
    public Quaternion Rotation { get; private set; }

    IEnumerator coroutineMethod;

    double lastCompassUpdateTime = 0;
    float targetYaw;


    void Update()
    {
        if (!this.Started) { return; }

        if (Input.compass.timestamp > this.lastCompassUpdateTime)
        {
            //Debug.Log("Compass updated: " + Input.compass.timestamp + ", " + this.lastCompassUpdateTime);
            this.lastCompassUpdateTime = Input.compass.timestamp;
            // Yawだけ取得する
            // LocationServiceが有効な場合は真北が取れる
            this.targetYaw = (Input.location.status == LocationServiceStatus.Running)
                             ? Input.compass.trueHeading
                             : Input.compass.magneticHeading;
        }

        var tq = Quaternion.AngleAxis(this.targetYaw, Vector3.up);
        this.Rotation = Quaternion.Slerp(this.Rotation, tq, this.InterporateRate);
    }


    void OnEnable()
    {
        Debug.Log("Compass:OnEnable");
        // 初期化処理を実行
        this.coroutineMethod = enableCompass();
        StartCoroutine(this.coroutineMethod);
    }

    void OnDisable()
    {
        Debug.Log("Compass:OnDisable");
        // 初期化処理を停止
        StopCoroutine(this.coroutineMethod);
        this.coroutineMethod = null;
        Input.compass.enabled = false;
        this.Started = false;
    }


    IEnumerator enableCompass()
    {
        this.Started = false;
        this.Rotation = Quaternion.identity;
        this.targetYaw = 0;

        // NOTE Unity Remoteだとすぐに有効にならない
        // FIXME コンパスの無い実行環境で延々初期化を試みる
        while (!Input.compass.enabled)
        {
            // コンパスを有効にして一定時間待つ
            Input.compass.enabled = true;
            yield return new WaitForSeconds(1);
        }

        this.Started = true;
        Debug.Log("Compass enabled:" + Input.compass.timestamp);
    }
}
}
