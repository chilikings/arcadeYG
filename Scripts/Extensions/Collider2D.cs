using System;
using System.Linq;
using UnityEngine;
using GAME.Utils.Calculation;
using GAME.Extensions.Objects;
using System.Collections.Generic;

namespace GAME.Extensions.Colliders
{
    public static class ColliderExt
    {
        const string _Enemy = "Enemy", _EnemyKinematic = "EnemyKinematic", _Level = "Level", _Field = "Field", _Zone = "Zone", _Buff = "Buff", _Player = "Player";
        static readonly Vector2 _ZeroVector = Vector2.zero;


        public static void FitToBounds(this EdgeCollider2D collider, Vector2 boundsCenter, float topbarHeight)
        {
            Vector2 cameraPos = Camera.main.transform.position, screenSize = Calc.ScreenSize();
            float halfHeight = screenSize.y * 0.5f, bottomY = cameraPos.y - halfHeight, newTopY = cameraPos.y + halfHeight - screenSize.y * topbarHeight,
                  halfWidth = screenSize.x * 0.5f, leftX = cameraPos.x - halfWidth, rightX = cameraPos.x + halfWidth;

            collider.points = new[] { new Vector2(leftX,  bottomY) - boundsCenter, new Vector2(rightX, bottomY) - boundsCenter,
                                      new Vector2(rightX, newTopY) - boundsCenter, new Vector2(leftX,  newTopY) - boundsCenter,
                                      new Vector2(leftX,  bottomY) - boundsCenter };
        }

        public static void FitToBounds(this EdgeCollider2D collider, Vector2 size, Vector2 center)
        {
            float halfWidth = size.x * 0.5f, halfHeight = size.y * 0.5f,
                  leftX = center.x - halfWidth, rightX = center.x + halfWidth, 
                  bottomY = center.y - halfHeight, topY = center.y + halfHeight;

            collider.points = new Vector2[] { new(leftX,  bottomY), new(rightX, bottomY), 
                new(rightX, topY), new(leftX,  topY), new(leftX,  bottomY) };
        }

        public static void FitToBounds(this PolygonCollider2D collider, Vector2 size, Vector2 center)
        {
            float halfWidth = size.x * 0.5f, halfHeight = size.y * 0.5f,
                  leftX = center.x - halfWidth, rightX = center.x + halfWidth,
                  bottomY = center.y - halfHeight, topY = center.y + halfHeight;

            collider.points = new Vector2[] { new(leftX,  bottomY), new(rightX, bottomY), new(rightX, topY), new(leftX,  topY) };
        }


        public static void SetPoints(this EdgeCollider2D edge, Vector2[] points)
        {
            if (!edge || points is not { Length: >= 2 }) return;
            edge.SetPoints(Calc.LoopPoints(points).ToList());
        }

        public static void ResetPoints(this EdgeCollider2D edge)
        {
            if (!edge) return;
            edge.points = new[] { _ZeroVector, _ZeroVector };
            edge.offset = _ZeroVector;
        }

        public static void AddPoint(this EdgeCollider2D edge, Vector2 point)
        {
            if (!edge) return;
            var points = new List<Vector2>(edge.points); int pointsCount = points.Count;
            if (pointsCount == 0) points.AddRange(new[] { point, point });
            else if (pointsCount == 2 && points[0] == points[1])
            {
                if (points[0] == _ZeroVector) points[0] = point;
                points[1] = point;
            }
            else points.Add(point);
            edge.SetPoints(points);
        }

        public static Vector2[] GetAllPoints(this CompositeCollider2D collider)
        {
            int pathsCount = collider.pathCount;
            var points = new Vector2[collider.pointCount];
            for (int p = 0, destIndex = 0; p < pathsCount; p++)
            {
                int pointsCount = collider.GetPathPointCount(p);
                var pathPoints = new Vector2[pointsCount];
                collider.GetPath(p, pathPoints);
                Array.Copy(pathPoints, 0, points, destIndex, pointsCount);
                destIndex += pointsCount;
            }
            return points;
        }

        public static bool IsEnemy(this Collider2D collider) => IsLayer(collider, _Enemy);
        public static bool IsKinEnemy(this Collider2D collider) => IsLayer(collider, _EnemyKinematic);
        public static bool IsField(this Collider2D collider) => IsLayer(collider, _Field);
        public static bool IsZone(this Collider2D collider) => IsLayer(collider, _Zone);
        public static bool IsPlayer(this Collider2D collider) => IsLayer(collider, _Player);
        public static bool IsBuff(this Collider2D collider) => IsLayer(collider, _Buff);
        public static bool IsZonePolygon(this Collider2D collider) => collider.IsZone() && collider is PolygonCollider2D;
        public static bool IsFieldPolygon(this Collider2D collider) => collider.IsField() && collider is PolygonCollider2D;
        public static bool IsFieldEdge(this Collider2D collider) => collider.IsField() && collider is EdgeCollider2D;

        static bool IsLayer(Collider2D collider, string layerName) => collider.gameObject.IsLayer(layerName);
    }
}