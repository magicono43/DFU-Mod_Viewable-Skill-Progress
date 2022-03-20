// Project:         ViewableSkillProgress mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.00
// Created On: 	    3/20/2022, 12:00 PM
// Last Edit:		3/20/2022, 12:00 PM
// Modifier:
// Special Thanks:  Alphaus, Kab the Bird Ranger, Hazelnut

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;

namespace ViewableSkillProgress
{
    public class ViewableSkillProgressMain : MonoBehaviour
    {
        static Mod mod;

        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            mod = initParams.Mod;
            var go = new GameObject(mod.Title);
            go.AddComponent<ViewableSkillProgressMain>();
        }

        void Awake()
        {
            InitMod();

            mod.IsReady = true;
        }

        private static void InitMod()
        {
            Debug.Log("Begin mod init: ViewableSkillProgress");

            UIWindowFactory.RegisterCustomUIWindow(UIWindowType.CharacterSheet, typeof(VSPCharacterSheetOverride));
            Debug.Log("ViewableSkillProgress Registered Override For DaggerfallCharacterSheetWindow");

            Debug.Log("Finished mod init: ViewableSkillProgress");
        }
    }
}