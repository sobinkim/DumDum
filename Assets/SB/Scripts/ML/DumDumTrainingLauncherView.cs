using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SB.App.ML
{
    public sealed class DumDumTrainingLauncherView : MonoBehaviour
    {
        private const string TrainingScriptPath = "Tools/MLAgents/train-support-intervention.ps1";
        private const string DefaultRunId = "dumdum-support-intervention-100k";
#if UNITY_EDITOR
        private const string PendingEditorTrainingKey = "SB.App.ML.DumDumTrainingLauncherView.PendingTraining";
        private const string PendingEditorTimeScaleKey = "SB.App.ML.DumDumTrainingLauncherView.PendingTimeScale";
        private const double EditorPlayDelaySeconds = 8.0;

        private static double _enterPlayAt;
        private static Process _editorTrainerProcess;
#endif

        [SerializeField] private Button startTrainingButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text detailText;
        [SerializeField] private int timeScale = 20;

        private Process _trainerProcess;

        private void Awake()
        {
            EnsureControls();

            if (startTrainingButton != null)
                startTrainingButton.onClick.AddListener(StartTraining);

            SetStatus("Ready", "Press the button. Unity will restart Play after the trainer opens.");
        }

        private void OnDestroy()
        {
            if (startTrainingButton != null)
                startTrainingButton.onClick.RemoveListener(StartTraining);
        }

        private void Update()
        {
            if (_trainerProcess == null)
                return;

            if (!_trainerProcess.HasExited)
                return;

            int exitCode = _trainerProcess.ExitCode;
            _trainerProcess.Dispose();
            _trainerProcess = null;

            if (startTrainingButton != null)
                startTrainingButton.interactable = true;

            SetStatus(
                exitCode == 0 ? "Training finished" : $"Training stopped ({exitCode})",
                "Check MLRuns/dumdum-support-intervention-100k for results.");
        }

        public void StartTraining()
        {
#if UNITY_EDITOR
            if (UnityEngine.Application.isEditor)
            {
                StartEditorTraining();
                return;
            }
#endif

            if (_trainerProcess != null && !_trainerProcess.HasExited)
            {
                SetStatus("Training already running", "The ML-Agents trainer process is already active.");
                return;
            }

            string projectRoot = GetProjectRoot();
            string scriptPath = Path.Combine(projectRoot, TrainingScriptPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(scriptPath))
            {
                SetStatus("Script missing", scriptPath);
                return;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -ExecutionPolicy Bypass -File \"{scriptPath}\" -RunId {DefaultRunId} -Force -TimeScale {timeScale}",
                    WorkingDirectory = projectRoot,
                    UseShellExecute = true
                };

                _trainerProcess = Process.Start(startInfo);

                if (startTrainingButton != null)
                    startTrainingButton.interactable = false;

                SetStatus("Training started", "A PowerShell trainer window should be running. Keep Unity Play mode active.");
            }
            catch (Exception exception)
            {
                SetStatus("Failed to start training", exception.Message);
            }
        }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void RegisterEditorTrainingResume()
        {
            EditorApplication.update -= ResumeEditorTrainingIfPending;
            EditorApplication.update += ResumeEditorTrainingIfPending;
        }

        private void StartEditorTraining()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorPrefs.SetBool(PendingEditorTrainingKey, true);
                EditorPrefs.SetInt(PendingEditorTimeScaleKey, timeScale);
                SetStatus("Restarting for training", "Unity will stop Play, open the trainer, then enter Play again.");
                EditorApplication.isPlaying = false;
                return;
            }

            if (StartEditorTrainerProcess(timeScale))
                QueueEditorPlayMode();
        }

        private static void ResumeEditorTrainingIfPending()
        {
            if (!EditorPrefs.GetBool(PendingEditorTrainingKey, false))
                return;

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                return;

            int pendingTimeScale = EditorPrefs.GetInt(PendingEditorTimeScaleKey, 20);
            EditorPrefs.DeleteKey(PendingEditorTrainingKey);
            EditorPrefs.DeleteKey(PendingEditorTimeScaleKey);

            if (StartEditorTrainerProcess(pendingTimeScale))
                QueueEditorPlayMode();
        }

        private static bool StartEditorTrainerProcess(int targetTimeScale)
        {
            if (_editorTrainerProcess != null && !_editorTrainerProcess.HasExited)
            {
                UnityEngine.Debug.Log("ML-Agents trainer is already running. Entering Play Mode.");
                return true;
            }

            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            string scriptPath = Path.Combine(projectRoot, TrainingScriptPath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(scriptPath))
            {
                UnityEngine.Debug.LogError($"Training script is missing: {scriptPath}");
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -ExecutionPolicy Bypass -File \"{scriptPath}\" -RunId {DefaultRunId} -Force -TimeScale {targetTimeScale}",
                    WorkingDirectory = projectRoot,
                    UseShellExecute = true
                };

                _editorTrainerProcess = Process.Start(startInfo);
                UnityEngine.Debug.Log("ML-Agents trainer started. Unity will enter Play Mode shortly.");
                return _editorTrainerProcess != null;
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogError($"Failed to start ML-Agents trainer: {exception.Message}");
                return false;
            }
        }

        private static void QueueEditorPlayMode()
        {
            _enterPlayAt = EditorApplication.timeSinceStartup + EditorPlayDelaySeconds;
            EditorApplication.update -= EnterPlayModeAfterTrainerStart;
            EditorApplication.update += EnterPlayModeAfterTrainerStart;
        }

        private static void EnterPlayModeAfterTrainerStart()
        {
            if (EditorApplication.timeSinceStartup < _enterPlayAt)
                return;

            EditorApplication.update -= EnterPlayModeAfterTrainerStart;

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                return;

            EditorApplication.EnterPlaymode();
        }
