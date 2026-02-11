using System;
using System.Linq;
using UnityEngine;
using GAME.Utils.Core;
using GAME.Managers.Game;
using UnityEngine.Events;
using GAME.Managers.Audio;
using GAME.Extensions.Lines;
using GAME.Extensions.Colliders;
using System.Collections.Generic;

namespace GAME.Enemies.Trail
{
    [RequireComponent(typeof(LineRenderer), typeof(EdgeCollider2D))]
    public class EnemyTrail : MonoBehaviour
    {
        [Header("VISUAL")]
        [SerializeField][Space(2)] Gradient _color;
        [SerializeField][Range(0, 0.05f)][Space(2)] float _tolerance;
        [SerializeField][Range(0.01f, 0.5f)] float _stepLength;
        [SerializeField][Range(1, 50)] int _maxLength;
        [SerializeField][Range(10, 200)] int _maxPoints;
        [SerializeField][Range(0.01f, 2f)][Space(4)] float _beginWidth;
        [SerializeField][Range(0.01f, 2f)] float _endWidth;
        [Header("EVENTS")]
        [SerializeField][Space(4)] UnityEvent _onTrailCollided;
        [Header("DEBUG")]
        [SerializeField] bool _logging;
        [SerializeField] bool _insideField;
        [SerializeField] bool _collideField;

        const string _ShaderName = "Universal Render Pipeline/2D/Sprite-Lit-Default"; //"Universal Render Pipeline/Lit";
        const float _CrossError = 0.001f;
        const int _MinPointsCount = 4;
        int _frameNumber;
        float _lineLength;
        GameManager GAME;
        GameObject _go;
        Transform _enemy;
        List<(Vector2 pos, float dist)> _distPoints = new();
        EdgeCollider2D _ec;
        LineRenderer _lr;

        List<Vector2> Points => _distPoints.Select(dp => dp.pos).ToList();
        (Vector2 pos, float dist) LastDistPoint { get => _distPoints[PointsCount - 1]; set => _distPoints[PointsCount - 1] = value; }
        Vector2 LastLinePoint { get => _lr.GetPosition(PointsCount - 1); set => _lr.SetPosition(PointsCount - 1, value); }
        int PointsCount => _lr.positionCount;
            

        public void Disable() => _go.SetActive(false);

        public void SetEnemy(Transform enemy) => _enemy ??= enemy;
        
        public void Reset()
        {
            Enable();
            _ec.ResetPoints();
            _lr.ResetPoints();
            _distPoints.Clear();
            _lineLength = 0;
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
            if (_enemy)
            {
                var enemyPosition = (Vector2)_enemy.position;
                if (TryAddPoint(enemyPosition, _distPoints, _lr, ref _lineLength, _stepLength))
                {
                    int trimIndex = FindTrimIndex(_distPoints, _lr, _lineLength, _maxLength, _stepLength);
                    if (trimIndex == -1) _ec.AddPoint(enemyPosition);              
                    else
                    {
                        RemovePoints(_distPoints, _lr, trimIndex + 1, ref _lineLength);
                        _ec.SetPoints(Points);
                    }
                }
                _ec.offset = -enemyPosition;
            }
            _frameNumber++;
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.IsPlayer() && GAME.IsBerserk || collider.IsZonePolygon())
            {
                int cutIndex = FindCutIndex(_distPoints, collider);
                if (cutIndex != -1)
                {
                    RemovePoints(_distPoints, _lr, cutIndex + 1, ref _lineLength);
                    _ec.SetPoints(Points);
                }
                if (_logging) Debug.Log($"Enter ZONE [{_frameNumber}]");
            }
        }

        void InitComponents()
        {
            _go = gameObject;
            _lr = GetComponent<LineRenderer>();
            _ec = GetComponent<EdgeCollider2D>();
            GAME = GameManager.I;
        }

        void ApplySettings()
        {
            _lr.sortingLayerName = "Environment";
            _lr.startWidth = _endWidth;
            _lr.endWidth = _beginWidth;

            if (!_lr.material || _lr.material.shader.name != _ShaderName)
                _lr.material = new Material(Shader.Find(_ShaderName));

            //_lr.material.SetColor("_BaseColor", _color);
            _lr.colorGradient = _color;
        }

        bool TryAddPoint(Vector2 point, List<(Vector2 pos, float dist)> distPoints, LineRenderer line, ref float lineLength, float stepLength)
        {
            var distance = PointsCount > 0 ? Vector2.Distance(LastLinePoint, point) : 0;
            if (distance > 0 && distance < stepLength) return false;
            lineLength += distance;         
            line.AddPoint(point);
            AddDistPoint(point, distance, distPoints);
            return true;
        }

        void RemovePoints(List<(Vector2, float)> distPoints, LineRenderer line, int count, ref float lineLength)
        {
            line.RemoveRange(0, count);
            distPoints.RemoveRange(0, count);
            lineLength = CalcSumLength(distPoints);
        }

        void AddDistPoint(Vector2 point, float distance, List<(Vector2 pos, float)> distPoints)
        {
            if (distPoints.Count > 0) distPoints[^1] = (distPoints[^1].pos, distance);
            distPoints.Add((point, 0));
        }

        int FindTrimIndex(List<(Vector2 pos, float dist)> distPoints, LineRenderer line, float lineLength, float maxLength, float stepLength)
        {
            int trimIndex = -1;
            while (maxLength - lineLength < stepLength && trimIndex < PointsCount)
            {
                trimIndex++;
                lineLength -= distPoints[trimIndex].dist;
            }
            return trimIndex;
        }

        int FindCutIndex(List<(Vector2 pos, float dist)> distPoints, Collider2D polygon)
        {
            for (int i = PointsCount - 1; i >= 0; i--) 
                if (polygon.OverlapPoint(distPoints[i].pos)) return i;
            return -1;
        }

        void Enable() => _go.SetActive(true);

        float CalcSumLength(List<(Vector2, float dist)> distPoints) => distPoints.Sum(dp => dp.dist);

    }
}