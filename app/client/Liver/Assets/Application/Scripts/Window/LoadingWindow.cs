using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Tsl.UI;

namespace Liver
{
public class LoadingWindow : Window
{
    public Monitor monitor;

    [SerializeField]
    GameObject effect;

    [SerializeField]
    Image LoadingImage;

    public string[] ImagePath;
    int index = 0;

    // テスト用
    // TODO リリース時には消す
    public Text Progress;


    void Awake()
    {
        var ef = Instantiate(effect);
        ef.transform.SetParent(monitor.MonitorRoot.transform, false);
        BgmManager.Instance.Play(Sound.BgmKind.Loading, 0.0f, 0.0f, true, 1.0f);
        LoadImage();

        if (Debug.isDebugBuild)
        {
            // デバッグ時のみ有効
            this.Progress.gameObject.SetActive(true);
        }
    }

    public void OnClick()
    {
        //image.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
        LoadImage();
    }


    void LoadImage()
    {
        // NOTE 同じのが続かないようにランダム
        int i = 0;

        do
        {
            i = UnityEngine.Random.Range(0, this.ImagePath.Length);
        }
        while (this.index == i);

        this.index = i;
        var path = $"Textures/Loading/{this.ImagePath[this.index]}";
        this.LoadingImage.sprite = Resources.Load<Sprite>(path);
    }
}
}