#endif

        private static string GetProjectRoot()
        {
#if UNITY_EDITOR
            return Directory.GetParent(UnityEngine.Application.dataPath).FullName;
#else
            return Directory.GetParent(UnityEngine.Application.dataPath).Parent.FullName;
#endif
        }

        private void SetStatus(string status, string detail)
        {
            if (statusText != null)
                statusText.text = status;

            if (detailText != null)
                detailText.text = detail;
        }

        private void EnsureControls()
        {
            if (startTrainingButton != null && statusText != null && detailText != null)
                return;

            GameObject existingCanvas = GameObject.Find("Training Launcher Canvas");
            Transform canvasTransform = existingCanvas != null
                ? existingCanvas.transform
                : CreateCanvas().transform;

            RectTransform panel = CreateRect("Training Launcher Panel", canvasTransform);
            Anchor(panel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -158f), new Vector2(-24f, -24f));

            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.9f);

            HorizontalLayoutGroup panelLayout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
            panelLayout.padding = new RectOffset(20, 20, 16, 16);
            panelLayout.spacing = 18f;
            panelLayout.childControlHeight = true;
            panelLayout.childControlWidth = false;
            panelLayout.childAlignment = TextAnchor.MiddleLeft;

            startTrainingButton = CreateButton(panel, "Start 100k Training", new Color(0.11f, 0.44f, 0.53f), Color.white, 240f, 72f);

            RectTransform textColumn = CreateRect("Status Column", panel);
            VerticalLayoutGroup textLayout = textColumn.gameObject.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 4f;
            textLayout.childControlHeight = false;
            textLayout.childControlWidth = true;
            AddLayout(textColumn.gameObject, 880f, 92f);

            statusText = CreateText(textColumn, "Ready", 28, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            AddLayout(statusText.gameObject, -1f, 36f);

            detailText = CreateText(
                textColumn,
                "Press the button. Unity will restart Play after the trainer opens.",
                18,
                FontStyle.Normal,
                new Color(0.8f, 0.86f, 0.84f),
                TextAnchor.UpperLeft);
            AddLayout(detailText.gameObject, -1f, 54f);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("Training Launcher Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280f, 720f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            if (FindFirstObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<StandaloneInputModule>();
            }

            return canvas;
        }

        private static Button CreateButton(RectTransform parent, string label, Color background, Color textColor, float width, float height)
        {
            RectTransform root = CreateRect(label + " Button", parent);
            Image image = root.gameObject.AddComponent<Image>();
            image.color = background;

            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            AddLayout(root.gameObject, width, height);

            Text text = CreateText(root, label, 22, FontStyle.Bold, textColor, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);
            return button;
        }

        private static Text CreateText(RectTransform parent, string value, int size, FontStyle style, Color color, TextAnchor alignment)
        {
            RectTransform root = CreateRect("Text", parent);
            Text text = root.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            return text;
        }

        private static RectTransform CreateRect(string objectName, Transform parent)
        {
            GameObject target = new GameObject(objectName);
            RectTransform rect = target.AddComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
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
