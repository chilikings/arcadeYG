using UnityEngine;
using GAME.Utils.Core;

namespace GAME.Settings.UI
{
    [CreateAssetMenu(fileName = Helper.UIName, menuName = Helper.SettingsMenu + Helper.UIName)]
    public class UISettings : ScriptableObject
    {
        [SerializeField][Space(4)] bool _debugPanel;
        [SerializeField][Space(4)] bool _logging;

        public bool Logging => _logging;
        public bool IsDebug => _debugPanel;
    }
}
