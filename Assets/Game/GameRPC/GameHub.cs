using GameCore.Service;

namespace Game.Network.GameRPC
{
    public struct GameHub : IGameHubReceiver
    {
        public void OnReceiveMessage(string message)
        {
            Global.Log.Info($"Received: {message}");
        }
    }
}