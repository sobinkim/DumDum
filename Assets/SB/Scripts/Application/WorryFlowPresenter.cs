using Core.EventBus;
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
        [SerializeField] private FactCheckView factCheckView;
        [SerializeField] private ThoughtCheckView thoughtCheckView;
        [SerializeField] private ActionPlanView actionPlanView;
        [SerializeField] private TakeawaySummaryView takeawaySummaryView;
        [SerializeField] private EmotionCheckView emotionCheckView;

        private readonly WorryDraft _draft = new WorryDraft();
        private WorryRepository _repository;
        private DailyClosureService _dailyClosureService;
        private MobileNotificationService _notificationService;
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
            FactCheckView factCheck,
            ThoughtCheckView thoughtCheck,
            ActionPlanView actionPlan,
            TakeawaySummaryView takeawaySummary,
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
            factCheckView = factCheck;
            thoughtCheckView = thoughtCheck;
            actionPlanView = actionPlan;
            takeawaySummaryView = takeawaySummary;
            emotionCheckView = emotionCheck;
            dailyClosureView = dailyClosure;

            InitializeFlow();
        }

        private void InitializeFlow()
        {
            _dailyClosureService = new DailyClosureService();
            _dailyClosureService.ClearExpiredLock();
            _notificationService = new MobileNotificationService(notificationCatalog);
            _repository = new WorryRepository();

            if (whiteboardPresenter != null)
                whiteboardPresenter.Initialize(_repository);

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

            if (factCheckView != null)
            {
                factCheckView.Submitted += SubmitFactCheck;
                factCheckView.BackRequested += BackToWorryInput;
            }

            if (thoughtCheckView != null)
            {
                thoughtCheckView.Submitted += SubmitThoughtCheck;
                thoughtCheckView.BackRequested += BackToFactCheck;
            }

            if (actionPlanView != null)
            {
                actionPlanView.Submitted += SubmitActionPlan;
                actionPlanView.BackRequested += BackToThoughtCheck;
            }

            if (takeawaySummaryView != null)
            {
                takeawaySummaryView.Submitted += SubmitTakeaway;
                takeawaySummaryView.BackRequested += BackToActionPlan;
            }

            if (emotionCheckView != null)
            {
                emotionCheckView.EmotionSelected += SubmitEmotion;
                emotionCheckView.BackRequested += BackToTakeawaySummary;
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

            if (factCheckView != null)
            {
                factCheckView.Submitted -= SubmitFactCheck;
                factCheckView.BackRequested -= BackToWorryInput;
            }

            if (thoughtCheckView != null)
            {
                thoughtCheckView.Submitted -= SubmitThoughtCheck;
                thoughtCheckView.BackRequested -= BackToFactCheck;
            }

            if (actionPlanView != null)
            {
                actionPlanView.Submitted -= SubmitActionPlan;
                actionPlanView.BackRequested -= BackToThoughtCheck;
            }

            if (takeawaySummaryView != null)
            {
                takeawaySummaryView.Submitted -= SubmitTakeaway;
                takeawaySummaryView.BackRequested -= BackToActionPlan;
            }

            if (emotionCheckView != null)
            {
                emotionCheckView.EmotionSelected -= SubmitEmotion;
                emotionCheckView.BackRequested -= BackToTakeawaySummary;
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
                ShowFeedback("지금 고민되는 일을 한 문장으로 적어주세요.");
                return;
            }

            ShowStep(WorryFlowStep.FactCheck);
        }

        private void SubmitFactCheck(string facts, string assumptions)
        {
            _draft.SetFactCheck(facts, assumptions);

            if (!_draft.HasFactCheck)
            {
                ShowFeedback("확실한 사실과 내가 추측한 내용을 나누어 적어주세요.");
                return;
            }

            ShowStep(WorryFlowStep.ThoughtCheck);
        }

        private void SubmitThoughtCheck(string evidence, string counterEvidence, string alternativeThought)
        {
            _draft.SetThoughtCheck(evidence, counterEvidence, alternativeThought);

            if (!_draft.HasThoughtCheck)
            {
                ShowFeedback("지금 생각을 점검할 근거나 다른 해석을 하나 적어주세요.");
                return;
            }

            ShowStep(WorryFlowStep.ActionPlan);
        }

        private void SubmitActionPlan(string actionPlan)
        {
            _draft.SetActionPlan(actionPlan);

            if (!_draft.HasActionPlan)
            {
                ShowFeedback("지금 할 수 있는 작은 행동을 하나 적어주세요.");
                return;
            }

            ShowStep(WorryFlowStep.TakeawaySummary);
        }

        private void SubmitTakeaway(string takeaway)
        {
            _draft.SetTakeaway(takeaway);

            if (!_draft.HasTakeaway)
            {
                ShowFeedback("이번 정리를 기억할 한 줄을 적어주세요.");
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
            _notificationService?.ScheduleWorryReview(card, _repository);
            Bus<WorryCardCreatedEvent>.Raise(new WorryCardCreatedEvent(card));

            _draft.Clear();
            ShowWhiteboard();
        }

        private void BackToWorryInput()
        {
            ShowStep(WorryFlowStep.WorryInput);
        }

        private void BackToFactCheck()
        {
            ShowStep(WorryFlowStep.FactCheck);
        }

        private void BackToThoughtCheck()
        {
            ShowStep(WorryFlowStep.ThoughtCheck);
        }

        private void BackToActionPlan()
        {
            ShowStep(WorryFlowStep.ActionPlan);
        }

        private void BackToTakeawaySummary()
        {
            ShowStep(WorryFlowStep.TakeawaySummary);
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
                case WorryFlowStep.FactCheck:
                    ShowFactCheck(content);
                    break;
                case WorryFlowStep.ThoughtCheck:
                    ShowThoughtCheck(content);
                    break;
                case WorryFlowStep.ActionPlan:
                    ShowActionPlan(content);
                    break;
                case WorryFlowStep.TakeawaySummary:
                    ShowTakeawaySummary(content);
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

        private void ShowFactCheck(WorryStepContent content)
        {
            if (factCheckView == null)
                return;

            factCheckView.ApplyContent(content);
            factCheckView.SetInputs(_draft.FactsText, _draft.AssumptionsText);
            factCheckView.Show();
        }

        private void ShowThoughtCheck(WorryStepContent content)
        {
            if (thoughtCheckView == null)
                return;

            thoughtCheckView.ApplyContent(content);
            thoughtCheckView.SetInputs(_draft.EvidenceText, _draft.CounterEvidenceText, _draft.AlternativeThoughtText);
            thoughtCheckView.Show();
        }

        private void ShowActionPlan(WorryStepContent content)
        {
            if (actionPlanView == null)
                return;

            actionPlanView.ApplyContent(content);
            actionPlanView.SetInput(_draft.ActionPlan);
            actionPlanView.Show();
        }

        private void ShowTakeawaySummary(WorryStepContent content)
        {
            if (takeawaySummaryView == null)
                return;

            takeawaySummaryView.ApplyContent(content);
            takeawaySummaryView.SetInput(_draft.Takeaway);
            takeawaySummaryView.Show();
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

            if (factCheckView != null)
                factCheckView.Hide();

            if (thoughtCheckView != null)
                thoughtCheckView.Hide();

            if (actionPlanView != null)
                actionPlanView.Hide();

            if (takeawaySummaryView != null)
                takeawaySummaryView.Hide();

            if (emotionCheckView != null)
                emotionCheckView.Hide();
        }

        private void ShowDailyClosure()
        {
            DailyClosureState state = _dailyClosureService != null
                ? _dailyClosureService.CloseToday()
                : new DailyClosureState(System.DateTime.Now, System.DateTime.Now.Date.AddDays(1).AddHours(12));

            _notificationService?.ScheduleDailyClosure(state, _repository);
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
