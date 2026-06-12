using Core.EventBus;
using System.Collections.Generic;
using SB.App.Domain;
using SB.App.Events;
using SB.App.Views;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class WorryFlowPresenter : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private WorryFlowContent flowContent;
        [SerializeField] private NotificationCatalog notificationCatalog;

        [Header("Presenters")]
        [SerializeField] private WhiteboardPresenter whiteboardPresenter;

        [Header("Shell Views")]
        [SerializeField] private MainMenuView mainMenuView;
        [SerializeField] private CompanionBubbleView companionBubbleView;
        [SerializeField] private FocusOverlayView focusOverlayView;
        [SerializeField] private FeedbackPopupView feedbackPopupView;
        [SerializeField] private StepProgressView progressView;
        [SerializeField] private DailyClosureView dailyClosureView;

        [Header("Flow Views")]
        [SerializeField] private WorryInputView worryInputView;
        [SerializeField] private ProbabilityEstimateView probabilityEstimateView;
        [SerializeField] private CopingPlanView copingPlanView;
        [SerializeField] private ActionPlanView actionPlanView;
        [SerializeField] private EmotionCheckView emotionCheckView;

        private readonly WorryDraft _draft = new WorryDraft();
        private WorryRepository _repository;
        private DailyClosureService _dailyClosureService;
        private MobileNotificationService _notificationService;
        private SupportInterventionService _supportInterventionService;
        private bool _viewsBound;

        private void Awake()
        {
            InitializeFlow();
        }

        private void OnDestroy()
        {
            UnbindViews();
        }

        public void SetComposition(
            WorryFlowContent content,
            WhiteboardPresenter whiteboard,
            MainMenuView mainMenu,
            CompanionBubbleView companionBubble,
            FocusOverlayView focusOverlay,
            FeedbackPopupView feedbackPopup,
            StepProgressView progress,
            WorryInputView worryInput,
            ProbabilityEstimateView probabilityEstimate,
            CopingPlanView copingPlan,
            ActionPlanView actionPlan,
            EmotionCheckView emotionCheck,
            DailyClosureView dailyClosure = null,
            NotificationCatalog notifications = null)
        {
            UnbindViews();

            flowContent = content;
            notificationCatalog = notifications;
            whiteboardPresenter = whiteboard;
            mainMenuView = mainMenu;
            companionBubbleView = companionBubble;
            focusOverlayView = focusOverlay;
            feedbackPopupView = feedbackPopup;
            progressView = progress;
            worryInputView = worryInput;
            probabilityEstimateView = probabilityEstimate;
            copingPlanView = copingPlan;
            actionPlanView = actionPlan;
            emotionCheckView = emotionCheck;
            dailyClosureView = dailyClosure;

            InitializeFlow();
        }

        private void InitializeFlow()
        {
            _dailyClosureService = new DailyClosureService();
            _dailyClosureService.ClearExpiredLock();
            _notificationService = new MobileNotificationService(notificationCatalog);
            _supportInterventionService = new SupportInterventionService(_notificationService);
            _repository = new WorryRepository();

            if (whiteboardPresenter != null)
                whiteboardPresenter.Initialize(_repository, _supportInterventionService);

            BindViews();

            if (_dailyClosureService.IsCreateLocked)
                ShowDailyClosureScreen();
            else
                ShowWhiteboard();
        }

        public void StartNewWorry()
        {
            if (_dailyClosureService != null && !_dailyClosureService.CanCreateWorry)
            {
                ShowDailyClosureScreen();
                return;
            }

            _draft.Clear();
            ShowStep(WorryFlowStep.WorryInput);
        }

        public void CancelFlow()
        {
            _draft.Clear();
            ShowWhiteboard();
        }

        private void BindViews()
        {
            if (_viewsBound)
                return;

            if (mainMenuView != null)
                mainMenuView.StartRequested += StartNewWorry;

            if (whiteboardPresenter != null)
            {
                whiteboardPresenter.CreateRequested += StartNewWorry;
                whiteboardPresenter.DailyCloseRequested += ShowDailyClosure;
            }

            if (worryInputView != null)
            {
                worryInputView.Submitted += SubmitWorry;
                worryInputView.BackRequested += CancelFlow;
            }

            if (probabilityEstimateView != null)
            {
                probabilityEstimateView.Submitted += SubmitProbability;
                probabilityEstimateView.BackRequested += BackToWorryInput;
            }

            if (copingPlanView != null)
            {
                copingPlanView.Submitted += SubmitCopingPlan;
                copingPlanView.BackRequested += BackToProbabilityEstimate;
            }

            if (actionPlanView != null)
            {
                actionPlanView.Submitted += SubmitActionPlan;
                actionPlanView.BackRequested += BackToCopingPlan;
            }

            if (emotionCheckView != null)
            {
                emotionCheckView.EmotionSelected += SubmitEmotion;
                emotionCheckView.BackRequested += BackToActionPlan;
            }

            if (dailyClosureView != null)
            {
                dailyClosureView.Completed += CompleteDailyClosure;
                dailyClosureView.TestUnlockRequested += UnlockDailyClosureForTesting;
            }

            _viewsBound = true;
        }

        private void UnbindViews()
        {
            if (!_viewsBound)
                return;

            if (mainMenuView != null)
                mainMenuView.StartRequested -= StartNewWorry;

            if (whiteboardPresenter != null)
            {
                whiteboardPresenter.CreateRequested -= StartNewWorry;
                whiteboardPresenter.DailyCloseRequested -= ShowDailyClosure;
            }

            if (worryInputView != null)
            {
                worryInputView.Submitted -= SubmitWorry;
                worryInputView.BackRequested -= CancelFlow;
            }

            if (probabilityEstimateView != null)
            {
                probabilityEstimateView.Submitted -= SubmitProbability;
                probabilityEstimateView.BackRequested -= BackToWorryInput;
            }

            if (copingPlanView != null)
            {
                copingPlanView.Submitted -= SubmitCopingPlan;
                copingPlanView.BackRequested -= BackToProbabilityEstimate;
            }

            if (actionPlanView != null)
            {
                actionPlanView.Submitted -= SubmitActionPlan;
                actionPlanView.BackRequested -= BackToCopingPlan;
            }

            if (emotionCheckView != null)
            {
                emotionCheckView.EmotionSelected -= SubmitEmotion;
                emotionCheckView.BackRequested -= BackToActionPlan;
            }

            if (dailyClosureView != null)
            {
                dailyClosureView.Completed -= CompleteDailyClosure;
                dailyClosureView.TestUnlockRequested -= UnlockDailyClosureForTesting;
            }

            _viewsBound = false;
        }

        private void SubmitWorry(string worryText)
        {
            _draft.SetWorry(worryText);

            if (!_draft.HasWorry)
            {
                ShowFeedback("먼저 고민을 적어주세요.");
                return;
            }

            ShowStep(WorryFlowStep.ProbabilityEstimate);
        }

        private void SubmitProbability(int probabilityPercent)
        {
            _draft.SetProbability(probabilityPercent);
            ShowStep(WorryFlowStep.CopingPlan);
        }

        private void SubmitCopingPlan(IReadOnlyList<string> copingActions)
        {
            _draft.SetCopingActions(copingActions);

            if (!_draft.HasCopingPlan)
            {
                ShowFeedback("정말 일어났을 때 할 수 있는 일을 하나 적어주세요.");
                return;
            }

            ShowStep(WorryFlowStep.ActionPlan);
        }

        private void SubmitActionPlan(string actionPlan)
        {
            _draft.SetActionPlan(actionPlan);
            _draft.SetTakeaway(string.Empty);

            if (!_draft.HasActionPlan)
            {
                ShowFeedback("지금 할 수 있는 작은 행동을 하나 적어주세요.");
                return;
            }

            ShowStep(WorryFlowStep.EmotionCheck);
        }

        private void SubmitEmotion(WorryEmotionState emotionState)
        {
            _draft.SetEmotionState(emotionState);

            if (!_draft.HasEmotionState)
            {
                ShowFeedback("지금 마음 상태를 선택해주세요.");
                return;
            }

            SaveCurrentDraft();
        }

        private void SaveCurrentDraft()
        {
            WorryCard card = WorryCard.FromDraft(_draft);
            _repository.Add(card);
            _notificationService?.ScheduleWorryReview(card);
            _supportInterventionService?.ScheduleBestReminder(_repository, card);
            Bus<WorryCardCreatedEvent>.Raise(new WorryCardCreatedEvent(card));

            _draft.Clear();
            ShowWhiteboard();
        }

        private void BackToWorryInput()
        {
            ShowStep(WorryFlowStep.WorryInput);
        }

        private void BackToProbabilityEstimate()
        {
            ShowStep(WorryFlowStep.ProbabilityEstimate);
        }

        private void BackToCopingPlan()
        {
            ShowStep(WorryFlowStep.CopingPlan);
        }

        private void BackToActionPlan()
        {
            ShowStep(WorryFlowStep.ActionPlan);
        }

        private void ShowWhiteboard()
        {
            if (_dailyClosureService != null && _dailyClosureService.IsCreateLocked)
            {
                ShowDailyClosureScreen();
                return;
            }

            HideFlowViews();

            if (focusOverlayView != null)
                focusOverlayView.Hide();

            if (progressView != null)
                progressView.Hide();

            if (companionBubbleView != null)
                companionBubbleView.Hide();

            if (dailyClosureView != null)
                dailyClosureView.Hide();

            if (mainMenuView != null)
                mainMenuView.Show();

            if (whiteboardPresenter != null)
            {
                whiteboardPresenter.Show();
                whiteboardPresenter.SetCreateAvailability(true, "+ 새로운 걱정 정리하기");
            }
        }

        private void ShowStep(WorryFlowStep step)
        {
            HideFlowViews();

            if (mainMenuView != null)
                mainMenuView.Hide();

            if (whiteboardPresenter != null)
                whiteboardPresenter.Hide();

            if (dailyClosureView != null)
                dailyClosureView.Hide();

            if (focusOverlayView != null)
                focusOverlayView.Show();

            if (progressView != null)
                progressView.ShowStep(step);

            WorryStepContent content = GetContent(step);

            if (companionBubbleView != null)
                companionBubbleView.ShowMessage(content.companionMessage);

            switch (step)
            {
                case WorryFlowStep.WorryInput:
                    ShowWorryInput(content);
                    break;
                case WorryFlowStep.ProbabilityEstimate:
                    ShowProbabilityEstimate(content);
                    break;
                case WorryFlowStep.CopingPlan:
                    ShowCopingPlan(content);
                    break;
                case WorryFlowStep.ActionPlan:
                    ShowActionPlan(content);
                    break;
                case WorryFlowStep.EmotionCheck:
                    ShowEmotionCheck(content);
                    break;
            }
        }

        private void ShowWorryInput(WorryStepContent content)
        {
            if (worryInputView == null)
                return;

            worryInputView.ApplyContent(content);
            worryInputView.SetInput(_draft.WorryText);
            worryInputView.Show();
        }

        private void ShowProbabilityEstimate(WorryStepContent content)
        {
            if (probabilityEstimateView == null)
                return;

            probabilityEstimateView.ApplyContent(content);
            probabilityEstimateView.SetProbability(_draft.ProbabilityPercent);
            probabilityEstimateView.Show();
        }

        private void ShowCopingPlan(WorryStepContent content)
        {
            if (copingPlanView == null)
                return;

            copingPlanView.ApplyContent(content);
            copingPlanView.SetActions(_draft.CopingActions);
            copingPlanView.Show();
        }

        private void ShowActionPlan(WorryStepContent content)
        {
            if (actionPlanView == null)
                return;

            actionPlanView.ApplyContent(content);
            actionPlanView.SetInput(_draft.ActionPlan);
            actionPlanView.Show();
        }

        private void ShowEmotionCheck(WorryStepContent content)
        {
            if (emotionCheckView == null)
                return;

            emotionCheckView.ApplyContent(content);
            emotionCheckView.Show();
        }

        private void HideFlowViews()
        {
            if (worryInputView != null)
                worryInputView.Hide();

            if (probabilityEstimateView != null)
                probabilityEstimateView.Hide();

            if (copingPlanView != null)
                copingPlanView.Hide();

            if (actionPlanView != null)
                actionPlanView.Hide();

            if (emotionCheckView != null)
                emotionCheckView.Hide();
        }

        private void ShowDailyClosure()
        {
            DailyClosureState state = _dailyClosureService != null
                ? _dailyClosureService.CloseToday()
                : new DailyClosureState(System.DateTime.Now, System.DateTime.Now.Date.AddDays(1).AddHours(12));

            _notificationService?.ScheduleDailyClosure(state);
            ShowDailyClosureScreen();
        }

        private void ShowDailyClosureScreen()
        {
            HideFlowViews();

            if (mainMenuView != null)
                mainMenuView.Hide();

            if (whiteboardPresenter != null)
                whiteboardPresenter.Hide();

            if (focusOverlayView != null)
                focusOverlayView.Hide();

            if (progressView != null)
                progressView.Hide();

            if (companionBubbleView != null)
                companionBubbleView.Hide();

            if (dailyClosureView != null)
                dailyClosureView.Show();
        }

        private void CompleteDailyClosure()
        {
            ShowWhiteboard();
        }

        private void UnlockDailyClosureForTesting()
        {
            if (_dailyClosureService == null)
                _dailyClosureService = new DailyClosureService();

            _dailyClosureService.ClearLockForTesting();

            if (dailyClosureView != null)
                dailyClosureView.Hide();

            ShowWhiteboard();
            ShowFeedback("테스트용으로 하루 마무리 잠금을 해제했어요.");
        }

        private void ShowFeedback(string message)
        {
            if (feedbackPopupView != null)
                feedbackPopupView.ShowMessage(message);
        }

        private WorryStepContent GetContent(WorryFlowStep step)
        {
            if (flowContent != null)
                return flowContent.GetStep(step);

            return new WorryStepContent
            {
                step = step,
                title = WorryFlowContent.GetFallbackTitle(step),
                companionMessage = string.Empty,
                primaryActionLabel = step == WorryFlowStep.EmotionCheck ? "저장" : "다음",
                secondaryActionLabel = step == WorryFlowStep.WorryInput ? "취소" : "이전"
            };
        }
    }
}
