using System;
using UnityEngine;
using GAME.Settings.AD;
using GAME.Managers.Audio;
using GAME.Managers.Singleton;
#if YandexGamesPlatform_yg
using YG;
#endif

namespace GAME.Managers.AD
{
    public class ADManager : Singleton<ADManager, ADSettings>
    {
        AudioManager AUDIO;
        float _lastInterstitTime, _lastRewardedTime;

        protected override void Initialize()
        {
            base.Initialize();
            AUDIO = AudioManager.I;
            _lastInterstitTime = -_settings.InterInterval;
            _lastRewardedTime = -_settings.RewardInterval;
        }

        public void ShowInterstitial()
        {
            if (!_settings.Enabled) return;

#if YandexGamesPlatform_yg
            if (IsInterstitReady()) 
            {
                AUDIO.SwitchMusic();
                YG2.InterstitialAdvShow();
                _lastInterstitTime = Time.time;
                AUDIO.SwitchMusic();
            }
#endif
        }

        public void ShowRewarded(string type, Action callback)
        {
            if (!_settings.Enabled) 
            {
                callback?.Invoke();
                return;
            }
#if YandexGamesPlatform_yg
            if (IsRewardedReady()) 
            {
                AUDIO.SwitchMusic();
                YG2.RewardedAdvShow(type, callback); 
                _lastRewardedTime = Time.time;
                AUDIO.SwitchMusic();
            } 
#endif
        }

        bool IsInterstitReady() => _settings.IsCustom ? Time.time - _lastInterstitTime >= _settings.InterInterval : true;
        bool IsRewardedReady() => _settings.IsCustom ? Time.time - _lastRewardedTime >= _settings.RewardInterval : true;

    }
} 