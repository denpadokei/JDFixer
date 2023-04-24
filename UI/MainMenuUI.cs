﻿using Zenject;
using System;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using System.ComponentModel;

namespace JDFixer.UI
{
    public class MainMenuUI : IInitializable, IDisposable//, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;

        public MainMenuUI()
        {

        }

        public void Initialize()
        {
            BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.AddSettingsMenu("JDFixer", "JDFixer.UI.BSML.mainMenuUI.bsml", this);
        }

        public void Dispose()
        {
            if (BeatSaberMarkupLanguage.Settings.BSMLSettings.instance != null)
            {
                BeatSaberMarkupLanguage.Settings.BSMLSettings.instance.RemoveSettingsMenu(this);
            }
        }


        [UIValue("rt_display_value")]
        private bool RT_Display_Value
        {
            get => PluginConfig.Instance.rt_display_enabled;
            set
            {
                PluginConfig.Instance.rt_display_enabled = value;
            }
        }
        [UIAction("set_rt_display")]
        private void Set_RT_Display(bool value)
        {
            RT_Display_Value = value;
        }


        [UIValue("legacy_display_value")]
        private bool Legacy_Display_Value
        {
            get => PluginConfig.Instance.legacy_display_enabled;
            set
            {
                PluginConfig.Instance.legacy_display_enabled = value;
            }
        }
        [UIAction("set_legacy_display")]
        private void Set_Legacy_Display(bool value)
        {
            Legacy_Display_Value = value;
        }


        /*[UIValue("")]
        private bool Song_Speed_Value
        {
            get => PluginConfig.Instance.song_speed_setting;
            set
            {
                PluginConfig.Instance.song_speed_setting = value;
            }
        }
        [UIAction("set_song_speed")]
        private void Set_Song_Speed(bool value)
        {
            Song_Speed_Value = value;
        }*/


        [UIValue("song_speed_increment_value")]
        private int Song_Speed_Increment_Value
        {
            get => PluginConfig.Instance.song_speed_setting;
            set
            {
                PluginConfig.Instance.song_speed_setting = value;
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Song_Speed_Increment_Value)));
            }
        }
        [UIAction("song_speed_increment_formatter")]
        private string Song_Speed_Increment_Formatter(int value) => ((SongSpeedEnum)value).ToString();


        [UIValue("use_offset_value")]
        private bool Use_Offset_Value
        {
            get => PluginConfig.Instance.use_offset;
            set
            {
                PluginConfig.Instance.use_offset = value;
            }
        }
        [UIAction("set_use_offset")]
        private void Set_Use_Offset(bool value)
        {
            Use_Offset_Value = value;
        }


        [UIComponent("offset_fraction_slider")]
        private SliderSetting Offset_Fraction_Slider;

        [UIValue("offset_fraction_value")]
        private float Offset_Fraction_Value
        {
            get => PluginConfig.Instance.offset_fraction;
            set
            {
                PluginConfig.Instance.offset_fraction = value;
            }
        }
        [UIAction("set_offset_fraction")]
        private void Set_Offset_Fraction(float value)
        {
            Offset_Fraction_Value = value;
        }


        [UIComponent("lower_threshold_slider")]
        private SliderSetting Lower_Threshold_Slider;

        [UIValue("lower_threshold_value")]
        private float Lower_Threshold_Value
        {
            get => PluginConfig.Instance.lower_threshold;
            set
            {
                PluginConfig.Instance.lower_threshold = value;
            }
        }
        [UIAction("set_lower_threshold")]
        private void Set_Lower_Threshold(float value)
        {
            Lower_Threshold_Value = value;
        }


        [UIComponent("upper_threshold_slider")]
        private SliderSetting Upper_Threshold_Slider;

        [UIValue("upper_threshold_value")]
        private float Upper_Threshold_Value
        {
            get => PluginConfig.Instance.upper_threshold;
            set
            {
                PluginConfig.Instance.upper_threshold = value;
            }
        }
        [UIAction("set_upper_threshold")]
        private void Set_Upper_Threshold(float value)
        {
            Upper_Threshold_Value = value;
        }

        [UIValue("press_ok_text")]
        private string Press_Ok_Text = "<#ffffffff>Press OK to apply settings  <#ff0080ff>♡       <size=70%>v6.0.0 by Zephyr#9125<#00000000>--";
    }

    internal enum SongSpeedEnum
    {
        Off = 0,
        ReactionTimeOnly = 1,
        Always
    }
}