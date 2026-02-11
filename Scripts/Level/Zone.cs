using System;
using System.Linq;
using UnityEngine;
using GAME.Audio.SFX;
using GAME.Utils.Core;
using GAME.Level.Field;
using System.Collections;
using UnityEngine.Events;
using GAME.Managers.Level;
using GAME.Managers.Audio;
using GAME.Settings.Levels;
using GAME.Utils.Calculation;
using GAME.Level.Zone.Border;
using GAME.Extensions.Colliders;
using System.Collections.Generic;

namespace GAME.Level.Zone
{
    [RequireComponent(typeof(CompositeCollider2D), typeof(PolygonCollider2D), typeof(SpriteMask))]
    public class LevelZone : MonoBehaviour
    {
        [Header("VISUALS")]
        [SerializeField][Space(2)] Transform _borderPrefab;
        [SerializeField][Space(4)][Range(0, 30)] float _offsetY = 0;
        [Header("MASK")]
        [SerializeField][Range(10, 100)] int _pixelsPerUnit = 20;
        [SerializeField][Range(10000, 50000)][Space(2)] int _pixelsPerFrame = 20000;
        [SerializeField][Space] UnityEvent<Sprite> _onMaskInit;
        [Header("DEBUG")]
        [SerializeField][Space] bool _logging;

        const int _CheckPointsCount = 5;
        const float _MinShapeArea = 1.0f;
        readonly Color _MaskColor = Color.white, _NoColor = Color.clear;
        readonly Vector2 _MaskSizeError = new(0.1f, 0.1f);
        List<ZoneBorder> _borders = new();
        Color32[] _maskPixels;
        Vector2[] _maskUnits;
        CompositeCollider2D _compCollider;
        PolygonCollider2D _polyCompCollider, _polygonCollider, _levelPolygon;
        EdgeCollider2D _levelEdge;
        Sprite _maskSprite;
        Texture2D _texture;
        SpriteMask _mask;
        GameObject _go;
        Transform _tr;
        Coroutine _maskGenerating;
        FieldShape _levelShape;
        Vector2 _levelSize, _levelMinPos, _levelOffset, _maskSizeUnits, _maskMinPos;
        Vector2Int _textureSize;
        bool? _isInited;


        public void Enable() => gameObject.SetActive(true);      
        public void Disable() => gameObject.SetActive(false);

        public void ApplyLevelInfo(LevelInfo level)
        {
            _isInited ??= Initialize();
            _tr.localPosition = _levelOffset = new Vector2(0, -Helper.ScreenHeight * _offsetY / 200f);
            _levelShape = level.Shape;
            _levelSize = level.Size;
            _levelMinPos = -level.Size * 0.5f;
            _levelEdge.SetPoints(level.ShapePoints);
            _levelPolygon.SetPath(0, level.ShapePoints);
            _maskSizeUnits = level.Size + _MaskSizeError;
            _maskMinPos = -_maskSizeUnits * 0.5f;
            _textureSize = Calc.TextureSize(level.Size, _pixelsPerUnit);
            Physics2D.SyncTransforms();
        }

        public void Reset()
        {
            _isInited ??= Initialize();
            _polyCompCollider.pathCount = 0;
            _polygonCollider.pathCount = 0;
            _maskSprite = Helper.CreateSprite(_textureSize, _pixelsPerUnit);
            _maskPixels = InitMaskPixels(_textureSize, _MaskColor);
            _maskUnits = CalcMaskUnits(_textureSize, _maskSizeUnits, _maskMinPos + _levelOffset); // (_textureSize, _levelSize, _levelMinPos + _levelOffset); // Initial
            //_maskUnits = Helper.TransPoints(CalcMaskUnits(_textureSize, _levelSize, _levelMinLocal), _tr); Also Works
            _mask.sprite = InitSprite(_maskPixels, _maskUnits, _levelPolygon, _MaskColor, _NoColor);
            _onMaskInit?.Invoke(_mask.sprite);
            ResetBorders(_borders);
        }

        public void SetPoints(Vector2[] newPoints, Collider2D fieldCollider) // AddPoints
        {
            _polyCompCollider.SetPath(_polyCompCollider.pathCount++, newPoints);
            Helper.CopyCollider(_compCollider, _polygonCollider);
            StartCoroutine(HandleMaskGenerating(newPoints, _compCollider, fieldCollider, _levelEdge, _textureSize, _levelSize, _levelMinPos, _pixelsPerFrame));
            AudioManager.I.PlaySFX(SFXName.Cut);
        }

        bool Initialize()
        {
            _go = gameObject;
            _tr = transform;
            _mask = GetComponent<SpriteMask>();
            InitColliders();
            return true;
        }

        void InitColliders()
        {
            _compCollider = GetComponent<CompositeCollider2D>();
            _levelEdge = GetComponentInChildren<EdgeCollider2D>();
            var polyColliders = GetComponents<PolygonCollider2D>();
            _polyCompCollider = polyColliders.Single(c => c.compositeOperation == Collider2D.CompositeOperation.Merge);
            _polygonCollider = polyColliders.Single(c => c.compositeOperation == Collider2D.CompositeOperation.None);
            _levelPolygon = GetComponentsInChildren<PolygonCollider2D>().Single(c => c.transform.parent == _tr);
        }

