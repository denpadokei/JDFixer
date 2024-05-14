using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System.ComponentModel;
using System.Linq;

namespace JDFixer.UI
{
    internal sealed class PreferencesListViewController : BSMLResourceViewController, INotifyPropertyChanged
    {
        public override string ResourceName => "JDFixer.UI.BSML.preferencesList.bsml";

        [UIComponent("njs_slider")]
        private readonly SliderSetting NJS_Slider;

        [UIValue("njs_value")]
        private float NJS_Value { get; set; } = 16f;
        [UIAction("set_njs_value")]
        private void Set_NJS_Value(float value)
        {
            this.NJS_Value = value;
        }

        [UIValue("min_jd_slider")]
        private float Min_JD_Slider => PluginConfig.Instance.minJumpDistance;
        [UIValue("max_jd_slider")]
        private float Max_JD_Slider => PluginConfig.Instance.maxJumpDistance;

        [UIComponent("jd_slider")]
        private readonly SliderSetting JD_Slider;

        [UIValue("jd_value")]
        private float JD_Value { get; set; } = 18f;
        [UIAction("set_jd_value")]
        private void Set_JD_Value(float value)
        {
            this.JD_Value = value;
        }

        [UIComponent("pref_list")]
        private readonly CustomListTableData Pref_List;
        private JDPref Selected_Pref = null;

        [UIAction("select_pref")]
        private void Select_Pref(TableView tableView, int row)
        {
            this.Selected_Pref = PluginConfig.Instance.preferredValues[row];
        }

        [UIAction("add_pressed")]
        private void Add_Pressed()
        {
            if (PluginConfig.Instance.preferredValues.Any(x => x.njs == this.NJS_Value))
            {
                _ = PluginConfig.Instance.preferredValues.RemoveAll(x => x.njs == this.NJS_Value);
            }
            PluginConfig.Instance.preferredValues.Add(new JDPref(this.NJS_Value, this.JD_Value));
            this.Reload_List_From_Config();
        }

        [UIAction("remove_pressed")]
        private void Remove_Pressed()
        {
            if (this.Selected_Pref == null)
            {
                return;
            }
            _ = PluginConfig.Instance.preferredValues.RemoveAll(x => x == this.Selected_Pref);
            this.Reload_List_From_Config();
        }

        private void Reload_List_From_Config()
        {
            this.Pref_List.data.Clear();

            if (PluginConfig.Instance.preferredValues == null)
            {
                return;
            }

            PluginConfig.Instance.preferredValues.Sort((x, y) => y.njs.CompareTo(x.njs));

            foreach (var pref in PluginConfig.Instance.preferredValues)
            {
                this.Pref_List.data.Add(new CustomListTableData.CustomCellInfo($"{pref.njs} NJS | {pref.jumpDistance} Jump Distance"));
            }

            this.Pref_List.tableView.ReloadData();
            this.Pref_List.tableView.ClearSelection();
            this.Selected_Pref = null;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation)
            {
                this.Reload_List_From_Config();
            }
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            this.Reload_List_From_Config();
        }
    }
}