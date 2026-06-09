using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SB.App.Views
{
    [DefaultExecutionOrder(1000)]
    public sealed class MobileKeyboardAvoider : MonoBehaviour
    {
        [SerializeField] private RectTransform targetRoot;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float keyboardPadding = 18f;
        [SerializeField] private float maxLift = 260f;
        [SerializeField] private float moveDuration = 0.28f;
        [SerializeField] private float returnDelay = 0.12f;

        private readonly Vector3[] _inputCorners = new Vector3[4];
        private Vector2 _baseAnchoredPosition;
        private float _currentLift;
        private float _liftVelocity;
        private float _returnAfterTime;
        private TMP_InputField _activeInput;

        private void Awake()
        {
            ResolveReferences();
            CaptureBasePosition();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CaptureBasePosition();
            _currentLift = 0f;
            _liftVelocity = 0f;
            _returnAfterTime = 0f;
        }

        private void LateUpdate()
        {
            ResolveReferences();

            if (targetRoot == null)
                return;

            targetRoot.anchoredPosition = _baseAnchoredPosition;

            TMP_InputField selectedInput = GetSelectedInput();
            float desiredLift = 0f;

            if (selectedInput != null && IsKeyboardVisible())
            {
                _activeInput = selectedInput;
                desiredLift = CalculateRequiredLift(selectedInput);
                _returnAfterTime = Time.unscaledTime + returnDelay;
            }
            else if (Time.unscaledTime < _returnAfterTime && _activeInput != null)
            {
                desiredLift = _currentLift;
            }
            else
            {
                _activeInput = null;
            }

            float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            _currentLift = Mathf.SmoothDamp(_currentLift, desiredLift, ref _liftVelocity, moveDuration, Mathf.Infinity, deltaTime);

            if (_currentLift < 0.05f && desiredLift <= 0f)
                _currentLift = 0f;

            targetRoot.anchoredPosition = _baseAnchoredPosition + Vector2.up * _currentLift;
        }

        public void SetTarget(RectTransform root, Canvas ownerCanvas)
        {
            targetRoot = root;
            canvas = ownerCanvas;
            CaptureBasePosition();
        }

        private void ResolveReferences()
        {
            if (targetRoot == null)
                targetRoot = transform as RectTransform;

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
        }

        private void CaptureBasePosition()
        {
            if (targetRoot != null)
                _baseAnchoredPosition = targetRoot.anchoredPosition;
        }

        private TMP_InputField GetSelectedInput()
        {
            GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            TMP_InputField input = selected != null ? selected.GetComponentInParent<TMP_InputField>() : null;

            if (input != null && input.isFocused)
                return input;

            if (_activeInput != null && _activeInput.isFocused)
                return _activeInput;

            return null;
        }

        private bool IsKeyboardVisible()
        {
            return TouchScreenKeyboard.visible || TouchScreenKeyboard.area.height > 0f;
        }

        private float CalculateRequiredLift(TMP_InputField input)
        {
            RectTransform inputRect = input.transform as RectTransform;
            RectTransform parentRect = targetRoot.parent as RectTransform;
            if (inputRect == null || parentRect == null)
                return 0f;

            Rect keyboardArea = TouchScreenKeyboard.area;
            float keyboardTop = keyboardArea.height > 0f
                ? keyboardArea.y + keyboardArea.height
                : Screen.height * 0.42f;

            inputRect.GetWorldCorners(_inputCorners);
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            float inputBottom = float.MaxValue;

            for (int i = 0; i < _inputCorners.Length; i++)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, _inputCorners[i]);
                inputBottom = Mathf.Min(inputBottom, screenPoint.y);
            }

            float desiredBottom = keyboardTop + keyboardPadding;
            if (inputBottom >= desiredBottom)
                return 0f;

            Vector2 inputBottomLocal;
            Vector2 desiredBottomLocal;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                new Vector2(Screen.width * 0.5f, inputBottom),
                uiCamera,
                out inputBottomLocal);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                new Vector2(Screen.width * 0.5f, desiredBottom),
                uiCamera,
                out desiredBottomLocal);

            float requiredLift = desiredBottomLocal.y - inputBottomLocal.y;
            return Mathf.Clamp(requiredLift, 0f, maxLift);
        }
    }
}
