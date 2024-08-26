using System;
using System.Collections.Generic;
using Network;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 避免反射,如果存在一对一的关系,可以通过这个类进行查找
    /// 根据Component找到对应的Behavior 然后动态创建
    /// </summary>
    internal static class ComponentLookup
    {
        private static readonly Dictionary<Type, Func<NetworkEntityBehavior, NetworkComponentBehavior>>
            _component2Behavior =
                new Dictionary<Type, Func<NetworkEntityBehavior, NetworkComponentBehavior>>()
                {
                    {
                        typeof(TransformComponent),
                        entityBehavior => entityBehavior.gameObject.AddComponent<NetworkTransform>()
                    }
                };

        public static bool Create(NetworkEntityBehavior entityBehavior, NetworkComponent component,
            out NetworkComponentBehavior behavior)
        {
            if (_component2Behavior.TryGetValue(component.GetType(), out var func))
            {
                behavior = func(entityBehavior);
                return true;
            }

            behavior = null;
            return false;
        }
    }
}