using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System.ComponentModel;
using System.Linq;

namespace JDFixer.UI
{
    internal sealed class RTPreferencesListViewController : BSMLResourceViewController, INotifyPropertyChanged
    {
        public override string ResourceName => "JDFixer.UI.BSML.rtPreferencesList.bsml";

        [UIComponent("njs_slider")]
        private readonly SliderSetting NJS_Slider;

        [UIValue("njs_value")]
        private float NJS_Value { get; set; } = 16f;
        [UIAction("set_njs_value")]
        private void Set_NJS_Value(float value)
        {
            this.NJS_Value = value;
        }

        [UIValue("min_rt_slider")]
        private float Min_RT_Slider => PluginConfig.Instance.minReactionTime;
        [UIValue("max_rt_slider")]
        private float Max_RT_Slider => PluginConfig.Instance.maxReactionTime;

        [UIComponent("rt_slider")]
        private readonly SliderSetting RT_Slider;

        [UIValue("rt_value")]
        private float RT_Value { get; set; } = 500f;
        [UIAction("set_rt_value")]
        private void Set_RT_Value(float value)
        {
            this.RT_Value = value;
        }

        [UIAction("rt_slider_formatter")]
        private string RT_Slider_Formatter(float value)
        {
            return value.ToString("0") + " ms";
        }

        [UIComponent("pref_list")]
        private readonly CustomListTableData Pref_List;
        private RTPref Selected_Pref = null;

        [UIAction("select_pref")]
        private void Select_Pref(TableView tableView, int row)
        {
            this.Selected_Pref = PluginConfig.Instance.rt_preferredValues[row];
        }

        [UIAction("add_pressed")]
        private void Add_Pressed()
        {
            if (PluginConfig.Instance.rt_preferredValues.Any(x => x.njs == this.NJS_Value))
            {
                _ = PluginConfig.Instance.rt_preferredValues.RemoveAll(x => x.njs == this.NJS_Value);
            }
            PluginConfig.Instance.rt_preferredValues.Add(new RTPref(this.NJS_Value, this.RT_Value));
            this.Reload_List_From_Config();
        }

        [UIAction("remove_pressed")]
        private void Remove_Pressed()
        {
            if (this.Selected_Pref == null)
            {
                return;
            }

            _ = PluginConfig.Instance.rt_preferredValues.RemoveAll(x => x == this.Selected_Pref);
            this.Reload_List_From_Config();
        }

        private void Reload_List_From_Config()
        {
            this.Pref_List.data.Clear();

            if (PluginConfig.Instance.rt_preferredValues == null)
            {
                return;
            }

            PluginConfig.Instance.rt_preferredValues.Sort((x, y) => y.njs.CompareTo(x.njs));

            foreach (var pref in PluginConfig.Instance.rt_preferredValues)
            {
                this.Pref_List.data.Add(new CustomListTableData.CustomCellInfo($"{pref.njs} NJS | {pref.reactionTime} ms"));
            }

            this.Pref_List.tableView.ReloadData();
            this.Pref_List.tableView.ClearSelection();
            this.Selected_Pref = null;
        }

        //----------------------------------------------------------------------------

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
