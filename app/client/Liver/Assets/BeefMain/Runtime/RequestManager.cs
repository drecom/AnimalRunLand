using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;

namespace BeefMain.RuntimeUnityWebRequestManager
{
/// <summary>
/// Unity ランタイムでかんたんな通信をできるようにする。
/// </summary>
public class RequestManager : MonoBehaviour
{
    /// <summary>
    /// 通信オブジェクトとコールバックを登録し、通信を実行する。
    /// </summary>
    /// <param name="www">通信オブジェクト</param>
    /// <param name="callback">通信終了時のコールバック</param>
    public static void SetWebRequest(UnityWebRequest www, Action<UnityWebRequest> callback)
    {
        www.SendWebRequest();
        Instance.list.Add(new RequestAndActionSet
        {
            www = www,
            action = callback
        });
    }

    private static RequestManager instance = null;

    private static RequestManager Instance
    {
        get
        {
            if (instance == null)
            {
                var gameObject = new GameObject("PlayerUnityWebRequest Manager");
                instance = gameObject.AddComponent<RequestManager>();
            }

            return instance;
        }
    }

    private struct RequestAndActionSet
    {
        public UnityWebRequest www;
        public Action<UnityWebRequest> action;
    }

    private List<RequestAndActionSet> list = new List<RequestAndActionSet>();

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < list.Count;)
        {
            var www = list[i].www;
            var action = list[i].action;

            if (www.isDone)
            {
                action(www);
                list.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }
}
}
