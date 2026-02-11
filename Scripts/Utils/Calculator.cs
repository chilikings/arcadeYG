using UnityEngine;
using GAME.Utils.Conversion;
using System.Collections.Generic;

namespace GAME.Utils.Calculation
{
    public static class Calc
    {
        static float DistToPolygon(Vector2 point, Vector2[] points)
        {
            if (points == null || points.Length == 0) return -1;
            var minDistSqr = float.MaxValue;
            var count = points.Length;
            for (int p = 0; p < count; p++)
            {
                Vector2 start = points[p], end = points[(p + 1) % count], line = end - start;
                float lineSqr = line.sqrMagnitude, distSqr;
                if (lineSqr == 0f) distSqr = (point - start).sqrMagnitude;
                else
                {
                    var t = Mathf.Clamp01(Vector2.Dot(point - start, line) / lineSqr);
                    distSqr = (point - start - line * t).sqrMagnitude;
                }
                if (distSqr < minDistSqr) minDistSqr = distSqr;
            }
            return Mathf.Sqrt(minDistSqr);
        }

        public static float DistToComposite(Vector2 point, CompositeCollider2D collider)
        {
            if (collider.geometryType == CompositeCollider2D.GeometryType.Outlines) return Vector2.Distance(point, collider.ClosestPoint(point));
            var pathsCount = collider.pathCount;
            var minDistance = float.MaxValue;
            for (int p = 0; p < pathsCount; p++)
            {
                var pathPoints = new Vector2[collider.GetPathPointCount(p)];
                collider.GetPath(p, pathPoints);
                var distance = DistToPolygon(point, pathPoints);
                if (distance < minDistance) minDistance = distance;
            }
            return minDistance;
        }

        public static float PolyArea(Vector2[] points)
        {
            if (points.Length < 3) return 0;
            var length = points.Length;
            var area = 0f;
            for (int p = 0; p < length; p++)
            {
                Vector2 curr = points[p], next = points[(p + 1) % length];
                area += (curr.x * next.y) - (next.x * curr.y);
            }
            return Mathf.Abs(area * 0.5f);
        }

        public static Vector2[] PolyPoints(int count, Vector2 size)
        {
            var points = new Vector2[count];
            if (count == 6)
            {
                var halfSize = size * 0.5f;
                float[] xFactors = { -0.5f, 0.5f, 1, 0.5f, -0.5f, -1 }, yFactors = { -1, -1, 0, 1, 1, 0 };
                for (int i = 0; i < count; i++)
                    points[i] = new Vector2(xFactors[i] * halfSize.x, yFactors[i] * halfSize.y);
            }
            else
            {
                float step = 360f / count, startDeg = count == 8 ? 22.5f : (count % 2 == 0 ? 0 : 90);
                var halfSize = size * 0.5f * (count == 8 ? 1.0824f : 1);
                for (int i = 0; i < count; i++)
                    points[i] = Vector2.Scale(Conv.DegToVector(startDeg + i * step), halfSize);
            }
            return points;
        }

        public static Vector2 PointProjection(Vector2 point, Vector2 beg, Vector2 end)
        {
            var line = end - beg;
            var len = line.magnitude;
            var dir = line / len;
            return beg + dir * Mathf.Clamp(Vector2.Dot(point - beg, dir), 0, len);
        }

        public static Vector2[] RectPoints(Vector2 size, bool loop = false)
        {
            float x = size.x * 0.5f, y = size.y * 0.5f;
            var points = new Vector2[] { new(-x, y), new(-x, -y), new(x, -y), new(x, y) };
            return loop ? LoopPoints(points) : points;
        }

        public static Vector2[] CirclePoints(int count, Vector2 size)
        {
            var points = new Vector2[count];
            var halfSize = size * 0.5f;
            var step = 360f / count;
            for (int i = 0; i < count; i++)
                points[i] = Vector2.Scale(Conv.DegToVector(i * step), halfSize);
            return points;
        }

