using System;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class NetworkTransform : NetworkComponentBehavior
    {
        private TransformComponent _localTransform;
        public float transformSyncInterval = 0.1f;
        private float _syncTimer;

        protected override void OnNetworkSpawn(NetworkComponent component)
        {
            _localTransform = component as TransformComponent;
            Assert.IsNotNull(_localTransform);
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
            if (entity == null || _localTransform == null) return;
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
                entity.UpdateComponent(componentIdx);
            }
        }

        /// <summary>
        /// 使用远程的数据更新本地的Transform
        /// </summary>
        private void SyncLocalTransformFromRemote()
        {
            if (entity == null || _localTransform == null) return;
            // do some thing like dots system
            Assert.IsTrue(_localTransform.pos.HasValue);
            if (!Vector3Equal(transform.position, _localTransform.pos.Value))
            {
                // _posEma.Add(_localTransform.pos.Value);
                // transform.position = _posEma.Value;
                transform.position = _localTransform.pos.Value;
            }

            Assert.IsTrue(_localTransform.rotation.HasValue);
            if (!QuaternionEqual(transform.rotation, _localTransform.rotation.Value))
            {
                // _rotEma.Add(_localTransform.rotation.Value);
                // transform.rotation = _rotEma.Value;
                transform.rotation = _localTransform.rotation.Value;
            }

            Assert.IsTrue(_localTransform.scale.HasValue);
            if (!Vector3Equal(transform.localScale, _localTransform.scale.Value))
            {
                // _scaleEma.Add(_localTransform.scale.Value);
                // transform.localScale = _scaleEma.Value;
                transform.localScale = _localTransform.scale.Value;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_localTransform == null) return;
            if (_localTransform.pos.HasValue)
            {
                Gizmos.color = Color.red;
                if (_localTransform.scale.HasValue)
                    Gizmos.DrawWireCube(_localTransform.pos.Value, _localTransform.scale.Value);
                else
                {
                    Gizmos.DrawWireSphere(_localTransform.pos.Value, 1f);
                }
            }
        }
#endif
    }
}