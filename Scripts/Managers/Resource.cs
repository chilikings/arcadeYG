using UnityEngine;
using GAME.Settings.Assets;
using GAME.Managers.Singleton;
using System.Collections.Generic;

namespace GAME.Managers.Resources
{
    public class ResourceManager : Singleton<ResourceManager, ResourceSettings>
    {
        private Dictionary<string, Object> _resources = new();


        protected override void Initialize()
        {
            base.Initialize();
            LoadAll();
        }

        public T LoadResource<T>(string path) where T : Object
        {
            var fullPath = $"{typeof(T).Name}/{path}";

            if (_resources.TryGetValue(fullPath, out Object cachedResource))
                return cachedResource as T;

            T loadedResource = UnityEngine.Resources.Load<T>(path);
            if (loadedResource is not null)
                _resources.Add(fullPath, loadedResource);

            return loadedResource;
        }

        void LoadAll()
        {
            if (_settings.MainResourcePath is not null)
            {
                foreach (var path in _settings.PrefabPaths) LoadResource<GameObject>(path);

                foreach (var path in _settings.SpritePaths) LoadResource<Sprite>(path);

                foreach (var path in _settings.AudioPaths) LoadResource<AudioClip>(path);
            }
        }
    }
}
