using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shand
{
public class EasingVector2
{
    private Easing xposAnimation = new Easing();
    private Easing yposAnimation = new Easing();
    private Action<Vector2> onUpdate;

    public bool Ended { get { return yposAnimation.Ended; } }

    public void SetFinishEvent(Action f)
    {
        yposAnimation.SetFinishEvent(f);
    }

    public void SetOnUpdate(Action<Vector2> f)
    {
        this.onUpdate = f;
    }

    public void Start(EasingPattern pattern, float durationSec, Vector2 startValue, Vector2 endValue)
    {
        this.xposAnimation.Start(pattern, durationSec, startValue.x, endValue.x);
        this.yposAnimation.Start(pattern, durationSec, startValue.y, endValue.y);
    }

    public Vector2 UpdateValue(float deltaTime)
    {
        var pos = new Vector2(
            this.xposAnimation.UpdateValue(deltaTime),
            this.yposAnimation.UpdateValue(deltaTime)
        );
        return pos;
    }

    public void Update(float deltaTime)
    {
        var x = UpdateValue(deltaTime);

        if (this.onUpdate != null)
        {
            this.onUpdate(x);
        }
    }

    public void ForceEnd()
    {
        this.xposAnimation.ForceEnd();
        this.yposAnimation.ForceEnd();
    }
}

public class EasingVector3
{
    private Easing xposAnimation = new Easing();
    private Easing yposAnimation = new Easing();
    private Easing zposAnimation = new Easing();
    private Action<Vector3> onUpdate;

    public bool Ended { get { return zposAnimation.Ended; } }

    public void SetFinishEvent(Action f)
    {
        this.zposAnimation.SetFinishEvent(f);
    }

    public void SetOnUpdate(Action<Vector3> f)
    {
        this.onUpdate = f;
    }

    public void Start(EasingPattern pattern, float durationSec, Vector3 startValue, Vector3 endValue)
    {
        this.xposAnimation.Start(pattern, durationSec, startValue.x, endValue.x);
        this.yposAnimation.Start(pattern, durationSec, startValue.y, endValue.y);
        this.zposAnimation.Start(pattern, durationSec, startValue.z, endValue.z);
    }

    public Vector3 UpdateValue(float deltaTime)
    {
        var pos = new Vector3(
            this.xposAnimation.UpdateValue(deltaTime),
            this.yposAnimation.UpdateValue(deltaTime),
            this.zposAnimation.UpdateValue(deltaTime)
        );
        return pos;
    }

    public void Update(float deltaTime)
    {
        var x = UpdateValue(deltaTime);

        if (this.onUpdate != null)
        {
            this.onUpdate(x);
        }
    }

    public void ForceEnd()
    {
        this.xposAnimation.ForceEnd();
        this.yposAnimation.ForceEnd();
        this.zposAnimation.ForceEnd();
    }
}
}
