using System;
using Network;
using UnityEngine;

namespace Game
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    public class NetworkEntityBehavior : MonoBehaviour
    {
        public bool ownerShip => _entity.owner == NetworkMgr.Singleton.connectionId;
        private NetworkEntity _entity;
        private int _transformCompIdx = -1;
        private TransformComponent _transformComponent;
        private ExponentialMovingAverageVector3 _posEma = new ExponentialMovingAverageVector3(10);
        private ExponentialMovingAverageQuaternion _rotEma = new ExponentialMovingAverageQuaternion(10);
        private ExponentialMovingAverageVector3 _scaleEma = new ExponentialMovingAverageVector3(10);

        public float transformSyncInterval = 0.1f; // 每隔多久同步一次Transform
        private float _currentSyncTime = 0;

        private void Awake()
        {
            _transformComponent = new TransformComponent
            {
                pos = transform.position,
                rotation = transform.rotation,
                scale = transform.localScale
            };
        }

        public void Bind(NetworkEntity entity)
        {
            if (_entity != null)
            {
                Debug.LogError($"{this} already related to {_entity}");
                return;
            }

            _entity = entity;
            _transformCompIdx = -1;
            for (int i = 0; i < _entity.components.Count; i++)
            {
                if (_entity.components[i] is TransformComponent)
                {
                    _transformCompIdx = i;
                    break;
                }
            }

            if (_transformCompIdx == -1)
            {
                Debug.LogError($"{_entity} doesn't have TransformComponent");
                return;
            }

            if (_transformComponent.pos.HasValue)
            {
                _posEma.Reset();
                _posEma.Add(_transformComponent.pos.Value);
                transform.position = _posEma.Value;
            }

            if (_transformComponent.rotation.HasValue)
            {
                _rotEma.Reset();
                _rotEma.Add(_transformComponent.rotation.Value);
                transform.rotation = _rotEma.Value;
            }

            if (_transformComponent.scale.HasValue)
            {
                _scaleEma.Reset();
                _scaleEma.Add(_transformComponent.scale.Value);
                transform.localScale = _scaleEma.Value;
            }
        }

        public void Unbind()
        {
            _entity = null;
            _transformComponent = null;
        }

        private void Update()
        {
            if (_entity != null && _transformComponent != null)
            {
                _currentSyncTime += Time.deltaTime;
                if (_currentSyncTime >= transformSyncInterval)
                {
                    _currentSyncTime = 0;
                    NetworkMgr.Singleton.UpdateComponent(_entity, _transformCompIdx);
                }
            }
        }
    }
}