using System;
using System.Buffers;
using System.Collections.Generic;
using Network;
using Network.Client;
// using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Pool;
using UnityToolkit;

namespace Game
{
    public class NetworkMgr : MonoSingleton<NetworkMgr>
    {
        public IClientSocket socket;
        public NetworkClient client;


        [Sirenix.OdinInspector.ShowInInspector]
        public NetworkEntityMgr selfEntityMgr { get; private set; }

        public Dictionary<int, NetworkEntityMgr> otherEntityMgrs { get; private set; }

        /// <summary>
        /// 所有的网络实体 自己的和其他人的
        /// </summary>
        public Dictionary<uint, NetworkEntity> entities { get; private set; }

        public Dictionary<uint, NetworkEntityBehavior> entityBehaviors { get; private set; }

        public int connectionId => client.connectionId;

        /// <summary>
        /// 针对不同类型的NetworkComponent的序列化器
        /// </summary>
        public NetworkComponentSerializer componentSerializer { get; private set; }

        public NetworkBufferPool bufferPool { get; private set; }
        public NetworkBufferPool<NetworkComponentPacket> componentBufferPool { get; private set; }


        protected override void OnInit()
        {
            selfEntityMgr = new NetworkEntityMgr();
            otherEntityMgrs = new Dictionary<int, NetworkEntityMgr>();
            entities = new Dictionary<uint, NetworkEntity>();
            entityBehaviors = new Dictionary<uint, NetworkEntityBehavior>();
            componentSerializer = new NetworkComponentSerializer();
            bufferPool = new NetworkBufferPool(16);
            componentBufferPool = new NetworkBufferPool<NetworkComponentPacket>(16);

            socket = new TelepathyClientSocket();
            client = new NetworkClient(socket, 60);

            client.AddSystem<NetworkClientTime>();
            client.AddMsgHandler<NetworkEntitySpawn>(OnEntitySpawn);
            componentSerializer.Register<TransformComponent>();

            NetworkLoop.OnEarlyUpdate += OnNetworkEarlyUpdate;
            NetworkLoop.OnLateUpdate += OnNetworkLateUpdate;
        }

        protected override void OnDispose()
        {
            NetworkLoop.OnEarlyUpdate -= OnNetworkEarlyUpdate;
            NetworkLoop.OnLateUpdate -= OnNetworkLateUpdate;
            client.Dispose();
        }


        /// <summary>
        /// 有时候我们希望 网络消息在LateUpdate之后处理
        /// </summary>
        private void OnNetworkLateUpdate()
        {
            if (client.Cts == null || client.Cts.IsCancellationRequested)
            {
                return;
            }


            client.socket.TickIncoming();
            client.UpdateSystems();
        }

        /// <summary>
        /// 有时候我们希望 网络消息在Update之前处理
        /// </summary>
        private void OnNetworkEarlyUpdate()
        {
            if (client.Cts == null || client.Cts.IsCancellationRequested)
            {
                return;
            }

            client.socket.TickOutgoing();
        }


        private void OnEntitySpawn(NetworkEntitySpawn spawn)
        {
            Debug.Assert(connectionId != 0, "connectionId!=0");
            Debug.Assert(spawn.id != null, "spawn.id != null");
            Debug.Assert(spawn.owner != null, "spawn.owner != null");
            NetworkEntity entity = NetworkEntity.From(spawn.id.Value, spawn.owner.Value, spawn, componentSerializer);
            entities.Add(entity.id, entity);
            if (spawn.owner == connectionId)
            {
                NetworkLogger.Info($"服务器为自己生成了一个实体:{entity}");
                selfEntityMgr.Add(entity);
            }
            else
            {
                if (!otherEntityMgrs.ContainsKey(spawn.owner.Value))
                {
                    otherEntityMgrs.Add(spawn.owner.Value, new NetworkEntityMgr());
                }

                NetworkLogger.Info($"服务器为其他人[{spawn.owner}]生成了一个实体:{entity}");
                otherEntityMgrs[spawn.owner.Value].Add(entity);
            }

            var entityObject = new GameObject($"Entity:{entity.id}");
            var entityBehavior = entityObject.AddComponent<NetworkEntityBehavior>();
            entityBehavior.Bind(entity);
            entityBehaviors.Add(entity.id, entityBehavior);
        }


        public void Run(string host, int port)
        {
            if (client is { Cts: { IsCancellationRequested: false } })
            {
                Debug.LogError("Client is already running");
                return;
            }

            UriBuilder uriBuilder = new UriBuilder
            {
                Host = host,
                Port = port
            };
            client.Run(uriBuilder.Uri, false);
        }


        public void UpdateComponent(NetworkEntity entity, int idx)
        {
            Assert.IsTrue(entity.components.Count > idx, "entity.components.Count > idx");
            Assert.IsTrue(entities.ContainsKey(entity.id), "entities.ContainsKey(entity.id)");

            NetworkBuffer buffer = bufferPool.Get();
            NetworkComponent component = entity.components[idx];
            NetworkComponentPacket packet = component.ToDummyPacket(buffer);
            packet.idx = idx;
            packet.entityId = entity.id;
            NetworkComponentUpdate updateMsg = new NetworkComponentUpdate(packet);
            client.Send(updateMsg);
            bufferPool.Return(buffer);
        }

        public void SpawnEntity(params NetworkComponent[] components)
        {
            NetworkBuffer<NetworkComponentPacket> componentBuffer = componentBufferPool.Get();
            var list = ListPool<NetworkBuffer>.Get();
            for (int i = 0; i < components.Length; i++)
            {
                var component = components[i];
                NetworkBuffer buffer = bufferPool.Get();
                list.Add(buffer);
                NetworkComponentPacket packet = component.ToDummyPacket(buffer);
                componentBuffer.Write(packet);
            }

            NetworkEntitySpawn spawn = new NetworkEntitySpawn(null, client.connectionId, componentBuffer);
            client.Send(spawn);
            componentBufferPool.Return(componentBuffer);
            foreach (var buffer in list)
            {
                bufferPool.Return(buffer);
            }

            ListPool<NetworkBuffer>.Release(list);
        }
    }
}