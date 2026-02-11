using System;
using System.Collections.Generic;

namespace GAME.Test
{
    [Serializable]
    public class PlayerData
    {
        public string playerName;
        public int level;
        public float health;
        public List<string> inventory;
    }

    [System.Serializable]
    public class EnemyData
    {
        public string enemyType;
        public float moveSpeed;
        public int damage;
        public float health;
    }

    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public string levelName;
        public int enemyCount;
        public float timeLimit;
    }

    [System.Serializable]
    public class GameSettingsData
    {
        public float musicVolume;
        public float soundVolume;
        public bool isFullscreen;
        public int screenResolutionWidth;
        public int screenResolutionHeight;
    }

    [System.Serializable]
    public class SaveGameData
    {
        public PlayerData playerInfo;
        //public List<ItemData> collectedItems;
        public int currentScore;
        public DateTime saveTime;
    }

}