        public static Vector2[] FlagPoints(Vector2 size, float depthFactor = 0.6f, string orientation = "Left", bool simetricMode = false)
        {

            float x = size.x * 0.5f;
            float y = size.y * 0.5f;
            var flag = new List<Vector2>();

            flag.Add(new Vector2(x, -y));
            if(orientation == "Bottom" || (orientation == "Top" && simetricMode)) { flag.Add(new Vector2(0, -y * depthFactor)); }
            flag.Add(new Vector2(-x, -y));
            if (orientation == "Left" || (orientation == "Right" && simetricMode)) { flag.Add(new Vector2(-x * depthFactor, 0)); }
            flag.Add(new Vector2(-x, y));
            if (orientation == "Top" || (orientation == "Bottom" && simetricMode)) { flag.Add(new Vector2(0, y * depthFactor)); }
            flag.Add(new Vector2(x, y));
            if (orientation == "Right" || (orientation == "Left" && simetricMode)) { flag.Add(new Vector2(x * depthFactor, 0)); }

            return flag.ToArray();

        }

        public static Vector2[] StarPoints(Vector2 size, int spikes = 5, float innerRatio = 0.5f)
        {
            int count = spikes * 2;
            var points = new Vector2[count];

            float outerR = Mathf.Min(size.x, size.y) * 0.5f;
            float innerR = outerR * innerRatio;

            float stepRad = (Mathf.PI * 2f) / count;
            float angleRad = Mathf.PI * 0.5f;

            for (int i = 0; i < count; i++)
            {
                float r = ((i & 1) == 0) ? outerR : innerR;

                points[i].x = Mathf.Cos(angleRad) * r;
                points[i].y = Mathf.Sin(angleRad) * r;
                angleRad -= stepRad;
            }

            return points;
        }
        public static Vector2[] GearPoints(Vector2 size, int teeth = 8, float innerRatio = 0.75f, float toothWidth = 0.4f, int valleySegments = 5, float valleyWidthFactor = 0.5f)
        {
            float pitch = Mathf.PI * 2f / teeth;
            float halfTooth = pitch * toothWidth * 0.5f;
            float rawValley = pitch - 2f * halfTooth;
            float valleyAngle = rawValley * valleyWidthFactor;
            float halfValley = valleyAngle * 0.5f;
            Vector2 half = size * 0.5f;
            float outerR = Mathf.Min(half.x, half.y);
            float innerR = outerR * innerRatio;
            int totalPoints = teeth * (4 + valleySegments);
            Vector2[] pts = new Vector2[totalPoints];
            int idx = 0;
            float invSeg = 1f / (valleySegments + 1);

            for (int i = 0; i < teeth; i++)
            {
                float baseA = i * pitch;
                float A0 = baseA - halfTooth - halfValley;
                float A1 = baseA - halfTooth;
                float A2 = baseA + halfTooth;
                float A3 = baseA + halfTooth + halfValley;

                pts[idx++] = new Vector2(Mathf.Cos(A0) * innerR, Mathf.Sin(A0) * innerR);
                pts[idx++] = new Vector2(Mathf.Cos(A1) * outerR, Mathf.Sin(A1) * outerR);
                pts[idx++] = new Vector2(Mathf.Cos(A2) * outerR, Mathf.Sin(A2) * outerR);
                pts[idx++] = new Vector2(Mathf.Cos(A3) * innerR, Mathf.Sin(A3) * innerR);

                for (int j = 1; j <= valleySegments; j++)
                {
                    float ang = A3 + valleyAngle * (j * invSeg);
                    pts[idx++] = new Vector2(Mathf.Cos(ang) * innerR, Mathf.Sin(ang) * innerR);
                }
            }

            return pts;
        }

        public static Vector2[] BirdPoints(Vector2 size)
        {
            float x = size.x * 0.5f;
            float y = size.y * 0.5f;
            var points = new List<Vector2>();
            points.Add(new Vector2(-x, -y * 0.2f));
            points.Add(new Vector2(-x * 0.8f, y * 0.2f));
            points.Add(new Vector2(-x * 0.6f, y * 0.25f));
            points.Add(new Vector2(-x * 0.4f, 0));
            points.Add(new Vector2(x * 0.3f, y * 0.3f));
            points.Add(new Vector2(x, 0));
            points.Add(new Vector2(x * 0.4f, -y * 0.1f));
            points.Add(new Vector2(0, -y * 0.4f));
            points.Add(new Vector2(x * 0.2f, -y * 0.6f));
            points.Add(new Vector2(x, -y * 0.5f));
            points.Add(new Vector2(x * 0.9f, -y));
            points.Add(new Vector2(-x * 0.3f, -y * 0.9f));
            points.Add(new Vector2(-x * 0.8f, -y * 0.15f));

            float offsetY = 0.2f * y;

            var arr = points.ToArray();
            for (int i = 0; i < arr.Length; i++)
                arr[i].y += offsetY;

            return arr;
        }

