using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Liver
{
public class Sound : MonoBehaviour
{
    public enum BgmKind
    {
        TOP = 0,            // TOP
        Animal_Capybara,    // カピバラ
        Animal_Panda,       // パンダ
        Animal_Kirin,       // キリン
        Animal_elephant,    // ゾウ
        Animal_Horse,       // ウマ
        Animal_Rabbit,      // ウサギ
        Animal_Lion,        // ライオン
        Animal_Pig,         // ブタ
        Hurryup,            // 残り時間30秒からの煽り
        Loading,            // 本物の動物夢見るシーン
        result,             // リザルト画面
    }

    public enum SeKind
    {
        botton_main = 1,    // ボタン音
        item_gradeup,       // グレードアップ等完了音
        unlocked_animalcar, // アニマルカー開放完了音
        jump_short,         // ジャンプ(小)
        jump_standard,      // ジャンプ(中)
        hit_wall,           // 障害物にぶつかり時間減る
        got_powerups,       // パワーアップアイテム取得時
        got_star,           // スター取得時
        Time_Recovery,      // 時間回復時
        turn_around,        // Uターン時
        Countdown,          // ゲーム再開カウントダウン
        SE_Powerup_Items,   // パワーアップアイテム発動中のエフェクト音
        timeup,             // Timeupジングル
    }

    public enum SoundAnimationType
    {
        FadeIn,
        FadeOut,
        None
    }


    // アニマルカー別BGM番号
    public static BgmKind AnimalBgm(AnimalKind kind)
    {
        Dictionary<AnimalKind, BgmKind> bgm = new Dictionary<AnimalKind, BgmKind>()
        {
            { AnimalKind.Capybara, BgmKind.Animal_Capybara },
            { AnimalKind.Panda,    BgmKind.Animal_Panda },
            { AnimalKind.Giraffe,  BgmKind.Animal_Kirin },
            { AnimalKind.Elephant, BgmKind.Animal_elephant },
            { AnimalKind.Horse,    BgmKind.Animal_Horse },
            { AnimalKind.Rabbit,   BgmKind.Animal_Rabbit },
            { AnimalKind.Lion,     BgmKind.Animal_Lion },
            { AnimalKind.Pig,      BgmKind.Animal_Pig }
        };
        return bgm[kind];
    }
}
}


