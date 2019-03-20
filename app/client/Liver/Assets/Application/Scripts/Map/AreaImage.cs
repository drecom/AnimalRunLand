using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class AreaImage : MonoBehaviour
{
    // エリアを真上から捉えるカメラ
    public Camera AreaTopCamera;
    // レンダリングしたエリアを再レンダリングして加工するカメラ
    public Camera RenderImageCamera;

    // レンダリング用マテリアル
    public Material Material;

    public RenderTexture RenderArea { get; private set; }
    public RenderTexture RenderImage { get; private set; }

    public int ImageSize = 3000;


    void Start()
    {
        // NOTE 生成オプション
        RenderTextureDescriptor desc = new RenderTextureDescriptor(this.ImageSize, this.ImageSize, RenderTextureFormat.ARGB32);
        desc.bindMS            = false;
        desc.autoGenerateMips  = false;
        desc.depthBufferBits   = 0;
        desc.enableRandomWrite = false;
        desc.useMipMap         = false;
        desc.sRGB              = false;
        // 真上から見たエリア全体像をレンダリング
        this.RenderArea = RenderTexture.GetTemporary(desc);
        this.AreaTopCamera.targetTexture = this.RenderArea;
        this.Material.SetTexture("_MainTex", this.RenderArea);
        // レンダリングしたエリアを再レンダリングして加工
        this.RenderImage = RenderTexture.GetTemporary(desc);
        this.RenderImageCamera.targetTexture = this.RenderImage;
    }


    void OnDestroy()
    {
        Debug.Log("Destroy RenderTexture");

        // NOTE 明示的に破棄
        if (this.RenderArea) { RenderTexture.ReleaseTemporary(this.RenderArea); }

        if (this.RenderImage) { RenderTexture.ReleaseTemporary(this.RenderImage); }
    }

}
