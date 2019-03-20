using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImageCrossFadeManager : MonoBehaviour
{
    private Shand.Easing fadeAnimation;
    private Util.Waiter waiter;

    private static ImageCrossFadeManager _instance;
    private ImageCrossFadeManager() { }

    public static ImageCrossFadeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = GameObject.Find("ImageCrossFadeManager");

                if (obj == null)
                {
                    GameObject resource =  Resources.Load<GameObject>("UI/ImageCrossFadeManager");
                    GameObject instance = Instantiate(resource);
                    _instance = instance.AddComponent<ImageCrossFadeManager>();
                }
            }

            return _instance;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (this.waiter != null)
        {
            this.waiter.Update();
        }

        if (this.fadeAnimation != null)
        {
            this.fadeAnimation.Update(Time.deltaTime);
        }
    }

    private UnityEngine.UI.Image GetImage()
    {
        return _instance.transform.Find("Background").GetComponent<UnityEngine.UI.Image>();
    }

    public void SetAlpha(float alpha)
    {
        var image = GetImage();
        image.color = new Color(1.0f, 1.0f, 1.0f, alpha);
        int childCount = image.transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            var child = image.transform.GetChild(i);
            var render = child.GetComponent<Renderer>();

            if (render != null)
            {
                var c = render.material.color;
                render.material.color = new Color(c.r, c.g, c.b, alpha);
                continue;
            }

            var text = child.GetComponent<UnityEngine.UI.Text>();

            if (text != null)
            {
                text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            }
        }
    }

    public void StartFadeOut(Texture2D tex, Action<GameObject> configUi,
                             float durationSec, Action onEnd)
    {
        _instance.gameObject.SetActive(true);
        UnityEngine.UI.Image image = GetImage();
        image.overrideSprite = Sprite.Create(
                                   tex,
                                   new Rect(0, 0, tex.width, tex.height),
                                   new Vector2(0, 0)
                               );
        configUi(_instance.gameObject);
        SetAlpha(0.0f);
        this.fadeAnimation = new Shand.Easing();
        this.fadeAnimation.Start(Shand.EasingPattern.Linear, durationSec, 0.0f, 1.0f);
        this.fadeAnimation.SetOnUpdate(alpha =>
        {
            SetAlpha(alpha);
        });
        this.fadeAnimation.SetFinishEvent(() =>
        {
            this.fadeAnimation = null;
            onEnd();
        });
    }

    public void StartFadeIn(float durationSec, Action<GameObject> onEnd)
    {
        this.fadeAnimation = new Shand.Easing();
        this.fadeAnimation.Start(Shand.EasingPattern.Linear, durationSec, 1.0f, 0.0f);
        this.fadeAnimation.SetOnUpdate(alpha =>
        {
            SetAlpha(alpha);
        });
        this.fadeAnimation.SetFinishEvent(() =>
        {
            this.fadeAnimation = null;
            onEnd(_instance.gameObject);
            _instance.gameObject.SetActive(false);
        });
    }

    public void StartWait(Action startWaitF, Func<bool> isReadyF, Action onEnd)
    {
        startWaitF();
        this.waiter = new Util.Waiter();
        this.waiter.StartWait(isReadyF, () =>
        {
            this.waiter = null;
            onEnd();
        });
    }

    public bool IsFading
    {
        get { return this.fadeAnimation != null || this.waiter != null; }
    }
}

