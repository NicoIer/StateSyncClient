using System;
using IngameDebugConsole;
using UnityEngine;
using UnityToolkit;

namespace Game
{
    [DisallowMultipleComponent]
    public class GameDebug : MonoSingleton<GameDebug>
    {
        private GameDebugUI _gameDebugUI;
        private DebugLogManager _debugLogManager;


        protected override bool DontDestroyOnLoad() => true;

        protected override void OnInit()
        {
            _gameDebugUI = GetComponentInChildren<GameDebugUI>();
#if UNITY_EDITOR
            OpenDebugUI();
#else
            CloseDebugUI();
#endif


            _debugLogManager = GetComponentInChildren<DebugLogManager>();
            _debugLogManager.OnLogWindowHidden += OnConsoleWindowHidden;
            _debugLogManager.OnLogWindowShown += OnConsoleWindowShown;
            _debugLogManager.GetComponent<Canvas>().enabled = false; // 减少DrawCall

            DebugLogConsole.AddCommand("hello", "This command says hello",
                (string[] args) => { Global.Log.Info("Hello!"); });
        }


        protected override void OnDispose()
        {
            DebugLogConsole.RemoveCommand("hello");
        }

        public void OpenConsole()
        {
            _debugLogManager.ShowLogWindow();
        }

        public void CloseConsole()
        {
            _debugLogManager.HideLogWindow();
        }

        private void OnConsoleWindowShown()
        {
            _debugLogManager.GetComponent<Canvas>().enabled = true;
        }

        private void OnConsoleWindowHidden()
        {
            _debugLogManager.GetComponent<Canvas>().enabled = false;
        }

        [Sirenix.OdinInspector.Button]
        public void OpenDebugUI()
        {
            _gameDebugUI.Open();
        }

        [Sirenix.OdinInspector.Button]
        public void CloseDebugUI()
        {
            _gameDebugUI.Hide();
        }
    }
}