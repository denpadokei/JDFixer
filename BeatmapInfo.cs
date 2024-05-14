namespace JDFixer
{
    public class BeatmapInfo
    {
        internal delegate void BeatmapInfoEventHandler(BeatmapInfo e);

        internal static event BeatmapInfoEventHandler SelectedChanged;

        internal static void SetSelected(BeatmapLevel level, BeatmapKey key)
        {
            var updatedMapInfo = level == null ? Empty : new BeatmapInfo(level, key);
            Selected = updatedMapInfo;

            SelectedChanged?.Invoke(updatedMapInfo);
        }

        public static BeatmapInfo Selected { get; private set; } = Empty;

        internal static BeatmapInfo Empty { get; } = new BeatmapInfo();

        private BeatmapInfo()
        {
            // To enable Campaigns and TA to show 0 instead of values from the last selected map in Solo Mode,
            // Better UX as players may forget to ignore the display.
            this.JumpDistance = 0f;
            this.MinJumpDistance = 0f;
            this.ReactionTime = 0f;
            this.MinReactionTime = 0f;

            // Ultra hack way to prevent divide by zero in Reaction Time Display
            this.NJS = 0.001f;

            // Experimental
            this.MinRTSlider = 0f;
            this.MaxRTSlider = 3000f;

            this.MinJDSlider = 0f;
            this.MaxJDSlider = 50f;

            // 1.26.0-1.29.0 Feature update
            this.JDOffsetQuantum = 0.1f;
            this.RTOffsetQuantum = 5f;
        }

        internal BeatmapInfo(BeatmapLevel level, BeatmapKey key)
        {
            if (level == null)
            {
                return;
            }

            var mapData = level.GetDifficultyBeatmapData(key.beatmapCharacteristic, key.difficulty);
            if (mapData == null)
            {
                return;
            }
            var bpm = level.beatsPerMinute;
            var njs = mapData.noteJumpMovementSpeed;
            var offset = mapData.noteJumpStartBeatOffset;

            if (njs <= 0.01f)
            {
                njs = 10f;
            }

            this.JumpDistance = BeatmapUtils.CalculateJumpDistance(bpm, njs, offset);
            this.MinJumpDistance = BeatmapUtils.CalculateJumpDistance(bpm, njs, -50f);
            this.NJS = njs;
            this.ReactionTime = this.JumpDistance * 500 / this.NJS;
            this.MinReactionTime = this.MinJumpDistance * 500 / this.NJS;

            // Experimental
            if (PluginConfig.Instance.slider_setting == 0)
            {
                this.MinRTSlider = PluginConfig.Instance.minJumpDistance * 500 / this.NJS;
                this.MaxRTSlider = PluginConfig.Instance.maxJumpDistance * 500 / this.NJS;

                this.MinJDSlider = PluginConfig.Instance.minJumpDistance;
                this.MaxJDSlider = PluginConfig.Instance.maxJumpDistance;
            }
            else
            {
                this.MinRTSlider = PluginConfig.Instance.minReactionTime;
                this.MaxRTSlider = PluginConfig.Instance.maxReactionTime;

                this.MinJDSlider = PluginConfig.Instance.minReactionTime * this.NJS / 500;
                this.MaxJDSlider = PluginConfig.Instance.maxReactionTime * this.NJS / 500;
            }

            // 1.26.0-1.29.0 Feature update
            this.Offset = offset;
            this.JDOffsetQuantum = BeatmapUtils.CalculateJumpDistance(bpm, njs, offset + (1 / PluginConfig.Instance.offset_fraction)) - BeatmapUtils.CalculateJumpDistance(bpm, njs, offset);
            this.RTOffsetQuantum = this.JDOffsetQuantum * 500 / this.NJS;

            //Plugin.Log.Debug("BeatmapInfo minJD: " + PluginConfig.Instance.minJumpDistance);
            //Plugin.Log.Debug("BeatmapInfo maxJD: " + PluginConfig.Instance.maxJumpDistance);
            //Plugin.Log.Debug("BeatmapInfo minRT: " + MinRTSlider);
            //Plugin.Log.Debug("BeatmapInfo maxRT: " + MaxRTSlider);
        }

        // 1.29.1
        internal static float speedMultiplier = 1f;

        public float JumpDistance { get; }
        public float MinJumpDistance { get; }
        public float NJS { get; }
        public float ReactionTime { get; }
        public float MinReactionTime { get; }

        // Experimental
        internal float MinRTSlider { get; }
        internal float MaxRTSlider { get; }

        internal float MinJDSlider { get; }
        internal float MaxJDSlider { get; }

        // 1.26.0-1.29.0 Feature update
        public float Offset { get; }
        internal float JDOffsetQuantum { get; }
        internal float RTOffsetQuantum { get; }
    }
}