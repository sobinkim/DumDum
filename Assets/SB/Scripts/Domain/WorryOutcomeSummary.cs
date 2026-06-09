using System;
using System.Collections.Generic;

namespace SB.App.Domain
{
    public sealed class WorryOutcomeSummary
    {
        public static readonly WorryOutcomeSummary Empty = new WorryOutcomeSummary(0, 0, 0, 0, 0);

        public int TotalCount { get; }
        public int UntaggedCount { get; }
        public int DidNotHappenCount { get; }
        public int PartiallyHappenedCount { get; }
        public int HappenedCount { get; }

        public int TaggedCount => DidNotHappenCount + PartiallyHappenedCount + HappenedCount;
        public int DidNotHappenPercent => CalculatePercent(DidNotHappenCount);
        public int PartiallyHappenedPercent => CalculatePercent(PartiallyHappenedCount);
        public int HappenedPercent => CalculatePercent(HappenedCount);

        private WorryOutcomeSummary(
            int totalCount,
            int untaggedCount,
            int didNotHappenCount,
            int partiallyHappenedCount,
            int happenedCount)
        {
            TotalCount = totalCount;
            UntaggedCount = untaggedCount;
            DidNotHappenCount = didNotHappenCount;
            PartiallyHappenedCount = partiallyHappenedCount;
            HappenedCount = happenedCount;
        }

        public static WorryOutcomeSummary FromCards(IReadOnlyList<WorryCard> cards)
        {
            if (cards == null || cards.Count == 0)
                return Empty;

            int untagged = 0;
            int didNotHappen = 0;
            int partiallyHappened = 0;
            int happened = 0;

            for (int i = 0; i < cards.Count; i++)
            {
                WorryCard card = cards[i];
                if (card == null)
                    continue;

                switch (card.OutcomeTag)
                {
                    case WorryOutcomeTag.DidNotHappen:
                        didNotHappen++;
                        break;
                    case WorryOutcomeTag.PartiallyHappened:
                        partiallyHappened++;
                        break;
                    case WorryOutcomeTag.Happened:
                        happened++;
                        break;
                    default:
                        untagged++;
                        break;
                }
            }

            return new WorryOutcomeSummary(cards.Count, untagged, didNotHappen, partiallyHappened, happened);
        }

        private int CalculatePercent(int count)
        {
            if (TaggedCount <= 0)
                return 0;

            double ratio = (double)count / TaggedCount * 100d;
            return (int)Math.Round(ratio, MidpointRounding.AwayFromZero);
        }
    }
}
