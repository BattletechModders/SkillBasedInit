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
                details.Add($"<space=5em>Base: {tonnageMod}");

                // Any modifiers that come from the chassis/mech/vehicle defs
                int componentsMod = UnitHelper.GetNormalizedComponentModifier(___selectedMech);
                if (componentsMod > 0) {
                    details.Add($"<space=5em><color=#00FF00>Components: {componentsMod:+0}</color>");
                } else if (componentsMod < 0) {
                    details.Add($"<space=5em><color=#FF0000>Components: {componentsMod:0}</color>");
                } else {
                    details.Add($"<space=5em>Components: {componentsMod:+0}");
                }

                // Modifier from the engine
                int engineMod = UnitHelper.GetEngineModifier(___selectedMech);
                if (engineMod > 0) {
                    details.Add($"<space=5em><color=#00FF00>Engine: {engineMod:+0}</color>");
                } else if (engineMod < 0) {
                    details.Add($"<space=5em><color=#FF0000>Engine: {engineMod:0}</color>");
                } else {
                    details.Add($"<space=5em>Engine: {engineMod:+0}");
                }

                // --- Badge ---
                ___initiativeObj.SetActive(true);
                ___initiativeText.SetText($"{tonnageMod + componentsMod + engineMod}");

                // --- Tooltip ---
                int maxInit = (SkillBasedInit.MaxPhase + 1) - (tonnageMod + componentsMod);
                details.Add("---\n");
                details.Add($"Expected Phase: {maxInit} ");

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
                details.Add($"<space=5em>Base: {tonnageMod}");

                // Any special modifiers by type - NA, Mech is the only type

                // Any modifiers that come from the chassis/mech/vehicle defs
                int componentsMod = UnitHelper.GetNormalizedComponentModifier(selectedMechDef);
                initValue += componentsMod;
                if (componentsMod > 0) {
                    details.Add($"<space=5em><color=#00FF00>Components: {componentsMod:+0}</color>");
                } else if (componentsMod < 0) {
                    details.Add($"<space=5em><color=#FF0000>Components: {componentsMod:0}</color>");
                } else {
                    details.Add($"<space=5em>Components: {componentsMod:+0}");
                }

                // Modifier from the engine
                int engineMod = UnitHelper.GetEngineModifier(selectedMechDef);
                initValue += engineMod;
                if (engineMod > 0) {
                    details.Add($"<space=5em><color=#00FF00>Engine: {engineMod:+0}</color>");
                } else if (engineMod < 0) {
                    details.Add($"<space=5em><color=#FF0000>Engine: {engineMod:0}</color>");
                } else {
                    details.Add($"<space=5em>Engine: {engineMod:+0}");
                }

                // --- PILOT ---
                Pilot selectedPilot = __instance.SelectedPilot.Pilot;

                int tacticsMod = PilotHelper.GetTacticsModifier(selectedPilot);
                details.Add($"<space=5em>Tactics: {tacticsMod:+0}");
                initValue += tacticsMod;

                int pilotTagsMod = PilotHelper.GetTagsModifier(selectedPilot);
                details.Concat(PilotHelper.GetTagsModifierDetails(selectedPilot));

                int[] randomnessBounds = PilotHelper.GetRandomnessBounds(selectedPilot);
                details.Add($"<space=5em>Piloting: -{randomnessBounds[0]} to -{randomnessBounds[1]}");

                // --- LANCE ---
                if (___LC != null) {
                    initValue += ___LC.lanceInitiativeModifier;
                    if (___LC.lanceInitiativeModifier > 0) {
                        details.Add($"<space=5em><color=#00FF00>Lance: {___LC.lanceInitiativeModifier:+0}</color>");
                    } else if (___LC.lanceInitiativeModifier < 0) {
                        details.Add($"<space=5em><color=#FF0000>Lance: {___LC.lanceInitiativeModifier:0}</color>");
                    } 
                }

                // --- Badge ---
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
                BaseDescriptionDef initiativeData = new BaseDescriptionDef("LLS_MECH_TT", tooltipTitle, tooltipText, null);
                ___initiativeTooltip.enabled = true;
                ___initiativeTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
            }
        }

    }
}