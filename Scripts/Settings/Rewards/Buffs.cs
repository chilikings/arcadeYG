using System;
using UnityEngine;
using GAME.Utils.Core;

namespace GAME.Settings.Rewards.Buffs
{
    [CreateAssetMenu(fileName = Helper.BuffsName, menuName = Helper.RewardsPath + Helper.BuffsName)]
    public class BuffsSettings : RewardSettings<Buff>
    {

    }

    [Serializable]
    public record Buff : Reward
    {

    }
}