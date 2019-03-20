using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor
{
public static class LiverBuildScripts
{
    private static string[] scenes = new string[]
    {
        "Assets/Application/Scenes/Title.unity",
        "Assets/Application/Scenes/Menu.unity",
        "Assets/Application/Scenes/Race.unity",
    };

    private class PreBuildArgumentsAndroid
    {
        public string bundleVersion = "";
        public string bundleVersionCode = "";
        public string keystoreName = "";
        public string keystorePass = "";
        public string keyaliasName = "";
        public string keyaliasPass = "";
    }

    private const string ApplicationIdentifier = "jp.co.drecom.ARL";
    private const string CompanyName = "ドリコム";
    private const string ProductName = "アニマルラン";


    public static void PreBuildSettingsAndroid()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        var parameter = CommandArgumentsParser.Parse<PreBuildArgumentsAndroid>(Environment.GetCommandLineArgs());
        PlayerSettings.productName = ProductName;
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.applicationIdentifier = ApplicationIdentifier;
        PlayerSettings.bundleVersion = parameter.bundleVersion;
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.Android.bundleVersionCode = int.Parse(parameter.bundleVersionCode);
        PlayerSettings.Android.keystoreName = parameter.keystoreName;
        PlayerSettings.Android.keystorePass = parameter.keystorePass;
        PlayerSettings.Android.keyaliasName = parameter.keyaliasName;
        PlayerSettings.Android.keyaliasPass = parameter.keyaliasPass;
    }

    private class PreBuildArgumentsIos
    {
        public string bundleVersion = "";
        public string bundleVersionCode = "";
        public string teamId = "";
    }

    public static void PreBuildSettingsIos()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        var parameter = CommandArgumentsParser.Parse<PreBuildArgumentsIos>(Environment.GetCommandLineArgs());
        PlayerSettings.productName = ProductName;
        PlayerSettings.companyName = CompanyName;
        PlayerSettings.applicationIdentifier = ApplicationIdentifier;
        PlayerSettings.bundleVersion = parameter.bundleVersion;
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.iOS.buildNumber = parameter.bundleVersionCode;
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = parameter.teamId;
    }

    public static void ReleaseBuildAndroid()
    {
        PreBuildSettingsAndroid();
        var buildPath = "build";
        Directory.CreateDirectory(buildPath);
        var androidProject = Path.Combine(buildPath, "android");
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.androidBuildType = AndroidBuildType.Release;
        BuildOptions buildOptions = BuildOptions.None;
        buildOptions |= BuildOptions.Il2CPP;
        buildOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
        var errorMessage = BuildPipeline.BuildPlayer(scenes, androidProject, BuildTarget.Android, buildOptions);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Console.WriteLine("[Liver Build Error!] " + errorMessage);
            bool batching = UnityEditorInternal.InternalEditorUtility.inBatchMode;

            if (batching)
            {
                EditorApplication.Exit(1);
            }

            return;
        }
    }

    public static void ReleaseBuildIos()
    {
        PreBuildSettingsIos();
        var buildPath = "build";
        Directory.CreateDirectory(buildPath);
        var iosProject = Path.Combine(buildPath, "ios");
        EditorUserBuildSettings.iOSBuildConfigType = iOSBuildType.Release;
        BuildOptions buildOptions = BuildOptions.None;
        buildOptions |= BuildOptions.Il2CPP;
        buildOptions |= BuildOptions.AcceptExternalModificationsToPlayer;
        var errorMessage = BuildPipeline.BuildPlayer(scenes, iosProject, BuildTarget.iOS, buildOptions);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Console.WriteLine("[Liver Build Error!] " + errorMessage);
            bool batching = UnityEditorInternal.InternalEditorUtility.inBatchMode;

            if (batching)
            {
                EditorApplication.Exit(1);
            }

            return;
        }
    }
}


/// <summary>
/// バッチモードのコマンドライン引数をパースする。
/// </summary>
public static class CommandArgumentsParser
{
    /// <summary>
    /// 取得したい型に合わせてコマンドライン引数をパースする。
    /// 取得したい型の変数名を元にパースを実行する。
    /// "〜 -key value -key2 value" のようなコマンドライン引数に対応。
    /// デフォルトでは stringとbool の変数のみ対応。
    /// </summary>
    /// <param name="arguments">すべてのコマンドライン引数 Environment.GetCommandLineArgs() の結果が渡される想定</param>
    /// <typeparam name="T">取得したい型</typeparam>
    /// <returns>パース結果</returns>
    public static T Parse<T>(string[] arguments) where T : new ()
    {
        T parameter = Parse<T>(arguments, DefaultSetValue);
        return parameter;
    }

    /// <summary>
    /// 取得したい型に合わせてコマンドライン引数をパースする。
    /// 取得したい型の変数名を元にパースを実行する。
    /// "〜 -key value -key2 value" のようなコマンドライン引数に対応。
    /// </summary>
    /// <param name="arguments">すべてのコマンドライン引数 Environment.GetCommandLineArgs() の結果が渡される想定</param>
    /// <param name="callback"> カスタムパーサ 実装は DefaultSetValue を参照</param>
    /// <typeparam name="T">取得したい型</typeparam>
    /// <returns>パース結果</returns>
    public static T Parse<T>(string[] arguments, Action<T, string, string> callback) where T : new ()
    {
        T parameter = new T();

        for (int i = 0; i < arguments.Length; ++i)
        {
            string key = arguments[i];
            string value = "";

            // キーの抽出
            if (!key.StartsWith("-"))
            {
                continue;
            }

            key = Regex.Replace(key, "^\\-+", "");

            if (arguments.Length >= i + 2)
            {
                value = arguments[i + 1];
            }

            callback(parameter, key, value);
        }

        return parameter;
    }

    /// <summary>
    /// デフォルトのパーサ
    /// キーと型の変数名を照らし合わせて、インスタンスに SetValue をしていく
    /// </summary>
    /// <param name="instance">インスタンス</param>
    /// <param name="key">     キー</param>
    /// <param name="value">   バリュー</param>
    /// <typeparam name="T">   取得したい型</typeparam>
    private static void DefaultSetValue<T>(T instance, string key, string value) where T : new ()
    {
        Type type = typeof(T);
        FieldInfo field = type.GetField(key);

        if (field == null)
        {
            return;
        }

        // フィールド名と同じオプションの値を設定する
        switch (field.FieldType.Name)
        {
            case "String":
                {
                    field.SetValue(instance, value);
                    break;
                }

            // 明示的にfalseの場合にはfalse、それ以外はtrue
            case "Boolean" :
                {
                    if (value == "0" || value.ToLower() == "false")
                    {
                        field.SetValue(instance, false);
                        break;
                    }
                    else
                    {
                        field.SetValue(instance, true);
                        break;
                    }
                }
        }
    }
}
}
