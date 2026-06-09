using System;
using SB.App.Domain;

namespace SB.App.Application
{
    [Serializable]
    public sealed class WorryStepContent
    {
        public WorryFlowStep step;
        public string title;
        public string companionMessage;
        public string primaryActionLabel;
        public string secondaryActionLabel;
    }
}
