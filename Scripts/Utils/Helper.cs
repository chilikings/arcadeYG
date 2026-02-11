using System;
using UnityEngine;
using GAME.Level.Field;
using System.Collections;
using UnityEngine.Events;
using GAME.Utils.Calculation;
using GAME.Extensions.Objects;
using System.Linq.Expressions;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace GAME.Utils.Core
{
    public static class Helper
    {
        public const int MaxLivesCount = 5;
        public const string DeathName = "Death", SpawnName = "Spawn", EnvirName = "Environment", SettingsMenu = "GAME/Settings/", RewardsPath = "GAME/Rewards/", SettingsPath = "Settings/",
                            BuffsName = "Buffs", LivesName = "Lives", ItemsName = "Items", SkinsName = "Skins", ADName = "AD", AudioName = "Audio", GameName = "Game", ImagesName = "Images",
                            LevelsName = "Levels", ResourcesName = "Resources", SavesName = "Saves", UIName = "UI";

        const string _Camera = "MainCamera", _Enemy = "Enemy", _EnemyKin = "EnemyKinematic", _Level = "Level", _Field = "Field", _Zone = "Zone", _Respawn = "Respawn", 
                     _Buff = "Buff", _Player = "Player", _Trail = "Trail", _Picture = "Picture", _Background = "Background", 
                     _EnemySpawner = "EnemySpawner", _Character = "Character", _Environment = "Environment";
        const float _Error = 0.01f;
        static readonly Vector2 _ZeroVector = Vector2.zero;
        static Transform _camera, _player, _trail, _field, _level, _zone, _respawn, _enemySpawner;
        static PolygonCollider2D _fieldPolygon;
        static float? _screenHeight;

        public static Transform Player => _player ??= GetByTag(_Player);    
        public static Transform Trail => _trail ??= GetByTag(_Trail);
        public static Transform Field => _field ??= GetByTag(_Field);
        public static Transform Level => _level ??= GetByTag(_Level);
        public static Transform Zone => _zone ??= GetByTag(_Zone);
        public static Transform Respawn => _respawn ??= GetByTag(_Respawn);
        public static Transform EnemySpawner => _enemySpawner ??= GetByTag(_EnemySpawner);
        public static PolygonCollider2D FieldPolygon => _fieldPolygon ??= Field.GetComponent<PolygonCollider2D>();
        public static bool IsPlayerInsideField => FieldPolygon.OverlapPoint(Player.position);
        public static int FieldMask => LayerMask.GetMask(_Field);
        public static int EnemyLayer => LayerMask.NameToLayer(_Enemy);
        public static int EnemyKinLayer => LayerMask.NameToLayer(_EnemyKin);
        public static float ScreenHeight => _screenHeight ??= Camera.main.orthographicSize * 2;
        public static Vector2 RandomDirection => Random.insideUnitCircle.normalized;
        public static Color White => Color.white;
        public static Color Transp => Color.clear;
        
        
        public static void Reset()
        {
            _player = null;
            _trail = null;
            _field = null;
            _level = null;
            _zone = null;
            _respawn = null;
            _enemySpawner = null;
            _fieldPolygon = null;
        }

        public static Sprite CreateSprite(Vector2Int size, int ppu) => Sprite.Create(new Texture2D(size.x, size.y), new Rect(0, 0, size.x, size.y), new Vector2(0.5f, 0.5f), ppu);

        public static void SetField(Transform field)
        {
            _field = field;
            _fieldPolygon = field ? field.GetComponent<PolygonCollider2D>() : null;
        }

        public static void Log(params Expression<Func<object>>[] expressions)
        {
            var result = "";
            foreach (var expr in expressions)
            {
                string name = null;
                if (expr.Body is MemberExpression memberExpr)
                    name = memberExpr.Member.Name;
                else if (expr.Body is UnaryExpression unaryExpr && unaryExpr.Operand is MemberExpression memberOper)
                    name = memberOper.Member.Name;
                
                object value = expr.Compile().Invoke();
                result += name != null ? $"{name}: {value};  " : $"[Unnamed]: {value};  ";
            }
            Debug.Log(result);
        }

        public static Vector2[] GetShapePoints(FieldShape shape, Vector2 size) => shape switch
        {
            FieldShape.Rectangle => Calc.RectPoints(size),
            FieldShape.Circle => Calc.CirclePoints(32, size),
            FieldShape.Hexagon or FieldShape.Octagon => Calc.PolyPoints((int)shape, size),
            FieldShape.FlagR => Calc.FlagPoints(size, 0.6f, "Right", false),
            FieldShape.FlagL => Calc.FlagPoints(size, 0.6f, "Left", false),
            FieldShape.FlagT => Calc.FlagPoints(size, 0.6f, "Top", false),
            FieldShape.FlagB => Calc.FlagPoints(size, 0.6f, "Bottom", false),
            FieldShape.FlagRS => Calc.FlagPoints(size, 0.6f, "Right", true),
            FieldShape.FlagTS => Calc.FlagPoints(size, 0.6f, "Top", true),
            FieldShape.Star4 => Calc.StarPoints(size, spikes: 4, innerRatio: 0.5f),
            FieldShape.Star5 => Calc.StarPoints(size, spikes: 5, innerRatio: 0.5f),
            FieldShape.Star7L => Calc.StarPoints(size, spikes: 7, innerRatio: 0.6f),
            FieldShape.Star7S => Calc.StarPoints(size, spikes: 7, innerRatio: 0.8f),
            FieldShape.Gear => Calc.GearPoints(size),
            FieldShape.Bird => Calc.BirdPoints(size),
            FieldShape.Umbrella => Calc.UmbrellaPoints(size),
            _ => Calc.RectPoints(size)
        };

        public static Vector2[] TransPoints(Vector2[] points, Transform tr, bool toWorld = true)
        {
            if (points is null || points.Length == 0) return points;
            int pointsCount = points.Length; var resultPoints = new Vector2[pointsCount];
            for (int i = 0; i < pointsCount; i++) 
                resultPoints[i] = toWorld ? tr.TransformPoint(points[i]) : tr.InverseTransformPoint(points[i]);
            return resultPoints;
        }  

        public static T GetScript<T>(Transform transform, Transform parent = null, bool isWorld = true, bool enabled = false) where T : MonoBehaviour
        {
            if (!transform) return null;
            T script;

            if (transform.IsObject()) script = transform.GetComponent<T>();
            else
            {
                script = Transform.Instantiate(transform, parent, isWorld).GetComponent<T>();
                script.gameObject.SetActive(enabled);
            }
            return script;
        }

        public static IEnumerator InvokeInSec(UnityEvent @event, float delay)
        {
            yield return new WaitForSeconds(delay);
            @event?.Invoke();
        }

        public static void StopCoroutine(ref Coroutine coroutine, MonoBehaviour owner)
        {
            if (coroutine is null) return;
            owner.StopCoroutine(coroutine);
            coroutine = null;
        }

        public static void RestartCoroutine(ref Coroutine coroutine, Func<IEnumerator> method, MonoBehaviour owner)
        {
            StopCoroutine(ref coroutine, owner);
            coroutine ??= owner.StartCoroutine(method());
        }

        public static void CopyCollider(CompositeCollider2D composite, PolygonCollider2D polygon)
        {
            var pathsCount = composite.pathCount; 
            polygon.pathCount = pathsCount;
            for (int p = 0; p < pathsCount; p++)
            {
                var pathPoints = new Vector2[composite.GetPathPointCount(p)];
                composite.GetPath(p, pathPoints);
                polygon.SetPath(p, pathPoints);
            }
        }

        public static bool AreValuesClose(float value1, float value2, float error = _Error) => Mathf.Abs(value1 - value2) < error;

        public static bool AreLinesCrossed(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float error = _Error)
        {
            var d = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
            if (Mathf.Abs(d) < error) return false;
            float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / d,
                  ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / d;
            return ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1;
        }

        public static bool TryChance50() => Random.value < 0.5f;

        static Transform GetByTag(string tag) => GameObject.FindWithTag(tag).transform;

        [RuntimeInitializeOnLoadMethod]
        static void ResetOnLoad()
        {
            SceneManager.activeSceneChanged += (_, __) =>
            {
                _field = null;
                _fieldPolygon = null;
                _screenHeight = null;
                _player = _trail = _level = _zone = _respawn = _enemySpawner = null;
            };
        }


        //public static bool AreCollidersClose(Collider2D colliderA, Collider2D colliderB, float error) => colliderA.Distance(colliderB).distance < error;
        //public static Transform MainCamera => _camera ??= GetByTag(_Camera);
        //if (collider is CompositeCollider2D compCollider) return CalcDistToComposite(point, compCollider);
        //if (collider is PolygonCollider2D polyCollider) return CalcDistToPolygon(point, polyCollider.points);
        //public static Vector3 QuaterToAngles(Transform transform) => transform.rotation.eulerAngles;
        //public static bool IsPrefab(GameObject gameObject) => !IsObject(gameObject);
        //static bool IsLayer(Collision2D collision, string layerName) => collision.gameObject.layer == LayerMask.NameToLayer(layerName);
        //static bool IsMask(Collider2D collider, string maskName) => collider.gameObject.layer == LayerMask.GetMask(maskName);
    }
}
