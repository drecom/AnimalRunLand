using System;

namespace Util
{
public class Waiter
{
    private Func<bool> pred;
    private Action onEnd;

    public void StartWait(Func<bool> pred, Action newOnEnd)
    {
        this.pred = pred;

        if (this.onEnd != null)
        {
            var oldOnEnd = this.onEnd;
            this.onEnd = () => { oldOnEnd(); newOnEnd(); };
        }
        else
        {
            this.onEnd = newOnEnd;
        }
    }
    public bool Empty()
    {
        return this.pred == null;
    }

    public void Update()
    {
        if (this.pred == null)
        {
            return;
        }

        if (this.pred())
        {
            this.pred = null;
            Action finishF = this.onEnd;
            this.onEnd = null;
            finishF();
        }
    }
}
}
