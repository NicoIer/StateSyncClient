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
        #region Static

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Vector3Equal(Vector3 a, Vector3 b)
        {
            float epsilon = 0.001f;
            return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon && Mathf.Abs(a.z - b.z) < epsilon;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool QuaternionEqual(Quaternion a, Quaternion b)
        {
            float epsilon = 0.001f;
            return Mathf.Abs(a.x - b.x) < epsilon && Mathf.Abs(a.y - b.y) < epsilon && Mathf.Abs(a.z - b.z) < epsilon &&
                   Mathf.Abs(a.w - b.w) < epsilon;
        }

        #endregion

        protected NetworkEntityBehavior entity;
#if ODIN_INSPECTOR && UNITY_EDITOR
        [Sirenix.OdinInspector.ReadOnly]
        [field: SerializeField]
#endif
        public int componentIdx { get; internal set; } = -1;

        private void Awake()
        {
            entity = GetComponentInParent<NetworkEntityBehavior>(true);
        }

        public void Bind(NetworkComponent component)
        {
            OnNetworkSpawn(component);
        }

        public void UnBind()
        {
            OnNetworkDespawn();
        }

        protected abstract void OnNetworkSpawn(NetworkComponent component);

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