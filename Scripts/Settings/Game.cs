using UnityEngine;

namespace GAME.Settings.Game
{
    [CreateAssetMenu(fileName = "Game", menuName = "GAME/Settings/Game")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField][Space(4)] Platform _platform;
        [SerializeField][Space(4)] Localization _localization;
        [SerializeField][Range(0,5)][Space(4)] int _startLivesCount;


        public int StartLivesCount => _startLivesCount;
        public Platform Platform => _platform;
        public Localization Localization => _localization;
    }

    public enum Platform { Auto, Mobile, PC }

    public enum Localization { Auto, RU, EN }
}