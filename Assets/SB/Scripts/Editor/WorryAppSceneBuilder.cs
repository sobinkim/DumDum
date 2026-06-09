using SB.App.Application;
using SB.App.Domain;
using SB.App.Views;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SB.App.Editor
{
    public static class WorryAppSceneBuilder
    {
        private const string StartScenePath = "Assets/SB/Scene/Start.unity";
        private const string MainScenePath = "Assets/SB/Scene/Main.unity";
        private const string FlowContentPath = "Assets/SB/ScriptableObjects/WorryFlowContent.asset";
        private const string NotificationCatalogPath = "Assets/SB/ScriptableObjects/NotificationCatalog.asset";
        private const string CardPrefabPath = "Assets/SB/03.Prefab/WorryCardView.prefab";
        private const string KoreanFontAssetPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSansKR-Regular SDF.asset";

        private static TMP_FontAsset _koreanFont;

        private static readonly Color BackgroundColor = new Color(0.96f, 0.97f, 0.95f);
        private static readonly Color BoardColor = Color.white;
        private static readonly Color TextColor = new Color(0.13f, 0.14f, 0.16f);
        private static readonly Color MutedTextColor = new Color(0.43f, 0.46f, 0.5f);
        private static readonly Color PrimaryColor = new Color(0.11f, 0.44f, 0.53f);
        private static readonly Color SecondaryColor = new Color(0.88f, 0.91f, 0.86f);
        private static readonly Color AccentColor = new Color(0.93f, 0.72f, 0.25f);

        [MenuItem("DumDum/Rebuild App Scenes")]
        public static void RebuildAppScenes()
        {
            RebuildStartScene();
            RebuildMainScene();
            UpdateBuildSettings();
        }

        [MenuItem("DumDum/Rebuild Start Scene")]
        public static void RebuildStartScene()
        {
            EnsureFolders();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Start";

            Canvas canvas = CreateCanvas();

            RectTransform launchRoot = CreateRect("Launch Screen View", canvas.transform);
            Stretch(launchRoot);
            AddImage(launchRoot.gameObject, new Color(0.98f, 0.96f, 1f), true);

            VerticalLayoutGroup layout = launchRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 176, 96);
            layout.spacing = 18f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            TMP_Text title = CreateText(launchRoot, "DumDum", 42f, FontStyles.Bold, new Color(0.72f, 0.08f, 0.92f), TextAlignmentOptions.Center);
            AddLayout(title.gameObject, -1f, 64f);

            TMP_Text subtitle = CreateText(launchRoot, "걱정을 생각의 흐름으로 정리해요.", 18f, FontStyles.Normal, TextColor, TextAlignmentOptions.Center);
            AddLayout(subtitle.gameObject, -1f, 44f);

            TMP_Text tapHint = CreateText(launchRoot, "화면을 터치해서 시작하기", 16f, FontStyles.Bold, new Color(0.46f, 0.23f, 0.72f), TextAlignmentOptions.Center);
            AddLayout(tapHint.gameObject, -1f, 56f);

            LaunchScreenView launchView = launchRoot.gameObject.AddComponent<LaunchScreenView>();
            launchView.SetRoot(launchRoot.gameObject);

            GameObject managersRoot = new GameObject("App Managers");
            managersRoot.AddComponent<AppSceneNavigator>();
            managersRoot.AddComponent<SoundManager>();

            GameObject presentersRoot = new GameObject("Presenters");
            LaunchScreenPresenter presenter = presentersRoot.AddComponent<LaunchScreenPresenter>();
            presenter.SetComposition(launchView, AppSceneNavigator.MainSceneName);

            EditorUtility.SetDirty(launchView);
            EditorUtility.SetDirty(presenter);

            EditorSceneManager.SaveScene(scene, StartScenePath);
            UpdateBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("DumDum/Rebuild Main Scene")]
        public static void RebuildMainScene()
        {
            EnsureFolders();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Main";

            WorryFlowContent content = CreateOrUpdateFlowContent();
            NotificationCatalog notificationCatalog = CreateOrUpdateNotificationCatalog();
            WorryCardView cardPrefab = CreateOrUpdateCardPrefab();

            Canvas canvas = CreateCanvas();
            RectTransform screenBackground = CreateRect("Screen Background", canvas.transform);
            Stretch(screenBackground);
            AddImage(screenBackground.gameObject, BackgroundColor, false);

            RectTransform shell = CreateRect("App Frame", canvas.transform);
            shell.sizeDelta = new Vector2(375f, 667f);
            AddImage(shell.gameObject, BoardColor, false);
            ResponsiveScaledFrame frame = shell.gameObject.AddComponent<ResponsiveScaledFrame>();
            frame.SetFrame(shell, new Vector2(375f, 667f));
            MobileKeyboardAvoider keyboardAvoider = shell.gameObject.AddComponent<MobileKeyboardAvoider>();
            keyboardAvoider.SetTarget(shell, canvas);

            WhiteboardView whiteboardView = CreateWhiteboard(shell, cardPrefab);
            WorryDetailView detailView = CreateDetail(shell);
            MainMenuView mainMenuView = null;
            FocusOverlayView focusOverlayView = CreateFocusOverlay(shell);
            StepProgressView progressView = CreateProgress(shell);
            CompanionBubbleView companionBubbleView = CreateCompanionBubble(shell);
            FeedbackPopupView feedbackPopupView = CreateFeedbackPopup(shell);

            WorryInputView worryInputView = CreateTextStep<WorryInputView>(
                shell,
                "Worry Input View",
                "고민 적기",
                "지금 머릿속에 맴도는 걱정을 가능한 한 구체적으로 적어보세요.",
                "다음",
                "취소",
                "예: 발표를 망칠까 봐 걱정돼요.");

            ProbabilityEstimateView probabilityEstimateView = CreateProbabilityStep(shell);

            CopingPlanView copingPlanView = CreateCopingActionsStep(shell);
            ActionPlanView actionPlanView = CreateSummaryStep(shell);

            EmotionCheckView emotionCheckView = CreateEmotionStep(shell);
            DailyClosureView dailyClosureView = CreateDailyClosure(shell);

            GameObject managersRoot = new GameObject("App Managers");
            managersRoot.AddComponent<AppSceneNavigator>();
            managersRoot.AddComponent<SoundManager>();

            GameObject presentersRoot = new GameObject("Presenters");
            WhiteboardPresenter whiteboardPresenter = presentersRoot.AddComponent<WhiteboardPresenter>();
            whiteboardPresenter.SetViews(whiteboardView, detailView);

            WorryFlowPresenter flowPresenter = presentersRoot.AddComponent<WorryFlowPresenter>();
            flowPresenter.SetComposition(
                content,
                whiteboardPresenter,
                mainMenuView,
                companionBubbleView,
                focusOverlayView,
                feedbackPopupView,
                progressView,
                worryInputView,
                probabilityEstimateView,
                copingPlanView,
                actionPlanView,
                emotionCheckView,
                dailyClosureView,
                notificationCatalog);

            EditorUtility.SetDirty(whiteboardPresenter);
            EditorUtility.SetDirty(flowPresenter);

            EditorSceneManager.SaveScene(scene, MainScenePath);
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets/SB", "Scripts");
            CreateFolderIfMissing("Assets/SB/Scripts", "Editor");
            CreateFolderIfMissing("Assets/SB", "ScriptableObjects");
            CreateFolderIfMissing("Assets/SB", "03.Prefab");
            CreateFolderIfMissing("Assets/SB", "Scene");
        }

        private static void UpdateBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath) != null)
                scenes.Add(new EditorBuildSettingsScene(StartScenePath, true));

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath) != null)
                scenes.Add(new EditorBuildSettingsScene(MainScenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void CreateFolderIfMissing(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static WorryFlowContent CreateOrUpdateFlowContent()
        {
            WorryFlowContent content = AssetDatabase.LoadAssetAtPath<WorryFlowContent>(FlowContentPath);
            if (content == null)
            {
                content = ScriptableObject.CreateInstance<WorryFlowContent>();
                AssetDatabase.CreateAsset(content, FlowContentPath);
            }

            content.SetRuntimeContent(
                8,
                new[]
                {
                    Step(WorryFlowStep.WorryInput, "고민 적기", "먼저 걱정을 화이트보드 위에 꺼내볼게요.", "다음", "취소"),
                    Step(WorryFlowStep.ProbabilityEstimate, "확률 가늠하기", "이 걱정이 실제로 일어날 것 같은 정도를 숫자로 잡아봐요.", "다음", "이전"),
                    Step(WorryFlowStep.CopingPlan, "최악의 경우 대비하기", "정말 일어난다면 그때 할 수 있는 일을 여러 개 적어봐요.", "다음", "이전"),
                    Step(WorryFlowStep.ActionPlan, "지금 할 수 있는 행동 정하기", "통제 가능한 작은 행동 하나를 정해요.", "다음", "이전"),
                    Step(WorryFlowStep.EmotionCheck, "마음 상태 확인하기", "생각을 정리한 뒤 지금 마음이 어떤지 표시해요.", "저장", "이전")
                });

            EditorUtility.SetDirty(content);
            return content;
        }

        private static WorryStepContent Step(WorryFlowStep step, string title, string message, string primary, string secondary)
        {
            return new WorryStepContent
            {
                step = step,
                title = title,
                companionMessage = message,
                primaryActionLabel = primary,
                secondaryActionLabel = secondary
            };
        }

        private static NotificationCatalog CreateOrUpdateNotificationCatalog()
        {
            NotificationCatalog catalog = AssetDatabase.LoadAssetAtPath<NotificationCatalog>(NotificationCatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<NotificationCatalog>();
                AssetDatabase.CreateAsset(catalog, NotificationCatalogPath);
            }

            if (catalog.Rules.Count <= 0)
                catalog.SetRuntimeRules(NotificationCatalog.CreateDefaultRules());

            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        private static WorryCardView CreateOrUpdateCardPrefab()
        {
            GameObject prefabRoot = new GameObject("WorryCardView");
            RectTransform root = prefabRoot.AddComponent<RectTransform>();
            root.sizeDelta = new Vector2(150f, 150f);

            WorryCardView view = CreateCardTemplate(root);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, CardPrefabPath);
            Object.DestroyImmediate(prefabRoot);

            return AssetDatabase.LoadAssetAtPath<WorryCardView>(CardPrefabPath);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("DumDum Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(375f, 667f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();

            return canvas;
        }

        private static MainMenuView CreateMainMenu(RectTransform parent)
        {
            RectTransform root = CreateRect("Main Menu View", parent);
            Anchor(root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -88f), new Vector2(-24f, -24f));

            HorizontalLayoutGroup layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleRight;

            Button startButton = CreateButton(root, "고민 정리 시작", PrimaryColor, Color.white, 170f, 48f);

            MainMenuView view = root.gameObject.AddComponent<MainMenuView>();
            view.SetRoot(root.gameObject);
            view.SetControls(startButton);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static WhiteboardView CreateWhiteboard(RectTransform parent, WorryCardView cardPrefab)
        {
            RectTransform root = CreateRect("Whiteboard View", parent);
            Stretch(root);
            AddImage(root.gameObject, BoardColor, true);

            RectTransform header = CreateRect("Whiteboard Header", root);
            Anchor(header, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -112f), new Vector2(-20f, -16f));

            TMP_Text title = CreateText(header, "내 걱정 정리함 ✨", 29f, FontStyles.Normal, new Color(0.72f, 0.08f, 0.92f), TextAlignmentOptions.Left);
            Anchor(title.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-132f, 0f));

            Button soundButton = CreateButton(header, "소리", new Color(0.97f, 0.91f, 1f), new Color(0.72f, 0.08f, 0.92f), 54f, 54f);
            Anchor(soundButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-116f, -66f), new Vector2(-62f, -12f));
            Outline soundOutline = soundButton.gameObject.AddComponent<Outline>();
            soundOutline.effectColor = new Color(0.84f, 0.66f, 0.98f, 0.55f);
            soundOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Button summaryButton = CreateButton(header, "통계", new Color(0.97f, 0.91f, 1f), new Color(0.72f, 0.08f, 0.92f), 54f, 54f);
            Anchor(summaryButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-54f, -66f), new Vector2(0f, -12f));
            Outline summaryOutline = summaryButton.gameObject.AddComponent<Outline>();
            summaryOutline.effectColor = new Color(0.84f, 0.66f, 0.98f, 0.55f);
            summaryOutline.effectDistance = new Vector2(1.5f, -1.5f);

            RectTransform scrollRoot = CreateRect("Whiteboard Scroll", root);
            Anchor(scrollRoot, Vector2.zero, Vector2.one, new Vector2(20f, 94f), new Vector2(-20f, -124f));

            ScrollRect scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 2.2f;

            RectTransform viewport = CreateRect("Viewport", scrollRoot);
            Stretch(viewport);
            Image viewportImage = AddImage(viewport.gameObject, Color.white, true);
            Mask viewportMask = viewport.gameObject.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            RectTransform content = CreateRect("Board Content", viewport);
            content.pivot = new Vector2(0.5f, 1f);
            Anchor(content, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            VerticalLayoutGroup contentLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(0, 0, 20, 36);
            contentLayout.spacing = 22f;
            contentLayout.childControlHeight = false;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;

            ContentSizeFitter contentFitter = content.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;

            TMP_Text emptyState = CreateText(content, "아직 고민 카드가 없어요.\n걱정 하나를 행동 계획으로 바꿔보세요.", 18f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Center);
            AddLayout(emptyState.gameObject, -1f, 74f);

            RectTransform cardRoot = CreateRect("Card Grid", content);
            GridLayoutGroup cardGrid = cardRoot.gameObject.AddComponent<GridLayoutGroup>();
            cardGrid.cellSize = new Vector2(150f, 176f);
            cardGrid.spacing = new Vector2(15f, 34f);
            cardGrid.padding = new RectOffset(8, 8, 10, 20);
            cardGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            cardGrid.constraintCount = 2;
            cardGrid.childAlignment = TextAnchor.UpperLeft;

            ContentSizeFitter gridFitter = cardRoot.gameObject.AddComponent<ContentSizeFitter>();
            gridFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            AddLayout(cardRoot.gameObject, -1f, 0f);

            Button createButton = CreateButton(content, "+  새로운 걱정 정리하기", new Color(0.99f, 0.96f, 1f), new Color(0.72f, 0.08f, 0.92f), -1f, 58f);
            Outline createOutline = createButton.gameObject.AddComponent<Outline>();
            createOutline.effectColor = new Color(0.78f, 0.42f, 0.96f, 0.55f);
            createOutline.effectDistance = new Vector2(1.2f, -1.2f);

            Button dailyCloseButton = CreateButton(root, "🌙 오늘 하루 마무리", new Color(0.98f, 0.96f, 1f), new Color(0.46f, 0.23f, 0.72f), -1f, 52f);
            RectTransform dailyCloseRect = dailyCloseButton.GetComponent<RectTransform>();
            Anchor(dailyCloseRect, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(44f, 28f), new Vector2(-44f, 80f));
            Outline dailyCloseOutline = dailyCloseButton.gameObject.AddComponent<Outline>();
            dailyCloseOutline.effectColor = new Color(0.76f, 0.62f, 0.95f, 0.45f);
            dailyCloseOutline.effectDistance = new Vector2(1.2f, -1.2f);

            WhiteboardView view = root.gameObject.AddComponent<WhiteboardView>();
            OutcomeSummaryView outcomeSummaryView = CreateOutcomeSummary(parent);
            CreateSoundSettings(parent, soundButton);
            view.SetRoot(root.gameObject);
            view.SetControls(cardRoot, cardPrefab, emptyState.gameObject, createButton, summaryButton, outcomeSummaryView);
            view.SetDailyCloseButton(dailyCloseButton);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static SoundSettingsPanelView CreateSoundSettings(RectTransform parent, Button openButton)
        {
            RectTransform controller = CreateRect("Sound Settings View", parent);
            Stretch(controller);

            RectTransform overlay = CreateRect("Sound Settings Panel", controller);
            Stretch(overlay);
            AddImage(overlay.gameObject, new Color(0.08f, 0.08f, 0.1f, 0.36f), true);

            RectTransform panel = CreateRect("Panel", overlay);
            Anchor(panel, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(24f, -138f), new Vector2(-24f, 138f));
            AddImage(panel.gameObject, new Color(0.99f, 0.97f, 1f), true);

            VerticalLayoutGroup panelLayout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(20, 20, 18, 18);
            panelLayout.spacing = 14f;
            panelLayout.childControlHeight = false;
            panelLayout.childControlWidth = true;

            RectTransform header = CreateRect("Sound Header", panel);
            HorizontalLayoutGroup headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 10f;
            headerLayout.childControlHeight = false;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            AddLayout(header.gameObject, -1f, 42f);

            TMP_Text title = CreateText(header, "사운드 설정", 22f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            AddLayout(title.gameObject, -1f, 42f);
            Button closeButton = CreateButton(header, "닫기", SecondaryColor, TextColor, 72f, 38f);

            RectTransform volumeRow = CreateRect("Volume Row", panel);
            HorizontalLayoutGroup volumeRowLayout = volumeRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            volumeRowLayout.spacing = 10f;
            volumeRowLayout.childControlHeight = false;
            volumeRowLayout.childControlWidth = true;
            volumeRowLayout.childForceExpandWidth = false;
            volumeRowLayout.childAlignment = TextAnchor.MiddleCenter;
            AddLayout(volumeRow.gameObject, -1f, 30f);

            TMP_Text volumeLabel = CreateText(volumeRow, "전체 소리", 16f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            AddLayout(volumeLabel.gameObject, -1f, 30f);
            TMP_Text volumeValue = CreateText(volumeRow, "80%", 16f, FontStyles.Bold, new Color(0.72f, 0.08f, 0.92f), TextAlignmentOptions.Right);
            AddLayout(volumeValue.gameObject, 58f, 30f);

            Slider slider = CreateSlider(panel);
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.value = 0.8f;

            Button muteButton = CreateButton(panel, "음소거", PrimaryColor, Color.white, -1f, 46f);
            TMP_Text muteLabel = muteButton.GetComponentInChildren<TMP_Text>(true);

            SoundSettingsPanelView view = controller.gameObject.AddComponent<SoundSettingsPanelView>();
            view.SetControls(overlay.gameObject, openButton, closeButton, muteButton, slider, volumeValue, muteLabel);
            overlay.gameObject.SetActive(false);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static OutcomeSummaryView CreateOutcomeSummary(RectTransform parent)
        {
            RectTransform root = CreateRect("Outcome Summary View", parent);
            Anchor(root, Vector2.zero, Vector2.one, new Vector2(20f, 28f), new Vector2(-20f, -28f));
            AddImage(root.gameObject, new Color(0.99f, 0.97f, 1f), true);

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 16f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;

            RectTransform header = CreateRect("Summary Header", root);
            HorizontalLayoutGroup headerLayout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            headerLayout.spacing = 12f;
            headerLayout.childControlHeight = false;
            headerLayout.childControlWidth = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childAlignment = TextAnchor.MiddleCenter;
            AddLayout(header.gameObject, -1f, 44f);

            TMP_Text title = CreateText(header, "걱정 통계", 24f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            AddLayout(title.gameObject, -1f, 40f);

            Button closeButton = CreateButton(header, "돌아가기", SecondaryColor, TextColor, 92f, 36f);

            TMP_Text reviewed = CreateText(root, "아직 고민 카드가 없어요.", 15f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Center);
            AddLayout(reviewed.gameObject, -1f, 40f);

            RectTransform metrics = CreateRect("Outcome Metrics", root);
            HorizontalLayoutGroup metricLayout = metrics.gameObject.AddComponent<HorizontalLayoutGroup>();
            metricLayout.spacing = 12f;
            metricLayout.childControlWidth = true;
            metricLayout.childControlHeight = false;
            metricLayout.childForceExpandWidth = true;
            AddLayout(metrics.gameObject, -1f, 92f);

            TMP_Text didNotHappenPercent;
            TMP_Text didNotHappenCount;
            CreateOutcomeMetric(metrics, "일어나지 않음", PrimaryColor, out didNotHappenPercent, out didNotHappenCount);

            TMP_Text partiallyHappenedPercent;
            TMP_Text partiallyHappenedCount;
            CreateOutcomeMetric(metrics, "일부만 일어남", AccentColor, out partiallyHappenedPercent, out partiallyHappenedCount);

            TMP_Text happenedPercent;
            TMP_Text happenedCount;
            CreateOutcomeMetric(metrics, "일어남", new Color(0.72f, 0.23f, 0.24f), out happenedPercent, out happenedCount);

            OutcomeSummaryView view = root.gameObject.AddComponent<OutcomeSummaryView>();
            view.SetControls(
                reviewed,
                didNotHappenPercent,
                didNotHappenCount,
                partiallyHappenedPercent,
                partiallyHappenedCount,
                happenedPercent,
                happenedCount,
                closeButton);
            view.SetRoot(root.gameObject);
            view.Hide();
            EditorUtility.SetDirty(view);
            return view;
        }

        private static void CreateOutcomeMetric(
            RectTransform parent,
            string label,
            Color valueColor,
            out TMP_Text percentText,
            out TMP_Text countText)
        {
            RectTransform root = CreateRect(label + " Metric", parent);
            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 1f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childAlignment = TextAnchor.UpperLeft;
            AddLayout(root.gameObject, -1f, 90f);

            TMP_Text labelText = CreateText(root, label, 11f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);
            AddLayout(labelText.gameObject, -1f, 18f);

            percentText = CreateText(root, "0%", 26f, FontStyles.Bold, valueColor, TextAlignmentOptions.Left);
            AddLayout(percentText.gameObject, -1f, 34f);

            countText = CreateText(root, "0개", 12f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);
            AddLayout(countText.gameObject, -1f, 16f);
        }

        private static WorryCardView CreateCardTemplate(RectTransform root)
        {
            Image background = AddImage(root.gameObject, new Color(0.97f, 0.91f, 1f), true);
            Outline outline = root.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.86f, 0.74f, 1f, 0.55f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            AddLayout(root.gameObject, 150f, 150f);

            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            root.gameObject.AddComponent<UIButtonFeedback>();

            RectTransform pin = CreateRect("Pin", root);
            Anchor(pin, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-10f, -8f), new Vector2(10f, 12f));
            AddImage(pin.gameObject, new Color(0.88f, 0.43f, 0.92f), false);

            RectTransform content = CreateRect("Card Content", root);
            Anchor(content, Vector2.zero, Vector2.one, new Vector2(14f, 12f), new Vector2(-14f, -14f));

            VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            RectTransform row = CreateRect("Card Header", content);
            HorizontalLayoutGroup rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 6f;
            rowLayout.childControlHeight = false;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            AddLayout(row.gameObject, -1f, 26f);

            TMP_Text emotion = CreateText(row, "🙂", 20f, FontStyles.Normal, TextColor, TextAlignmentOptions.Left);
            AddLayout(emotion.gameObject, 34f, 24f);

            TMP_Text date = CreateText(row, "6월 5일", 11f, FontStyles.Bold, MutedTextColor, TextAlignmentOptions.Right);
            AddLayout(date.gameObject, -1f, 24f);

            TMP_Text worry = CreateText(content, "발표에서 실수하면 어떡하지?", 15f, FontStyles.Bold, TextColor, TextAlignmentOptions.TopLeft);
            worry.overflowMode = TextOverflowModes.Ellipsis;
            worry.maxVisibleLines = 3;
            AddLayout(worry.gameObject, -1f, 58f);

            TMP_Text action = CreateText(content, "\"준비하면 괜찮을 거야\"", 11f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.TopLeft);
            action.overflowMode = TextOverflowModes.Ellipsis;
            action.maxVisibleLines = 2;
            AddLayout(action.gameObject, -1f, 32f);

            TMP_Text outcome = CreateText(content, string.Empty, 10f, FontStyles.Bold, MutedTextColor, TextAlignmentOptions.Right);
            AddLayout(outcome.gameObject, -1f, 16f);

            WorryCardView view = root.gameObject.AddComponent<WorryCardView>();
            view.SetControls(emotion, date, worry, action, outcome, button, background, outline);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static T CreateTextStep<T>(
            RectTransform parent,
            string objectName,
            string title,
            string helper,
            string submitLabel,
            string backLabel,
            string placeholder) where T : TextInputStepView
        {
            RectTransform root = CreateStepPanel(parent, objectName);
            TMP_Text titleText = CreateText(root, title, 24f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            TMP_Text helperText = CreateText(root, helper, 15f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);
            TMP_InputField input = CreateInput(root, placeholder, 136f);
            RectTransform buttonRow = CreateButtonRow(root);
            Button backButton = CreateButton(buttonRow, backLabel, SecondaryColor, TextColor, 92f, 42f);
            Button submitButton = CreateButton(buttonRow, submitLabel, PrimaryColor, Color.white, 112f, 42f);

            T view = root.gameObject.AddComponent<T>();
            view.SetRoot(root.gameObject);
            view.SetPromptTexts(titleText, helperText);
            view.SetControls(input, submitButton, backButton);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static CopingPlanView CreateCopingActionsStep(RectTransform parent)
        {
            RectTransform root = CreateStepPanel(parent, "Coping Plan View");
            TMP_Text titleText = CreateText(root, "최악의 경우 대비하기", 24f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            TMP_Text helperText = CreateText(root, "만약 정말 일어난다면, 그때 할 수 있는 일을 적어봐요.", 15f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);

            TMP_InputField first = CreateInput(root, "행동 1  예: 미리 연습하기", 52f);
            TMP_InputField second = CreateInput(root, "행동 2  예: 친구에게 피드백 받기", 52f);
            TMP_InputField third = CreateInput(root, "행동 3  선택 입력", 52f);
            Button addActionButton = CreateButton(root, "+  행동 추가", new Color(0.99f, 0.96f, 1f), new Color(0.72f, 0.08f, 0.92f), -1f, 44f);

            RectTransform buttonRow = CreateButtonRow(root);
            Button backButton = CreateButton(buttonRow, "이전", SecondaryColor, TextColor, 92f, 42f);
            Button submitButton = CreateButton(buttonRow, "다음", PrimaryColor, Color.white, 112f, 42f);

            CopingPlanView view = root.gameObject.AddComponent<CopingPlanView>();
            view.SetRoot(root.gameObject);
            view.SetPromptTexts(titleText, helperText);
            view.SetControls(new[] { first, second, third }, addActionButton, submitButton, backButton);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static ActionPlanView CreateSummaryStep(RectTransform parent)
        {
            RectTransform root = CreateStepPanel(parent, "Action Plan View");
            TMP_Text titleText = CreateText(root, "지금 할 수 있는 행동 정하기", 24f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            TMP_Text helperText = CreateText(root, "막연한 안심보다 작고 분명한 행동 하나가 생각을 안정시켜줘요.", 15f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);

            TMP_Text actionLabel = CreateText(root, "✨ 지금 할 수 있는 것", 14f, FontStyles.Bold, PrimaryColor, TextAlignmentOptions.Left);
            AddLayout(actionLabel.gameObject, -1f, 22f);

            TMP_InputField actionInput = CreateInput(root, "지금 당장 실천할 수 있는 것은?", 142f);

            RectTransform buttonRow = CreateButtonRow(root);
            Button backButton = CreateButton(buttonRow, "이전", SecondaryColor, TextColor, 92f, 42f);
            Button submitButton = CreateButton(buttonRow, "다음", PrimaryColor, Color.white, 112f, 42f);

            ActionPlanView view = root.gameObject.AddComponent<ActionPlanView>();
            view.SetRoot(root.gameObject);
            view.SetPromptTexts(titleText, helperText);
            view.SetControls(actionInput, submitButton, backButton);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static ProbabilityEstimateView CreateProbabilityStep(RectTransform parent)
        {
            RectTransform root = CreateStepPanel(parent, "Probability Estimate View");
            TMP_Text title = CreateText(root, "확률 가늠하기", 24f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            TMP_Text helper = CreateText(root, "지금은 이 일이 얼마나 일어날 것처럼 느껴지나요?", 15f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);
            TMP_Text percent = CreateText(root, "0%", 36f, FontStyles.Bold, PrimaryColor, TextAlignmentOptions.Center);

            Slider slider = CreateSlider(root);
            RectTransform buttonRow = CreateButtonRow(root);
            Button backButton = CreateButton(buttonRow, "이전", SecondaryColor, TextColor, 92f, 42f);
            Button submitButton = CreateButton(buttonRow, "다음", PrimaryColor, Color.white, 112f, 42f);

            ProbabilityEstimateView view = root.gameObject.AddComponent<ProbabilityEstimateView>();
            view.SetRoot(root.gameObject);
            view.SetPromptTexts(title, helper);
            view.SetControls(slider, percent, submitButton, backButton);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static EmotionCheckView CreateEmotionStep(RectTransform parent)
        {
            RectTransform root = CreateStepPanel(parent, "Emotion Check View");
            TMP_Text title = CreateText(root, "지금 마음은 어떤가요?", 24f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            TMP_Text helper = CreateText(root, "현재 마음 상태를 고르면 카드에 함께 기록돼요.", 15f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);

            Button distressed = CreateButton(root, "아직 심란함", SecondaryColor, TextColor, -1f, 44f);
            Button relieved = CreateButton(root, "조금 편해짐", SecondaryColor, TextColor, -1f, 44f);
            Button calm = CreateButton(root, "이제 괜찮음", SecondaryColor, TextColor, -1f, 44f);

            RectTransform buttonRow = CreateButtonRow(root);
            Button backButton = CreateButton(buttonRow, "이전", SecondaryColor, TextColor, 92f, 42f);

            EmotionCheckView view = root.gameObject.AddComponent<EmotionCheckView>();
            view.SetRoot(root.gameObject);
            view.SetPromptTexts(title, helper);
            view.SetControls(distressed, relieved, calm, backButton);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static WorryDetailView CreateDetail(RectTransform parent)
        {
            RectTransform root = CreateRect("Worry Detail View", parent);
            Anchor(root, Vector2.zero, Vector2.one, new Vector2(20f, 42f), new Vector2(-20f, -42f));
            AddImage(root.gameObject, BoardColor, true);

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 18, 18);
            layout.spacing = 8f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;

            TMP_Text worry = CreateText(root, "고민", 23f, FontStyles.Bold, TextColor, TextAlignmentOptions.Left);
            TMP_Text probability = CreateText(root, "0%", 20f, FontStyles.Bold, PrimaryColor, TextAlignmentOptions.Left);
            TMP_Text coping = CreateText(root, "대처 계획", 15f, FontStyles.Normal, TextColor, TextAlignmentOptions.Left);
            TMP_Text action = CreateText(root, "지금 할 행동", 15f, FontStyles.Normal, TextColor, TextAlignmentOptions.Left);
            TMP_Text emotion = CreateText(root, "감정", 13f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);
            TMP_Text outcome = CreateText(root, "결과", 13f, FontStyles.Normal, MutedTextColor, TextAlignmentOptions.Left);

            RectTransform outcomeRow = CreateButtonRow(root);
            Button didNotHappen = CreateButton(outcomeRow, "일어나지 않음", SecondaryColor, TextColor, 118f, 40f);
            Button partial = CreateButton(outcomeRow, "일부만", SecondaryColor, TextColor, 64f, 40f);
            Button happened = CreateButton(outcomeRow, "일어남", AccentColor, TextColor, 68f, 40f);

            RectTransform closeRow = CreateButtonRow(root);
            Button close = CreateButton(closeRow, "닫기", PrimaryColor, Color.white, 96f, 40f);

            WorryDetailView view = root.gameObject.AddComponent<WorryDetailView>();
            view.SetRoot(root.gameObject);
            view.SetControls(worry, probability, coping, action, emotion, outcome, close, didNotHappen, partial, happened);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static FocusOverlayView CreateFocusOverlay(RectTransform parent)
        {
            RectTransform root = CreateRect("Focus Overlay View", parent);
            Stretch(root);
            Image image = AddImage(root.gameObject, new Color(0f, 0f, 0f, 0.35f), false);
            image.raycastTarget = false;

            FocusOverlayView view = root.gameObject.AddComponent<FocusOverlayView>();
            view.SetRoot(root.gameObject);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static StepProgressView CreateProgress(RectTransform parent)
        {
            RectTransform root = CreateRect("Step Progress View", parent);
            Anchor(root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-118f, -62f), new Vector2(118f, -22f));

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 4f;

            TMP_Text label = CreateText(root, "단계", 13f, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            Slider slider = CreateSlider(root);
            slider.interactable = false;

            StepProgressView view = root.gameObject.AddComponent<StepProgressView>();
            view.SetRoot(root.gameObject);
            view.SetControls(label, slider);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static CompanionBubbleView CreateCompanionBubble(RectTransform parent)
        {
            RectTransform root = CreateRect("Companion Bubble View", parent);
            Anchor(root, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(20f, -150f), new Vector2(-20f, -76f));
            AddImage(root.gameObject, new Color(1f, 0.98f, 0.9f), true);

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(14, 14, 12, 12);

            TMP_Text message = CreateText(root, "메시지", 14f, FontStyles.Normal, TextColor, TextAlignmentOptions.Left);

            CompanionBubbleView view = root.gameObject.AddComponent<CompanionBubbleView>();
            view.SetRoot(root.gameObject);
            view.SetMessageText(message);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static FeedbackPopupView CreateFeedbackPopup(RectTransform parent)
        {
            RectTransform root = CreateRect("Feedback Popup View", parent);
            Anchor(root, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(20f, 24f), new Vector2(-20f, 92f));
            AddImage(root.gameObject, new Color(0.12f, 0.13f, 0.15f), true);

            HorizontalLayoutGroup layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(18, 12, 12, 12);
            layout.spacing = 12f;
            layout.childControlWidth = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            TMP_Text message = CreateText(root, "안내", 14f, FontStyles.Normal, Color.white, TextAlignmentOptions.Left);
            Button close = CreateButton(root, "확인", PrimaryColor, Color.white, 64f, 36f);

            FeedbackPopupView view = root.gameObject.AddComponent<FeedbackPopupView>();
            view.SetRoot(root.gameObject);
            view.SetControls(message, close);
            EditorUtility.SetDirty(view);
            return view;
        }

        private static DailyClosureView CreateDailyClosure(RectTransform parent)
        {
            RectTransform root = CreateRect("Daily Closure View", parent);
            Stretch(root);
            AddImage(root.gameObject, new Color(0.03f, 0.03f, 0.05f), true);

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 150, 120);
            layout.spacing = 28f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childAlignment = TextAnchor.MiddleCenter;

            TMP_Text message = CreateText(root, "잘 생각했어요.\n아직 많은 고민과 불안이 남아 있겠지만,\n오늘은 여기까지 하고 지금 이 순간에 집중해 봐요.", 19f, FontStyles.Normal, Color.white, TextAlignmentOptions.Center);
            AddLayout(message.gameObject, -1f, 180f);

            DailyClosureView view = root.gameObject.AddComponent<DailyClosureView>();
            view.SetRoot(root.gameObject);
            view.SetControls(message);
            view.Hide();
            EditorUtility.SetDirty(view);
            return view;
        }

        private static RectTransform CreateStepPanel(RectTransform parent, string objectName)
        {
            RectTransform root = CreateRect(objectName, parent);
            Anchor(root, Vector2.zero, Vector2.one, new Vector2(20f, 28f), new Vector2(-20f, -170f));
            AddImage(root.gameObject, BoardColor, true);

            VerticalLayoutGroup layout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 11f;
            layout.childControlHeight = false;
            layout.childControlWidth = true;

            return root;
        }

        private static RectTransform CreateButtonRow(RectTransform parent)
        {
            RectTransform row = CreateRect("Button Row", parent);
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = false;
            AddLayout(row.gameObject, -1f, 48f);
            return row;
        }

        private static Button CreateButton(RectTransform parent, string label, Color background, Color textColor, float preferredWidth, float preferredHeight)
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

        private static TMP_InputField CreateInput(RectTransform parent, string placeholder, float height)
        {
            RectTransform root = CreateRect("Input Field", parent);
            Image image = AddImage(root.gameObject, new Color(0.94f, 0.95f, 0.93f), true);
            AddLayout(root.gameObject, -1f, height);

            TMP_InputField input = root.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = image;
            input.lineType = TMP_InputField.LineType.MultiLineNewline;
            input.keyboardType = TouchScreenKeyboardType.Default;
            input.shouldActivateOnSelect = true;
            input.shouldHideMobileInput = true;
            input.resetOnDeActivation = false;
            input.scrollSensitivity = 18f;

            RectTransform viewport = CreateRect("Viewport", root);
            Anchor(viewport, Vector2.zero, Vector2.one, new Vector2(12f, 10f), new Vector2(-12f, -10f));

            TMP_Text placeholderText = CreateText(viewport, placeholder, 17f, FontStyles.Italic, MutedTextColor, TextAlignmentOptions.TopLeft);
            Stretch(placeholderText.rectTransform);

            TMP_Text inputText = CreateText(viewport, string.Empty, 17f, FontStyles.Normal, TextColor, TextAlignmentOptions.TopLeft);
            Stretch(inputText.rectTransform);

            input.textViewport = viewport;
            input.placeholder = placeholderText;
            input.textComponent = inputText;
            return input;
        }

        private static Slider CreateSlider(RectTransform parent)
        {
            RectTransform root = CreateRect("Slider", parent);
            AddLayout(root.gameObject, -1f, 42f);

            Slider slider = root.gameObject.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = true;

            RectTransform background = CreateRect("Background", root);
            Anchor(background, Vector2.zero, Vector2.one, new Vector2(8f, 16f), new Vector2(-8f, -16f));
            AddImage(background.gameObject, SecondaryColor, true);

            RectTransform fillArea = CreateRect("Fill Area", root);
            Anchor(fillArea, Vector2.zero, Vector2.one, new Vector2(8f, 16f), new Vector2(-8f, -16f));

            RectTransform fill = CreateRect("Fill", fillArea);
            Stretch(fill);
            AddImage(fill.gameObject, PrimaryColor, true);

            RectTransform handleArea = CreateRect("Handle Slide Area", root);
            Stretch(handleArea);

            RectTransform handle = CreateRect("Handle", handleArea);
            Anchor(handle, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(-12f, -12f), new Vector2(12f, 12f));
            Image handleImage = AddImage(handle.gameObject, AccentColor, true);

            slider.fillRect = fill;
            slider.handleRect = handle;
            slider.targetGraphic = handleImage;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static TMP_Text CreateText(RectTransform parent, string value, float size, FontStyles style, Color color, TextAlignmentOptions alignment)
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

        private static TMP_FontAsset GetKoreanFont()
        {
            if (_koreanFont == null)
                _koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(KoreanFontAssetPath);

            return _koreanFont;
        }

        private static Image AddImage(GameObject target, Color color, bool raycastTarget)
        {
            Image image = target.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;
            return image;
        }

        private static LayoutElement AddLayout(GameObject target, float preferredWidth, float preferredHeight)
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

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject target = new GameObject(objectName);
            RectTransform rect = target.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            Anchor(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        private static void Anchor(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
