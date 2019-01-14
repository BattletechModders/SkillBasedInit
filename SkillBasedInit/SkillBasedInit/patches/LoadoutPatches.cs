using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using SkillBasedInit.Helper;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillBasedInit {

    // Sets the initiative value in the mech-bay
    [HarmonyPatch(typeof(MechBayMechInfoWidget), "SetInitiative")]
    [HarmonyPatch(new Type[] { })]
    public static class MechBayMechInfoWidget_SetInitiative {
        public static void Postfix(MechBayMechInfoWidget __instance, MechDef ___selectedMech,
            GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText, HBSTooltip ___initiativeTooltip) {

            if (___initiativeObj == null || ___initiativeText == null) {
                return; 
            }

            //SkillBasedInit.Logger.Log($"MechBayMechInfoWidget::SetInitiative::post - disabling text");
            if (___selectedMech == null) {
                ___initiativeObj.SetActive(true);
                ___initiativeText.SetText("{0}", new object[] { "-" });
            } else {
                List<string> details = new List<string>();

                // Static initiative from tonnage
                float tonnage = ___selectedMech.Chassis.Tonnage;
                int tonnageMod = UnitHelper.GetTonnageModifier(tonnage);
                details.Add($"Tonnage Base: {tonnageMod}");

                // Any modifiers that come from the chassis/mech/vehicle defs
                int componentsMod = UnitHelper.GetNormalizedComponentModifier(___selectedMech);
                if (componentsMod > 0) {
                    details.Add($"<space=2em><color=#00FF00>{componentsMod:+0} components</color>");
                } else if (componentsMod < 0) {
                    details.Add($"<space=2em><color=#FF0000>{componentsMod:0} components </color>");
                } 

                // Modifier from the engine
                int engineMod = UnitHelper.GetEngineModifier(___selectedMech);
                if (engineMod > 0) {
                    details.Add($"<space=2em><color=#00FF00>{engineMod:+0} engine</color>");
                } else if (engineMod < 0) {
                    details.Add($"<space=2em><color=#FF0000>{engineMod:0} engine</color>");
                } 

                // --- Badge ---
                ___initiativeObj.SetActive(true);
                ___initiativeText.SetText($"{tonnageMod + componentsMod + engineMod}");

                // --- Tooltip ---
                int maxInit = Math.Max(tonnageMod + componentsMod + engineMod, SkillBasedInit.MinPhase);
                details.Add($"Expected Phase: <b>{maxInit}</b> ");

                string tooltipTitle = $"{___selectedMech.Name}";
                string tooltipText = String.Join("\n", details.ToArray());
                BaseDescriptionDef initiativeData = new BaseDescriptionDef("MB_MIW_MECH_TT", tooltipTitle, tooltipText, null);
                ___initiativeTooltip.enabled = true;
                ___initiativeTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
            }
        }
    }

    // Sets the initiative value in the lance loading screen. Because we want it to be understandable to players, 
    //  invert the values so they reflect the phase IDs, not the actual value
    [HarmonyPatch(typeof(LanceLoadoutSlot), "RefreshInitiativeData")]
    [HarmonyPatch(new Type[] { })]
    public static class LanceLoadoutSlot_RefreshInitiativeData {
        public static void Postfix(LanceLoadoutSlot __instance, GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText,
            UIColorRefTracker ___initiativeColor, HBSTooltip ___initiativeTooltip, LanceConfiguratorPanel ___LC) {

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
                details.Add($"Tonnage Base: {tonnageMod}");

                // Any special modifiers by type - NA, Mech is the only type

                // Any modifiers that come from the chassis/mech/vehicle defs
                int componentsMod = UnitHelper.GetNormalizedComponentModifier(selectedMechDef);
                initValue += componentsMod;
                if (componentsMod > 0) {
                    details.Add($"<space=2em><color=#00FF00>{componentsMod:+0} components</color>");
                } else if (componentsMod < 0) {
                    details.Add($"<space=2em><color=#FF0000>{componentsMod:0} components</color>");
                } 

                // Modifier from the engine
                int engineMod = UnitHelper.GetEngineModifier(selectedMechDef);
                initValue += engineMod;
                if (engineMod > 0) {
                    details.Add($"<space=2em><color=#00FF00>{engineMod:+0} engine</color>");
                } else if (engineMod < 0) {
                    details.Add($"<space=2em><color=#FF0000>{engineMod:0} engine</color>");
                } 

                // --- PILOT ---
                Pilot selectedPilot = __instance.SelectedPilot.Pilot;

                int tacticsMod = PilotHelper.GetTacticsModifier(selectedPilot);
                details.Add($"<space=2em>{tacticsMod:+0} tactics");
                initValue += tacticsMod;

                int pilotTagsMod = PilotHelper.GetTagsModifier(selectedPilot);
                details.AddRange(PilotHelper.GetTagsModifierDetails(selectedPilot));
                initValue += pilotTagsMod;

                int[] randomnessBounds = PilotHelper.GetRandomnessBounds(selectedPilot);
                
                // --- LANCE ---
                if (___LC != null) {
                    initValue += ___LC.lanceInitiativeModifier;
                    if (___LC.lanceInitiativeModifier > 0) {
                        details.Add($"<space=2em><color=#00FF00>{___LC.lanceInitiativeModifier:+0} lance</color>");
                    } else if (___LC.lanceInitiativeModifier < 0) {
                        details.Add($"<space=2em><color=#FF0000>{___LC.lanceInitiativeModifier:0} lance</color>");
                    } 
                }

                // --- Badge ---
                ___initiativeText.SetText($"{initValue}");
                ___initiativeText.color = Color.black;
                ___initiativeColor.SetUIColor(UIColor.White);

                // --- Tooltip ---
                int maxInit = Math.Max(initValue - randomnessBounds[0], SkillBasedInit.MinPhase);
                int minInit = Math.Max(initValue - randomnessBounds[1], SkillBasedInit.MinPhase);
                details.Add($"Total:{initValue}");
                details.Add($"<space=2em><color=#FF0000>-{randomnessBounds[0]} to -{randomnessBounds[1]} randomness</color> (piloting)");                
                details.Add($"<b>Expected Phase<b>: {maxInit} to {minInit}");

                string tooltipTitle = $"{selectedMechDef.Name}: {selectedPilot.Name}";
                string tooltipText = String.Join("\n", details.ToArray());
                BaseDescriptionDef initiativeData = new BaseDescriptionDef("LLS_MECH_TT", tooltipTitle, tooltipText, null);
                ___initiativeTooltip.enabled = true;
                ___initiativeTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
            }
        }

    }
}