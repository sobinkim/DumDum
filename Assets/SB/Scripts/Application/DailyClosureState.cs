using System;

namespace SB.App.Application
{
    public readonly struct DailyClosureState
    {
        public DailyClosureState(DateTime closedAt, DateTime nextAvailableAt)
        {
            ClosedAt = closedAt;
            NextAvailableAt = nextAvailableAt;
        }

        public DateTime ClosedAt { get; }
        public DateTime NextAvailableAt { get; }
    }
}
