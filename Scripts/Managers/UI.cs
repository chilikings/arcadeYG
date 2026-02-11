using UnityEngine;
using GAME.Settings.UI;
using GAME.Managers.Singleton;
using System.Collections.Generic;

namespace GAME.Managers.UI
{
    public class UIManager : Singleton<UIManager, UISettings>
    {
        Dictionary<string, GameObject> _screens = new();

        public bool IsDebug => _settings.IsDebug;


        public void ShowScreen(string name)
        {
            foreach (var screen in _screens) screen.Value.SetActive(screen.Key == name);
        }

        public void RegisterScreen(string name, GameObject screen)
        {
            if (!_screens.ContainsKey(name)) _screens.Add(name, screen);
        }

        protected override void Initialize()
        {
            base.Initialize();
            SetupScreens();
        }

        void SetupScreens()
        {
            //var screens = FindObjectsByType<Screen>();
            //foreach (var screen in screens)
            //    RegisterScreen(screen.ScreenName, screen.gameObject);
        }
    }
}