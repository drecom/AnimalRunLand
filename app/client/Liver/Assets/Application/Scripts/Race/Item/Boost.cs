/// <summary>
/// ブースト(3)：一定時間、速度が速くなる
/// </summary>
class Boost : ItemEffect
{
    Animal animal;
    float addition;
    public Boost(Animal animal, float addition, float endTime) : base(endTime)
    {
        this.animal = animal;
        this.addition = addition;
    }
    public override void Start()
    {
        animal.AdditionSpeed = animal.MaxSpeed * addition;
    }
    public override void End()
    {
        animal.AdditionSpeed = 0;
        base.End();
    }

}