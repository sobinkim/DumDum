using SB.App.Views;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SB.App.Application
{
    public sealed class SoundSettingsInstaller : MonoBehaviour
    {
        private static readonly Color TextColor = new Color(0.13f, 0.14f, 0.16f);
        private static readonly Color PrimaryColor = new Color(0.11f, 0.44f, 0.53f);
        private static readonly Color SecondaryColor = new Color(0.88f, 0.91f, 0.86f);
        private static readonly Color PurpleColor = new Color(0.72f, 0.08f, 0.92f);
        private static readonly Color PanelColor = new Color(0.99f, 0.97f, 1f);

        private void Awake()
        {
            SoundManager.GetOrCreate();

            if (FindSceneComponent<SoundSettingsPanelView>() != null)
                return;

            RectTransform frame = FindSceneRect("App Frame");
            if (frame == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                frame = canvas != null ? canvas.transform as RectTransform : null;
            }

            if (frame == null)
                return;

            RectTransform header = FindSceneRect("Whiteboard Header");
            Button openButton = CreateOpenButton(header, frame);
            CreatePanel(frame, openButton);
        }

        private static Button CreateOpenButton(RectTransform header, RectTransform fallbackParent)
        {
            RectTransform parent = header != null ? header : fallbackParent;
            Button openButton = RuntimeUiFactory.CreateButton(parent, "소리", new Color(0.97f, 0.91f, 1f), PurpleColor, 54f, 54f);
            RectTransform buttonRect = openButton.GetComponent<RectTransform>();

            if (header != null)
            {
                NarrowWhiteboardTitle(header);
                RuntimeUiFactory.Anchor(buttonRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-116f, -66f), new Vector2(-62f, -12f));
            }
            else
            {
                RuntimeUiFactory.Anchor(buttonRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-78f, -78f), new Vector2(-24f, -24f));
            }

            Outline outline = openButton.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.84f, 0.66f, 0.98f, 0.55f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return openButton;
        }

        private static void CreatePanel(RectTransform parent, Button openButton)
        {
            RectTransform controller = RuntimeUiFactory.CreateRect("Sound Settings View", parent);
            RuntimeUiFactory.Stretch(controller);

            RectTransform overlay = RuntimeUiFactory.CreateRect("Sound Settings Panel", controller);
            RuntimeUiFactory.Stretch(overlay);
            RuntimeUiFactory.AddImage(overlay.gameObject, new Color(0.08f, 0.08f, 0.1f, 0.36f), true);

            RectTransform panel = RuntimeUiFactory.CreateRect("Panel", overlay);
            RuntimeUiFactory.Anchor(panel, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(24f, -138f), new Vector2(-24f, 138f));
            RuntimeUiFactory.AddImage(panel.gameObject, PanelColor, true);

            VerticalLayoutGroup panelLayout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(20, 20, 18, 18);
            panelLayout.spacing = 14f;
            panelLayout.childControlHeight = false;
            panelLayout.childControlWidth = true;

            RectTransform header = RuntimeUiFactory.CreateRect("Sound Header", panel);
            HorizontalLayoutGroup headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 10f;
            headerLayout.childControlHeight = false;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            RuntimeUiFactory.AddLayout(header.gameObject, -1f, 42f);

            TMP_Text title = RuntimeUiFactory.CreateText(header, "사운드 설정", 22f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            RuntimeUiFactory.AddLayout(title.gameObject, -1f, 42f);
            Button closeButton = RuntimeUiFactory.CreateButton(header, "닫기", SecondaryColor, TextColor, 72f, 38f);

            RectTransform volumeRow = RuntimeUiFactory.CreateRect("Volume Row", panel);
            HorizontalLayoutGroup volumeLayout = volumeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            volumeLayout.spacing = 10f;
            volumeLayout.childControlHeight = false;
            volumeLayout.childControlWidth = true;
            volumeLayout.childForceExpandWidth = false;
            volumeLayout.childAlignment = TextAnchor.MiddleCenter;
            RuntimeUiFactory.AddLayout(volumeRow.gameObject, -1f, 30f);

            TMP_Text volumeLabel = RuntimeUiFactory.CreateText(volumeRow, "전체 소리", 16f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            RuntimeUiFactory.AddLayout(volumeLabel.gameObject, -1f, 30f);
            TMP_Text volumeValue = RuntimeUiFactory.CreateText(volumeRow, "80%", 16f, FontStyles.Bold, PurpleColor, TextAlignmentOptions.Right);
            RuntimeUiFactory.AddLayout(volumeValue.gameObject, 58f, 30f);

            Slider slider = RuntimeUiFactory.CreateSlider(panel, SecondaryColor, PrimaryColor, new Color(0.93f, 0.72f, 0.25f));
            slider.value = 0.8f;

            Button muteButton = RuntimeUiFactory.CreateButton(panel, "음소거", PrimaryColor, Color.white, -1f, 46f);
            TMP_Text muteLabel = muteButton.GetComponentInChildren<TMP_Text>(true);

            SoundSettingsPanelView view = controller.gameObject.AddComponent<SoundSettingsPanelView>();
            view.SetControls(overlay.gameObject, openButton, closeButton, muteButton, slider, volumeValue, muteLabel);
            overlay.gameObject.SetActive(false);
        }

        private static void NarrowWhiteboardTitle(RectTransform header)
        {
            TMP_Text[] texts = header.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (text == null || !text.text.Contains("걱정"))
                    continue;

                RectTransform rect = text.rectTransform;
                Vector2 offsetMax = rect.offsetMax;
                if (offsetMax.x > -132f)
                    rect.offsetMax = new Vector2(-132f, offsetMax.y);

                return;
            }
        }

        private static RectTransform FindSceneRect(string objectName)
        {
            RectTransform[] rects = Resources.FindObjectsOfTypeAll<RectTransform>();
            Scene activeScene = SceneManager.GetActiveScene();

            for (int i = 0; i < rects.Length; i++)
            {
                RectTransform rect = rects[i];
                if (rect == null || rect.name != objectName || rect.gameObject.scene != activeScene)
                    continue;

                return rect;
            }

            return null;
        }

        private static T FindSceneComponent<T>() where T : Component
        {
            T[] components = Resources.FindObjectsOfTypeAll<T>();
            Scene activeScene = SceneManager.GetActiveScene();

            for (int i = 0; i < components.Length; i++)
            {
                T component = components[i];
                if (component != null && component.gameObject.scene == activeScene)
                    return component;
            }

            return null;
        }
    }
}
