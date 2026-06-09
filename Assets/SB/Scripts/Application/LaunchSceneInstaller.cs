using SB.App.Views;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SB.App.Application
{
    public sealed class LaunchSceneInstaller : MonoBehaviour
    {
        private static readonly Color TextColor = new Color(0.13f, 0.14f, 0.16f);
        private static readonly Color PrimaryColor = new Color(0.72f, 0.08f, 0.92f);

        private void Awake()
        {
            SoundManager.GetOrCreate();
            AppSceneNavigator.GetOrCreate();

            if (FindFirstObjectByType<LaunchScreenView>() != null)
                return;

            Canvas canvas = CreateCanvas();
            EnsureEventSystem();

            RectTransform launchRoot = RuntimeUiFactory.CreateRect("Launch Screen View", canvas.transform);
            RuntimeUiFactory.Stretch(launchRoot);
            RuntimeUiFactory.AddImage(launchRoot.gameObject, new Color(0.98f, 0.96f, 1f), true);

            VerticalLayoutGroup layout = launchRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 176, 96);
            layout.spacing = 18f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            TMP_Text title = RuntimeUiFactory.CreateText(launchRoot, "DumDum", 42f, FontStyles.Bold, PrimaryColor, TextAlignmentOptions.Center);
            RuntimeUiFactory.AddLayout(title.gameObject, -1f, 64f);

            TMP_Text subtitle = RuntimeUiFactory.CreateText(launchRoot, "걱정을 생각의 흐름으로 정리해요.", 18f, FontStyles.Normal, TextColor, TextAlignmentOptions.Center);
            RuntimeUiFactory.AddLayout(subtitle.gameObject, -1f, 44f);

            TMP_Text tapHint = RuntimeUiFactory.CreateText(launchRoot, "화면을 터치해서 시작하기", 16f, FontStyles.Bold, new Color(0.46f, 0.23f, 0.72f), TextAlignmentOptions.Center);
            RuntimeUiFactory.AddLayout(tapHint.gameObject, -1f, 56f);

            LaunchScreenView launchView = launchRoot.gameObject.AddComponent<LaunchScreenView>();
            launchView.SetRoot(launchRoot.gameObject);

            GameObject presenterRoot = new GameObject("Launch Screen Presenter");
            LaunchScreenPresenter presenter = presenterRoot.AddComponent<LaunchScreenPresenter>();
            presenter.SetComposition(launchView, AppSceneNavigator.MainSceneName);
        }

        private static Canvas CreateCanvas()
        {
            Canvas existingCanvas = FindFirstObjectByType<Canvas>();
            if (existingCanvas != null)
                return existingCanvas;

            GameObject canvasObject = new GameObject("DumDum Launch Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(375f, 667f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
                return;

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }
}
