using Network;
using UnityEngine;

namespace Game
{
    public class OnGUIDebug : MonoBehaviour
    {
        private void OnGUI()
        {
            GUILayout.BeginVertical();
            GUILayout.Label($"Network {NetworkMgr.Singleton.connectionId}");
            if (NetworkMgr.Singleton.client is { Cts: { IsCancellationRequested: false } })
            {
                if (GUILayout.Button("Stop"))
                {
                    NetworkMgr.Singleton.client.Stop();
                }
            }
            else
            {
                if (GUILayout.Button("Run"))
                {
                    NetworkMgr.Singleton.Run("localhost", 8080);
                }
            }
            // 生成一个自己拥有的实体
            if (GUILayout.Button("Spawn"))
            {
                TransformComponent transformComponent = new TransformComponent()
                {
                    pos = Vector3.zero,
                    rotation = Quaternion.identity,
                    scale = Vector3.one
                };
                NetworkMgr.Singleton.SpawnEntity(transformComponent);
            }
            

            GUILayout.EndVertical();
        }
    }
}