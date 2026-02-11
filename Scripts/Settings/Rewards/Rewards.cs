using System;
using UnityEngine;

namespace GAME.Settings.Rewards
{
    public enum RewardType { Heart, Buff, Skin, Item }
    public enum RewardContext { Start, Win, Lose, Click }

    [Serializable]
    public abstract record Reward
    {
        [SerializeField] protected string _name;
        [SerializeField] protected Sprite _icon;

        public string Name => _name;
        public Sprite Icon => _icon;
    }

    public interface IRewardList
    {
        Reward Get(int id);
        Sprite GetIcon(int id);
        int Count();
    }

    public abstract class RewardSettings<T> : ScriptableObject, IRewardList where T : Reward
    {
        [SerializeField] public T[] _items;

        public T Get(int id) => _items[id];
        Reward IRewardList.Get(int id) => Get(id);
        public Sprite GetIcon(int id) => _items[id].Icon;
        public int Count() => _items.Length;
    }

    [Serializable]
    public class RewardSlot
    {
        [SerializeField] RewardType _type;
        [SerializeField] int _ID;
        [SerializeField] bool _random;

        public RewardType Type => _type;
        public int ID => _ID;
        public bool IsRandom => _random;

    }

}