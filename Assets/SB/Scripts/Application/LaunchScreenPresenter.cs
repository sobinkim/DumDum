using SB.App.Views;
using UnityEngine;

namespace SB.App.Application
{
    public sealed class LaunchScreenPresenter : MonoBehaviour
    {
        [SerializeField] private LaunchScreenView launchScreenView;
        [SerializeField] private string mainSceneName = AppSceneNavigator.MainSceneName;

        private void Awake()
        {
            SoundManager.GetOrCreate();
            AppSceneNavigator.GetOrCreate();

            if (launchScreenView != null)
                launchScreenView.ContinueRequested += ContinueToMain;
        }

        private void OnDestroy()
        {
            if (launchScreenView != null)
                launchScreenView.ContinueRequested -= ContinueToMain;
        }

        public void SetComposition(LaunchScreenView view, string targetSceneName)
        {
            if (launchScreenView != null)
                launchScreenView.ContinueRequested -= ContinueToMain;

            launchScreenView = view;
            mainSceneName = string.IsNullOrWhiteSpace(targetSceneName)
                ? AppSceneNavigator.MainSceneName
                : targetSceneName;

            if (launchScreenView != null)
                launchScreenView.ContinueRequested += ContinueToMain;
        }

        private void ContinueToMain()
        {
            AppSceneNavigator.GetOrCreate().LoadScene(mainSceneName);
        }
    }
}
