using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game
{
    public abstract class NetworkComponentBehavior : MonoBehaviour
    {
        protected NetworkEntityBehavior entityBehavior;
#if ODIN_INSPECTOR && UNITY_EDITOR
        [Sirenix.OdinInspector.ReadOnly]
        [field: SerializeField]
#endif
        public int componentIdx { get; internal set; } = -1;

        private void Awake()
        {
            entityBehavior = GetComponentInParent<NetworkEntityBehavior>(true);
        }

        public void Bind(INetworkComponent component)
        {
            OnNetworkSpawn(component);
        }

        public void UnBind()
        {
            OnNetworkDespawn();
        }

        protected abstract void OnNetworkSpawn(INetworkComponent component);

        protected virtual void OnNetworkDespawn()
        {
        }

        private void OnValidate()
        {
            if (GetComponent<NetworkEntityBehavior>() == null && GetComponentInParent<NetworkEntityBehavior>() == null)
            {
                Debug.LogError($"{this} must be a child of a NetworkEntityBehavior");
            }
        }
    }
}