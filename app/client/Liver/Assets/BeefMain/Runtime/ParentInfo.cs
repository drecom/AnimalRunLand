using UnityEngine;

namespace BeefMain.Runtime
{
/// <summary>
/// ゲームオブジェクトの経緯度座標(東経 x, 北緯 y)とスケール情報を保持する.
/// </summary>
public class ParentInfo : MonoBehaviour
{
    /// <summary>
    /// このゲームオブジェクトの経緯度座標(東経 x, 北緯 y)
    /// </summary>
    public Vector2 WorldCenter = Vector2.zero;
    /// <summary>
    /// このゲームオブジェクト以下の経緯度をスケールする値
    /// </summary>
    public Vector2 WorldScale = Vector2.one;

    /// <summary>
    /// ParentInfo を持つゲームオブジェクトを探す.
    /// </summary>
    /// <returns>parent info. or null.</returns>
    /// <param name="gameObjectName">Game object name.</param>
    public static ParentInfo GetParentInfo(string gameObjectName)
    {
        var gameObject = GameObject.Find(gameObjectName);

        if (gameObject == null)
        {
            return null;
        }

        var parentInfo = gameObject.GetComponent<ParentInfo>();

        if (parentInfo == null)
        {
            return null;
        }

        return parentInfo;
    }

    /// <summary>
    /// ParentInfo を持つゲームオブジェクトを探す.
    /// なかった場合は ParentInfo を持つゲームオブジェクトを作り、WorldCenter, WorldScale を設定する.
    /// </summary>
    /// <returns>parent info.</returns>
    /// <param name="gameObjectName">Game object name.</param>
    /// <param name="worldCenter">このゲームオブジェクトの経緯度座標(東経 x, 北緯 y). すでに ParentInfo が存在した場合、上書きしない.</param>
    /// <param name="worldScale">このゲームオブジェクト以下の経緯度をスケールする値. すでに ParentInfo が存在した場合、上書きしない.</param>
    public static ParentInfo GetOrCreateParentInfo(string gameObjectName, Vector2 worldCenter, Vector2 worldScale)
    {
        var gameObject = GameObject.Find(gameObjectName);

        if (gameObject == null)
        {
            gameObject = new GameObject(gameObjectName);
        }

        var parentInfo = gameObject.GetComponent<ParentInfo>();

        if (parentInfo == null)
        {
            parentInfo = gameObject.AddComponent<ParentInfo>();
            parentInfo.WorldCenter = worldCenter;
            parentInfo.WorldScale = worldScale;
        }

        return parentInfo;
    }

}
}
