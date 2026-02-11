using UnityEngine;

namespace GAME.Extensions.Mono
{
    public static class MonoExt
    {
        public static MonoBehaviour Disable(this MonoBehaviour mono)
        {
            if (!mono) return null;
            mono.gameObject.SetActive(false);
            return mono;
        }

        public static MonoBehaviour Enable(this MonoBehaviour mono)
        {
            if (!mono) return null;
            mono.gameObject.SetActive(true);
            return mono;
        }
    }
}