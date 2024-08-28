using System;
using System.Collections.Generic;
using Network;
using Network.Client;
using UnityEngine;
using UnityToolkit;

namespace Game
{
    public class NetworkClientMgr : MonoSingleton<NetworkClientMgr>
    {
        private class MyNetworkMgr : NetworkMgr
        {
            public override void OnInit()
            {
                base.OnInit();
                componentSerializer.Register<TransformComponent>();
            }
        }

        public SnapshotInterpolationSettings interpolationSettings = new SnapshotInterpolationSettings();
        public Dictionary<uint, NetworkEntityBehavior> entityBehaviors { get; private set; }
        private MyNetworkMgr _mgr;
        public int connectionId => _mgr.connectionId;

        public NetworkClient client => _mgr.client;
        public NetworkClientTime time { get; private set; }

        protected override void OnInit()
        {
            _mgr = new MyNetworkMgr();
            _mgr.OnInit();
            _mgr.OnEntitySpawned += OnEntitySpawned;
            _mgr.client.socket.OnDisconnected += OnDisconnected;

            time = _mgr.client.GetSystem<NetworkClientTime>();
            entityBehaviors = new Dictionary<uint, NetworkEntityBehavior>();

            NetworkLoop.OnEarlyUpdate += OnNetworkEarlyUpdate;
            NetworkLoop.OnLateUpdate += OnNetworkLateUpdate;
        }

        protected override void OnDispose()
        {
            NetworkLoop.OnEarlyUpdate -= OnNetworkEarlyUpdate;
            NetworkLoop.OnLateUpdate -= OnNetworkLateUpdate;

            _mgr.OnDispose();
        }

        private void OnDisconnected()
        {
            foreach (var networkEntityBehavior in entityBehaviors.Values)
            {
                Destroy(networkEntityBehavior.gameObject);
            }

            entityBehaviors.Clear();
        }


        /// <summary>
        /// 有时候我们希望 网络消息在LateUpdate之后处理
        /// </summary>
        private void OnNetworkLateUpdate()
        {
            if (_mgr.client.Cts == null || _mgr.client.Cts.IsCancellationRequested)
            {
                return;
            }


            _mgr.client.socket.TickIncoming();
            _mgr.client.UpdateSystems();
        }

        /// <summary>
        /// 有时候我们希望 网络消息在Update之前处理
        /// </summary>
        private void OnNetworkEarlyUpdate()
        {
            if (_mgr.client.Cts == null || _mgr.client.Cts.IsCancellationRequested)
            {
                return;
            }

            _mgr.client.socket.TickOutgoing();
        }


        /// <summary>
        /// 连接到指定的服务器
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public void Connect(string host, int port)
        {
            if (_mgr.client is { Cts: { IsCancellationRequested: false } })
            {
                Debug.LogError("客户端已经连接到服务器");
                return;
            }

            UriBuilder uriBuilder = new UriBuilder
            {
                Host = host,
                Port = port
            };
            _mgr.client.Run(uriBuilder.Uri, false);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            if (_mgr.client is not { Cts: { IsCancellationRequested: false } })
            {
                Debug.LogError("客户端并未连接到服务器");
                return;
            }

            _mgr.client.Stop();
        }

        public bool ContainsEntity(uint entityID) => _mgr.ContainsEntity(entityID);

        public void UpdateComponent(NetworkEntity entity, int componentIdx) =>
            _mgr.UpdateComponent(entity, componentIdx);

        public void UpdateComponent(NetworkEntity entity, int componentIdx, INetworkComponent local) =>
            _mgr.UpdateComponent(entity, componentIdx, local);

        public void SpawnEntity(INetworkComponent component) => _mgr.SpawnEntity(component);

        private void OnEntitySpawned(NetworkEntity entity)
        {
            var entityObject = new GameObject($"[E{entity.id}]-[O{entity.owner}]");
            var entityBehavior = entityObject.AddComponent<NetworkEntityBehavior>();
            entityBehavior.Bind(entity);
            entityBehaviors.Add(entity.id, entityBehavior);
        }
    }
}