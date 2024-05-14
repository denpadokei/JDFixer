using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System;
using System.ComponentModel;
using Zenject;

namespace JDFixer.UI
{
    internal sealed class CustomOnlineUI : IInitializable, IDisposable, INotifyPropertyChanged
    {
        internal static CustomOnlineUI Instance { get; private set; }
        private readonly MainFlowCoordinator _mainFlow;
        private readonly PreferencesFlowCoordinator _prefFlow;

        public event PropertyChangedEventHandler PropertyChanged;

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("JDFixer-TA/MP", "JDFixer.UI.BSML.customOnlineUI.bsml", this, MenuType.Custom | MenuType.Online);
            Donate.Refresh_Text();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Donate_Update_Dynamic)));
        }

        public void Dispose()
        {
            if (GameplaySetup.instance != null)
            {
                PluginConfig.Instance.Changed();
                GameplaySetup.instance.RemoveTab("JDFixer-TA/MP");
            }
        }

        // To get the flow coordinators using zenject, we use a constructor
        private CustomOnlineUI(MainFlowCoordinator mainFlowCoordinator, PreferencesFlowCoordinator preferencesFlowCoordinator)
        {
            Instance = this;
            this._mainFlow = mainFlowCoordinator;
            this._prefFlow = preferencesFlowCoordinator;
            Donate.Refresh_Text();
        }

        // For updating UI values to match those last used in Solo, when coming from Solo to Online
        internal void Refresh()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Slider_Setting_Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Increment_Value)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Pref_Button)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Heuristic_Increment_Value)));

            this.PostParse();
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

        [UIValue("slider_setting_value")]
        private int Slider_Setting_Value
        {
            get => PluginConfig.Instance.slider_setting;
            set
            {
                PluginConfig.Instance.slider_setting = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Slider_Setting_Value)));

                this.PostParse();
            }
        }

        [UIAction("slider_setting_increment_formatter")]
        private string Slider_Setting_Increment_Formatter(int value)
        {
            return ((SliderSettingEnum)value).ToString();
        }

        //=============================================================================================
        // JD and RT Sliders

        [UIValue("jd_text")]
        private string JD_Text => this.Get_JD_Text();

        private string Get_JD_Text()
        {
            return PluginConfig.Instance.slider_setting == 0 && PluginConfig.Instance.pref_selected == 0
                ? "Desired Jump Distance"
                : PluginConfig.Instance.slider_setting == 0 && PluginConfig.Instance.pref_selected != 0
                    ? "<#555555dd>Desired Jump Distance"
                    : "<#555555dd>Inactive JD";
        }

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
            set => PluginConfig.Instance.jumpDistance = value;
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

        [UIValue("rt_text")]
        private string RT_Text => this.Get_RT_Text();

        private string Get_RT_Text()
        {
            return PluginConfig.Instance.slider_setting == 1 && PluginConfig.Instance.pref_selected == 0
                ? "Desired Reaction Time"
                : PluginConfig.Instance.slider_setting == 1 && PluginConfig.Instance.pref_selected != 0
                    ? "<#555555dd>Desired Reaction Time"
                    : "<#555555dd>Inactive RT";
        }

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
            set => PluginConfig.Instance.reactionTime = value;
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

        //=============================================================================================

        [UIValue("increment_value")]
        private int Increment_Value
        {
            get => PluginConfig.Instance.pref_selected;
            set
            {
                PluginConfig.Instance.pref_selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Increment_Value)));

                this.Set_Preference_Mode();
                this.PostParse();
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
        private string Thresholds => "≤ " + PluginConfig.Instance.lower_threshold.ToString() + " or ≥ " + PluginConfig.Instance.upper_threshold.ToString();

        //=============================================================================================

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
            this.rt_slider_text = this.RT_Slider.slider.GetComponentInChildren<CurvedTextMeshPro>();

            if (this.jd_slider_text != null && this.rt_slider_text != null)
            {
                if (PluginConfig.Instance.use_jd_pref || PluginConfig.Instance.use_rt_pref)
                {
                    this.jd_slider_text.color = new UnityEngine.Color(0.3f, 0.3f, 0.3f);
                    this.rt_slider_text.color = new UnityEngine.Color(0.3f, 0.3f, 0.3f);
                }

                else if (PluginConfig.Instance.slider_setting == 0)
                {
                    this.jd_slider_text.color = new UnityEngine.Color(1f, 1f, 0f);
                    this.rt_slider_text.color = new UnityEngine.Color(0.3f, 0.3f, 0.3f);
                }

                else // PluginConfig.Instance.slider_setting == 1
                {
                    this.jd_slider_text.color = new UnityEngine.Color(0.3f, 0.3f, 0.3f);
                    this.rt_slider_text.color = new UnityEngine.Color(204f / 255f, 153f / 255f, 1f);
                }
            }

            // These are critical:
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.JD_Text)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Text)));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RT_Value)));
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
}