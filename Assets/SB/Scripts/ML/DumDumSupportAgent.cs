using SB.App.Application;
using SB.App.Domain;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace SB.App.ML
{
    public sealed class DumDumSupportAgent : Agent
    {
        public const string BehaviorName = "DumDumSupportIntervention";
        public const int ObservationSize = 12;
        public const int ActionCount = 4;

        [SerializeField] private DumDumSupportTrainingEnvironment trainingEnvironment;

        private SupportInterventionContext _context;

        public void SetTrainingEnvironment(DumDumSupportTrainingEnvironment environment)
        {
            trainingEnvironment = environment;
        }

        public override void Initialize()
        {
            if (trainingEnvironment == null)
                trainingEnvironment = FindFirstObjectByType<DumDumSupportTrainingEnvironment>();
        }

        public override void OnEpisodeBegin()
        {
            if (trainingEnvironment == null)
                return;

            _context = trainingEnvironment.CreateScenario();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizeCount(_context.TotalCards, DumDumSupportTrainingEnvironment.MaxCardObservationCount));
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizeCount(_context.TaggedCards, DumDumSupportTrainingEnvironment.MaxCardObservationCount));
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizeCount(_context.UntaggedCards, DumDumSupportTrainingEnvironment.MaxCardObservationCount));
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizePercent(_context.DidNotHappenPercent));
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizePercent(_context.HappenedPercent));
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizeCount(_context.TodayCards, DumDumSupportTrainingEnvironment.MaxTodayCardObservationCount));
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizeCount(_context.TakeawayCount, DumDumSupportTrainingEnvironment.MaxTakeawayObservationCount));
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizePercent(_context.LatestProbabilityPercent));
            sensor.AddObservation(DumDumSupportTrainingEnvironment.NormalizeEmotion(_context.LatestEmotion));
            sensor.AddObservation(_context.HasTakeaway ? 1f : 0f);
            sensor.AddObservation(_context.LatestEmotionIsHeavy ? 1f : 0f);
            sensor.AddObservation(_context.HasTaggedEvidence ? 1f : 0f);
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            int actionIndex = actions.DiscreteActions.Length > 0 ? actions.DiscreteActions[0] : 0;
            SupportInterventionType selectedType = ActionToIntervention(actionIndex);

            float reward = trainingEnvironment != null
                ? trainingEnvironment.Evaluate(_context, selectedType)
                : 0f;

            SetReward(reward);
            EndEpisode();
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
            discreteActions[0] = InterventionToAction(ChooseRuleBasedAction(_context));
        }

        public static SupportInterventionType ActionToIntervention(int actionIndex)
        {
            switch (Mathf.Clamp(actionIndex, 0, ActionCount - 1))
            {
                case 1:
                    return SupportInterventionType.TakeawayReminder;
                case 2:
                    return SupportInterventionType.OutcomeStatsReminder;
                case 3:
                    return SupportInterventionType.DailyClosureSuggestion;
                default:
                    return SupportInterventionType.None;
            }
        }

        public static int InterventionToAction(SupportInterventionType type)
        {
            switch (type)
            {
                case SupportInterventionType.TakeawayReminder:
                    return 1;
                case SupportInterventionType.OutcomeStatsReminder:
                    return 2;
                case SupportInterventionType.DailyClosureSuggestion:
                    return 3;
                default:
                    return 0;
            }
        }

        private static SupportInterventionType ChooseRuleBasedAction(SupportInterventionContext context)
        {
            if (context.TodayCards >= 2 && context.LatestEmotionIsHeavy)
                return SupportInterventionType.DailyClosureSuggestion;

            if (context.HasTakeaway && context.TakeawayCount >= 1)
                return SupportInterventionType.TakeawayReminder;

            if (context.TaggedCards >= 3 && context.DidNotHappenPercent >= 50)
                return SupportInterventionType.OutcomeStatsReminder;

            return SupportInterventionType.None;
        }
    }
}
