using System;
using UnityEngine;
using System.Linq;
using GAME.Utils.Core;
using System.Collections;
using UnityEngine.Events;
using GAME.Managers.Level;
using GAME.Settings.Levels;
using GAME.Extensions.Lines;
using GAME.Utils.Calculation;
using GAME.Extensions.Objects;
using GAME.Extensions.Sprites;
using GAME.Extensions.Colliders;
using Debug = UnityEngine.Debug;

namespace GAME.Level.Field
{
    [DefaultExecutionOrder(-1000)]
    [RequireComponent(typeof(PolygonCollider2D), typeof(EdgeCollider2D))]
    public class LevelField : MonoBehaviour
    {
        [Header("SHAPE")]
        [SerializeField] FieldShape _shape;
        [SerializeField][Range(16, 64)] int _circlePoints = 32;
        [Header("VISUALS")]
        [SerializeField] Color _fillColor;
        [SerializeField] Color _borderColor;
        [SerializeField][Space(4)][Range(0, 30)] float _offsetY = 0;
        [SerializeField][Space(2)][Range(0, 0.5f)] float _borderWidth = 0.2f;
        [Header("LOGIC")]
        [SerializeField][Range(1, 3)] float _minCutArea = 2.0f;
        [SerializeField][Space(2)] UnityEvent<Vector2[], Collider2D> _onCut;
        [Header("DEBUG")]
        [SerializeField] bool _logging;

        Vector2[] _fieldPoints, _levelPoints, _trailPoints, _maskUnits, _fieldPiecePoints, _zonePiecePoints;
        LevelManager LEVEL;
        GameObject _go;
        Transform _tr, _picture;
        Coroutine _cutting;
        LineRenderer _border;
        SpriteRenderer _texture, _pictureSprite;
        SpriteMask _pictureMask;
        EdgeCollider2D _edgeCollider;
        PolygonCollider2D _polyCollider;
        float  _zonePieceArea, _fieldArea;
        int _begColliderIndex, _endColliderIndex;
        bool? _isInited;
        // bool _isTrailReversed; ???


        public void Enable() => gameObject.SetActive(true);
        public void Disable() => gameObject.SetActive(false);

        public void Cut(Vector2[] trailPoints)
        {
            var locaPoints = Helper.TransPoints(trailPoints, _tr, false);
            _cutting ??= StartCoroutine(Cutting(locaPoints));
        }

        public void ApplyLevelInfo(LevelInfo level)
        {
            _isInited ??= Initialize();
            _levelPoints = level.ShapePoints.ToArray();
            _border.SetPoints(_levelPoints);
            _texture.sprite = LEVEL.Texture;
            _pictureSprite.sprite = LEVEL.Picture;
            _texture.FitToSize(level.Size);
            _pictureSprite.FitToSize(level.Size);
            _tr.localPosition = new Vector2(0, -Helper.ScreenHeight * _offsetY / 200f);
        }

