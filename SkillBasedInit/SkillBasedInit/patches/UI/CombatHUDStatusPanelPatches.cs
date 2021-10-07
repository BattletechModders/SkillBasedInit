using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using IRBTModUtils.Extension;
using Localize;
using SkillBasedInit.Helper;
using SVGImporter;
using System;
using System.Collections.Generic;
using UnityEngine;
using us.frostraptor.modUtils;

namespace SkillBasedInit.patches {
    [HarmonyPatch(typeof(CombatHUDStatusPanel), "ShowActorStatuses")]
    public static class CombatHUDStatusPanel_ShowActorStatuses {

        // Display the initiative modifiers for the current unit as a buff that folks can hover over for details.
        public static void Postfix(CombatHUDStatusPanel __instance) {
            Mod.Log.Trace?.Write("___ CombatHUDStatusPanel:ShowActorStatuses:post - entered.");

            if (__instance.DisplayedCombatant != null) {

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {
                    Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                    Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                    DataManager dm = __instance.DisplayedCombatant.Combat.DataManager;
                    SVGAsset icon = dm.GetObjectOfType<SVGAsset>(Mod.Config.Icons.Stopwatch, BattleTechResourceType.SVGAsset);
                    Text tooltipText = new Text(BuildTooltipText(actor));
                    showBuffIconMethod.GetValue(new object[]
                        { icon, new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_TITLE]), tooltipText, __instance.effectIconScale, false }
                    );
                }
                
            }
        }

        private static string BuildTooltipText(AbstractActor actor) {
            
            Mod.Log.Debug?.Write($"Building tooltip for {actor.DistinctId()}");

            List<string> chassisDetails = new List<string> { };

            // Tonnage
            int tonnageMod = actor.StatCollection.GetValue<int>(ModStats.STATE_TONNAGE);
            chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_MECH_TONNAGE], new object[] { tonnageMod }).ToString());
            int expectedInitMax = tonnageMod;

            // Type modifier
            int typeMod = actor.StatCollection.GetValue<int>(ModStats.STATE_UNIT_TYPE);
            chassisDetails.Add(new Text(Mod.LocalizedText.MechBay[ModText.LT_TT_UNIT_TYPE], new object[] { typeMod }).ToString());

            if (actor.StatCollection.GetValue<int>(ModStats.MOD_INJURY) != 0)
            {
                // TODO: FIX
                // pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_PAINFUL_INJURY], new object[] { penalty }).ToString());
                // pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_FRESH_INJURY], new object[] { actorInit.lastRoundInjuryMod }).ToString());
            }

            if (actor.StatCollection.GetValue<int>(ModStats.MOD_MISC) != 0)
            {

            }

            if (actor.StatCollection.GetValue<int>(ModStats.STATE_CALLED_SHOT) != 0)
            {
                // pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_CALLED_SHOT_TARG], new object[] { actorInit.lastRoundCalledShotMod }).ToString());
            }

            if (actor.StatCollection.GetValue<int>(ModStats.STATE_VIGILIANCE) != 0)
            {
               // pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_VIGILANCE], new object[] { actorInit.lastRoundVigilanceMod }).ToString());
            }

            if (actor.StatCollection.GetValue<int>(ModStats.STATE_KNOCKDOWN) != 0)
            {

            }

            if (actor.StatCollection.GetValue<int>(ModStats.STATE_HESITATION) != 0)
            {
                //pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_HESITATION], new object[] { actorInit.lastRoundHesitationPenalty }).ToString());
            }

            int proneMod = actor.ProneInitModifier();
            if (proneMod != 0)
            {
                chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_LEG_DESTROYED], new object[] { proneMod }).ToString());
                expectedInitMax += proneMod;
            }

            int crippledMod = actor.CrippledInitModifier();
            if (crippledMod != 0)
            {
                chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_PRONE], new object[] { crippledMod }).ToString());
                expectedInitMax += crippledMod;
            }

            int shutdownMod =  actor.ShutdownInitModifier();
            if (shutdownMod != 0)
            {
                chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_SHUTDOWN], new object[] { shutdownMod }).ToString());
                expectedInitMax += shutdownMod;
            }


            // --- PILOT ---
            List<string> pilotDetails = new List<string> { };

            Pilot selectedPilot = actor.GetPilot();
            int tacticsMod = selectedPilot.CurrentSkillMod(selectedPilot.Tactics, ModStats.MOD_SKILL_TACTICS);
            pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_TACTICS], new object[] { tacticsMod }).ToString());
            expectedInitMax += tacticsMod;

            // TODO: Fix
            //int pilotTagsMod = actor.StatCollection.GetValue<int>(ModStats.STATE_PILOT_TAGS);
            //pilotDetails.AddRange(PilotHelper.GetTagsModifierDetails(selectedPilot));
            //expectedInitMax += pilotTagsMod;

            UnitCfg unitCfg = actor.GetUnitConfig();
            int[] randomnessBounds = selectedPilot.RandomnessBounds(unitCfg);
            int expectedInitRandMin = randomnessBounds[0];
            int expectedInitRandMax = randomnessBounds[1];

            // Check for inspired status                
            if (actor.IsMoraleInspired || actor.IsFuryInspired) {
                pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_INSPIRED]).ToString());
                expectedInitRandMin += unitCfg.InspiredMin;
                expectedInitRandMax += unitCfg.InspiredMax;
            }

            // Finally, randomness bounds
            pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_RANDOM], 
                new object[] { randomnessBounds[0], randomnessBounds[1] }).ToString());

            int maxInit = Math.Max(expectedInitMax - expectedInitRandMin, Mod.MinPhase);
            int minInit = Math.Max(expectedInitMax - expectedInitRandMax, Mod.MinPhase);

            List<string> toolTipDetails = new List<string> { };
            toolTipDetails.Add(String.Join(", ", chassisDetails.ToArray()));
            toolTipDetails.Add(String.Join(", ", pilotDetails.ToArray()));
            toolTipDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_EXPECTED], new object[] { maxInit, minInit }).ToString());
            toolTipDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_HOVER], new object[] { }).ToString());

            string tooltipText = String.Join("\n", toolTipDetails.ToArray());
            return tooltipText;
        }
    }
}
