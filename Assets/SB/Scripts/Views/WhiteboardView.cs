using System;
using System.Collections.Generic;
using SB.App.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class WhiteboardView : UIScreenView
    {
        [SerializeField] private Transform cardRoot;
        [SerializeField] private WorryCardView cardPrefab;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private Button createButton;
        [SerializeField] private Button dailyCloseButton;
        [SerializeField] private Button outcomeSummaryButton;
        [SerializeField] private OutcomeSummaryView outcomeSummaryView;

        private readonly List<WorryCardView> _spawnedCards = new List<WorryCardView>();
        private bool _isListening;

        public event Action<WorryCard> CardSelected;
        public event Action CreateRequested;
        public event Action DailyCloseRequested;

        private void Awake()
        {
            RegisterListeners();
        }

        public void SetControls(Transform cardsParent, WorryCardView cardTemplate, GameObject emptyStateObject)
        {
            SetControls(cardsParent, cardTemplate, emptyStateObject, null, null);
        }

        public void SetControls(
            Transform cardsParent,
            WorryCardView cardTemplate,
            GameObject emptyStateObject,
            OutcomeSummaryView summaryView)
        {
            SetControls(cardsParent, cardTemplate, emptyStateObject, null, summaryView);
        }

        public void SetControls(
            Transform cardsParent,
            WorryCardView cardTemplate,
            GameObject emptyStateObject,
            Button summaryButton,
            OutcomeSummaryView summaryView)
        {
            SetControls(cardsParent, cardTemplate, emptyStateObject, null, summaryButton, summaryView);
        }

        public void SetControls(
            Transform cardsParent,
            WorryCardView cardTemplate,
            GameObject emptyStateObject,
            Button create,
            Button summaryButton,
            OutcomeSummaryView summaryView)
        {
            UnregisterListeners();
            cardRoot = cardsParent;
            cardPrefab = cardTemplate;
            emptyState = emptyStateObject;
            createButton = create;
            outcomeSummaryButton = summaryButton;
            outcomeSummaryView = summaryView;
            RegisterListeners();
        }

        public void SetDailyCloseButton(Button dailyClose)
        {
            UnregisterListeners();
            dailyCloseButton = dailyClose;
            RegisterListeners();
        }

        public void SetCreateAvailability(bool canCreate, string label)
        {
            if (createButton == null)
                return;

            createButton.interactable = canCreate;

            TMP_Text labelText = createButton.GetComponentInChildren<TMP_Text>(true);
            if (labelText != null)
                labelText.text = label;
        }

        public override void Hide()
        {
            base.Hide();

            if (outcomeSummaryView != null)
                outcomeSummaryView.Hide();
        }

        public void Render(IReadOnlyList<WorryCard> cards)
        {
            Render(cards, WorryOutcomeSummary.FromCards(cards));
        }

        public void Render(IReadOnlyList<WorryCard> cards, WorryOutcomeSummary summary)
        {
            ClearCards();

            if (outcomeSummaryView != null)
                outcomeSummaryView.Render(summary);

            bool hasCards = cards != null && cards.Count > 0;
            ConfigureCardGrid(hasCards ? cards.Count : 0);

            if (emptyState != null)
                emptyState.SetActive(!hasCards);

            if (!hasCards || cardPrefab == null || cardRoot == null)
                return;

            for (int i = 0; i < cards.Count; i++)
            {
                WorryCardView cardView = Instantiate(cardPrefab, cardRoot);
                cardView.gameObject.SetActive(true);
                cardView.Bind(cards[i]);
                cardView.Selected += HandleCardSelected;
                _spawnedCards.Add(cardView);
            }

            ConfigureCardGrid(cards.Count);
        }

        private void OnDestroy()
        {
            UnregisterListeners();
            ClearCards();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (outcomeSummaryButton != null)
                outcomeSummaryButton.onClick.AddListener(OpenOutcomeSummary);

            if (createButton != null)
                createButton.onClick.AddListener(RequestCreate);

            if (dailyCloseButton != null)
                dailyCloseButton.onClick.AddListener(RequestDailyClose);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (outcomeSummaryButton != null)
                outcomeSummaryButton.onClick.RemoveListener(OpenOutcomeSummary);

            if (createButton != null)
                createButton.onClick.RemoveListener(RequestCreate);

            if (dailyCloseButton != null)
                dailyCloseButton.onClick.RemoveListener(RequestDailyClose);

            _isListening = false;
        }

        private void OpenOutcomeSummary()
        {
            if (outcomeSummaryView != null)
                outcomeSummaryView.Show();
        }

        private void RequestCreate()
        {
            CreateRequested?.Invoke();
        }

        private void RequestDailyClose()
        {
            DailyCloseRequested?.Invoke();
        }

        private void ClearCards()
        {
            for (int i = 0; i < _spawnedCards.Count; i++)
            {
                WorryCardView cardView = _spawnedCards[i];
                if (cardView == null)
                    continue;

                cardView.Selected -= HandleCardSelected;
            }

            _spawnedCards.Clear();

            if (cardRoot == null)
                return;

            for (int i = cardRoot.childCount - 1; i >= 0; i--)
            {
                Transform child = cardRoot.GetChild(i);
                WorryCardView leftoverCard = child.GetComponent<WorryCardView>();
                if (leftoverCard == null)
                    continue;

                if (UnityEngine.Application.isPlaying)
                {
                    child.gameObject.SetActive(false);
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        private void ConfigureCardGrid(int cardCount)
        {
            if (cardRoot == null)
                return;

            GridLayoutGroup grid = cardRoot.GetComponent<GridLayoutGroup>();
            LayoutElement layout = cardRoot.GetComponent<LayoutElement>();
            RectTransform rect = cardRoot as RectTransform;

            if (grid == null)
                return;

            grid.childAlignment = TextAnchor.UpperLeft;
            grid.padding = new RectOffset(8, 8, 10, 20);
            grid.cellSize = new Vector2(150f, 176f);

            int columns = grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                ? Mathf.Max(1, grid.constraintCount)
                : 2;

            int rows = cardCount <= 0 ? 0 : Mathf.CeilToInt(cardCount / (float)columns);
            float preferredHeight = rows <= 0
                ? 0f
                : grid.padding.vertical + rows * grid.cellSize.y + Mathf.Max(0, rows - 1) * grid.spacing.y;

            if (layout != null)
            {
                layout.minHeight = preferredHeight;
                layout.preferredHeight = preferredHeight;
            }

            if (rect != null)
            {
                Vector2 size = rect.sizeDelta;
                rect.sizeDelta = new Vector2(size.x, preferredHeight);
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
                LayoutRebuilder.MarkLayoutForRebuild(rect);

                RectTransform parent = rect.parent as RectTransform;
                if (parent != null)
                    LayoutRebuilder.MarkLayoutForRebuild(parent);
            }
        }

        private void HandleCardSelected(WorryCard card)
        {
            CardSelected?.Invoke(card);
        }
    }
}
