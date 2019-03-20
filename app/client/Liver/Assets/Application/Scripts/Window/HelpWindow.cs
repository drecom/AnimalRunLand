using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Tsl.UI;
using Shand;

namespace Liver
{
public class HelpWindow : Window
{
    public Transform HelpRoot;
    public Sprite[] Sprites;
    public Image Prev;
    public Image Center;
    public Image Next;

    private float clickPosX = 0.0f;


    // 左右の画像を画面端に合わせる
    public RectTransform TopFrame;
    // 移動量も画面幅に合わせる
    float moveDistance;

    IEnumerator coroutineMethod;
    // true: 操作可能
    bool active = true;


    protected override void OnStart()
    {
        var gesture = HelpRoot.GetComponent<Gesture>();
        gesture.OnFlick += OnFlick;
        // 左右の画像を画面端に移動
        this.Prev.GetComponent<RectTransform>().anchoredPosition = new Vector2(this.TopFrame.rect.xMin, 0);
        this.Next.GetComponent<RectTransform>().anchoredPosition = new Vector2(this.TopFrame.rect.xMax, 0);
        // 画像と画面の幅から移動量を決める
        float w = this.Center.GetComponent<RectTransform>().rect.width;
        this.moveDistance = w + (this.TopFrame.rect.width - w) * 0.5f;
        SetDefaultPosition();
        this.Prev.gameObject.SetActive(true);
        this.Next.gameObject.SetActive(true);
    }

    // タッチイベント
    public void SetClickPosition()
    {
        this.clickPosX = Input.mousePosition.x;

        if (!this.active) { return; }

        if (this.coroutineMethod != null)
        {
            // スクロール中なら中断
            StopCoroutine(this.coroutineMethod);
            this.coroutineMethod = null;
        }
    }
    public void OnMove()
    {
        var x = Input.mousePosition.x;

        if (this.active)
        {
            var pos = this.HelpRoot.localPosition;
            pos.x += x - this.clickPosX;
            this.HelpRoot.localPosition = pos;
        }

        this.clickPosX = x;
    }
    public void CancelMove()
    {
        if (!this.active) { return; }

        // ある程度動いていたらページ切り替え
        var ofs = this.HelpRoot.localPosition.x;

        if (Mathf.Abs(ofs) > this.moveDistance * 0.5f)
        {
            ChangePage(ofs < 0);
        }
        else
        {
            // 中央に戻す
            RestorePage();
        }
    }

    private void OnFlick(Gesture.Flick dir)
    {
        if (!this.active) { return; }

        switch (dir)
        {
            case Gesture.Flick.Left:
                ChangePage(true);
                break;

            case Gesture.Flick.Right:
                ChangePage(false);
                break;

            default:
                // 上下フリックの場合は画像位置から向きを決める
                ChangePage(this.HelpRoot.localPosition.x < 0);
                break;
        }
    }

    void RestorePage()
    {
        if (this.coroutineMethod != null)
        {
            // ページ切り替え中なら中断
            StopCoroutine(this.coroutineMethod);
        }

        this.coroutineMethod = MoveImage(0, null);
        StartCoroutine(this.coroutineMethod);
    }

    void ChangePage(bool toLeft)
    {
        if (this.coroutineMethod != null)
        {
            // ページ切り替え中なら中断
            StopCoroutine(this.coroutineMethod);
        }

        this.active = false;
        this.coroutineMethod = MoveImage(toLeft ? -this.moveDistance
                                         : this.moveDistance,
                                         () =>
        {
            SetImageKind(toLeft);
            SetDefaultPosition();
            this.active = true;
        });
        StartCoroutine(this.coroutineMethod);
    }

    private IEnumerator MoveImage(float endPos, Action cb)
    {
        var ImageChange = new Easing();
        ImageChange.Start(EasingPattern.CubicOut, 0.3f, this.HelpRoot.localPosition.x, endPos);
        var pos = this.HelpRoot.localPosition;

        while (!ImageChange.Ended)
        {
            pos.x = ImageChange.UpdateValue(Time.deltaTime);
            this.HelpRoot.localPosition = pos;
            yield return null;
        }

        if (cb != null) { cb(); }

        this.coroutineMethod = null;
    }


    private void SetDefaultPosition()
    {
        var pos = this.HelpRoot.localPosition;
        pos.x = 0;
        this.HelpRoot.localPosition = pos;
        this.Prev.sprite   = this.Sprites[this.Sprites.Length - 1];
        this.Center.sprite = this.Sprites[0];
        this.Next.sprite   = this.Sprites[1];
    }

    private void SetImageKind(bool isLeft)
    {
        // 配列を直接書き換え
        if (isLeft)
        {
            for (int i = 0; i < this.Sprites.Length - 1; ++i)
            {
                Swap(ref this.Sprites[i], ref this.Sprites[i + 1]);
            }
        }
        else
        {
            for (int i = this.Sprites.Length - 1; i > 0; --i)
            {
                Swap(ref this.Sprites[i], ref this.Sprites[i - 1]);
            }
        }
    }

    static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
}
}
