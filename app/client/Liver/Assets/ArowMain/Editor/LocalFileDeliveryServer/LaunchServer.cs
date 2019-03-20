using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace ArowMain.Editor.LocalFileDeliveryServer
{
public static class LaunchServer
{
    public const string kLocalServerMenu = "Arow/Local Server";

    private const string kLocalServerMenu_LaunchServer = kLocalServerMenu + "/Launch Server";
    private const string kLocalServerMenu_DeleteEditorPrefsForLaunchServer = kLocalServerMenu + "/DeleteEditorPrefsForLaunchServer";

    private const string kEditorPrefsLocalServerUrlKey = "Arow_Local_File_Server_Url";
    private const string kEditorPrefsLocalServerPidKey = "Arow_Local_File_Server_Pid";
    public const string kEditorPrefsLocalServerPortKey = "Arow_Local_File_Server_Port";
    public const string kEditorPrefsLocalServerRootPathKey = "Arow_Local_File_Server_RootPath";

    /// <summary>
    /// ローカルサーバを利用するかをUnityのメニューから設定する。
    /// </summary>
    [MenuItem(kLocalServerMenu_LaunchServer, false)]
    public static void ToggleLocalServer()
    {
        var port = EditorPrefs.GetInt(kEditorPrefsLocalServerPortKey, 18080);
        var serverRootPath = EditorPrefs.GetString(kEditorPrefsLocalServerRootPathKey, "MapData");
        ToggleRunLocalServer(port, serverRootPath);
    }

    [MenuItem(kLocalServerMenu_LaunchServer, true)]
    public static bool ToggleUseLocalServerValidate()
    {
        Menu.SetChecked(kLocalServerMenu_LaunchServer, IsRunningProcess());
        return true;
    }

    /// <summary>
    /// LaunchServer で使っている EditorPrefs の設定を削除する。
    /// </summary>
    [MenuItem(kLocalServerMenu_DeleteEditorPrefsForLaunchServer, false)]
    public static void DeleteEditorPrefsForLaunchServer()
    {
        EditorPrefs.DeleteKey(kEditorPrefsLocalServerUrlKey);
        EditorPrefs.DeleteKey(kEditorPrefsLocalServerPidKey);
        EditorPrefs.DeleteKey(kEditorPrefsLocalServerPortKey);
        EditorPrefs.DeleteKey(kEditorPrefsLocalServerRootPathKey);
    }

    /// <summary>
    /// 主に起動時、前回のローカルサーバの設定を削除する。
    /// </summary>
    [InitializeOnLoadMethod]
    public static void RefreshLocalServerProcesses()
    {
        DeleteEditorPrefsForLaunchServer();
    }

    /// <summary>
    /// ファイル配信サーバの起動をトグルする。
    /// </summary>
    /// <param name="port">ポート</param>
    /// <param name="serverRootPath">配信するディレクトリパス</param>
    /// <returns>true : サーバが起動している</returns>
    public static bool ToggleRunLocalServer(int port, string serverRootPath)
    {
        string pidKey = kEditorPrefsLocalServerPidKey;
        int serverPID = EditorPrefs.GetInt(pidKey, 0);

        if (!IsRunningProcess(serverPID))
        {
            var fullPath = Path.GetFullPath(serverRootPath);
            RunServer(port, fullPath, ref serverPID);
        }
        else
        {
            KillRunningServer(ref serverPID);
        }

        EditorPrefs.SetInt(pidKey, serverPID);
        return IsRunningProcess(serverPID);
    }

    /// <summary>
    /// 渡されたプロセスIDが実行中か調べる。
    /// </summary>
    /// <param name="ServerPID">プロセスID</param>
    /// <returns>true : 実行中</returns>
    public static bool IsRunningProcess(int ServerPID)
    {
        if (ServerPID == 0)
        {
            return false;
        }

        try
        {
            var process = Process.GetProcessById(ServerPID);
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsRunningProcess()
    {
        string pidKey = kEditorPrefsLocalServerPidKey;
        int serverPID = EditorPrefs.GetInt(pidKey, 0);
        bool isRunning = IsRunningProcess(serverPID);
        return isRunning;
    }

    /// <summary>
    /// ファイル配信サーバを起動する。
    /// </summary>
    /// <param name="port">ポート</param>
    /// <param name="serverRootPath">配信するディレクトリパス</param>
    /// <param name="serverPID">起動したサーバのプロセスID</param>
    /// <returns>true : サーバが正常に起動した</returns>
    public static bool RunServer(int port, string serverRootPath, ref int serverPID)
    {
        if (!Directory.Exists(serverRootPath))
        {
            UnityEngine.Debug.Log(" Unable Start Server process");
            serverPID = 0;
            return false;
        }

        string pathToAssetServer = Path.GetFullPath("Assets/ArowMain/Editor/LocalFileDeliveryServer/MonoFileDeliveryServer.exe");
        KillRunningServer(ref serverPID); // 安全のために終了しておく。
        string args = string.Format("-Path \"{0}\" -ParentProcessId {1} -Port {2}", serverRootPath, Process.GetCurrentProcess().Id, port);
        UnityEngine.Debug.Log(" args: " + args);
        ProcessStartInfo startInfo = ExecuteInternalMono.GetProfileStartInfoForMono(MonoInstallationFinder.GetMonoInstallation("MonoBleedingEdge"), "4.5", pathToAssetServer, args, true);
        startInfo.WorkingDirectory = serverRootPath;
        startInfo.UseShellExecute = false;
        Process launchProcess = Process.Start(startInfo);

        if (launchProcess == null || launchProcess.HasExited == true || launchProcess.Id == 0)
        {
            //Unable to start process
            UnityEngine.Debug.Log(" Unable Start Server process");
            serverPID = 0;
            return false;
        }
        else
        {
            //We seem to have launched, let's save the PID
            string urlKey = kEditorPrefsLocalServerUrlKey;
            string serverUrl = GetServerURL(port);
            EditorPrefs.SetString(urlKey, serverUrl);
            UnityEngine.Debug.Log(" Start Server: " + serverUrl);
            launchProcess.EnableRaisingEvents = true;
            launchProcess.Exited += delegate(object sender, EventArgs eventArgs)
            {
                UnityEngine.Debug.Log(string.Format("サーバが終了しました: {0}", serverRootPath));
            };
            serverPID = launchProcess.Id;
            return true;
        }
    }

    /// <summary>
    /// ファイル配信サーバを停止する。
    /// </summary>
    /// <param name="serverPID"></param>
    private static void KillRunningServer(ref int serverPID)
    {
        // Kill the last time we ran
        try
        {
            if (serverPID == 0)
            {
                return;
            }

            var lastProcess = Process.GetProcessById(serverPID);
            lastProcess.Kill();
            serverPID = 0;
        }
        catch
        {
        }
    }

    private static string GetServerURL(int port)
    {
        UriBuilder downloadUrl = new UriBuilder()
        {
            Host = "localhost",
            Port = port
        };
        return downloadUrl.ToString();
    }
}
}