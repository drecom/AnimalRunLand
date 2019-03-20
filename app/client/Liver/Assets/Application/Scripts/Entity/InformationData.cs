using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Liver.Entity
{
public class InformationData
{
    public string Kind { get; set; }
    public string Title { get; set; }
    public string Title2 { get; set; }
    public string Text { get; set; }

    public static List<InformationData> infoList;

    static InformationData()
    {
        var textAsset = Resources.Load("Configs/" + "Information") as TextAsset;
        Tsl.Entity.Serializer.Deserialize(textAsset.text, out infoList);
    }

    public static InformationData Get(Tsl.Network.GameServer.RankingType type)
    {
        return infoList[(int)type];
    }
}
}
