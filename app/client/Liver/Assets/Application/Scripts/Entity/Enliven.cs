using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Tsl.Entity
{
// 0x(FF)[FFF]<FFF> => (SIZE)[X]<Y>
public class Enliven
{
    public int Count;
    public byte[] data;

    public static uint Pack(int x, int y, int size)
    {
        return ((uint)size << 24) | ((uint)x << 12) | ((uint)y);
    }

    public void Unpack(int index, out int x, out int y, out int size)
    {
        var start = (sizeof(uint) * index);
        var value = BitConverter.ToUInt32(data, start);
        size = (int)(value >> 24) & 0xFF;
        x = (int)(value >> 12) & 0xFFF;
        y = (int)(value) & 0xFFF;
    }

    static string GetFilename()
    {
        return "enliven";
    }

    public static Enliven Load()
    {
        var textAsset = Resources.Load<TextAsset>("Race/" + GetFilename());
        Enliven res = new Enliven();
        res.data = textAsset.bytes;
        res.Count = BitConverter.ToInt32(res.data, res.data.Length - 4);
        return res;
    }

#if UNITY_EDITOR
    public void Save()
    {
        File.WriteAllBytes($"./savedata/{GetFilename()}.bytes", data);
    }
#endif

}
}
