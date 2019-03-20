using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Liver
{

// スコア計算
public class Score
{
    // distance 走行距離
    // star     取得スター
    // powerup  パワーアップ回数
    public static int calc(float distance, int star, int powerup)
    {
        int baseScore = 0;
        // 1m = 1pt
        baseScore += (int)distance;
        // 1star = 5pt
        baseScore += star * 5;
        // 1powerup = 25pt
        baseScore += powerup * 25;
        // 倍率
        int multiplier = 10;
        multiplier += PlayerDataManager.PlayerData.CountCompletedMission();
        var score = baseScore * multiplier;
        Debug.Log($"Score: {score}");
        return score;
    }
}

}
