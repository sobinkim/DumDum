using System;
using SB.App.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class WorryDetailView : UIScreenView
    {
        [SerializeField] private TMP_Text worryText;
        [SerializeField] private TMP_Text probabilityText;
        [SerializeField] private TMP_Text copingPlanText;
        [SerializeField] private TMP_Text actionPlanText;
        [SerializeField] private TMP_Text emotionText;
        [SerializeField] private TMP_Text outcomeText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button didNotHappenButton;
        [SerializeField] private Button partiallyHappenedButton;
        [SerializeField] private Button happenedButton;

        [Header("Outcome Review")]
        [SerializeField] private GameObject reviewRoot;
        [SerializeField] private TMP_InputField takeawayInput;
        [SerializeField] private TMP_Text reviewHintText;
        [SerializeField] private Button reviewConfirmButton;

        private WorryCard _card;
        private bool _isListening;

        public event Action Closed;
        public event Action<WorryOutcomeTag> OutcomeTagged;
        public event Action<string> ReviewCompleted;

        private void Awake()
        {
            EnsureReviewControls();
            RegisterListeners();
            Hide();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(
            TMP_Text worry,
            TMP_Text probability,
            TMP_Text copingPlan,
            TMP_Text actionPlan,
            TMP_Text emotion,
            TMP_Text outcome,
            Button close,
            Button didNotHappen,
            Button partiallyHappened,
            Button happened)
        {
            UnregisterListeners();
            worryText = worry;
            probabilityText = probability;
            copingPlanText = copingPlan;
            actionPlanText = actionPlan;
            emotionText = emotion;
            outcomeText = outcome;
            closeButton = close;
            didNotHappenButton = didNotHappen;
            partiallyHappenedButton = partiallyHappened;
            happenedButton = happened;
            EnsureReviewControls();
            RegisterListeners();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            if (didNotHappenButton != null)
                didNotHappenButton.onClick.AddListener(TagDidNotHappen);

            if (partiallyHappenedButton != null)
                partiallyHappenedButton.onClick.AddListener(TagPartiallyHappened);

            if (happenedButton != null)
                happenedButton.onClick.AddListener(TagHappened);

            if (reviewConfirmButton != null)
                reviewConfirmButton.onClick.AddListener(CompleteReview);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);

            if (didNotHappenButton != null)
                didNotHappenButton.onClick.RemoveListener(TagDidNotHappen);

            if (partiallyHappenedButton != null)
                partiallyHappenedButton.onClick.RemoveListener(TagPartiallyHappened);

            if (happenedButton != null)
                happenedButton.onClick.RemoveListener(TagHappened);

            if (reviewConfirmButton != null)
                reviewConfirmButton.onClick.RemoveListener(CompleteReview);

            _isListening = false;
        }

        public void ShowCard(WorryCard card)
        {
            _card = card;

            if (card == null)
            {
                Hide();
                return;
            }

            EnsureReviewControls();
            SetReviewMode(false);
            SetOutcomeButtonsInteractable(true);
            ConfigureStaticButtonLabels();

            if (worryText != null)
                worryText.text = CreateWorryText(card);

            if (probabilityText != null)
                probabilityText.text = $"처음 예상 확률 {card.ProbabilityPercent}%";

            if (copingPlanText != null)
                copingPlanText.text = CreateCopingText(card);

            if (actionPlanText != null)
                actionPlanText.text = CreateSummaryText(card);

            if (emotionText != null)
                emotionText.text = $"기록 당시 마음: {card.EmotionState.ToLabel()}";

            if (outcomeText != null)
                outcomeText.text = card.OutcomeTag == WorryOutcomeTag.Untagged
                    ? $"{CreateDaysAgoText(card.CreatedAt)}의 걱정이에요. 실제 결과를 확인해볼까요?"
                    : $"실제 결과: {card.OutcomeTag.ToLabel()}";

            Show();
        }

        public void ShowOutcomeReview(WorryCard card, WorryOutcomeTag tag)
        {
            _card = card;

            if (card == null)
            {
                Hide();
                return;
            }

            EnsureReviewControls();
            SetReviewMode(true);
            ConfigureStaticButtonLabels();

            if (worryText != null)
                worryText.text = $"이번 걱정 돌아보기\n{Trim(card.WorryText, 72)}";

            if (probabilityText != null)
                probabilityText.text = $"처음 예상 {card.ProbabilityPercent}%";

            if (copingPlanText != null)
                copingPlanText.text = $"실제 결과\n{tag.ToLabel()}";

            if (actionPlanText != null)
                actionPlanText.text = CreateReviewMessage(card, tag);

            if (emotionText != null)
                emotionText.text = "다음의 나에게 남길 한 줄";

            if (outcomeText != null)
                outcomeText.text = "이 문장은 카드에 남아서 다음 걱정을 볼 때 근거가 됩니다.";

            if (reviewHintText != null)
                reviewHintText.text = "짧게 남겨도 충분해요.";

            if (takeawayInput != null)
                takeawayInput.text = string.IsNullOrWhiteSpace(card.Takeaway)
                    ? CreateDefaultTakeaway(card, tag)
                    : card.Takeaway;

            Show();
        }

        public void Close()
        {
            Hide();
            Closed?.Invoke();
        }

        public void TagDidNotHappen()
        {
            TagOutcome(WorryOutcomeTag.DidNotHappen);
        }

        public void TagPartiallyHappened()
        {
            TagOutcome(WorryOutcomeTag.PartiallyHappened);
        }

        public void TagHappened()
        {
            TagOutcome(WorryOutcomeTag.Happened);
        }

        private void TagOutcome(WorryOutcomeTag tag)
        {
            if (_card == null)
                return;

            SetOutcomeButtonsInteractable(false);
            OutcomeTagged?.Invoke(tag);
        }

        private void CompleteReview()
        {
            string takeaway = takeawayInput != null ? takeawayInput.text : string.Empty;
            ReviewCompleted?.Invoke(takeaway);
        }

        private void SetReviewMode(bool isReviewMode)
        {
            if (reviewRoot != null)
                reviewRoot.SetActive(isReviewMode);

            SetOutcomeButtonRowVisible(!isReviewMode);

            if (closeButton != null)
                closeButton.gameObject.SetActive(true);
        }

        private void SetOutcomeButtonRowVisible(bool visible)
        {
            GameObject row = GetOutcomeButtonRow();
            if (row != null)
            {
                row.SetActive(visible);
                return;
            }

            if (didNotHappenButton != null)
                didNotHappenButton.gameObject.SetActive(visible);

            if (partiallyHappenedButton != null)
                partiallyHappenedButton.gameObject.SetActive(visible);

            if (happenedButton != null)
                happenedButton.gameObject.SetActive(visible);
        }

        private void SetOutcomeButtonsInteractable(bool interactable)
        {
            if (didNotHappenButton != null)
                didNotHappenButton.interactable = interactable;

            if (partiallyHappenedButton != null)
                partiallyHappenedButton.interactable = interactable;

            if (happenedButton != null)
                happenedButton.interactable = interactable;
        }

        private GameObject GetOutcomeButtonRow()
        {
            return didNotHappenButton != null && didNotHappenButton.transform.parent != null
                ? didNotHappenButton.transform.parent.gameObject
                : null;
        }

        private void EnsureReviewControls()
        {
            if (reviewRoot != null && takeawayInput != null && reviewConfirmButton != null)
                return;

            Transform parent = Root.transform;
            RectTransform reviewRect = CreateRect("Outcome Review Section", parent);
            reviewRoot = reviewRect.gameObject;

            Image background = reviewRoot.AddComponent<Image>();
            background.color = new Color(0.94f, 0.97f, 0.95f);
            background.raycastTarget = true;

            VerticalLayoutGroup layout = reviewRoot.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 10, 10);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            LayoutElement layoutElement = reviewRoot.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 124f;
            layoutElement.minHeight = 112f;

            reviewHintText = CreateText(reviewRect, "짧게 남겨도 충분해요.", 13f, FontStyles.Normal, new Color(0.33f, 0.38f, 0.36f), TextAlignmentOptions.Left);
            AddLayout(reviewHintText.gameObject, -1f, 20f);

            takeawayInput = CreateTakeawayInput(reviewRect);
            reviewConfirmButton = CreateReviewButton(reviewRect);

            int reviewIndex = outcomeText != null ? outcomeText.transform.GetSiblingIndex() + 1 : parent.childCount - 1;
            reviewRoot.transform.SetSiblingIndex(Mathf.Clamp(reviewIndex, 0, parent.childCount - 1));
            reviewRoot.SetActive(false);
        }

        private TMP_InputField CreateTakeawayInput(RectTransform parent)
        {
            RectTransform root = CreateRect("Takeaway Input", parent);
            Image image = root.gameObject.AddComponent<Image>();
            image.color = Color.white;
            image.raycastTarget = true;
            AddLayout(root.gameObject, -1f, 42f);

            TMP_InputField input = root.gameObject.AddComponent<TMP_InputField>();
            input.targetGraphic = image;
            input.lineType = TMP_InputField.LineType.SingleLine;
            input.characterLimit = 42;
            input.shouldHideMobileInput = true;
            input.resetOnDeActivation = false;

            RectTransform viewport = CreateRect("Viewport", root);
            Anchor(viewport, Vector2.zero, Vector2.one, new Vector2(10f, 7f), new Vector2(-10f, -7f));

            TMP_Text placeholder = CreateText(viewport, "예: 생각보다 괜찮게 지나갔다", 14f, FontStyles.Italic, new Color(0.5f, 0.55f, 0.53f), TextAlignmentOptions.MidlineLeft);
            Stretch(placeholder.rectTransform);

            TMP_Text inputText = CreateText(viewport, string.Empty, 14f, FontStyles.Normal, new Color(0.13f, 0.14f, 0.16f), TextAlignmentOptions.MidlineLeft);
            Stretch(inputText.rectTransform);

            input.textViewport = viewport;
            input.placeholder = placeholder;
            input.textComponent = inputText;
            return input;
        }

        private Button CreateReviewButton(RectTransform parent)
        {
            RectTransform root = CreateRect("Save Review Button", parent);
            Image image = root.gameObject.AddComponent<Image>();
            image.color = new Color(0.11f, 0.44f, 0.53f);
            image.raycastTarget = true;
            AddLayout(root.gameObject, -1f, 36f);

            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            root.gameObject.AddComponent<UIButtonFeedback>();

            TMP_Text label = CreateText(root, "보드에 저장", 15f, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);
            return button;
        }

        private TMP_Text CreateText(RectTransform parent, string value, float size, FontStyles style, Color color, TextAlignmentOptions alignment)
        {
            RectTransform root = CreateRect("Text", parent);
            TextMeshProUGUI text = root.gameObject.AddComponent<TextMeshProUGUI>();
            if (worryText != null && worryText.font != null)
                text.font = worryText.font;

            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
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

        private void ConfigureStaticButtonLabels()
        {
            SetButtonLabel(didNotHappenButton, "일어나지 않음");
            SetButtonLabel(partiallyHappenedButton, "일부만");
            SetButtonLabel(happenedButton, "일어남");
            SetButtonLabel(closeButton, "닫기");
            SetButtonLabel(reviewConfirmButton, "보드에 저장");
        }

        private static void SetButtonLabel(Button button, string label)
        {
            if (button == null)
                return;

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
                text.text = label;
        }

        private static string CreateWorryText(WorryCard card)
        {
            return $"고민\n{card.WorryText}";
        }

        private static string CreateCopingText(WorryCard card)
        {
            if (card.CopingActions == null || card.CopingActions.Length <= 0)
                return "대처 행동 기록 없음";

            return "만약 일어났다면\n- " + string.Join("\n- ", card.CopingActions);
        }

        private static string CreateSummaryText(WorryCard card)
        {
            string action = string.IsNullOrWhiteSpace(card.ActionPlan) ? "기록 없음" : card.ActionPlan;
            if (string.IsNullOrWhiteSpace(card.Takeaway))
                return $"지금 할 수 있는 행동:\n{action}";

            return $"회고 한 줄:\n{card.Takeaway}\n\n지금 할 수 있는 행동:\n{action}";
        }

        private static string CreateReviewMessage(WorryCard card, WorryOutcomeTag tag)
        {
            switch (tag)
            {
                case WorryOutcomeTag.DidNotHappen:
                    return $"처음에는 {card.ProbabilityPercent}% 정도 일어날 것 같았지만, 실제로는 일어나지 않았어요.\n다음 걱정이 커질 때 이 카드를 근거로 써볼 수 있어요.";
                case WorryOutcomeTag.PartiallyHappened:
                    return $"처음에는 {card.ProbabilityPercent}% 정도로 예상했어요. 실제로는 일부만 일어났네요.\n걱정 전체가 현실이 된 건 아니었다는 점을 남겨둘 수 있어요.";
                case WorryOutcomeTag.Happened:
                    return $"처음에는 {card.ProbabilityPercent}% 정도로 예상했고, 실제로 일어났어요.\n그래도 미리 적어둔 대처 방법이 있었고, 다음에는 대응 가능성을 같이 볼 수 있어요.";
                default:
                    return "실제 결과를 기록했어요. 이번 걱정에서 배운 점을 짧게 남겨볼까요?";
            }
        }

        private static string CreateDefaultTakeaway(WorryCard card, WorryOutcomeTag tag)
        {
            switch (tag)
            {
                case WorryOutcomeTag.DidNotHappen:
                    return card.ProbabilityPercent >= 70 ? "크게 느껴졌지만 실제로는 지나갔다" : "걱정했지만 일어나지 않았다";
                case WorryOutcomeTag.PartiallyHappened:
                    return "걱정 전체가 현실이 된 건 아니었다";
                case WorryOutcomeTag.Happened:
                    return "일어나도 대응할 방법은 있었다";
                default:
                    return string.Empty;
            }
        }

        private static string Trim(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength) + "...";
        }

        private static string CreateDaysAgoText(DateTime createdAt)
        {
            int days = Mathf.Max(0, (DateTime.Now.Date - createdAt.Date).Days);
            return days <= 0 ? "오늘" : $"{days}일 전";
        }
    }
}
