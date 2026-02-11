using System;
using UnityEngine;
using GAME.Utils.Core;
using System.Collections.Generic;

namespace GAME.Settings.Saves
{
    [CreateAssetMenu(fileName = Helper.SavesName, menuName = Helper.SettingsMenu + Helper.SavesName)]
    public class SaveSettings : ScriptableObject
    {
        [SerializeField] bool _autoSaveEnabled = true;
        [SerializeField] float _autoSaveInterval = 300f;

        public bool AutoSaveEnabled => _autoSaveEnabled;
        public float AutoSaveInterval => _autoSaveInterval;
    }

    [Serializable]
    public class SaveInfo
    {
        [SerializeField] int _currentScore;
        [SerializeField] int _highestScore;
        [SerializeField] Dictionary<string, bool> _achievements;
    }

}