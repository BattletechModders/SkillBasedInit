using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using Localize;
using SkillBasedInit.Helper;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillBasedInit.Patches {

    // Sets the initiative value in the mech-bay
    [HarmonyPatch(typeof(MechBayMechInfoWidget), "SetInitiative")]
    [HarmonyPatch(new Type[] { })]
    public static class MechBayMechInfoWidget_SetInitiative {
        public static void Postfix(MechBayMechInfoWidget __instance, MechDef ___selectedMech,
            GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText, HBSTooltip ___initiativeTooltip) {
            Mod.Log.Trace?.Write("MBMIW:SI:post - entered.");

            if (___initiativeObj == null || ___initiativeText == null) {
                return;
            }

            //SkillBasedInit.Logger.Log($"MechBayMechInfoWidget::SetInitiative::post - disabling text");
            if (___selectedMech == null) {
                ___initiativeObj.SetActive(true);
                ___initiativeText.SetText("-");
            } else {
                List<string> details = new List<string>();

                // Static initiative from tonnage
                int tonnageMod = ___selectedMech.GetTonnageModifier();
                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_TONNAGE], new object[] { tonnageMod }).ToString());

                // Type modifier
                int typeMod = ___selectedMech.GetTypeModifier();
                string typeModColor = typeMod >= 0 ? "00FF00" : "FF0000";                
                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_UNIT_TYPE], new object[] { typeModColor, typeMod }).ToString());

                // Any modifiers that come from the chassis/mech/vehicle defs
                //int componentsMod = UnitHelper.GetNormalizedComponentModifier(___selectedMech);
                //string compsColor = componentsMod >= 0 ? "00FF00" : "FF0000";
                // details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_COMPONENTS], new object[] { compsColor, componentsMod }).ToString());

                // Modifier from the engine
                // int engineMod = UnitHelper.GetEngineModifier(___selectedMech);
                //string engineColor = engineMod >= 0 ? "00FF00" : "FF0000";
                // details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_ENGINES], new object[] { engineColor, engineMod }).ToString());

                // --- Badge ---
                ___initiativeObj.SetActive(true);
                ___initiativeText.SetText($"{tonnageMod + typeMod}");

                // --- Tooltip ---
                int maxInit = Math.Max(tonnageMod + typeMod, Mod.MinPhase);
                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_EXPECTED_NO_PILOT], new object[] { maxInit }).ToString());

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
            Mod.Log.Trace?.Write("LLS:RID:post - entered.");

            if (___initiativeObj == null || ___initiativeText == null || ___initiativeColor == null || ___initiativeTooltip == null) {
                return;
            }
            
            //SkillBasedInit.Logger.Log($"LanceLoadoutSlot::RefreshInitiativeData::post - disabling text");
            bool bothSelected = __instance.SelectedMech != null && __instance.SelectedPilot != null;
            if (!bothSelected) {
                ___initiativeText.SetText("-");
                ___initiativeColor.SetUIColor(UIColor.MedGray);
            } else {
                Mod.Log.Debug?.Write($"Building init data for mechdef: {__instance.SelectedMech.name} with pilot: {__instance.SelectedPilot.name}");

                int initValue = 0;

                // --- MECH ---
                MechDef selectedMechDef = __instance.SelectedMech.MechDef;
                List<string> details = new List<string>();

                // Static initiative from tonnage
                int tonnageMod = __instance.SelectedMech.MechDef.GetTonnageModifier();
                initValue += tonnageMod;
                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_TONNAGE], new object[] { tonnageMod }).ToString());

                // Check tags
                int typeMod = __instance.SelectedMech.MechDef.GetTypeModifier();
                initValue += typeMod;
                string typeModColor = typeMod >= 0 ? "00FF00" : "FF0000";
                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_UNIT_TYPE], new object[] { typeModColor, typeMod }).ToString());

                // --- PILOT ---
                Pilot selectedPilot = __instance.SelectedPilot.Pilot;
                // TODO: Iterate abilities, look for SBI stats, and apply that logic to the tooltip display

                int tacticsMod = selectedPilot.SBITacticsMod();
                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_TACTICS], new object[] { tacticsMod }).ToString());
                initValue += tacticsMod;

                int pilotTagsMod = PilotHelper.GetTagsModifier(selectedPilot);
                details.AddRange(PilotHelper.GetTagsModifierDetails(selectedPilot));
                initValue += pilotTagsMod;

                UnitCfg unitCfg = __instance.SelectedMech.MechDef.GetUnitConfig();
                int[] randomnessBounds = selectedPilot.RandomnessBounds(unitCfg);

                // --- LANCE ---
                if (___LC != null) {
                    initValue += ___LC.lanceInitiativeModifier;
                    string lanceColor = ___LC.lanceInitiativeModifier >= 0 ? "00FF00" : "FF0000";
                    details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_LANCE], new object[] { lanceColor, ___LC.lanceInitiativeModifier }).ToString());
                }

                // --- Tooltip ---
                int maxInit = Math.Max(initValue - randomnessBounds[0], Mod.MinPhase);
                int minInit = Math.Max(initValue - randomnessBounds[1], Mod.MinPhase);
                int avgMod = (int)Math.Ceiling((maxInit - minInit) / 2f);
                int avgInit = maxInit - avgMod;

                // --- Badge ---
                ___initiativeText.SetText($"{avgInit}");
                ___initiativeText.color = Color.black;
                ___initiativeColor.SetUIColor(UIColor.White);

                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_TOTAL], new object[] { initValue }).ToString());
                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_RANDOM], new object[] { randomnessBounds[0], randomnessBounds[1] }).ToString());
                details.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_MB_EXPECTED], new object[] { maxInit, minInit }).ToString());

                string tooltipTitle = $"{selectedMechDef.Name}: {selectedPilot.Name}";
                string tooltipText = String.Join("\n", details.ToArray());
                BaseDescriptionDef initiativeData = new BaseDescriptionDef("LLS_MECH_TT", tooltipTitle, tooltipText, null);
                ___initiativeTooltip.enabled = true;
                ___initiativeTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
            }
        }

    }
}