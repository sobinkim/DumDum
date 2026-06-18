using SB.App.Views;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SB.App.Application
{
    public sealed class SoundSettingsInstaller : MonoBehaviour
    {
        private const string SoundButtonName = "\uC18C\uB9AC Button";
        private static readonly Color PurpleColor = new Color(0.72f, 0.08f, 0.92f);

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Awake()
        {
            Install();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Install();
        }

        private void Install()
        {
            SoundManager.GetOrCreate();
            RemoveLegacySoundSettingsViews();

            RectTransform frame = FindSceneRect("App Frame");
            if (frame == null)
            {
                Canvas canvas = FindFirstObjectByType<Canvas>();
                frame = canvas != null ? canvas.transform as RectTransform : null;
            }

            if (frame == null)
                return;

            RectTransform header = FindSceneRect("Whiteboard Header");
            Button soundButton = FindExistingSoundButton(header);
            if (soundButton == null)
                soundButton = CreateSoundButton(header, frame);

            SoundSettingsPanelView toggleView = soundButton.GetComponent<SoundSettingsPanelView>();
            if (toggleView == null)
                toggleView = soundButton.gameObject.AddComponent<SoundSettingsPanelView>();

            toggleView.SetControls(soundButton);
        }

        private static Button CreateSoundButton(RectTransform header, RectTransform fallbackParent)
        {
            RectTransform parent = header != null ? header : fallbackParent;
            Button soundButton = RuntimeUiFactory.CreateButton(parent, string.Empty, Color.white, PurpleColor, 54f, 54f);
            soundButton.gameObject.name = SoundButtonName;
            RectTransform buttonRect = soundButton.GetComponent<RectTransform>();

            if (header != null)
            {
                NarrowWhiteboardTitle(header);
                RuntimeUiFactory.Anchor(buttonRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-116f, -66f), new Vector2(-62f, -12f));
            }
            else
            {
                RuntimeUiFactory.Anchor(buttonRect, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-78f, -78f), new Vector2(-24f, -24f));
            }

            return soundButton;
        }

        private static void RemoveLegacySoundSettingsViews()
        {
            RectTransform[] rects = Resources.FindObjectsOfTypeAll<RectTransform>();
            Scene activeScene = SceneManager.GetActiveScene();

            for (int i = 0; i < rects.Length; i++)
            {
                RectTransform rect = rects[i];
                if (rect == null || rect.name != "Sound Settings View" || rect.gameObject.scene != activeScene)
                    continue;

                if (UnityEngine.Application.isPlaying)
                    Destroy(rect.gameObject);
                else
                    DestroyImmediate(rect.gameObject);
            }
        }

        private static Button FindExistingSoundButton(RectTransform header)
        {
            if (header == null)
                return null;

            Button[] buttons = header.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == null)
                    continue;

                RectTransform rect = button.transform as RectTransform;
                if (rect == null)
                    continue;

                if (button.name == SoundButtonName)
                    return button;

                bool isSoundButtonSlot =
                    rect.anchorMin == new Vector2(1f, 1f) &&
                    rect.anchorMax == new Vector2(1f, 1f) &&
                    Mathf.Approximately(rect.offsetMin.x, -116f) &&
                    Mathf.Approximately(rect.offsetMax.x, -62f);

                if (isSoundButtonSlot)
                    return button;
            }

            return null;
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
    }
}
