using System;
using UnityEngine;
using GAME.Utils.Core;

namespace GAME.Settings.Rewards.Lives
{
    [CreateAssetMenu(fileName = Helper.LivesName, menuName = Helper.RewardsPath + Helper.LivesName)]
    public class LivesSettings : RewardSettings<Live>
    {
        public int GetCount(int id) => _items[id].Count;
    }

    [Serializable]
    public record Live : Reward
    {
        [SerializeField] int _count;

        public int Count => _count;
    }
}