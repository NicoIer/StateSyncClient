using System;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class NetworkEntityBehavior : MonoBehaviour
    {
        public NetworkAuthority authority = NetworkAuthority.ServerOnly;
        public bool self => _entity.owner == NetworkMgr.Singleton.connectionId;
        private NetworkEntity _entity;
        private int _transformCompIdx = -1;

        private TransformComponent _localTransform;
        // private ExponentialMovingAverageVector3 _posEma = new ExponentialMovingAverageVector3(10);
        // private ExponentialMovingAverageQuaternion _rotEma = new ExponentialMovingAverageQuaternion(10);
        // private ExponentialMovingAverageVector3 _scaleEma = new ExponentialMovingAverageVector3(10);

        public float transformSyncInterval = 0.1f; // 每隔多久同步一次Transform
        private float _currentSyncTime = 0;


        public void Bind(NetworkEntity entity)
        {
            if (_entity != null)
            {
                Debug.LogError($"{this} already related to {_entity}");
                return;
            }

            _entity = entity;
            for (int i = 0; i < _entity.components.Count; i++)
            {
                if (_entity.components[i] is TransformComponent local)
                {
                    _localTransform = local;
                    _transformCompIdx = i;
                    break;
                }
            }

            NetworkLogger.Info($"{this} Bind {_entity}");
            if (_transformCompIdx == -1)
            {
                Debug.LogError($"{_entity} doesn't have TransformComponent");
                return;
            }

            Assert.IsTrue(_localTransform != null, "_localTransform != null");
            Assert.IsTrue(_localTransform == _entity.components[_transformCompIdx],
                "_localTransform == _entity.components[_transformCompIdx]");
            if (_localTransform.pos.HasValue)
            {
                // _posEma.Reset();
                // _posEma.Add(_localTransform.pos.Value);
                transform.position = _localTransform.pos.Value;
            }

            if (_localTransform.rotation.HasValue)
            {
                // _rotEma.Reset();
                // _rotEma.Add(_localTransform.rotation.Value);
                transform.rotation = _localTransform.rotation.Value;
            }

            if (_localTransform.scale.HasValue)
            {
                // _scaleEma.Reset();
                // _scaleEma.Add(_localTransform.scale.Value);
                transform.localScale = _localTransform.scale.Value;
            }
        }

        public void Unbind()
        {
            _entity = null;
            _localTransform = null;
        }

        [Sirenix.OdinInspector.Button]
        private void TestPosSync(Vector3 pos)
        {
            _localTransform.pos = pos;
            NetworkMgr.Singleton.UpdateComponent(_entity, _transformCompIdx);
        }

        private void UploadTransform()
        {
            if (_entity == null || _localTransform == null) return;
            _currentSyncTime += Time.deltaTime;
            if (!(_currentSyncTime >= transformSyncInterval)) return;
            _currentSyncTime = 0;

            // 如果位置发生变化 才更新
            Assert.IsTrue(_localTransform.pos != null, "_transformComponent.pos != null");
            if (_localTransform.pos.Value != transform.position)
            {
                _localTransform.pos = transform.position;
            }

            Assert.IsTrue(_localTransform.rotation != null,
                "_transformComponent.rotation != null");
            if (_localTransform.rotation.Value != transform.rotation)
            {
                _localTransform.rotation = transform.rotation;
            }

            Assert.IsTrue(_localTransform.scale != null, "_transformComponent.scale != null");
            if (_localTransform.scale.Value != transform.localScale)
            {
                _localTransform.scale = transform.localScale;
            }

            NetworkMgr.Singleton.UpdateComponent(_entity, _transformCompIdx);
        }

        private void DownloadTransform()
        {
            if (_entity == null || _localTransform == null) return;
            // do some thing like dots system
            Assert.IsTrue(_localTransform.pos.HasValue);
            if (transform.position != _localTransform.pos.Value)
            {
                // _posEma.Add(_localTransform.pos.Value);
                // transform.position = _posEma.Value;
                transform.position = _localTransform.pos.Value;
            }

            Assert.IsTrue(_localTransform.rotation.HasValue);
            if (transform.rotation != _localTransform.rotation.Value)
            {
                // _rotEma.Add(_localTransform.rotation.Value);
                // transform.rotation = _rotEma.Value;
                transform.rotation = _localTransform.rotation.Value;
            }

            Assert.IsTrue(_localTransform.scale.HasValue);
            if (transform.localScale != _localTransform.scale.Value)
            {
                // _scaleEma.Add(_localTransform.scale.Value);
                // transform.localScale = _scaleEma.Value;
                transform.localScale = _localTransform.scale.Value;
            }
        }
        
        private void Update()
        {
            UploadTransform();
            DownloadTransform();
        }
    }
}