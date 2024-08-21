using Network;
using Network.Client;

namespace Game
{
    public class MyNetworkMgr : NetworkMgr
    {
        public override void OnInit()
        {
            base.OnInit();
            componentSerializer.Register<TransformComponent>();
        }
    }
}