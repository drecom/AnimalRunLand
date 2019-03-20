using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// NOTE LIT JSONの初期化
//      アプリ開始時に呼び出して、JSONで型を使えるようにしておく
public class RegisterJsonType
{
    static readonly DateTime UNIX_EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [RuntimeInitializeOnLoadMethod]
    public static void Initialize()
    {
        LitJson.JsonMapper.RegisterExporter<DateTime>(
            (dt, writer) => writer.Write((int)(dt - UNIX_EPOCH).TotalSeconds));
        LitJson.JsonMapper.RegisterImporter<int, DateTime>(
            input => UNIX_EPOCH.AddSeconds(input).ToLocalTime());
        LitJson.JsonMapper.RegisterImporter<long, DateTime>(
            input => UNIX_EPOCH.AddSeconds(input).ToLocalTime());
        LitJson.JsonMapper.RegisterExporter<float>(
            (f, writer) => writer.Write((double)f));
        LitJson.JsonMapper.RegisterImporter<double, float>(
            input => (float)input);
        LitJson.JsonMapper.RegisterExporter<Vector2>(
            (f, writer) =>
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");
            writer.Write((double)f.x);
            writer.WritePropertyName("y");
            writer.Write((double)f.y);
            writer.WriteObjectEnd();
        });
        LitJson.JsonMapper.RegisterImporter<double[], Vector3>(
            input => new Vector3((float)input[0], (float)input[1], (float)input[3]));
        LitJson.JsonMapper.RegisterExporter<Vector3>(
            (f, writer) =>
        {
            writer.WriteArrayStart();
            writer.Write((double)f.x);
            writer.Write((double)f.y);
            writer.Write((double)f.z);
            writer.WriteArrayEnd();
        });
    }

}



