using System;
using SB.App.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class WorryCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text emotionText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private TMP_Text worryText;
        [SerializeField] private TMP_Text actionText;
        [SerializeField] private TMP_Text outcomeText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Outline outcomeOutline;

        private WorryCard _card;
        private bool _isListening;

        public event Action<WorryCard> Selected;

        private void Awake()
        {
            RegisterListeners();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(
            TMP_Text emotion,
            TMP_Text date,
            TMP_Text worry,
            TMP_Text action,
            TMP_Text outcome,
            Button select,
            Image background,
            Outline outline)
        {
            UnregisterListeners();
            emotionText = emotion;
            dateText = date;
            worryText = worry;
            actionText = action;
            outcomeText = outcome;
            selectButton = select;
            backgroundImage = background;
            outcomeOutline = outline;
            RegisterListeners();
        }

        public void Bind(WorryCard card)
        {
            _card = card;

            if (card == null)
                return;

            ApplyCardStyle(card);

            if (emotionText != null)
                emotionText.text = CreateEmotionMark(card.EmotionState);

            if (dateText != null)
                dateText.text = $"{card.CreatedAt.Month}월 {card.CreatedAt.Day}일";

            if (worryText != null)
                worryText.text = Trim(card.WorryText, 34);

            if (actionText != null)
                actionText.text = CreateActionText(string.IsNullOrWhiteSpace(card.Takeaway) ? card.ActionPlan : card.Takeaway);

            if (outcomeText != null)
                outcomeText.text = card.OutcomeTag == WorryOutcomeTag.Untagged ? string.Empty : card.OutcomeTag.ToLabel();
        }

        public void Select()
        {
            if (_card != null)
                Selected?.Invoke(_card);
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (selectButton != null)
                selectButton.onClick.AddListener(Select);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (selectButton != null)
                selectButton.onClick.RemoveListener(Select);

            _isListening = false;
        }

        private void ApplyCardStyle(WorryCard card)
        {
            if (backgroundImage != null)
                backgroundImage.color = CreateBackgroundColor(card);

            if (outcomeOutline != null)
            {
                outcomeOutline.effectDistance = new Vector2(1.5f, -1.5f);
                outcomeOutline.effectColor = CreateOutcomeColor(card.OutcomeTag);
            }

            transform.localEulerAngles = new Vector3(0f, 0f, CreateCardRotation(card.Id));
        }

        private static string CreateEmotionMark(WorryEmotionState state)
        {
            switch (state)
            {
                case WorryEmotionState.StillDistressed:
                    return "😵";
                case WorryEmotionState.SlightlyRelieved:
                    return "🙂";
                case WorryEmotionState.Calm:
                    return "😌";
                default:
                    return "🙂";
            }
        }

        private static string CreateActionText(string actionPlan)
        {
            if (string.IsNullOrWhiteSpace(actionPlan))
                return string.Empty;

            return $"\"{Trim(actionPlan, 24)}\"";
        }

        private static string Trim(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            string trimmed = value.Trim();
            return trimmed.Length <= maxLength ? trimmed : trimmed.Substring(0, maxLength) + "...";
        }

        private static Color CreateBackgroundColor(WorryCard card)
        {
            int paletteIndex = Mathf.Abs(card.Id.GetHashCode()) % 3;
            switch (paletteIndex)
            {
                case 0:
                    return new Color(0.97f, 0.91f, 1f);
                case 1:
                    return new Color(0.91f, 0.95f, 1f);
                default:
                    return new Color(0.96f, 0.93f, 1f);
            }
        }

        private static Color CreateOutcomeColor(WorryOutcomeTag tag)
        {
            switch (tag)
            {
                case WorryOutcomeTag.DidNotHappen:
                    return new Color(0.39f, 0.86f, 0.56f);
                case WorryOutcomeTag.PartiallyHappened:
                    return new Color(0.95f, 0.73f, 0.24f);
                case WorryOutcomeTag.Happened:
                    return new Color(0.86f, 0.34f, 0.39f);
                default:
                    return new Color(0.86f, 0.74f, 1f, 0.55f);
            }
        }

        private static float CreateCardRotation(string id)
        {
            if (string.IsNullOrEmpty(id))
                return 0f;

            int hash = Mathf.Abs(id.GetHashCode());
            return hash % 7 - 3f;
        }
    }
}
