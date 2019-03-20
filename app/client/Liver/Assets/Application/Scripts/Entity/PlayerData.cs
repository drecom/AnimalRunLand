using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Liver.Entity
{
// ストレージやサーバーに保存するプレイヤーデータです
// NOTE 一部の値はAPIを通して読み書きしてください
public class PlayerData
{
    public string PlayerName = "no name";
    public string LastSelectedAnimalId = AnimalKind.Panda.ToString();     // 最後にプレイしたAnimal
    public string LastPlayMapId = "";                                     // 最後にプレイしたマップ

    // ハイスコアを記録したステージ
    public class HighScore
    {
        public int Score    = 0;
        public string MapId = "";
    }
    public HighScore HighScoreData = new HighScore();

    public int TotalPlayCount = 0;                  // 総プレイ回数
    public int StarNum = 0;                         // 所持スター
    public float TotalDistance = 0;                 // 累計走行距離

    // 過去に走ったフィールド履歴
    public List<string> AreaHistory = new List<string>();
    // 直近10件のプレイ履歴
    public class Result
    {
        // エリア名
        public string AreaName;
        // 緯度経度
        public float Latitude;
        public float Longitude;
        // 使用したアニマルカー
        public string Animal;
        // 取得スター数
        public int Star;
        // スコア
        public int Score;
        // 走行距離
        public float Distance;
    }
    public List<Result> ResultHistory = new List<Result>();

    // パワーアップアイテムのレベル
    public Dictionary<string, int> PowerUpItemLevels = new Dictionary<string, int>()
    {
        { PowerUpItem.Magnet.ToString(), 0 },
        { PowerUpItem.Barrier.ToString(), 0 },
        { PowerUpItem.Boost.ToString(), 0 },
        { PowerUpItem.EnergyDrink.ToString(), 0 },
        { PowerUpItem.Ratio.ToString(), 0 },
        { PowerUpItem.Tire.ToString(), 0 },
    };

    // 解放済みアニマルカー
    public List<string> ReleasedAnimals = new List<string>()
    {
        AnimalKind.Panda.ToString(),
    };

    // 達成済みミッション
    public List<string> CompletedMission = new List<string>();

    // 各種設定
    public bool BgmOn = true;
    public bool SeOn = true;


    // NOTE セーブデータの変更は必ずAPI経由で
    public void SetPlayerName(string name)
    {
        this.PlayerName = name;
        PlayerDataManager.DoUpdate();
    }

    // プレイ後のデータ更新
    public void SetResult(string area, float latitude, float longitude, AnimalKind animal, int star, float distance, int score)
    {
        Debug.Log($"Result: Map: {area}  Animal: {animal.ToString()} Star: {star} Dist: {distance} Score: {score}");

        // サイズが10を超えている時は先頭要素を削除
        if (this.ResultHistory.Count > 10)
        {
            this.ResultHistory.RemoveAt(0);
        }

        var result = new Result()
        {
            AreaName  = area,
            Latitude  = latitude,
            Longitude = longitude,
            Animal    = animal.ToString(),
            Star      = star,
            Score     = score,
            Distance  = distance
        };
        // 最後尾に追加
        this.ResultHistory.Add(result);
        // 最後のプレイ状況
        //this.LastSelectedAnimalId = animal.ToString();
        this.LastPlayMapId        = area;
        // 所持スター
        this.StarNum += star;
        // 総プレイ回数
        this.TotalPlayCount += 1;
        // 総走行距離
        this.TotalDistance += distance;
        // エリア履歴
        RegistPlayArea(area);

        // ハイスコアを記録したエリア
        // FIXME 同じスコアだった場合の扱い
        if (score > this.HighScoreData.Score)
        {
            this.HighScoreData.Score = score;
            this.HighScoreData.MapId = area;
        }

        PlayerDataManager.DoUpdate();
    }

    // 所持スター変更
    public void SetStarNum(int star)
    {
        this.StarNum = star;
        PlayerDataManager.DoUpdate();
    }


    public void EnableBgm(bool value)
    {
        this.BgmOn = value;
        PlayerDataManager.DoUpdate();
    }

    public void EnableSe(bool value)
    {
        this.SeOn = value;
        PlayerDataManager.DoUpdate();
    }


    // 選択中のアニマルカーを変更
    public void ChangeSelectedAnimalCar(AnimalKind kind)
    {
        this.LastSelectedAnimalId = kind.ToString();
        PlayerDataManager.DoUpdate();
    }


    // 解放済アニマルカーか？
    public bool CanUseAnimal(AnimalKind kind)
    {
        var key = kind.ToString();
        return this.ReleasedAnimals.Contains(key);
    }

    // アニマル解放
    // 戻り値 true 解放した
    public bool RegistAnimal(AnimalKind kind)
    {
        if (CanUseAnimal(kind)) { return false; }

        var key = kind.ToString();
        this.ReleasedAnimals.Add(key);
        PlayerDataManager.DoUpdate();
        return true;
    }

    // パワーアップアイテムのレベルを取得
    public int GetPoweUpItemLevel(PowerUpItem kind)
    {
        var key = kind.ToString();

        if (this.PowerUpItemLevels.ContainsKey(key))
        {
            return this.PowerUpItemLevels[key];
        }

        return 0;
    }

    // パワーアップアイテムのレベルを指定
    public void SetPowerUpItemLevel(PowerUpItem kind, int level)
    {
        var key = kind.ToString();
        this.PowerUpItemLevels[key] = level;
        PlayerDataManager.DoUpdate();
    }

    // 走ったことのあるフィールドか？
    public bool IsPlayedArea(string areaName)
    {
        return this.AreaHistory.Contains(areaName);
    }

    // 走ったフィールドを登録
    // 戻り値 true 初めて走った
    bool RegistPlayArea(string areaName)
    {
        if (IsPlayedArea(areaName)) { return false; }

        this.AreaHistory.Add(areaName);
        PlayerDataManager.DoUpdate();
        return true;
    }

    // ミッションを達成しているか？
    public bool IsCompletedMission(string missionId)
    {
        return this.CompletedMission.Contains(missionId);
    }

    // ミッション達成
    // 戻り値 true 達成した
    public bool CompleteMission(string missionId)
    {
        if (IsCompletedMission(missionId)) { return false; }

        this.CompletedMission.Add(missionId);
        PlayerDataManager.DoUpdate();
        return true;
    }

    // ミッション達成数
    public int CountCompletedMission()
    {
        return this.CompletedMission.Count;
    }
}
}
