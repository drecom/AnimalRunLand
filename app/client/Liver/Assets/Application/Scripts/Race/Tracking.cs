///========================================
/// 指定オブジェクトの背後に追尾する
///========================================
using UnityEngine;
using System;
using Shand;
using Liver.Entity;

public class Tracking : MonoBehaviour
{
    Transform target;           // 対象
    public Vector3 distance = new Vector3(16, 8, 16);  // 離れる距離
    public float lerp = 0.1f;   // 補間
    public float pitchAngle = 8f;

    private EasingVector3 pos;
    private Easing rot;
    private Easing pitch;

    private Quaternion rotateStart;
    private Quaternion rotateEnd;

    Quaternion pitchStart;
    Quaternion pitchEnd;

    private RaceCameraSettings settings;

    // 開始演出中かどうか
    int demo      = 0;
    bool demoExec = false;
    float demoDelay = 0;


    public void Awake()
    {
        enabled = false;
    }

    // カメラ初期化
    public void Init(Transform target)
    {
        this.target = target;
        enabled     = true;
        this.settings = RaceCameraSettings.Instance;
        // カメラの最終位置
        TraceUpdate(1.0f);
        // 開始演出準備
        InitRaceStart();
    }

    private void LateUpdate()
    {
        // Demo中かどうかで処理を変えている
        if (IsDemo())
        {
            UpdateRaceStart();
        }
        else
        {
            TraceUpdate(this.lerp);
        }
    }


    // 通常の更新
    void TraceUpdate(float lerp)
    {
        var current = transform.localRotation.eulerAngles;
        current.x = pitchAngle;

        // 角度計算
        if (target.localRotation.eulerAngles.y - current.y >= 180)
        {
            current.y += 360;
        }
        else if (target.localRotation.eulerAngles.y - current.y <= -180)
        {
            current.y -= 360;
        }

        current.y += (target.localRotation.eulerAngles.y - current.y) * lerp;
        transform.localRotation = Quaternion.Euler(current);
        // 座標計算
        var rotation = Quaternion.AngleAxis(current.y, Vector3.up);
        var ofs = rotation * distance;
        transform.localPosition = target.transform.localPosition + ofs;
    }

    // レース開始演出
    void InitRaceStart()
    {
        // 位置
        var beginPos = new Vector3(0, this.settings.StartHeight, 0) + transform.localPosition;
        this.pos = new EasingVector3();
        this.pos.Start(this.settings.PositionPattern, this.settings.PositionTime, beginPos, transform.localPosition);
        this.pos.SetOnUpdate(value =>
        {
            transform.localPosition = value;
        });
        this.pos.SetFinishEvent(() =>
        {
            this.demo -= 1;
        });
        // 向き
        this.rotateStart = Quaternion.LookRotation(Vector3.down, Vector3.forward);
        var r = transform.localRotation;
        this.rotateEnd = Quaternion.Euler(90.0f, r.eulerAngles.y, 0.0f);
        this.rot = new Easing();
        // 開始Delayを実現
        this.rot.Start(EasingPattern.Linear, RaceCameraSettings.Instance.RotationDelay, 0.0f, 0.0f);
        this.rot.SetOnUpdate(value =>
        {
            transform.localRotation = Quaternion.Lerp(this.rotateStart, this.rotateEnd, value);
        });
        this.rot.SetFinishEvent(() =>
        {
            // Delay完了後に改めて演出を設定
            this.rot.Start(this.settings.RotationPattern, this.settings.RotationTime, 0.0f, 1.0f);
            this.rot.SetFinishEvent(() =>
            {
                // 最後に前方を向く
                this.pitch = new Easing();
                this.pitch.Start(this.settings.PitchPattern, this.settings.PitchTime, 0.0f, 1.0f);
                this.pitch.SetOnUpdate(value =>
                {
                    transform.localRotation = Quaternion.Lerp(this.pitchStart, this.pitchEnd, value);
                });
                this.pitch.SetFinishEvent(() =>
                {
                    this.demo -= 1;
                });
                this.pitchStart = transform.localRotation;
                this.pitchEnd = r;
            });
        });
        // 演出開始位置
        transform.localPosition = beginPos;
        transform.localRotation = this.rotateStart;
        this.demo      = 2;
        this.demoExec  = false;
        this.demoDelay = RaceCameraSettings.Instance.CameraDelayTime;
    }

    // カメラ演出開始
    public void StartDemo()
    {
        this.demoExec = true;
    }

    void UpdateRaceStart()
    {
        if (!this.demoExec) { return; }

        if (this.demoDelay > 0)
        {
            this.demoDelay -= Time.deltaTime;
            return;
        }

        this.pos.Update(Time.deltaTime);
        this.rot.Update(Time.deltaTime);
        this.pitch?.Update(Time.deltaTime);
    }


    public bool IsDemo()
    {
        return this.demo > 0;
    }
}
