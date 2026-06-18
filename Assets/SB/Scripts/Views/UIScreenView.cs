using UnityEngine;

namespace SB.App.Views
{
    public abstract class UIScreenView : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private float showDuration = 0.32f;
        [SerializeField] private float showScale = 0.985f;

        protected GameObject Root => root != null ? root : gameObject;

        public bool IsVisible => Root.activeSelf;

        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private float _animationTime;
        private bool _isAnimatingShow;

        public void SetRoot(GameObject rootObject)
        {
            root = rootObject;
        }

        public virtual void Show()
        {
            Root.SetActive(true);
            Root.transform.SetAsLastSibling();
            BeginShowAnimation();
        }

        public virtual void Hide()
        {
            _isAnimatingShow = false;
            Root.SetActive(false);
        }

        protected virtual void Update()
        {
            if (!_isAnimatingShow)
                return;

            _animationTime += Time.unscaledDeltaTime;
            float progress = showDuration <= 0f ? 1f : Mathf.Clamp01(_animationTime / showDuration);
            float eased = progress * progress * (3f - 2f * progress);

            if (_canvasGroup != null)
                _canvasGroup.alpha = eased;

            if (_rectTransform != null)
            {
                float scale = Mathf.Lerp(showScale, 1f, eased);
                _rectTransform.localScale = new Vector3(scale, scale, 1f);
            }

            if (progress >= 1f)
                _isAnimatingShow = false;
        }

        private void BeginShowAnimation()
        {
            _canvasGroup = Root.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = Root.AddComponent<CanvasGroup>();

            _rectTransform = Root.transform as RectTransform;
            _animationTime = 0f;
            _isAnimatingShow = true;
            _canvasGroup.alpha = 0f;

            if (_rectTransform != null)
                _rectTransform.localScale = new Vector3(showScale, showScale, 1f);
        }
    }
}
