using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tsl.UI;
using Shand;
using System;

public enum AnimationType
{
    Idle,       // 待機
    Dash,       // ダッシュ
    Damage,     // 障害物の衝突
    Jump,       // 跳ぶ
    Run,        // 通常の走り
    TurnL,      // 左曲がり
    TurnR,      // 右曲がり
    TurnU,      // Uターン
}

public class AnimalController : MonoBehaviour
{
    Animator animator;
    Easing easing = new Easing();
    EasingVector3 easingShadow = new EasingVector3();
    public Animal animal { get; set; }
    public event Action<GameObject> OnGetItem = (obj) => { };
    public event Action<GameObject> OnObstacle = (obj) => { };
    public event Action<GameObject> OnWall = (obj) => { };
    public event Action<GameObject> OnPoi = (obj) => { };

    List<Easing> items = new List<Easing>();
    Coroutine coroutine = null;
    Dictionary<AnimationType, GameObject> effects = new Dictionary<AnimationType, GameObject>();
    GameObject shadow;

    /// <summary>
    /// 状態によって自動的切り替えエフェクト追加
    /// 同時再生は 1 つです
    /// </summary>
    /// <param name=""></param>
    /// <param name=""></param>
    public void AutoEffect(AnimationType type, GameObject effect)
    {
        effects.Add(type, effect);
    }
    private void Awake()
    {
        //animator = GetComponent<Animator>();
        this.animator = GetComponentInChildren<Animator>();
        easing.SetOnUpdate(OnEasingUpdate);
        easingShadow.SetOnUpdate(OnEasingShadowUpdate);
    }

    void Start()
    {
        PlayAnimation(AnimationType.Idle);

        // 障害物と衝突時のコールバックを設定する
        foreach (var c in GetComponentsInChildren<ObstacleCollider>(true))
        {
            c.OnCollision += OnObstacle;
            c.OnWall += OnWall;
        }

        // アイテムと衝突時のコールバックを設定する
        foreach (var c in GetComponentsInChildren<ItemCollider>(true))
        {
            c.OnCollision += OnItemGet;

            if (c.poiEnable) { c.OnPoi += OnPoi; }
        }
    }
    public void SetShadow(GameObject shadow)
    {
        this.shadow = shadow;
    }

    private void Update()
    {
        if (!easing.Ended) { easing.Update(Time.deltaTime); }

        if (!easingShadow.Ended) { easingShadow.Update(Time.deltaTime); }

        for (int i = items.Count - 1; i >= 0; i--)
        {
            items[i].Update(Time.deltaTime);
        }
    }

    public void PlayAnimation(AnimationType type, Action cb = null)
    {
        foreach (var ef in effects)
        {
            ef.Value.SetActive(ef.Key == type);
        }

        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = StartCoroutine(PlayAnimationIn(type, cb));
    }

    IEnumerator PlayAnimationIn(AnimationType type, Action cb)
    {
        var name = type.ToString().ToCharArray();
        name[0] = Char.ToLower(name[0]);
        animator.Play(new string(name));
        yield return null;
        yield return new WaitForAnimation(animator, 0);
        coroutine = null;

        if (cb != null) { cb(); }
    }

    public void Jump(Action cb = null)
    {
        if (!easing.Ended) { return; }

        PlayAnimation(AnimationType.Jump);
        var origin = animator.transform.parent.localPosition;
        var jump = animal.Jump; // Jump中にアイテム効果がなくなる可能性があるため、ローカル変数に保存して使う!

        if (jump == Animal.JumpStandard)
        {
            // ジャンプ（中）
            SeManager.Instance.Play(Liver.Sound.SeKind.jump_standard.ToString(), 0.0f, false, 1.0f, transform);
        }
        else
        {
            // ジャンプ（小）
            SeManager.Instance.Play(Liver.Sound.SeKind.jump_short.ToString(), 0.0f, false, 1.0f, transform);
        }

        easing.Start(EasingPattern.CircularOut, .3f, origin.y, origin.y + jump);
        easing.SetFinishEvent(() =>
        {
            easing.Start(EasingPattern.CircularIn, .3f, origin.y + jump, origin.y);
            easing.SetFinishEvent(() =>
            {
                if (cb != null) { cb(); }

                easing.SetFinishEvent(null);
            });
        });

        if (shadow)
        {
            var scale = shadow.transform.localScale;
            easingShadow.Start(EasingPattern.CircularOut, .3f, scale, scale * 0.5f);
            easingShadow.SetFinishEvent(() =>
            {
                easingShadow.Start(EasingPattern.CircularIn, .3f, scale * 0.5f, scale);
            });
        }
    }

    void OnEasingUpdate(float v)
    {
        var pos = animator.transform.parent.localPosition;
        pos.y = v;
        animator.transform.parent.localPosition = pos;
    }

    void OnEasingShadowUpdate(Vector3 scale)
    {
        shadow.transform.localScale = scale;
    }

    void OnItemGet(GameObject item, bool flag)
    {
        foreach (var c in item.GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }

        OnGetItem(item);
        // 消失のため ItemBase に座標情報を削除します
        var itembase = item.GetComponentInParent<ItemBase>();
        itembase.WillDestroy(item);

        if (flag)
        {
            var e = new Easing();
            items.Add(e);
            item.transform.SetParent(transform.parent);
            var start = item.transform.localPosition;
            e.SetOnUpdate((v) =>
            {
                if (item != null)
                {
                    item.transform.localPosition = Vector3.Lerp(start, transform.localPosition, v);
                }
            });
            e.SetFinishEvent(() =>
            {
                items.Remove(e);
                GameObject.Destroy(item);
            });
            e.Start(EasingPattern.ExponentialOut, .5f, 0.0f, 1.0f);
        }
        else
        {
            GameObject.Destroy(item);
        }
    }
}
