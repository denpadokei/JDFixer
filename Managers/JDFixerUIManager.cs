using JDFixer.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Zenject;

namespace JDFixer.Managers
{
    internal class JDFixerUIManager : IInitializable, IDisposable
    {
        private static StandardLevelDetailViewController levelDetail;
        private static MissionSelectionMapViewController missionSelection;
        private static MainMenuViewController mainMenu;

        private readonly List<IBeatmapInfoUpdater> beatmapInfoUpdaters;

        [Inject]
        private JDFixerUIManager(StandardLevelDetailViewController standardLevelDetailViewController, MissionSelectionMapViewController missionSelectionMapViewController, MainMenuViewController mainMenuViewController, List<IBeatmapInfoUpdater> iBeatmapInfoUpdaters)
        {
            //Plugin.Log.Debug("JDFixerUIManager()");

            levelDetail = standardLevelDetailViewController;
            missionSelection = missionSelectionMapViewController;
            mainMenu = mainMenuViewController;

            this.beatmapInfoUpdaters = iBeatmapInfoUpdaters;
        }

        public void Initialize()
        {
            //Plugin.Log.Debug("Initialize()");

            levelDetail.didChangeDifficultyBeatmapEvent += this.LevelDetail_didChangeDifficultyBeatmapEvent;
            levelDetail.didChangeContentEvent += this.LevelDetail_didChangeContentEvent;

            if (Plugin.CheckForCustomCampaigns())
            {
                missionSelection.didSelectMissionLevelEvent += this.MissionSelection_didSelectMissionLevelEvent_CC;
            }
            else
            {
                missionSelection.didSelectMissionLevelEvent += this.MissionSelection_didSelectMissionLevelEvent_Base;
            }

            mainMenu.didDeactivateEvent += this.MainMenu_didDeactivateEvent;
            ;
        }

        public void Dispose()
        {
            //Plugin.Log.Debug("Dispose()");

            levelDetail.didChangeDifficultyBeatmapEvent -= this.LevelDetail_didChangeDifficultyBeatmapEvent;
            levelDetail.didChangeContentEvent -= this.LevelDetail_didChangeContentEvent;

            missionSelection.didSelectMissionLevelEvent -= this.MissionSelection_didSelectMissionLevelEvent_CC;
            missionSelection.didSelectMissionLevelEvent -= this.MissionSelection_didSelectMissionLevelEvent_Base;

            mainMenu.didDeactivateEvent -= this.MainMenu_didDeactivateEvent;
        }

        private void LevelDetail_didChangeDifficultyBeatmapEvent(StandardLevelDetailViewController arg1)
        {
            //Plugin.Log.Debug("LevelDetail_didChangeDifficultyBeatmapEvent()");

            if (arg1 != null)
            {
                this.DiffcultyBeatmapUpdated(arg1.beatmapLevel, arg1.beatmapKey);
            }
        }

        private void LevelDetail_didChangeContentEvent(StandardLevelDetailViewController arg1, StandardLevelDetailViewController.ContentType arg2)
        {
            //Plugin.Log.Debug("LevelDetail_didChangeContentEvent()");

            if (arg2 == StandardLevelDetailViewController.ContentType.OwnedAndReady && arg1 != null && arg1.beatmapLevel != null)
            {
                //var mapData = arg1.beatmapLevel.GetDifficultyBeatmapData(arg1.beatmapKey.beatmapCharacteristic, arg1.beatmapKey.difficulty);
                //if (mapData != null)
                //{
                //    Plugin.Log.Debug("NJS: " + mapData.noteJumpMovementSpeed);
                //    Plugin.Log.Debug("Offset: " + mapData.noteJumpStartBeatOffset);
                //}
                this.DiffcultyBeatmapUpdated(arg1.beatmapLevel, arg1.beatmapKey);
            }
        }

