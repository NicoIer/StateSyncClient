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
    public class NetworkClientMgr : MonoSingleton<NetworkClientMgr>
    {
        public Dictionary<uint, NetworkEntityBehavior> entityBehaviors { get; private set; }
        private MyNetworkMgr _mgr;
        public NetworkClient client => _mgr.client;
        public int connectionId => _mgr.connectionId;

        protected override void OnInit()
        {
            _mgr = new MyNetworkMgr();
            _mgr.OnInit();
            _mgr.OnEntitySpawned += OnEntitySpawned;
            _mgr.client.socket.OnDisconnected += OnDisconnected;

            entityBehaviors = new Dictionary<uint, NetworkEntityBehavior>();

            NetworkLoop.OnEarlyUpdate += OnNetworkEarlyUpdate;
            NetworkLoop.OnLateUpdate += OnNetworkLateUpdate;
        }
        private void OnEntitySpawned(NetworkEntity entity)
        {
            var entityObject = new GameObject($"[E{entity.id}]-[O{entity.owner}]");
            var entityBehavior = entityObject.AddComponent<NetworkEntityBehavior>();
            entityBehavior.Bind(entity);
            entityBehaviors.Add(entity.id, entityBehavior);
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


        public void Connect(string host, int port)
        {
            if (_mgr.client is { Cts: { IsCancellationRequested: false } })
            {
                Debug.LogError("Client is already running");
                return;
            }

            UriBuilder uriBuilder = new UriBuilder
            {
                Host = host,
                Port = port
            };
            _mgr.client.Run(uriBuilder.Uri, false);
        }

        public bool ContainsEntity(uint entityID) => _mgr.ContainsEntity(entityID);

        public void UpdateComponent(NetworkEntity entity, int componentIdx) =>
            _mgr.UpdateComponent(entity, componentIdx);
        
        public void UpdateComponent(NetworkEntity entity,int componentIdx,INetworkComponent local) =>
            _mgr.UpdateComponent(entity, componentIdx, local);

        public void SpawnEntity(INetworkComponent component) => _mgr.SpawnEntity(component);
    }
}