        Sprite InitSprite(Color32[] pixels, Vector2[] units, PolygonCollider2D collider, Color fillColor, Color clearColor) //Helper.FieldPolygon
        {
            int pixelsCount = pixels.Length;
            for (int i = 0; i < pixelsCount; i++)
                if (pixels[i] == fillColor && !collider.OverlapPoint(units[i])) 
                    pixels[i] = clearColor;             
            return SetMaskSprite(pixels);
        }

        void SetBorders(Collider2D fieldCollider, CompositeCollider2D zoneCollider, EdgeCollider2D levelCollider)
        {
            int pathsCount = zoneCollider.shapeCount, bordersCount = _borders.Count;
            float totalZoneArea = 0, levelArea = Calc.PolyArea(levelCollider.points);
            for (int pathIndex = 0; pathIndex < pathsCount; pathIndex++)
            {
                if (pathIndex >= bordersCount) _borders.Add(InitBorder(pathIndex, _borderPrefab, _tr));
                var pathPoints = Helper.TransPoints(GetPathPoints(pathIndex, zoneCollider), _tr);

                float zoneArea = Calc.PolyArea(pathPoints);
                if (_logging) Debug.Log($"ZoneArea={zoneArea} | LevelArea={levelArea} | Zone/Level={zoneArea / levelArea:F6}");

                if (zoneArea / levelArea > 0.999f) _borders[pathIndex].Reset();
                else if (zoneArea > _MinShapeArea)
                {
                    var borderPoints = FilterCommonPoints(pathPoints.ToArray(), fieldCollider, 0.001f, false);
                    borderPoints = TrySortPoints(borderPoints, zoneCollider, 0.01f, false);
                    var isBorderLooped = IsLineInsideCollider(borderPoints, levelCollider, 0.1f, false);
                    _borders[pathIndex].SetPoints(borderPoints, isBorderLooped);
                    totalZoneArea += isBorderLooped ? levelArea - zoneArea : zoneArea; // ! ! ! ! ! !
                }
            }
            ResetBorders(_borders.Skip(pathsCount).ToList());
            LevelManager.I.CalcScore(totalZoneArea, levelArea); // ! ! ! ! ! !
        }

        void ResetBorders(List<ZoneBorder> borders) => borders.ForEach(border => border.Reset());

        ZoneBorder InitBorder(int index, Transform prefab, Transform zone)
        {
            var @object = Instantiate(prefab, zone, true);
            var border = @object.GetComponent<ZoneBorder>();
            return border;
        }

        Vector2[] GetPathPoints(int index, CompositeCollider2D collider,  bool loop = false)
        {
            var pathPoints = new Vector2[collider.GetPathPointCount(index) + (loop ? 1 : 0)];
            collider.GetPath(index, pathPoints);
            if (loop) pathPoints[^1] = pathPoints[0];
            return pathPoints;
        }

        Vector2[] FilterCommonPoints(Vector2[] points, Collider2D fieldCollider, float error, bool log = false)
        {
            if (log) Debug.Log($"Point before: {points.Length}");
            var borderPoints = new List<Vector2>();
            foreach (var point in points) if (IsPointNearCollider(point, fieldCollider, error)) borderPoints.Add(point);
            if (log) Debug.Log($"Point after: {borderPoints.Count}");
            return borderPoints.ToArray();
        }

        Vector2[] TrySortPoints(Vector2[] points, Collider2D zoneCollider, float error, bool log = false)
        {
            int sortIndex = -1, pointsCount = points.Length;
            for (int index = 0; index < pointsCount - 1; index++)
                if (IsEdgeInsideCollider(points[index], points[index + 1], zoneCollider, error, index, log))
                {
                    sortIndex = index + 1;
                    break;
                }
            if (sortIndex == -1) return points;
            if (log) Debug.Log($"Sort Point[{sortIndex}] ({points[sortIndex]})");

            return SortPoints(points, sortIndex);
        }

        Vector2[] SortPoints(Vector2[] points, int fixIndex)
        {
            int pointsCount = points.Length;
            var sortedPoints = new Vector2[pointsCount];
            Array.Copy(points, fixIndex, sortedPoints, 0, pointsCount - fixIndex);
            Array.Copy(points, 0, sortedPoints, pointsCount - fixIndex, fixIndex);
            Array.Copy(sortedPoints, points, pointsCount);
            return sortedPoints;
        }

        bool IsEdgeInsideCollider(Vector2 pointA, Vector2 pointB, Collider2D collider, float error, int index = -1, bool log = false)
        {
            for (int checkIndex = 0; checkIndex < _CheckPointsCount; checkIndex++)
            {
                var checkPoint = Vector2.Lerp(pointA, pointB, (checkIndex + 1f) / (_CheckPointsCount + 1f));
                var distance = Calc.DistToCollider(checkPoint, collider);
                if (log) Debug.Log($"Distance [{index}]->[{index + 1}] | {checkIndex}): {distance}");
                if (distance > error) return true;
            }
            return false;
        }

