using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Shand;

namespace Tsl.UI
{
using AnimationType = ImageFader.AnimationType;

public class ImageFader : MonoBehaviour
{
    public enum AnimationType
    {
        SlideRight,
        SlideLeft,
        SlideUp,
        SlideDown,
        Zoom,
        None
    }

    public AnimationType openAnimationType = AnimationType.SlideRight;
    public AnimationType closeAnimationType = AnimationType.None;

    public RectTransform rectTransform; // アニメーションするRect(通常はPanelのRectTransformを設定)
    public Vector3 ZoomInPoint = Vector3.zero;
    public Action OnOpenAnimationFinished;
    [NonSerialized]
    public bool IsOpenAnimationFinished = false;
    [NonSerialized]
    // public UnityEngine.Rendering.PostProcessing.PostProcessVolume postProcessVolume = null;

    private Shand.Easing animation;

    void Awake()
    {
        // PostProcessingの設定のあるカメラをセット
        var canvas = this.GetComponent<Canvas>();

        if (canvas == null || canvas.worldCamera == null)
        {
            var mainCamera = GameObject.Find("Main Camera");

            if (mainCamera != null)
            {
                if (canvas != null) { canvas.worldCamera = mainCamera.GetComponent<Camera>(); }

                //this.postProcessVolume = mainCamera.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>();
            }
        }
        else
        {
            //this.postProcessVolume = canvas.worldCamera.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>();
        }

        if (this.rectTransform != null)
        {
            this.rectTransform.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("ImageFader::rectTransform is null");
        }
    }

    void Update()
    {
        if (this.animation != null)
        {
            if (!this.animation.Ended)
            {
                this.animation.Update(Time.deltaTime);
            }
            else
            {
                this.animation = null;
            }
        }
    }
    public void StartOpenAnimation()
    {
        StartAnimation(openAnimationType, true, () =>
        {
            this.IsOpenAnimationFinished = true;
            this.animation = null;

            if (this.OnOpenAnimationFinished != null) { this.OnOpenAnimationFinished(); }
        });
    }

    public void StartAnimation(AnimationType animationType, bool isOpen, Action end)
    {
        this.animation = new Easing();
        EasingPattern? pattern = null;
        float durationSec = 0f;
        float startValue = 0f;
        float endValue = 0f;
        var rect = this.rectTransform.rect;

        switch (animationType)
        {
            case AnimationType.SlideRight:
                pattern = EasingPattern.QuinticOut;
                durationSec = 0.3f;
                startValue = isOpen ? -rect.width : GetXPosition();
                endValue = isOpen ? GetXPosition() : rect.width;
                this.animation.SetOnUpdate(v => SetXPosition(v));
                break;

            case AnimationType.SlideLeft:
                pattern = EasingPattern.QuinticOut;
                durationSec = 0.3f;
                startValue = isOpen ? rect.width : GetXPosition();
                endValue = isOpen ? GetXPosition() : -rect.width;
                this.animation.SetOnUpdate(v => SetXPosition(v));
                break;

            case AnimationType.SlideUp:
                pattern = EasingPattern.QuinticOut;
                durationSec = 0.3f;
                startValue = isOpen ? -rect.height : GetYPosition();
                endValue = isOpen ? GetYPosition() : rect.height;
                this.animation.SetOnUpdate(v => SetYPosition(v));
                break;

            case AnimationType.SlideDown:
                pattern = EasingPattern.QuinticOut;
                durationSec = 0.3f;
                startValue = isOpen ? rect.height : GetYPosition();
                endValue = isOpen ? GetYPosition() : -rect.height;
                this.animation.SetOnUpdate(v => SetYPosition(v));
                break;

            case AnimationType.Zoom:
                pattern = isOpen ? EasingPattern.ElasticOut : EasingPattern.QuinticOut;
                durationSec = 0.3f;
                startValue = isOpen ? 0.5f : 1.0f;
                endValue = isOpen ? 1.0f : 0.0f;
                this.animation.SetOnUpdate(v => SetZoom(v));
                break;

            case AnimationType.None:

                /// HACK : Close の場合、ホワイトアウト処理なので、別対応します
                if (isOpen)
                {
                    this.animation = null;
                    this.rectTransform.gameObject.SetActive(true);
                    SetXPosition(GetXPosition());
                }

                return;
        }

        if (pattern.HasValue)
        {
            this.animation.Start(pattern.Value, durationSec, startValue, endValue);
        }
        else
        {
            this.animation = null;
        }

        if (this.animation != null)
        {
            this.animation.SetFinishEvent(end);
            this.animation.UpdateValue(0.0f);
        }
    }

    // panelの上に白いフェーダーを置いて、フェードアウトする
    public void CloseAnimation(Action onEnd, float duration = 0.3f)
    {
        if (closeAnimationType == AnimationType.None)
        {
            StartCoroutine(CloseAnimationCoroutine(onEnd, duration));
        }
        else
        {
            StartAnimation(closeAnimationType, false, onEnd);
        }
    }
    public IEnumerator CloseAnimationCoroutine(Action onEnd, float duration = 0.3f)
    {
        // TitleBarがあれば隠す
        //var titleBoard = this.GetComponentInChildren<TitleBoard>();
        //if (titleBoard != null) titleBoard.Hide();
        // 黒い板を作る
        var obj = new GameObject();
        var rect = obj.AddComponent<RectTransform>();
        var image = obj.AddComponent<UnityEngine.UI.Image>();
        rect.sizeDelta = this.rectTransform.sizeDelta;// new Vector2(720, 1280);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        image.color = new Color(0, 0, 0, 0);
        obj.transform.SetParent(this.rectTransform, false);
        var easing = new Shand.Easing();
        easing.Start(Shand.EasingPattern.Linear, duration, 0.0f, 1.0f);

        while (!easing.Ended)
        {
            var val = easing.UpdateValue(Time.deltaTime);
            image.color = new Color(0, 0, 0, val);
            yield return null;
        }

        Destroy(obj);
        onEnd();
    }




    public void SetBlur(float z)
    {
        this.rectTransform.gameObject.SetActive(true);
        this.rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
#if false
        // TODO: Unity2017対応
        UnityEngine.Rendering.PostProcessing.DepthOfField dof;

        if (this.postProcessVolume.profileRef.TryGetSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>(out dof))
        {
            dof.focusDistance.value = 400 + z * 500.0f;
        }

        UnityEngine.Rendering.PostProcessing.Bloom bloom;

        if (this.postProcessVolume.profileRef.TryGetSettings<UnityEngine.Rendering.PostProcessing.Bloom>(out bloom))
        {
            bloom.softKnee.value = 1.0f - z;
        }

#endif
    }
    public float GetXPosition()
    {
        return this.rectTransform.anchoredPosition.x;
    }

    public void SetXPosition(float x)
    {
        this.rectTransform.gameObject.SetActive(true);
        this.rectTransform.anchoredPosition = new Vector2(
            x,
            this.rectTransform.anchoredPosition.y
        );
    }

    public void SetZoom(float scale)
    {
        this.rectTransform.gameObject.SetActive(true);
        this.rectTransform.localScale = new Vector3(scale, scale, 1.0f);
        this.rectTransform.localPosition = Vector3.Slerp(this.ZoomInPoint, Vector3.zero, scale);
    }

    public float GetYPosition()
    {
        return this.rectTransform.anchoredPosition.y;
    }

    public void SetYPosition(float y)
    {
        this.rectTransform.gameObject.SetActive(true);
        this.rectTransform.anchoredPosition = new Vector2(
            this.rectTransform.anchoredPosition.x,
            y
        );
    }

}
}

