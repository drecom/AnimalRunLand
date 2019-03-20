using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Liver.Entity;

namespace Liver
{
public class StoreWindow : Tsl.UI.Window
{
    public Transform ItemContent;
    public Transform AnimalContent;
    public Text StarTxt;
    public Color OpenColor;
    public Color CloseColor;
    public Button AnimalOpenBtn;

    public Transform Content;
    public Text HasStarTxt;

    private PowerUpItem selectItemKind = PowerUpItem.Magnet;
    private AnimalKind selectAnimalKind = 0;

    private int itemNeedStar = 0;
    private int openNeedStar = 0;
    protected override void OnStart()
    {
        MakeItemList();
    }

    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "GradeUpButton":
                //GradeUp(true);
                break;

            case "BuyButton":
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }
    private GameObject GetPrefab(string fileName, bool isItem)
    {
        var prefab = Instantiate(Resources.Load($"UI/{fileName}")) as GameObject;
        prefab.transform.SetParent(isItem ? this.Content : this.AnimalContent);
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localScale = Vector3.one;
        return prefab;
    }
    private void MakeItemList()
    {
        foreach (Transform tran in this.Content)
        {
            GameObject.Destroy(tran.gameObject);
        }

        this.HasStarTxt.text = PlayerDataManager.PlayerData.StarNum.ToString();
        this.HasStarTxt.text = $"所持★ ： {PlayerDataManager.PlayerData.StarNum}";
        var sprites = Resources.LoadAll<Sprite>("Textures/UiParts/Item");

        foreach (PowerUpItem pKind in Enum.GetValues(typeof(PowerUpItem)))
        {
            var kind = pKind;
            var itemDat = ItemData.itemList[(int)kind];
            var itemTgl = GetPrefab("GradeupItem", true);
            var bar = itemTgl.GetComponent<GradeupItem>();
            Debug.Log($"kind:{kind.ToString()} level:{PlayerDataManager.PlayerData.GetPoweUpItemLevel(kind)}");

            foreach (var item in PlayerDataManager.PlayerData.PowerUpItemLevels)
            {
                if (item.Key == kind.ToString())
                {
                    // TODO:アイテム名取得確認
                    bar.ItemName.text = itemDat.ItemName;
                    Debug.Log($"NameTxt:{bar.ItemName.text} Item:{itemDat.ItemName}");
                    bar.ItemIcon.sprite = sprites[(int)kind];

                    for (int i = 0; i <= PlayerDataManager.PlayerData.GetPoweUpItemLevel(kind); ++i)
                    {
                        Debug.Log($"i:{i} level:{item.Value}");
                        bar.Level.Find((i + 1).ToString()).gameObject.SetActive(true);
                    }

                    if (PlayerDataManager.PlayerData.GetPoweUpItemLevel(kind) < 4)
                    {
                        bar.NeedStar.text = $"{itemDat.StarNum[item.Value]}";
                    }
                    else
                    {
                        bar.NeedStar.text = "Level\nMAX";
                        bar.BuyBtn.interactable = false;
                    }

                    bar.ItemDiscription.text = $"{itemDat.Time[item.Value]}秒間、{itemDat.ItemText}";
                    bar.BuyBtn.onClick.RemoveAllListeners();
                    bar.BuyBtn.onClick.AddListener(() =>
                    {
                        if (PlayerDataManager.PlayerData.GetPoweUpItemLevel(kind) == 4) { return; }

                        this.itemNeedStar = itemDat.StarNum[item.Value];
                        GradeUp(kind, true);
                    });
                }
            }
        }
    }

    private void GradeUp(PowerUpItem kind, bool isItem)
    {
        var dialog = BaseScene.AddModalWindow<DialogWindow>("DialogWindow");

        if (PlayerDataManager.PlayerData.StarNum >= (isItem ? this.itemNeedStar : this.openNeedStar))
        {
            dialog.SetDialog("確認", "", "よろしいですか？", null,
                             () =>
            {
                if (isItem)
                {
                    Mission.GradeUpPowerUpItem(() =>
                    {
                        PlayerDataManager.PlayerData.StarNum -= this.itemNeedStar;
                        PlayerDataManager.PlayerData.SetPowerUpItemLevel(kind, PlayerDataManager.PlayerData.GetPoweUpItemLevel(kind) + 1);
                        //PlayerDataManager.Save();
                        MakeItemList();
                        dialog.onClose();
                    });
                }
                else
                {
                    Mission.UnlockAnimalCar(() =>
                    {
                        PlayerDataManager.PlayerData.StarNum -= this.openNeedStar;
                        //PlayerDataManager.Save();
                        //MakeAnimalCarList();
                        dialog.onClose();
                    });
                }
            }, () => { dialog.onClose(); });
        }
        else
        {
            dialog.SetDialog("確認", "", "スターが足りません。", () => { dialog.onClose(); }, null, null);
        }
    }
}
}
