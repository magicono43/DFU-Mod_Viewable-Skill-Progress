// Project:         ViewableSkillProgress mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.20
// Created On: 	    3/20/2022, 12:00 PM
// Last Edit:		7/30/2022, 9:45 PM
// Modifier:
// Special Thanks:  Alphaus, Kab the Bird Ranger, Hazelnut, BadLuckBurt, DunnyOfPenwick, Sordid, Thevm, Nkrisztian89

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Serialization;
using System;

namespace ViewableSkillProgress
{
    public class ViewableSkillProgressMain : MonoBehaviour, IHasModSaveData
    {
        static ViewableSkillProgressMain instance;

        public static ViewableSkillProgressMain Instance
        {
            get { return instance ?? (instance = FindObjectOfType<ViewableSkillProgressMain>()); }
        }

        static Mod mod;

        // Options
        public static int ProgressDisplayType { get; set; }
        public static bool GovernAttributeText { get; set; }

        public static int ProgressTextType { get; set; }
        public static int TextWithOrWithoutBrackets { get; set; }
        public static float ProgressTextScale { get; set; }
        public static Color32 ProgressTextColor { get; set; }

        public static int ProgressBarPosX { get; set; }
        public static int ProgressBarPosY { get; set; }
        public static int ProgressBarWidth { get; set; }
        public static Color32 ProgressBarOutlineColor { get; set; }
        public static Color32 ProgressBarColor { get; set; }
        public static Color32 ReadyToLevelBarColor { get; set; }
        public static Color32 MasteredBarColor { get; set; }
        public static Color32 MaxedBarColor { get; set; }

        public static bool SkillReadyNotifications { get; set; }
        public static int NotificationCheckFrequency { get; set; }
        public static bool AllowSoundNotification { get; set; }
        public static int NotificationSoundClip { get; set; }
        public static float SoundClipVolume { get; set; }
        public static bool AllowHUDTextNotification { get; set; }
        public static int NotificationTextType { get; set; }

        // Attached To SaveData
        public static byte[] notifiedSkillsList = new byte[35];

        // Global Variables
        public static int FixedUpdateCounter { get; set; }

        private static PlayerEntity player = GameManager.Instance.PlayerEntity;
        private static List<DFCareer.Skills> playerSkills = new List<DFCareer.Skills>();
        private static short[] validSoundClipIndexArray = new short[62] {
            361, 74, 300, 316, 362, 363, 364, 383, 380, 449, 450, 452, 304, 308, 18, 19, 94, 107,
            69, 16, 31, 348, 342, 26, 365, 28, 369, 370, 366, 32, 33, 99, 100, 101, 102, 103, 129,
            134, 158, 196, 197, 320, 39, 321, 335, 251, 155, 156, 161, 162, 164, 165, 167, 168, 200,
            205, 202, 208, 455, 302, 345, 456 };

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            instance = new GameObject("ViewableSkillProgress").AddComponent<ViewableSkillProgressMain>(); // Add script to the scene.
            mod.SaveDataInterface = instance;

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Viewable Skill Progress");

            mod.LoadSettings();

            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.CharacterSheet, typeof(VSPCharacterSheetOverride));
            Debug.Log("ViewableSkillProgress Registered Override For DaggerfallCharacterSheetWindow");

            StartGameBehaviour.OnStartGame += PopulateSkillList_OnStartGame;
            SaveLoadManager.OnLoad += PopulateSkillList_OnSaveLoad;

