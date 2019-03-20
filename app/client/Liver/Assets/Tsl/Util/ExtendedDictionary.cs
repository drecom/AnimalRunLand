using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Util.Extension
{
public static class ExtendedDictionary
{
    public static void CheckedAdd<Key, Value>(this Dictionary<Key, Value> dict, Key key, Value value)
    {
        string trace = Environment.StackTrace;

        try
        {
            dict.Add(key, value);
        }
        catch (System.ArgumentException)
        {
            Debug.LogError(string.Format("<color=red>キー「{0}」がすでに存在しています</color>\n{1}", key, trace));
            throw;
        }
    }
    public static Vector3 CheckedAt(this Dictionary<string, Vector3> dict, string key)
    {
        string trace = Environment.StackTrace;

        try
        {
            return dict[key];
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            Debug.LogError(string.Format("<color=red>キー「{0}」が見つかりません</color>\n{1}", key, trace));
            return Vector3.zero;
        }
    }

    public static Camera CheckedAt(this Dictionary<string, Camera> dict, string key)
    {
        string trace = Environment.StackTrace;

        try
        {
            return dict[key];
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            Debug.LogError(string.Format("CheckedAt:Camera:<color=red>キー「{0}」が見つかりません</color>\n{1}", key, trace));
            return null;
        }
    }
    public static Value CheckedAt<Key, Value>(this Dictionary<Key, Value> dict, Key key)
    {
        string trace = Environment.StackTrace;

        try
        {
            return dict[key];
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            Debug.LogError(string.Format("<color=red>キー「{0}」が見つかりません</color>\n{1}", key, trace));
            throw;
        }
    }

    public static string Dump<Key, Value>(this Dictionary<Key, Value> dict)
    {
        string dump = "{";
        bool first = true;

        foreach (var x in dict)
        {
            if (first)
            {
                first = false;
            }
            else
            {
                dump += ",";
            }

            dump += x.Key.ToString();
        }

        dump += "}";
        return dump;
    }
}
}