        private void MissionSelection_didSelectMissionLevelEvent_CC(MissionSelectionMapViewController arg1, MissionNode arg2)
        {
#if true
            //Yes, we must check for both arg2.missionData and arg2.missionData.beatmapCharacteristic:
            //If a map is not dled, missionID and beatmapDifficulty will be correct, but beatmapCharacteristic will be null
            //Accessing any null values of arg1 or arg2 will crash CC horribly

            if (arg2.missionData != null && arg2.missionData.beatmapCharacteristic != null)
            {
                Plugin.Log.Debug("In CC, MissionNode exists");

                Plugin.Log.Debug("MissionNode - missionid: " + arg2.missionId); //"<color=#0a92ea>[STND]</color> Holdin' Oneb28Easy-1"
                Plugin.Log.Debug("MissionNode - difficulty: " + arg2.missionData.beatmapDifficulty); // "Easy" etc
                Plugin.Log.Debug("MissionNode - characteristic: " + arg2.missionData.beatmapCharacteristic.serializedName); //"Standard" etc

                if (MissionSelectionPatch.cc_level != null) // lol null check just to print?
                {
                    //If a map is not dled, this will be the previous selected node's map
                    Plugin.Log.Debug("CC Level: " + MissionSelectionPatch.cc_level.levelID);  // For cross check with arg2.missionId

                    var difficulty_beatmap = GetLevelFromCC(MissionSelectionPatch.cc_level.levelID, arg2.missionData.beatmapCharacteristic, arg2.missionData.beatmapDifficulty);

                    if (difficulty_beatmap != null) // lol null check just to print?
                    {
                        //Plugin.Log.Debug("MissionNode Diff: " + difficulty_beatmap.difficulty);  // For cross check with arg2.missionData.beatmapDifficulty
                        //Plugin.Log.Debug("MissionNode Offset: " + difficulty_beatmap.noteJumpStartBeatOffset);
                        //Plugin.Log.Debug("MissionNode NJS: " + difficulty_beatmap.noteJumpMovementSpeed);

                        this.DiffcultyBeatmapUpdated(difficulty_beatmap, arg2.missionData.beatmapKey);
                    }
                }
            }
            else // Map not dled
            {
                this.DiffcultyBeatmapUpdated(null, default);
            }
#endif
        }

        private static BeatmapLevel GetLevelFromCC(string s, BeatmapCharacteristicSO beatmapCharacteristicSO, BeatmapDifficulty beatmapDifficulty)
        {
            if (!Plugin.CheckForCustomCampaigns())
            {
                return null;
            }

            var CCScoreSubmissionPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Custom Campaigns.dll");
            try
            {
                var CCScoreSubmissionAssembly = Assembly.LoadFrom(CCScoreSubmissionPath);
                var util = CCScoreSubmissionAssembly.GetType("CustomCampaigns.Utils.BeatmapUtils");
                return (BeatmapLevel)util.GetMethod("GetMatchingBeatmapDifficulty").Invoke(null, new object[] { s, beatmapCharacteristicSO, beatmapDifficulty });
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private void MissionSelection_didSelectMissionLevelEvent_Base(MissionSelectionMapViewController arg1, MissionNode arg2)
        {
            // Base campaign
            if (arg2 != null)
            {
                var beatmapLevel = arg1._beatmapLevelsModel.GetBeatmapLevel(arg2.missionData.beatmapKey.levelId);
                this.DiffcultyBeatmapUpdated(beatmapLevel, arg2.missionData.beatmapKey);
            }
        }

        private void MainMenu_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            //Plugin.Log.Debug("MainMenu_didDeactivate");

            UI.LegacyModifierUI.Instance?.Refresh();

            UI.ModifierUI.Instance?.Refresh();

            UI.CustomOnlineUI.Instance?.Refresh();
        }

        private void DiffcultyBeatmapUpdated(BeatmapLevel level, BeatmapKey difficultyBeatmap)
        {
            //Plugin.Log.Debug("DiffcultyBeatmapUpdated()");

            foreach (var beatmapInfoUpdater in this.beatmapInfoUpdaters)
            {
                beatmapInfoUpdater.BeatmapInfoUpdated(new BeatmapInfo(level, difficultyBeatmap));
            }
        }
    }
}