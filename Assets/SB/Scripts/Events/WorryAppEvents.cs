using Core.EventBus;
using SB.App.Domain;

namespace SB.App.Events
{
    public struct WorryCardCreatedEvent : IEvent
    {
        public WorryCard Card { get; }

        public WorryCardCreatedEvent(WorryCard card)
        {
            Card = card;
        }
    }

    public struct WorryCardSelectedEvent : IEvent
    {
        public WorryCard Card { get; }

        public WorryCardSelectedEvent(WorryCard card)
        {
            Card = card;
        }
    }

    public struct WorryOutcomeTaggedEvent : IEvent
    {
        public WorryCard Card { get; }
        public WorryOutcomeTag OutcomeTag { get; }

        public WorryOutcomeTaggedEvent(WorryCard card, WorryOutcomeTag outcomeTag)
        {
            Card = card;
            OutcomeTag = outcomeTag;
        }
    }
}
