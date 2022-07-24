// Project:         ViewableSkillProgress mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.20
// Created On: 	    3/20/2022, 12:00 PM
// Last Edit:		7/24/2022, 4:15 PM
// Modifier:
// Special Thanks:  Alphaus, Kab the Bird Ranger, Hazelnut, BadLuckBurt, Sordid, Thevm, Nkrisztian89

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;

namespace ViewableSkillProgress
{
    public class ViewableSkillProgressMain : MonoBehaviour
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

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            instance = new GameObject("ViewableSkillProgress").AddComponent<ViewableSkillProgressMain>(); // Add script to the scene.

            mod.LoadSettingsCallback = LoadSettings; // To enable use of the "live settings changes" feature in-game.

            mod.IsReady = true;
        }

        private void Start()
        {
            Debug.Log("Begin mod init: Viewable Skill Progress");

            mod.LoadSettings();

            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.CharacterSheet, typeof(VSPCharacterSheetOverride));
            Debug.Log("ViewableSkillProgress Registered Override For DaggerfallCharacterSheetWindow");

            Debug.Log("Finished mod init: Viewable Skill Progress");
        }

        #region Settings

        static void LoadSettings(ModSettings modSettings, ModSettingsChange change)
        {
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
        }

        #endregion
    }
}