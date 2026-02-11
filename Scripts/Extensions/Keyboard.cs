using UnityEngine.InputSystem;

namespace GAME.Extensions.Input
{
    public static class KeyboardExt
    {
        public static bool IsEnter(this Keyboard kb) => kb != null && kb.enterKey.wasPressedThisFrame;

        public static bool IsSpace(this Keyboard kb) => kb != null && kb.spaceKey.wasPressedThisFrame;

        public static bool IsEscape(this Keyboard kb) => kb != null && kb.escapeKey.wasPressedThisFrame;

        public static bool IsAny(this Keyboard kb) => kb != null && kb.anyKey.wasPressedThisFrame;
    }
}