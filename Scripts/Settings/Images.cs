using System;
using System.Linq;
using UnityEngine;
using GAME.Utils.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace GAME.Settings.Level.Images
{
    [CreateAssetMenu(fileName = _Images, menuName = Helper.SettingsMenu + _Images)]
    public class ImageSettings : ScriptableObject
    {
        [SerializeField] Image<PictureName>[] _pictures;
        [SerializeField] Image<TextureName>[] _textures;
        [SerializeField] Image<BackgroundName>[] _backgrounds;

        const string _Images = "Images";
        static ImageSettings _instance;

        Dictionary<PictureName, Sprite> _picMap = new();
        Dictionary<TextureName, Sprite> _texMap = new();
        Dictionary<BackgroundName, Sprite> _bgdMap = new();
        Dictionary<PictureName, AsyncOperationHandle<Sprite>> _picHandles = new();
        Dictionary<TextureName, AsyncOperationHandle<Sprite>> _texHandles = new();
        Dictionary<BackgroundName, AsyncOperationHandle<Sprite>> _bgdHandles = new();

        static TaskCompletionSource<bool> _readyTcs = new();
        public static Task Ready => _readyTcs.Task;

        public static ImageSettings Instance => _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnGameStart()
        {
            var h = Addressables.LoadAssetAsync<ImageSettings>(_Images);
            h.Completed += handle =>
            {
                _instance = handle.Result;
                _instance.Initialize();
                _readyTcs.TrySetResult(true);
            };
        }

        [RuntimeInitializeOnLoadMethod]
        static void RegisterQuitCallback() => Application.quitting += () => _instance?.UnloadAll();

        void Initialize() { }

        public void UnloadAll()
        {
            foreach (var h in _picHandles.Values) Addressables.Release(h);
            foreach (var h in _texHandles.Values) Addressables.Release(h);
            foreach (var h in _bgdHandles.Values) Addressables.Release(h);
            _picHandles.Clear(); _picMap.Clear();
            _texHandles.Clear(); _texMap.Clear();
            _bgdHandles.Clear(); _bgdMap.Clear();
        }

        public AsyncOperationHandle<Sprite> LoadBackground(BackgroundName name)
        {
            if (_bgdHandles.TryGetValue(name, out var h)) return h;
            var handle = Addressables.LoadAssetAsync<Sprite>(name.ToString());
            _bgdHandles[name] = handle;
            handle.Completed += op => { if (op.Status == AsyncOperationStatus.Succeeded) _bgdMap[name] = op.Result; };
            return handle;
        }

        public AsyncOperationHandle<Sprite> LoadPicture(PictureName name)
        {
            if (_picHandles.TryGetValue(name, out var h)) return h;
            var handle = Addressables.LoadAssetAsync<Sprite>(name.ToString());
            _picHandles[name] = handle;
            handle.Completed += op => { if (op.Status == AsyncOperationStatus.Succeeded) _picMap[name] = op.Result; };
            return handle;
        }

        public AsyncOperationHandle<Sprite> LoadTexture(TextureName name)
        {
            if (_texHandles.TryGetValue(name, out var h)) return h;
            var handle = Addressables.LoadAssetAsync<Sprite>(name.ToString());
            _texHandles[name] = handle;
            handle.Completed += op => { if (op.Status == AsyncOperationStatus.Succeeded) _texMap[name] = op.Result; };
            return handle;
        }

        public void UnloadPicture(PictureName name)
        {
            if (_picHandles.TryGetValue(name, out var h))
            {
                Addressables.Release(h);
                _picHandles.Remove(name);
                _picMap.Remove(name);
            }
        }

        public void UnloadTexture(TextureName name)
        {
            if (_texHandles.TryGetValue(name, out var h))
            {
                Addressables.Release(h);
                _texHandles.Remove(name);
                _texMap.Remove(name);
            }
        }

        public void UnloadBackground(BackgroundName name)
        {
            if (_bgdHandles.TryGetValue(name, out var h))
            {
                Addressables.Release(h);
                _bgdHandles.Remove(name);
                _bgdMap.Remove(name);
            }
        }

        public Sprite GetSprite(PictureName name) => _picMap.TryGetValue(name, out var s) ? s : (_picMap.Count > 0 ? _picMap.First().Value : null);
        public Sprite GetSprite(TextureName name) => _texMap.TryGetValue(name, out var s) ? s : (_texMap.Count > 0 ? _texMap.First().Value : null);
        public Sprite GetSprite(BackgroundName name) => _bgdMap.TryGetValue(name, out var s) ? s : (_bgdMap.Count > 0 ? _bgdMap.First().Value : null);
    }

    [Serializable]
    public class Image<T> where T : Enum
    {
        [SerializeField] T _name;
        public T Name => _name;
    }

    public enum TextureName { Texture1, Texture2, Texture3, Texture4, Texture5 }
    public enum PictureName
    {
        Picture1, Picture2, Picture3, Picture4, Picture5, Picture6, Picture7, Picture8, Picture9, Picture10,
        Picture11, Picture12, Picture13, Picture14, Picture15, Picture16, Picture17, Picture18, Picture19, Picture20,
        Picture21, Picture22, Picture23, Picture24, Picture25, Picture26, Picture27, Picture28, Picture29, Picture30
    }
    public enum BackgroundName
    {
        Background1, Background2, Background3, Background4, Background5, Background6, Background7, Background8,
        Background9, Background10, Background11, Background12, Background13, Background14, Background15, Background16,
        Background17, Background18, Background19, Background20, Background21, Background22, Background23, Background24,
        Background25, Background26, Background27, Background28, Background29, Background30, Background31, Background32,
        Background33, Background34, Background35, Background36, Background37, Background38, Background39, Background40,
        Background41, Background42, Background43, Background44, Background45, Background46, Background47, Background48
    }
}
