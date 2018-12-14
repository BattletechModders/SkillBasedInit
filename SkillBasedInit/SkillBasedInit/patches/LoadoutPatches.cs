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
        public static void Postfix(MechBayMechInfoWidget __instance, MechDef ___selectedMech,
            GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText) {

            if (___initiativeObj == null || ___initiativeText == null) {
                return; 
            }

            //SkillBasedInit.Logger.Log($"MechBayMechInfoWidget::SetInitiative::post - disabling text");
            if (___selectedMech == null) {
                ___initiativeObj.SetActive(true);
                ___initiativeText.SetText("{0}", new object[] { "-" });
            } else {
                // Static initiative from tonnage
                float tonnage = ___selectedMech.Chassis.Tonnage;
                int tonnageMod = UnitHelper.GetTonnageModifier(tonnage);

                // Any modifiers that come from the chassis/mech/vehicle defs
                int componentsMod = UnitHelper.GetNormalizedComponentModifier(___selectedMech);

                // Modifier from the engine
                int engineMod = UnitHelper.GetEngineModifier(___selectedMech);

                ___initiativeObj.SetActive(true);
                ___initiativeText.SetText($"{tonnageMod + componentsMod + engineMod}");
            }
        }
    }

    /*
            public void SetInitiative ()
        {
            if (!((UnityEngine.Object)initiativeObj == (UnityEngine.Object)null) && !((UnityEngine.Object)initiativeText == (UnityEngine.Object)null)) {
                if (selectedMech == null) {
                    initiativeObj.SetActive (false);
                } else {
                    initiativeObj.SetActive (true);
                    int num = 0;
                    switch (selectedMech.Chassis.weightClass) {
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
                    initiativeText.SetText ("{0}", num);
                    if ((UnityEngine.Object)initiativeTooltip != (UnityEngine.Object)null) {
                        BaseDescriptionDef initiativeData = GetInitiativeData (selectedMech.Chassis.weightClass);
                        if (initiativeData != null) {
                            initiativeTooltip.enabled = true;
                            initiativeTooltip.SetDefaultStateData (TooltipUtilities.GetStateDataFromObject (initiativeData));
                        }
                    }
                }
            }
        }
     */

    // Sets the initiative value in the lance loading screen. Because we want it to be understandable to players, 
    //  invert the values so they reflect the phase IDs, not the actual value
    [HarmonyPatch(typeof(LanceLoadoutSlot), "RefreshInitiativeData")]
    [HarmonyPatch(new Type[] { })]
    public static class LanceLoadoutSlot_RefreshInitiativeData {
        public static void Postfix(LanceLoadoutSlot __instance, ref GameObject ___initiativeObj, ref TextMeshProUGUI ___initiativeText,
            ref UIColorRefTracker ___initiativeColor, HBSTooltip ___initiativeTooltip) {

            if (___initiativeObj == null || ___initiativeText == null || ___initiativeColor == null || ___initiativeTooltip == null) {
                return;
            }

            //SkillBasedInit.Logger.Log($"LanceLoadoutSlot::RefreshInitiativeData::post - disabling text");
            bool bothSelected = __instance.SelectedMech != null && __instance.SelectedPilot != null;
            if (!bothSelected) {
                ___initiativeText.SetText("{0}", new object[] { "-" });
                ___initiativeColor.SetUIColor(UIColor.MedGray);
            } else {
                int initValue = 0;

                // --- MECH ---
                MechDef selectedMechDef = __instance.SelectedMech.MechDef;
                List<string> details = new List<string>();

                // Static initiative from tonnage
                float tonnage = __instance.SelectedMech.MechDef.Chassis.Tonnage;
                int tonnageMod = UnitHelper.GetTonnageModifier(tonnage);
                initValue += tonnageMod;
                details.Add($"<space=5em>Base: {tonnageMod}");

                // Any special modifiers by type - NA, Mech is the only type

                // Any modifiers that come from the chassis/mech/vehicle defs
                int componentsMod = UnitHelper.GetNormalizedComponentModifier(selectedMechDef);
                initValue += componentsMod;
                if (componentsMod > 0) {
                    details.Add($"<space=5em>Components: {componentsMod}");
                } else if (componentsMod < 0) {
                    details.Add($"<space=5em>Components: {componentsMod}");
                } else {
                    details.Add($"<space=5em>Components: {componentsMod}");
                }

                // Modifier from the engine
                int engineMod = UnitHelper.GetEngineModifier(selectedMechDef);
                initValue += engineMod;
                if (engineMod > 0) {
                    details.Add($"<space=5em><color=#00FF00>Engine: {engineMod}</color>");
                } else if (engineMod < 0) {
                    details.Add($"<space=5em><color=#FF0000>Engine: {engineMod}</color>");
                } else {
                    details.Add($"<space=5em>Engine: {engineMod}");
                }

                // --- PILOT ---
                Pilot selectedPilot = __instance.SelectedPilot.Pilot;

                int tacticsMod = PilotHelper.GetTacticsModifier(selectedPilot);
                details.Add($"<space=5em>Tactics: {tacticsMod}");
                initValue += tacticsMod;

                int pilotTagsMod = PilotHelper.GetTagsModifier(selectedPilot);
                details.Concat(PilotHelper.GetTagsModifierDetails(selectedPilot));

                int[] randomnessBounds = PilotHelper.GetRandomnessBounds(selectedPilot);
                details.Add($"<space=5em>Piloting: -{randomnessBounds[0]} to -{randomnessBounds[1]}");

                ___initiativeText.SetText($"{initValue}");
                ___initiativeText.color = Color.black;
                ___initiativeColor.SetUIColor(UIColor.White);

                // --- Tooltip ---
                int maxInit = (SkillBasedInit.MaxPhase + 1) - (initValue - randomnessBounds[0]);
                int minInit = (SkillBasedInit.MaxPhase + 1) - (initValue - randomnessBounds[1]);
                details.Add("---\n");
                details.Add($"Expected Phase: {maxInit} to {minInit}");

                string tooltipTitle = $"{selectedMechDef.Name}: {selectedPilot.Name}";
                string tooltipText = String.Join("\n", details.ToArray());
                BaseDescriptionDef initiativeData = new BaseDescriptionDef("TEST_ID", tooltipTitle, tooltipText, null);
                ___initiativeTooltip.enabled = true;
                ___initiativeTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
            }
        }

        /* TODO:
                        int num3 = 0;
                        if ((Object)LC != (Object)null) {
                            num3 = LC.lanceInitiativeModifier;
                        }

                        -- Shows init from equipment that adds to lance mates
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