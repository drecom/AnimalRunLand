using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class DialogWindow : Tsl.UI.Window
{
    public Text TitleTxt;
    public Text Title2Txt;
    public Text BtnMessage;
    public Tsl.UI.AnimeButton YesBtn;
    public Tsl.UI.AnimeButton NoBtn;
    public Tsl.UI.AnimeButton CenterBtn;
    public Tsl.UI.AnimeButton CloseBtn;
    public Transform BtnPanel;
    public Transform NoBtnPanel;
    public CustomUI.UIText Message;

    protected override void OnStart()
    {
        CloseBtn.OnClick = () =>
        {
            this.GetPanel().gameObject.SetActive(false);
            this.Close();
        };
    }
    public void SetDialog(string title, string title2, string msg, Action center = null, Action yes = null, Action no = null)
    {
        this.TitleTxt.text = title;
        this.Title2Txt.text = title2;

        if (center != null || yes != null || no != null)
        {
            // ボタンありレイアウト
            this.BtnMessage.text = msg;
            this.NoBtnPanel.gameObject.SetActive(false);
            this.BtnPanel.gameObject.SetActive(true);

            if (center == null) { SetYesNoButton(yes, no); }
            else { SetCenterButton(center); }
        }
        else
        {
            // ボタン無しレイアウト
            this.Message.Text = msg;
            this.BtnPanel.gameObject.SetActive(false);
            this.NoBtnPanel.gameObject.SetActive(true);
        }
    }

    // Closeボタンの有無
    public void EnableCloseButton(bool enable)
    {
        // NOTE ボタンの有無でCloseボタンのOn/Offを決める
        //this.CloseBtn.gameObject.SetActive(enable);
    }

    private void SetYesNoButton(Action yes, Action no)
    {
        this.CenterBtn.gameObject.SetActive(false);
        this.YesBtn.OnClick = yes;
        this.NoBtn.OnClick = no;
        this.YesBtn.gameObject.SetActive(true);
        this.NoBtn.gameObject.SetActive(true);
        this.CloseBtn.gameObject.SetActive(false);
    }
    private void SetCenterButton(Action act)
    {
        this.YesBtn.gameObject.SetActive(false);
        this.NoBtn.gameObject.SetActive(false);
        this.CenterBtn.OnClick = act;
        this.CenterBtn.gameObject.SetActive(true);
        this.CloseBtn.gameObject.SetActive(false);
    }
    public void onClose()
    {
        GetPanel().gameObject.SetActive(false);
        this.Close();
    }
}
}
