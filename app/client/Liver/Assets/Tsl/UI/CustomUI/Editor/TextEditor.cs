using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;

namespace CustomUI
{
// TODO REVIEW
// Have material live under text
// move stencil mask into effects *make an efects top level element like there is
// paragraph and character

/// <summary>
/// Editor class used to edit UI Labels.
/// </summary>
[CustomEditor(typeof(UIText), true)]
[CanEditMultipleObjects]
public class CustomTextEditor : UnityEditor.UI.TextEditor
{
    private GUIContent topAlignText;
    private GUIContent middleAlignText;
    private GUIContent bottomAlignText;

    private GUIContent leftAlignText;
    private GUIContent centerAlignText;
    private GUIContent rightAlignText;

    // private GUIContent m_TopAlignTextActive;

    private void EditorUpdateCallback()
    {
        if (target != null)
        {
            EditorUtility.SetDirty(target);
        }
    }

    protected override void OnEnable()
    {
        EditorApplication.update += EditorUpdateCallback;
        topAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_top", "Top Align");
        middleAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_center", "Middle Align");
        bottomAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_bottom", "Bottom Align");
        leftAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_left", "Left Align");
        centerAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_center", "Center Align");
        rightAlignText = EditorGUIUtility.IconContent(@"GUISystem/align_horizontally_right", "Right Align");
        // m_TopAlignTextActive = EditorGUIUtility.IconContent(@"GUISystem/align_vertically_top_active", "Top Align");
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        EditorApplication.update -= EditorUpdateCallback;
        var uiText = target as UIText;

        if (uiText != null)
        {
            uiText.IsPlaying = false;
            uiText.SetVerticesDirty();
        }

        base.OnDisable();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var font = serializedObject.FindProperty("font");
        EditorGUILayout.PropertyField(font, new GUIContent("Font"));
        var fontSize = serializedObject.FindProperty("fontSize");
        EditorGUILayout.PropertyField(fontSize, new GUIContent("FontSize"));
        var style = serializedObject.FindProperty("style");
        EditorGUILayout.PropertyField(style, new GUIContent("Style"));
        var color = serializedObject.FindProperty("m_Color");
        EditorGUILayout.PropertyField(color, new GUIContent("Color"));
        var text = serializedObject.FindProperty("text");
        EditorGUILayout.PropertyField(text, new GUIContent("Text"));
        var horizontalLayout = serializedObject.FindProperty("horizontalLayout");
        EditorGUILayout.PropertyField(horizontalLayout, new GUIContent("HorizontalLayout"));
        var verticalLayout = serializedObject.FindProperty("verticalLayout");
        EditorGUILayout.PropertyField(verticalLayout, new GUIContent("VerticalLayout"));
        var lineSpaceing = serializedObject.FindProperty("lineSpaceing");
        EditorGUILayout.PropertyField(lineSpaceing, new GUIContent("LineSpaceing"));
        var justfit = serializedObject.FindProperty("justfit");
        EditorGUILayout.PropertyField(justfit, new GUIContent("Justfit"));

        if (justfit.boolValue)
        {
            EditorGUILayout.HelpBox("JustFitを有効にする文字間隔の大きさ\n指定したpxサイズ以上の文字間隔の場合JustFitを行う", MessageType.None);
            var fitEnabledCharacterInterval = serializedObject.FindProperty("fitEnabledCharacterInterval");
            EditorGUILayout.PropertyField(fitEnabledCharacterInterval, new GUIContent("FitEnabledCharacterInterval"));
        }

        var raycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        EditorGUILayout.PropertyField(raycastTarget, new GUIContent("RaycastTarget"));
        EditorGUILayout.HelpBox("アルファベットの自動改行をしない文字数\n0の場合連続したアルファベットを１行に収める", MessageType.None);
        var alphabetWarpLimit = serializedObject.FindProperty("alphabetWarpLimit");
        EditorGUILayout.PropertyField(alphabetWarpLimit, new GUIContent("alphabetWarpLimit"));
        AnchorSettings();
        ShowDisplayMode();
        serializedObject.ApplyModifiedProperties();
    }

    private void AnchorSettings()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            HorizontalAnchorSetting();
            EditorGUILayout.Space();
            VerticalAnchorSetting();
        }

