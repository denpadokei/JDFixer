using HMUI;
using Zenject;

namespace JDFixer.UI
{
    internal sealed class PreferencesFlowCoordinator : FlowCoordinator
    {
        internal FlowCoordinator _parentFlow;
        private PreferencesListViewController _prefListView;
        private RTPreferencesListViewController _rtPrefListView;

        /* Since this is binded as a unity component, our "Constructor" is actually a method called Construct (with an inject attribute)
         * We would do the same for ViewControllers if we wanna ask for stuff from Zenject
         */
        [Inject]
        private void Construct(PreferencesListViewController preferencesListViewController, RTPreferencesListViewController rTPreferencesListViewController)
        {
            this._prefListView = preferencesListViewController;
            this._rtPrefListView = rTPreferencesListViewController;
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            this.showBackButton = true;
            this.SetTitle("JDFixer Preferences");

            if (PluginConfig.Instance.use_rt_pref)
            {
                this.ProvideInitialViewControllers(this._rtPrefListView);
            }
            else
            {
                this.ProvideInitialViewControllers(this._prefListView);
            }
        }

        public override void BackButtonWasPressed(ViewController topViewController)
        {
            this._parentFlow?.DismissFlowCoordinator(this);
        }
    }
}
