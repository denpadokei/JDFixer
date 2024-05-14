using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using JDFixer.Interfaces;
using System;
using System.ComponentModel;
using Zenject;

namespace JDFixer.UI
{
    internal sealed class LegacyModifierUI : IInitializable, IDisposable, INotifyPropertyChanged, IBeatmapInfoUpdater
    {
        internal static LegacyModifierUI Instance { get; set; }
        private readonly MainFlowCoordinator _mainFlow;
        private readonly PreferencesFlowCoordinator _prefFlow;

        public event PropertyChangedEventHandler PropertyChanged;
        private BeatmapInfo _selectedBeatmap = BeatmapInfo.Empty;

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("JDFixer", "JDFixer.UI.BSML.legacyModifierUI.bsml", this, MenuType.Solo | MenuType.Campaign);
            Donate.Refresh_Text();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Donate_Update_Dynamic)));
        }

        public void Dispose()
        {
            if (GameplaySetup.instance != null)
            {
                PluginConfig.Instance.Changed();
                GameplaySetup.instance.RemoveTab("JDFixer");
            }
        }

        // To get the flow coordinators using zenject, we use a constructor
        private LegacyModifierUI(MainFlowCoordinator mainFlowCoordinator, PreferencesFlowCoordinator preferencesFlowCoordinator)
        {
            Instance = this;
            this._mainFlow = mainFlowCoordinator;
            this._prefFlow = preferencesFlowCoordinator;
            Donate.Refresh_Text();
        }

        public void BeatmapInfoUpdated(BeatmapInfo beatmapInfo)
        {
            this._selectedBeatmap = beatmapInfo;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Map_Default_JD)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Map_Min_JD)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Display)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Display)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_JD_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_RT_Slider)));

            if (PluginConfig.Instance.use_offset)
            {
                //Plugin.Log.Debug("Map JD: " + _selectedBeatmap.JumpDistance + " " + _selectedBeatmap.MinJDSlider + " " + _selectedBeatmap.MaxJDSlider);
                //Plugin.Log.Debug("Map RT: " + _selectedBeatmap.ReactionTime + " " + _selectedBeatmap.MinRTSlider + " " + _selectedBeatmap.MaxRTSlider);

                BeatmapOffsets.Create_Snap_Points(ref BeatmapOffsets.JD_Snap_Points, ref BeatmapOffsets.JD_Offset_Points, this._selectedBeatmap.Offset, this._selectedBeatmap.JumpDistance, this._selectedBeatmap.JDOffsetQuantum, this._selectedBeatmap.MinJDSlider, this._selectedBeatmap.MaxJDSlider);
                BeatmapOffsets.Create_Snap_Points(ref BeatmapOffsets.RT_Snap_Points, ref BeatmapOffsets.RT_Offset_Points, this._selectedBeatmap.Offset, this._selectedBeatmap.ReactionTime, this._selectedBeatmap.RTOffsetQuantum, this._selectedBeatmap.MinRTSlider, this._selectedBeatmap.MaxRTSlider);

                this.Refresh_BeatmapOffsets();
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_JD_Display)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_RT_Display)));

            this.PostParse();
        }

        internal void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Slider_Setting_Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Increment_Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Pref_Button)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Heuristic_Increment_Value)));

            if (PluginConfig.Instance.use_offset)
            {
                this.Refresh_BeatmapOffsets();
            }
        }

        internal void Refresh_BeatmapOffsets()
        {
            Plugin.Log.Debug("Refresh_BeatmapOffsets");

            BeatmapOffsets.Calculate_Nearest_JD_Snap_Point(this.JD_Value);
            BeatmapOffsets.Calculate_Nearest_RT_Snap_Point(this.RT_Value);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Snapped_JD)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Snapped_RT)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_Snapped_JD)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_Snapped_RT)));
        }

        //=============================================================================================

        [UIValue("enabled")]
        private bool Enabled
        {
            get => PluginConfig.Instance.enabled;
            set => PluginConfig.Instance.enabled = value;
        }
        [UIAction("set_enabled")]
        private void SetEnabled(bool value)
        {
            this.Enabled = value;
        }

        [UIValue("map_jd_rt")]
        private string Map_JD_RT => this.Get_Map_JD_RT();
        private string Get_Map_JD_RT()
        {
            return PluginConfig.Instance.rt_display_enabled ? "Map JD and RT" : "Map JD";
        }

        [UIValue("map_default_jd")]
        private string Map_Default_JD => this.Get_Map_Default_JD();
        private string Get_Map_Default_JD()
        {
            return PluginConfig.Instance.rt_display_enabled
                ? "<#ffff00>" + this._selectedBeatmap.JumpDistance.ToString("0.##") + "     <#8c1aff>" + this._selectedBeatmap.ReactionTime.ToString("0") + " ms"
                : "<#ffff00>" + this._selectedBeatmap.JumpDistance.ToString("0.##");
        }

        [UIValue("map_min_jd")]
        private string Map_Min_JD => this.Get_Map_Min_JD();
        private string Get_Map_Min_JD()
        {
            return PluginConfig.Instance.rt_display_enabled
                ? "<#8c8c8c>" + this._selectedBeatmap.MinJumpDistance.ToString("0.##") + "     <#8c8c8c>" + this._selectedBeatmap.MinReactionTime.ToString("0") + " ms"
                : "<#8c8c8c>" + this._selectedBeatmap.MinJumpDistance.ToString("0.##");
        }

        [UIValue("snapped_jd")]
        private string Snapped_JD => this.Get_Snapped_JD();
        private string Get_Snapped_JD()
        {
            BeatmapOffsets.Calculate_Nearest_JD_Snap_Point(this.JD_Value);
            return "<#8c8c8c>" + BeatmapOffsets.jd_offset_snap_value + "     <#ffff00>" + BeatmapOffsets.jd_snap_value.ToString("0.##") + "     " + BeatmapUtils.Calculate_ReactionTime_Setpoint_String(BeatmapOffsets.jd_snap_value, this._selectedBeatmap.NJS);
        }
        [UIValue("show_snapped_jd")]
        private bool Show_Snapped_JD => PluginConfig.Instance.use_offset && this.Show_JD_Slider;

        [UIValue("snapped_rt")]
        private string Snapped_RT => this.Get_Snapped_RT();
        private string Get_Snapped_RT()
        {
            BeatmapOffsets.Calculate_Nearest_RT_Snap_Point(this.RT_Value);
            return "<#8c8c8c>" + BeatmapOffsets.rt_offset_snap_value + "     " + BeatmapUtils.Calculate_JumpDistance_Setpoint_String(BeatmapOffsets.rt_snap_value, this._selectedBeatmap.NJS) + "     <#cc99ff>" + BeatmapOffsets.rt_snap_value.ToString("0") + " ms";
        }
        [UIValue("show_snapped_rt")]
        private bool Show_Snapped_RT => PluginConfig.Instance.use_offset && this.Show_RT_Slider;

        //=============================================================================================

        [UIValue("min_jd_slider")]
        private float Min_JD_Slider => PluginConfig.Instance.minJumpDistance;
        [UIValue("max_jd_slider")]
        private float Max_JD_Slider => PluginConfig.Instance.maxJumpDistance;

        [UIComponent("jd_slider")]
        private readonly SliderSetting JD_Slider;

        [UIValue("jd_value")]
        private float JD_Value
        {
            get => PluginConfig.Instance.jumpDistance;
            set
            {
                PluginConfig.Instance.jumpDistance = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Display)));

                if (PluginConfig.Instance.use_offset)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Snapped_JD)));
                }
            }
        }
        [UIAction("set_jd_value")]
        private void Set_JD_Value(float value)
        {
            this.JD_Value = value;
        }
        [UIAction("jd_slider_formatter")]
        private string JD_Slider_Formatter(float value)
        {
            return value.ToString("0.##");
        }

        [UIValue("jd_display")]
        private string JD_Display => BeatmapUtils.Calculate_JumpDistance_Setpoint_String(this.RT_Value, this._selectedBeatmap.NJS); //"<#ffff00>" + (PluginConfig.Instance.reactionTime * (2 * _selectedBeatmap.NJS) / 1000).ToString("0.##");
        [UIValue("show_jd_display")]
        private bool Show_JD_Display => PluginConfig.Instance.use_offset == false && this.Show_RT_Slider;

        [UIValue("min_rt_slider")]
        private float Min_RT_Slider => PluginConfig.Instance.minReactionTime;

        [UIValue("max_rt_slider")]
        private float Max_RT_Slider => PluginConfig.Instance.maxReactionTime;

        [UIComponent("rt_slider")]
        private readonly SliderSetting RT_Slider;

        [UIValue("rt_value")]
        private float RT_Value
        {
            get => PluginConfig.Instance.reactionTime;
            set
            {
                PluginConfig.Instance.reactionTime = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Display)));

                if (PluginConfig.Instance.use_offset)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Snapped_RT)));
                }
            }
        }
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

        [UIValue("rt_display")]
        private string RT_Display => BeatmapUtils.Calculate_ReactionTime_Setpoint_String(this.JD_Value, this._selectedBeatmap.NJS);
        [UIValue("show_rt_display")]
        private bool Show_RT_Display => PluginConfig.Instance.use_offset == false && this.Show_JD_Slider;

        //##############################################
        // New for BS 1.19.0

        [UIValue("increment_value")]
        private int Increment_Value
        {
            get => PluginConfig.Instance.pref_selected;
            set
            {
                PluginConfig.Instance.pref_selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Increment_Value)));

                this.Set_Preference_Mode();
            }
        }

        [UIAction("increment_formatter")]
        private string Increment_Formatter(int value)
        {
            return ((PreferenceEnum)value).ToString();
        }

        private void Set_Preference_Mode()
        {
            if (PluginConfig.Instance.pref_selected == 2)
            {
                PluginConfig.Instance.use_jd_pref = false;
                PluginConfig.Instance.use_rt_pref = true;
            }
            else if (PluginConfig.Instance.pref_selected == 1)
            {
                PluginConfig.Instance.use_jd_pref = true;
                PluginConfig.Instance.use_rt_pref = false;
            }
            else
            {
                PluginConfig.Instance.use_jd_pref = false;
                PluginConfig.Instance.use_rt_pref = false;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Pref_Button)));
        }
        //##############################################

        [UIValue("pref_button")]
        private string Pref_Button => this.Get_Pref_Button();

        private string Get_Pref_Button()
        {
            if (PluginConfig.Instance.pref_selected == 2)
            {
                return "<#00000000>----<#cc99ff>Configure  RT  Preferences<#00000000>----"; //#8c1aff
            }
            else
            {
                return PluginConfig.Instance.pref_selected == 1
                    ? "<#00000000>----<#ffff00>Configure  JD  Preferences<#00000000>----"
                    : "Configure  JD  and  RT  Preferences";
            }
        }

        [UIAction("pref_button_clicked")]
        private void Pref_Button_Clicked()
        {
            /* Kyle used to have a helper function which you also used (DeepestChildFlowCoordinator). 
             * Beat Games has added this to the game since, so we can just use something they helpfully provided us
             */
            var currentFlow = this._mainFlow.YoungestChildFlowCoordinatorOrSelf();
            // We need to give our current flow coordinator to the pref flow so it can exit
            this._prefFlow._parentFlow = currentFlow;
            currentFlow.PresentFlowCoordinator(this._prefFlow);
        }

        // Changed to Increment Setting for 1.26.0
        /*[UIValue("use_heuristic")]
        private bool Use_Heuristic
        {
            get => PluginConfig.Instance.use_heuristic;
            set
            {
                PluginConfig.Instance.use_heuristic = value;
            }
        }

        [UIAction("set_use_heuristic")]
        private void Set_Use_Heuristic(bool value)
        {
            Use_Heuristic = value;
        }*/

        [UIValue("heuristic_increment_value")]
        private int Heuristic_Increment_Value
        {
            get => PluginConfig.Instance.use_heuristic;
            set
            {
                PluginConfig.Instance.use_heuristic = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Heuristic_Increment_Value)));

                this.PostParse();
            }
        }

        [UIAction("heuristic_increment_formatter")]
        private string Heuristic_Increment_Formatter(int value)
        {
            return ((HeuristicEnum)value).ToString();
        }

        [UIValue("thresholds")]
        private string Thresholds => "≤ " + PluginConfig.Instance.lower_threshold.ToString() + " or  ≥ " + PluginConfig.Instance.upper_threshold.ToString();

        //###################################
        // KEEP: In case
        /*[UIValue("lowerthreshold")]
        public string lowerthreshold
        {
            get => PluginConfig.Instance.lower_threshold.ToString();
        }

        // Thresholds Display
        [UIValue("upperthreshold")]
        public string upperthreshold
        {
            get => PluginConfig.Instance.upper_threshold.ToString();
        }*/
        //###################################

        private CurvedTextMeshPro jd_slider_text;
        private CurvedTextMeshPro rt_slider_text;

        [UIAction("#post-parse")]
        private void PostParse()
        {
            if (this.JD_Slider == null || this.RT_Slider == null)
            {
                return;
            }

            this.jd_slider_text = this.JD_Slider.slider.GetComponentInChildren<CurvedTextMeshPro>();
            if (this.jd_slider_text != null)
            {
                this.jd_slider_text.color = new UnityEngine.Color(1f, 1f, 0f);
            }

            this.rt_slider_text = this.RT_Slider.slider.GetComponentInChildren<CurvedTextMeshPro>();
            if (this.rt_slider_text != null)
            {
                this.rt_slider_text.color = new UnityEngine.Color(204f / 255f, 153f / 255f, 1f);
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Value)));
        }

        //1.20.0 Feature update
        [UIValue("slider_setting_value")]
        private int Slider_Setting_Value
        {
            get => PluginConfig.Instance.slider_setting;
            set
            {
                PluginConfig.Instance.slider_setting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Slider_Setting_Value)));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Value)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Value)));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Display)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Display)));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_JD_Slider)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_RT_Slider)));

                // 1.26.0-1.29.0 Feature update
                if (PluginConfig.Instance.use_offset)
                {
                    this.Refresh_BeatmapOffsets();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_JD_Display)));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Show_RT_Display)));
            }
        }
        [UIAction("slider_setting_increment_formatter")]
        private string Slider_Setting_Increment_Formatter(int value)
        {
            return ((SliderSettingEnum)value).ToString();
        }

        [UIValue("show_jd_slider")]
        private bool Show_JD_Slider => this.Get_JD_Slider();

        private bool Get_JD_Slider()
        {
            return PluginConfig.Instance.slider_setting == 0;
        }

        [UIValue("show_rt_slider")]
        private bool Show_RT_Slider => this.Get_RT_Slider();

        private bool Get_RT_Slider()
        {
            return PluginConfig.Instance.slider_setting == 1;
        }

        //===============================================================

        [UIValue("open_donate_text")]
        private string Open_Donate_Text => Donate.donate_clickable_text;

        [UIValue("open_donate_hint")]
        private string Open_Donate_Hint => Donate.donate_clickable_hint;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        [UIAction("open_donate_modal")]
        private void Open_Donate_Modal()
        {
            this.parserParams.EmitEvent("hide_donate_modal");
            Donate.Refresh_Text();
            this.parserParams.EmitEvent("show_donate_modal");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Donate_Modal_Text_Dynamic)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Donate_Modal_Hint_Dynamic)));
        }

        private void Open_Donate_Patreon()
        {
            Donate.Patreon();
        }
        private void Open_Donate_Kofi()
        {
            Donate.Kofi();
        }

        [UIValue("donate_modal_text_static_1")]
        private string Donate_Modal_Text_Static_1 => Donate.donate_modal_text_static_1;

        [UIValue("donate_modal_text_static_2")]
        private string Donate_Modal_Text_Static_2 => Donate.donate_modal_text_static_2;

        [UIValue("donate_modal_text_dynamic")]
        private string Donate_Modal_Text_Dynamic => Donate.donate_modal_text_dynamic;

        [UIValue("donate_modal_hint_dynamic")]
        private string Donate_Modal_Hint_Dynamic => Donate.donate_modal_hint_dynamic;

        [UIValue("donate_update_dynamic")]
        private string Donate_Update_Dynamic => Donate.donate_update_dynamic;
    }
}