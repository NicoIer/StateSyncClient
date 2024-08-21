using System;
using Network;
using UnityEngine;
using UnityEngine.Assertions;
using UnityToolkit;

namespace Game
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class NetworkTransform : NetworkComponentBehavior
    {
        private TransformComponent _remoteTransform; // 远程的
        private TransformComponent _localTransform; // 本地的
        public float transformSyncInterval = 0.1f;
        private float _syncTimer;

        protected override void OnNetworkSpawn(INetworkComponent component)
        {
            _remoteTransform = component as TransformComponent;
            Assert.IsNotNull(_remoteTransform);
            _localTransform = _remoteTransform.DeepCopy();
        }

        private void Update()
        {
            if (componentIdx == -1) return;
            UploadTransform();
            // SyncLocalTransformFromRemote();
        }

        /// <summary>
        /// 上传本地的Transform数据到远程
        /// </summary>
        private void UploadTransform()
        {
            if (entityBehavior == null || _localTransform == null) return;
            _syncTimer += Time.deltaTime;
            if (!(_syncTimer >= transformSyncInterval)) return;
            _syncTimer = 0;

            // 如果位置发生变化 才更新
            Assert.IsTrue(_localTransform.pos != null, "_transformComponent.pos != null");
            bool changed = false;
            if (!Vector3Equal(_localTransform.pos.Value, transform.position))
            {
                _localTransform.pos = transform.position;
                changed |= true;
            }

            Assert.IsTrue(_localTransform.rotation != null,
                "_transformComponent.rotation != null");
            if (!QuaternionEqual(_localTransform.rotation.Value, transform.rotation))
            {
                _localTransform.rotation = transform.rotation;
            }

            Assert.IsTrue(_localTransform.scale != null, "_transformComponent.scale != null");
            if (!Vector3Equal(_localTransform.scale.Value, transform.localScale))
            {
                _localTransform.scale = transform.localScale;
                changed |= true;
            }

            if (changed)
            {
                NetworkLogger.Info($"{this} UploadTransform");
                entityBehavior.UpdateComponent(componentIdx, _localTransform);
            }
        }

        /// <summary>
        /// 使用远程的数据更新本地的Transform
        /// </summary>
        private void SyncLocalTransformFromRemote()
        {
            if (entityBehavior == null || _remoteTransform == null) return;
            // do some thing like dots system
            Assert.IsTrue(_remoteTransform.pos.HasValue);
            if (!Vector3Equal(transform.position, _remoteTransform.pos.Value))
            {
                // _posEma.Add(_remoteTransform.pos.Value);
                // transform.position = _posEma.Value;
                transform.position = _remoteTransform.pos.Value;
            }

            Assert.IsTrue(_remoteTransform.rotation.HasValue);
            if (!QuaternionEqual(transform.rotation, _remoteTransform.rotation.Value))
            {
                // _rotEma.Add(_remoteTransform.rotation.Value);
                // transform.rotation = _rotEma.Value;
                transform.rotation = _remoteTransform.rotation.Value;
            }

            Assert.IsTrue(_remoteTransform.scale.HasValue);
            if (!Vector3Equal(transform.localScale, _remoteTransform.scale.Value))
            {
                // _scaleEma.Add(_remoteTransform.scale.Value);
                // transform.localScale = _scaleEma.Value;
                transform.localScale = _remoteTransform.scale.Value;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_localTransform == null || _remoteTransform == null) return;
            if (_localTransform.pos.HasValue)
            {
                Gizmos.color = Color.red;
                if (_localTransform.scale.HasValue)
                {
                    Gizmos.DrawWireCube(_localTransform.pos.Value, _localTransform.scale.Value);
                }
                else
                {
                    Gizmos.DrawWireSphere(_localTransform.pos.Value, 1f);
                }

                Gizmos.color = Color.green;
            }
            
            if(_remoteTransform.pos.HasValue)
            {
                Gizmos.color = Color.blue;
                if (_remoteTransform.scale.HasValue)
                {
                    Gizmos.DrawWireCube(_remoteTransform.pos.Value, _remoteTransform.scale.Value * 0.95f);
                }
                else
                {
                    Gizmos.DrawWireSphere(_remoteTransform.pos.Value, 1f);
                }
            }
        }
#endif
    }
}