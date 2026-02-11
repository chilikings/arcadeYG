using System;
using UnityEngine;
using System.Linq;
using GAME.Settings.Levels;
using GAME.Enemies.Controller;
using System.Collections.Generic;

namespace GAME.Enemies.Spawn
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField][Space(2)] Transform _simple;
        [SerializeField] Transform _hedgehog;
        [SerializeField] Transform _planet;
        [SerializeField] Transform _shooter;
        [SerializeField] Transform _trailer;
        [SerializeField] Transform _sniper;
        [SerializeField][Space(4)][Range(0, 2)] float _offset;
        [SerializeField][Range(0f, 20f)] float _minDistStatic = 2f;

        readonly EnemyType[] _EnemyTypes = { EnemyType.Simple, EnemyType.Hedgehog, EnemyType.Planet, EnemyType.Shooter, EnemyType.Trailer, EnemyType.Sniper };
        List<EnemySpawnInfo> _enemiesInfo = new();
        Dictionary<EnemyType, List<EnemyController>> _spawnedEnemies = new();
        GameObject _go;
        Transform _tr;
        bool? _isInited;


        public void Enable() => gameObject.SetActive(true);
        public void Disable() => gameObject.SetActive(false);
    
        public void ApplyLevelInfo(LevelInfo level)
        {
            _isInited ??= Initialize();
            _enemiesInfo = level.Enemies.ToList();
        }

        public void Restart()
        {
            _isInited ??= Initialize();
            RespawnAll(_spawnedEnemies, _enemiesInfo, _tr, _offset);
        }

        bool Initialize()
        {
            _go = gameObject;
            _tr = transform;
            return true;
        }

        void RespawnAll(Dictionary<EnemyType, List<EnemyController>> spawnedEnemies, List<EnemySpawnInfo> enemiesInfo, Transform transform, float offset)
        {
            foreach(var type in _EnemyTypes)
            {
                var typedEnemies = enemiesInfo.Where(ei => ei.Type == type);
                if (typedEnemies.Any())
                    foreach (var typedEnemy in typedEnemies)
                    {
                        if (!spawnedEnemies.ContainsKey(type)) spawnedEnemies[type] = new();
                        var enemyList = spawnedEnemies[type];
                        int reqCount = typedEnemy.Count;
                        for (int i = 0; i < enemyList.Count; i++)
                            if (i < reqCount) enemyList[i].Respawn(transform, offset, _minDistStatic);
                            else enemyList[i].DisableFull();

                        while (enemyList.Count < reqCount) enemyList.Add(SpawnEnemy(typedEnemy, transform, offset));
                    }
                else if (spawnedEnemies.ContainsKey(type)) spawnedEnemies[type].ForEach(se => se.DisableFull());    
            }

        }

        EnemyController SpawnEnemy(EnemySpawnInfo enemy, Transform transform, float offset) => GetEnemy(enemy.Type).GetComponent<EnemyController>().Respawn(transform, offset, _minDistStatic);

        Transform GetEnemy(EnemyType type) => type switch
        {
            EnemyType.Simple => _simple,
            EnemyType.Hedgehog => _hedgehog,
            EnemyType.Planet => _planet,
            EnemyType.Trailer => _trailer,
            EnemyType.Shooter => _shooter,
            EnemyType.Sniper => _sniper,
        };

    }

    [Serializable]
    public class EnemySpawnInfo
    {
        public EnemyType Type => _type;
        public int Count => _count;

        [SerializeField] EnemyType _type;
        [SerializeField] int _count;
    }
}