using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


public class Stream
{
    // 新しくファイルを作って書き出す
    static public void Write(string path, string text, bool append)
    {
        path = Application.persistentDataPath + "/" + path;

        try
        {
            using (var writer = new StreamWriter(path, append))
            {
                writer.Write(text);
                writer.Flush();
                //writer.Close();
                Debug.Log("Write text to " + path);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    static public string Read(string path)
    {
        path = Application.persistentDataPath + "/" + path;

        try
        {
            using (var reader = new StreamReader(path))
            {
                var text = reader.ReadToEnd();
                //reader.Close();
                Debug.Log("Read text from " + path);
                return text;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        return null;
    }

    // ファイル削除
    static public void Delete(string path)
    {
        path = Application.persistentDataPath + "/" + path;

        try
        {
            File.Delete(path);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }
}
