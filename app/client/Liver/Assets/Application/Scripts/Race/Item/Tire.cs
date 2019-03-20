/// <summary>
/// ハンドル(6)：一定時間、角を曲がった後もスピードが落ちなくなる
/// </summary>
class Tire : ItemEffect
{
    public Tire(float endTime) : base(endTime)
    {
    }

    public override void Start()
    {
    }

    public override void End()
    {
        base.End();
    }
}