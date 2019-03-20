using System.IO;
using System.Collections.Generic;
using UnityEngine;


// SOURCE http://wannabe-jellyfish.hatenablog.com/entry/2017/03/04/215206
public class CsvReader
{
    static public List<string[]> Read(string filepath, char delim = '\t')
    {
        // Assets/Resources配下のファイルを読み込む
        TextAsset csvFile = Resources.Load(filepath) as TextAsset;
        // StringReaderで一行ずつ読み込んで、区切り文字で分割
        List<string[]> data = new List<string[]>();
        StringReader sr = new StringReader(csvFile.text);

        while (sr.Peek() > -1)
        {
            string line = sr.ReadLine();
            data.Add(line.Split(delim));
        }

        return data;
    }
}
