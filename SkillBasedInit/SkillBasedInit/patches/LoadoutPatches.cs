using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using SkillBasedInit.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace SkillBasedInit {

    // Sets the initiative value in the mech-bay
    [HarmonyPatch(typeof(MechBayMechInfoWidget), "SetInitiative")]
    [HarmonyPatch(new Type[] { })]
    public static class MechBayMechInfoWidget_SetInitiative {
        public static void Postfix(MechBayMechInfoWidget __instance, GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText) {
            //SkillBasedInit.Logger.Log($"MechBayMechInfoWidget::SetInitiative::post - disabling text");
            if (___initiativeObj.activeSelf) {
                ___initiativeText.SetText("{0}", new object[] { "-" });
            }
        }
    }

    // Sets the initiative value in the lance loading screen. Because we want it to be understandable to players, 
    //  invert the values so they reflect the phase IDs, not the actual value
    [HarmonyPatch(typeof(LanceLoadoutSlot), "RefreshInitiativeData")]
    [HarmonyPatch(new Type[] { })]
    public static class LanceLoadoutSlot_RefreshInitiativeData {
        public static void Postfix(LanceLoadoutSlot __instance, ref GameObject ___initiativeObj, ref TextMeshProUGUI ___initiativeText,
            ref UIColorRefTracker ___initiativeColor, HBSTooltip ___initiativeTooltip) {
            SkillBasedInit.Logger.Log($"LanceLoadoutSlot::RefreshInitiativeData::post - disabling text");
            if (___initiativeObj != null && ___initiativeText != null && ___initiativeObj.activeSelf) {
                ___initiativeText.SetText("{0}", new object[] { "-" });
            }

            bool bothSelected = __instance.SelectedMech != null && __instance.SelectedPilot != null;

            string mechLabel = null;
            List<string> mechModsDescs = new List<string>();
            int mechMod = 99;
            if (__instance.SelectedMech != null) {
                SkillBasedInit.Logger.Log($"LanceLoadoutSlot::RefreshInitiativeData::post - there is a selected mech!");
                MechDef selectedMechDef = __instance.SelectedMech.MechDef;
                mechLabel = selectedMechDef.Name;
                mechMod = 0;

                // Static initiative from tonnage
                float tonnage = __instance.SelectedMech.MechDef.Chassis.Tonnage;
                int tonnageMod = UnitHelper.GetTonnageModifier(tonnage);
                mechMod += tonnageMod;
                mechModsDescs.Add($"<space=5em>Base: {tonnageMod}");

                // Any special modifiers by type - NA, Mech is the only type

                // Any modifiers that come from the chassis/mech/vehicle defs
                int componentsMod = UnitHelper.GetNormalizedComponentModifier(selectedMechDef);
                mechMod += componentsMod;
                if (componentsMod > 0) {
                    mechModsDescs.Add($"<space=5em>Components: {componentsMod}");
                } else if (componentsMod < 0) {
                    mechModsDescs.Add($"<space=5em>Components: {componentsMod}");
                } else {
                    mechModsDescs.Add($"<space=5em>Components: {componentsMod}");
                }

                // Modifier from the engine
                int engineMod = UnitHelper.GetEngineModifier(selectedMechDef);
                mechMod += engineMod;
                if (componentsMod > 0) {
                    mechModsDescs.Add($"<space=5em>Engine: +{engineMod}");
                } else if (componentsMod < 0) {
                    mechModsDescs.Add($"<space=5em>Engine: {engineMod}");
                } else {
                    mechModsDescs.Add($"<space=5em>Engine: +{engineMod}");
                }

                if (!bothSelected) {
                    ___initiativeObj.SetActive(true);
                    ___initiativeText.fontSize = 20;
                    ___initiativeText.enableWordWrapping = false;
                    ___initiativeText.SetText($"{mechMod}");
                    ___initiativeColor.SetUIColor(UIColor.MedGray);
                }
            }

            string pilotLabel = null;
            List<string> pilotModsDescs = new List<string>();
            int[] pilotMod = { 99, 99 };
            if (__instance.SelectedPilot != null) {
                SkillBasedInit.Logger.Log($"LanceLoadoutSlot::RefreshInitiativeData::post - there is a selected pilot!");

                Pilot selectedPilot = __instance.SelectedPilot.Pilot;
                pilotLabel = selectedPilot.Callsign;
                int tacticsMod = PilotHelper.GetTacticsModifier(selectedPilot);
                pilotModsDescs.Add($"<space=5em>Tactics: {tacticsMod}");

                int pilotTagsMod = PilotHelper.GetTagsModifier(selectedPilot);

                int[] randomnessBounds = PilotHelper.GetRandomnessBounds(selectedPilot);
                pilotModsDescs.Add($"<space=5em>Piloting: -{randomnessBounds[0]} to -{randomnessBounds[1]}");

                pilotMod[0] = tacticsMod + pilotTagsMod + randomnessBounds[0];
                pilotMod[1] = tacticsMod + pilotTagsMod + randomnessBounds[1];

                if (!bothSelected) {
                    ___initiativeObj.SetActive(true);
                    ___initiativeText.fontSize = 12;
                    ___initiativeText.enableWordWrapping = false;
                    ___initiativeText.color = Color.white;
                    ___initiativeText.SetText($"{pilotMod[0]}-{pilotMod[1]}");
                    ___initiativeColor.SetUIColor(UIColor.Red);
                }
            }

            List<string> expectedPhase = new List<string> { "\n---\n" };
            if (bothSelected) {
                int max = mechMod + pilotMod[0];
                int min = mechMod + pilotMod[1];

                expectedPhase.Add($"Expected Phase: {SkillBasedInit.MaxPhase - 1 - max} to {SkillBasedInit.MaxPhase - 1 - min}");

                ___initiativeObj.SetActive(true);
                ___initiativeText.fontSize = 12;
                ___initiativeText.enableWordWrapping = false;
                ___initiativeText.color = Color.white;
                ___initiativeText.SetText($"{max}-{min}");
                ___initiativeColor.SetUIColor(UIColor.Gold);
            }

            if (___initiativeTooltip != null) {
                string title = "Unknown";
                if (bothSelected) {
                    title = $"{mechLabel}:{pilotLabel}";
                } else if (mechLabel != null) {
                    title = $"{mechLabel}:?";
                } else if (pilotLabel != null) {
                    title = $"?:{pilotLabel}";
                }

                List<string> allDetails = mechModsDescs.Concat(pilotModsDescs)
                    .Concat(expectedPhase)
                    .ToList();
                string details = String.Join("\n", allDetails.ToArray());

                BaseDescriptionDef initiativeData = new BaseDescriptionDef("TEST_ID", title, details, null);
                    
                if (initiativeData != null) {
                    ___initiativeTooltip.enabled = true;
                    ___initiativeText.color = Color.white;
                    ___initiativeTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
                }
            }
        }

        /*
         
        private BaseDescriptionDef GetInitiativeData (WeightClass weight)
        {
            string text = null;
            switch (weight) {
            case WeightClass.ASSAULT:
                text = "ConceptMechWeightAssault";
                break;
            case WeightClass.HEAVY:
                text = "ConceptMechWeightHeavy";
                break;
            case WeightClass.LIGHT:
                text = "ConceptMechWeightLight";
                break;
            case WeightClass.MEDIUM:
                text = "ConceptMechWeightMedium";
                break;
            }
            BaseDescriptionDef baseDescriptionDef = new BaseDescriptionDef ();
            if (!string.IsNullOrEmpty (text) && UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Exists (text)) {
                return UnityGameInstance.BattleTechGame.DataManager.BaseDescriptionDefs.Get (text);
            }
            return null;
        }
         */

        /* TODO:
                            int num3 = 0;
                        if ((Object)LC != (Object)null) {
                            num3 = LC.lanceInitiativeModifier;
                        }
                        num2 += num3;
                        int num4 = Mathf.Clamp (num + num2, 1, 5);
                        initiativeText.SetText ($"{num4}");
                        if ((Object)initiativeTooltip != (Object)null) {
                            BaseDescriptionDef initiativeData = GetInitiativeData (SelectedMech.MechDef.Chassis.weightClass);
                            if (initiativeData != null) {
                                initiativeTooltip.enabled = true;
                                initiativeTooltip.SetDefaultStateData (TooltipUtilities.GetStateDataFromObject (initiativeData));
                            }
                        }
         */

        /*
            if (!((Object)initiativeObj == (Object)null) && !((Object)initiativeText == (Object)null) && !((Object)initiativeColor == (Object)null)) {
                    if ((Object)SelectedMech == (Object)null || SelectedMech.MechDef == null || SelectedMech.MechDef.Chassis == null) {
                        initiativeObj.SetActive (false);
                    } else if ((Object)SelectedPilot == (Object)null || SelectedPilot.Pilot == null || SelectedPilot.Pilot.pilotDef == null) {
                        initiativeObj.SetActive (false);
                    } else {
                        initiativeObj.SetActive (true);
                        int num = 0;
                        int num2 = 0;
                        switch (SelectedMech.MechDef.Chassis.weightClass) {
                        case WeightClass.LIGHT:
                            num = 4;
                            break;
                        case WeightClass.MEDIUM:
                            num = 3;
                            break;
                        case WeightClass.HEAVY:
                            num = 2;
                            break;
                        case WeightClass.ASSAULT:
                            num = 1;
                            break;
                        }
                        if (SelectedPilot.Pilot.pilotDef.AbilityDefs != null) {
                            foreach (AbilityDef abilityDef in SelectedPilot.Pilot.pilotDef.AbilityDefs) {
                                foreach (EffectData effectDatum in abilityDef.EffectData) {
                                    if (MechStatisticsRules.GetInitiativeModifierFromEffectData (effectDatum, true, null) == 0) {
                                        num2 += MechStatisticsRules.GetInitiativeModifierFromEffectData (effectDatum, false, null);
                                    }
                                }
                            }
                        }
                        if (selectedMech.MechDef.Inventory != null) {
                            MechComponentRef[] inventory = SelectedMech.MechDef.Inventory;
                            foreach (MechComponentRef mechComponentRef in inventory) {
                                if (mechComponentRef.Def != null && mechComponentRef.Def.statusEffects != null) {
                                    EffectData[] statusEffects = mechComponentRef.Def.statusEffects;
                                    foreach (EffectData effect in statusEffects) {
                                        if (MechStatisticsRules.GetInitiativeModifierFromEffectData (effect, true, null) == 0) {
                                            num2 += MechStatisticsRules.GetInitiativeModifierFromEffectData (effect, false, null);
                                        }
                                    }
                                }
                            }
                        }
                        int num3 = 0;
                        if ((Object)LC != (Object)null) {
                            num3 = LC.lanceInitiativeModifier;
                        }
                        num2 += num3;
                        int num4 = Mathf.Clamp (num + num2, 1, 5);
                        initiativeText.SetText ($"{num4}");
                        if ((Object)initiativeTooltip != (Object)null) {
                            BaseDescriptionDef initiativeData = GetInitiativeData (SelectedMech.MechDef.Chassis.weightClass);
                            if (initiativeData != null) {
                                initiativeTooltip.enabled = true;
                                initiativeTooltip.SetDefaultStateData (TooltipUtilities.GetStateDataFromObject (initiativeData));
                            }
                        }
                        initiativeColor.SetUIColor ((num2 <= 0) ? ((num2 >= 0) ? UIColor.White : UIColor.Red) : UIColor.Gold);
                    }
                }

         */

    }
}