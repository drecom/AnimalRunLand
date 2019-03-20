using UnityEngine;
using System.Collections.Generic;
using Liver.Entity;

public class Animal
{
    public static readonly int JumpShort = 2;
    public static readonly int JumpStandard = 4;

    //float force = 1;
    float mass = 1f;        // パラメータを減らすため重量を１に固定してみる。必要になったら外部から設定できるようにします
    float maxJump = 0;
    AnimalSettings.AnimalSetting setting;

    public float Speed { get; private set; }
    public float MaxSpeed { get; private set; }
    public float Jump { get { return maxJump + AdditionJump; } }

    public float AdditionSpeed { get; set; }
    public float AdditionJump { get; set; }

    public float TurnAngle { get { return setting.turnAngle; } }

    public float Penalty { get { return setting.Penalty; } }
    public int POIGridDistance { get { return this.setting.POIGridDistance; } }
    public float StarDoublerRate { get { return this.setting.StarDoublerRate; } }

    public Vector3 ItemNormal { get { return this.setting.ItemNormal; } }
    public Vector3 ItemBoost { get { return this.setting.ItemBoost; } }

    /// <summary>
    ///
    /// </summary>
    /// <param name="force">推進力</param>
    /// <param name="mass">重量</param>
    /// <param name="maxSpeed">最高速度</param>
    /// <param name="jump">跳躍力</param>
    public Animal(float maxSpeed, float maxJump, AnimalSettings.AnimalSetting setting)
    {
        this.setting = setting;
        //this.mass = mass;
        this.MaxSpeed = maxSpeed;
        this.maxJump = maxJump;
    }

    public void Updata(float deltaTime)
    {
        Speed += ((setting.force / mass) * 3600 / 1000) * deltaTime;
        Speed = Mathf.Min(Speed, MaxSpeed + AdditionSpeed);
    }

    /// <summary>
    /// ダッシュ：いきなり最高スピードにします
    /// </summary>
    public void Dash()
    {
        Speed = MaxSpeed + AdditionSpeed;
    }

    /// <summary>
    /// 減速
    /// </summary>
    public void Deceleration()
    {
        Speed *= setting.deceleration;
    }

    /// <summary>
    /// 衝突した
    /// </summary>
    public void Break()
    {
    }
}


public static class AnimalFactory
{
    /// <summary>
    /// タイプしてしてアニマルカーを生成する
    /// </summary>
    public static Animal Create(AnimalKind type)
    {
        var setting = AnimalSettings.Instance[type];
        var jump = (setting.jump == AnimalSettings.Jump.Short) ? Animal.JumpShort : Animal.JumpStandard;
        return new Animal(setting.topSpeed, jump, setting);
    }
}
