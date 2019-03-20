using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Tsl.Network;

namespace Liver
{
public class RankingWindow : Tsl.UI.Window
{
    public Toggle DailyBtn;
    public Toggle AnimalCarBtn;
    public Toggle AreaMeisterBtn;
    public Toggle AreaBtn;
    public Toggle PandaTgl;

    public Transform AnimalCarRanking;
    public Transform AnimalCarContent;
    public Text AnimalCarMyRankText;
    public Transform AnimalContent;
    public Transform Ranking;
    public Transform RankingContent;
    public Text RankingMyRankText;

    public Tsl.UI.AnimeButton AnimalInfoBtn;
    public Tsl.UI.AnimeButton RankingInfoBtn;

    public Sprite[] FrameSprites;
    public Sprite[] RankingIcons;

    private GameServer.RankingType selectRanking = GameServer.RankingType.AnimalScore;
    private AnimalKind selectAnimal = AnimalKind.Panda;
    private int myRank = 3;
    private Sprite[] frame;
    private Sprite[] rankIcon;


    private void Awake()
    {
        this.DailyBtn.onValueChanged.RemoveAllListeners();
        this.DailyBtn.onValueChanged.AddListener(isOn =>
        {
            if (isOn) { MakeRanking(GameServer.RankingType.TotalScore); }
        });
        this.AnimalCarBtn.onValueChanged.RemoveAllListeners();
        this.AnimalCarBtn.onValueChanged.AddListener(isOn =>
        {
            if (isOn)
            {
                // アニマルカーランキング選択時はパンダから表示
                foreach (Transform tgl in this.AnimalContent)
                {
                    tgl.GetComponent<Toggle>().isOn = false;
                }

                this.PandaTgl.isOn = true;
                //OnClickToggle(this.PandaTgl.transform);
            }
        });
        this.AreaMeisterBtn.onValueChanged.RemoveAllListeners();
        this.AreaMeisterBtn.onValueChanged.AddListener(isOn =>
        {
            if (isOn) { MakeRanking(GameServer.RankingType.StageWinnerRanking); }
        });
        this.AreaBtn.onValueChanged.RemoveAllListeners();
        this.AreaBtn.onValueChanged.AddListener(isOn =>
        {
            if (isOn) { MakeRanking(GameServer.RankingType.StageHighScore); }
        });
        this.AnimalInfoBtn.OnClick = () => { OnClickInformation(this.selectRanking); };
        this.RankingInfoBtn.OnClick = () => { OnClickInformation(this.selectRanking); };
    }
    protected override void OnStart()
    {
        MakeRanking(GameServer.RankingType.TotalScore);
    }
    public void OnClickToggle(Transform trn)
    {
        var tgl = trn.GetComponent<Toggle>();

        if (!tgl.isOn) { return; }

        switch (trn.name)
        {
            case "Panda":
                this.selectAnimal = AnimalKind.Panda;
                MakeRanking(GameServer.RankingType.AnimalScore, (int)AnimalKind.Panda);
                break;

            case "Capybara":
                this.selectAnimal = AnimalKind.Capybara;
                MakeRanking(GameServer.RankingType.AnimalScore, (int)AnimalKind.Capybara);
                break;

            case "Giraffe":
                this.selectAnimal = AnimalKind.Giraffe;
                MakeRanking(GameServer.RankingType.AnimalScore, (int)AnimalKind.Giraffe);
                break;

            case "Elephant":
                this.selectAnimal = AnimalKind.Elephant;
                MakeRanking(GameServer.RankingType.AnimalScore, (int)AnimalKind.Elephant);
                break;

            case "Horse":
                this.selectAnimal = AnimalKind.Horse;
                MakeRanking(GameServer.RankingType.AnimalScore, (int)AnimalKind.Horse);
                break;

            case "Rabbit":
                this.selectAnimal = AnimalKind.Rabbit;
                MakeRanking(GameServer.RankingType.AnimalScore, (int)AnimalKind.Rabbit);
                break;

            case "Lion":
                this.selectAnimal = AnimalKind.Lion;
                MakeRanking(GameServer.RankingType.AnimalScore, (int)AnimalKind.Lion);
                break;

            case "Pig":
                this.selectAnimal = AnimalKind.Pig;
                MakeRanking(GameServer.RankingType.AnimalScore, (int)AnimalKind.Pig);
                break;
        }
    }
    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }
    private void EraseRankingList()
    {
        this.AnimalCarRanking.gameObject.SetActive(false);
        this.Ranking.gameObject.SetActive(false);
    }
    private void MakeRanking(GameServer.RankingType kind, int animal = 0)
    {
        // 同じ画面を選択されてもなにもしない
        if (this.selectRanking == kind && kind != GameServer.RankingType.AnimalScore) { return; }
        else { this.selectRanking = kind; }

        bool isAnimal = kind == GameServer.RankingType.AnimalScore ? true : false;

        // 一回消す
        foreach (Transform t in isAnimal ? this.AnimalCarContent : this.RankingContent)
        {
            GameObject.Destroy(t.gameObject);
        }

        EraseRankingList();
        var rank = new GameServer();
        string stageId = kind == GameServer.RankingType.StageHighScore ? PlayerDataManager.PlayerData.HighScoreData.MapId : "";
        StartCoroutine(rank.RetrieveRankingCoroutine((int)kind, stageId, animal, 0, res =>
        {
            int cnt = 0;
            // res.Orderは0が1位。参加なしは-1
            string myRank = res.Order > -1 ? $"あなたの順位は{res.Order + 1}位です" : "このランキングには参加していません。";

            if (isAnimal) { this.AnimalCarMyRankText.text = myRank; }
            else { this.RankingMyRankText.text = myRank; }

            Action MakeBar = () =>
            {
                foreach (var dat in res.Rankings)
                {
                    var bar = GetRankingBarPrefab(isAnimal);
                    bar.PlayerNameTxt.text = dat.PlayerName;
                    bar.ScoreTxt.text = kind == GameServer.RankingType.StageWinnerRanking ? $"{dat.Score}回" : $"{dat.Score}pt";
                    Sprite frame = cnt == res.Order ? this.FrameSprites[1] : this.FrameSprites[0];
                    bar.RankingBaseImg.sprite = frame;
                    Sprite icon = cnt < 3 ? this.RankingIcons[cnt] : this.RankingIcons[3];
                    bar.RankingImg.sprite = icon;

                    if (cnt > 2)
                    {
                        bar.RankingNumTxt.text = (cnt + 1).ToString();
                        bar.RankingNumTxt.gameObject.SetActive(true);
                    }

                    ++cnt;
                }
            };

            // レースを行っていればエリア名を取得してからリスト作成
            if (PlayerDataManager.PlayerData.HighScoreData.MapId != "")
            {
                // ハイスコア記録したId
                Map.AreaName.Get(PlayerDataManager.PlayerData.HighScoreData.MapId, name =>
                {
                    if (kind == GameServer.RankingType.StageHighScore)
                    {
                        var areaBar = GetRankingBarPrefab(isAnimal);
                        areaBar.RankingBaseImg.sprite = this.FrameSprites[0];
                        areaBar.AreaTxt.text = name;
                        areaBar.RankingImg.gameObject.SetActive(false);
                    }

                    MakeBar();
                });
            }
            else
            {
                MakeBar();
            }
        }));
        this.AnimalCarRanking.gameObject.SetActive(isAnimal);
        this.Ranking.gameObject.SetActive(!isAnimal);
    }
    private RankingBar GetRankingBarPrefab(bool isAnimal)
    {
        var rankingBar = Instantiate(Resources.Load("UI/RankingBar")) as GameObject;
        rankingBar.transform.SetParent(isAnimal ? this.AnimalCarContent : this.RankingContent);
        rankingBar.transform.localPosition = Vector3.zero;
        rankingBar.transform.localScale = Vector3.one;
        return rankingBar.GetComponent<RankingBar>();
    }
    private void OnClickInformation(GameServer.RankingType kind)
    {
        var infoDat =  Entity.InformationData.infoList[(int)kind];
        var dialog = BaseScene.AddModalWindow<Liver.DialogWindow>("DialogWindow");
        dialog.SetDialog(infoDat.Title, infoDat.Title2, infoDat.Text, null, null, null);
    }
}
}