        bool IsLineInsideCollider(Vector2[] points, Collider2D levelCollider, float error, bool log = false)
        {
            var distance = MathF.Min(Calc.DistToCollider(points[0], levelCollider), Calc.DistToCollider(points[^1], levelCollider));
            if (log) Debug.Log($"Dist: Zone->Level = {distance}");
            return distance > error;
        }

        bool IsPointNearCollider(Vector2 point, Collider2D collider, float error, bool log = false)
        {
            var distance = Calc.DistToCollider(point, collider);
            if (log) Debug.Log(distance);
            return distance < error;
        }

        Color32[] InitMaskPixels(Vector2Int textureSize, Color color)
        {
            var maskPixels = new Color32[textureSize.x * textureSize.y];
            Array.Fill(maskPixels, color);
            return maskPixels;
        }

        Vector2[] CalcMaskUnits(Vector2Int textureSize, Vector2 levelsize, Vector2 levelMin)
        {
            var maskUnits = new Vector2[textureSize.x * textureSize.y];
            for (int y = 0; y < textureSize.y; y++)
                for (int x = 0; x < textureSize.x; x++)
                    maskUnits[y * textureSize.x + x] = PixelToUnit(x, y, textureSize, levelsize, levelMin);
            return maskUnits;
        }

        IEnumerator HandleMaskGenerating(Vector2[] newPoints, CompositeCollider2D zoneCollider, Collider2D fieldCollider, EdgeCollider2D levelCollider, Vector2Int textureSize, Vector2 levelsize, Vector2 levelMin, int ppf)
        {
            zoneCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            yield return _maskGenerating ??= StartCoroutine(MaskGenerating(newPoints, zoneCollider, textureSize, levelsize, levelMin, ppf));
            _maskGenerating = null;
            zoneCollider.geometryType = CompositeCollider2D.GeometryType.Outlines;
            SetBorders(fieldCollider, zoneCollider, levelCollider);
        }

        IEnumerator MaskGenerating(Vector2[] newPoints, Collider2D collider, Vector2Int textureSize, Vector2 levelsize, Vector2 levelMin, int ppf)
        {         
            int pointsCount = newPoints.Length;
            Vector2 pointsMin = pointsCount > 0 ? new Vector2(newPoints.Min(p => p.x), newPoints.Min(p => p.y)) : Vector2.zero,
                    pointsMax = pointsCount > 0 ? new Vector2(newPoints.Max(p => p.x), newPoints.Max(p => p.y)) : Vector2.zero;
            Vector2Int textureMin = pointsCount > 0 ? UnitToPixel(pointsMin, textureSize, levelsize, levelMin, false) : Vector2Int.zero,
                       textureMax = pointsCount > 0 ? UnitToPixel(pointsMax, textureSize, levelsize, levelMin, true) : textureSize;

            int pixelsCount = 0;
            for (int y = textureMin.y; y < textureMax.y; y++)
                for (int x = textureMin.x; x < textureMax.x; x++, pixelsCount++)
                {
                    if (pixelsCount > ppf)
                    {
                        pixelsCount = 0;
                        yield return null;
                    }
                    int pixelIndex = y * textureSize.x + x;
                    if (_maskPixels[pixelIndex] == _MaskColor && collider.OverlapPoint(_maskUnits[pixelIndex]))
                        _maskPixels[pixelIndex] = _NoColor;
                }
            yield return null;
            _mask.sprite = SetMaskSprite(_maskPixels);
        }

        Sprite SetMaskSprite(Color32[] maskPixels)
        {
            _maskSprite.texture.SetPixels32(maskPixels);
            _maskSprite.texture.Apply();
            return _maskSprite;
        }

        Vector2 PixelToUnit(int pixelX, int pixelY, Vector2Int textureSize, Vector2 levelsize, Vector2 levelMin)
        {
            float unitX = levelMin.x + ((pixelX + 0.5f) / textureSize.x) * levelsize.x,
                  unitY = levelMin.y + ((pixelY + 0.5f) / textureSize.y) * levelsize.y;
            return new Vector2(unitX, unitY);
        }

        Vector2Int UnitToPixel(Vector2 unit, Vector2Int textureSize, Vector2 levelSize, Vector2 levelMin, bool isMax = true)
        {
            float pixelX = (unit.x - levelMin.x) * textureSize.x / levelSize.x - 0.5f,
                  pixelY = (unit.y - levelMin.y) * textureSize.y / levelSize.y - 0.5f;
            if (isMax)
            {
                pixelX = Mathf.Clamp(Mathf.CeilToInt(pixelX) + 1, 1, textureSize.x);
                pixelY = Mathf.Clamp(Mathf.CeilToInt(pixelY) + 1, 1, textureSize.y);
            }
            else
            {
                pixelX = Mathf.Clamp(Mathf.FloorToInt(pixelX) - 1, 0, textureSize.x - 1);
                pixelY = Mathf.Clamp(Mathf.FloorToInt(pixelY) - 1, 0, textureSize.y - 1);
            }
            return new Vector2Int((int)pixelX, (int)pixelY);
        }

    }
}
