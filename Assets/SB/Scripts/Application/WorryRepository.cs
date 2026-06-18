using System;
using System.Collections.Generic;
using SB.App.Domain;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class WorryRepository
    {
        private const string StorageKey = "DumDum.WorryCards.Json";
        private readonly List<WorryCard> _cards = new List<WorryCard>();

        public event Action Changed;

        public IReadOnlyList<WorryCard> Cards => _cards;

        public WorryRepository()
        {
            Load();
        }

        public void Add(WorryCard card)
        {
            if (card == null)
                return;

            _cards.Add(card);
            Save();
            Changed?.Invoke();
        }

        public WorryCard Find(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                return null;

            return _cards.Find(card => card.Id == cardId);
        }

        public void TagOutcome(string cardId, WorryOutcomeTag tag)
        {
            WorryCard card = Find(cardId);
            if (card == null)
                return;

            card.TagOutcome(tag);
            Save();
            Changed?.Invoke();
        }

        public void UpdateTakeaway(string cardId, string takeaway)
        {
            WorryCard card = Find(cardId);
            if (card == null)
                return;

            card.UpdateTakeaway(takeaway);
            Save();
            Changed?.Invoke();
        }

        private void Load()
        {
            _cards.Clear();

            string json = PlayerPrefs.GetString(StorageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
                return;

            StoredWorryCardCollection collection = JsonUtility.FromJson<StoredWorryCardCollection>(json);
            if (collection == null || collection.cards == null)
                return;

            for (int i = 0; i < collection.cards.Length; i++)
            {
                WorryCard restored = collection.cards[i].ToDomain();
                if (restored != null)
                    _cards.Add(restored);
            }
        }

        private void Save()
        {
            StoredWorryCard[] storedCards = new StoredWorryCard[_cards.Count];
            for (int i = 0; i < _cards.Count; i++)
                storedCards[i] = StoredWorryCard.FromDomain(_cards[i]);

            string json = JsonUtility.ToJson(new StoredWorryCardCollection { cards = storedCards });
            PlayerPrefs.SetString(StorageKey, json);
            PlayerPrefs.Save();
        }

        [Serializable]
        private sealed class StoredWorryCardCollection
        {
            public StoredWorryCard[] cards;
        }

        [Serializable]
        private sealed class StoredWorryCard
        {
            public string id;
            public long createdAtUtcTicks;
            public string worryText;
            public string factsText;
            public string assumptionsText;
            public string evidenceText;
            public string counterEvidenceText;
            public string alternativeThoughtText;
            public int probabilityPercent;
            public string[] copingActions;
            public string actionPlan;
            public string takeaway;
            public WorryEmotionState emotionState;
            public WorryOutcomeTag outcomeTag;
            public long outcomeTaggedAtUtcTicks;
            public int memoDesignIndex;

            public static StoredWorryCard FromDomain(WorryCard card)
            {
                return new StoredWorryCard
                {
                    id = card.Id,
                    createdAtUtcTicks = card.CreatedAt.ToUniversalTime().Ticks,
                    worryText = card.WorryText,
                    factsText = card.FactsText,
                    assumptionsText = card.AssumptionsText,
                    evidenceText = card.EvidenceText,
                    counterEvidenceText = card.CounterEvidenceText,
                    alternativeThoughtText = card.AlternativeThoughtText,
                    probabilityPercent = card.ProbabilityPercent,
                    copingActions = card.CopingActions,
                    actionPlan = card.ActionPlan,
                    takeaway = card.Takeaway,
                    emotionState = card.EmotionState,
                    outcomeTag = card.OutcomeTag,
                    outcomeTaggedAtUtcTicks = card.OutcomeTaggedAt.HasValue
                        ? card.OutcomeTaggedAt.Value.ToUniversalTime().Ticks
                        : 0,
                    memoDesignIndex = card.MemoDesignIndex
                };
            }

            public WorryCard ToDomain()
            {
                if (string.IsNullOrWhiteSpace(id))
                    return null;

                DateTime createdAt = new DateTime(createdAtUtcTicks, DateTimeKind.Utc).ToLocalTime();
                DateTime? taggedAt = outcomeTaggedAtUtcTicks > 0
                    ? new DateTime(outcomeTaggedAtUtcTicks, DateTimeKind.Utc).ToLocalTime()
                    : null;

                return WorryCard.Restore(
                    id,
                    createdAt,
                    worryText,
                    factsText,
                    assumptionsText,
                    evidenceText,
                    counterEvidenceText,
                    alternativeThoughtText,
                    actionPlan,
                    takeaway,
                    emotionState,
                    outcomeTag,
                    taggedAt,
                    probabilityPercent,
                    copingActions,
                    memoDesignIndex);
            }
        }
    }
}
