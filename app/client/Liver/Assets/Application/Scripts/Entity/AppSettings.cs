using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Liver.Entity
{

[CreateAssetMenu(menuName = "Configs/AppSettings")]
public class AppSettings : ScriptableObject
{
    [Tooltip("Mapデータのあるサーバー")]
    public string Server = "";

    [Tooltip("Folder")]
    public string Folfer    = "beef_files_v5";
    public string SubFolder = "osm_beefmap";

    [Tooltip("AROW Webサイト")]
    public string ArowURL = "https://arow.world";
    [Tooltip("プライバシーポリシー")]
    public string PrivacyPolicyURL = "https://www.drecom.co.jp";
    [Tooltip("利用規約")]
    public string TermsOfUseURL = "https://www.drecom.co.jp";


    // シングルトン実装
    static AppSettings _Instance;
    public static AppSettings Instance
    {
        get
        {
            if (_Instance == null) { _Instance = Resources.Load<AppSettings>("Configs/AppSettings"); }

            return _Instance;
        }
    }


    // Mapが格納されているURLを取得
    public string MapUrl()
    {
        return $"{this.Server}/{this.Folfer}/{this.SubFolder}/";
    }
}

}
