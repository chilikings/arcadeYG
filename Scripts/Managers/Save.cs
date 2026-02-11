using UnityEngine;
using UnityEngine.Events;
using GAME.Settings.Saves;
using GAME.Managers.Level;
using GAME.Managers.Singleton;
using System.Collections.Generic;
#if YandexGamesPlatform_yg
using YG;
#endif

namespace GAME.Managers.Saves
{
    public class SaveManager : Singleton<SaveManager, SaveSettings>
    {
        [SerializeField] UnityEvent<int> _onLevelLoad;
        [SerializeField] UnityEvent<string> _onNumberLoad;
        [SerializeField] UnityEvent<int> _onLivesLoad;
        //[SerializeField] UnityEvent<bool> _onMusicLoad;
        //[SerializeField] UnityEvent<bool> _onSoundLoad;

        const string _Number = "Number", _Lives = "Lives", _Level = "Level", _Buff = "Buff", 
                     _Skin = "Skin", _Item = "Item", _Music = "Music", _Sound = "SFX";
        int? _playerNumber, _livesCount, _levelID, _buffID, _itemID, _skinID;
        bool? _isMusicMuted, _isSoundMuted;

        public int PlayerNumber => _playerNumber ??= GetValue(_Number);
        public int LivesCount => _livesCount ??= GetValue(_Lives);
        public int LevelID => _levelID ??= GetValue(_Level);
        public int BuffID => _buffID ??= GetValue(_Buff);
        public int SkinID => _skinID ??= GetValue(_Skin);
        public int ItemID => _itemID ??= GetValue(_Item);
        public bool IsMusicMuted => _isMusicMuted ??= GetValue(_Music) == 1;
        public bool IsSoundMuted => _isSoundMuted ??= GetValue(_Sound) == 1;


        public void ResetAll()
        {
#if UNITY_EDITOR
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
#endif
#if YandexGamesPlatform_yg
            YG2.SetAllStats(new Dictionary<string, int>());
            YG2.LoadStats();
#endif
        }
        public void ResetLevel() => _levelID = SetValue(_Level, 0);
        public void SaveNumber(int number) => _playerNumber = SetValue(_Number, number);
        public void SaveLives(int count) { if (_livesCount != count) _livesCount = SetValue(_Lives, count); }
        public void SaveLevel(int id) { if (id > LevelID) _levelID = SetValue(_Level, id); }
        public void UnlockLevels() => _levelID = SetValue(_Level, LevelManager.I.LevelsCount - 1);
        public void SaveBuff(int id) => _buffID = SetValue(_Buff, id);
        public void SaveSkin(int id) => _skinID = SetValue(_Skin, id);
        public void SaveItem(int id) => _itemID = SetValue(_Item, id);
        public void SaveMusicMute(bool mute) => _isMusicMuted = SetValue(_Music, mute ? 1 : 0) == 1;  
        public void SaveSoundMute(bool mute) => _isSoundMuted = SetValue(_Sound, mute ? 1 : 0) == 1;
        

        public int LoadNumber() 
        {           
            _onNumberLoad?.Invoke(PlayerNumber.ToString("D3"));
            return PlayerNumber;
        }
        public int LoadLives() 
        {
            int livesCount = LivesCount;
            _onLivesLoad?.Invoke(livesCount);
            return livesCount;
        }
        public int LoadLevel()
        {
            _onLevelLoad?.Invoke(LevelID);
            return LevelID;
        }

        //public bool LoadMusicMute()
        //{
        //    _onMusicLoad?.Invoke(IsMusicMuted);
        //    return IsMusicMuted;
        //}

        //public bool LoadSoundMute()
        //{
        //    _onSoundLoad?.Invoke(IsSoundMuted);
        //    return IsSoundMuted;
        //}

        int GetValue(string key)
        {
#if YandexGamesPlatform_yg
            return YG2.GetState(key);
#else
            return PlayerPrefs.GetInt(key);
#endif
        }

        int SetValue(string key, int value)
        {
#if YandexGamesPlatform_yg
            YG2.SetState(key, value);
#else
            PlayerPrefs.SetInt(key, value);
#endif
            return value;
        }

        // void SetupAutoSave() => InvokeRepeating(nameof(SaveGame), _settings.AutoSaveInterval, _settings.AutoSaveInterval);
    }
}