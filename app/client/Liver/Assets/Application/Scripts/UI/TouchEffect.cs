using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tsl.Scene;
using Liver;

public class TouchEffect : MonoBehaviour
{
    static string ef = "Effects/ef_011";
    static GameObject Instance;

    EffectWindow effectWindow;

    [RuntimeInitializeOnLoadMethod]
    public static void Init()
    {
        if (Instance == null)
        {
            Instance = new GameObject("TouchEffect");
            Instance.AddComponent<TouchEffect>();
            GameObject.DontDestroyOnLoad(Instance);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //return;
        if (effectWindow == null) { effectWindow = BaseScene.GetBaseScene().AddModalWindow<EffectWindow>("EffectWindow"); }

        if (/*Input.GetMouseButtonUp(0) || */Input.GetMouseButtonDown(0))
        {
            // マウスのワールド座標までパーティクルを移動し、パーティクルエフェクトを1つ生成する
            var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Camera.main.transform.forward * 50);
            ParticleGen.Instance.PlayOnce(ef, effectWindow.monitor.MonitorRoot.transform, pos);
        }
    }
}