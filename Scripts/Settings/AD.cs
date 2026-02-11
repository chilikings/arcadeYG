using UnityEngine;
using GAME.Utils.Core;

namespace GAME.Settings.AD
{
    [CreateAssetMenu(fileName = Helper.ADName, menuName = Helper.SettingsMenu + Helper.ADName)]
    public class ADSettings : ScriptableObject
    {
        [SerializeField][Space] bool _enabled;
        [Header("INTERVALS")]
        [SerializeField][Space(2)] bool _custom;
        [SerializeField][Range(0, 300)][Space(4)] int _interstitial = 40;
        [SerializeField][Range(0, 300)][Space(2)] int _rewarded = 0;
        

        public int InterInterval => _interstitial;
        public int RewardInterval => _rewarded;
        public bool Enabled => _enabled;
        public bool IsCustom => _custom;

    }
} 