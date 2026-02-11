using UnityEngine;
using GAME.Audio.SFX;
using GAME.Utils.Core;
using System.Collections;
using GAME.Managers.Game;
using UnityEngine.Events;
using GAME.Managers.Audio;
using GAME.Extensions.Animation;
using GAME.Extensions.Colliders;

namespace GAME.Enemies.Disk
{
    public class EnemyDisk : MonoBehaviour
    {
        [Header("BASICS")]
        [SerializeField][Range(1, 5)] float _colliderRadius;
        [SerializeField][Space(2)] Vector2 _scaleRange;
        [Header("ANIMATION")]
        [SerializeField][Range(0, 10)] int _smallShapeDuration;
        [SerializeField][Range(0, 10)] int _bigShapeDuration;
        [SerializeField][Space] UnityEvent _onCollide;

        readonly Vector2 _VectorOne = Vector2.one; 
        Vector2 _spriteSizeRange, _colliderRadiusRange;
        GameManager GAME;
        AudioManager AUDIO;
        CircleCollider2D _cl;
        SpriteRenderer _sr;
        Animator _an;
        GameObject _go;
        Transform _tr;
        Coroutine _pulsating;
        const string _Decrease = "Decrease", _Increase = "Increase", _SmallShape = "Small", _BigShape = "Big";
        const int _AnimFramesCount = 21;
        const float _SpriteSizeScale = 2.2f;//, _ColliderScale = 0.455f;
        float _sizeScale, _animSizeStep, _animRadiusStep;
        bool _isAlive;
        bool? _isInited, _isSmallStart;


        public void Launch()
        {
            Helper.StopCoroutine(ref _pulsating, this);
            _isSmallStart = Helper.TryChance50();
            _pulsating ??= StartCoroutine(Pulsating(_an, _isSmallStart.Value));
            _cl.enabled = _isAlive = true;
        }    

        public void Init(float coreRadius)
        {
            _isInited ??= InitComponents();

            _colliderRadiusRange.x = coreRadius;
            _spriteSizeRange.x = coreRadius * _SpriteSizeScale;
            _sizeScale = Random.Range(_scaleRange.x, _scaleRange.y);
            _colliderRadiusRange.y = _colliderRadius * _sizeScale;
            _spriteSizeRange.y = _colliderRadiusRange.y * _SpriteSizeScale;

            _animSizeStep = (_spriteSizeRange.y - _spriteSizeRange.x) / _AnimFramesCount;
            _animRadiusStep = (_colliderRadiusRange.y - _colliderRadiusRange.x) / _AnimFramesCount;
        }

        public void StartDeath()
        {
            Helper.StopCoroutine(ref _pulsating, this);
            _an.PlayDeath();
            _cl.enabled = _isAlive = false;
        }

        public void Reset()
        {
            Helper.StopCoroutine(ref _pulsating, this);
            _cl.enabled = _isAlive = true;
            //_isSmallStart = Helper.TryChance50();
        }

        bool InitComponents()
        {
            _go = gameObject;
            _tr = transform;
            _an = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();
            _cl = GetComponent<CircleCollider2D>();
            AUDIO = AudioManager.I;
            GAME = GameManager.I;
            return true;
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (!_isAlive) return;
            if (collider.IsPlayer() && GAME.IsBerserk) _onCollide?.Invoke();
            
        }

        //void Awake() 
        //{
        //    GameManager.I.OnEnemyDebuff.AddListener(SetSpeedFactor);
        //}

        IEnumerator Pulsating(Animator animator, bool smallStart)
        {
            while (smallStart)
            {
                animator.SetTrigger(_SmallShape);
                yield return new WaitForSeconds(_smallShapeDuration);
                yield return animator.PlayAndWait(_Increase);
                yield return new WaitForSeconds(_bigShapeDuration);
                yield return animator.PlayAndWait(_Decrease);
            }
            while (!smallStart)
            {
                animator.SetTrigger(_BigShape);
                yield return new WaitForSeconds(_bigShapeDuration);
                yield return animator.PlayAndWait(_Decrease);
                yield return new WaitForSeconds(_smallShapeDuration);
                yield return animator.PlayAndWait(_Increase);
            }
        }

        void SetMaxSize()
        {
            _sr.size = _spriteSizeRange.y * _VectorOne;
            _cl.radius = _colliderRadiusRange.y;
        }

        void SetMinSize()
        {
            _sr.size = _spriteSizeRange.x * _VectorOne;
            _cl.radius = _colliderRadiusRange.x;
        }

        void IncreaseSize()
        {
            _sr.size += _animSizeStep * _VectorOne;
            _cl.radius += _animRadiusStep;
        }

        void DecreaseSize()
        {
            _sr.size -= _animSizeStep * _VectorOne;
            _cl.radius -= _animRadiusStep;
        }

    }
}
