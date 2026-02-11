using System;
using System.Linq;
using UnityEngine;
using GAME.Audio.SFX;
using GAME.Utils.Core;
using GAME.Audio.Music;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GAME.Settings.Audio
{
    [CreateAssetMenu(fileName = Helper.AudioName, menuName = Helper.SettingsMenu + Helper.AudioName)]
    public class AudioSettings : ScriptableObject
    {
        [field: Header("MUSIC")]
        [SerializeField, Range(0, 1), Space(2)] float _music = 0.2f;

        [field: Header("SOUNDS")]
        [SerializeField][Range(0, 1)][Space(2)] float _level = 0.4f;
        [SerializeField][Range(0, 1)][Space(2)] float _player = 0.9f;
        [SerializeField][Range(0, 1)][Space(2)] float _enemies = 0.6f;
        [SerializeField][Range(0, 1)][Space(2)] float _buffs = 0.35f;
        [SerializeField][Range(0, 1)][Space(2)] float _UI = 0.7f;

        [Header("LISTS")]
        //[SerializeField][Space] Track[] _tracks;
        [SerializeField][Space] SFX[] _sounds;

        public static AudioSettings Instance => _instance;
        public static Task Ready => _readyTcs.Task;
        //public SFX this[SFXName name] => _SFXDict.TryGetValue(name, out var sound) ? sound : null;
        public float MusicVolume => _music;
        public float LevelVolume => _level;
        public float PlayerVolume => _player;
        public float EnemyVolume => _enemies;
        public float BuffVolume => _buffs;
        public float UIVolume => _UI;


        static AudioSettings _instance;
        static TaskCompletionSource<bool> _readyTcs = new();
        Dictionary<TrackName, Track> _tracksDict;
        Dictionary<SFXName, SFX> _sfxDict;
        Dictionary<TrackName, AudioClip> _trackMap = new();
        Dictionary<TrackName, AsyncOperationHandle<AudioClip>> _trackHandles = new();


        public AsyncOperationHandle<AudioClip> LoadTrack(TrackName name)
        {
            if (_trackHandles.TryGetValue(name, out var h)) return h;

            var handle = Addressables.LoadAssetAsync<AudioClip>(name.ToString());
            _trackHandles[name] = handle;
            handle.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded) _trackMap[name] = op.Result;
            };

            return handle;
        }

        public async Task<AudioClip> LoadTrackAsync(TrackName name)
        {
            if (_trackMap.TryGetValue(name, out var cached)) return cached;

            var handle = LoadTrack(name);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Debug.LogError($"[AudioSettings] Track '{name}' failed: {handle.OperationException}");
                return null;
            }
            return handle.Result;
        }


        public SFX GetSFX(SFXName name) => _sfxDict.TryGetValue(name, out var sfx) ? sfx : null;

        public AudioClip GetTrack(TrackName name) => _trackMap.TryGetValue(name, out var c) ? c : null;
        //public Track GetTrack(TrackName name) => _tracksDict.TryGetValue(name, out var track) ? track : null;

        public void UnloadTrack(TrackName name)
        {
            if (_trackHandles.TryGetValue(name, out var h))
            {
                Addressables.Release(h);
                _trackHandles.Remove(name);
                _trackMap.Remove(name);
            }
        }

        public void UnloadAllTracks()
        {
            foreach (var h in _trackHandles.Values) Addressables.Release(h);
            _trackHandles.Clear();
            _trackMap.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnGameStart()
        {
            var h = Addressables.LoadAssetAsync<AudioSettings>(Helper.AudioName);
            h.Completed += op =>
            {
                _instance = op.Result;
                _instance.Initialize();
                _readyTcs.TrySetResult(true);
            };
        }

        [RuntimeInitializeOnLoadMethod]
        static void RegisterQuit() => Application.quitting += () => _instance?.UnloadAllTracks();


        void OnEnable()
        {
            _sfxDict = _sounds?.Where(s => s.Clip != null).ToDictionary(s => s.Name, s => s);
            //_tracksDict = _tracks?.ToDictionary(t => t.Name, t => t);
        }

        void Initialize()
        {

        }

        //void OnEnable()
        //{
        //    _sfxDict = _sounds?.Where(s => s.Clip != null).ToDictionary(s => s.Name, s => s);
        //    _tracksDict = _tracks?.Where(t => t.Clip != null).ToDictionary(t => t.Name, t => t);
        //}

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //static void OnGameStart()
        //{
        //    _instance = Resources.Load<ImageSettings>(Helper.SettingsPath + _Images);
        //    _instance.Initialize();
        //}

    }

}
