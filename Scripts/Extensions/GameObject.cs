using UnityEngine;

namespace GAME.Extensions.Objects
{
    public static class ObjectExt
    {
        public static bool IsPicture(this Transform transform) => IsLayer(transform, "Picture");
        public static bool IsPrefab(this Transform transform) => !IsObj(transform);
        public static bool IsObject(this Transform transform) => IsObj(transform);
        public static bool IsLayer(this Transform transform, string layerName) => transform.gameObject.IsLayer(layerName);
        public static bool IsLayer(this GameObject gameObject, string layerName) => gameObject.layer == LayerMask.NameToLayer(layerName);
   
        static bool IsObj(Transform transform) => transform ? transform.gameObject.scene.isLoaded : false;
    }
}