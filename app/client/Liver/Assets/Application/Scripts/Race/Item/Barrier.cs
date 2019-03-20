/// <summary>
/// バリア(2)：一定時間、障害物を破壊してくれる
/// </summary>
class Barrier : ItemEffect
{
    public Barrier(float endTime) : base(endTime)
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