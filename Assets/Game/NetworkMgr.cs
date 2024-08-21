using System;
using System.Threading.Tasks;
using GameCore;
using Network;
using Network.Client;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityToolkit;

namespace Game
{
    public class NetworkMgr : MonoBehaviour
    {
        public IClientSocket socket;
        public NetworkClient client;
        public string host = "localhost";
        public int port = 8080;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            ToolkitLog.logAction = Debug.Log;
            ToolkitLog.warningAction = Debug.LogWarning;
            ToolkitLog.errorAction = Debug.LogError;
        }

        private void Awake()
        {
        }

        private void OnDestroy()
        {
            Dispose();
        }

        [Button]
        private void ReqCreateNetworkIdentity()
        {
            // TransformComponent transformComponent = new TransformComponent();
            // NetworkIdentityCreateReq req = new NetworkIdentityCreateReq();
            // req.components.Add(new NetworkComponentPacket()
            // {
            //     idx = 0,
            //     type =
            // });
            // client.Send(req);
        }

        [Button]
        private void Run()
        {
            if (client is { Cts: not null })
            {
                Debug.LogError("Client is already running");
                return;
            }

            socket = new TelepathyClientSocket();
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Host = host;
            uriBuilder.Port = port;
            client = new NetworkClient(socket, 60);
            client.AddSystem<NetworkClientTime>();
            client.Run(uriBuilder.Uri);
        }


        public void Dispose()
        {
            client.Dispose();
        }
    }
}