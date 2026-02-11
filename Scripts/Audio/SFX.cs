using System;
using UnityEngine;

namespace GAME.Audio.SFX
{
    [Serializable]
    public class SFX
    {
        [SerializeField] SFXGroup _group;
        [SerializeField] SFXName _name;
        [SerializeField] AudioClip _clip;
        [SerializeField][Range(0, 1)] float _volume = 1;

        public SFXName Name => _name;
        public SFXGroup Group => _group;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
    }

    [Serializable]
    public class SFXSource
    {
        [SerializeField] AudioSource _source;
        [SerializeField] SFXGroup _group;

        public AudioSource Source => _source;
        public SFXGroup Group => _group;
    }

    public enum SFXName
    {
        Win, Lose, Respawn, Despawn, Cut, Shot, Kill, Smash, Bounce, Ricochet, BuffOn, BuffOff,
        Boost, Slomo, Berserk, Click, Toggle, Reward
    }

    public enum SFXGroup { Level, Player, Enemy, Buff, UI }

}
