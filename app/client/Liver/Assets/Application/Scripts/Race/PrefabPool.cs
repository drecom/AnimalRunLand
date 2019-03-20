using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class PrefabPool
{
    static Dictionary<GameObject, Stack<GameObject>> pools = new Dictionary<GameObject, Stack<GameObject>>();
    public static GameObject Pop(GameObject prefab, Action<GameObject> OnInstantiate = null)
    {
        Stack<GameObject> stack;

        if (!pools.TryGetValue(prefab, out stack))
        {
            stack = new Stack<GameObject>();
            pools[prefab] = stack;
        }

        while (stack.Count > 0 && stack.Peek() == null)
        {
            stack.Pop();    // null の場合要らない
        }

        if (stack.Count <= 0)
        {
            var res = GameObject.Instantiate<GameObject>(prefab);

            if (OnInstantiate != null) { OnInstantiate(res); }  // カスタム可能のポイントを提供する

            return res;
        }
        else
        {
            return stack.Pop();
        }
    }
    public static void Push(GameObject prefab, GameObject obj)
    {
        if (obj == null) { return; }

        Stack<GameObject> stack;

        if (!pools.TryGetValue(prefab, out stack))
        {
            stack = new Stack<GameObject>();
            pools[prefab] = stack;
        }

        stack.Push(obj);
    }
}
