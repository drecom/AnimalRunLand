using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Liver.Entity
{

[CreateAssetMenu(menuName = "Configs/Dialog")]
public class Dialog : ScriptableObject
{
    public enum Kind
    {
        NoGps,               // GPS使えず
        NoMap,               // エリアマップが読み込めず
        TimeOut,             // GPS取得タイムアウト
    }


    [System.Serializable]
    public class Body
    {
        [Tooltip("タイトルバーに表示する文字列")]
        public string Title;
        [Tooltip("見出し")]
        public string SubTitle;
        [Tooltip("本文")]
        [Multiline]
        public string Message;

        [Tooltip("閉じるボタンの有無")]
        public bool CloseButton;
    }

    public Body[] bodies;


    public static Dialog Load()
    {
        return Resources.Load("Configs/DialogSettings") as Dialog;
    }

}

}
