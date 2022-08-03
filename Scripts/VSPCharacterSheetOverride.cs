using UnityEngine;
using System;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using ViewableSkillProgress;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    public class VSPCharacterSheetOverride : DaggerfallCharacterSheetWindow
    {
        PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;

        public static Outline outline;

        #region Constructors

        public VSPCharacterSheetOverride(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
        }

        #endregion

        public static int CurrentTallyCount(DFCareer.Skills skill)
        {
            int i = (int)skill;

            int reflexesMod = 0x10000 - (((int)GameManager.Instance.PlayerEntity.Reflexes - 2) << 13);
            int calculatedSkillUses = (GameManager.Instance.PlayerEntity.SkillUses[i] * reflexesMod) >> 16;

            return calculatedSkillUses;
        }

        public static int TallysNeededToAdvance(DFCareer.Skills skill)
        {
            int i = (int)skill;

            int skillAdvancementMultiplier = DaggerfallSkills.GetAdvancementMultiplier((DFCareer.Skills)i);
            float careerAdvancementMultiplier = GameManager.Instance.PlayerEntity.Career.AdvancementMultiplier;
            int usesNeededForAdvancement = FormulaHelper.CalculateSkillUsesForAdvancement(GameManager.Instance.PlayerEntity.Skills.GetPermanentSkillValue(i), skillAdvancementMultiplier, careerAdvancementMultiplier, GameManager.Instance.PlayerEntity.Level);

            return usesNeededForAdvancement;
        }

        public string GetBracketLeftType()
        {
            if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 0) { return ""; } // None
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 1) { return "<"; } // <>
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 2) { return "["; } // []
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 3) { return "("; } // ()
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 4) { return "+"; } // ++
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 5) { return "*"; } // **
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 6) { return "="; } // ==
            else { return ""; }
        }

        public string GetBracketRightType()
        {
            if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 0) { return ""; } // None
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 1) { return ">"; } // <>
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 2) { return "]"; } // []
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 3) { return ")"; } // ()
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 4) { return "+"; } // ++
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 5) { return "*"; } // **
            else if (ViewableSkillProgressMain.TextWithOrWithoutBrackets == 6) { return "="; } // ==
            else { return ""; }
        }

        // Creates formatting tokens for skill popups
        TextFile.Token[] CreateSkillTokens(DFCareer.Skills skill, bool twoColumn = false, int startPosition = 0)
        {
            bool highlight = playerEntity.GetSkillRecentlyIncreased(skill);

            List<TextFile.Token> tokens = new List<TextFile.Token>();
            TextFile.Formatting formatting = highlight ? TextFile.Formatting.TextHighlight : TextFile.Formatting.Text;

            TextFile.Token skillNameToken = new TextFile.Token();
            skillNameToken.formatting = formatting;
            skillNameToken.text = DaggerfallUnity.Instance.TextProvider.GetSkillName(skill);

            TextFile.Token skillValueToken = new TextFile.Token();
            skillValueToken.formatting = formatting;
            skillValueToken.text = string.Format("{0}", playerEntity.Skills.GetLiveSkillValue(skill));

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
                positioningToken.x = DaggerfallUnity.Settings.SDFFontRendering ?  65 + ViewableSkillProgressMain.ProgressBarWidth : 75 + ViewableSkillProgressMain.ProgressBarWidth;
                tokens.Add(positioningToken);
                tokens.Add(skillValueToken);

                if (ViewableSkillProgressMain.GovernAttributeText)
                {
                    positioningToken.x = DaggerfallUnity.Settings.SDFFontRendering ? startPosition + 78 + ViewableSkillProgressMain.ProgressBarWidth : startPosition + 92 + ViewableSkillProgressMain.ProgressBarWidth;
                    tokens.Add(positioningToken);
                    tokens.Add(skillPrimaryStatToken);
                }
            }
            else // miscellaneous skills
            {
                if (startPosition != 0) // if this is the second column
                {
                    positioningToken.x = startPosition;
                    tokens.Add(positioningToken);
                }
                tokens.Add(skillNameToken);
                positioningToken.x = startPosition + 50; // was 50
                tokens.Add(positioningToken);
                positioningToken.x = DaggerfallUnity.Settings.SDFFontRendering ? startPosition + 70 + ViewableSkillProgressMain.ProgressBarWidth : startPosition + 80 + ViewableSkillProgressMain.ProgressBarWidth; // was 85
                tokens.Add(positioningToken);
                tokens.Add(skillValueToken);

                if (ViewableSkillProgressMain.GovernAttributeText)
                {
                    positioningToken.x = DaggerfallUnity.Settings.SDFFontRendering ? startPosition + 83 + ViewableSkillProgressMain.ProgressBarWidth : startPosition + 97 + ViewableSkillProgressMain.ProgressBarWidth;
                    tokens.Add(positioningToken);
                    tokens.Add(skillPrimaryStatToken);
                }
            }

            return tokens.ToArray();
        }

        protected override void ShowSkillsDialog(List<DFCareer.Skills> skills, bool twoColumn = false)
        {
            string brL = GetBracketLeftType();
            string brR = GetBracketRightType(); // Just used for if user has the "TextBrackets" option enabled and set to something, otherwise returns empty string.
            int miscSpaceModifier = DaggerfallUnity.Settings.SDFFontRendering ? 100 : 123;
            bool secondColumn = false;
            bool showHandToHandDamage = false;
            List<TextFile.Token> tokens = new List<TextFile.Token>();
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
                        tokens.AddRange(CreateSkillTokens(skills[i], true, miscSpaceModifier + ViewableSkillProgressMain.ProgressBarWidth));
                        secondColumn = !secondColumn;
                        if (i < skills.Count - 1)
                            tokens.Add(TextFile.NewLineToken);
                    }
                }
            }

            secondColumn = false;

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


            messageBox.ImagePanel.VerticalAlignment = VerticalAlignment.None;
            messageBox.ImagePanel.HorizontalAlignment = HorizontalAlignment.None;
            messageBox.ImagePanel.Position = new Vector2(0, 0);


            for (int i = 0; i < skills.Count; i++)
            {
                float cT = CurrentTallyCount(skills[i]);
                float aT = TallysNeededToAdvance(skills[i]);
                float sT = cT; // Just here to make the "For Nerds" text display option cleaner to implement in this code.
                if (cT > aT)
                    cT = aT;

                if (!twoColumn)
                {
                    int xPanModifier = DaggerfallUnity.Settings.SDFFontRendering ? -5 : 13;
                    int yPanModifier = showHandToHandDamage ? 7 : 0;

                    Panel pan = DaggerfallUI.AddPanel(new Rect(ViewableSkillProgressMain.ProgressBarPosX + xPanModifier, (ViewableSkillProgressMain.ProgressBarPosY + yPanModifier + i * 7) - 8, ViewableSkillProgressMain.ProgressBarWidth, 5), messageBox.ImagePanel); // 50

                    if (ViewableSkillProgressMain.ProgressDisplayType == 0 || ViewableSkillProgressMain.ProgressDisplayType == 2) // Only show progress bar when "DisplayType" is 0 or 2
                    {
                        Panel bar = DaggerfallUI.AddPanel(new Rect(1, 0, ViewableSkillProgressMain.ProgressBarWidth, 5), pan);

                        if (playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 100)
                            bar.BackgroundColor = ViewableSkillProgressMain.MasteredBarColor; // black by default
                        else if (playerEntity.AlreadyMasteredASkill() && playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 95)
                            bar.BackgroundColor = ViewableSkillProgressMain.MaxedBarColor; // dark blue by default
                        else if (cT >= aT)
                            bar.BackgroundColor = ViewableSkillProgressMain.ReadyToLevelBarColor; // dark green by default
                        else
                        {
                            bar = DaggerfallUI.AddPanel(new Rect(1, 0, (int)Mathf.Floor((cT / aT) * ViewableSkillProgressMain.ProgressBarWidth), 5), pan);
                            bar.BackgroundColor = ViewableSkillProgressMain.ProgressBarColor; // dark red by default
                        }

                        Outline panBorder = DaggerfallUI.AddOutline(new Rect(0, 0, ViewableSkillProgressMain.ProgressBarWidth, 5), ViewableSkillProgressMain.ProgressBarOutlineColor, pan); // yellow by default
                    }

                    if (ViewableSkillProgressMain.ProgressDisplayType == 0 || ViewableSkillProgressMain.ProgressDisplayType == 1) // Only show progress text when "DisplayType" is 0 or 1
                    {
                        TextLabel progText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 1), string.Empty, pan);
                        progText.HorizontalAlignment = HorizontalAlignment.Center;
                        progText.TextScale = ViewableSkillProgressMain.ProgressTextScale;
                        progText.TextColor = ViewableSkillProgressMain.ProgressTextColor;

                        if (playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 100 && ViewableSkillProgressMain.ProgressTextType != 2)
                            progText.Text = brL + "MASTERED" + brR;
                        else if (playerEntity.AlreadyMasteredASkill() && playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 95 && ViewableSkillProgressMain.ProgressTextType != 2)
                            progText.Text = brL + "MAXED" + brR;
                        else if (cT >= aT && ViewableSkillProgressMain.ProgressTextType != 2)
                            progText.Text = brL + "READY" + brR;
                        else
                        {
                            if (ViewableSkillProgressMain.ProgressTextType == 0)
                                progText.Text = string.Format(brL + "{0} / {1}" + brR, cT, aT);
                            else if (ViewableSkillProgressMain.ProgressTextType == 1)
                                progText.Text = string.Format(brL + "{0}" + brR, aT - cT);
                            else
                                progText.Text = string.Format(brL + "{0} / {1}" + brR, sT, aT);
                        }
                    }
                }
                else
                {
                    if (!secondColumn)
                    {
                        int xPanModifier = DaggerfallUnity.Settings.SDFFontRendering ? 0 : 15;
                        float yPanModifier = showHandToHandDamage ? 0f : -7.0f;

                        Panel pan = DaggerfallUI.AddPanel(new Rect(ViewableSkillProgressMain.ProgressBarPosX + xPanModifier, ViewableSkillProgressMain.ProgressBarPosY + yPanModifier + i * 3.5f, ViewableSkillProgressMain.ProgressBarWidth, 5), messageBox.ImagePanel); // 50

                        if (ViewableSkillProgressMain.ProgressDisplayType == 0 || ViewableSkillProgressMain.ProgressDisplayType == 2) // Only show progress bar when "DisplayType" is 0 or 2
                        {
                            Panel bar = DaggerfallUI.AddPanel(new Rect(1, 0, ViewableSkillProgressMain.ProgressBarWidth, 5), pan);

                            if (playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 100)
                                bar.BackgroundColor = ViewableSkillProgressMain.MasteredBarColor; // black by default
                            else if (playerEntity.AlreadyMasteredASkill() && playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 95)
                                bar.BackgroundColor = ViewableSkillProgressMain.MaxedBarColor; // dark blue by default
                            else if (cT >= aT)
                                bar.BackgroundColor = ViewableSkillProgressMain.ReadyToLevelBarColor; // dark green by default
                            else
                            {
                                bar = DaggerfallUI.AddPanel(new Rect(1, 0, (int)Mathf.Floor((cT / aT) * ViewableSkillProgressMain.ProgressBarWidth), 5), pan);
                                bar.BackgroundColor = ViewableSkillProgressMain.ProgressBarColor; // dark red by default
                            }

                            Outline panBorder = DaggerfallUI.AddOutline(new Rect(0, 0, ViewableSkillProgressMain.ProgressBarWidth, 5), ViewableSkillProgressMain.ProgressBarOutlineColor, pan); // yellow by default
                        }

                        if (ViewableSkillProgressMain.ProgressDisplayType == 0 || ViewableSkillProgressMain.ProgressDisplayType == 1) // Only show progress text when "DisplayType" is 0 or 1
                        {
                            TextLabel progText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 1), string.Empty, pan);
                            progText.HorizontalAlignment = HorizontalAlignment.Center;
                            progText.TextScale = ViewableSkillProgressMain.ProgressTextScale;
                            progText.TextColor = ViewableSkillProgressMain.ProgressTextColor;

                            if (playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 100 && ViewableSkillProgressMain.ProgressTextType != 2)
                                progText.Text = brL + "MASTERED" + brR;
                            else if (playerEntity.AlreadyMasteredASkill() && playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 95 && ViewableSkillProgressMain.ProgressTextType != 2)
                                progText.Text = brL + "MAXED" + brR;
                            else if (cT >= aT && ViewableSkillProgressMain.ProgressTextType != 2)
                                progText.Text = brL + "READY" + brR;
                            else
                            {
                                if (ViewableSkillProgressMain.ProgressTextType == 0)
                                    progText.Text = string.Format(brL + "{0} / {1}" + brR, cT, aT);
                                else if (ViewableSkillProgressMain.ProgressTextType == 1)
                                    progText.Text = string.Format(brL + "{0}" + brR, aT - cT);
                                else
                                    progText.Text = string.Format(brL + "{0} / {1}" + brR, sT, aT);
                            }
                        }

                        secondColumn = !secondColumn;
                    }
                    else
                    {
                        int xPanModifier = DaggerfallUnity.Settings.SDFFontRendering ? 100 : 138;
                        float yPanModifier = showHandToHandDamage ? 0f : -7.0f;

                        Panel pan = DaggerfallUI.AddPanel(new Rect(ViewableSkillProgressMain.ProgressBarPosX + xPanModifier + ViewableSkillProgressMain.ProgressBarWidth, (ViewableSkillProgressMain.ProgressBarPosY + yPanModifier + i * 3.5f) - 3, ViewableSkillProgressMain.ProgressBarWidth, 5), messageBox.ImagePanel); // 190

                        if (ViewableSkillProgressMain.ProgressDisplayType == 0 || ViewableSkillProgressMain.ProgressDisplayType == 2) // Only show progress bar when "DisplayType" is 0 or 2
                        {
                            Panel bar = DaggerfallUI.AddPanel(new Rect(1, 0, ViewableSkillProgressMain.ProgressBarWidth, 5), pan);

                            if (playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 100)
                                bar.BackgroundColor = ViewableSkillProgressMain.MasteredBarColor; // black by default
                            else if (playerEntity.AlreadyMasteredASkill() && playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 95)
                                bar.BackgroundColor = ViewableSkillProgressMain.MaxedBarColor; // dark blue by default
                            else if (cT >= aT)
                                bar.BackgroundColor = ViewableSkillProgressMain.ReadyToLevelBarColor; // dark green by default
                            else
                            {
                                bar = DaggerfallUI.AddPanel(new Rect(1, 0, (int)Mathf.Floor((cT / aT) * ViewableSkillProgressMain.ProgressBarWidth), 5), pan);
                                bar.BackgroundColor = ViewableSkillProgressMain.ProgressBarColor; // dark red by default
                            }

                            Outline panBorder = DaggerfallUI.AddOutline(new Rect(0, 0, ViewableSkillProgressMain.ProgressBarWidth, 5), ViewableSkillProgressMain.ProgressBarOutlineColor, pan); // yellow by default
                        }

                        if (ViewableSkillProgressMain.ProgressDisplayType == 0 || ViewableSkillProgressMain.ProgressDisplayType == 1) // Only show progress text when "DisplayType" is 0 or 1
                        {
                            TextLabel progText = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 1), string.Empty, pan);
                            progText.HorizontalAlignment = HorizontalAlignment.Center;
                            progText.TextScale = ViewableSkillProgressMain.ProgressTextScale;
                            progText.TextColor = ViewableSkillProgressMain.ProgressTextColor;

                            if (playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 100 && ViewableSkillProgressMain.ProgressTextType != 2)
                                progText.Text = brL + "MASTERED" + brR;
                            else if (playerEntity.AlreadyMasteredASkill() && playerEntity.Skills.GetPermanentSkillValue(skills[i]) >= 95 && ViewableSkillProgressMain.ProgressTextType != 2)
                                progText.Text = brL + "MAXED" + brR;
                            else if (cT >= aT && ViewableSkillProgressMain.ProgressTextType != 2)
                                progText.Text = brL + "READY" + brR;
                            else
                            {
                                if (ViewableSkillProgressMain.ProgressTextType == 0)
                                    progText.Text = string.Format(brL + "{0} / {1}" + brR, cT, aT);
                                else if (ViewableSkillProgressMain.ProgressTextType == 1)
                                    progText.Text = string.Format(brL + "{0}" + brR, aT - cT);
                                else
                                    progText.Text = string.Format(brL + "{0} / {1}" + brR, sT, aT);
                            }
                        }

                        secondColumn = !secondColumn;
                    }
                }
            }
        }
    }
}
