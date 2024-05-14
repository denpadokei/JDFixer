using JDFixer.Managers;
using JDFixer.UI;
using Zenject;

namespace JDFixer.Installers
{
    internal sealed class JDFixerMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            _ = this.Container.BindInterfacesTo<JDFixerUIManager>().AsSingle();
            _ = this.Container.BindInterfacesTo<MainMenuUI>().AsSingle();
            _ = this.Container.BindInterfacesTo<CustomOnlineUI>().AsSingle();

            if (PluginConfig.Instance.legacy_display_enabled)
            {
                this.Container.UnbindInterfacesTo<ModifierUI>();
                _ = this.Container.BindInterfacesTo<LegacyModifierUI>().AsSingle();
            }
            else
            {
                this.Container.UnbindInterfacesTo<LegacyModifierUI>();
                _ = this.Container.BindInterfacesTo<ModifierUI>().AsSingle();
            }

            // Flow Coordinators need to binded like this, as a component since it is a Unity Component
            _ = this.Container.Bind<PreferencesFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();

            // Even though ViewControllers are also Unity Components, we bind them with this helper method provided by SiraUtil (FromNewComponentAsViewController)
            _ = this.Container.Bind<PreferencesListViewController>().FromNewComponentAsViewController().AsSingle();
            _ = this.Container.Bind<RTPreferencesListViewController>().FromNewComponentAsViewController().AsSingle();
        }
    }

    internal sealed class JDFixerTimeInstaller : Installer
    {
        public override void InstallBindings()
        {
            //Container.Bind<TimeController>().FromNewComponentOnNewGameObject().AsSingle();
            _ = this.Container.InstantiateComponentOnNewGameObject<TimeController>();
        }
    }
}