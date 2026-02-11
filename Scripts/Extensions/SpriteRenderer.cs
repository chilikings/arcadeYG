using UnityEngine;
using GAME.Utils.Calculation;

namespace GAME.Extensions.Sprites
{
    public static class SpriteExt
    {
        const float _ScreenError = 1.01f;


        public static void FillScreen(this SpriteRenderer sr) => sr.size = Calc.ScreenSize() * _ScreenError;
        
        public static void FitToScreen(this SpriteRenderer sr, float factor = 1)
        {
            if (!sr) return;
            Vector2 screenSize = Calc.ScreenSize() * factor, spriteSize = sr.sprite.bounds.size;
            var scale = Mathf.Max(screenSize.x / spriteSize.x, screenSize.y / spriteSize.y);
            sr.size = spriteSize * scale * _ScreenError;
        }

        public static void FitToSize(this SpriteRenderer sr, Vector2 size)
        {
            if (!sr) return;
            var spriteSize = sr.sprite.bounds.size;
            var scale = Mathf.Max(size.x / spriteSize.x, size.y / spriteSize.y);
            sr.size = spriteSize * scale;
        }

        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Gradient SetColor(this Gradient gradient, Color color)
        {
            gradient ??= new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(color.a, 1f) }
            );
            return gradient;
        }

        public static Vector2 GetUnitSize(this Sprite s) => s?.bounds.size ?? Vector2.zero;
        public static Vector2 GetPixelSize(this Sprite s) => s?.rect.size ?? Vector2.zero;

        public static float GetSpriteAspect(this SpriteRenderer sr) => sr?.sprite.GetAspectRatio() ?? 1f;
        public static float GetRenderAspect(this SpriteRenderer sr) => sr != null && sr.size.y != 0f ? sr.size.x / sr.size.y : 1f;
        public static float GetAspectRatio(this Sprite s) => s != null && s.rect.height != 0f ? s.rect.width / s.rect.height : 1f;

    }
}