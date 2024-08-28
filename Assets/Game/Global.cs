using System.Runtime.CompilerServices;
using GameCore.AOTGeneration;
using UnityEngine;
using UnityToolkit;

namespace Game
{
    public class Global : MonoSingleton<Global>
    {
        protected override bool DontDestroyOnLoad() => true;

        public static class Log
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Info(string message)
            {
                Debug.Log(message);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Warning(string obj)
            {
                Debug.LogWarning(obj);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static void Error(string obj)
            {
                Debug.LogError(obj);
            }
        }

        protected override void OnInit()
        {
            ToolkitLog.infoAction = Log.Info;
            ToolkitLog.warningAction = Log.Warning;
            ToolkitLog.errorAction = Log.Error;
            
            Startup.Initialize();

        }

        protected override void OnDispose()
        {
            
        }
    }
}