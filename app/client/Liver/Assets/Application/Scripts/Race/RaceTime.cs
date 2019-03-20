using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Liver
{

// ゲーム本編内での時間進行
// NOTE Time互換
public class RaceTime
{
    public static float realtimeSinceStartup = 0;


    // 初期化
    public static void Init()
    {
        realtimeSinceStartup = 0;
    }

    // NOTE 更新が不要な場合は呼ばない
    public static void Update()
    {
        realtimeSinceStartup += Time.deltaTime;
    }

}

}