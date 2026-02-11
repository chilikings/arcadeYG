using System;
using UnityEngine;
using GAME.Utils.Core;
using GAME.Buffs.Spawn;
using GAME.Level.Field;
using GAME.Audio.Music;
using GAME.Enemies.Spawn;
using GAME.Settings.Level.Images;
using System.Collections.Generic;

namespace GAME.Settings.Levels
{
    [CreateAssetMenu(fileName = Helper.LevelsName, menuName = Helper.SettingsMenu + Helper.LevelsName)]
    public class LevelSettings : ScriptableObject
    {
        [SerializeField][Space(2)][Range(0, 1)] float _winDelaySec;
        [SerializeField][Space(4)] List<LevelInfo> _levels = new();

        public float WinDelay => _winDelaySec;
        public int Count => _levels.Count;

        public LevelInfo GetLevel(int index) => index >= 0 && index < Count ? _levels[index] : null;

        void OnValidate() => _levels.ForEach(l => l.Reset());
    }

    [Serializable]
    public record LevelInfo
    {
        [SerializeField] Vector2Int _size;
        [SerializeField] FieldShape _shape;
        [SerializeField][Space(4)] PictureName _picture;
        [SerializeField] TextureName _texture;
        [SerializeField] BackgroundName _background;
        [SerializeField][Space] TrackName _track;
        [SerializeField] EnemySpawnInfo[] _enemies;
        [SerializeField] BuffSpawnInfo[] _buffs;
        [NonSerialized] Vector2[] _shapePoints;

        public FieldShape Shape => _shape;
        public Vector2[] ShapePoints => _shapePoints ??= Helper.GetShapePoints(_shape, _size); 
        public Vector2 Size => _size;
        public PictureName Picture => _picture;
        public TextureName Texture => _texture;
        public BackgroundName Background => _background;
        public EnemySpawnInfo[] Enemies => _enemies;
        public BuffSpawnInfo[] Buffs => _buffs;
        public TrackName Track => _track;

        public void Reset()
        {
            _shapePoints = null;
            //_size = new Vector2Int((int)_foreground.Size.x, (int)_foreground.Size.y);
        }
    }

}