        EditorGUILayout.Space();
    }

    private void HorizontalAnchorSetting()
    {
        var horizontalAlignment = serializedObject.FindProperty("horizontalAlignment");
        var horizontalAlignmentEnum = (UIText.Alignment.Horizontal)horizontalAlignment.enumValueIndex;
        var left = GUILayout.Toggle(horizontalAlignmentEnum == UIText.Alignment.Horizontal.Left, leftAlignText, EditorStyles.miniButtonLeft);
        var center = GUILayout.Toggle(horizontalAlignmentEnum == UIText.Alignment.Horizontal.Center, centerAlignText, EditorStyles.miniButtonMid);
        var right = GUILayout.Toggle(horizontalAlignmentEnum == UIText.Alignment.Horizontal.Right, rightAlignText, EditorStyles.miniButtonRight);

        if (horizontalAlignmentEnum == UIText.Alignment.Horizontal.Left && center)
        {
            horizontalAlignment.enumValueIndex = 1;
        }
        else if (horizontalAlignmentEnum == UIText.Alignment.Horizontal.Left && right)
        {
            horizontalAlignment.enumValueIndex = 2;
        }
        else if (horizontalAlignmentEnum == UIText.Alignment.Horizontal.Center && left)
        {
            horizontalAlignment.enumValueIndex = 0;
        }
        else if (horizontalAlignmentEnum == UIText.Alignment.Horizontal.Center && right)
        {
            horizontalAlignment.enumValueIndex = 2;
        }
        else if (horizontalAlignmentEnum == UIText.Alignment.Horizontal.Right && left)
        {
            horizontalAlignment.enumValueIndex = 0;
        }
        else if (horizontalAlignmentEnum == UIText.Alignment.Horizontal.Right && center)
        {
            horizontalAlignment.enumValueIndex = 1;
        }
    }

    private void VerticalAnchorSetting()
    {
        var verticalAlignment = serializedObject.FindProperty("verticalAlignment");
        var varticalAlignmentEnum = (UIText.Alignment.Vertical)verticalAlignment.enumValueIndex;
        var top = GUILayout.Toggle(varticalAlignmentEnum == UIText.Alignment.Vertical.Top, topAlignText, EditorStyles.miniButtonLeft);
        var middle = GUILayout.Toggle(varticalAlignmentEnum == UIText.Alignment.Vertical.Middle, middleAlignText, EditorStyles.miniButtonMid);
        var bottom = GUILayout.Toggle(varticalAlignmentEnum == UIText.Alignment.Vertical.Bottom, bottomAlignText, EditorStyles.miniButtonRight);

        if (varticalAlignmentEnum == UIText.Alignment.Vertical.Top && middle)
        {
            verticalAlignment.enumValueIndex = 1;
        }
        else if (varticalAlignmentEnum == UIText.Alignment.Vertical.Top && bottom)
        {
            verticalAlignment.enumValueIndex = 2;
        }
        else if (varticalAlignmentEnum == UIText.Alignment.Vertical.Middle && top)
        {
            verticalAlignment.enumValueIndex = 0;
        }
        else if (varticalAlignmentEnum == UIText.Alignment.Vertical.Middle && bottom)
        {
            verticalAlignment.enumValueIndex = 2;
        }
        else if (varticalAlignmentEnum == UIText.Alignment.Vertical.Bottom && top)
        {
            verticalAlignment.enumValueIndex = 0;
        }
        else if (varticalAlignmentEnum == UIText.Alignment.Vertical.Bottom && middle)
        {
            verticalAlignment.enumValueIndex = 1;
        }
    }

    private void ShowDisplayMode()
    {
        EditorGUILayout.HelpBox(@"文字の表示の仕方を指定する
Flush : Textの文字を即座に反映
Teletype : 1文字づつ表示をする", MessageType.None);
        var showModeProperty = serializedObject.FindProperty("displayType");
        EditorGUILayout.PropertyField(showModeProperty, new GUIContent("DisplayMode"));
        UIText.DisplayType showMode = (UIText.DisplayType)showModeProperty.enumValueIndex;

        if (showMode == UIText.DisplayType.Teletype)
        {
            var teletypeText = serializedObject.FindProperty("teletypeText");
            EditorGUILayout.LabelField("Teletype");
            EditorGUI.indentLevel += 1;
            var teletypeSpeed = teletypeText.FindPropertyRelative("speed");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(teletypeSpeed, new GUIContent("Speed"));
                var uiText = target as UIText;
                bool playing = uiText.IsPlaying;
                uiText.IsPlaying = GUILayout.Toggle(playing, "Simurate", EditorStyles.miniButton);

                if (uiText.IsPlaying != playing)
                {
                    if (uiText.IsPlaying)
                    {
                        uiText.ResetTextRange();
                    }
                    else
                    {
                        // falseに普通に変更すると描画処理が走らないので
                        // 再生成を外から実行するように指定
                        uiText.SetVerticesDirty();
                    }
                }
            }

            var teletypeFinishEvent = teletypeText.FindPropertyRelative("onFinished");
            EditorGUILayout.PropertyField(teletypeFinishEvent, new GUIContent("OnFinished"));
            EditorGUI.indentLevel -= 1;
        }
    }
}
}
