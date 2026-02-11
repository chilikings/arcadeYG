using UnityEngine;
using GAME.Utils.Core;

namespace GAME.Settings.Assets
{
    [CreateAssetMenu(fileName = Helper.ResourcesName, menuName = Helper.SettingsMenu + Helper.ResourcesName)]
    public class ResourceSettings : ScriptableObject
    {
        [SerializeField] string _mainResourcePath;
        [SerializeField] string[] _prefabPaths;
        [SerializeField] string[] _spritePaths;
        [SerializeField] string[] _audioPaths;

        public string MainResourcePath => _mainResourcePath;
        public string[] PrefabPaths => _prefabPaths;
        public string[] SpritePaths => _spritePaths;
        public string[] AudioPaths => _audioPaths;
    }
}
