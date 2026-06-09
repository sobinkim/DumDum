using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public static class RuntimeUiFactory
    {
        private const string KoreanFontResourcePath = "Fonts & Materials/NotoSansKR-Regular SDF";

        private static TMP_FontAsset _koreanFont;

        public static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject target = new GameObject(objectName);
            RectTransform rect = target.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        public static void Stretch(RectTransform rect)
        {
            Anchor(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        public static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static Image AddImage(GameObject target, Color color, bool raycastTarget)
        {
            Image image = target.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static LayoutElement AddLayout(GameObject target, float preferredWidth, float preferredHeight)
        {
            LayoutElement layout = target.GetComponent<LayoutElement>();
            if (layout == null)
                layout = target.AddComponent<LayoutElement>();

            if (preferredWidth >= 0f)
                layout.preferredWidth = preferredWidth;

            if (preferredHeight >= 0f)
                layout.preferredHeight = preferredHeight;

            return layout;
        }

        public static TMP_Text CreateText(RectTransform parent, string value, float size, FontStyles style, Color color, TextAlignmentOptions alignment)
        {
            RectTransform root = CreateRect("Text", parent);
            TextMeshProUGUI text = root.gameObject.AddComponent<TextMeshProUGUI>();
            TMP_FontAsset font = GetKoreanFont();
            if (font != null)
                text.font = font;

            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            AddLayout(root.gameObject, -1f, -1f);
            return text;
        }

        public static Button CreateButton(RectTransform parent, string label, Color background, Color textColor, float preferredWidth, float preferredHeight)
        {
            RectTransform root = CreateRect(label + " Button", parent);
            Image image = AddImage(root.gameObject, background, true);
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            root.gameObject.AddComponent<UIButtonFeedback>();
            AddLayout(root.gameObject, preferredWidth, preferredHeight);

            TMP_Text text = CreateText(root, label, 16f, FontStyles.Bold, textColor, TextAlignmentOptions.Center);
            Stretch(text.rectTransform);
            return button;
        }

        public static Slider CreateSlider(RectTransform parent, Color trackColor, Color fillColor, Color handleColor)
        {
            RectTransform root = CreateRect("Slider", parent);
            AddLayout(root.gameObject, -1f, 42f);

            Slider slider = root.gameObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;

            RectTransform background = CreateRect("Background", root);
            Anchor(background, Vector2.zero, Vector2.one, new Vector2(8f, 16f), new Vector2(-8f, -16f));
            AddImage(background.gameObject, trackColor, true);

            RectTransform fillArea = CreateRect("Fill Area", root);
            Anchor(fillArea, Vector2.zero, Vector2.one, new Vector2(8f, 16f), new Vector2(-8f, -16f));

            RectTransform fill = CreateRect("Fill", fillArea);
            Stretch(fill);
            AddImage(fill.gameObject, fillColor, true);

            RectTransform handleArea = CreateRect("Handle Slide Area", root);
            Stretch(handleArea);

            RectTransform handle = CreateRect("Handle", handleArea);
            Anchor(handle, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(-12f, -12f), new Vector2(12f, 12f));
            Image handleImage = AddImage(handle.gameObject, handleColor, true);

            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static TMP_FontAsset GetKoreanFont()
        {
            if (_koreanFont == null)
                _koreanFont = Resources.Load<TMP_FontAsset>(KoreanFontResourcePath);

            return _koreanFont;
        }
    }
}
