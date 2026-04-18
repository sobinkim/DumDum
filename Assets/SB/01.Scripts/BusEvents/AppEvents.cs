using Core.EventBus;
using SB._01.Scripts.Interface;
using UnityEngine;

public struct WorryEnterEvent : IEvent
{
}

public struct WorrySubmittedEvent : IEvent
{
    public WorryData worryData;

    public WorrySubmittedEvent(WorryData inputWorryData)
    {
        worryData = inputWorryData;
    }
}

public struct WorrySubmitFailEvent : IEvent
{
    
}

