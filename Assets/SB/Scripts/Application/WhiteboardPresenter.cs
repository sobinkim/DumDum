using System;
using Core.EventBus;
using SB.App.Domain;
using SB.App.Events;
using SB.App.Views;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class WhiteboardPresenter : MonoBehaviour
    {
        [SerializeField] private WhiteboardView whiteboardView;
        [SerializeField] private WorryDetailView detailView;

        private WorryRepository _repository;
        private SupportInterventionService _supportInterventionService;
        private WorryCard _selectedCard;
        private bool _viewsBound;
        private bool _busBound;

        public event Action CreateRequested;
        public event Action DailyCloseRequested;

        private void Awake()
        {
            BindViews();
            BindBus();
        }

        private void OnDestroy()
        {
            UnbindViews();

            if (_repository != null)
                _repository.Changed -= Refresh;

            UnbindBus();
        }

        public void SetViews(WhiteboardView whiteboard, WorryDetailView detail)
        {
            UnbindViews();
            whiteboardView = whiteboard;
            detailView = detail;
            BindViews();
        }

        private void BindViews()
        {
            if (_viewsBound)
                return;

            if (whiteboardView != null)
            {
                whiteboardView.CardSelected += HandleCardSelected;
                whiteboardView.CreateRequested += HandleCreateRequested;
                whiteboardView.DailyCloseRequested += HandleDailyCloseRequested;
            }

            if (detailView != null)
            {
                detailView.Closed += HandleDetailClosed;
                detailView.OutcomeTagged += HandleOutcomeTagged;
                detailView.ReviewCompleted += HandleReviewCompleted;
            }

            _viewsBound = true;
        }

        private void UnbindViews()
        {
            if (!_viewsBound)
                return;

            if (whiteboardView != null)
            {
                whiteboardView.CardSelected -= HandleCardSelected;
                whiteboardView.CreateRequested -= HandleCreateRequested;
                whiteboardView.DailyCloseRequested -= HandleDailyCloseRequested;
            }

            if (detailView != null)
            {
                detailView.Closed -= HandleDetailClosed;
                detailView.OutcomeTagged -= HandleOutcomeTagged;
                detailView.ReviewCompleted -= HandleReviewCompleted;
            }

            _viewsBound = false;
        }

        private void BindBus()
        {
            if (_busBound)
                return;

            Bus<WorryCardCreatedEvent>.OnEvent += HandleCardCreated;
            _busBound = true;
        }

        private void UnbindBus()
        {
            if (!_busBound)
                return;

            Bus<WorryCardCreatedEvent>.OnEvent -= HandleCardCreated;
            _busBound = false;
        }

        public void Initialize(WorryRepository repository)
        {
            Initialize(repository, null);
        }

        public void Initialize(WorryRepository repository, SupportInterventionService supportInterventionService)
        {
            if (_repository != null)
                _repository.Changed -= Refresh;

            _repository = repository;
            _supportInterventionService = supportInterventionService;

            if (_repository != null)
                _repository.Changed += Refresh;

            Refresh();
        }

        public void Show()
        {
            if (whiteboardView != null)
                whiteboardView.Show();

            Refresh();
        }

        public void Hide()
        {
            if (whiteboardView != null)
                whiteboardView.Hide();

            if (detailView != null)
                detailView.Hide();
        }

        public void Refresh()
        {
            if (whiteboardView != null && _repository != null)
            {
                WorryOutcomeSummary summary = WorryOutcomeSummary.FromCards(_repository.Cards);
                whiteboardView.Render(_repository.Cards, summary);
            }
        }

        public void SetCreateAvailability(bool canCreate, string label)
        {
            if (whiteboardView != null)
                whiteboardView.SetCreateAvailability(canCreate, label);
        }

        private void HandleCardCreated(WorryCardCreatedEvent evt)
        {
            Refresh();
        }

        private void HandleCardSelected(WorryCard card)
        {
            _selectedCard = card;

            Bus<WorryCardSelectedEvent>.Raise(new WorryCardSelectedEvent(card));

            if (detailView != null)
                detailView.ShowCard(card);
        }

        private void HandleCreateRequested()
        {
            CreateRequested?.Invoke();
        }

        private void HandleDailyCloseRequested()
        {
            DailyCloseRequested?.Invoke();
        }

        private void HandleOutcomeTagged(WorryOutcomeTag tag)
        {
            if (_repository == null || _selectedCard == null)
                return;

            _repository.TagOutcome(_selectedCard.Id, tag);
            _selectedCard = _repository.Find(_selectedCard.Id);

            Bus<WorryOutcomeTaggedEvent>.Raise(new WorryOutcomeTaggedEvent(_selectedCard, tag));

            if (detailView != null)
                detailView.ShowOutcomeReview(_selectedCard, tag);

            Refresh();
        }

        private void HandleReviewCompleted(string takeaway)
        {
            if (_repository == null || _selectedCard == null)
                return;

            _repository.UpdateTakeaway(_selectedCard.Id, takeaway);
            _selectedCard = _repository.Find(_selectedCard.Id);
            _supportInterventionService?.ScheduleBestReminder(_repository, _selectedCard);

            if (detailView != null)
                detailView.Hide();

            _selectedCard = null;
            Refresh();
        }

        private void HandleDetailClosed()
        {
            _selectedCard = null;
        }
    }
}
