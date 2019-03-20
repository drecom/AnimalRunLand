using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Tsl.Entity;
using Util.Extension;
using Boot = Api.Net.Boot;
using Player = Api.Net.Player;
using Ranking = Api.Net.Ranking;
using PlayerData = Liver.Entity.PlayerData;

namespace Tsl.Network
{
public partial class GameServer : MonoBehaviour
{
    private ApiStatus<Player.Signup.Response> signupApi = new ApiStatus<Player.Signup.Response>();
    private ApiStatus<Player.SaveScore.Response> saveScoreApi = new ApiStatus<Player.SaveScore.Response>();
    private ApiStatus<Ranking.Retrieve.Response> retrieveRankingApi = new ApiStatus<Ranking.Retrieve.Response>();


    // コンストラクタ
    private void Awake()
    {
        AwakeCommon();
        var added = new ApiStatusBase[]
        {
            signupApi,
            saveScoreApi,
            retrieveRankingApi,
        };
        var list = this.allApi.ToList();
        list.AddRange(added);
        this.allApi = list.ToArray();
        DontDestroyOnLoad(this.gameObject);
    }

    // SignUp処理
    public IEnumerator SignUpCoroutine(PlayerData pd, Action<Player.Signup.Response> onComplete = null)
    {
        yield return this.signupApi.Request(() =>
                                            Api.Net.Player.Signup.Post(this.Requests, new Api.Net.Player.Signup.Request(
                                                    pd.PlayerName,
                                                    Entity.Storage.SaveToString<PlayerData>(pd),
                                                    "passcode", // 機種変更用のパスワード
                                                    0)),
                                            onComplete);
    }

    // スコアの送信
    public void SendScore(string stageId, int score, int animalId, Action<Player.SaveScore.Response> onComplete)
    {
        StartCoroutine(SendScoreCoroutine(stageId, score, animalId, onComplete));
    }
    public IEnumerator SendScoreCoroutine(string stageId, int score, int animalId, Action<Player.SaveScore.Response> onComplete = null)
    {
        this.saveScoreApi.CacheData = null;
        yield return this.saveScoreApi.Request(() =>
                                               Api.Net.Player.SaveScore.Post(this.Requests, new Api.Net.Player.SaveScore.Request(
                                                       stage_id: stageId,
                                                       score: score,
                                                       kind1: animalId,
                                                       kind2: 0,
                                                       kind3: 0)),
                                               onComplete);
    }
    public Player.SaveScore.Response SendScoreResult { get { return this.saveScoreApi.CacheData; } }


    // ランキング情報の取得
    public enum RankingType
    {
        TotalScore = 0,         // 全体のスコア順
        StageHighScore = 1,     // stageId毎のハイスコア
        AnimalScore = 2,        // animalIdごとのハイスコア
        StageWinnerRanking = 3, // ステージで1位になった数のランキング
    }
    // term: 0:すべて 1:週間 2:月間
    public IEnumerator RetrieveRankingCoroutine(int rankingType, string stageId, int animalId, int term, Action<Ranking.Retrieve.Response> onComplete)
    {
        this.retrieveRankingApi.CacheData = null;
        yield return this.retrieveRankingApi.Request(() =>
                     Ranking.Retrieve.Post(this.Requests, new Ranking.Retrieve.Request(
                                               ranking_type: rankingType,
                                               stage_id: stageId,
                                               kind1: animalId,
                                               kind2: 0,
                                               term: term,
                                               count: 100)),
                     onComplete);
    }
}
}