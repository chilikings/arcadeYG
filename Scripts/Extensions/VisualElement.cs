using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace GAME.Extensions.UI
{
    public static class UIElementExt
    {
        public static T SetDisplay<T>(this T element, bool display) where T : VisualElement => display ? element.Show() : element.Hide();
        //public static T SwitchDisplay<T>(this T element) where T : VisualElement => element.IsDisplayed() ? element.Hide() : element.Show();
        //public static T SwitchVisibleShowFirst<T>(this T element) where T : VisualElement => element.IsDisplayed() ? element.Show() : element.Hide();
        //public static bool IsDisplayed<T>(this T element) where T : VisualElement => element is not null && element.style.display != DisplayStyle.None;

        public static void SetSquareSize(this VisualElement elem, float size) => elem.style.width = elem.style.height = size;

        public static void SetWidthFromHeight(this VisualElement elem, float aspect, float? height = null) => elem.style.width = height ??= elem.layout.height * aspect;

        public static void SetHeightFromWidth(this VisualElement elem, float aspect, float? width = null) => elem.style.height = width ??= elem.layout.width / aspect;

        public static T Show<T>(this T elem) where T : VisualElement
        {
            if (elem != null) elem.style.display = DisplayStyle.Flex;
            return elem;
        }

        public static T Hide<T>(this T elem) where T : VisualElement
        {
            if (elem != null) elem.style.display = DisplayStyle.None;
            return elem;
        }

        //public static T SetDisplay<T>(this T element, DisplayStyle display) where T : VisualElement
        //{
        //    if (element != null) element.style.display = display;
        //    return element;
        //}

        public static T SetSize<T>(this T element, float width, float height, LengthUnit unit = LengthUnit.Percent) where T : VisualElement
        {
            return element?.SetWidth(width, unit).SetHeight(height, unit);
        }

        public static T SetWidth<T>(this T element, float value, LengthUnit unit = LengthUnit.Percent) where T : VisualElement
        {
            if (element != null) element.style.width = new Length(value, unit);
            return element;
        }

        public static T SetHeight<T>(this T element, float value, LengthUnit unit = LengthUnit.Percent) where T : VisualElement
        {
            if (element != null) element.style.height = new Length(value, unit);
            return element;
        }

        public static T SetSquareOnResize<T>(this T element) where T : VisualElement
        {
            element?.RegisterCallback<GeometryChangedEvent>(evt => element.SetWidth(evt.newRect.height, LengthUnit.Pixel));
            return element;
        }

        public static T SetWidthOnResize<T>(this T element, float aspect) where T : VisualElement
        {
            element?.RegisterCallback<GeometryChangedEvent>(evt => element.SetWidth(evt.newRect.height * aspect, LengthUnit.Pixel));
            return element;
        }

        public static T SetHeightOnResize<T>(this T element, float aspect) where T : VisualElement
        {
            element?.RegisterCallback<GeometryChangedEvent>(evt => element.SetHeight(evt.newRect.width / aspect, LengthUnit.Pixel));
            return element;
        }

        public static T SetColor<T>(this T element, Gradient gradient, float time = 0f) where T : VisualElement => element?.SetColor(new StyleColor(gradient.Evaluate(time)));

        public static T SetColor<T>(this T element, StyleColor color) where T : VisualElement
        {
            if (element != null) element.style.backgroundColor = color;
            return element;
        }

        public static void EnableClass(this VisualElement element, string className, bool enable)
        {
            if (enable) element.AddToClassList(className);
            else element.RemoveFromClassList(className);
        }

    }
}