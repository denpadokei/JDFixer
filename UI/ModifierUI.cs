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
    internal sealed class ModifierUI : IInitializable, IDisposable, INotifyPropertyChanged, IBeatmapInfoUpdater
    {
        internal static ModifierUI Instance { get; set; }
        private readonly MainFlowCoordinator _mainFlow;
        private readonly PreferencesFlowCoordinator _prefFlow;

        public event PropertyChangedEventHandler PropertyChanged;
        private BeatmapInfo _selectedBeatmap = BeatmapInfo.Empty;

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("JDFixer", "JDFixer.UI.BSML.modifierUI.bsml", this, MenuType.Solo | MenuType.Campaign);
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
        private ModifierUI(MainFlowCoordinator mainFlowCoordinator, PreferencesFlowCoordinator preferencesFlowCoordinator)
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
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReactionTimeText))); // For old RT Display

            this.PostParse();
        }

        internal void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Slider_Setting_Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Increment_Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Pref_Button)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Heuristic_Increment_Value)));
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
                ? "<#8c8c8c>" + this._selectedBeatmap.MinJumpDistance.ToString("0.##") + "     <#8c8c8c>" + this._selectedBeatmap.MinReactionTime.ToString("0" + " ms")
                : "<#8c8c8c>" + this._selectedBeatmap.MinJumpDistance.ToString("0.##");
        }

        [UIValue("min_jd_slider")]
        private float Min_JD_Slider => this._selectedBeatmap.MinJDSlider; //PluginConfig.Instance.minJumpDistance;
        [UIValue("max_jd_slider")]
        private float Max_JD_Slider => this._selectedBeatmap.MaxJDSlider; //PluginConfig.Instance.maxJumpDistance;

        [UIComponent("jd_slider")]
        private readonly SliderSetting JD_Slider;

        [UIValue("jd_value")]
        private float JD_Value
        {
            get => this.Get_Jump_Distance(); //PluginConfig.Instance.jumpDistance; //GetJumpDistance();
            set
            {
                /*if (PluginConfig.Instance.use_offset)
                {
                    PluginConfig.Instance.jumpDistance = BeatmapUtils.Calculate_ReactionTime_Nearest_Offset(value);
                }

                else*/
                if (PluginConfig.Instance.slider_setting == 0)
                {
                    PluginConfig.Instance.jumpDistance = value;
                }
                else
                {
                    if (this._selectedBeatmap.NJS > 0.002)
                    {
                        PluginConfig.Instance.reactionTime = value / (2 * this._selectedBeatmap.NJS) * 1000;
                    }
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Value)));
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReactionTimeText))); // For old RT Display                
            }
        }

        /*private float GetJumpDistance()
        {
            return PluginConfig.Instance.jumpDistance;
        }*/

        // 1.19.1
        private float Get_Jump_Distance()
        {
            return PluginConfig.Instance.slider_setting == 0
                ? PluginConfig.Instance.jumpDistance
                : PluginConfig.Instance.reactionTime * (2 * this._selectedBeatmap.NJS) / 1000;
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

        [UIValue("min_rt_slider")]
        private float Min_RT_Slider => this._selectedBeatmap.MinRTSlider; //Get_Min_RT();

        [UIValue("max_rt_slider")]
        private float Max_RT_Slider => this._selectedBeatmap.MaxRTSlider; //Get_Max_RT();

        /*public float Get_Min_RT()
        {
            return _selectedBeatmap.MinRTSlider;
        }
        public float Get_Max_RT()
        {
            return _selectedBeatmap.MaxRTSlider;
        }*/

        //=============================================================
        // Old Reaction Time Display: Replaced by RT Slider (KEEP THIS)

        //[UIValue("reactionTime")]
        //public string ReactionTimeText => CalculateReactionTime();

        //<horizontal>
        //  <grid cell-size-y='5' cell-size-x='28' spacing-x='2' align='Right'>
        //	  <text text='Reaction Time' align='Left'/>
        //	  <text text='----------------' align='Left' rich-text='true' font-color='#00000000'/>
        //	  <text text='~reactionTime' min-width='29' align='Right'/>
        //  </grid>
        //</horizontal>

        //=============================================================

        [UIComponent("rt_slider")]
        private readonly SliderSetting RT_Slider;

        [UIValue("rt_value")]
        private float RT_Value
        {
            get => this.Get_Reaction_Time(); //CalculateReactionTime_Float(PluginConfig.Instance.jumpDistance);
            set
            {
                if (PluginConfig.Instance.slider_setting == 0) // Fixed JD
                {
                    if (this._selectedBeatmap.NJS > 0.002)
                    {
                        PluginConfig.Instance.jumpDistance = value / 1000 * (2 * this._selectedBeatmap.NJS);
                    }
                }
                else
                {
                    PluginConfig.Instance.reactionTime = value;
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Value)));
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReactionTimeText))); // For validation               
            }
        }

        // 1.19.1
        private float Get_Reaction_Time()
        {
            return PluginConfig.Instance.slider_setting == 0
                ? BeatmapUtils.Calculate_ReactionTime_Setpoint_Float(PluginConfig.Instance.jumpDistance, this._selectedBeatmap.NJS)
                : PluginConfig.Instance.reactionTime;
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

        //=============================================================
        // Old JD Preferences and RT Preferences Toggles: Replaced with Increment Setting

        //<checkbox-setting value = 'usePrefJumpValues' on-change='setUsePrefJumpValues' text='Use JD Preferences'></checkbox-setting>
        //<checkbox-setting value = 'rtEnabled' on-change='setRTEnabled' text='Use RT Preferences' hover-hint='Overrides JD Preferences if enabled'></checkbox-setting>

        /*[UIValue("usePrefJumpValues")]
        public bool usePrefJumpValues
        {
            get => PluginConfig.Instance.usePreferredJumpDistanceValues;
            set
            {
                PluginConfig.Instance.usePreferredJumpDistanceValues = value;
            }
        }
        [UIAction("setUsePrefJumpValues")]
        public void SetUsePrefJumpValues(bool value)
        {
            usePrefJumpValues = value;

            //if (value)
            //{
            //    PluginConfig.Instance.rt_enabled = false;
            //    NotifyPropertyChanged(nameof(RTEnabled));
            //}
        }


        // Reaction Time Mode
        [UIValue("rtEnabled")]
        public bool RTEnabled
        {
            get => PluginConfig.Instance.rt_enabled;
            set
            {
                PluginConfig.Instance.rt_enabled = value;
            }
        }
        [UIAction("setRTEnabled")]
        public void SetRTEnabled(bool value)
        {
            RTEnabled = value;

            //if (value)
            //{
            //    PluginConfig.Instance.usePreferredJumpDistanceValues = false;
            //    NotifyPropertyChanged(nameof(usePrefJumpValues));
            //}
        }*/
        //=============================================================

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
        // <checkbox-setting value='use_heuristic' on-change='set_use_heuristic' text='Play at Map JD and RT If Lower' hover-hint='If original JD and RT is lower than the matching preference, map will run at original JD and RT. You MUST set base game to Dynamic Default for this to work properly!'></checkbox-setting>

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

        private HMUI.CustomFormatRangeValuesSlider rt_slider_range;
        private HMUI.CustomFormatRangeValuesSlider jd_slider_range;

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

            this.rt_slider_range = this.RT_Slider.slider.GetComponentInChildren<HMUI.CustomFormatRangeValuesSlider>();
            this.rt_slider_range.minValue = this._selectedBeatmap.MinRTSlider;
            this.rt_slider_range.maxValue = this._selectedBeatmap.MaxRTSlider;

            this.jd_slider_range = this.JD_Slider.slider.GetComponentInChildren<HMUI.CustomFormatRangeValuesSlider>();
            this.jd_slider_range.minValue = this._selectedBeatmap.MinJDSlider;
            this.jd_slider_range.maxValue = this._selectedBeatmap.MaxJDSlider;

            // These are critical:
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Min_RT_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Max_RT_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Value)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Min_JD_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Max_JD_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Value)));
        }

        //1.19.1 Feature update
        [UIValue("slider_setting_value")]
        private int Slider_Setting_Value
        {
            get => PluginConfig.Instance.slider_setting;
            set
            {
                PluginConfig.Instance.slider_setting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Slider_Setting_Value)));

                // This doesnt work because the MinRTSlider etc can't be publically set, crashes
                //BeatmapUtils.RefreshSliderMinMax(_selectedBeatmap.NJS);

                // This is critcal!
                this.RefreshSliderMinMax();
            }
        }

        [UIAction("slider_setting_increment_formatter")]
        private string Slider_Setting_Increment_Formatter(int value)
        {
            return ((SliderSettingEnum)value).ToString();
        }

        // This function is critical:
        // Without this function, when slider setting is flipped, the slider min maxes will be wrong because they are/were set in BeatmapInfo
        // Ex: When JD flips to RT, sliders will be draw as if set to JD (with JD min-max) until a new map is clicked that triggers BeatmapInfo
        // and PostParse to run again with the new setting.
        // Must "recalculate" them here then trigger everything to update
        private void RefreshSliderMinMax()
        {
            Plugin.Log.Debug("Refresh Slider Min Max");
            this.rt_slider_range = this.RT_Slider.slider.GetComponentInChildren<HMUI.CustomFormatRangeValuesSlider>();
            this.jd_slider_range = this.JD_Slider.slider.GetComponentInChildren<HMUI.CustomFormatRangeValuesSlider>();

            if (PluginConfig.Instance.slider_setting == 0)
            {
                this.rt_slider_range.minValue = PluginConfig.Instance.minJumpDistance * 500 / this._selectedBeatmap.NJS;
                this.rt_slider_range.maxValue = PluginConfig.Instance.maxJumpDistance * 500 / this._selectedBeatmap.NJS;

                this.jd_slider_range.minValue = PluginConfig.Instance.minJumpDistance;
                this.jd_slider_range.maxValue = PluginConfig.Instance.maxJumpDistance;
            }
            else
            {
                this.rt_slider_range.minValue = PluginConfig.Instance.minReactionTime;
                this.rt_slider_range.maxValue = PluginConfig.Instance.maxReactionTime;

                this.jd_slider_range.minValue = PluginConfig.Instance.minReactionTime * this._selectedBeatmap.NJS / 500;
                this.jd_slider_range.maxValue = PluginConfig.Instance.maxReactionTime * this._selectedBeatmap.NJS / 500;
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Min_RT_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Max_RT_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Value)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Min_JD_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Max_JD_Slider)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Value)));
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

    internal enum SliderSettingEnum
    {
        JumpDistance = 0,
        ReactionTime = 1
    }

    internal enum PreferenceEnum
    {
        Off = 0,
        JumpDistance = 1,
        ReactionTime = 2
    }

    internal enum HeuristicEnum
    {
        Off = 0,
        On = 1
    }
}