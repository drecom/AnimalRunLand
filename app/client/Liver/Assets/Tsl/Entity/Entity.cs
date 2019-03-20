using System;

namespace Tsl.Entity
{
//アイテムの属性
public enum ItemAttribute
{
    None,  // 属性なし
    Fire,  // 火
    Water, // 水
    Earth, // 土
    Air,   // 風
    Light, // 光
}
// 属性の優劣に関する定義
public enum AttributeGain
{
    Even, // 互角
    Might, // 強い
    Lose   // 弱い
}



// 静的なコンフィギュレーション
static class Configuration
{
    static public bool UsingUnitychanFor3dModel = false; // 3Dモデルデータにユニティーちゃんを使用しない
}

class Directory
{
    public static string GetDataPath(string filename)
    {
        //#if UNITY_EDITOR
        //			return "../../data/" + filename;
        //#else
        return "data/" + System.IO.Path.GetFileNameWithoutExtension(filename);
        //#endif
    }

    public static string LoadData(string filename)
    {
        var text = UnityEngine.Resources.Load(GetDataPath(filename)) as UnityEngine.TextAsset;

        if (text == null)
        {
            return null;
        }

        return text.text;
    }
}

public class Entities
{
    // singleton
    private static Entities instance;
    private Entities()
    {
    }
    public static Entities Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Entities();
            }

            return instance;
        }
    }
}
}

