using UnityEngine;

namespace Liver
{
public class SettingWindow : Tsl.UI.Window
{
    public Tsl.UI.AnimeButton SoundBtn;
    public Tsl.UI.AnimeButton PrivacyBtn;
    public Tsl.UI.AnimeButton TosBtn;
    public Tsl.UI.AnimeButton CreditBtn;

    public override void OnClickButtonEvent(Transform button)
    {
        switch (button.name)
        {
            case "StatusButton":
                BaseScene.AddModalWindow<SoundSettingWindow>("StatusWindow");
                break;

            case "AreaHistoryButton":
                BaseScene.AddModalWindow<SoundSettingWindow>("AreaHistoryWindow");
                break;

            case "PrivacyPolicyButton":
                // ブラウザ起動
                Application.OpenURL(Entity.AppSettings.Instance.PrivacyPolicyURL);
                break;

            case "TermsOfUseButton":
                // ブラウザ起動
                Application.OpenURL(Entity.AppSettings.Instance.TermsOfUseURL);
                break;

            case "SoundButton":
                BaseScene.AddModalWindow<SoundSettingWindow>("SoundSettingWindow");
                break;

            case "CreditButton":
                BaseScene.AddModalWindow<CreditsWindow>("CreditsWindow");
                break;

            default:
                base.OnClickButtonEvent(button);
                break;
        }
    }
}
}
