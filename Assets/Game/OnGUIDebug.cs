using Network;
using UnityEngine;

namespace Game
{
    public class OnGUIDebug : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.BeginVertical("window");
            GUILayout.Label($"Network {NetworkClientMgr.Singleton.connectionId}");
            if (NetworkClientMgr.Singleton.client is { Cts: { IsCancellationRequested: false } })
            {
                if (GUILayout.Button("Disconnect"))
                {
                    NetworkClientMgr.Singleton.client.Stop();
                }
            }
            else
            {
                if (GUILayout.Button("Connect"))
                {
                    NetworkClientMgr.Singleton.Connect("localhost", 8080);
                }
            }

            // 生成一个自己拥有的实体
            if (GUILayout.Button("Spawn"))
            {
                TransformComponent transformComponent = new TransformComponent()
                {
                    pos = Network.Vector3.zero,
                    rotation = Network.Quaternion.identity,
                    scale = Network.Vector3.one
                };
                NetworkClientMgr.Singleton.SpawnEntity(transformComponent);
            }


            GUILayout.EndVertical();
        }
    }
}