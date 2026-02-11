using System;
using UnityEngine;
using GAME.Utils.Conversion;
using GAME.Utils.Calculation;

namespace GAME.Extensions.Lines
{
    public static class LineExt
    {
        public static void SetPoints(this LineRenderer line, Vector2[] points, bool loop = false)
        {
            if (!line || points is not { Length: >= 2 }) return;
            line.positionCount = points.Length + (loop ? 1 : 0);
            line.SetPositions(Conv.Vector2ToVector3(loop ? Calc.LoopPoints(points) : points));
        }

        public static void AddPoint(this LineRenderer line, Vector2 point)
        {
            if (!line) return;
            line.positionCount++;
            line.SetPosition(line.positionCount - 1, point);
        }

        public static void ResetPoints(this LineRenderer line)
        {
            if (!line) return;
            line.positionCount = 0;
        }

        public static Vector2[] GetPoints(this LineRenderer line)
        {
            if (!line) return new Vector2[0];
            var points = new Vector3[line.positionCount];
            line.GetPositions(points);
            return Array.ConvertAll(points, p => (Vector2)p);
        }

        public static bool RemoveRange(this LineRenderer line, int index, int count)
        {
            if (line == null || index < 0 || index >= line.positionCount || count <= 0) return false;
            count = Math.Min(count, line.positionCount - index);
            int remainCount = line.positionCount - count;
            if (remainCount <= 0)
            {
                line.positionCount = 0;
                return false;
            }

            var remainPoints = new Vector3[remainCount];
            for (int i = 0; i < index; i++) remainPoints[i] = line.GetPosition(i);
            for (int i = index; i < remainCount; i++) remainPoints[i] = line.GetPosition(i + count);

            line.positionCount = remainCount;
            line.SetPositions(remainPoints);
            return true;
        }

        public static bool RemoveTail(this LineRenderer line, int index)
        {
            if (line == null || index < 0 || index >= line.positionCount) return false;
            int remainCount = line.positionCount - (index + 1);
            if (remainCount <= 0) line.positionCount = 0;
            else
            {
                var remainPoints = new Vector3[remainCount];
                for (int i = 0; i < remainCount; i++) remainPoints[i] = line.GetPosition(index + 1 + i);

                line.positionCount = remainCount;
                line.SetPositions(remainPoints);
            }
            return true;
        }

        public static LineRenderer SetColor(this LineRenderer line, Color color)
        {
            if (!line) return null;
            line.startColor = line.endColor = color;
            return line;
        }     
    }
}