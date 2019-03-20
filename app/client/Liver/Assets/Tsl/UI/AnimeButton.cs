using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tsl.UI
{
public class AnimeButton : MonoBehaviour
{
    public enum StartupMotion
    {
        None,
        ZoomIn,
        FadeIn,
        WipeFromLeft,
        WipeFromRight,
        Hide,
        ZoomOut,
    }
    public bool ExclusiveMode = false; // ボタンがタップされたとき、同一階層の他のAnimeButtonを無効化する
    public StartupMotion FirstMotion = AnimeButton.StartupMotion.ZoomIn;
    public string ExecuteWindowName;
    public string CallWindowName;
    public System.Action OnClick = null;
    public System.Action<bool> OnPressed = null;

    public Button Button { get { return GetComponent<Button>(); } }

    public void Awake()
    {
        var anime = this.transform.GetComponent<Animator>();

        if (this.FirstMotion != StartupMotion.None)
        {
            anime.Play(this.FirstMotion.ToString());
        }
    }

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Play(StartupMotion anim)
    {
        var anime = this.transform.GetComponent<Animator>();
        anime.StopPlayback();
        anime.Play(anim.ToString());
    }

    // Animatorのイベントから呼ばれる
    void Selected()
    {
        //Debug.Log("Selected");
        if (this.OnClick != null)
        {
            this.OnClick();
        }
        else
        {
            // Windowクラスを探す。
            var window = Tsl.UI.Window.SearchParentWindow(this.transform.parent);

            if (window != null)
            {
                window.OnClickButtonEvent(this.transform);
            }
            else
            {
                Debug.LogError("AnimeButton:イベントハンドらがありません");
            }
        }
    }

    // ボタンが押された
    public void PointerDown()
    {
        if (GetComponent<UnityEngine.UI.Button>().interactable == false) { return; }

        //Debug.Log("PointerDown");
        var basescene = Tsl.Scene.BaseScene.GetBaseScene();
        SeManager.Instance.Play(Liver.Sound.SeKind.botton_main.ToString(), 0.0f, false, 1.0f);
        //basescene.PlaySe(BaseScene.SoundKind.Ok);
        // ボタンが押された時は、メニューボタンを押せなくする
        //basescene.GrandMenu.EnableMenuButton(false);

        // 排他モードのときボタンがタップされたら同一階層の他のAnimeButtonを無効化する
        if (this.ExclusiveMode)
        {
            var window = Tsl.UI.Window.SearchParentWindow(this.transform.parent);

            for (int c = 0; c < this.transform.parent.childCount; ++c)
            {
                var animeButton = this.transform.parent.GetChild(c).GetComponent<AnimeButton>();

                if (animeButton != null && animeButton != this && animeButton.Button.IsInteractable())
                {
                    window.SetExclusivdButton(animeButton.Button);
                }
            }
        }

        if (this.OnPressed != null) { this.OnPressed(true); }
    }
    // ボタンが離された
    public void PointerUp()
    {
        //Debug.Log("PointerUp");
        if (this.OnPressed != null) { this.OnPressed(false); }
    }
}

}
