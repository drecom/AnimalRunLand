using UnityEditor;
using UnityEngine.Networking;
using System;

namespace BeefMain.EditorUnityWebRequestManager
{
/// <summary>
/// Unity Editorでかんたんな通信をできるようにする。
/// </summary>
public class RequestManager
{
    /// <summary>
    /// 通信オブジェクトとコールバックを登録し、通信を実行する。
    /// </summary>
    /// <param name="www">通信オブジェクト</param>
    /// <param name="callback">通信終了時のコールバック</param>
    public static void SetWebRequest(UnityWebRequest www, Action<UnityWebRequest> callback)
    {
        www.SendWebRequest();
        EditorApplication.CallbackFunction editorUpdate = null;
        editorUpdate = () =>
        {
            //毎フレームチェック
            if (www.isDone)
            {
                callback(www);
                EditorApplication.update -= editorUpdate;
            }
        };
        EditorApplication.update += editorUpdate;
    }

    /// <summary>
    /// Unity Editor で通信できることを確認するテスト
    /// </summary>
    [MenuItem("BeefTest/EditorUnityWebRequestManager/EditorUnityWebRequestManagerSample")]
    private static void EditorUnityWebRequestManagerSample()
    {
        UnityWebRequest unityWebRequest = UnityWebRequest.Get("http://example.com/");
        RequestManager.SetWebRequest(unityWebRequest, (www) =>
        {
            if (!string.IsNullOrEmpty(www.error))
            {
                EditorUtility.DisplayDialog("EditorUnityWebRequestManagerSample Error", www.error, "OK", "");
            }
            else
            {
                EditorUtility.DisplayDialog("EditorUnityWebRequestManagerSample Success", www.downloadHandler.text, "OK", "");
            }
        });
    }
}
}
