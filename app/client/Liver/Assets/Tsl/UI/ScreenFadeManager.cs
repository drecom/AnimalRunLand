using UnityEngine;
using System;
using System.Collections.Generic;

public class ScreenFadeManager : MonoBehaviour
{
    private enum Status
    {
        NotWorking,        // 未動作
        FadeOutProgress,   // フェードアウト中
        FadeOut,           // フェードアウト完了
        FadeInProgress,    // フェードイン中
    }

    private static ScreenFadeManager instance;
    private ScreenFadeManager() {}

    public static ScreenFadeManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(ScreenFadeManager)) as ScreenFadeManager;

                if (instance == null)
                {
                    GameObject obj = new GameObject("ScreenFadeManager");
                    instance = obj.AddComponent<ScreenFadeManager>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }

            return instance;
        }
    }

    private Status status = Status.NotWorking; // フェード中かどうか

    private Texture2D _bgtexture;
    private Texture2D _texture;

    private String _sequence = null;
    private Color _from;
    private Color _to;
    private Color _now;
    private float _time;

    public delegate void OnComplete(); 	// delegate
    private Queue<OnComplete> callbackQueue = new Queue<OnComplete>();
    private Util.Waiter waiter;
    private string loadingPanelName = string.Empty;
    private Transform panel = null;
    private bool hideFader = false;
    void Awake()
    {
        _bgtexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        _texture = new Texture2D(1, 1, TextureFormat.ARGB32, false); // Resources.Load<UnityEngine.Texture2D>("Textures/ResultFrameBase");
        _bgtexture.SetPixel(0, 0, Color.black);
        _bgtexture.Apply();
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        if (this.waiter != null)
        {
            this.waiter.Update();
        }
    }

    void OnGUI()
    {
        if (_now.a != 0)
        {
            GUI.color = _now;
            GUI.depth = -1000;

            if (!this.hideFader)
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _bgtexture);
            }

            GUI.depth = -1001;

            if (string.IsNullOrEmpty(loadingPanelName))
            {
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _texture);
            }
        }
    }

    // 共通処理
    void StartSequence(String function_name)
    {
        if (_sequence == null)
        {
            StopCoroutine(_sequence);
            _sequence = null;
        }

        _sequence = function_name;
        StartCoroutine(_sequence);
    }

    // フェード処理
    System.Collections.IEnumerator FadeUpdate()
    {
        float now_time = 0;

        while (0 < _time && now_time < _time)
        {
            now_time += Time.deltaTime;
            _now = Color.Lerp(_from, _to, now_time / _time);
            yield return 0;
        }

        _now = _to;

        if (this.status == Status.FadeOutProgress)
        {
            this.status = Status.FadeOut;
        }
        else if (this.status == Status.FadeInProgress)
        {
            this.status = Status.NotWorking;
        }
        else if (this.status != Status.NotWorking)
        {
            throw new Exception("ScreenFaderManagerの状態異常:" + this.status.ToString());
        }

        while (callbackQueue.Count > 0)
        {
            OnComplete callback = callbackQueue.Dequeue();
            callback();
        }
    }

    // 強制的にフェードインする
    public void ForceFadeIn()
    {
        if (_sequence == null)
        {
            StopCoroutine(_sequence);
            _sequence = null;
        }

        _to.a = 0;
        _now = _to;
        this.status = Status.NotWorking;
    }

    // フェードインを開始する
    public void FadeIn(float t_time, Color t_color, OnComplete t_cb)
    {
        Debug.LogWarning("ScreenFadeManager::FadeIn " + this.loadingPanelName);

        switch (this.status)
        {
            // 通常
            case Status.NotWorking:
            case Status.FadeOut:
                break;

            // フェードイン中はコールバックに加えるだけ
            case Status.FadeInProgress:
                Debug.LogWarning("Now fading, can't start fade in");
                callbackQueue.Enqueue(t_cb);
                return;

            // フェードアウト中はフェードインにスイッチ
            case Status.FadeOutProgress:
                while (callbackQueue.Count > 0)
                {
                    OnComplete callback = callbackQueue.Dequeue();
                    callback();
                }

                callbackQueue.Clear();
                break;
        }

        this.status = Status.FadeInProgress;

        if (this.panel != null) { this.panel.gameObject.SetActive(false); }

        this.hideFader = false;
        callbackQueue.Enqueue(() =>
        {
            t_cb();
        });
        //t_color = Color.white;
        _to = _from = t_color;
        _to.a = 0;
        _time = t_time;
        StartSequence("FadeUpdate");
    }

    // フェードアウトを開始する
    public void FadeOut(float t_time, Color t_color, OnComplete t_cb)
    {
        Debug.LogWarning("ScreenFadeManager::FadeOut " + this.loadingPanelName);

        switch (this.status)
        {
            // 通常
            case Status.NotWorking:
            case Status.FadeOut:
                break;

            // フェードアウト中はコールバックに加えるだけ
            case Status.FadeOutProgress:
                Debug.LogWarning("Now fading, can't start fade in");
                callbackQueue.Enqueue(t_cb);
                return;

            // フェードイン中はフェードアウトにスイッチ
            case Status.FadeInProgress:
                while (callbackQueue.Count > 0)
                {
                    OnComplete callback = callbackQueue.Dequeue();
                    callback();
                }

                callbackQueue.Clear();
                break;
        }

        this.status = Status.FadeOutProgress;
        //t_color = Color.white;
        callbackQueue.Enqueue(() =>
        {
            if (this.panel != null)
            {
                this.panel.gameObject.SetActive(true);
                this.hideFader = true;
            }

            t_cb();
        });
        _to = _from = t_color;
        _from.a = 0;
        _time = t_time;
        StartSequence("FadeUpdate");

        if (!string.IsNullOrEmpty(this.loadingPanelName))
        {
            var go = GameObject.Find("ScreenFadeManager") as GameObject;

            if (go != null)
            {
                this.panel = go.transform.Find("Panel");

                if (this.panel != null)
                {
                    bool exist = false;

                    for (int n = 0; n < this.panel.childCount; ++n)
                    {
                        var child = this.panel.GetChild(n);
                        child.gameObject.SetActive(child.name == loadingPanelName);

                        if (child.name == loadingPanelName) { exist = true; }
                    }

                    this.panel.gameObject.SetActive(false);
                    this.hideFader = false;

                    if (!exist)
                    {
                        Debug.LogError("ScreenFadeManager: not found child:" + this.loadingPanelName);
                    }
                }
                else
                {
                    Debug.LogError("ScreenFadeManager 'Panel' not found in GameObject");
                }
            }
            else
            {
                Debug.LogError("ScreenFadeManager not found in GameObject");
            }
        }
        else
        {
            Debug.LogWarning("loadingPanelName is empty");
        }
    }
    public void SetLoadingPanel(string name)
    {
        this.loadingPanelName = name;
    }

    public void StartWait(Func<bool> waitF, Action onEnd)
    {
        this.waiter = new Util.Waiter();
        this.waiter.StartWait(waitF, () =>
        {
            this.waiter = null;
            onEnd();
        });
    }

}
