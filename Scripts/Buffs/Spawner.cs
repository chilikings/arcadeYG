using System;
using UnityEngine;
using System.Linq;
using GAME.Utils.Core;
using System.Collections;
using GAME.Settings.Levels;
using GAME.Utils.Randomization;
using System.Collections.Generic;

namespace GAME.Buffs.Spawn
{
    public class BuffSpawner : MonoBehaviour
    {
        [SerializeField][Space(2)] Transform _boost;
        [SerializeField] Transform _slomo;
        [SerializeField] Transform _berserk;
        [SerializeField] Transform _shield;
        [SerializeField][Space(4)][Range(0, 2)] float _offset;
        [SerializeField][Range(0f, 20f)] float _minDistToStatic = 3f;


        List<BuffSpawnInfo> _buffsInfo = new();
        List<Buff> _spawnedBuffs = new();
        Coroutine _spawning;
        bool? _isInited;
        //GameObject _go;


        public void Enable() => gameObject.SetActive(true);     
        public void Disable() => gameObject.SetActive(false);
        
        public void ApplyLevelInfo(LevelInfo level) => _buffsInfo = level.Buffs.ToList();
        
        public void Restart()
        {
            StopAllCoroutines();
            foreach (var buff in _spawnedBuffs) buff.Disable();
            _spawnedBuffs.Clear();
            StartSpawning(_buffsInfo);
        }

        //bool Initialize()
        //{
        //    _go = gameObject;
        //    return true;
        //}

        void StartSpawning(List<BuffSpawnInfo> buffs)
        {
            if (!enabled) return;
            foreach (var buffInfo in buffs)
                if (buffInfo.Type > 0 && buffInfo.Enabled)
                {
                    var buff = Helper.GetScript<Buff>(GetBuff(buffInfo.Type), transform);
                    _spawnedBuffs.Add(buff);
                    StartCoroutine(Spawning(buffInfo, buff, transform, _offset));
                }
        }

        IEnumerator Spawning(BuffSpawnInfo buffInfo, Buff buff, Transform spawner, float offset)
        {
            yield return new WaitForSeconds(buffInfo.SpawnTime);
            Spawn(buff, spawner, offset);
            for (int respCount = 0; respCount < buffInfo.RespawnCount || buffInfo.RespawnCount == -1; respCount++)
            {
                yield return new WaitForSeconds(buffInfo.RespawnRate);
                Spawn(buff, spawner, offset);
            }
        }

        //void Spawn(Buff buff, Transform spawner, float offset) => buff.Spawn(Helper.CalcRndFieldPoint(offset), spawner);
        void Spawn(Buff buff, Transform spawner, float offset) => buff.Spawn(Rand.FindFreePoint(_minDistToStatic, offset), spawner);

        Transform GetBuff(BuffType type) => type switch
        {
            BuffType.None => null,
            BuffType.Boost => _boost,
            BuffType.Slowmo => _slomo,
            BuffType.Berserk => _berserk,
            BuffType.Shield => _shield
        };
    }

    [Serializable]
    public record BuffSpawnInfo
    {
        [SerializeField] BuffType _type;
        [SerializeField][Space(2)] bool _enabled;
        [SerializeField][Range(0, 60)][Space(4)] int _spawnTime;
        [SerializeField][Space(6)][Range(-1, 10)] int _respawnCount;
        [SerializeField][Range(1, 30)] int _respawnRate;

        public BuffType Type => _type;
        public bool Enabled => _enabled;
        public int SpawnTime => _spawnTime;
        public int RespawnRate => _respawnRate;
        public int RespawnCount => _respawnCount;
    }
}
