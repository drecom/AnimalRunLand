using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;



namespace Liver
{

public class UserName
{
    // 部分一致
    List<string> ngWord1;
    // 完全一致
    List<string> ngWord2;


    public UserName()
    {
        LoadText("Configs/ngword1", out this.ngWord1);
        LoadText("Configs/ngword2", out this.ngWord2);
    }


    void LoadText(string path, out List<string> list)
    {
        list = new List<string>();
        var textAsset = Resources.Load(path) as TextAsset;
        var values = textAsset.text.Split('\n');

        foreach (var t in values)
        {
            list.Add(t);
        }
    }

    // ユーザー名として使えるか判別
    public bool Validate(string text)
    {
        // 文字列の長さ
        // NOTE InputFieldでも抑制可能
        var len = text.Length;

        if (len < 1 || len > 8) { return false; }

        // NOTE 空白はどうする??

        // 完全一致
        if (this.ngWord2.Contains(text)) { return false; }

        // 部分一致
        return !this.ngWord1.Exists((obj) => obj.Contains(text));
    }

}

}
