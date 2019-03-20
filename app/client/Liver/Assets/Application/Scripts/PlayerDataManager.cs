using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Liver
{
public class PlayerDataManager : MonoBehaviour
{
    Entity.PlayerData _PlayerData;
    public static Entity.PlayerData PlayerData
    {
        get
        {
            return Instance._PlayerData;
        }
        set
        {
            Instance._PlayerData = value;
        }
    }

    // シングルトン実装的な
    static PlayerDataManager _Instance;
    static PlayerDataManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                var go = new GameObject(typeof(PlayerDataManager).Name);
                DontDestroyOnLoad(go);
                _Instance = go.AddComponent<PlayerDataManager>();
                _Instance._PlayerData = new Entity.PlayerData();
            }

            return _Instance;
        }
    }


    int updateCount = 0;


    static string GetFileName()
    {
        return "user_data";
    }


    static public bool Load()
    {
        bool result;

        try
        {
            result = Tsl.Entity.Storage.Load(GetFileName(), out Instance._PlayerData);
            Debug.Log("User data loaded.");
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
            // エラーの場合は新規作成
            _Instance._PlayerData = new Entity.PlayerData();
            result = false;
            Debug.Log("New user data(error.)");
        }

        return result;
    }

    // 初期化
    static public void Clear()
    {
        _Instance._PlayerData = new Entity.PlayerData();
    }


    void Update()
    {
        // カウンターをチェックして時間差で書き出す
        if (this.updateCount == 0) { return; }

        if (--this.updateCount == 0)
        {
            Tsl.Entity.Storage.Save(GetFileName(), _PlayerData);
            Debug.Log("User data saved.");
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            Debug.Log("applicationWillResignActive or onPause");
            Tsl.Entity.Storage.Save(GetFileName(), _PlayerData);
            this.updateCount = 0;
        }
        else
        {
            Debug.Log("applicationDidBecomeActive or onResume");
        }
    }


    public static void DoUpdate()
    {
        Instance.updateCount = 5;
    }
}
}