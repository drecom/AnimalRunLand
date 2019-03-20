using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArowMain.Editor
{
public static class OpenDirectoryMenuItems
{
    private const string kOpenDirectoryMenuPath = "Arow/OpenDirectory/";
    private const int kOpenDirectoryMenuPriority = 2000;

    /// <summary>
    /// Unity標準の永続的なデータディレクトリを開く.
    /// </summary>
    [MenuItem(kOpenDirectoryMenuPath + "Unity Persistent Data Directory", false, kOpenDirectoryMenuPriority)]
    static void OpenPersistentDataDirectory()
    {
        OpenDirectory(Application.persistentDataPath);
    }

    /// <summary>
    /// Unity標準のキャッシュディレクトリを開く.
    /// </summary>
    [MenuItem(kOpenDirectoryMenuPath + "Unity Temporary Cache Directory", false, kOpenDirectoryMenuPriority + 1)]
    static void OpenTemporaryCachesDirectory()
    {
        OpenDirectory(Application.temporaryCachePath);
    }

    /// <summary>
    /// 指定されたディレクトリをウインドウで開く
    /// </summary>
    /// <param name="directoryPath"></param>
    static void OpenDirectory(string directoryPath)
    {
        if (string.IsNullOrEmpty(directoryPath))
        {
            Debug.LogError("Path is empty.");
            return;
        }

        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            Debug.Log("ディレクトリを自動生成しました : " + directoryPath);
        }

        System.Diagnostics.Process.Start(directoryPath);
    }
}
}
