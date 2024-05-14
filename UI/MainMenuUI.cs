using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System;
using Zenject;

namespace JDFixer.UI
{
    internal sealed class MainMenuUI : IInitializable, IDisposable
    {
        private MainMenuUI()
        {

        }

        public void Initialize()
        {
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu("JDFixer", "JDFixer.UI.BSML.mainMenuUI.bsml", this);
        }

        public void Dispose()
        {
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance?.RemoveSettingsMenu(this);
        }

        [UIValue("rt_display_value")]
        private bool RT_Display_Value
        {
            get => PluginConfig.Instance.rt_display_enabled;
            set => PluginConfig.Instance.rt_display_enabled = value;
        }
        [UIAction("set_rt_display")]
        private void Set_RT_Display(bool value)
        {
            this.RT_Display_Value = value;
        }

        [UIValue("legacy_display_value")]
        private bool Legacy_Display_Value
        {
            get => PluginConfig.Instance.legacy_display_enabled;
            set => PluginConfig.Instance.legacy_display_enabled = value;
        }
        [UIAction("set_legacy_display")]
        private void Set_Legacy_Display(bool value)
        {
            this.Legacy_Display_Value = value;
        }

        [UIValue("song_speed_increment_value")]
        private int Song_Speed_Increment_Value
        {
            get => PluginConfig.Instance.song_speed_setting;
            set => PluginConfig.Instance.song_speed_setting = value;
        }
        [UIAction("song_speed_increment_formatter")]
        private string Song_Speed_Increment_Formatter(int value)
        {
            return ((SongSpeedEnum)value).ToString();
        }

        [UIValue("use_offset_value")]
        private bool Use_Offset_Value
        {
            get => PluginConfig.Instance.use_offset;
            set => PluginConfig.Instance.use_offset = value;
        }
        [UIAction("set_use_offset")]
        private void Set_Use_Offset(bool value)
        {
            this.Use_Offset_Value = value;
        }

        [UIComponent("offset_fraction_slider")]
        private readonly SliderSetting Offset_Fraction_Slider;

        [UIValue("offset_fraction_value")]
        private float Offset_Fraction_Value
        {
            get => PluginConfig.Instance.offset_fraction;
            set => PluginConfig.Instance.offset_fraction = value;
        }
        [UIAction("set_offset_fraction")]
        private void Set_Offset_Fraction(float value)
        {
            this.Offset_Fraction_Value = value;
        }

        [UIComponent("lower_threshold_slider")]
        private readonly SliderSetting Lower_Threshold_Slider;

        [UIValue("lower_threshold_value")]
        private float Lower_Threshold_Value
        {
            get => PluginConfig.Instance.lower_threshold;
            set => PluginConfig.Instance.lower_threshold = value;
        }
        [UIAction("set_lower_threshold")]
        private void Set_Lower_Threshold(float value)
        {
            this.Lower_Threshold_Value = value;
        }

        [UIComponent("upper_threshold_slider")]
        private readonly SliderSetting Upper_Threshold_Slider;

        [UIValue("upper_threshold_value")]
        private float Upper_Threshold_Value
        {
            get => PluginConfig.Instance.upper_threshold;
            set => PluginConfig.Instance.upper_threshold = value;
        }
        [UIAction("set_upper_threshold")]
        private void Set_Upper_Threshold(float value)
        {
            this.Upper_Threshold_Value = value;
        }

        [UIValue("press_ok_text_1")]
        private readonly string Press_Ok_Text_1 = "<#ffffffff>Press OK to apply settings  <#ff0080ff>♡";
        [UIValue("press_ok_text_2")]
        private readonly string Press_Ok_Text_2 = "<size=70%><#ff0080ff>v7.1.1 by Zephyr9125";
        [UIValue("press_ok_hint_2")]
        private readonly string Press_Ok_Hint_2 = "";
    }

    internal enum SongSpeedEnum
    {
        JD_Settings = 0,
        RT_Settings = 1,
        JD_RT_Respectively
    }
}