using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GAME.Audio.Music;
using GAME.Audio.SFX;
using GAME.Managers.Singleton;
using GAME.Settings.Levels;
using UnityEngine;
using AudioSettings = GAME.Settings.Audio.AudioSettings;

namespace GAME.Managers.Audio
{
    public class AudioManager : Singleton<AudioManager, AudioSettings>
    {
        [Header("SOURCES")]
        [SerializeField] AudioSource _music;
        [SerializeField, Space(4)] SFXSource[] _SFX;

        //[Header("EVENTS")]
        //[SerializeField, Space(2)] UnityEvent<bool> _onMusicSwitch;
        //[SerializeField, Space(4)] UnityEvent<bool> _onSoundSwitch;


        Dictionary<SFXGroup, SFXSource> _SFXSources;
        AudioSource _level, _player, _enemy, _buff, _ui;
        Coroutine _playingMusic;
        TrackName _currentTrack;

        SFXSource this[SFXGroup group] => _SFXSources.TryGetValue(group, out var source) ? source : null;
        public bool IsMusicMuted => _music.mute;
        public bool IsSoundMuted => _level.mute;


        public void SetMusicMute(bool mute) => _music.mute = mute;

        public void SetSoundMute(bool mute) => _level.mute = _player.mute = _enemy.mute = _buff.mute = _ui.mute = mute;

        public void PlaySFX(SFXName name)
        {
            var sfx = _settings.GetSFX(name);
            if (sfx == null || sfx.Clip == null) return;
            _SFXSources[sfx.Group].Source.PlayOneShot(sfx.Clip, sfx.Volume);
        }

        public void PlayMusic(LevelInfo level)
        {
            if (_playingMusic != null)
            {
                StopCoroutine(_playingMusic);
                _playingMusic = null;
            }
            _playingMusic = StartCoroutine(PlayingMusic(level.Track));
            //StartCoroutine(PlayMusicRoutine(level.Track));
        }

        IEnumerator PlayingMusic(TrackName trackName)
        {
            while (!AudioSettings.Ready.IsCompleted) yield return null;

            if (_music.clip != null && _currentTrack.Equals(trackName))
            {
                _playingMusic = null;
                yield break;
            }

            if (!Equals(_currentTrack, default(TrackName)) && !Equals(_currentTrack, trackName))
            {
                _settings.UnloadTrack(_currentTrack);
                _music.clip = null;
            }

            var task = _settings.LoadTrackAsync(trackName);
            while (!task.IsCompleted) yield return null;

            var clip = task.Result;
            if (clip == null)
            {
                _playingMusic = null;
                yield break;
            }

            _music.Stop();
            _music.clip = clip;
            _music.Play();
            _currentTrack = trackName;

            _playingMusic = null;
        }

        public void PlayReward() => PlaySFX(SFXName.Reward);

        public void PlayClick() => PlaySFX(SFXName.Click);

        public void PlayToggle() => PlaySFX(SFXName.Toggle);

        //public void StopMusic() => _music?.Stop();
        public void StopMusic()
        {
            if (_playingMusic != null) { StopCoroutine(_playingMusic); _playingMusic = null; }

            _music?.Stop();
            if (!Equals(_currentTrack, default(TrackName)))
            {
                _settings.UnloadTrack(_currentTrack);
                _currentTrack = default;
                _music.clip = null;
            }
        }

        public void SwitchMusic() => _music.mute = !_music.mute;

        public void SwitchSFX() => _level.mute = _player.mute = _enemy.mute = _buff.mute = _ui.mute = !_level.mute;


        override protected void Awake()
        {
            base.Awake();
            _SFXSources = _SFX.Where(s => s.Source != null).ToDictionary(s => s.Group, s => s);
            CacheSources();
        }

        void Start()
        {
            StartCoroutine(SetupVolumeWhenReady(_settings));
        }

        IEnumerator SetupVolumeWhenReady(AudioSettings settings)
        {
            while (!AudioSettings.Ready.IsCompleted) yield return null;
            SetupVolume(settings);
        }

        void SetupVolume(AudioSettings settings)
        {
            _music.volume = settings.MusicVolume;
            _level.volume = settings.LevelVolume;
            _player.volume = settings.PlayerVolume;
            _enemy.volume = settings.EnemyVolume;
            _buff.volume = settings.BuffVolume;
            _ui.volume = settings.UIVolume;
        }

        void CacheSources()
        {
            _level = this[SFXGroup.Level]?.Source;
            _player = this[SFXGroup.Player]?.Source;
            _enemy = this[SFXGroup.Enemy]?.Source;
            _buff = this[SFXGroup.Buff]?.Source;
            _ui = this[SFXGroup.UI]?.Source;
        }

    }

}