            Debug.Log("Finished mod init: Viewable Skill Progress");
        }

        #region Settings

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
            int storedSoundIndex = -1;
            if (SkillReadyNotifications && AllowSoundNotification && NotificationSoundClip != 0)
                storedSoundIndex = NotificationSoundClip;

            ProgressDisplayType = mod.GetSettings().GetValue<int>("GeneralSettings", "DisplayType");
            GovernAttributeText = mod.GetSettings().GetValue<bool>("GeneralSettings", "ShowGovAttributeText");

            ProgressTextType = mod.GetSettings().GetValue<int>("TextSettings", "TextDisplayType");
            TextWithOrWithoutBrackets = mod.GetSettings().GetValue<int>("TextSettings", "TextBrackets");
            ProgressTextScale = mod.GetSettings().GetValue<float>("TextSettings", "TextScale");
            ProgressTextColor = mod.GetSettings().GetValue<Color32>("TextSettings", "TextColor");

            ProgressBarPosX = mod.GetSettings().GetValue<int>("ProgressBarSettings", "BarPositionX");
            ProgressBarPosY = mod.GetSettings().GetValue<int>("ProgressBarSettings", "BarPositionY");
            ProgressBarWidth = mod.GetSettings().GetValue<int>("ProgressBarSettings", "ProgBarWidth");
            ProgressBarOutlineColor = mod.GetSettings().GetValue<Color32>("ProgressBarSettings", "ProgBarOutlineColor");
            ProgressBarColor = mod.GetSettings().GetValue<Color32>("ProgressBarSettings", "ProgBarColor");
            ReadyToLevelBarColor = mod.GetSettings().GetValue<Color32>("ProgressBarSettings", "ReadyToLevelColor");
            MasteredBarColor = mod.GetSettings().GetValue<Color32>("ProgressBarSettings", "MasteredColor");
            MaxedBarColor = mod.GetSettings().GetValue<Color32>("ProgressBarSettings", "MaxedColor");

            SkillReadyNotifications = mod.GetSettings().GetValue<bool>("NotificationSettings", "SkillReadyNotif");
            NotificationCheckFrequency = mod.GetSettings().GetValue<int>("NotificationSettings", "NotifCheckFreq");
            AllowSoundNotification = mod.GetSettings().GetValue<bool>("NotificationSettings", "AllowSoundNotif");
            NotificationSoundClip = mod.GetSettings().GetValue<int>("NotificationSettings", "NotifSoundClip");
            SoundClipVolume = mod.GetSettings().GetValue<float>("NotificationSettings", "ClipVolume");
            AllowHUDTextNotification = mod.GetSettings().GetValue<bool>("NotificationSettings", "AllowTextNotif");
            NotificationTextType = mod.GetSettings().GetValue<int>("NotificationSettings", "NotifTextType");

            if (SkillReadyNotifications && AllowSoundNotification && storedSoundIndex != -1 && storedSoundIndex != NotificationSoundClip)
                DaggerfallUI.Instance.DaggerfallAudioSource.PlayOneShot((SoundClips)validSoundClipIndexArray[NotificationSoundClip], 0, SoundClipVolume);

            PopulatePlayerSkillList();
        }

        #endregion

        static void PopulateSkillList_OnStartGame(object sender, EventArgs e)
        {
            PopulatePlayerSkillList();
        }

        static void PopulateSkillList_OnSaveLoad(SaveData_v1 saveData)
        {
            PopulatePlayerSkillList();
        }

        static void PopulatePlayerSkillList()
        {
            if (player == null)
                return;

            playerSkills.Clear();
            playerSkills.AddRange(player.GetPrimarySkills());
            playerSkills.AddRange(player.GetMajorSkills());
            playerSkills.AddRange(player.GetMinorSkills());
            playerSkills.AddRange(player.GetMiscSkills());
        }

        private void FixedUpdate()
        {
            if (!SkillReadyNotifications)
                return;

            if (SaveLoadManager.Instance.LoadInProgress)
                return;

            if (GameManager.IsGamePaused)
                return;

            FixedUpdateCounter++; // Increments the FixedUpdateCounter by 1 every FixedUpdate.

            if (FixedUpdateCounter >= 50 * NotificationCheckFrequency) // 50 FixedUpdates is approximately equal to 1 second since each FixedUpdate happens every 0.02 seconds, that's what Unity docs say at least.
            {
                FixedUpdateCounter = 0;

                if (playerSkills.Count > 0)
                {
                    for (int i = 0; i < playerSkills.Count; i++)
                    {
                        float cT = VSPCharacterSheetOverride.CurrentTallyCount(playerSkills[i]);
                        float aT = VSPCharacterSheetOverride.TallysNeededToAdvance(playerSkills[i]);
                        if (cT > aT)
                            cT = aT;

                        if (notifiedSkillsList[i] == 1 && cT < aT)
                            notifiedSkillsList[i] = 0;

                        if (notifiedSkillsList[i] == 0 && cT >= aT)
                        {
                            notifiedSkillsList[i] = 1;

                            NotifyPlayer(playerSkills[i]);
                        }
                    }
                }
            }
        }

        public static void NotifyPlayer(DFCareer.Skills skill)
        {
            if (GameManager.Instance.PlayerDeath.DeathInProgress)
                return;

            if (GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue(skill) >= 100)
                return;

            if (GameManager.Instance.PlayerEntity.AlreadyMasteredASkill() && GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue(skill) >= 95)
                return;

            if (AllowHUDTextNotification)
            {
                DaggerfallUI.AddHUDText(CreateNotificationText(skill), 3.0f);
            }

            if (AllowSoundNotification)
            {
                if (DaggerfallUI.Instance.DaggerfallAudioSource != null && !DaggerfallUI.Instance.DaggerfallAudioSource.IsPlaying()) // Meant to keep notification sound from overlapping each other.
                {
                    DaggerfallUI.Instance.DaggerfallAudioSource.PlayOneShot((SoundClips)validSoundClipIndexArray[NotificationSoundClip], 0, SoundClipVolume);
                }
            }
        }

        public static string CreateNotificationText(DFCareer.Skills skill)
        {
            int variant = UnityEngine.Random.Range(0, 4);
            string raw = "";

            if (NotificationTextType == 1)
            {
                if (variant == 0)
                {
                    raw = "You should meditate on what you have learned about " + DaggerfallUnity.Instance.TextProvider.GetSkillName(skill) + ".";
                }
                else if (variant == 1)
                {
                    raw = "Perhaps it is time to reflect on what you have learned about " + DaggerfallUnity.Instance.TextProvider.GetSkillName(skill) + ".";
                }
                else if (variant == 2)
                {
                    raw = "After much practice, you feel ready to advance further in " + DaggerfallUnity.Instance.TextProvider.GetSkillName(skill) + ".";
                }
                else
                {
                    raw = "You should rest on what you have learned about " + DaggerfallUnity.Instance.TextProvider.GetSkillName(skill) + ".";
                }

                /*string[] raws = new string[4]{
                    "You should meditate on what you have learned about " + DaggerfallUnity.Instance.TextProvider.GetSkillName(skill) + ".",
                    "You should rest on what you have learned about " + DaggerfallUnity.Instance.TextProvider.GetSkillName(skill) + ".",
                    "",
                    "Far"
                };

                return raws[UnityEngine.Random.Range(0, raws.Length)];*/
            }
            else
            {
                raw = DaggerfallUnity.Instance.TextProvider.GetSkillName(skill) + " is ready.";
            }

            return raw;
        }

        #region SaveData Junk

        public Type SaveDataType
        {
            get { return typeof(ViewableSkillProgressSaveData); }
        }

        public object NewSaveData()
        {
            return new ViewableSkillProgressSaveData
            {
                NotifiedSkillsList = new byte[35]
            };
        }

        public object GetSaveData()
        {
            return new ViewableSkillProgressSaveData
            {
                NotifiedSkillsList = notifiedSkillsList
            };
        }

        public void RestoreSaveData(object saveData)
        {
            var viewableSkillProgressSaveData = (ViewableSkillProgressSaveData)saveData;
            notifiedSkillsList = viewableSkillProgressSaveData.NotifiedSkillsList;
        }
    }

    [FullSerializer.fsObject("v1")]
    public class ViewableSkillProgressSaveData
    {
        public byte[] NotifiedSkillsList;
    }

    #endregion

}