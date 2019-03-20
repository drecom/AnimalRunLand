using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Tsl.UI;

// シーンの設計。
// 新たなシーンを作成する方法。
// 1. BaseSceneを継承したクラスを定義する。
// 2. シーンクラスの名前は、「シーン名 + SceneBehaviour」という命名規則にすること。
//    フェードインのイベントを得るために、MyApplication.ChangeScene()内で、
//    その命名規則を必要としている。
// 3. Unityエディタのデザイン側では、空のGameObjectを用意して、
//    「SceneBehaviour」という名前を付ける。
// 4. 作成したSceneBehaviour GameObjectに対して、シーンクラスのスクリプトを紐付ける。

// シーン起動の手順
// 1. OnStart() が呼ばれる。初期化などの処理をここで行う
// 2. 初期化が終わったら、IsReady()で trueを返す。(OnStart終了時に初期化が終わる場合はIsReadyをオーバーライドしなくてよい)
// 3. OnReady() が呼ばれる。シーンの開始処理を行う
// 4. SceneMain()　がCoroutineで呼ばれる。シーンのメイン処理を行う
namespace Tsl.Scene
{
public abstract class BaseScene : MonoBehaviour
{
    public string BgmName = "bgm001";
    public string InitialWindowName;
    public string InitialWindowArgment;
    public virtual void OnFadeIn(string previousSceneName) { }
    public virtual void SetButtonVisible(string btnName, bool visible) { }

    //private AudioSource seAudioSource;
    private Shand.Easing soundFadeAnimation;
    //private Entity.Option option;
    private Util.Waiter readyWait = null;
    private bool hasModalWindow = false;
    private CommonWindow.Dialog ErrorDialog;
    private ImageFader ImageFader;


    private void Awake()
    {
        Network.GameServer.OnFatalError = this.ShowErrorDialog;
        this.ImageFader = this.GetComponent<ImageFader>();
    }

    private IEnumerator Start()
    {
        // Androidの場合はシステムボタンを常に非表示にする
        if (Application.platform == RuntimePlatform.Android)
        {
            Screen.fullScreen = true;
        }

        // this.seAudioSource = gameObject.transform.Find("SeAudio").GetComponent<AudioSource>();

        if (!DoNotInitialize())
        {
            // initialize if first pass
            yield return Initialize();
        }

        OnStart();
        this.readyWait = new Util.Waiter();
        this.readyWait.StartWait(IsReady, () =>
        {
            this.readyWait = null;
            OnReady();
            StartCoroutine(SceneMain());
        });

        if (this.ImageFader != null)
        {
            this.ImageFader.StartOpenAnimation();
        }
    }

    private void Update()
    {
        if (!initializeCompleted)
        {
            return;
        }

        if (this.closedModalWindowCount > 0)
        {
            if (--this.closedModalWindowCount == 0)
            {
                this.hasModalWindow = false;
            }
        }

        if (this.readyWait != null)
        {
            this.readyWait.Update();
        }

        //if (MyApplication.Instance.IsStoppingInput) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ESCキー（Androidの戻るボタン)が押された
            //this.GrandMenu.CloseButtonAction();
        }

        // NetWorkアクセスアイコンの逝去
        //NetAcccessIcon.SetRetryCount(Api.Net.Requests.RetryCountRemain);
        //NetAcccessIcon.Set(0, Api.Net.Requests.InNetAccess > 0);
        //NetAcccessIcon.Set(1, Tsl.Asset.AssetLoader.InNetworkAccess);
        OnUpdate();
    }

    protected virtual void OnUpdate()
    {
    }


    protected virtual System.Collections.IEnumerator SceneMain()
    {
        yield break;
    }

    // シーンがReadyになったとき、初期Windowを表示する
    protected virtual void OnReady()
    {
        if (!string.IsNullOrEmpty(this.InitialWindowName))
        {
            AddModalWindow(this.InitialWindowName, new Window.Argments(this.InitialWindowArgment));
        }
    }

    // シーンが実行可能な状態になった場合にtrueを返す。
    public virtual bool IsReady()
    {
        if (this.ImageFader != null)
        {
            return this.ImageFader.IsOpenAnimationFinished;
        }

        return true;
    }

    public virtual string GetBgmName()
    {
        return this.BgmName;
    }