        public void SetMaskSprite(Sprite sprite)
        {
            var newTexture = new Texture2D(sprite.texture.width, sprite.texture.height);
            newTexture.SetPixels(sprite.texture.GetPixels());
            newTexture.Apply();
            var newSprite = Sprite.Create(newTexture, sprite.rect, sprite.pivot / sprite.rect.size, sprite.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            _pictureMask.sprite = newSprite;
        }


        public void Reset()
        {
            _isInited ??= Initialize();
            _polyCollider.SetPath(0, _levelPoints);
            _edgeCollider.SetPoints(_polyCollider.points);
            _fieldPoints = _levelPoints.ToArray();
            _fieldArea = Calc.PolyArea(_fieldPoints);
        }

        IEnumerator Cutting(Vector2[] trailPoints)
        {
            if (trailPoints == null || trailPoints.Length == 0)
            {
                _cutting = null;
                yield break;
            }

            _trailPoints = PrepareTrail(trailPoints, _fieldPoints);
            _fieldPiecePoints = CreateFirstPiece(_fieldPoints, _trailPoints);
            _zonePiecePoints = CreateSecondPiece(_fieldPoints, _trailPoints);
            _zonePieceArea = Calc.PolyArea(_zonePiecePoints);

            if (_zonePieceArea > _fieldArea * 0.5f)
            {
                _zonePieceArea = _fieldArea - _zonePieceArea;
                SwapPoints(ref _fieldPiecePoints, ref _zonePiecePoints);
                if (_logging) Debug.LogWarning("SWAP Areas");
            }

            if (_zonePieceArea > _minCutArea)
            {
                _fieldArea -= _zonePieceArea;
                _polyCollider.SetPath(0, _fieldPiecePoints);
                _fieldPoints = GetColliderPoints(_polyCollider);
                _edgeCollider.SetPoints(_fieldPoints);

                Physics2D.SyncTransforms();
                yield return new WaitForFixedUpdate();
                _onCut?.Invoke(_zonePiecePoints, _polyCollider);
                if (_logging) Helper.Log(() => _zonePieceArea, () => _fieldArea);            
            }
            _cutting = null;
        }

        void Awake()
        {
            Helper.SetField(transform);
        }

        void Start()
        {
            _isInited ??= Initialize();
            ApplyVisualSettings(_fillColor, _borderColor, _borderWidth);
        }

        bool Initialize()
        {
            _go = gameObject;
            _tr = transform;
            _polyCollider = GetComponent<PolygonCollider2D>();
            _edgeCollider = GetComponent<EdgeCollider2D>();
            _texture = GetComponent<SpriteRenderer>();
            _border = GetComponent<LineRenderer>();
            _picture = GetComponentsInChildren<Transform>().Single(child => child.IsPicture());
            _pictureSprite = _picture.GetComponent<SpriteRenderer>();
            _pictureMask = _picture.GetComponent<SpriteMask>();
            LEVEL = LevelManager.I;
            return true;
        }

        void InitFields(Collider2D collider)
        {
            _fieldPoints = GetColliderPoints((PolygonCollider2D)collider);
            _levelPoints = new Vector2[_fieldPoints.Length];
            _fieldPoints.CopyTo(_levelPoints, 0);
            _fieldArea = Calc.PolyArea(_fieldPoints);
        }

        void ApplyVisualSettings(Color fillColor, Color borderColor, float borderWidth)
        {
            _texture.color = fillColor;
            _border.startColor = _border.endColor = borderColor;
            _border.startWidth = _border.endWidth = borderWidth;
        }

        Vector2[] GetColliderPoints(PolygonCollider2D collider) => collider.points;

        Vector2[] PrepareTrail(Vector2[] trailPoints, Vector2[] colliderPoints)
        {
            ProjTrailToCollider(trailPoints, colliderPoints);
            if (IsTrailReversed(trailPoints, colliderPoints)) ReverseTrail(trailPoints);
            return trailPoints;
        }

        Vector2[] CreateFirstPiece(Vector2[] fieldPoints, Vector2[] trailPoints)
        {
            int fieldLength = fieldPoints.Length, trailLength = trailPoints.Length;
            var majorPiecePoints = new Vector2[_begColliderIndex + trailLength + fieldLength - _endColliderIndex];
            Array.Copy(fieldPoints, 0, majorPiecePoints, 0, _begColliderIndex);
            Array.Copy(trailPoints, 0, majorPiecePoints, _begColliderIndex, trailLength);
            if (_endColliderIndex < fieldLength) Array.Copy(fieldPoints, _endColliderIndex, majorPiecePoints, _begColliderIndex + trailLength, fieldLength - _endColliderIndex);
            return majorPiecePoints;
        }

        Vector2[] CreateSecondPiece(Vector2[] fieldPoints, Vector2[] trailPoints)
        {
            int jointIndex = _endColliderIndex - _begColliderIndex, trailLength = trailPoints.Length;
            var minorPiecePoints = new Vector2[jointIndex + trailLength];
            Array.Copy(fieldPoints, _begColliderIndex, minorPiecePoints, 0, jointIndex);
            Array.Copy(trailPoints, 0, minorPiecePoints, jointIndex, trailLength);
            Array.Reverse(minorPiecePoints, jointIndex, trailLength);
            return minorPiecePoints;
        }

        void SwapPoints(ref Vector2[] points1, ref Vector2[] points2) => (points1, points2) = (points2, points1);

        void ProjTrailToCollider(Vector2[] trailPoints, Vector2[] colliderPoints)
        {
            var colliderIndexes = new int[2];
            int colliderLength = colliderPoints.Length, trailLength = trailPoints.Length;
            for (int edgeOrder = 0; edgeOrder < 2; edgeOrder++)
            {
                var trailIndex = edgeOrder * (trailLength - 1);
                var trailPoint = trailPoints[trailIndex];
                var minDistance = float.MaxValue;
                for (int cp = 0; cp < colliderLength; cp++)
                {
                    var nextIndex = cp + 1;
                    var projection = Calc.PointProjection(trailPoint, colliderPoints[cp], colliderPoints[nextIndex % colliderLength]);
                    var projDistance = Vector2.Distance(trailPoint, projection);
                    if (projDistance < minDistance)
                    {
                        minDistance = projDistance;
                        colliderIndexes[edgeOrder] = nextIndex;
                        trailPoints[trailIndex] = projection;
                    }
                }
            }
            (_begColliderIndex, _endColliderIndex) = (colliderIndexes[0], colliderIndexes[1]);
        }

        bool IsTrailReversed(Vector2[] trailPoints, Vector2[] fieldPoints)
        {
            int fieldLength = fieldPoints.Length;
            var isReversed = false;
            if (_endColliderIndex < _begColliderIndex) isReversed = true;
            else if (_begColliderIndex == _endColliderIndex)
            {
                var colliderPoint = fieldPoints[_begColliderIndex % fieldLength];
                float distToBeg = Vector2.Distance(trailPoints[0], colliderPoint), distToEnd = Vector2.Distance(trailPoints[^1], colliderPoint);
                if (distToBeg < distToEnd) isReversed = true;
            }
            return isReversed;
        }

        void ReverseTrail(Vector2[] trailPoints)
        {
            Array.Reverse(trailPoints);
            (_begColliderIndex, _endColliderIndex) = (_endColliderIndex, _begColliderIndex);
        }

    }

    public enum FieldShape
    {
        Circle = 0,
        Rectangle = 4,
        Hexagon = 6,
        Octagon = 8,

        FlagR = 10,
        FlagL = 11,
        FlagT = 12,
        FlagB = 13,
        FlagRS = 101,
        FlagTS = 121,

        Gear = 14,
        Umbrella = 15,
        Bird = 16,

        Star4 = 17,
        Star5 = 18,
        Star7L = 19,
        Star7S = 20
    }
}