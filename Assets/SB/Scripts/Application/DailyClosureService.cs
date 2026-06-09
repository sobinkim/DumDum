using System;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class DailyClosureService
    {
        private const string ClosedAtUtcTicksKey = "DumDum.DailyClosure.ClosedAtUtcTicks";
        private const string NextAvailableUtcTicksKey = "DumDum.DailyClosure.NextAvailableUtcTicks";

        public bool IsCreateLocked => TryGetNextAvailableAt(out DateTime nextAvailableAt) && DateTime.Now < nextAvailableAt;

        public bool CanCreateWorry => !IsCreateLocked;

        public DailyClosureState CloseToday()
        {
            DateTime closedAt = DateTime.Now;
            DateTime nextAvailableAt = GetNextNoonAfter(closedAt);

            PlayerPrefs.SetString(ClosedAtUtcTicksKey, closedAt.ToUniversalTime().Ticks.ToString());
            PlayerPrefs.SetString(NextAvailableUtcTicksKey, nextAvailableAt.ToUniversalTime().Ticks.ToString());
            PlayerPrefs.Save();

            return new DailyClosureState(closedAt, nextAvailableAt);
        }

        public bool TryGetNextAvailableAt(out DateTime nextAvailableAt)
        {
            nextAvailableAt = default;
            string rawTicks = PlayerPrefs.GetString(NextAvailableUtcTicksKey, string.Empty);

            if (!long.TryParse(rawTicks, out long ticks) || ticks <= 0)
                return false;

            nextAvailableAt = new DateTime(ticks, DateTimeKind.Utc).ToLocalTime();
            return true;
        }

        public void ClearExpiredLock()
        {
            if (!TryGetNextAvailableAt(out DateTime nextAvailableAt))
                return;

            if (DateTime.Now < nextAvailableAt)
                return;

            PlayerPrefs.DeleteKey(ClosedAtUtcTicksKey);
            PlayerPrefs.DeleteKey(NextAvailableUtcTicksKey);
            PlayerPrefs.Save();
        }

        public void ClearLockForTesting()
        {
            PlayerPrefs.DeleteKey(ClosedAtUtcTicksKey);
            PlayerPrefs.DeleteKey(NextAvailableUtcTicksKey);
            PlayerPrefs.Save();
        }

        private static DateTime GetNextNoonAfter(DateTime source)
        {
            return source.Date.AddDays(1).AddHours(12);
        }
    }
}