    public static BaseScene GetBaseScene()
    {
        var bs = GameObject.Find("SceneBehaviour");

        if (bs != null)
        {
            return bs.GetComponent<BaseScene>();
        }

        return null;
    }

    // 初期状態にする
    public static void Reset()
    {
        initializeCompleted = false;
    }


    private void CreateErrorDialog()
    {
        // エラーダイアログのロード
        var prefab = Resources.Load("UI/Dialog") as GameObject;
        var errorDialog = Instantiate(prefab);
        errorDialog.SetActive(false);
        this.ErrorDialog = errorDialog.GetComponent<CommonWindow.Dialog>();
    }

    private static bool initializeCompleted = false;
    protected IEnumerator Initialize()
    {
        if (initializeCompleted)
        {
            // 初期化済みでもプレイヤー情報は再取得する
            // yield return GameServer.Instance.RetrievePlayerInfoCoroutine();
            yield break;
        }

        if (Debug.isDebugBuild)
        {
            // デバッグログウインドウ起動
            DebugLogWindow.Load();
        }
        else
        {
            // Debug.Logの抑止
            Debug.unityLogger.logEnabled = false;
        }

        bool newUser = false;
        // 新規ユーザーが作成された場合のコールバック
        Network.GameServer.Instance.Requests.OnCreateAuth = (id) =>
        {
            newUser = true;
        };
        // boot処理
        yield return Network.GameServer.Instance.Boot(r =>
        {
            /// TODO : AssetBundleUrlを保存する
            //Tsl.Asset.AssetBundleManager.ServerUrl = r.AssetBundleUrl;
            Debug.Log("マスターデータのダウンロード完了: " + r.AssetBundleUrl);

            foreach (var x in Masterdata.Test.Test.All())
            {
                Debug.Log(string.Format("name:{0} relation:{1}", x.Name, x.Relation.Name));
            }

            //StartHello();
        });
        Network.GameServer.Instance.Requests.OnCreateAuth = null; // コールバックは解除しておく

        if (this.hasFatalError)
        {
            // 開発ビルドの場合、スルー
            if (!Debug.isDebugBuild)
            {
                yield return new WaitForSeconds(100000.0f);
            }
            else
            {
                Debug.LogError("致命的なエラーが発生しました");
            }
        }

        // 新規ユーザーの場合名前入力
        //yield return nameInput();
        /// TODO : ユーザ名の入力UIを作成する
        /// TODO : Liverに依存しているので別の場所に持っていく
        Liver.PlayerDataManager.Load();
        yield return Network.GameServer.Instance.SignUpCoroutine(Liver.PlayerDataManager.PlayerData, (r) =>
        {
            Debug.Log(string.Format("SignUp Accepted? ({0})", r.Accepted));
        });
        // プレイヤー情報の取得
        // 新規プレイヤーは、currentNationがUnKnownになるため、そこで判断する
#if flase
        yield return GameServer.Instance.RetrievePlayerInfoCoroutine();

        // プレイヤー名をセット
        if (MyApplication.Instance.PlayerInfo != null)
        {
            GrandMenu.TopBar.SetTitleText(MyApplication.Instance.PlayerInfo.PlayerName);

            if (MyApplication.Instance.PlayerInfo.CurrentCampaignType == 0 && !newUser)
            {
                // 国選択していない場合は、名前入力から
                yield return nameInput();
            }
        }

        // アセットバンドル初期化
        yield return Tsl.Asset.AssetBundleManager.DownloadCRC();
#endif
        initializeCompleted = true;
    }

    // サーバーに対する初期化を行わないフラグ
    protected virtual bool DoNotInitialize()
    {
        return false;
    }

    protected virtual void OnStart()
    {
    }


    public Transform GetCanvas()
    {
        return GameObject.Find("Canvas").transform;
    }

    public Transform GetPanel(string panelname = "Panel")
    {
        return GetCanvas().transform.Find(panelname);
    }

    public UnityEngine.UI.Text GetText(string id)
    {
        return GetPanel().Find(id).GetComponent<UnityEngine.UI.Text>();
    }

