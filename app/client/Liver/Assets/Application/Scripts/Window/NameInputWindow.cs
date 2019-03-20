using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Liver
{
public class NameInputWindow : Tsl.UI.Window
{
    public InputField NameInput;
    public InputField NewNameInput;
    public Button OkButton;
    public Transform NewGame;
    public Transform NameChange;
    public Transform ErrorChangeText;
    public Transform ErrorNewText;
    public Action onEnd;

    private bool isNew = true;
    protected override void OnStart()
    {
        var arg = this.GetArgments();
        this.isNew = arg.args[0] == "new" ? true : false;
        NameInput.text = PlayerDataManager.PlayerData.PlayerName;

        if (this.isNew) { this.NewGame.gameObject.SetActive(true); }
        else { this.NameChange.gameObject.SetActive(true); }
    }
    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "OkButton":
                var nameChk = new Liver.UserName();
                bool nameSuccess = nameChk.Validate(this.isNew ? this.NewNameInput.text : this.NameInput.text);

                if (this.isNew) { this.ErrorNewText.gameObject.SetActive(!nameSuccess); }
                else { this.ErrorChangeText.gameObject.SetActive(!nameSuccess); }

                if (!nameSuccess) { break; }

                var playerName = this.isNew ? NewNameInput.text : NameInput.text;
                PlayerDataManager.PlayerData.SetPlayerName(playerName);
                StartCoroutine(
                    Tsl.Network.GameServer.Instance.SignUpCoroutine(PlayerDataManager.PlayerData, (r) =>
                {
                    Debug.Log(string.Format("SignUp Accepted? ({0})", r.Accepted));
                }));

                if (this.isNew) { this.BaseScene.ChangeScene("Menu", 1.0f); }
                else { onClose(); }

                break;

            case "CloseButton":
                onClose();
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }
    private void onClose()
    {
        this.GetPanel().gameObject.SetActive(false);

        if (this.onEnd != null) { this.onEnd(); }

        this.Close();
    }
}
}
