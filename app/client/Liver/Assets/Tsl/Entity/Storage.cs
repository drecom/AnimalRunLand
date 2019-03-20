using System;
using UnityEngine;

namespace Tsl.Entity
{
class Storage
{
    private static bool IsPcPlatform()
    {
        switch (UnityEngine.Application.platform)
        {
            case UnityEngine.RuntimePlatform.WindowsPlayer:
            case UnityEngine.RuntimePlatform.WindowsEditor:
            case UnityEngine.RuntimePlatform.OSXEditor:
            case UnityEngine.RuntimePlatform.OSXPlayer:
                return true;

            default:
                return false;
        }
    }

    public static bool Load<T>(string keyOrFilename, out T data)
    where T : new ()
    {
        string saveData = string.Empty;

        if (IsPcPlatform())
        {
            try
            {
                using (var file = new System.IO.StreamReader(GetDirectoryName() + keyOrFilename + GetFileExtension()))
                {
                    saveData = file.ReadToEnd();
                }
            }
            catch (System.IO.FileNotFoundException)
            {
                data = new T();
                return false;
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                data = new T();
                return false;
            }
            catch (System.IO.IsolatedStorage.IsolatedStorageException)
            {
                data = new T();
                return false;
            }
        }
        else
        {
            saveData = UnityEngine.PlayerPrefs.GetString(keyOrFilename);

            if (string.IsNullOrEmpty(saveData))
            {
                data = new T();
                return false;
            }
        }

        if (string.IsNullOrEmpty(saveData) || saveData.Length < 16)
        {
            data = new T();
            return false;
        }
        else
        {
            Serializer.Deserialize(saveData, out data);
            return true;
        }
    }
    public static string SaveToString<T>(T data)
    {
        return Serializer.SerializeString(data);
    }
    public static void StringToData<T>(string saveData, out T data)
    {
        Serializer.Deserialize(saveData, out data);
    }

    public static void Save<T>(string keyOrFilename, T data)
    {
        string saveData = Serializer.SerializeString(data);

        if (IsPcPlatform())
        {
            string saveDataDirectory = GetDirectoryName();
            System.IO.Directory.CreateDirectory(saveDataDirectory);

            using (var file = new System.IO.StreamWriter(
                saveDataDirectory + keyOrFilename + GetFileExtension(),
                false,
                new System.Text.UTF8Encoding(false)
            ))
            {
                file.Write(saveData);
            }
        }
        else
        {
            UnityEngine.PlayerPrefs.SetString(keyOrFilename, saveData);
            UnityEngine.PlayerPrefs.Save();
        }
    }

    public static void DeleteAll()
    {
        if (IsPcPlatform())
        {
            System.IO.Directory.Delete(GetDirectoryName(), true);
        }
        else
        {
            UnityEngine.PlayerPrefs.DeleteAll();
        }
    }
    public static void Delete(string keyOrFilename)
    {
        if (IsPcPlatform())
        {
            System.IO.FileInfo f = new System.IO.FileInfo(GetDirectoryName() + keyOrFilename + GetFileExtension());

            if (f != null) { f.Delete(); }
        }
        else
        {
            UnityEngine.PlayerPrefs.DeleteKey(keyOrFilename);
        }
    }

    private static string GetDirectoryName()
    {
        return "./savedata/";
    }

    private static string GetFileExtension()
    {
        return ".json";
    }
}
}
