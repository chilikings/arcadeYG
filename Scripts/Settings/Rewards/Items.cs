using System;
using UnityEngine;
using GAME.Utils.Core;

namespace GAME.Settings.Rewards.Items
{
    [CreateAssetMenu(fileName = Helper.ItemsName, menuName = Helper.RewardsPath + Helper.ItemsName)]
    public class ItemsSettings : RewardSettings<Item>
    {
        public Sprite GetSprite(int id) => _items[id].Sprite;
    }

    [Serializable]
    public record Item : Reward
    {
        [SerializeField] Sprite _sprite;

        public Sprite Sprite => _sprite;
    }
}