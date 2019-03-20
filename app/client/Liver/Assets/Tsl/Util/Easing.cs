// Easing functions for animation
// http://easings.net/ja
//
// Copyright Akira Takahashi 2015.
//
// Copyright Adrian Seeley 2012.
// https://gist.github.com/adrianseeley/4242677
//
// Copyright Robert Penner 2001.
// https://github.com/jesusgollonet/ofpennereasing
//
// Use, modification and distribution is subject to the BSD License.
// http://opensource.org/licenses/bsd-license.php

using System;
using System.Collections.Generic;

namespace Shand
{
// t = currentStep
// b = startValue
// c = changeValue
// d = totalSteps
public class RawEasing
{
    public static float Linear(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * (currentStep / totalSteps) + startValue;
    }
    public static float SineIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return -changeValue * (float)Math.Cos(currentStep / totalSteps * (Math.PI / 2)) + changeValue + startValue;
    }
    public static float SineOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * (float)Math.Sin(currentStep / totalSteps * (Math.PI / 2)) + startValue;
    }
    public static float SineInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return -changeValue / 2 * ((float)Math.Cos(Math.PI * currentStep / totalSteps) - 1) + startValue;
    }
    public static float QuadraticIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * (currentStep /= totalSteps) * currentStep + startValue;
    }
    public static float QuadraticOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return -changeValue * (currentStep /= totalSteps) * (currentStep - 2) + startValue;
    }
    public static float QuadraticInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if ((currentStep /= totalSteps / 2) < 1)
        {
            return changeValue / 2 * currentStep * currentStep + startValue;
        }
        else
        {
            return -changeValue / 2 * ((--currentStep) * (currentStep - 2) - 1) + startValue;
        }
    }
    public static float CubicIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * (currentStep /= totalSteps) * currentStep * currentStep + startValue;
    }
    public static float CubicOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * ((currentStep = currentStep / totalSteps - 1) * currentStep * currentStep + 1) + startValue;
    }
    public static float CubicInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if ((currentStep /= totalSteps / 2) < 1)
        {
            return changeValue / 2 * currentStep * currentStep * currentStep + startValue;
        }
        else
        {
            return changeValue / 2 * ((currentStep -= 2) * currentStep * currentStep + 2) + startValue;
        }
    }
    public static float QuarticIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * (currentStep /= totalSteps) * currentStep * currentStep * currentStep + startValue;
    }
    public static float QuarticOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return -changeValue * ((currentStep = currentStep / totalSteps - 1) * currentStep * currentStep * currentStep - 1) + startValue;
    }
    public static float QuarticInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if ((currentStep /= totalSteps / 2) < 1)
        {
            return changeValue / 2 * currentStep * currentStep * currentStep * currentStep + startValue;
        }
        else
        {
            return -changeValue / 2 * ((currentStep -= 2) * currentStep * currentStep * currentStep - 2) + startValue;
        }
    }
    public static float QuinticIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * (currentStep /= totalSteps) * currentStep * currentStep * currentStep * currentStep + startValue;
    }
    public static float QuinticOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * ((currentStep = currentStep / totalSteps - 1) * currentStep * currentStep * currentStep * currentStep + 1) + startValue;
    }
    public static float QuinticInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if ((currentStep /= totalSteps / 2) < 1)
        {
            return changeValue / 2 * currentStep * currentStep * currentStep * currentStep * currentStep + startValue;
        }
        else
        {
            return changeValue / 2 * ((currentStep -= 2) * currentStep * currentStep * currentStep * currentStep + 2) + startValue;
        }
    }
    public static float ExponentialIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if (currentStep == 0)
        {
            return startValue;
        }
        else
        {
            return changeValue * (float)Math.Pow(2, 10 * (currentStep / totalSteps - 1)) + startValue;
        }
    }
    public static float ExponentialOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if (currentStep == totalSteps)
        {
            return startValue + changeValue;
        }
        else
        {
            return changeValue * (-(float)Math.Pow(2, -10 * currentStep / totalSteps) + 1) + startValue;
        }
    }
    public static float ExponentialInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if (currentStep == 0)
        {
            return startValue;
        }
        else if (currentStep == totalSteps)
        {
            return startValue + changeValue;
        }
        else if ((currentStep /= totalSteps / 2) < 1)
        {
            return changeValue / 2 * (float)Math.Pow(2, 10 * (currentStep - 1)) + startValue;
        }
        else
        {
            return changeValue / 2 * ((float) - Math.Pow(2, -10 * --currentStep) + 2) + startValue;
        }
    }
    public static float CircularIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return -changeValue * ((float)Math.Sqrt(1 - (currentStep /= totalSteps) * currentStep) - 1) + startValue;
    }
    public static float CircularOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue * (float)Math.Sqrt(1 - (currentStep = currentStep / totalSteps - 1) * currentStep) + startValue;
    }
    public static float CircularInOut(float currentStep, float startValue, float changeValue, float totalStep)
    {
        if ((currentStep /= totalStep / 2) < 1)
        {
            return -changeValue / 2 * ((float)Math.Sqrt(1 - (float)Math.Pow(currentStep, 2)) - 1) + startValue;
        }
        else
        {
            return changeValue / 2 * ((float)Math.Sqrt(1 - currentStep * (currentStep -= 2)) + 1) + startValue;
        }
    }

    public static float BackIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        const float s = 1.70158f;
        return changeValue * ((currentStep = currentStep / totalSteps - 1) * currentStep * ((s + 1) * currentStep + s)) + startValue;
    }
    public static float BackOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        const float s = 1.70158f;
        return changeValue * ((currentStep = currentStep / totalSteps - 1) * currentStep * ((s + 1) * currentStep + s) + 1) + startValue;
    }

    public static float BackInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        float s = 1.70158f;

        if ((currentStep /= totalSteps / 2) < 1)
        {
            return changeValue / 2 * (currentStep * currentStep * (((s *= (1.525f)) + 1) * currentStep - s)) + startValue;
        }

        float postFix = currentStep -= 2;
        return changeValue / 2 * ((postFix) * currentStep * (((s *= (1.525f)) + 1) * currentStep + s) + 2) + startValue;
    }
    public static float ElasticIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if (currentStep == 0)
        {
            return startValue;
        }

        if ((currentStep /= totalSteps) == 1) return
                startValue + changeValue;

        float p = totalSteps * 0.3f;
        float a = changeValue;
        float s = p / 4;
        float postFix = a * (float)Math.Pow(2, 10 * (currentStep -= 1)); // this is a fix, again, with post-increment operators
        return -(postFix * (float)Math.Sin((currentStep * totalSteps - s) * (2 * Math.PI) / p)) + startValue;
    }
    public static float ElasticOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if (currentStep == 0)
        {
            return startValue;
        }

        if ((currentStep /= totalSteps) == 1)
        {
            return startValue + changeValue;
        }

        float p = totalSteps * 0.3f;
        float a = changeValue;
        float s = p / 4;
        return (a * (float)Math.Pow(2, -10 * currentStep) * (float)Math.Sin((currentStep * totalSteps - s) * (2 * Math.PI) / p) + changeValue + startValue);
    }

    public static float ElasticInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if (currentStep == 0)
        {
            return startValue;
        }

        if ((currentStep /= totalSteps / 2) == 2)
        {
            return startValue + changeValue;
        }

        float p = totalSteps * (0.3f * 1.5f);
        float a = changeValue;
        float s = p / 4;

        if (currentStep < 1)
        {
            float postFix = a * (float)Math.Pow(2, 10 * (currentStep -= 1)); // postIncrement is evil
            return -0.5f * (postFix * (float)Math.Sin((currentStep * totalSteps - s) * (2 * Math.PI) / p)) + startValue;
        }
        else
        {
            float postFix = a * (float)Math.Pow(2, -10 * (currentStep -= 1)); // postIncrement is evil
            return postFix * (float)Math.Sin((currentStep * totalSteps - s) * (2 * Math.PI) / p) * 0.5f + changeValue + startValue;
        }
    }
    public static float BounceIn(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        return changeValue - BounceOut(totalSteps - currentStep, totalSteps, 0, changeValue) + startValue;
    }
    public static float BounceOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if ((currentStep /= totalSteps) < (1 / 2.75f))
        {
            return changeValue * (7.5625f * currentStep * currentStep) + startValue;
        }
        else if (currentStep < (2 / 2.75f))
        {
            float postFix = currentStep -= (1.5f / 2.75f);
            return changeValue * (7.5625f * (postFix) * currentStep + 0.75f) + startValue;
        }
        else if (currentStep < (2.5 / 2.75))
        {
            float postFix = currentStep -= (2.25f / 2.75f);
            return changeValue * (7.5625f * (postFix) * currentStep + 0.9375f) + startValue;
        }
        else
        {
            float postFix = currentStep -= (2.625f / 2.75f);
            return changeValue * (7.5625f * (postFix) * currentStep + 0.984375f) + startValue;
        }
    }
    public static float BounceInOut(float currentStep, float totalSteps, float startValue, float changeValue)
    {
        if (currentStep < totalSteps / 2)
        {
            return BounceIn(currentStep * 2, 0, changeValue, totalSteps) * 0.5f + startValue;
        }
        else
        {
            return BounceOut(currentStep * 2 - totalSteps, 0, changeValue, totalSteps) * 0.5f + changeValue * 0.5f + startValue;
        }
    }

    public static float BezierFunc(float x, float x1, float y1, float x2, float y2, float epsilon = 0.01f)
    {
        var down = 0.0f;
        var up = 1.0f;
        var lower = x - epsilon;
        var upper = x + epsilon;

        if (x < 0.0f + epsilon)
        {
            return 0;
        }

        if (x > 1.0f - epsilon)
        {
            return 1.0f;
        }

        // 二分探索でtを求める
        while (true)
        {
            var t = (down + up) / 2;
            var px = 3 * (1 - t) * (1 - t) * t * x1 + 3 * (1 - t) * t * t * x2 + t * t * t;

            if (px < lower) { down = t; }
            else if (px > upper) { up = t; }
            else
                // 求めたtからyを求める
            {
                return 3 * (1 - t) * (1 - t) * t * y1 + 3 * (1 - t) * t * t * y2 + t * t * t;
            }
        }
    }

    public static float Bezier(float currentStep, float totalSteps, float startValue, float changeValue, float x1, float y1, float x2, float y2)
    {
        return changeValue * BezierFunc(currentStep / totalSteps, x1, y1, x2, y2) + startValue;
    }
}

