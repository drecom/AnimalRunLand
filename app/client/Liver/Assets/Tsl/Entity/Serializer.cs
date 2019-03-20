using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tsl.Entity
{
public class Serializer
{
    public static void Serialize<T>(System.IO.TextWriter stream, T data)
    {
        var writer = new LitJson.JsonWriter(stream);
        LitJson.JsonMapper.ToJson(data, writer);
    }

    public static string SerializeString<T>(T data)
    {
        return LitJson.JsonMapper.ToJson(data);
    }

    public static void Save<T>(string path, T data)
    {
        var stream = new System.IO.StreamWriter(
            path,
            false,
            new System.Text.UTF8Encoding(false)
        );
        Serialize(stream, data);
        stream.Close();
    }

    public static void Deserialize<T>(string serializedText, out T data)
    {
        data = LitJson.JsonMapper.ToObject<T>(serializedText);
    }

    public static void Load<T>(Func<string, string> loader, string filename, out T data)
    {
        Deserialize(loader(filename), out data);
    }

    public static string Dump<T>(T data)
    {
        return LitJson.JsonMapper.ToJson(data);
    }
}
}
