using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Tsl.UI
{
using OpenAnimationType = ImageFader.AnimationType;


// Windowのベースクラス
// Window Frameと状態を持つすべてのUIはWindowクラスを派生する
public class Window : MonoBehaviour
{
    // デフォルトの引数型。Windowsの派生クラス毎に作成することができる
    public class Argments
    {
        public List<string> args;

        public Argments(List<string> args) { this.args = args; }
        public Argments(string arg) { this.args = arg.Split(',').ToList(); }
        public bool exist(string key) { return this.args.Any(s => s == key); }
    }
    // Windowのタイトル名 TitleBoardに表示される
    public string WindowTitleName = "Window Name";
    public Sprite WindowBackGround;
    public int WindowBackGroundType = 0;

    public enum BarAction { Disabled, Enabled, DoNotCare };
    public BarAction EnableTopBar = BarAction.Enabled;
    public BarAction EnableBottomBar = BarAction.Enabled;
    public BarAction EnableMenuButton = BarAction.DoNotCare;

    public Func<Transform, bool> OnClickHook; // ボタンなどのイベント発生時に呼ばれるfalseを返すと通常処理を行う
    public Action OnClose;
    protected Action OnWindowAnimationFinished = null;

    private System.Object args; // 引数
    public bool HasArgments { get { return this.args != null; } }
    public T GetArgments<T>() { return (T)this.args; }
    public Argments GetArgments() { return (Argments)this.args; }
    public ImageFader ImageFader;

    private Util.Waiter waiter = new Util.Waiter();
    private Shand.Easing timer = null;
    private GameObject windowInstance = null;
    private Window childWindow;
    public bool Ready = false;
    public bool inTutorial = false; // チュートリアル中のフラグ
    public Action OnStartHook = null; // OnStartの前に呼ばれる

    public virtual void SetArgment<T>(T arg) { this.args = arg as System.Object; }

    public Tsl.Scene.BaseScene BaseScene { get { return GetSceneBehaviour<Tsl.Scene.BaseScene>(); } }
    private Action onWindowClosed = null;
    private Action onWindowWillClose = null;

    // public virtual void SetButtonVisible(string btnName, bool visible) { } // 未使用

    // 継承クラスでAwakeを使用する場合はかならずWindowクラスのAwakeを呼びなおすこと
    private void Awake()
    {
        // 初期状態ではPanelは非表示にする
        this.GetPanel().gameObject.SetActive(false);
        this.ImageFader = this.GetComponent<ImageFader>();
    }

    #region Start/Update methods
    // and override methods likes NVI (Non virtual interface) idiom.
    // ウィンドウの初期化処理には、Start()メソッドの代わりに、OnStart()メソッドを使用すること。
    // Start()メソッドでは、ウィンドウの基本的な初期化処理を行っているため、
    // オーバーライドで上書きしてはならない。
    // これはNVIイディオムによるものである。
    // 更新処理も同様に、Update()メソッドの代わりにOnUpdate()メソッドを使用すること。
    protected virtual void OnStart() { }
    private void Start()
    {
        this.gameObject.GetType();

        if (this.OnStartHook != null) { this.OnStartHook(); }

        //if (this.WindowBackGround != null && BackgroundResource.Instance != null)
        //{
        //    if (this.WindowBackGroundType >= 0)
        //    {
        //        this.WindowBackGround.sprite = BackgroundResource.Instance.Background(this.WindowBackGroundType);
        //    }
        //}
#if false

        if (this.WindowBackGround != null)
        {
            // WindowBackgroundがセットされていたら背景にする
            var bg = this.GetPanel().Find("BackGround");

            if (bg != null)
            {
                bg.gameObject.SetActive(true);
                bg.GetComponent<UnityEngine.UI.Image>().sprite = this.WindowBackGround;
            }
        }

#endif
        OnStart();
        OnActivateWindow(true);
        // オープンアニメーションが終わったときの処理
        this.ImageFader.OnOpenAnimationFinished = () =>
        {
            if (!this.closed)
            {
                if (this.OnWindowAnimationFinished != null) { this.OnWindowAnimationFinished(); }

                StartCoroutine(this.TutorialStart(() => { OnStartAnimationFinished(); }));
            }
        };
        // オープンアニメーション開始
        this.ImageFader.StartOpenAnimation();
    }

    // Windowがアクティブになったときに呼ばれます
    public virtual void OnActivateWindow(bool active)
    {
        if (active)
        {
            //            if (this.EnableTopBar != BarAction.DoNotCare)
            //                this.BaseScene.GrandMenu.EnableTopBar(this.EnableTopBar == BarAction.Enabled);
            //            if (this.EnableBottomBar != BarAction.DoNotCare)
            //                this.BaseScene.GrandMenu.EnableBottomBar(this.EnableBottomBar == BarAction.Enabled);
            //            if (this.EnableMenuButton != BarAction.DoNotCare)
            //                this.BaseScene.GrandMenu.EnableMenuButton(this.EnableMenuButton == BarAction.Enabled);
            //            this.BaseScene.GrandMenu.TopBar.OnClickCloseButton = Close;
            ////if (MyApplication.Instance.PlayerInfo != null)
            ////this.BaseScene.GrandMenu.TopBar.SetTitleText(MyApplication.Instance.PlayerInfo.PlayerName);
            ////this.BaseScene.GrandMenu.TopBar.SetTitleText(GetWindowTitle());
            //this.BaseScene.GrandMenu.TopBar.OnClickMenuBotton = () =>
            //{
            //	CallWindow("1000MenuWindow");
            //};
            ResumeExclusiveButton();
        }
        else
        {
            //if (this.BaseScene.GrandMenu.TopBar.OnClickCloseButton == Close)
            //{
            //    this.BaseScene.GrandMenu.TopBar.OnClickCloseButton = null;
            //}
        }
    }

    // タイトルバーに表示するウインドウ名を返します。
    // 各クラスでオーバーライドしてください。
    // デフォルトはクラス名です。
    public virtual string GetWindowTitle()
    {
        return this.WindowTitleName;
    }

    protected virtual void OnUpdate() { }
    private void Update()
    {
        OnUpdate();
        this.waiter.Update();

        if (this.timer != null)
        {
            timer.UpdateValue(Time.deltaTime);
        }
    }
    protected virtual void OnStartAnimationFinished() { this.Ready = true; }

    #endregion

    #region Tutorial Methods

    protected Func<string, int> tutorialHook = null;

    // チュートリアルのリソースがあればチュートリアルを再生する
    public IEnumerator TutorialStart(Action onTutorialFinished, string subdir = null)
    {
        yield break;
        yield return null;
    }

    #endregion


    #region Event Handler
    // インスペクタで設定したOnClickイベント
    public virtual void OnClickButtonEvent(Transform button)
    {
        bool done = false;

        if (this.OnClickHook != null)
        {
            done = this.OnClickHook(button);
        }

        if (!done)
        {
            // オーバーライドなしでOnClickも未定儀の場合、buttonの名前で動作を決める
            var animebtn = button.GetComponent<AnimeButton>();

            if (animebtn != null)
            {
                if (!string.IsNullOrEmpty(animebtn.CallWindowName))
                {
                    CallWindow<Window, Argments>(animebtn.CallWindowName, new Argments(button.name));
                }
                else if (!string.IsNullOrEmpty(animebtn.ExecuteWindowName))
                {
                    ExecuteWindow<Window, Argments>(animebtn.ExecuteWindowName, new Argments(button.name));
                }
                else
                {
                    if (button.name == "ButtonClose")
                    {
                        this.Close();
                    }
                    else
                    {
                        CallWindow(button.name);
                    }
                }
            }
            else
            {
                switch (button.name)
                {
                    case "ButtonOK":
                    case "ButtonClose":
                        this.Close();
                        break;

                    default:
                        // その他のときは、Button名のWindowを呼び出す
                        CallWindow(button.name);
                        break;
                }
            }
        }
    }
    #endregion

    #region Window Control Methods
    // 自分を閉じてwindowNameを表示し、windowNameが閉じたら再び自身を表示する
    public Window CallWindow(string windowName, Action<Window> onClosed = null, bool noHideWindow = false)
    {
        return CallWindow<Window, Window.Argments>(windowName, null, onClosed, noHideWindow);
    }
    public Window CallWindow(string windowName, string arg, Action<Window> onClosed = null, bool noHideWindow = false)
    {
        return CallWindow<Window, Window.Argments>(windowName, new Argments(arg), onClosed, noHideWindow);
    }
    // 自分を閉じてwindowNameを表示し、windowNameが閉じたら再び自身を表示する(引数無しバージョン)
    public T CallWindow<T>(string windowName, Action<T> onClosed = null, bool noHideWindow = false) where T : Tsl.UI.Window
    {
        return CallWindow<T, Window.Argments>(windowName, null, onClosed, noHideWindow);
    }
    // 自分を閉じてwindowNameを表示し、windowNameが閉じたら再び自身を表示する(引数付きバージョン)
    public T CallWindow<T, A>(string windowName, A arg, Action<T> onClosed = null, bool noHideWindow = false)
    where T : Tsl.UI.Window
    {
        T w = this.BaseScene.AddModalWindow<T, A>(windowName, arg);
        this.childWindow = w;

        if (this.childWindow == null)
        {
            this.BaseScene.ShowErrorDialog(string.Format("{0}のWindowコンポーネントがありません", windowName));
        }
        else
        {
            OnActivateWindow(false);
            this.childWindow.OnWindowAnimationFinished = () =>
            {
                if (!noHideWindow && !this.closed) { this.Show(false); }
            };
            this.childWindow.onWindowWillClose = () =>
            {
                this.Show(true);
            };
            this.childWindow.onWindowClosed = () =>
            {
                this.childWindow = null;

                if (onClosed != null)
                {
                    onClosed(w);
                }
            };
        }

        return w;
    }

    // Windowを呼び出す。その後の処理は自前で行う
    public T ExecuteWindow<T>(string windowName) where T : Tsl.UI.Window
    {
        return ExecuteWindow<T, Argments>(windowName, null);
    }
    public T ExecuteWindow<T, A>(string windowName, A arg) where T : Tsl.UI.Window
    {
        T win = this.BaseScene.AddModalWindow<T, A>(windowName, arg);

        if (win == null)
        {
            this.BaseScene.ShowErrorDialog(string.Format("{0}のWindowコンポーネントがありません", windowName));
        }

        OnActivateWindow(false);
        win.onWindowClosed = this.onWindowClosed;

        if (this.onWindowWillClose != null) { this.onWindowWillClose(); }

        this.onWindowWillClose = null;
        win.OnWindowAnimationFinished = () =>
        {
            this.Close();
        };
        return win;
    }

    public void Show(bool show)
    {
        this.GetWindow().SetActive(show);
        this.OnActivateWindow(show);
    }

    bool closed = false;
    public void Close()
    {
        if (this.onWindowWillClose != null) { this.onWindowWillClose(); }

        this.onWindowWillClose = null;
        this.ImageFader.CloseAnimation(() =>
        {
            if (this.OnClose != null) { this.OnClose(); }   // ユーザー定義のフック

            this.OnClose = null;

            if (this.onWindowClosed != null) { this.onWindowClosed(); } // Windowクラス内部で使うフック

            this.onWindowClosed = null;

            if (this.closed)
            {
                Debug.LogError("Widnow is already closed. ");
            }
            else
            {
                this.closed = true;
                this.BaseScene.ClosedModalWindow();
                Destroy(this.gameObject);
            }
        });
    }

    #endregion


    #region Public methods
    public GameObject GetWindow()
    {
        if (this.closed)
        {
            return null;
        }

        return this.gameObject;
    }
    public Transform GetPanel(string panelname = "Panel")
    {
        return this.gameObject.transform.Find(panelname);
    }
    public T GetWindowComponent<T>()
    {
        return this.gameObject.transform.GetComponent<T>();
    }

    public T GetSceneBehaviour<T>()
    {
        return GameObject.Find("SceneBehaviour").GetComponent<T>();
    }

    public UnityEngine.UI.Button GetButton(string id)
    {
        return GetPanel().Find(id).GetComponent<UnityEngine.UI.Button>();
    }

    public void SetButtonContents(string id, Action<UnityEngine.UI.Button> onContent)
    {
        var button = GetButton(id);
        onContent(button);
    }

    public void SetButtonEvent(string id, Action onButton)
    {
        SetButtonContents(id, button =>
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onButton());
        });
    }

    public UnityEngine.UI.Text GetButtonText(string id)
    {
        return GetButton(id).transform.Find("Text").GetComponent<UnityEngine.UI.Text>();
    }

    public UnityEngine.UI.Text GetText(string id)
    {
        var child = GetPanel().Find(id);

        if (child != null) { return child.GetComponent<UnityEngine.UI.Text>(); }

        return null;
    }

    public void SetButtonImage(UnityEngine.UI.Button button, UnityEngine.Texture2D tex)
    {
        button.image.overrideSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
    }

    public void SetImage(UnityEngine.UI.Image image, UnityEngine.Texture2D tex)
    {
        if (tex != null)
        {
            image.overrideSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0, 0));
        }
    }


    public T GetWindowSetting<T>(string name)
    {
        var prefab = Resources.Load(name) as GameObject;
        return prefab.GetComponent<T>();
    }

    public void FadeAnimation(string windowName, Action start, Action fadeOut, Action fadeIn)
    {
        ScreenFadeManager.Instance.FadeOut(0.5f, Color.black, () =>
        {
            start();
            StartWait(windowName, () =>
            {
                fadeOut();
                StartTimer(() =>
                {
                    ScreenFadeManager.Instance.FadeIn(1.0f, Color.black, () =>
                    {
                        fadeIn();
                    });
                }, 30);
            });
        });
    }

    public void StartTimer(Action timeUpFunc, int durationFrame)
    {
        float waitTime = Util.TimeUtility.ConvertFrameToSec(durationFrame);
        this.timer = new Shand.Easing();
        this.timer.SetFinishEvent(() =>
        {
            this.timer = null;
            timeUpFunc();
        });
        this.timer.Start(Shand.EasingPattern.Linear, waitTime, 0.0f, 1.0f);
    }

    public void StartWait(string objectName, Action endFunc)
    {
        Func<bool> isReady = () =>
        {
            if (GameObject.Find(objectName) != null)
            {
                return true;
            }

            return false;
        };
        this.waiter.StartWait(isReady, endFunc);
    }

    #endregion

    #region Implementation details


    #endregion

    public static void SetText(Transform trans, string text)
    {
        if (trans != null)
        {
            var t = trans.GetComponent<UnityEngine.UI.Text>();

            if (t != null)
            {
                t.text = text;
            }
            else
            {
                var ut = trans.GetComponent<CustomUI.UIText>();

                if (ut != null)
                {
                    ut.Text = text;
                }
                else
                {
                    Debug.LogError("SetText: no Text component. text=" + text);
                }
            }
        }
        else
        {
            Debug.LogError("SetText: trans is null. text=" + text);
        }
    }

    // 親のWindowを探す
    public static Window SearchParentWindow(Transform parent)
    {
        while (parent != null)
        {
            var window = parent.GetComponent<Tsl.UI.Window>();

            if (window != null) { return window; }

            parent = parent.parent;
        }

        return null;
    }

    // ボタンがExclusiveModeにより非アクティブ化したときに呼ばれる
    private List<UnityEngine.UI.Button> exclusiveButtons = new List<UnityEngine.UI.Button>();
    public void SetExclusivdButton(UnityEngine.UI.Button btn)
    {
        if (btn.interactable)
        {
            btn.interactable = false;
            this.exclusiveButtons.Add(btn);
        }
    }
    // 非アクティブ化したボタンを戻す
    public void ResumeExclusiveButton()
    {
        foreach (var ab in this.exclusiveButtons)
        {
            if (ab.transform != null) { ab.interactable = true; }
        }

        this.exclusiveButtons.Clear();
    }
}
}

