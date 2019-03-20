using UnityEngine;
using System;

namespace Api.Net
{
[CreateAssetMenu(menuName = "Configs/ServerSettings")]
public class ServerSettings : ScriptableObject
{
    [SerializeField]
    ServerType serverType;
    [SerializeField]
    ServerSetting[] servers;

    static ServerSettings _Instance;
    static ServerSettings Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = Resources.Load<ServerSettings>("Configs/ServerSettings");
            }

            return _Instance;
        }
    }

    public static ServerSetting ServerSetting()
    {
        return ServerSetting(Instance.serverType);
    }

    public static ServerSetting ServerSetting(ServerType serverType)
    {
        return Array.Find(Instance.servers, server => server.ServerType == serverType);
    }
}
}
