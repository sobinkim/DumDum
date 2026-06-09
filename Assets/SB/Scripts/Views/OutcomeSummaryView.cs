using System;
using SB.App.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SB.App.Views
{
    public sealed class OutcomeSummaryView : UIScreenView
    {
        [SerializeField] private TMP_Text reviewedCountText;
        [SerializeField] private TMP_Text didNotHappenPercentText;
        [SerializeField] private TMP_Text didNotHappenCountText;
        [SerializeField] private TMP_Text partiallyHappenedPercentText;
        [SerializeField] private TMP_Text partiallyHappenedCountText;
        [SerializeField] private TMP_Text happenedPercentText;
        [SerializeField] private TMP_Text happenedCountText;
        [SerializeField] private Button closeButton;

        private bool _isListening;

        public event Action Closed;

        private void Awake()
        {
            RegisterListeners();
            Hide();
        }

        private void OnDestroy()
        {
            UnregisterListeners();
        }

        public void SetControls(
            TMP_Text reviewedCount,
            TMP_Text didNotHappenPercent,
            TMP_Text didNotHappenCount,
            TMP_Text partiallyHappenedPercent,
            TMP_Text partiallyHappenedCount,
            TMP_Text happenedPercent,
            TMP_Text happenedCount)
        {
            SetControls(
                reviewedCount,
                didNotHappenPercent,
                didNotHappenCount,
                partiallyHappenedPercent,
                partiallyHappenedCount,
                happenedPercent,
                happenedCount,
                null);
        }

        public void SetControls(
            TMP_Text reviewedCount,
            TMP_Text didNotHappenPercent,
            TMP_Text didNotHappenCount,
            TMP_Text partiallyHappenedPercent,
            TMP_Text partiallyHappenedCount,
            TMP_Text happenedPercent,
            TMP_Text happenedCount,
            Button close)
        {
            UnregisterListeners();
            reviewedCountText = reviewedCount;
            didNotHappenPercentText = didNotHappenPercent;
            didNotHappenCountText = didNotHappenCount;
            partiallyHappenedPercentText = partiallyHappenedPercent;
            partiallyHappenedCountText = partiallyHappenedCount;
            happenedPercentText = happenedPercent;
            happenedCountText = happenedCount;
            closeButton = close;
            RegisterListeners();
        }

        public void Render(WorryOutcomeSummary summary)
        {
            summary = summary ?? WorryOutcomeSummary.Empty;

            if (reviewedCountText != null)
                reviewedCountText.text = CreateReviewedText(summary);

            SetMetric(didNotHappenPercentText, didNotHappenCountText, summary.DidNotHappenPercent, summary.DidNotHappenCount);
            SetMetric(partiallyHappenedPercentText, partiallyHappenedCountText, summary.PartiallyHappenedPercent, summary.PartiallyHappenedCount);
            SetMetric(happenedPercentText, happenedCountText, summary.HappenedPercent, summary.HappenedCount);
        }

        private static void SetMetric(TMP_Text percentText, TMP_Text countText, int percent, int count)
        {
            if (percentText != null)
                percentText.text = $"{percent}%";

            if (countText != null)
                countText.text = $"{count}개";
        }

        private static string CreateReviewedText(WorryOutcomeSummary summary)
        {
            if (summary.TotalCount <= 0)
                return "아직 고민 카드가 없어요.";

            if (summary.TaggedCount <= 0)
                return $"결과 기록 전 · 확인 대기 {summary.UntaggedCount}개";

            if (summary.UntaggedCount <= 0)
                return $"결과 기록 {summary.TaggedCount}개 기준";

            return $"결과 기록 {summary.TaggedCount}개 기준 · 확인 대기 {summary.UntaggedCount}개";
        }

        public void Close()
        {
            Hide();
            Closed?.Invoke();
        }

        private void RegisterListeners()
        {
            if (_isListening)
                return;

            if (closeButton != null)
                closeButton.onClick.AddListener(Close);

            _isListening = true;
        }

        private void UnregisterListeners()
        {
            if (!_isListening)
                return;

            if (closeButton != null)
                closeButton.onClick.RemoveListener(Close);

            _isListening = false;
        }
    }
}
