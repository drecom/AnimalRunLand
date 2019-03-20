using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class MasterdataDownloader
{
    string resourceUrl;
    string downloadBaseUrl;
    string storeBasePath;
    BulkDownloader bulkDownloader;
    Dictionary<string, ResourceInfo> resourceInfoDic;

    public MasterdataDownloader(string resourceUrl, string downloadBaseUrl, string storeBasePath)
    {
        this.resourceUrl = resourceUrl;
        this.downloadBaseUrl = downloadBaseUrl;
        this.storeBasePath = storeBasePath;
    }

    public class ResourceInfo
    {
        public string name;
        public int size;
    }

    public Dictionary<string, ResourceInfo> GetResourceInfo()
    {
        return resourceInfoDic;
    }

    public Util.Future<Util.Unit> Start(MonoBehaviour mono)
    {
        return new Util.Future<Util.Unit>(promise => StartE(mono, promise));
    }

    IEnumerator StartE(MonoBehaviour mono, Util.Promise<Util.Unit> promise)
    {
        Debug.Log(resourceUrl);

        using (var www = new WWW(resourceUrl))
        {
            yield return www;

            if (www.error != null)
            {
                throw new Exception(www.error);
            }

            resourceInfoDic = LitJson.JsonMapper.ToObject<Dictionary<string, ResourceInfo>>(www.text);
        }

        // ディレクトリが無ければ作る
        if (!Directory.Exists(storeBasePath))
        {
            Directory.CreateDirectory(storeBasePath);
#if !UNITY_EDITOR && UNITY_IOS
            UnityEngine.iOS.Device.SetNoBackupFlag(storeBasePath);
#endif
        }

        bulkDownloader = new BulkDownloader();

        foreach (var kv in resourceInfoDic)
        {
            var url = downloadBaseUrl + "/" + kv.Value.name;
            var storePath = storeBasePath + "/" + kv.Value.name;
            Debug.Log(string.Format("Register Masterdata name={0} path={1} url={2}", kv.Key, storePath, url));
            bulkDownloader.Add(url, kv.Value.size, storePath);
        }

        var future = bulkDownloader.Start(mono);

        foreach (var v in future) { yield return v; }

        promise.Result = future.Result;
        Debug.Log("Completed Masterdata Downloading");
    }
}
