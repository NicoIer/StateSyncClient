using System;
using System.Collections.Generic;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class NetworkEntityBehavior : MonoBehaviour
    {
        // public NetworkAuthority authority = NetworkAuthority.ServerOnly;
        public bool Ownership => entity.owner == EntityNetworkMgr.Singleton.connectionId;
        public NetworkEntity entity { get; private set; }
        private readonly List<NetworkComponentBehavior> _componentBehaviors = new List<NetworkComponentBehavior>();

        public bool spawned
        {
            get
            {
                if (entity != null && EntityNetworkMgr.Singleton.ContainsEntity(entity.id))
                {
                    return true;
                }

                return false;
            }
        }


        private void Awake()
        {
            Assert.IsFalse(spawned);
            if (spawned)
            {
                Global.Log.Error($"{this} 已经被创建过了 自动销毁");
                Destroy(gameObject);
                return;
            }

            // foreach (var component in GetComponents<NetworkComponentBehavior>())
            // {
            //     _componentBehaviors.Add(component);
            // }
        }

        public void Bind(NetworkEntity networkEntity)
        {
            if (entity != null)
            {
                Global.Log.Error($"{this} already related to {entity}");
                return;
            }

            entity = networkEntity;
            for (var i = 0; i < entity.components.Count; i++)
            {
                var component = entity.components[i];
                Assert.IsNotNull(component);
                if (ComponentLookup.Create(this, component, out var behavior))
                {
                    _componentBehaviors.Add(behavior);
                    behavior.componentIdx = i;
                    behavior.Bind(component);
                }
            }
        }

        public void SendComponentUpdate(int componentIdx)
        {
            EntityNetworkMgr.Singleton.UpdateComponent(entity, componentIdx);
        }

        public void SendComponentUpdate(int componentIdx, INetworkComponent local)
        {
            EntityNetworkMgr.Singleton.UpdateComponent(entity, componentIdx, local);
        }

        // private void OnValidate()
        // {
        //     // 不允许有多个 NetworkEntityBehavior
        //     if (GetComponentsInChildren<NetworkEntityBehavior>(true).Length > 1)
        //     {
        //         Debug.LogError($"{this} 只能有一个 NetworkEntityBehavior");
        //     }
        // }

        public override string ToString()
        {
            return entity != null ? entity.ToString() : base.ToString();
        }
    }
}