    public Transform GetLayeredTransform(string id)
    {
        Transform current = null;

        foreach (string name in id.Split('/'))
        {
            if (current == null)
            {
                GameObject result = GameObject.Find(name);

                if (result == null)
                {
                    Debug.Log(string.Format("GetLayeredTransform : {0}が見つかりません", id));
                    throw new InvalidProgramException();
                }

                current = result.transform;
            }
            else
            {
                Transform result = current.Find(name);

                if (result == null)
                {
                    Debug.Log(string.Format("GetLayeredTransform : {0}が見つかりません", id));
                    throw new InvalidProgramException();
                }

                current = result;
            }
        }

        return current;
    }

    // Button seButton = GetButton("Plane/Button");
    public UnityEngine.UI.Button GetButton(string id)
    {
        return GetLayeredTransform(id).GetComponent<UnityEngine.UI.Button>();
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

    //public AudioSource GetSeSource()
    //{
    //    return this.seAudioSource;
    //}

    public void ChangeScene(string sceneName, float duration)
    {
        ChangeSceneWithAction(sceneName, duration, () => { });
    }


    public void ChangeSceneWithAction(string sceneName, float duration, Action onCompleteFadeout)
    {
        //BgmManager.Instance.Stop(0.5f);
        ChangeScene(sceneName, duration, onCompleteFadeout);
    }

    // イメージを表示してシーンを遷移する
    public void ChangeSceneWithImageAction(string sceneName, Texture2D tex, Action<GameObject> configUi,
                                           Action waitStartF, Func<bool> isReadyF, Action<GameObject> waitEndF)
    {
        //BgmManager.Instance.Stop(0.5f);
        ChangeScene(
            sceneName, 1.0f, tex, configUi,
            waitStartF, isReadyF, waitEndF);
    }

    public ComponentT AddModalWindow<ComponentT>(string prefabname) where ComponentT : Tsl.UI.Window
    {
        return AddModalWindow<ComponentT, Window.Argments>(prefabname, null);
    }

    public ComponentT AddModalWindow<ComponentT, A>(string prefabname, A arg) where ComponentT : Tsl.UI.Window
    {
        this.hasModalWindow = true;

        if (this.closedModalWindowCount > 0) { this.closedModalWindowCount = 0; }

        var prefab = Resources.Load("Window/" + prefabname) as GameObject;

        if (prefab == null)
        {
            ShowErrorDialog(prefabname + " not found");
            return null;
        }

        var windowInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
        var window = windowInstance.GetComponent<ComponentT>();

        if (arg != null) { window.SetArgment(arg); }

        windowInstance.SetActive(true);
        return window;
    }
    public Tsl.UI.Window AddModalWindow(string prefabname, Window.Argments arg = null)
    {
        return AddModalWindow<Tsl.UI.Window, Window.Argments>(prefabname, arg);
    }

    int closedModalWindowCount = 0;
    public void ClosedModalWindow()
    {
        closedModalWindowCount = 2; // 2フレーム後にhasModalWindowをfalseにする
        //        this.hasModalWindow = false;
    }

    public void SetModalWindow()
    {
        this.hasModalWindow = true;
    }

    public bool HasModalWindow()
    {
        return this.hasModalWindow;
    }

    public bool hasFatalError = false;
    public void ShowErrorDialog(string msg, int errorcode = 0)
    {
        this.hasFatalError = true;

        if (this.ErrorDialog != null) { return; }

        // TODO: エラーダイアログができるまでの暫定措置
        Debug.LogError(msg);
        //            CreateErrorDialog();
        // フェーダーをOFFにする
        //ScreenFadeManager.Instance.ForceFadeIn();
        //this.ErrorDialog.TitleText.text = "エラーが発生しました";
        //this.ErrorDialog.MessageText.text = msg;
        //this.ErrorDialog.MessageText.alignment = TextAnchor.MiddleCenter;
        //this.ErrorDialog.gameObject.SetActive(true);
        //NetAcccessIcon.Off();
        //this.ErrorDialog.OnClick = (act) =>
        //{
        //    NetAcccessIcon.Off();
        //    HideErrorDialog();
        //    ChangeScene("FatalErrorScene", 1.0f);
        //};
    }
    public void HideErrorDialog()
    {
        if (this.ErrorDialog != null)
        {
            this.ErrorDialog.Close();
            this.ErrorDialog = null;
        }
    }

    public static void SetText(Transform trans, string text)
    {
        Tsl.UI.Window.SetText(trans, text);
    }


    // 子供を殺す TODO: この手の機能はどこかに集約する
    public static void DestroyAllChildlen(Transform parent)
    {
        while (parent.childCount > 0)
        {
            var child = parent.GetChild(0);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    public void DelayedExecution(System.Action func, System.Func<bool> pred)
    {
        StartCoroutine(delayedExecution(func, pred));
    }

    public void DelayedExecution(System.Action func, float duration)
    {
        StartCoroutine(delayedExecution(func, () =>
        {
            duration -= Time.deltaTime;
            return duration < 0;
        }));
    }

    IEnumerator delayedExecution(System.Action func, System.Func<bool> pred)
    {
        while (!pred()) { yield return null; }

        func();
    }

    public static string GetCurrentSceneName()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    private BaseScene GetSceneObject(string sceneName)
    {
        Type scriptType = Type.GetType(sceneName + "Behaviour");

        if (scriptType == null)
        {
            return null;
        }

        var nextScene = GameObject.FindObjectOfType(scriptType) as BaseScene;
        return nextScene;
    }

    public void ChangeScene(string sceneName, float duration, Action onCompleteFadeout)
    {
        this.prevSceneName = string.Copy(GetCurrentSceneName());
        IsStoppingInput = true;
        float halfDuration = duration / 2.0f;
        ScreenFadeManager.Instance.FadeOut(halfDuration, Color.black, () =>
        {
            onCompleteFadeout();
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            Func<bool> isReady = () =>
            {
                var nextScene = GetSceneObject(sceneName);

                if (nextScene != null)
                {
                    return nextScene.IsReady();
                }

                return true;
            };
            ScreenFadeManager.Instance.StartWait(isReady, () =>
            {
                ScreenFadeManager.Instance.FadeIn(halfDuration, Color.black, () =>
                {
                    var nextScene = GetSceneObject(sceneName);

                    if (nextScene != null)
                    {
                        IsStoppingInput = false;
                        nextScene.OnFadeIn(this.prevSceneName);
                    }
                });
            });
        });
    }

    string prevSceneName;
    static bool IsStoppingInput = false;
    public void ChangeScene(string sceneName,
                            Action waitStartF, Func<bool> isReady, Action onEnd)
    {
        this.prevSceneName = string.Copy(GetCurrentSceneName());
        IsStoppingInput = true;
        float halfDuration = 0.5f;
        ScreenFadeManager.Instance.FadeOut(halfDuration, Color.black, () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            ScreenFadeManager.Instance.StartWait(isReady, () =>
            {
                ScreenFadeManager.Instance.FadeIn(halfDuration, Color.black, () =>
                {
                    onEnd();
                    Type scriptType = Type.GetType(sceneName + "SceneBehaviour");

                    if (scriptType == null)
                    {
                        return;
                    }

                    var nextScene = GameObject.FindObjectOfType(scriptType) as BaseScene;

                    if (nextScene != null)
                    {
                        IsStoppingInput = false;
                        nextScene.OnFadeIn(this.prevSceneName);
                    }
                });
            });
        });
    }

    public void ChangeScene(string sceneName, float duration,
                            Texture2D tex,
                            Action<GameObject> configUi,
                            Action waitStartF, Func<bool> isReadyF, Action<GameObject> waitEndF)
    {
        this.prevSceneName = string.Copy(GetCurrentSceneName());
        IsStoppingInput = true;
        float halfDuration = duration / 2.0f;
        ImageCrossFadeManager.Instance.StartFadeOut(tex, configUi, halfDuration, () =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            ImageCrossFadeManager.Instance.StartWait(waitStartF, isReadyF, () =>
            {
                waitStartF();
                ImageCrossFadeManager.Instance.StartFadeIn(halfDuration, (fader) =>
                {
                    waitEndF(fader);
                    Type scriptType = Type.GetType(sceneName + "SceneBehaviour");

                    if (scriptType == null)
                    {
                        return;
                    }

                    var nextScene = GameObject.FindObjectOfType(scriptType) as BaseScene;

                    if (nextScene != null)
                    {
                        IsStoppingInput = false;
                        nextScene.OnFadeIn(this.prevSceneName);
                    }
                });
            });
        });
    }


}


}