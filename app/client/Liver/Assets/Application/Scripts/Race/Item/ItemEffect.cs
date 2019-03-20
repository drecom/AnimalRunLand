using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// アイテム効果の基底クラス
/// </summary>
abstract class ItemEffect
{
    float endTime = 0;
    float startTime = 0;
    public Image gauge;
    public event Action OnEnd = () => { };

    public ItemEffect(float endTime)
    {
        startTime = Liver.RaceTime.realtimeSinceStartup;
        this.endTime = endTime;
    }

    public abstract void Start();
    public virtual void End()
    {
        OnEnd();
    }

    /// <summary>
    /// 更新
    /// </summary>
    /// <returns>効果有効(true) 無効(false)</returns>
    public bool Update()
    {
        if (gauge)
        {
            var remain = (endTime - Liver.RaceTime.realtimeSinceStartup) / (endTime - startTime);
            gauge.fillAmount = remain;
        }

        if (endTime <= Liver.RaceTime.realtimeSinceStartup)
        {
            this.End();
            return false;
        }

        return true;
    }
}