public enum EasingPattern
{
    Linear,
    SineIn,			SineOut,		SineInOut,
    QuadraticIn,	QuadraticOut,	QuadraticInOut,
    CubicIn,		CubicOut,		CubicInOut,
    QuarticIn,		QuarticOut,		QuarticInOut,
    QuinticIn,		QuinticOut,		QuinticInOut,
    ExponentialIn,	ExponentialOut,	ExponentialInOut,
    CircularIn,		CircularOut,	CircularInOut,
    BackIn,			BackOut,		BackInOut,
    ElasticIn,		ElasticOut,		ElasticInOut,
    BounceIn,		BounceOut,		BounceInOut,
    Bezier,
}

public class Easing
{
    private Func<float, float, float, float, float> easingFunc;
    private float prevCurrentSec;
    private float currentSec = 0.0f;
    private float durationSec;
    private float startValue;
    private float endValue;
    private bool isPositive = true;
    private Action onFinish;
    private Action<float> onUpdate;

    public void SetFinishEvent(Action f)
    {
        this.onFinish = f;
    }

    public void SetOnUpdate(Action<float> f)
    {
        this.onUpdate = f;
    }

    public float StartValue { get { return startValue; } }
    public float EndValue { get { return endValue; } }
    public bool Ended { get { return this.prevCurrentSec >= this.durationSec; } }

