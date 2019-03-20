///============================
/// ジェスチャー
/// ・タップ(クリック)
/// ・スワイプ
/// ・フリック
/// 上記の動作を同時に判別できる機能を提供します
///============================
using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace Tsl.UI
{
[RequireComponent(typeof(EventTrigger))]
public class Gesture : MonoBehaviour
{
    public bool EnableDirectionX = false;
    public bool EnableDirectionY = false;
    /// <summary>
    /// acceleration 用閾値。超えたらフリック作動します
    /// </summary>
    public float threshold = 200f;
    // 移動量での閾値
    public float moveThreshold = 10 * 10;

    EventTrigger trigger;
    Vector2 acceleration;
    float timeStamp;

    public event Action<Vector2> OnClick;
    public event Action<Vector2, Swip> OnSwip;
    public event Action<Flick> OnFlick;

    /// <summary>
    /// スワイプ状態
    /// </summary>
    public enum Swip
    {
        Begin,
        Swiping,
        End
    }

    /// <summary>
    /// フリック方向
    /// </summary>
    public enum Flick
    {
        Up,
        Down,
        Left,
        Right,
    }

    private void Awake()
    {
        // マルチタッチは禁止
        Input.multiTouchEnabled = false;
        trigger = GetComponent<EventTrigger>();
    }

    private void Start()
    {
        AddListener(EventTriggerType.PointerClick, PointerClick);
        AddListener(EventTriggerType.BeginDrag, BeginDrag);
        AddListener(EventTriggerType.Drag, Drag);
        AddListener(EventTriggerType.EndDrag, EndDrag);
    }

    /// <summary>
    /// EventTriggerType を指定して、イベントを登録する
    /// </summary>
    /// <param name="type"></param>
    /// <param name="cb"></param>
    private void AddListener(EventTriggerType type, Action<PointerEventData> cb)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((ev) => cb(ev as PointerEventData));
        trigger.triggers.Add(entry);
    }

    /// <summary>
    /// ポインタクリック
    /// </summary>
    /// <param name="ev"></param>
    private void PointerClick(PointerEventData ev)
    {
        if (ev.dragging) { return; }

        if (OnClick == null) { return; }

        var rect = RectTransformToScreenSpace(transform.GetComponent<RectTransform>());
        OnClick(ev.pressPosition - rect.position);
    }

    /// <summary>
    /// ドラッグ開始
    /// </summary>
    /// <param name="ev"></param>
    private void BeginDrag(PointerEventData ev)
    {
        this.timeStamp = Time.realtimeSinceStartup;
        Swiping(ev, Swip.Begin);
    }

    /// <summary>
    /// ドラッグ中
    /// </summary>
    /// <param name="obj"></param>
    private void Drag(PointerEventData ev)
    {
        Swiping(ev, Swip.Swiping);
        Calc(ev);
    }

    /// <summary>
    /// ドラッグ完了
    /// </summary>
    /// <param name="obj"></param>
    private void EndDrag(PointerEventData ev)
    {
        Swiping(ev, Swip.End);
        var d = (ev.position - ev.pressPosition).sqrMagnitude;
        CheckFlick(d);
    }


    /// <summary>
    /// 計算ロジック
    /// </summary>
    private void Calc(PointerEventData ev)
    {
        // 加速度計算
        var now = Time.realtimeSinceStartup;
        acceleration = ev.delta / (now - this.timeStamp);
        this.timeStamp = now;

        // 方向制限
        if (!EnableDirectionX) { acceleration.x = 0; }

        if (!EnableDirectionY) { acceleration.y = 0; }
    }

    /// <summary>
    /// スワイプ処理
    /// </summary>
    private void Swiping(PointerEventData ev, Swip state)
    {
        if (OnSwip == null) { return; }

        var delta = ev.position - ev.pressPosition;

        if (!EnableDirectionX) { delta.x = 0; }

        if (!EnableDirectionY) { delta.y = 0; }

        OnSwip(delta, state);
    }

    /// <summary>
    /// フリック判定
    /// </summary>
    private void CheckFlick(float d)
    {
        if (OnFlick == null) { return; }

        // ある程度ポインタが動かないとダメ
        Debug.Log($"Drag distance: {d}");

        if (d < this.moveThreshold) { return; }

        if (Math.Abs(acceleration.x) >= Math.Abs(acceleration.y))
        {
            if (acceleration.x <= -this.threshold) { OnFlick(Flick.Left); }

            if (acceleration.x >=  this.threshold) { OnFlick(Flick.Right); }
        }
        else
        {
            if (acceleration.y <= -this.threshold) { OnFlick(Flick.Down); }

            if (acceleration.y >=  this.threshold) { OnFlick(Flick.Up); }
        }
    }

    public Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }
}
}

