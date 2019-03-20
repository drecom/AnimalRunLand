using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tsl.Entity;
using UnityEngine;
using UnityEngine.UI;
using Liver.Entity;

namespace Liver
{
public class AnimalSelectWindow : Tsl.UI.Window
{
    public Transform AnimalContent;
    private AnimalKind animalKind = AnimalKind.Panda;
    private AnimalKind selectAnimal = AnimalKind.Panda;
    private bool isFirst = true;

    [SerializeField]
    Monitor monitorSelected;
    [SerializeField]
    Text name;
    [SerializeField]
    CustomUI.UIText desc;
    [SerializeField]
    Text strong;
    [SerializeField]
    Text weak;
    [SerializeField]
    Button okButton;
    [SerializeField]
    Button buyButton;
    [SerializeField]
    Text buyText;
    [SerializeField]
    Text starText;
    [SerializeField]
    Sprite[] selectIcon;    // 0:Lock 1:Check
    [SerializeField]
    Button selectBtn;
    [SerializeField]
    Image selectedImg;

    Dictionary<AnimalKind, AnimalToggle> animaltgls = new Dictionary<AnimalKind, AnimalToggle>();

    // 道路スクロール用
    List<Material> roadMaterial = new List<Material>();
    float scrollSpeed = 0;
    float offset = 0;


    protected override void OnStart()
    {
        this.animalKind = (AnimalKind)Enum.Parse(typeof(AnimalKind), PlayerDataManager.PlayerData.LastSelectedAnimalId);
        this.selectAnimal = this.animalKind;
        MakeSelectAnimal();
        base.OnStart();
    }

    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "OkButton":
                //Liver.RaceWindow.AnimalKind = this.animalKind;
                //BaseScene.AddModalWindow<Tsl.UI.Window, Argments>("RaceWindow", null);
                this.Close();
                break;

            case "BackButton":
                this.Close();
                break;

            case "Buy":
                var need = AnimalSettings.Instance[animalKind].registCost;
                var has = PlayerDataManager.PlayerData.StarNum;
                var dialog = BaseScene.AddModalWindow<DialogWindow>("DialogWindow");

                if (has >= need)
                {
                    // 解放確認
                    dialog.SetDialog("確認", "", string.Format("★{0}を使って解放します\nよろしいでしょうか", need), null,
                                     () =>
                    {
                        PlayerDataManager.PlayerData.SetStarNum(has - need);

                        if (PlayerDataManager.PlayerData.RegistAnimal(animalKind))
                        {
                            SetStar();
                            var tgl = animaltgls[animalKind];
                            tgl.AnimalTgl.onValueChanged.Invoke(true);
                            tgl.AnimalImg.color = Color.white;
                            tgl.Monitor.GetComponent<RawImage>().color = Color.white;
                            tgl.SelectIcon.gameObject.SetActive(false);
                        }

                        Mission.UnlockAnimalCar();
                        dialog.Close();
                    },
                    () =>
                    {
                        dialog.Close();
                    });
                }
                else
                {
                    // 解放できない
                    dialog.SetDialog("確認", "", "スターが足りません。", () => { dialog.onClose(); }, null, null);
                }

                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }

    private void MakeSelectAnimal()
    {
        // 表示の並び順 : 開放コストによるソート
        var enums = Enum.GetValues(typeof(AnimalKind)) as AnimalKind[];
        Array.Sort<AnimalKind>(enums, (a, b) =>
        {
            var aCost = AnimalSettings.Instance[a].registCost;
            var bCost = AnimalSettings.Instance[b].registCost;

            if (aCost == bCost) { return a.CompareTo(b); }
            else { return aCost.CompareTo(bCost); }
        });

        foreach (AnimalKind kind in enums)
        {
            var tgl = GetPrefab("UI/AnimalToggle", this.AnimalContent).GetComponent<AnimalToggle>();
            tgl.AnimalKind.text = $"{(int)kind}";
            animaltgls.Add(kind, tgl);
            var prefab = Resources.Load<GameObject>(string.Format("Models/Animal/{0}", AnimalSettings.Instance[kind].model));
            tgl.AnimalImg.gameObject.SetActive(false);
            /// TODO : タイプによって該当モデルを読み込む
            var go = GameObject.Instantiate<GameObject>(prefab);
            go.transform.SetParent(tgl.Monitor.MonitorRoot.transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, -7f);
            go.transform.localEulerAngles = new Vector3(0f, 180f, 0f);

            if (!PlayerDataManager.PlayerData.ReleasedAnimals.Contains(kind.ToString()))
            {
                tgl.AnimalImg.color = Color.gray;
                tgl.Monitor.GetComponent<RawImage>().color = Color.gray;
                tgl.SelectIcon.sprite = this.selectIcon[0];
                tgl.SelectIcon.gameObject.SetActive(true);
            }

            tgl.AnimalTgl.onValueChanged.AddListener(isOn =>
            {
                var isOpen = PlayerDataManager.PlayerData.ReleasedAnimals.Contains(kind.ToString());
                this.animalKind = (AnimalKind)int.Parse(tgl.AnimalKind.text);
                var isSelected = isOpen && this.animalKind == this.selectAnimal;

                if (isOn)
                {
                    // Window表示する際に再生されてしまうため、初回は鳴らさない
                    if (!this.isFirst) { SeManager.Instance.Play(Sound.SeKind.botton_main.ToString(), 0.0f, false, 1.0f); }

                    this.isFirst = false;
                    this.animalKind = (AnimalKind)int.Parse(tgl.AnimalKind.text);
                    Debug.Log(this.animalKind.ToString());
                    SetDetail();
                    // ボタンの有無効設定
                    okButton.interactable = PlayerDataManager.PlayerData.ReleasedAnimals.Contains(kind.ToString());
                    this.selectBtn.gameObject.SetActive(isOpen);
                    this.selectedImg.gameObject.SetActive(isSelected);
                    this.buyButton.gameObject.SetActive(!isOpen);
                    this.buyText.text = AnimalSettings.Instance[kind].registCost.ToString();
                    this.selectBtn.onClick.RemoveAllListeners();
                    this.selectBtn.onClick.AddListener(() =>
                    {
                        SeManager.Instance.Play(Sound.SeKind.botton_main.ToString(), 0.0f, false, 1.0f);
                        this.selectAnimal = (AnimalKind)int.Parse(tgl.AnimalKind.text);
                        // アニマルを確定
                        PlayerDataManager.PlayerData.ChangeSelectedAnimalCar(this.selectAnimal);
                        this.selectedImg.gameObject.SetActive(true);

                        // チェックアイコン表示更新
                        foreach (AnimalKind a in Enum.GetValues(typeof(AnimalKind)))
                        {
                            this.animaltgls[a].SelectIcon.gameObject.SetActive(true);

                            if (PlayerDataManager.PlayerData.ReleasedAnimals.Contains(a.ToString()))
                            {
                                this.animaltgls[a].SelectIcon.gameObject.SetActive(false);
                            }
                            else
                            {
                                this.animaltgls[a].SelectIcon.sprite = this.selectIcon[0];
                            }
                        }

                        tgl.SelectIcon.sprite = this.selectIcon[1];
                        tgl.SelectIcon.gameObject.SetActive(true);
                    });
                }
            });

            if (this.selectAnimal == kind)
            {
                tgl.AnimalTgl.isOn = true;
                tgl.SelectIcon.sprite = this.selectIcon[1];
                tgl.SelectIcon.gameObject.SetActive(true);
            }

            tgl.AnimalTgl.group = this.AnimalContent.GetComponent<ToggleGroup>();
        }

        SetStar();
    }

    private GameObject GetPrefab(string fileName, Transform content)
    {
        var prefab = Instantiate(Resources.Load(fileName)) as GameObject;
        prefab.transform.SetParent(content);
        prefab.transform.localPosition = Vector3.zero;
        prefab.transform.localScale = Vector3.one;
        return prefab;
    }

    void SetStar()
    {
        starText.text = string.Format("所持★：{0}", PlayerDataManager.PlayerData.StarNum);
    }


    void SetDetail()
    {
        var param = AnimalSettings.Instance[this.animalKind];
        monitorSelected.gameObject.SetActive(true);
        monitorSelected.Clear();
        {
            var prefab = Resources.Load<GameObject>(string.Format("Models/Animal/{0}", param.model));
            var go = GameObject.Instantiate<GameObject>(prefab);
            go.transform.SetParent(monitorSelected.MonitorRoot.transform, false);
            go.transform.localPosition = new Vector3(0f, 0f, -7f);
            go.transform.localEulerAngles = new Vector3(0f, 210f, 0f);
            // 影
            var shadow = Instantiate<GameObject>(Resources.Load<GameObject>("Models/Animal/cha_shadow"), go.transform);
            shadow.transform.localPosition += new Vector3(0, 0.025f, 0);
            // 走行アニメ
            var animator = go.GetComponentInChildren<Animator>();
            animator.Play("run");
        }
        {
            // 道
            var root = new GameObject("Roads");
            root.transform.SetParent(monitorSelected.MonitorRoot.transform);
            root.transform.localPosition = new Vector3(0, 0, -7);
            root.transform.localEulerAngles = new Vector3(0f, 210f, 0f);
            var prefab = Resources.Load<GameObject>("Models/Background/Road");

            for (int i = 0; i < 3; ++i)
            {
                var go = GameObject.Instantiate<GameObject>(prefab, root.transform);
                go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                go.transform.localPosition = new Vector3(0f, -0.4f, 6.4f * 0.4f * (i - 1));
                this.roadMaterial.Add(go.GetComponent<Renderer>().material);
            }

            // アニマルカーの速度から1秒あたりの道のスクロール速度を決める
            var animalspeed  = (param.topSpeed * 1000) / 3600.0f;
            this.scrollSpeed = animalspeed / (6.4f * 0.4f);
            this.offset      = 0.0f;
        }
        name.text   = param.name;
        desc.Text   = param.desc;
        strong.text = param.strong;
        weak.text   = param.weak;
    }


    void Update()
    {
        this.offset += Time.deltaTime * this.scrollSpeed;

        // UVスクロール
        foreach (var mat in this.roadMaterial)
        {
            var uv = new Vector2(0, offset);
            mat.SetTextureOffset("_MainTex", uv);
        }
    }
}
}
