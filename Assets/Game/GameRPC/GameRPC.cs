using System.Threading.Tasks;
using Cysharp.Net.Http;
using GameCore.AOTGeneration;
using GameCore.Service;
using Grpc.Net.Client;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Unity;
using UnityEngine;
using UnityToolkit;

namespace Game.Network.GameRPC
{
    public sealed class GameRPC : MonoBehaviour, ISystem, IOnInit
    {
        public string address = "http://localhost:5199";
        private GrpcChannelx _channel;
        public IGameService gameService { get; private set; }
        public IGameHubReceiver gameHubReceiver { get; private set; }
        private Task<IGameHub> _gameHub;

        public void OnInit()
        {
            var _ = Global.Singleton;

            var httpHandler = new YetAnotherHttpHandler()
            {
                Http2Only = true,
                SkipCertificateVerification = true
            };
            GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(() => new GrpcChannelOptions()
            {
                HttpHandler = httpHandler,
                DisposeHttpClient = true,
            }));

            _channel = GrpcChannelx.ForAddress(address);

            gameService = MagicOnionClient.Create<IGameService>(_channel);
            gameHubReceiver = new GameHub();
            _gameHub = StreamingHubClient.ConnectAsync<IGameHub, IGameHubReceiver>(_channel, gameHubReceiver);
        }

        public void Dispose()
        {
            _channel.Dispose();
            _gameHub.Dispose();
        }
    }
}