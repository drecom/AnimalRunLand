using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class MissionData
{
    public int Id { get; set; }
    public string MissionText { get; set; }
}

public class MissionWindow : Tsl.UI.Window
{
    public Transform Content;
    public List<MissionData> missionData = new List<MissionData>();

    private void Awake()
    {
        LoadState(() => MakeList());
    }
    public override void OnClickButtonEvent(Transform button)
    {
        if (button.name == "CloseButton")
        {
            this.GetPanel().gameObject.SetActive(false);
            this.Close();
        }
        else
        {
            base.OnClickButtonEvent(button);
        }
    }
    private void MakeList()
    {
        foreach (var dat in this.missionData)
        {
            var missionBar = Instantiate(Resources.Load("UI/MissionBar")) as GameObject;
            missionBar.transform.SetParent(this.Content);
            missionBar.transform.localPosition = Vector3.zero;
            missionBar.transform.localScale = Vector3.one;
            var bar = missionBar.GetComponent<MissionBar>();
            bar.Mission.text = dat.MissionText;

            foreach (var clear in PlayerDataManager.PlayerData.CompletedMission)
            {
                if (dat.Id == int.Parse(clear))
                {
                    bar.Complete.gameObject.SetActive(true);
                    break;
                }
            }
        }
    }
    private string GetFilename()
    {
        return "mission_data";
    }

    public void LoadState(Action onEnd = null)
    {
        var textAsset = Resources.Load("Race/" + GetFilename()) as TextAsset;
        Tsl.Entity.Serializer.Deserialize(textAsset.text, out this.missionData);

        if (onEnd != null) { onEnd(); }
    }
}
}
