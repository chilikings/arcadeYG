using UnityEngine;

namespace GAME.Extensions.Common
{
    public static class CommonExtensions
    {
        public static bool IsTrue(this bool? value)
        {
            return value.HasValue && value.Value;
        }

        public static bool IsFalseOrNull(this bool? value)
        {
            return !value.HasValue || !value.Value;
        }

        public static void SetAngleZ(this Transform transform, float angle)
        {
            var newRotation = transform.eulerAngles;
            newRotation.z = angle;
            transform.eulerAngles = newRotation;
        }

        public static void SetLocalAngleZ(this Transform transform, float angle)
        {
            var newRotation = transform.localEulerAngles;
            newRotation.z = angle;
            transform.localEulerAngles = newRotation;
        }

        public static void Rotate2D(this Transform transform, float angle, bool isLocal = false)
        {
            transform.Rotate(0, 0, angle, isLocal ? Space.Self : Space.World);
        }

    }
}