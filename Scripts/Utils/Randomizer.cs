using System;
using System.Linq;
using UnityEngine;
using GAME.Utils.Core;
using GAME.Utils.Calculation;
using GAME.Enemies.Controller;
using Random = UnityEngine.Random;

namespace GAME.Utils.Randomization
{
    public class Rand
    {
        static readonly System.Random _rnd = new System.Random(Guid.NewGuid().GetHashCode());

        public static Vector2 FindFreePoint(float minDist, float offset, int maxTries = 30)
        {
            var staticEnemies = Helper.EnemySpawner.GetComponentsInChildren<EnemyController>().Where(e => e.Move == MoveType.Static).ToArray();
            var point = Vector2.zero;
            var isFree = true;
            for (int i = 0; i < maxTries; i++)
            {
                point = GetFieldPoint(offset);
                isFree = true;
                foreach (var enemy in staticEnemies)
                {
                    if (Vector2.Distance(point, enemy.Position) < minDist)
                    {
                        isFree = false;
                        break;
                    }
                }
                if (isFree) return point;
            }
            return point;
        }

        public static Vector2 GetFieldPoint(float offset)
        {
            Collider2D polygon = Helper.Field.GetComponent<PolygonCollider2D>(), edge = Helper.Field.GetComponent<EdgeCollider2D>();
            Vector2 point = new(), bounds = (Vector2)polygon.bounds.extents - offset * Vector2.one, fieldPosition = Helper.Field.position;

            for (int loop = 0; loop < 50; loop++)
            {
                float randomX = (float)(_rnd.NextDouble() * 2 - 1) * bounds.x;
                float randomY = (float)(_rnd.NextDouble() * 2 - 1) * bounds.y;
                point = new Vector2(randomX, randomY) + fieldPosition;

                if (polygon.OverlapPoint(point) && Calc.DistToCollider(point, edge) > offset) return point;
            }

            Debug.LogWarning("Emergency Spawn");
            return polygon.bounds.center;
        }

        public static Vector2 GetFieldPointL(float offset)
        {
            Collider2D polygon = Helper.Field.GetComponent<PolygonCollider2D>(), edge = Helper.Field.GetComponent<EdgeCollider2D>();
            Vector2 point = new(), bounds = (Vector2)polygon.bounds.extents - offset * Vector2.one, fieldPosition = Helper.Field.position;
            for (int loop = 0; loop < 50; loop++)
            {
                point = new Vector2(Random.Range(-bounds.x, bounds.x), Random.Range(-bounds.y, bounds.y)) + fieldPosition;
                if (polygon.OverlapPoint(point) && Calc.DistToCollider(point, edge) > offset) return point;
            }
            Debug.LogWarning("Emergency Spawn");
            return polygon.bounds.center;
        }
    }

}