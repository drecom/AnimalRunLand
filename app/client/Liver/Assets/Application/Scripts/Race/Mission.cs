using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Liver
{

public class Mission
{
    // アニマルカーアンロック
    public static void UnlockAnimalCar(Action onEnd = null)
    {
        if (!PlayerDataManager.PlayerData.IsCompletedMission("21"))
        {
            // 初めてのアンロック
            PlayerDataManager.PlayerData.CompleteMission("21");
            return;
        }

        var num = PlayerDataManager.PlayerData.ReleasedAnimals.Count;

        if (num == 8)
        {
            // 全てアンロック
            PlayerDataManager.PlayerData.CompleteMission("22");
        }

        if (onEnd != null) { onEnd(); }
    }

    // パワーアップアイテムグレードアップ
    public static void GradeUpPowerUpItem(Action onEnd = null)
    {
        if (!PlayerDataManager.PlayerData.IsCompletedMission("23"))
        {
            // 初めてのグレードアップ
            PlayerDataManager.PlayerData.CompleteMission("23");
        }

        foreach (var item in PlayerDataManager.PlayerData.PowerUpItemLevels)
        {
            // どれか１つのレベルをカンスト
            if (item.Value == 4)
            {
                PlayerDataManager.PlayerData.CompleteMission("24");
                //return;
            }
        }

        if (onEnd != null) { onEnd(); }
    }


    // レースをプレイした
    // NOTE プレイヤーデータを更新したのちに実行
    public static void RacePlayed(float distance, int star, int powerup)
    {
        runDistance(distance);
        getStar(star);
        numPowerUp(powerup);
        totalDistance();
        areaPlay();
    }


    class Pair<Type>
    {
        public Type value;
        public string mission;
    }

    // 走行距離
    static void runDistance(float distance)
    {
        Pair<float>[] pair =
        {
            new Pair<float>(){ value =  300, mission = "1" },
            new Pair<float>(){ value =  500, mission = "2" },
            new Pair<float>(){ value =  700, mission = "3" },
            new Pair<float>(){ value = 1000, mission = "4" },
            new Pair<float>(){ value = 1500, mission = "5" },
            new Pair<float>(){ value = 2000, mission = "6" },
        };

        foreach (var p in pair)
        {
            if (distance < p.value) { break; }

            PlayerDataManager.PlayerData.CompleteMission(p.mission);
        }
    }

    // 取得スター
    static void getStar(int star)
    {
        Pair<int>[] pair =
        {
            new Pair<int>(){ value =  10, mission =  "7" },
            new Pair<int>(){ value =  50, mission =  "8" },
            new Pair<int>(){ value = 100, mission =  "9" },
            new Pair<int>(){ value = 150, mission = "10" },
            new Pair<int>(){ value = 200, mission = "11" },
            new Pair<int>(){ value = 250, mission = "12" },
            new Pair<int>(){ value = 300, mission = "13" },
        };

        foreach (var p in pair)
        {
            if (star < p.value) { break; }

            PlayerDataManager.PlayerData.CompleteMission(p.mission);
        }
    }

    // パワーアップアイテム発動回数
    static void numPowerUp(int powerup)
    {
        Pair<int>[] pair =
        {
            new Pair<int>(){ value = 1, mission =  "14" },
            new Pair<int>(){ value = 2, mission =  "15" },
            new Pair<int>(){ value = 3, mission =  "16" },
            new Pair<int>(){ value = 4, mission =  "17" },
            new Pair<int>(){ value = 5, mission =  "18" },
            new Pair<int>(){ value = 6, mission =  "19" },
            new Pair<int>(){ value = 7, mission =  "20" },
        };

        foreach (var p in pair)
        {
            if (powerup < p.value) { break; }

            PlayerDataManager.PlayerData.CompleteMission(p.mission);
        }
    }

    // 累計走行距離
    static void totalDistance()
    {
        Pair<int>[] pair =
        {
            new Pair<int>(){ value =  10000, mission =  "25" },
            new Pair<int>(){ value =  50000, mission =  "26" },
            new Pair<int>(){ value = 100000, mission =  "27" },
        };
        var dist = PlayerDataManager.PlayerData.TotalDistance;

        foreach (var p in pair)
        {
            if (dist < p.value) { break; }

            PlayerDataManager.PlayerData.CompleteMission(p.mission);
        }
    }

    // エリアのプレイ回数
    static void areaPlay()
    {
        Pair<int>[] pair =
        {
            new Pair<int>(){ value =  1, mission =  "28" },
            new Pair<int>(){ value =  5, mission =  "29" },
            new Pair<int>(){ value = 10, mission =  "30" },
        };
        var num = PlayerDataManager.PlayerData.AreaHistory.Count;

        foreach (var p in pair)
        {
            if (num < p.value) { break; }

            PlayerDataManager.PlayerData.CompleteMission(p.mission);
        }
    }
}
}
