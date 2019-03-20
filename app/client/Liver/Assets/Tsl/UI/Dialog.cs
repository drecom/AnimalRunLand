using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Util.Extension;

namespace CommonWindow
{
public class Dialog : Tsl.UI.Window
{
    public enum Kind
    {
        YesNo,
        OkCancel,
        Ok,
        Auto
    }

    private enum ButtonKind
    {
        Right,
        Left,
        Center
    }

    public enum Alignment
    {
        Left, // default
        Center,
        Right
    }

    private Kind kind;
    private string message;
    private string picture;     //スプライト名で画像表示（Tsuchiya)
    private Action yesEvent;
    private Action noEvent;

    private Texture2D yesTex;
    private Texture2D noTex;
    private Texture2D okTex;
    private Texture2D canTex;
    private Alignment align = Alignment.Left;
    private string prefab = "Dialog/Dialog_Information";
    int sortOrder = 6;


    public void Make(Kind kind, string prefab, string picture, string message)
    {
        this.kind = kind;
        this.message = message;
        this.picture = picture; //  "img_face_hint" //「Resouces/DialogPic」フォルダ内のスプライト画像名（新命令第二引数）。仮配置ですが引数で取得する形でお願いします(Tsuchiya)
        this.prefab = "Dialog/" + prefab;
        //SetButtonTexture();
    }

    public void SetSortOrder(int order)
    {
        this.sortOrder = order;
    }

    public void SetAlignment(Alignment align)
    {
        this.align = align;
    }

    private void SetButtonTexture()
    {
        this.yesTex = Resources.Load("Textures/Yes") as Texture2D;
        this.noTex = Resources.Load("Textures/No") as Texture2D;
        this.okTex = Resources.Load("Textures/Ok") as Texture2D;
        this.canTex = Resources.Load("Textures/Cancel") as Texture2D;
    }
    public void SetYesEvent(Action yesEvent)
    {
        this.yesEvent = yesEvent;
    }
    public void SetNoEvent(Action noEvent)
    {
        this.noEvent = noEvent;
    }
    public void SetOkEvent(Action okEvent)
    {
        this.yesEvent = okEvent;
    }
    public void SetCancelEvent(Action cancelEvent)
    {
        this.noEvent = cancelEvent;
    }

    private UnityEngine.TextAnchor GetAlignment()
    {
        var dict = new Dictionary<Alignment, UnityEngine.TextAnchor>
        {
            { Alignment.Left, UnityEngine.TextAnchor.MiddleLeft },
            { Alignment.Center, UnityEngine.TextAnchor.MiddleCenter },
            { Alignment.Right, UnityEngine.TextAnchor.MiddleRight },
        };
        return dict.CheckedAt(this.align);
    }

    protected override void OnUpdate()
    {
        GetWindow().GetComponent<UnityEngine.Canvas>().sortingOrder = this.sortOrder;

        switch (this.kind)
        {
            case Kind.YesNo:
                SetYesNoContents();
                break;

            case Kind.OkCancel:
                SetOkCancelContents();
                break;

            case Kind.Ok:
                SetOkContents();
                break;

            case Kind.Auto:
                SetAllContents();
                break;

            default:
                throw new InvalidOperationException(string.Format("Kind {0}は指定できません", this.kind.ToString()));
        }

        var textObject = GetText("Message");
        textObject.text = this.message;
        textObject.alignment = GetAlignment();

        //画像がある場合は表示(Tsuchiya)
        if (this.picture != null)
        {
            var picImage = GetButton("PicArea").GetComponent<UnityEngine.UI.Image>();
            SetImage(picImage, Resources.Load<Texture2D>("DialogPic/" + this.picture));
        }
    }

    private void OnYesButton()
    {
        Close();

        if (this.yesEvent != null)
        {
            this.yesEvent();
        }
    }

