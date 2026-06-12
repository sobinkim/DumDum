using SB.App.ML;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SB.App.Editor
{
    public static class DumDumSupportTrainingSceneBuilder
    {
        private const string TrainingScenePath = "Assets/SB/ML/TrainingScenes/DumDumSupportTraining.unity";
        private const int AgentCount = 12;

        [MenuItem("DumDum/ML/Rebuild Support Training Scene")]
        public static void RebuildTrainingScene()
        {
            EnsureFolders();

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "DumDumSupportTraining";

            CreateCamera();
            CreateEnvironmentRoot();
            CreateTrainingLauncherCanvas();

            EditorSceneManager.SaveScene(scene, TrainingScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new GameObject("Training Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.96f, 0.97f, 0.95f);
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static void CreateEnvironmentRoot()
        {
            GameObject root = new GameObject("DumDum Support Training Environment");
            DumDumSupportTrainingEnvironment environment = root.AddComponent<DumDumSupportTrainingEnvironment>();

            for (int i = 0; i < AgentCount; i++)
                CreateAgent(root.transform, environment, i);
        }

        private static void CreateAgent(Transform parent, DumDumSupportTrainingEnvironment environment, int index)
        {
            GameObject agentObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            agentObject.name = $"DumDum Support Agent {index + 1:00}";
            agentObject.transform.SetParent(parent, false);
            agentObject.transform.position = CreateGridPosition(index);
            agentObject.transform.localScale = new Vector3(0.72f, 0.72f, 0.72f);

            Renderer renderer = agentObject.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = CreateAgentMaterial(index);

            DumDumSupportAgent agent = agentObject.AddComponent<DumDumSupportAgent>();
            agent.SetTrainingEnvironment(environment);

            BehaviorParameters behavior = agentObject.GetComponent<BehaviorParameters>();
            if (behavior == null)
                behavior = agentObject.AddComponent<BehaviorParameters>();

            behavior.BehaviorName = DumDumSupportAgent.BehaviorName;
            behavior.BehaviorType = BehaviorType.Default;
            behavior.BrainParameters.VectorObservationSize = DumDumSupportAgent.ObservationSize;
            behavior.BrainParameters.NumStackedVectorObservations = 1;
            behavior.BrainParameters.ActionSpec = ActionSpec.MakeDiscrete(DumDumSupportAgent.ActionCount);

            DecisionRequester requester = agentObject.AddComponent<DecisionRequester>();
            requester.DecisionPeriod = 1;
            requester.DecisionStep = 0;
            requester.TakeActionsBetweenDecisions = false;
        }

        private static Vector3 CreateGridPosition(int index)
        {
            int columns = 4;
            int row = index / columns;
            int column = index % columns;
            return new Vector3((column - 1.5f) * 2f, (1f - row) * 2f, 0f);
        }

        private static Material CreateAgentMaterial(int index)
        {
            Material material = new Material(Shader.Find("Sprites/Default"));
            Color[] palette =
            {
                new Color(0.11f, 0.44f, 0.53f),
                new Color(0.93f, 0.72f, 0.25f),
                new Color(0.39f, 0.86f, 0.56f),
                new Color(0.72f, 0.08f, 0.92f)
            };

            material.color = palette[index % palette.Length];
            return material;
        }

        private static void CreateTrainingLauncherCanvas()
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

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();

            RectTransform panel = CreateRect("Training Launcher Panel", canvasObject.transform);
            Anchor(panel, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(24f, -158f), new Vector2(-24f, -24f));
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.9f);

            HorizontalLayoutGroup panelLayout = panel.gameObject.AddComponent<HorizontalLayoutGroup>();
            panelLayout.padding = new RectOffset(20, 20, 16, 16);
            panelLayout.spacing = 18f;
            panelLayout.childControlHeight = true;
            panelLayout.childControlWidth = false;
            panelLayout.childAlignment = TextAnchor.MiddleLeft;

            Button startButton = CreateButton(panel, "Start 100k Training", new Color(0.11f, 0.44f, 0.53f), Color.white, 240f, 72f);

            RectTransform textColumn = CreateRect("Status Column", panel);
            VerticalLayoutGroup textLayout = textColumn.gameObject.AddComponent<VerticalLayoutGroup>();
            textLayout.spacing = 4f;
            textLayout.childControlHeight = false;
            textLayout.childControlWidth = true;
            AddLayout(textColumn.gameObject, 880f, 92f);

            Text statusText = CreateText(textColumn, "Ready", 28, FontStyle.Bold, Color.white, TextAnchor.MiddleLeft);
            AddLayout(statusText.gameObject, -1f, 36f);

            Text detailText = CreateText(
                textColumn,
                "Press the button. Unity will restart Play after the trainer opens.",
                18,
                FontStyle.Normal,
                new Color(0.8f, 0.86f, 0.84f),
                TextAnchor.UpperLeft);
            AddLayout(detailText.gameObject, -1f, 54f);

            DumDumTrainingLauncherView launcher = panel.gameObject.AddComponent<DumDumTrainingLauncherView>();
            SerializedObject serializedLauncher = new SerializedObject(launcher);
            serializedLauncher.FindProperty("startTrainingButton").objectReferenceValue = startButton;
            serializedLauncher.FindProperty("statusText").objectReferenceValue = statusText;
            serializedLauncher.FindProperty("detailText").objectReferenceValue = detailText;
            serializedLauncher.FindProperty("timeScale").intValue = 20;
            serializedLauncher.ApplyModifiedPropertiesWithoutUndo();
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

        private static void EnsureFolders()
        {
            CreateFolderIfMissing("Assets/SB", "ML");
            CreateFolderIfMissing("Assets/SB/ML", "TrainingScenes");
        }

        private static void CreateFolderIfMissing(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
