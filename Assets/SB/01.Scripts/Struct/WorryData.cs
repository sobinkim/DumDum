namespace SB._01.Scripts.Interface
{
    public enum EmotionType
    {
        Default,
        Happy,
        Fun,
        Excited,
        Calm,
        Neutral,
        Confused,
        Sad,
        Stressed,
        Angry,
        Disappointed,
        Overwhelmed,
        Sad2
        
    }
    public struct WorryData
    {
        public string content;
        public EmotionType emotionType;
        
    }
}