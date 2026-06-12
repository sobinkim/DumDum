using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SB.App.Editor
{
    public sealed class DumDumSupportTrainingLauncher : EditorWindow
    {
        private const string TrainingScenePath = "Assets/SB/ML/TrainingScenes/DumDumSupportTraining.unity";
        private const string TrainingScriptPath = "Tools/MLAgents/train-support-intervention.ps1";
        private const string DefaultRunId = "dumdum-support-intervention-100k";
        private const int DefaultTimeScale = 20;
        private const double PlayDelaySeconds = 8.0;

        private static bool _startWhenEditModeReturns;
        private static double _enterPlayAt;
        private static Process _trainerProcess;

        private string _runId = DefaultRunId;
        private int _timeScale = DefaultTimeScale;

        [MenuItem("DumDum/ML/Support Training Launcher")]
        public static void ShowWindow()
        {
            DumDumSupportTrainingLauncher window = GetWindow<DumDumSupportTrainingLauncher>("Support Training");
            window.minSize = new Vector2(360f, 170f);
        }

        [MenuItem("DumDum/ML/Start Support Training 100k")]
        public static void StartDefaultTraining()
        {
            StartTraining(DefaultRunId, DefaultTimeScale);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("DumDum Support Training", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Starts the Python trainer first, then opens the training scene and enters Play Mode automatically.",
                MessageType.Info);

            _runId = EditorGUILayout.TextField("Run Id", string.IsNullOrWhiteSpace(_runId) ? DefaultRunId : _runId);
            _timeScale = EditorGUILayout.IntSlider("Time Scale", Mathf.Max(1, _timeScale), 1, 100);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Start 100k Training", GUILayout.Height(36f)))
                    StartTraining(_runId, _timeScale);

                if (GUILayout.Button("Open Scene", GUILayout.Height(36f)))
                    OpenTrainingScene();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Expected trainer output:");
            EditorGUILayout.SelectableLabel("Connected to Unity environment", GUILayout.Height(18f));
            EditorGUILayout.SelectableLabel("DumDumSupportIntervention. Step: ...", GUILayout.Height(18f));
        }

        private static void StartTraining(string runId, int timeScale)
        {
            if (EditorApplication.isCompiling)
            {
                Debug.LogWarning("Wait for script compilation to finish before starting ML-Agents training.");
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                _startWhenEditModeReturns = true;
                EditorApplication.update -= StartWhenEditModeReturns;
                EditorApplication.update += StartWhenEditModeReturns;
                EditorApplication.isPlaying = false;
                Debug.Log("Stopping Play Mode first. Training will start after Unity returns to Edit Mode.");
                return;
            }

            if (!OpenTrainingScene())
                return;

            if (!StartTrainerProcess(runId, timeScale))
                return;

            _enterPlayAt = EditorApplication.timeSinceStartup + PlayDelaySeconds;
            EditorApplication.update -= EnterPlayModeWhenTrainerIsReady;
            EditorApplication.update += EnterPlayModeWhenTrainerIsReady;

            Debug.Log("ML-Agents trainer started. Unity will enter Play Mode shortly.");
        }

        private static void StartWhenEditModeReturns()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            EditorApplication.update -= StartWhenEditModeReturns;

            if (!_startWhenEditModeReturns)
                return;

            _startWhenEditModeReturns = false;
            StartTraining(DefaultRunId, DefaultTimeScale);
        }

        private static bool OpenTrainingScene()
        {
            if (!File.Exists(TrainingScenePath))
            {
                Debug.LogError($"Training scene is missing: {TrainingScenePath}");
                return false;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return false;

            if (EditorSceneManager.GetActiveScene().path != TrainingScenePath)
                EditorSceneManager.OpenScene(TrainingScenePath);

            return true;
        }

        private static bool StartTrainerProcess(string runId, int timeScale)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            string scriptPath = Path.Combine(projectRoot, TrainingScriptPath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(scriptPath))
            {
                Debug.LogError($"Training script is missing: {scriptPath}");
                return false;
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -ExecutionPolicy Bypass -File \"{scriptPath}\" -RunId {runId} -Force -TimeScale {timeScale}",
                    WorkingDirectory = projectRoot,
                    UseShellExecute = true
                };

                _trainerProcess = Process.Start(startInfo);
                return _trainerProcess != null;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to start ML-Agents trainer: {exception.Message}");
                return false;
            }
        }

        private static void EnterPlayModeWhenTrainerIsReady()
        {
            if (EditorApplication.timeSinceStartup < _enterPlayAt)
                return;

            EditorApplication.update -= EnterPlayModeWhenTrainerIsReady;

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
                return;

            EditorApplication.EnterPlaymode();
        }
    }
}
