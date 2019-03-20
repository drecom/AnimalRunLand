/// <summary>
/// レッドブル(5)：一定時間、ジャンプ力が上がる
/// </summary>
class RedBull : ItemEffect
{
    Animal animal;

    public RedBull(Animal animal, float endTime) : base(endTime)
    {
        this.animal = animal;
    }

    public override void Start()
    {
        animal.AdditionJump = Animal.JumpStandard - animal.Jump;
    }

    public override void End()
    {
        animal.AdditionJump = 0;
        base.End();
    }
}