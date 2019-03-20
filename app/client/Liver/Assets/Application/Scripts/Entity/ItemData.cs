using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liver.Entity
{
public class ItemData
{
    public string ItemName { get; set; }
    public string ItemText { get; set; }
    public int[] StarNum { get; set; }
    public float[] Time { get; set; }

    public static List<ItemData> itemList;

    static ItemData()
    {
        var textAsset = Resources.Load("Race/" + "item_data") as TextAsset;
        Tsl.Entity.Serializer.Deserialize(textAsset.text, out itemList);
    }

    public static ItemData Get(PowerUpItem type)
    {
        return itemList[(int)type];
    }
}
}
