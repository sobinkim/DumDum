using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace SB.App.Editor
{
    public static class DumDumSupportTrainingPlayerBuilder
    {
        private const string TrainingScenePath = "Assets/SB/ML/TrainingScenes/DumDumSupportTraining.unity";
        private const string OutputPath = "Builds/MLAgents/DumDumSupportTraining/DumDumSupportTraining.exe";

        [MenuItem("DumDum/ML/Build Support Training Player")]
        public static void BuildTrainingPlayer()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { TrainingScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new System.Exception($"Support training player build failed: {report.summary.result}");
        }
    }
}
