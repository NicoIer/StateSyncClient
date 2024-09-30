using System.Runtime.CompilerServices;
using Game.Network.GameRPC;
using GameCore.AOTGeneration;
using UnityEngine;
using UnityToolkit;

namespace Game
{
    public sealed partial class Global : MonoSingleton<Global>
    {
        //------------Editor && Runtime------------START
        public static class Log
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Info(string message)
            {
                UnityEngine.Debug.Log(message);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Warning(string obj)
            {
                UnityEngine.Debug.LogWarning(obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Error(string obj)
            {
                UnityEngine.Debug.LogError(obj);
            }
        }

        private static TypeEventSystem _event;

        public static TypeEventSystem Event
        {
            get
            {
                if (_event == null)
                {
                    _event = new TypeEventSystem();
                }

                return _event;
            }
        }

#if UNITY_EDITOR

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            _event = null;
        }
#endif

        //------------Editor && Runtime------------END

        
        
        protected override bool DontDestroyOnLoad() => true;


        private SystemLocator _locator;

        private GameRPC _rpc;
        public static GameRPC RPC => Singleton._rpc;
        
        protected override void OnInit()
        {
            ToolkitLog.infoAction = Log.Info;
            ToolkitLog.warningAction = Log.Warning;
            ToolkitLog.errorAction = Log.Error;

            Startup.Initialize();

            _locator = new SystemLocator();

            _rpc = GetComponentInChildren<GameRPC>();
            _locator.Register<GameRPC>(_rpc);
        }

        private void Update()
        {
            // Profiler.BeginSample("Global.Update");
            foreach (var system in _locator.systems)
            {
                if (system is IOnUpdate onUpdate)
                {
                    onUpdate.OnUpdate();
                }
            }

            // Profiler.EndSample();
        }

        protected override void OnDispose()
        {
            _locator.Dispose();
        }
    }
}