        public static Vector2[] UmbrellaPoints(Vector2 size, float canopyHeightFactor = 0.4f, int canopySegments = 32, float handleWidth = 3f, float handleHeight = 3f, int semiSegments = 16)
        {
            float x = size.x * 0.5f;
            float canopyH = size.y * canopyHeightFactor;
            float hHalf = handleWidth * 0.5f;
            var pts = new List<Vector2>();

            for (int i = 0; i <= canopySegments; i++)
            {
                float t = i / (float)canopySegments;
                float ang = Mathf.PI * (1f - t);
                pts.Add(new Vector2(Mathf.Cos(ang) * x, Mathf.Sin(ang) * canopyH));
            }

            float rCapony = (pts[pts.Count - 1].x - hHalf) / 4f;
            Vector2 center = new Vector2(pts[pts.Count - 1].x - rCapony, pts[pts.Count - 1].y);
            for (int i = 0; i <= semiSegments; i++)
            {
                float t = i / (float)semiSegments;
                float ang = Mathf.PI * t;
                pts.Add(center + new Vector2(Mathf.Cos(ang) * rCapony, Mathf.Sin(ang) * rCapony));
            }

            center = new Vector2(pts[pts.Count - 1].x - rCapony, pts[pts.Count - 1].y);
            for (int i = 0; i <= semiSegments; i++)
            {
                float t = i / (float)semiSegments;
                float ang = Mathf.PI * t;
                pts.Add(center + new Vector2(Mathf.Cos(ang) * rCapony, Mathf.Sin(ang) * rCapony));
            }

            pts.Add(new Vector2(hHalf, 0));
            pts.Add(new Vector2(hHalf, -handleHeight));

            float r = handleWidth * 0.5f;
            center = new Vector2(hHalf * 2, -handleHeight);
            for (int i = 0; i <= semiSegments; i++)
            {
                float t = i / (float)semiSegments;
                float ang = Mathf.PI + Mathf.PI * t; 
                pts.Add(center + new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r));
            }

            center = new Vector2(pts[pts.Count - 1].x + hHalf, pts[pts.Count - 1].y);
            for (int i = 0; i <= semiSegments; i++)
            {
                float t = i / (float)semiSegments;
                float ang = Mathf.PI - Mathf.PI * t;
                pts.Add(center + new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r));
            }

            center = new Vector2(pts[pts.Count - 1].x - (hHalf * 3f), pts[pts.Count - 1].y);
            r = handleWidth * 1.5f;
            for (int i = 0; i <= semiSegments; i++)
            {
                float t = i / (float)semiSegments;
                float ang = 2f * Mathf.PI - Mathf.PI * t;
                pts.Add(center + new Vector2(Mathf.Cos(ang) * r, Mathf.Sin(ang) * r));
            }

            pts.Add(new Vector2(-hHalf, -handleHeight));
            pts.Add(new Vector2(-hHalf, 0));

            center = new Vector2(pts[pts.Count - 1].x - rCapony, pts[pts.Count - 1].y);
            for (int i = 0; i <= semiSegments; i++)
            {
                float t = i / (float)semiSegments;
                float ang = Mathf.PI * t;
                pts.Add(center + new Vector2(Mathf.Cos(ang) * rCapony, Mathf.Sin(ang) * rCapony));
            }

            center = new Vector2(pts[pts.Count - 1].x - rCapony, pts[pts.Count - 1].y);
            for (int i = 0; i <= semiSegments; i++)
            {
                float t = i / (float)semiSegments;
                float ang = Mathf.PI * t;
                pts.Add(center + new Vector2(Mathf.Cos(ang) * rCapony, Mathf.Sin(ang) * rCapony));
            }

            return pts.ToArray();
        }

        public static Vector2[] LoopPoints(Vector2[] points)
        {
            var loopPoints = new Vector2[points.Length + 1];
            points.CopyTo(loopPoints, 0);
            loopPoints[^1] = loopPoints[0];
            return loopPoints;
        }

        public static Vector2 ScreenSize()
        {
            float height = Camera.main.orthographicSize * 2, width = height * Screen.width / Screen.height;
            return new Vector2(width, height);
        }

        public static float DistToCollider(Vector2 point, Collider2D collider) => Vector2.Distance(point, collider.ClosestPoint(point));

        public static Vector2Int TextureSize(Vector2 fieldsize, int ppu) => new(Mathf.CeilToInt(fieldsize.x * ppu), Mathf.CeilToInt(fieldsize.y * ppu));
    }
}