    private void OnNoButton()
    {
        Close();

        if (this.noEvent != null)
        {
            this.noEvent();
        }
    }
    private void SetAllContents()
    {
        var cancel = GetText(GetButtonName(ButtonKind.Left) + "/TextCancel");
        var ok = GetText(GetButtonName(ButtonKind.Right) + "/TextOK");
        var okc = GetText(GetButtonName(ButtonKind.Center) + "/TextOK");
        var no = GetText(GetButtonName(ButtonKind.Left) + "/TextNo");
        var yes = GetText(GetButtonName(ButtonKind.Right) + "/TextYes");
        var yesc = GetText(GetButtonName(ButtonKind.Center) + "/TextYes");

        if (cancel != null)
        {
            cancel.gameObject.SetActive(true);
            GetButton(GetButtonName(ButtonKind.Left)).gameObject.SetActive(true);
            SetButtonEvent(GetButtonName(ButtonKind.Left), OnNoButton);
        }

        if (ok != null)
        {
            ok.gameObject.SetActive(true);
            GetButton(GetButtonName(ButtonKind.Right)).gameObject.SetActive(true);
            SetButtonEvent(GetButtonName(ButtonKind.Right), OnYesButton);
        }

        if (okc != null && GetButton(GetButtonName(ButtonKind.Right)).gameObject.activeSelf == false)
        {
            okc.gameObject.SetActive(true);
            GetButton(GetButtonName(ButtonKind.Center)).gameObject.SetActive(true);
            SetButtonEvent(GetButtonName(ButtonKind.Center), OnYesButton);
        }

        if (no != null)
        {
            no.gameObject.SetActive(true);
            GetButton(GetButtonName(ButtonKind.Left)).gameObject.SetActive(true);
            SetButtonEvent(GetButtonName(ButtonKind.Left), OnNoButton);
        }

        if (yes != null)
        {
            yes.gameObject.SetActive(true);
            GetButton(GetButtonName(ButtonKind.Right)).gameObject.SetActive(true);
            SetButtonEvent(GetButtonName(ButtonKind.Right), OnYesButton);
        }

        if (yesc != null && yes == null)
        {
            yesc.gameObject.SetActive(true);
            GetButton(GetButtonName(ButtonKind.Center)).gameObject.SetActive(true);
            SetButtonEvent(GetButtonName(ButtonKind.Center), OnYesButton);
        }
    }

    private void SetYesNoContents()
    {
        //Textureを外してテキストによるボタン表示ONOFFに変更(Tsuchiya)
        //SetButtonTexture(ButtonKind.Left, this.yesTex);
        //SetButtonTexture(ButtonKind.Right, this.noTex);
        GetButton(GetButtonName(ButtonKind.Left)).gameObject.SetActive(true);
        GetButton(GetButtonName(ButtonKind.Right)).gameObject.SetActive(true);
        GetText(GetButtonName(ButtonKind.Left) + "/TextNo").gameObject.SetActive(true);
        GetText(GetButtonName(ButtonKind.Right) + "/TextYes").gameObject.SetActive(true);
        SetButtonEvent(GetButtonName(ButtonKind.Left), OnNoButton);
        SetButtonEvent(GetButtonName(ButtonKind.Right), OnYesButton);
    }
    private void SetOkCancelContents()
    {
        //Textureを外してテキストによるボタン表示ONOFFに変更(Tsuchiya)
        //SetButtonTexture(ButtonKind.Left, this.okTex);
        //SetButtonTexture(ButtonKind.Right, this.canTex);
        GetButton(GetButtonName(ButtonKind.Left)).gameObject.SetActive(true);
        GetButton(GetButtonName(ButtonKind.Right)).gameObject.SetActive(true);
        GetText(GetButtonName(ButtonKind.Left) + "/TextCancel").gameObject.SetActive(true);
        GetText(GetButtonName(ButtonKind.Right) + "/TextOK").gameObject.SetActive(true);
        SetButtonEvent(GetButtonName(ButtonKind.Left), OnNoButton);
        SetButtonEvent(GetButtonName(ButtonKind.Right), OnYesButton);
    }
    private void SetOkContents()
    {
        //Textureを外してテキストによるボタン表示ONOFFに変更(Tsuchiya)
        //SetButtonTexture(ButtonKind.Center, this.okTex);
        GetButton(GetButtonName(ButtonKind.Center)).gameObject.SetActive(true);
        GetText(GetButtonName(ButtonKind.Center) + "/TextOK").gameObject.SetActive(true);
        SetButtonEvent(GetButtonName(ButtonKind.Center), OnYesButton);
    }
    private string GetButtonName(ButtonKind kind)
    {
        var buttonDict = new Dictionary<ButtonKind, string>
        {
            //ツリー名変更(Tsuchiya)
            { ButtonKind.Right, "PicArea/RightButton" },
            { ButtonKind.Left, "PicArea/LeftButton" },
            { ButtonKind.Center, "PicArea/CenterButton" }
        };
        return buttonDict[kind];
    }
    private void SetButtonTexture(ButtonKind kind, Texture2D btnTex)
    {
        var img = GetButton(GetButtonName(kind)).GetComponent<UnityEngine.UI.Image>();
        img.overrideSprite = Sprite.Create(btnTex, new Rect(0, 0, btnTex.width, btnTex.height), new Vector2(0, 0));
    }
}

}
