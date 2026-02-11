using YG;
using TMPro;
using GAME.Buffs;
using UnityEngine;
using GAME.Audio.SFX;
using GAME.Utils.Core;
using GAME.Managers.AD;
using GAME.Player.Trail;
using GAME.Managers.Game;
using System.Collections;
using GAME.Managers.Audio;
using GAME.Managers.Saves;
using GAME.Player.Joystick;
using GAME.Settings.Levels;
using GAME.Settings.Rewards;
using Buff = GAME.Buffs.Buff;
using UnityEngine.InputSystem;
using GAME.Extensions.Animation;
using GAME.Extensions.Colliders;
using GAME.Settings.Rewards.Items;
using GAME.Settings.Rewards.Lives;
using GAME.Settings.Rewards.Skins;

namespace GAME.Player.Controller
{
    enum InputAxis { O, X, Y, XY }

    [RequireComponent(typeof(PlayerInput), typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        //[SerializeField] InputAction movementAction;
        [Header("GENERAL")]
        [SerializeField][Range(10, 50)] float _maxSpeed = 20f;
        [Header("DESKTOP")]
        [SerializeField][Range(0.5f, 1.5f)] float _deskSpeedFactor = 1;
        [SerializeField][Range(0, 0.3f)][Space(2)] float _accelerationTime = 0.15f;
        [SerializeField][Range(0, 0.3f)] float _decelerationTime = 0.1f;
        [Header("VISUALS")]
        [SerializeField][Range(2, 4)] float _baseSize = 2.5f;
        [SerializeField][Range(1, 1.1f)][Space(2)] float _pulseScale = 1.03f;
        [Header("SETTINGS")]
        [SerializeField] PlayerJoystick _joystick;
        [SerializeField][Space(2)] SkinsSettings _skins;
        [SerializeField] ItemsSettings _items;
        [SerializeField] LivesSettings _lives;
        [Header("DEBUG")]
        [SerializeField] bool _logging;

        const int _PulseStepsCount = 10;
        const string _BuffOn = "Buff On", _BuffOff = "Buff Off", _Item = "Item", _Halo = "Halo";
        const float _InputError = 0f, _SpeedError = 0.01f, _CollScale = 0.7f, _SkinScale = 1.25f, _ItemScale =  1.875f, _HaloScale = 1.875f, _FontScale = 3.875f;
        readonly Vector2 _VectorOne = Vector2.one;
        Vector2[] _respawnPoints;
        ADManager AD;
        GameManager GAME;
        SaveManager SAVE;
        AudioManager AUDIO;
        PlayerInput _pi;
        TextMeshPro _tmp;
        SpriteRenderer _sr, _item, _halo;
        Sprite _spriteSheet;
        PlayerTrail _trail;
        Coroutine _moving, _buffing;
        GameObject _go;
        Transform _tr;
        Buff _buff;
        Animator _an;
        PolygonCollider2D _cl;
        Vector2 _moveInput, _currentSpeed, _joystickCenter, _boundsMin, _boundsMax, _skinSize, _itemSize, _haloSize, _skinSizeStep, _itemSizeStep, _haloSizeStep;
        Vector2? _respawnOffset;
        bool? _isInited, _isSetup;
        bool _isActive, _isAlive;
        float _acceleration, _deceleration, _speedFactor, _collSize, _fontSize, _halfSize, _fontSizeStep;
        string _number;
        int _livesCount; //,_frameNumber;

        bool IsInput => _moveInput.magnitude > _InputError;
        bool IsMoving => _currentSpeed.magnitude > _SpeedError;


        public void Enable() => gameObject.SetActive(true);
        public void Disable() => gameObject.SetActive(false);

        public void ApplyLevelInfo(LevelInfo level)
        {
            _isInited ??= Initialize();
            ResetSize();
            _respawnOffset ??= new Vector2(_skinSize.y, _skinSize.y);
            var fieldPos = (Vector2)Helper.Field.position;
            var respawnPos = 0.5f * level.Size + _respawnOffset.Value;
            _respawnPoints = new Vector2[4] { respawnPos * Vector2.left + fieldPos, respawnPos * Vector2.up + fieldPos,
                                              respawnPos * Vector2.down + fieldPos, respawnPos * Vector2.right + fieldPos };
        }

        public void Respawn()
        {
            _isInited ??= Initialize();
            _isSetup ??= ApplySettings();
            if (!_isAlive)
            {
                _isAlive = true;
                return;
            }
            Deactivate();
            ResetSize();
            ResetBuff();       
            _an.PlaySpawn();
            _trail.Respawn();
            ChangeLivesCount();
            AUDIO.PlaySFX(SFXName.Respawn);
        }

        public void OnMove(InputValue input)
        {
            if (GAME.IsMobile) return;
            if (_logging) Debug.Log("Action -> OnMove");
            _moveInput = input.Get<Vector2>();
            _moving ??= StartCoroutine(Moving());
        }

        public void SetBounds(Bounds bounds)
        {
            _boundsMin = bounds.min;
            _boundsMax = bounds.max;      
        }

        public void Deactivate()
        {
            if (_logging) Debug.Log("Deactivate");
            Helper.StopCoroutine(ref _moving, this);
            _currentSpeed = _moveInput = Vector2.zero;
            _pi.DeactivateInput();
            _isActive = false;
        }

        public void SetSprite(Sprite sprite) => _sr.sprite = sprite;

        public void LoseHeart()
        {
            ChangeLivesCount(-1);
            if (_livesCount < 0)
            {
                _livesCount = 0;
                _isAlive = false;
                Despawn();
            }
            else Despawn();

            if (_logging) Helper.Log(() => _livesCount);
        }

        public void ChangeLivesCount(int diffCount = 0)
        {
            if (_logging) Debug.Log($"ChangeHeartsCount {diffCount}");
            int oldLivesCount = _livesCount, newLivesCount = oldLivesCount + diffCount;
            if (oldLivesCount != newLivesCount) SetLivesCount(newLivesCount);       
        }

        void SetLivesCount(int newLivesCount)
        {
            GAME.SetLivesCount(newLivesCount);
            _livesCount = Mathf.Clamp(newLivesCount, 0, Helper.MaxLivesCount);
        }

        public void ApplyHearts(int id)
        {
            AUDIO.PlayReward();
            ChangeLivesCount(GetHearts(id));
        }

        public void ApplySkin(int id)
        {
            AUDIO.PlayReward();
            SetColor(GetColor(id));
            SAVE.SaveSkin(id);
        }

        public void ApplyItem(int id)
        {
            AUDIO.PlayReward();
            SetItem(GetItem(id));
            SAVE.SaveItem(id);
        }

        public void SetNumber(string text) => _number = text;

        void Update()
        {
            if (GAME.IsMobile && _isAlive && _isActive)
            {
                if (_logging) Debug.Log("Update -> Touch");
                //_moveInput = CalcTouchInput(); // ! ! ! ! ! !
                _moveInput = _joystick.Direction;
                _moving ??= StartCoroutine(Moving());
            }
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (!_isActive) return;
            if ((collider.IsEnemy() || collider.IsKinEnemy()) && !(_buff?.Type is BuffType.Shield or BuffType.Berserk))
                LoseHeart();
            else if (collider.IsBuff())
                SetBuff(collider.GetComponent<Buff>());
        }

        void OnValidate()
        {
            _sr ??= GetComponent<SpriteRenderer>();
            _cl ??= GetComponent<PolygonCollider2D>();
            _tmp ??= GetComponentInChildren<TextMeshPro>();
            _item ??= transform.Find(_Item)?.GetComponent<SpriteRenderer>();
            _halo ??= transform.Find(_Halo)?.GetComponent<SpriteRenderer>();
            SetAllSizes(_baseSize);
        }

        IEnumerator Moving()
        {
            while (IsInput || IsMoving)
            {
                var deltaTime = Time.deltaTime;
                _currentSpeed = ChangeSpeed(_moveInput, _currentSpeed, _maxSpeed, _speedFactor, deltaTime);
                Move(deltaTime);
                yield return null;
            }
            _moving = null;
        }

        IEnumerator Buffing()
        {
            EnableBuff();
            FadeHaloIn();
            yield return new WaitForSeconds(_buff.Duration);
            FadeHaloOut();
            AUDIO.PlaySFX(SFXName.BuffOff);
        }

        bool Initialize()
        {
            InitComponents();
            InitFields();
            Enable();
            return true;
        }

        void InitComponents()
        {
            GAME = GameManager.I;
            SAVE = SaveManager.I;
            AUDIO = AudioManager.I;
            AD = ADManager.I;
            _go = gameObject;
            _tr = transform;
            _an = GetComponent<Animator>();
            _pi = GetComponent<PlayerInput>();
            _sr ??= GetComponent<SpriteRenderer>();
            _cl ??= GetComponent<PolygonCollider2D>();
            _trail = GetComponentInChildren<PlayerTrail>();
            _tmp ??= GetComponentInChildren<TextMeshPro>();
            _item ??= transform.Find(_Item)?.GetComponent<SpriteRenderer>();
            _halo ??= transform.Find(_Halo)?.GetComponent<SpriteRenderer>();
        }

        void InitFields()
        {
            if (!GAME.IsMobile)
            {
                _maxSpeed *= _deskSpeedFactor;
                _acceleration = _maxSpeed / _accelerationTime;
                _deceleration = _maxSpeed / _decelerationTime;
            }
            SetAllSizes(_baseSize);
            _tmp.text = _number;
            _speedFactor = 1;
            _isAlive = true;
        }

        bool ApplySettings()
        {
            _livesCount = SAVE.LoadLives();
            SetColor(GetColor(SAVE.SkinID));
            SetItem(GetItem(SAVE.ItemID));
            return true;
        }

        void SetAllSizes(float baseSize)
        {
            (_collSize, _halfSize, _fontSize, _skinSize, _itemSize, _haloSize) = CalcAllSizes(baseSize);
            SetPulseSteps(_fontSize, _skinSize, _itemSize, _haloSize);
            SetColliderSize(_collSize);
            _tmp.fontSize = _fontSize;
            _item.size = _itemSize;
            _halo.size = _haloSize;
            _sr.size = _skinSize;
        }

        void SetPulseSteps(float fontSize, Vector2 skinSize, Vector2 itemSize, Vector2 haloSize)
        {
            (_fontSizeStep, _skinSizeStep, _itemSizeStep, _haloSizeStep) = CalcPulseSteps(fontSize, skinSize, itemSize, haloSize);
        }

        (float collSize, float halfSize, float fontSize, Vector2 skinSize, Vector2 itemSize, Vector2 haloSize) CalcAllSizes(float baseSize)
        {
            Vector2 baseVector = baseSize * _VectorOne, skinSize = baseVector * _SkinScale, itemSize = baseVector * _ItemScale, haloSize = baseVector * _HaloScale;
            float collSize = baseSize * _CollScale, fontSize = baseSize * _FontScale, halfSize = skinSize.x / 2;
            return (collSize, halfSize, fontSize, skinSize, itemSize, haloSize);
        }

        (float fontStep, Vector2 skinStep, Vector2 itemStep, Vector2 haloStep) CalcPulseSteps(float fontSize, Vector2 skinSize, Vector2 itemSize, Vector2 haloSize)
        {
            var pulseMult = (_pulseScale - 1) / _PulseStepsCount;
            return (fontSize * pulseMult, skinSize * pulseMult, itemSize * pulseMult, haloSize * pulseMult);
        }

        Vector2 ChangeSpeed(Vector2 moveInput, Vector2 currentSpeed, float maxSpeed, float speedFactor, float deltaTime) => GAME.IsMobile ? maxSpeed * speedFactor * moveInput: 
                            Vector2.MoveTowards(currentSpeed, maxSpeed * speedFactor * moveInput, deltaTime * speedFactor * (IsInput ? _acceleration : _deceleration));
        
        void Move(float deltaTime)
        {
            _tr.Translate(_currentSpeed * deltaTime);
            _tr.position = ClampPosition(_tr.position, _boundsMin, _boundsMax, _halfSize);
        }

        void SetSpawnPosition()
        {
            _tr.position = GetRespawnPoint(_tr.position, _respawnPoints);
            _tr.position = ClampPosition(_tr.position, _boundsMin, _boundsMax, _halfSize);
        }

        Vector2 ClampPosition(Vector2 pos, Vector2 min, Vector2 max, float size) => new Vector2(Mathf.Clamp(pos.x, min.x + size, max.x - size), 
                                                                                                Mathf.Clamp(pos.y, min.y + size, max.y - size));
            
        Vector2 GetRespawnPoint(Vector2 position, Vector2[] points)
        {
            var minDistance = Mathf.Infinity;
            int pointIndex = 0, pointsCount = points.Length;
            for (int index = 0; index < pointsCount; index++)
            {
                var distance = Vector2.Distance(position, points[index]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    pointIndex = index;
                }
            }
            return points[pointIndex];
        }

        void SetColliderSize(float size)
        {
            float a = size * 0.5f, b = a / Mathf.Sqrt(2f);
            _cl ??= GetComponent<PolygonCollider2D>();
            _cl.points = new Vector2[] {
                new(-a, b), new(-b, a), new(b, a), new(a, b),
                new(a, -b), new(b, -a), new(-b, -a), new(-a, -b)};
        }

        void SetBuff(Buff buff)
        {
            ResetBuff();
            _buff = buff;
            _halo.sprite = buff.Halo;
            _buffing ??= StartCoroutine(Buffing());
        }

        void ResetBuff()
        {
            DisableBuff();
            Helper.StopCoroutine(ref _buffing, this);
            _halo.sprite = null;
            _buff = null;
        }

        void EnableBuff()
        {
            switch (_buff?.Type)
            {
                case BuffType.Boost:
                    _speedFactor = _buff.Value;
                    AUDIO.PlaySFX(SFXName.Boost);
                    break;
                case BuffType.Berserk:
                    AUDIO.PlaySFX(SFXName.Berserk);
                    SetColliderSize(_baseSize * _buff.Value);
                    GAME.SetBerserk(true);
                    break;
                case BuffType.Slowmo:
                    //if (!reset)
                    GAME.SetEnemySpeed(_buff.Value);
                    AUDIO.PlaySFX(SFXName.Slomo);
                    break;
                default:
                    AUDIO.PlaySFX(SFXName.BuffOn);
                    break;
                    //case BuffType.Shield:
                    //    _trail.SetShield(enable);
                    //    break;
            }
        }

        void DisableBuff()
        {
            switch (_buff?.Type)
            {
                case BuffType.Boost:
                    _speedFactor = 1;
                    break;
                case BuffType.Berserk:
                    SetColliderSize(_baseSize);
                    GAME.SetBerserk(false);
                    break;
                case BuffType.Slowmo:
                    GAME.SetEnemySpeed(1);
                    break;
                    //case BuffType.Shield:
                    //    _trail.SetShield(enable);
                    //    break;
            }
        }

        void SetColor(Color color)
        {
            _sr.color = color;
            _trail.SetColor(color);
        }

        void DecreaseSize()
        {
            _sr.size -= _skinSizeStep;
            _item.size -= _itemSizeStep;
            _halo.size -= _haloSizeStep;
            _tmp.fontSize -= _fontSizeStep;
        }

        void IncreaseSize()
        {
            _sr.size += _skinSizeStep;
            _item.size += _itemSizeStep;
            _halo.size += _haloSizeStep;
            _tmp.fontSize += _fontSizeStep;
        }

        void ResetSize()
        {
            SetAllSizes(_baseSize);
        }

        void SetItem(Sprite sprite) => _item.sprite = sprite;

        Color GetColor(int id) => _skins.GetColor(id);

        Sprite GetItem(int id) => _items.GetSprite(id);

        int GetHearts(int id) => _lives.GetCount(id);

        void FadeHaloIn() => _an.SetTrigger(_BuffOn);
        void FadeHaloOut() => _an.SetTrigger(_BuffOff);

        void Activate()
        {
            _isActive = true;
            _pi.ActivateInput();
        }

        void Despawn()
        {
            Deactivate();
            _an.PlayDeath();
            _trail.SetDeath();
            ResetBuff();
            AUDIO.PlaySFX(SFXName.Despawn);
        }

    }
}