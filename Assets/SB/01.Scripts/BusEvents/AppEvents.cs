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

public struct HideFocusPanel : IEvent
{
}

public struct SubmitFailEvent : IEvent // 타입 이넘 받아서 원하는 내용 뿌리자 여러개 만들지 말고
{
}

public struct ShowSpeechBubbleEvent : IEvent
{
}

public struct HideSpeechBubbleEvent : IEvent
{
}

public struct MainPanelButtonEnable : IEvent
{
    public bool enable;

    public MainPanelButtonEnable(bool inputEnable)
    {
        this.enable = inputEnable;
    }
}

public struct ShowThoughtReframePanelEvent : IEvent
{
    public WorryData worryData;

    public ShowThoughtReframePanelEvent(WorryData inputWorryData)
    {
        worryData = inputWorryData;
    }
}

public struct RejectionButton_ThoughtReframePanelEvent : IEvent
{
}

public struct AcceptanceButton_ThoughtReframePanelEvent : IEvent
{
}

public struct RequestCreateCardEvent : IEvent
{
    public CardData cardData;

    public RequestCreateCardEvent(CardData inputCardData)
    {
        cardData = inputCardData;
    }
}

public struct ShowCardPanel : IEvent
{
    public CardData cardData;

    public ShowCardPanel(CardData inputCardData)
    {
        cardData = inputCardData;
    }
}