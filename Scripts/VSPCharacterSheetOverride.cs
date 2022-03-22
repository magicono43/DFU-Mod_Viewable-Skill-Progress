// Project:         ViewableSkillProgress mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2022 Kirk.O
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Kirk.O
// Version:			v.1.02
// Created On: 	    3/20/2022, 12:00 PM
// Last Edit:		3/21/2022, 9:10 PM
// Modifier:
// Special Thanks:  Alphaus, Kab the Bird Ranger, Hazelnut, BadLuckBurt, Sordid, Thevm

using UnityEngine;
using System;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class VSPCharacterSheetOverride : DaggerfallCharacterSheetWindow
    {
        PlayerEntity playerEntity;

        PlayerEntity PlayerEntity
        {
            get { return (playerEntity != null) ? playerEntity : playerEntity = GameManager.Instance.PlayerEntity; }
        }

        #region Constructors

        public VSPCharacterSheetOverride(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
        }

        #endregion

        protected override void Setup()
        {
            base.Setup();
            SetupSkillProgressText();
        }

        protected void SetupSkillProgressText()
        {
            // Primary skills button
            Button primarySkillsButton = DaggerfallUI.AddButton(new Rect(11, 106, 115, 8), NativePanel);
            primarySkillsButton.OnMouseClick += PrimarySkillsButton_OnMouseClick;
            primarySkillsButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.CharacterSheetPrimarySkills);

            // Major skills button
            Button majorSkillsButton = DaggerfallUI.AddButton(new Rect(11, 116, 115, 8), NativePanel);
            majorSkillsButton.OnMouseClick += MajorSkillsButton_OnMouseClick;
            majorSkillsButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.CharacterSheetMajorSkills);

            // Minor skills button
            Button minorSkillsButton = DaggerfallUI.AddButton(new Rect(11, 126, 115, 8), NativePanel);
            minorSkillsButton.OnMouseClick += MinorSkillsButton_OnMouseClick;
            minorSkillsButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.CharacterSheetMinorSkills);

            // Miscellaneous skills button
            Button miscSkillsButton = DaggerfallUI.AddButton(new Rect(11, 136, 115, 8), NativePanel);
            miscSkillsButton.OnMouseClick += MiscSkillsButton_OnMouseClick;
            miscSkillsButton.Hotkey = DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.CharacterSheetMiscSkills);
        }

        private void PrimarySkillsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ShowSkillsDialog(PlayerEntity.GetPrimarySkills());
        }

        private void MajorSkillsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ShowSkillsDialog(PlayerEntity.GetMajorSkills());
        }

        private void MinorSkillsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ShowSkillsDialog(PlayerEntity.GetMinorSkills());
        }

        private void MiscSkillsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            ShowSkillsDialog(PlayerEntity.GetMiscSkills(), true);
        }

        public int CurrentTallyCount(DFCareer.Skills skill)
        {
            int i = (int)skill;

            int reflexesMod = 0x10000 - (((int)playerEntity.Reflexes - 2) << 13);
            int calculatedSkillUses = (playerEntity.SkillUses[i] * reflexesMod) >> 16;

            return calculatedSkillUses;
        }

        public int TallysNeededToAdvance(DFCareer.Skills skill)
        {
            int i = (int)skill;

            int skillAdvancementMultiplier = DaggerfallSkills.GetAdvancementMultiplier((DFCareer.Skills)i);
            float careerAdvancementMultiplier = playerEntity.Career.AdvancementMultiplier;
            int usesNeededForAdvancement = FormulaHelper.CalculateSkillUsesForAdvancement(playerEntity.Skills.GetPermanentSkillValue(i), skillAdvancementMultiplier, careerAdvancementMultiplier, playerEntity.Level);

            return usesNeededForAdvancement;
        }

        // Creates formatting tokens for skill popups
        TextFile.Token[] CreateSkillTokens(DFCareer.Skills skill, bool twoColumn = false, int startPosition = 0)
        {
            bool highlight = playerEntity.GetSkillRecentlyIncreased(skill);
            int currentTallyCount = CurrentTallyCount(skill);
            int tallysNeededForAdvance = TallysNeededToAdvance(skill);

            List<TextFile.Token> tokens = new List<TextFile.Token>();
            TextFile.Formatting formatting = highlight ? TextFile.Formatting.TextHighlight : TextFile.Formatting.Text;

            if (DaggerfallUnity.Settings.SDFFontRendering) // For when SDF Font Rendering is enabled (I.E. The smoother text for higher resolutions.)
            {
                TextFile.Token skillNameToken = new TextFile.Token();
                skillNameToken.formatting = formatting;
                skillNameToken.text = DaggerfallUnity.Instance.TextProvider.GetSkillName(skill);

                TextFile.Token skillTallyTrackerToken = new TextFile.Token();
                skillTallyTrackerToken.formatting = formatting;
                if (playerEntity.Skills.GetPermanentSkillValue((int)skill) >= 100)
                    skillTallyTrackerToken.text = "MASTERED";
                else if (playerEntity.AlreadyMasteredASkill() && playerEntity.Skills.GetPermanentSkillValue((int)skill) >= 95)
                    skillTallyTrackerToken.text = "Maxed";
                else
                    skillTallyTrackerToken.text = string.Format("{0} / {1}", currentTallyCount, tallysNeededForAdvance);

                TextFile.Token skillValueToken = new TextFile.Token();
                skillValueToken.formatting = formatting;
                skillValueToken.text = string.Format("{0}%", playerEntity.Skills.GetLiveSkillValue(skill));

                DFCareer.Stats primaryStat = DaggerfallSkills.GetPrimaryStat(skill);
                TextFile.Token skillPrimaryStatToken = new TextFile.Token();
                skillPrimaryStatToken.formatting = formatting;
                skillPrimaryStatToken.text = DaggerfallUnity.Instance.TextProvider.GetAbbreviatedStatName(primaryStat);

                TextFile.Token positioningToken = new TextFile.Token();
                positioningToken.formatting = TextFile.Formatting.PositionPrefix;

                TextFile.Token tabToken = new TextFile.Token();
                tabToken.formatting = TextFile.Formatting.PositionPrefix;

                // Add tokens in order
                if (!twoColumn)
                {
                    tokens.Add(skillNameToken);
                    tokens.Add(tabToken);
                    tokens.Add(tabToken);
                    tokens.Add(skillTallyTrackerToken);
                }
                else // miscellaneous skills
                {
                    if (startPosition != 0) // if this is the second column
                    {
                        positioningToken.x = startPosition;
                        tokens.Add(positioningToken);
                    }
                    tokens.Add(skillNameToken);
                    positioningToken.x = startPosition + 55;
                    tokens.Add(positioningToken);
                    tokens.Add(skillTallyTrackerToken);
                }
            }
            else // For when the original Daggerfall font is being used (I.E. the less smooth looking "retro" font.)
            {
                TextFile.Token skillNameToken = new TextFile.Token();
                skillNameToken.formatting = formatting;
                skillNameToken.text = DaggerfallUnity.Instance.TextProvider.GetSkillName(skill);

                TextFile.Token skillTallyTrackerToken = new TextFile.Token();
                skillTallyTrackerToken.formatting = formatting;
                if (playerEntity.Skills.GetPermanentSkillValue((int)skill) >= 100)
                    skillTallyTrackerToken.text = "MASTERED";
                else if (playerEntity.AlreadyMasteredASkill() && playerEntity.Skills.GetPermanentSkillValue((int)skill) >= 95)
                    skillTallyTrackerToken.text = "Maxed";
                else
                    skillTallyTrackerToken.text = string.Format("{0} / {1}", currentTallyCount, tallysNeededForAdvance);

                TextFile.Token skillValueToken = new TextFile.Token();
                skillValueToken.formatting = formatting;
                skillValueToken.text = string.Format("{0}%", playerEntity.Skills.GetLiveSkillValue(skill));

                DFCareer.Stats primaryStat = DaggerfallSkills.GetPrimaryStat(skill);
                TextFile.Token skillPrimaryStatToken = new TextFile.Token();
                skillPrimaryStatToken.formatting = formatting;
                skillPrimaryStatToken.text = DaggerfallUnity.Instance.TextProvider.GetAbbreviatedStatName(primaryStat);

                TextFile.Token positioningToken = new TextFile.Token();
                positioningToken.formatting = TextFile.Formatting.PositionPrefix;

                TextFile.Token tabToken = new TextFile.Token();
                tabToken.formatting = TextFile.Formatting.PositionPrefix;

                // Add tokens in order
                if (!twoColumn)
                {
                    tokens.Add(skillNameToken);
                    tokens.Add(tabToken);
                    tokens.Add(tabToken);
                    tokens.Add(tabToken);
                    tokens.Add(skillTallyTrackerToken);
                }
                else // miscellaneous skills
                {
                    if (startPosition != 0) // if this is the second column
                    {
                        positioningToken.x = startPosition;
                        tokens.Add(positioningToken);
                    }
                    tokens.Add(skillNameToken);
                    positioningToken.x = startPosition + 90;
                    tokens.Add(positioningToken);
                    tokens.Add(skillTallyTrackerToken);
                }
            }

            return tokens.ToArray();
        }

        void ShowSkillsDialog(List<DFCareer.Skills> skills, bool twoColumn = false)
        {
            bool secondColumn = false;
            bool showHandToHandDamage = false;
            List<TextFile.Token> tokens = new List<TextFile.Token>();
            int secondColumnStartPos = 120;
            if (!DaggerfallUnity.Settings.SDFFontRendering) // For when the original Daggerfall font is being used (I.E. the less smooth looking "retro" font.)
                secondColumnStartPos = 180;

            for (int i = 0; i < skills.Count; i++)
            {
                if (!showHandToHandDamage && (skills[i] == DFCareer.Skills.HandToHand))
                    showHandToHandDamage = true;

                if (!twoColumn)
                {
                    tokens.AddRange(CreateSkillTokens(skills[i]));
                    if (i < skills.Count - 1)
                        tokens.Add(TextFile.NewLineToken);
                }
                else
                {
                    if (!secondColumn)
                    {
                        tokens.AddRange(CreateSkillTokens(skills[i], true));
                        secondColumn = !secondColumn;
                    }
                    else
                    {
                        tokens.AddRange(CreateSkillTokens(skills[i], true, secondColumnStartPos));
                        secondColumn = !secondColumn;
                        if (i < skills.Count - 1)
                            tokens.Add(TextFile.NewLineToken);
                    }
                }
            }

            if (showHandToHandDamage)
            {
                tokens.Add(TextFile.NewLineToken);
                TextFile.Token HandToHandDamageToken = new TextFile.Token();
                int minDamage = FormulaHelper.CalculateHandToHandMinDamage(playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
                int maxDamage = FormulaHelper.CalculateHandToHandMaxDamage(playerEntity.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
                HandToHandDamageToken.text = DaggerfallUnity.Instance.TextProvider.GetSkillName(DFCareer.Skills.HandToHand) + " dmg: " + minDamage + "-" + maxDamage;
                HandToHandDamageToken.formatting = TextFile.Formatting.Text;
                tokens.Add(HandToHandDamageToken);
            }

            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
            messageBox.SetHighlightColor(DaggerfallUI.DaggerfallUnityStatIncreasedTextColor);
            messageBox.SetTextTokens(tokens.ToArray(), null, false);
            messageBox.ClickAnywhereToClose = true;
            messageBox.Show();
        }
    }
}
