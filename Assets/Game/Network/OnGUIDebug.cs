using Game.Network.GameRPC;
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
                    pos = UnityToolkit.MathTypes.Vector3.zero,
                    rotation = UnityToolkit.MathTypes.Quaternion.identity,
                    scale = UnityToolkit.MathTypes.Vector3.one
                };
                transformComponent.mask = TransformComponent.Mask.All;
                NetworkClientMgr.Singleton.SpawnEntity(transformComponent);
            }


            GUILayout.EndVertical();
        }

        [Sirenix.OdinInspector.Button]
        private async void Sum()
        {
            int res = await GameRPC.Singleton.gameService.SumAsync(2, 3);
            Global.Log.Info($"Sum: {res}");
        }
    }
}