namespace SB.App.Application
{
    public interface ISupportInterventionPolicy
    {
        SupportInterventionDecision Choose(SupportInterventionContext context);
    }
}
