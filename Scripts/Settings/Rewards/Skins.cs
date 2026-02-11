using System;
using UnityEngine;
using GAME.Utils.Core;

namespace GAME.Settings.Rewards.Skins
{
    [CreateAssetMenu(fileName = Helper.ItemsName, menuName = Helper.RewardsPath + Helper.ItemsName)]
    public class SkinsSettings : RewardSettings<Skin>
    {
        public Color GetColor(int id) => _items[id].Color;  
    }

    [Serializable]
    public record Skin : Reward
    {
        [SerializeField] Color _color;

        public Color Color => _color;
    }
}