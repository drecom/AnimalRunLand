using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Network
{
public enum HTTPMethods
{
    Post,
    Get,
    Head,
    Put,
    Create,
    Delete,
}
public enum HTTPRequestStates
{
    Finished,
    Error,
}

public class HTTPRequest
{
    public byte[] RawData
    {
        set { request.uploadHandler = new UploadHandlerRaw(value); }
    }
    public HTTPRequestStates State = HTTPRequestStates.Error;
    public Exception Exception = null;

    UnityWebRequest request;
    Action<HTTPRequest, HTTPResponse> act;

    static readonly Dictionary<HTTPMethods, string> Methods = new Dictionary<HTTPMethods, string>
    {
        {HTTPMethods.Post,      UnityWebRequest.kHttpVerbPOST},
        {HTTPMethods.Get,       UnityWebRequest.kHttpVerbGET},
        {HTTPMethods.Head,      UnityWebRequest.kHttpVerbHEAD},
        {HTTPMethods.Put,       UnityWebRequest.kHttpVerbPUT},
        {HTTPMethods.Create,    UnityWebRequest.kHttpVerbCREATE},
        {HTTPMethods.Delete,    UnityWebRequest.kHttpVerbDELETE},
    };

    public HTTPRequest(string url, HTTPMethods method, Action<HTTPRequest, HTTPResponse> act, int timeout = 60)
    {
        this.act = act;
        this.request = new UnityWebRequest(url, Methods[method])
        {
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = timeout,
        };
    }
    public void SetHeader(string t, string s)
    {
        request.SetRequestHeader(t, s);
    }
    public IEnumerator Send()
    {
        if (request.uploadHandler == null)
        {
            request.uploadHandler = new UploadHandlerRaw(null);
        }

        // 通信開始
        yield return request.SendWebRequest();
        this.State = request.isDone ? HTTPRequestStates.Finished : HTTPRequestStates.Error;
        this.Exception = new Exception(request.error);
        act(this, new HTTPResponse { IsSuccess = string.IsNullOrEmpty(request.error), Data = request.downloadHandler.data, StatusCode = (int)request.responseCode });
    }
}
}

