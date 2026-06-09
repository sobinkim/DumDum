using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SB.App.Application
{
    public sealed class AppSceneNavigator : MonoBehaviour
    {
        public const string LaunchSceneName = "Start";
        public const string MainSceneName = "Main";

        private static AppSceneNavigator _instance;

        [SerializeField] private float fadeOutDuration = 0.28f;
        [SerializeField] private float fadeInDuration = 0.36f;
        [SerializeField] private Color transitionColor = Color.black;

        private CanvasGroup _transitionGroup;
        private Image _transitionImage;
        private Coroutine _loadRoutine;

        public static AppSceneNavigator Instance => GetOrCreate();

        public static AppSceneNavigator GetOrCreate()
        {
            if (_instance != null)
                return _instance;

            _instance = FindFirstObjectByType<AppSceneNavigator>();
            if (_instance != null)
                return _instance;

            GameObject root = new GameObject(nameof(AppSceneNavigator));
            _instance = root.AddComponent<AppSceneNavigator>();
            return _instance;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadLaunchScene()
        {
            LoadScene(LaunchSceneName);
        }

        public void LoadMainScene()
        {
            LoadScene(MainSceneName);
        }

        public void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                return;

            if (!UnityEngine.Application.isPlaying)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            if (_loadRoutine != null)
                return;

            _loadRoutine = StartCoroutine(LoadSceneWithTransition(sceneName));
        }

        private IEnumerator LoadSceneWithTransition(string sceneName)
        {
            EnsureTransitionOverlay();

            _transitionImage.color = transitionColor;
            _transitionGroup.blocksRaycasts = true;
            _transitionGroup.interactable = true;

            yield return FadeTransition(1f, fadeOutDuration);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
            while (operation != null && !operation.isDone)
                yield return null;

            yield return null;
            yield return FadeTransition(0f, fadeInDuration);

            _transitionGroup.blocksRaycasts = false;
            _transitionGroup.interactable = false;
            _loadRoutine = null;
        }

        private IEnumerator FadeTransition(float targetAlpha, float duration)
        {
            float startAlpha = _transitionGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
                float eased = progress * progress * (3f - 2f * progress);
                _transitionGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, eased);
                yield return null;
            }

            _transitionGroup.alpha = targetAlpha;
        }

        private void EnsureTransitionOverlay()
        {
            if (_transitionGroup != null && _transitionImage != null)
                return;

            GameObject canvasObject = new GameObject("Scene Transition Canvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = short.MaxValue;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390f, 844f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();
            _transitionGroup = canvasObject.AddComponent<CanvasGroup>();
            _transitionGroup.alpha = 0f;
            _transitionGroup.blocksRaycasts = false;
            _transitionGroup.interactable = false;

            GameObject imageObject = new GameObject("Fade");
            imageObject.transform.SetParent(canvasObject.transform, false);

            RectTransform rect = imageObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            _transitionImage = imageObject.AddComponent<Image>();
            _transitionImage.color = transitionColor;
            _transitionImage.raycastTarget = true;
        }
    }
}
