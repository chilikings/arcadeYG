using System;
using UnityEngine;
using GAME.Audio.SFX;
using GAME.Utils.Core;
using System.Collections;
using UnityEngine.Events;
using GAME.Managers.Audio;
using GAME.Extensions.Lines;
using GAME.Extensions.Colliders;
using GAME.Extensions.Animation;

namespace GAME.Player.Trail
{
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public class PlayerTrail : MonoBehaviour
    {
        [Header("VISUAL")]
        [SerializeField][Space(2)][Range(0, 1)] float _alpha;
        [SerializeField][Range(0, 0.05f)][Space(4)] float _tolerance;
        [SerializeField][Range(0.01f, 0.5f)] float _stepLength;
        [SerializeField][Range(0.1f, 3f)][Space(4)] float _beginWidth;
        [SerializeField][Range(0.1f, 3f)] float _endWidth;
        [Header("EVENTS")]
        [SerializeField][Space(4)] UnityEvent<Vector2[]> _onTrailComplete;
        [SerializeField][Space(4)] UnityEvent _onTrailCollide;
        [Header("DEBUG")]
        [SerializeField] bool _logging;
        //[SerializeField] bool _insideField;

        const string _ShaderName = "Universal Render Pipeline/2D/Sprite-Unlit-Default";
        const float _CrossError = 0.001f;
        const int _MinPointsCount = 4;
        AudioManager AUDIO;
        Collider2D _fieldCollider;
        GameObject _go;
        //Transform _player;
        EdgeCollider2D _ec;
        LineRenderer _lr;
        Coroutine _following;
        Animator _an;
        Color _color;
        int _frameNumber;
        bool _shield, _isActive = false; //_isBerserk;

        Vector2 LastPoint => _lr.GetPosition(_lr.positionCount - 1);
        int PointsCount => _lr.positionCount;
        bool IsTrailExisted => _lr.positionCount > 0;
            

        public void SetShield(bool enable) => _shield = enable;

        public void SetColor(Color color) => _color = new Color(color.r, color.g, color.b, _color.a);

        public void SetDeath() => _an.PlayDeath();

        public void Respawn()
        {
            ResetPoints();
            _an.PlaySpawn();
        }

        void Awake()
        {
            InitComponents();
        }

        void Start()
        {
            ApplySettings();
        }

        void FixedUpdate()
        {
            _color.a = _alpha;
            _lr.SetColor(_color);
            //_lr.material.SetColor("_Color", _color);
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (!_isActive) return;

            if (!_shield && (collider.IsEnemy() || collider.IsKinEnemy())) _onTrailCollide?.Invoke();       
            if (collider.IsFieldPolygon()) _fieldCollider ??= collider;
        }

        void InitComponents()
        {
            _go = gameObject;
            _lr = GetComponent<LineRenderer>();
            _ec = GetComponent<EdgeCollider2D>();
            _an = GetComponent<Animator>();
            //_player = Helper.Player;
            AUDIO = AudioManager.I;
        }

        void ApplySettings()
        {
            _lr.sortingLayerName = Helper.EnvirName;
            _lr.startWidth = _endWidth;
            _lr.endWidth = _beginWidth;
        }

        IEnumerator FollowingPlayer()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                var playerPosition = (Vector2)Helper.Player.position;
                _ec.offset = -playerPosition;
                if (_fieldCollider && (_fieldCollider.OverlapPoint(playerPosition)))
                {
                    if (IsCrossed(_lr, playerPosition)) _onTrailCollide?.Invoke();
                    else TryAddPoint(playerPosition);
                }
                else if (IsTrailExisted)
                {
                    if (PointsCount >= _MinPointsCount)
                    {
                        if (_tolerance > 0) _lr.Simplify(_tolerance);
                        _onTrailComplete?.Invoke(_lr.GetPoints());
                    }
                    ResetPoints();
                }
            }
        }

        void TryAddPoint(Vector2 point)
        {
            if (!IsTrailExisted || Vector2.Distance(LastPoint, point) >= _stepLength)
            {
                _lr.AddPoint(point);
                _ec.AddPoint(point);
            }
        }
        
        bool IsCrossed(LineRenderer line, Vector2 position, float error = _CrossError)
        {
            if (PointsCount >= _MinPointsCount)
                for (int p = 0; p < PointsCount - 2; p++)
                    if (Helper.AreLinesCrossed(LastPoint, position, line.GetPosition(p), line.GetPosition(p + 1), error))
                    {
                        if (_logging) Debug.Log("CROSS Trail");
                        return true;
                    }
            return false;
        }

        void ResetPoints()
        {
            _lr.ResetPoints();
            _ec.ResetPoints();
            //SetShield(false); // ! ! ! ! ! !
        }

        void Activate()
        {
            _isActive = true;
            _following ??= StartCoroutine(FollowingPlayer());
        }

        void Deactivate()
        {
            _isActive = false;
            Helper.StopCoroutine(ref _following, this);
        }

    }
}