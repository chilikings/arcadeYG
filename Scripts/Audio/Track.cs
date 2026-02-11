using System;
using UnityEngine;

namespace GAME.Audio.Music
{
    [Serializable]
    public class Track
    {
        [SerializeField] AudioClip _clip;
        [SerializeField] TrackName _name;
        [SerializeField][Range(0, 1)] float _volume = 1;

        public TrackName Name => _name;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
    }

    public enum TrackName
    {
        Track1, Track2, Track3, Track4, Track5, Track6, Track7, Track8, Track9, Track10
    }

}
