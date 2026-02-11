using System;
using UnityEngine;

namespace GAME.Utils.Conversion
{
    public class Conv
    {
        public static Vector2 DegToVector(float degree, float magnitude = 1f) => RadToVector(degree * Mathf.Deg2Rad, magnitude);
        
        public static Vector2 RadToVector(float radian, float magnitude = 1f) => new Vector2(Mathf.Cos(radian), Mathf.Sin(radian)) * magnitude;

        public static Quaternion DegToQuater(float degree) => Quaternion.Euler(0, 0, degree);

        public static Quaternion VectorToQuater(Vector2 vector) => DegToQuater(VectorToPosAngle(vector));

        public static float VectorToPosAngle(Vector2 vector) => (Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg + 360f) % 360f;

        public static float VectorToAngle(Vector2 vector) => Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;

        public static Vector3[] Vector2ToVector3(Vector2[] points) => Array.ConvertAll(points, p => (Vector3)p);

        //public static Vector2 DegToVector(float degree, float magnitude = 1f) => Quaternion.Euler(0, 0, degree) * Vector2.right * magnitude;
    }
}
