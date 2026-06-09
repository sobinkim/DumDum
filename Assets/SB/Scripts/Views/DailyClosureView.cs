using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class DailyClosureView : UIScreenView, IPointerClickHandler
    {
        private const int TestUnlockTapCount = 5;
        private const float TestUnlockTapResetSeconds = 1.4f;
        private const string ClosureMessage = "잘 생각했어요.\n아직 많은 고민과 불안이 남아 있겠지만,\n오늘은 여기까지 하고 지금 이 순간에 집중해 봐요.";

        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button completeButton;
        [SerializeField] private float characterInterval = 0.055f;

        private bool _isListening;
        private int _testTapCount;
        private float _lastTestTapTime;
        private Coroutine _typingRoutine;

        public event Action Completed;
        public event Action TestUnlockRequested;

        private void Awake()
        {
            RegisterListeners();
            Hide();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
            StopMessage();
        }

        public void SetControls(TMP_Text message)
        {
            SetControls(message, null, 0f);
        }

        public void SetControls(TMP_Text message, Button complete, float autoSeconds)
        {
            UnregisterListeners();
            messageText = message;
            completeButton = complete;
            RegisterListeners();
        }

        public override void Show()
        {
            base.Show();
            PlayMessage();
        }

        public override void Hide()
        {
            StopMessage();
            base.Hide();
        }

        public void Complete()
        {
            Hide();
            Completed?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Time.unscaledTime - _lastTestTapTime > TestUnlockTapResetSeconds)
                _testTapCount = 0;

            _lastTestTapTime = Time.unscaledTime;
            _testTapCount++;

            if (_testTapCount < TestUnlockTapCount)
                return;

            _testTapCount = 0;
            TestUnlockRequested?.Invoke();
#endif
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (completeButton != null)
                completeButton.onClick.AddListener(Complete);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (completeButton != null)
                completeButton.onClick.RemoveListener(Complete);

            _isListening = false;
        }

        private void PlayMessage()
        {
            if (messageText == null)
                return;

            StopMessage();
            messageText.text = string.Empty;

            if (!isActiveAndEnabled)
            {
                messageText.text = ClosureMessage;
                return;
            }

            _typingRoutine = StartCoroutine(TypeMessage());
        }

        private void StopMessage()
        {
            if (_typingRoutine == null)
                return;

            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }

        private IEnumerator TypeMessage()
        {
            for (int i = 0; i < ClosureMessage.Length; i++)
            {
                char nextCharacter = ClosureMessage[i];
                messageText.text = ClosureMessage.Substring(0, i + 1);

                float waitTime = characterInterval;
                if (nextCharacter == '\n')
                    waitTime *= 3f;
                else if (char.IsWhiteSpace(nextCharacter))
                    waitTime *= 0.45f;

                yield return new WaitForSecondsRealtime(waitTime);
            }

            _typingRoutine = null;
        }
    }
}
