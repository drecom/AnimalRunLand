/// <summary>
/// アイテム倍化(4)：一定時間、通常スターが2~3倍スターに変化する
/// </summary>
class Ratio : ItemEffect
{
    RaceArea area;
    float ratio;

    public Ratio(RaceArea area, float ratio, float endTime) : base(endTime)
    {
        this.area = area;
        this.ratio = ratio;
    }

    public override void Start()
    {
        //area.StarRatio = ratio;
        area.BeginNormalStarDoubler();
    }
    public override void End()
    {
        //area.StarRatio = 1.0f;
        area.EndNormalStarDoubler();
        base.End();
    }
}