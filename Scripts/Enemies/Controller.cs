using UnityEngine;
using System.Linq;
using GAME.Audio.SFX;
using GAME.Utils.Core;
using GAME.Enemies.Disk;
using GAME.Enemies.Ammo;
using System.Collections;
using GAME.Enemies.Trail;
using GAME.Managers.Game;
using GAME.Managers.Level;
using GAME.Managers.Audio;
using GAME.Utils.Conversion;
using GAME.Enemies.Satellite;
using GAME.Extensions.Objects;
using GAME.Utils.Randomization;
using GAME.Extensions.Animation;
using GAME.Extensions.Colliders;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace GAME.Enemies.Controller
{
    public enum EnemyType { Simple, Hedgehog, Planet, Trailer, Shooter, Sniper }
    public enum MoveType { Static, Bounce, Turning }

    public class EnemyController : MonoBehaviour
    {
        [Header("BASICS")]
        [SerializeField] EnemyType _type;
        [SerializeField][Range(0.1f, 2)] float _colliderRadius;
        [SerializeField][Space(2)] Vector2 _scaleRange;
        [Header("MOVEMENT")]
        [SerializeField] MoveType _moveType;
        [SerializeField] float _speed = 5f;
        [Space(2)]
        [SerializeField] Vector2 _turnRange;
        [SerializeField] Vector2 _turnInterval;
        [Header("SHOOTING")]
        [SerializeField] Transform _ammoPrefab;
        [SerializeField][Range(1, 5)][Space(4)] int _shotInterval = 3;
        [SerializeField][Range(0, 1)][Space(2)] float _shotJitter = 0.5f;
        [Header("OTHERS")]
        [SerializeField] List<SatelliteInfo> _satellites;
        [SerializeField][Space] bool _logging;


        List<EnemyAmmo> _ammoPool = new();
        List<EnemySatellite> _Satellites;
        GameManager GAME;
        AudioManager AUDIO;
        Animator _an;
        EnemyTrail _trail;
        EnemyDisk _disk;
        GameObject _go;
        Transform _tr;
        Rigidbody2D _rb;
        Coroutine _turning, _shooting;
        SpriteRenderer _sr;
        CircleCollider2D _cl, ccl;
        Vector2 _moveDirection;
        readonly Vector2 _VectorOne = Vector2.one;
        const int _DeathFramesCount = 14;
        const float _SpriteSizeScale = 2.5f, _DeathSizeScale = 1.5f;
        float? _sizeScale;
        float _speedFactor = 1, _scaledRadius, _scaledSize, _animSizeStep;
        int _totalCooldown;
        bool? _isInited, _isSetup;
        bool _shootOutside, _isInsideField, _isAlive;
        int _frameNumber;

        public Vector2 Position { get => _tr.position; set => _tr.position = value; }
        public EnemyType Type => _type;
        public MoveType Move => _moveType;


        public EnemyController Respawn(Transform spawner, float offset, float distStatic)
        {
            _isInited ??= Initialize();
            if (_tr.IsPrefab())
            {
                var enemy = Instantiate(_tr, Rand.GetFieldPoint(offset), Quaternion.identity, spawner).GetComponent<EnemyController>();
                enemy.Respawn(spawner, offset, distStatic);
                return enemy;
            }
            else
            {
                Reset();
                _isSetup ??= SetupEnemy(_type, _tr);
                _moveDirection = SetupMovement(_moveType, _speed, _speedFactor, ccl, _turnRange, _turnInterval);
                SetPosition(_tr, offset, distStatic);
                _Satellites?.ForEach(ss => ss.Spawn());
                _disk?.Launch();
                return this;
            }
        }

        public void SetSpeedFactor(float factor)
        {
            _speedFactor = factor;
            SetVelocity(_speed, _speedFactor, _moveDirection);
            _Satellites?.ForEach(ss => ss.SetSpeedFactor(factor));
        }

        public void StartDeath()
        {
            SwitchPhysics(false);
            _an.PlayDeath();
            AUDIO.PlaySFX(SFXName.Kill);
            if (_type == EnemyType.Planet) _Satellites.ForEach(s => s.StartDeath());
            if (_type == EnemyType.Hedgehog) _disk?.StartDeath();
            _isAlive = false;
        }

        public void DisableFull()
        {
            Disable();
            _trail?.Reset();
            _trail?.Disable();
            DisableAmmo();
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.IsFieldEdge()) AUDIO.PlaySFX(SFXName.Bounce);
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (!_isAlive) return;
            if (collider.IsPlayer() && GAME.IsBerserk || !_isInsideField && collider.IsZonePolygon())
                StartDeath();
            else if (_moveType == MoveType.Turning && collider.IsFieldEdge())
                RestartTurning(_speed, _turnRange, _turnInterval, ccl);
        }

        void OnTriggerStay2D(Collider2D collider)
        {
            if (!_isAlive) return;
            if (!_isInsideField && collider.IsZonePolygon()) StartDeath();
            else if (_moveType == MoveType.Turning && collider.IsFieldEdge())
                RestartTurning(_speed, _turnRange, _turnInterval, ccl);
        }

        void OnTriggerExit2D(Collider2D collider)
        {
            if (!_isAlive) return;
            if (_isInsideField && collider.IsFieldPolygon()) _isInsideField = false;
        }

        void FixedUpdate()
        {
            _frameNumber++;
        }

        bool Initialize()
        {
            InitComponents();
            InitFields();
            return true;
        }

        void InitComponents()
        {
            AUDIO = AudioManager.I;
            GAME = GameManager.I;
            _go = gameObject;
            _tr = transform;
            _rb = GetComponent<Rigidbody2D>();
            _an = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();
            _cl ??= GetComponent<CircleCollider2D>();    
            ccl ??= GetComponentsInChildren<CircleCollider2D>().Single(c => c.isTrigger && c.transform != _tr);
            GAME.OnEnemyDebuff.AddListener(SetSpeedFactor);
        }

        void InitFields()
        {
            _shootOutside = _type == EnemyType.Sniper;
            (_scaledRadius, _scaledSize, _animSizeStep) = CalcSizes(ref _sizeScale, _scaleRange, _colliderRadius);
        }

        void Reset()
        {
            Enable();
            SwitchPhysics(true);
            SetSpeedFactor(1);
            _isAlive = _isInsideField = true;
            if (_moveType == MoveType.Turning) Helper.StopCoroutine(ref _turning, this);     
            SetColliderRadius(_scaledRadius);
            SetSpriteSize(_scaledSize);
            ResetItems(_type);
        }

        void ResetItems(EnemyType type)
        {
            switch (type)
            {
                case EnemyType.Sniper:
                case EnemyType.Shooter:
                    Helper.StopCoroutine(ref _shooting, this);
                    _shooting ??= StartCoroutine(Shooting());
                    DisableAmmo();
                    break;
                case EnemyType.Trailer: _trail?.Reset(); break;
                case EnemyType.Hedgehog: _disk?.Reset(); break;
            }
        }

        (float scaledRadius, float scaledSize, float animSizeStep) CalcSizes(ref float? sizeScale, Vector2 scaleRange, float colliderRadius)
        {
            sizeScale = Random.Range(scaleRange.x, scaleRange.y);
            float scaledRadius = colliderRadius * sizeScale.Value, scaledSize = scaledRadius * _SpriteSizeScale,
                  animSizeStep = scaledSize * (_DeathSizeScale - 1) / _DeathFramesCount;
            return (scaledRadius, scaledSize, animSizeStep);
        }

        void SetColliderRadius(float radius) => _cl.radius = radius;

        void SetSpriteSize(float size) => _sr.size = size * _VectorOne;

        void IncreaseSprite() => _sr.size += _animSizeStep * _VectorOne;

        List<EnemySatellite> InitSatellites(List<SatelliteInfo> satellites, Transform planet)
        {
            List<EnemySatellite> spawnedSatellites = new();
            foreach (var satelliteInfo in satellites)
            {
                var satellite = Instantiate(satelliteInfo.Prefab).GetComponent<EnemySatellite>();
                satellite.Init(planet, satelliteInfo.Radius, satelliteInfo.Speed, satelliteInfo.Clockwise);
                spawnedSatellites.Add(satellite);
            }
            return spawnedSatellites;
        }

        void RestartTurning(float speed, Vector2 range, Vector2 interval, Collider2D collider) => Helper.RestartCoroutine(ref _turning, () => Turning(speed, range, interval, collider), this);

        IEnumerator Turning(float speed, Vector2 range, Vector2 interval, Collider2D collider)
        {
            while (true)
            {
                _moveDirection = CalcFreeDirection(_moveDirection, range, collider);
                SetVelocity(speed, _speedFactor, _moveDirection);
                collider.transform.rotation = Conv.VectorToQuater(_moveDirection);
                var waitTime = Random.Range(interval.x, interval.y);
                yield return new WaitForSeconds(waitTime);
            }
        }

        IEnumerator Shooting()
        {
            while (true)
            {
                var randInterval = _shotInterval + Random.Range(-_shotJitter, _shotJitter);
                yield return new WaitForSeconds(randInterval);
                if (!_shootOutside && !Helper.IsPlayerInsideField) continue;

                var dirToPlayer = (Helper.Player.position - _tr.position).normalized;
                var ammo = _ammoPool.FirstOrDefault(a => !a.gameObject.activeInHierarchy);
                if (ammo is null)
                {
                    var ammoObj = Instantiate(_ammoPrefab, _tr.position, Quaternion.identity, _tr);
                    ammo = ammoObj.GetComponent<EnemyAmmo>();
                    _ammoPool.Add(ammo);
                }
                ammo.Shoot(_tr.position, dirToPlayer, !_shootOutside);
            }
        }

        Vector2 CalcFreeDirection(Vector2 moveDirection, Vector2 redirRange, Collider2D checkCollider, int tryCount = 50)
        {
            Vector2 offset = checkCollider.offset, newPosition;
            float newAngle, diffAngle, currAngle = Conv.VectorToPosAngle(moveDirection), checkRadius = (checkCollider as CircleCollider2D).radius;
            for (int i = 0; i < tryCount; i++)
            {
                diffAngle = Random.Range(redirRange.x, redirRange.y) * (Random.value < 0.5f ? -1 : 1);
                newAngle = currAngle + diffAngle;
                newPosition = (Vector2)(_tr.position + Conv.DegToQuater(newAngle) * offset);
                if (_logging) Debug.Log($"[{i}] NewAngle = {currAngle} | diffAngle = {diffAngle}");
                if (!IsCircleCollided(newPosition, checkRadius, Helper.FieldMask))
                {
                    if (_logging) Debug.Log($"Tries: {i}");
                    return Conv.DegToVector(newAngle);
                }
            }
            if (_logging) Debug.LogWarning("Opposite");
            return -moveDirection;
        }

        bool IsCircleCollided(Vector2 position, float radius, int layerMask)
        {
            var colliders = Physics2D.OverlapCircleAll(position, radius, layerMask);
            var collision = false;
            foreach (var collider in colliders)
                if (collider is EdgeCollider2D)
                {
                    collision = true;
                    break;
                }
            return collision;
        }

        bool SetupEnemy(EnemyType type, Transform transform)
        {
            if (!transform) return false;
            switch (type)
            {
                case EnemyType.Hedgehog: 
                    _disk ??= GetComponentInChildren<EnemyDisk>();
                    _disk?.Init(_scaledRadius);
                    break;
                case EnemyType.Trailer:
                    _trail ??= GetComponentInChildren<EnemyTrail>();
                    _trail?.SetEnemy(transform);
                    break;
                case EnemyType.Planet: _Satellites ??= InitSatellites(_satellites, transform); break;
            }
            return true;
        }

        Vector2 SetupMovement(MoveType type, float speed, float factor, CircleCollider2D checkCollider, Vector2 turnRange, Vector2 turnInterval)
        {
            if (type == MoveType.Static) return Vector2.zero;
            var direction = Helper.RandomDirection;
            SetVelocity(speed, factor, direction);
            if (type == MoveType.Turning)
            {            
                checkCollider.enabled = true;
                RestartTurning(speed, turnRange, turnInterval, checkCollider);
            }
            else checkCollider.enabled = false;
            return direction;
        }

        void SetPosition(Transform transform, float offset, float distStatic)
        {
            if (!transform) return;
            if (_moveType == MoveType.Static)
                transform.position = Rand.FindFreePoint(distStatic, offset);
            else
                transform.position = Rand.GetFieldPoint(offset);
        }

        void DisableAmmo()
        {
            if (_ammoPool is null) return;
            _ammoPool.ForEach(a => a.Disable());
        }

        void SwitchPhysics(bool enabled) => _cl.enabled = _rb.simulated = enabled;
       
        void CompleteDeath()
        {
            DisableFull();
            LevelManager.I.KillEnemy();
        }

        void SetVelocity(float speed, float factor, Vector2 direction) => _rb.linearVelocity = speed * factor * direction;

        void Enable() => _go.SetActive(true);     
        void Disable() => _go.SetActive(false);

    }

}