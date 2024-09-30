using System;
using DebugUI;
using Game.Network.GameRPC;
using Network;
using UnityEngine;
using UnityEngine.UIElements;
using UnityToolkit;

namespace Game
{
    [DisallowMultipleComponent]
    public class GameDebugUI : DebugUIBuilderBase
    {
        protected override void Configure(IDebugUIBuilder builder)
        {
            builder.ConfigureWindowOptions(options =>
            {
                options.Title = "Debug";
                options.Draggable = true;
            });
            // Game Network Debug
            builder.AddFoldout("Network", uiBuilder =>
            {
                uiBuilder.AddButton("Disconnect", () =>
                {
                    if (NetworkClientMgr.Singleton.client is { Cts: { IsCancellationRequested: false } })
                    {
                        NetworkClientMgr.Singleton.client.Stop();
                    }
                });
                uiBuilder.AddButton("Connect", () =>
                {
                    if (NetworkClientMgr.Singleton.client is { Cts: { IsCancellationRequested: false } }) return;
                    NetworkClientMgr.Singleton.Connect("localhost", 8080);
                });
                uiBuilder.AddButton("Spawn", () =>
                {
                    TransformComponent transformComponent = new TransformComponent()
                    {
                        pos = UnityToolkit.MathTypes.Vector3.zero,
                        rotation = UnityToolkit.MathTypes.Quaternion.identity,
                        scale = UnityToolkit.MathTypes.Vector3.one
                    };
                    transformComponent.mask = TransformComponent.Mask.All;
                    NetworkClientMgr.Singleton.SpawnEntity(transformComponent);
                });
            });
            // Game RPC Debug
            builder.AddFoldout("RPC", uiBuilder => { uiBuilder.AddButton("Sum", GameRPC_SumAsync); });
        }

        private async void GameRPC_SumAsync()
        {
            var result = await Global.RPC.gameService.SumAsync(1, 2);
            Global.Log.Info($"Sum: {result}");
        }

        public void Dispose()
        {
            // TODO release managed resources here
        }

        public void Hide()
        {
            uiDocument.enabled = false;
        }

        public void Open()
        {
            if (uiDocument.enabled) return;
            uiDocument.enabled = true;
            var builder = new DebugUIBuilder();
            builder.ConfigureWindowOptions(options =>
            {
                options.Title = GetType().Name;
            });

            Configure(builder);
            builder.BuildWith(uiDocument);
        }
    }
}