    public void Start(EasingPattern pattern, float durationSec, float startValue, float endValue)
    {
        if (durationSec < 0.0f)
        {
            throw new InvalidProgramException("`durationSec` parameter must be positive value");
        }

        if (durationSec == 0.0f)
        {
            durationSec = 1.0f;
        }

        var funcMap = new Dictionary<EasingPattern, Func<float, float, float, float, float>>
        {
            {EasingPattern.Linear,				RawEasing.Linear},
            {EasingPattern.SineIn,				RawEasing.SineIn},
            {EasingPattern.SineOut,				RawEasing.SineOut},
            {EasingPattern.SineInOut,			RawEasing.SineInOut},
            {EasingPattern.QuadraticIn,			RawEasing.QuadraticIn},
            {EasingPattern.QuadraticOut,		RawEasing.QuadraticOut},
            {EasingPattern.QuadraticInOut,		RawEasing.QuadraticInOut},
            {EasingPattern.CubicIn,				RawEasing.CubicIn},
            {EasingPattern.CubicOut,			RawEasing.CubicOut},
            {EasingPattern.CubicInOut,			RawEasing.CubicInOut},
            {EasingPattern.QuarticIn,			RawEasing.QuarticIn},
            {EasingPattern.QuarticOut,			RawEasing.QuarticOut},
            {EasingPattern.QuarticInOut,		RawEasing.QuarticInOut},
            {EasingPattern.QuinticIn,			RawEasing.QuinticIn},
            {EasingPattern.QuinticOut,			RawEasing.QuinticOut},
            {EasingPattern.QuinticInOut,		RawEasing.QuinticInOut},
            {EasingPattern.ExponentialIn,		RawEasing.ExponentialIn},
            {EasingPattern.ExponentialOut,		RawEasing.ExponentialOut},
            {EasingPattern.ExponentialInOut,	RawEasing.ExponentialInOut},
            {EasingPattern.CircularIn,			RawEasing.CircularIn},
            {EasingPattern.CircularOut,			RawEasing.CircularOut},
            {EasingPattern.CircularInOut,		RawEasing.CircularInOut},
            {EasingPattern.BackIn,				RawEasing.BackIn},
            {EasingPattern.BackOut,				RawEasing.BackOut},
            {EasingPattern.BackInOut,			RawEasing.BackInOut},
            {EasingPattern.ElasticIn,			RawEasing.ElasticIn},
            {EasingPattern.ElasticOut,			RawEasing.ElasticOut},
            {EasingPattern.ElasticInOut,		RawEasing.ElasticInOut},
            {EasingPattern.BounceIn,			RawEasing.BounceIn},
            {EasingPattern.BounceOut,			RawEasing.BounceOut},
            {EasingPattern.BounceInOut,			RawEasing.BounceInOut}
        };
        this.easingFunc = funcMap[pattern];
        this.prevCurrentSec = 0.0f;
        this.currentSec = 0.0f;
        this.durationSec = durationSec;
        this.startValue = startValue;
        this.endValue = endValue;
        this.isPositive = startValue < endValue;
    }
    public void StartBezier(float durationSec, float startValue, float endValue, float x1, float y1, float x2, float y2)
    {
        this.easingFunc = (a, b, c, d) => RawEasing.Bezier(a, b, c, d, x1, y1, x2, y2);
        this.prevCurrentSec = 0.0f;
        this.currentSec = 0.0f;
        this.durationSec = durationSec;
        this.startValue = startValue;
        this.endValue = endValue;
        this.isPositive = startValue < endValue;
    }

    public void Update(float deltaTime)
    {
        UpdateValue(deltaTime);
    }

    public float UpdateValue(float deltaTime)
    {
        if (this.easingFunc == null)
        {
            throw new InvalidOperationException("this method should call after `Start()`");
        }

        float distance = this.isPositive ?
                         this.endValue - this.startValue :
                         this.startValue - this.endValue;
        float result = this.startValue;

        if (this.durationSec == 0.0f)
        {
            result = this.endValue;
        }
        else
        {
            float currentValue = this.easingFunc(this.currentSec, this.durationSec, 0.0f, distance);
            this.prevCurrentSec = this.currentSec;
            this.currentSec = Math.Min(this.currentSec + deltaTime, this.durationSec);
            result = this.isPositive ? (this.startValue + currentValue) : (this.startValue - currentValue);
        }

        if (this.onUpdate != null)
        {
            this.onUpdate(result);
        }

        if (this.prevCurrentSec >= this.durationSec)
        {
            if (this.onFinish != null)
            {
                Action f = this.onFinish;
                this.onFinish = null;
                f();
            }
        }

        return result;
    }

    public void ForceEnd()
    {
        this.currentSec = this.durationSec;
    }
}
}
