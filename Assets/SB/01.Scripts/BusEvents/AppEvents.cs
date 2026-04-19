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

public struct ShowFocusPanel : IEvent
{
    
}

public struct HideFocusPanel  : IEvent
{
    
}
public struct WorrySubmitFailEvent : IEvent
{
    
}

public struct ShowSpeechBubbleEvent : IEvent
{
    
}

public struct HideSpeechBubbleEvent : IEvent
{
    
}
public struct ShowThoughtReframePanelEvent : IEvent
{
    
}

public struct RejectionButton_ThoughtReframePanelEvent : IEvent
{
}public struct AcceptanceButton_ThoughtReframePanelEvent : IEvent
{
}



