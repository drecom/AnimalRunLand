using UnityEngine;
/// <summary>
/// マグネット(1)：一定時間、アイテム回収の半径が広がる
/// </summary>
class Magnet : ItemEffect
{
    GameObject normal;
    GameObject boost;
    public Magnet(GameObject go, float endTime) : base(endTime)
    {
        foreach (Collider c in go.GetComponentsInChildren<Collider>(true))
        {
            switch (c.name)
            {
                case "ItemNormal":
                    normal = c.gameObject;
                    break;

                case "ItemBoost":
                    boost = c.gameObject;
                    break;
            }
        }
    }
    public override void Start()
    {
        boost.SetActive(true);
    }

    public override void End()
    {
        boost.SetActive(false);
        base.End();
    }
}