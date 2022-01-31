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

            // MODS SHOWN AS ACTOR
            UnitCfg unitCfg = actor.GetUnitConfig();

            // Tonnage
            int tonnageMod = actor.StatCollection.GetValue<int>(ModStats.STATE_TONNAGE);
            chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_MECH_TONNAGE], new object[] { tonnageMod }).ToString());
            int initiativeBase = tonnageMod;

            // Type modifier
            int typeMod = actor.StatCollection.GetValue<int>(ModStats.STATE_UNIT_TYPE);
            initiativeBase += typeMod;
            string typeModColor = typeMod >= 0 ? "00FF00" : "FF0000";
            chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_UNIT_TYPE], new object[] { typeModColor, typeMod }).ToString());

            if (actor.StatCollection.GetValue<int>(ModStats.MOD_MISC) != 0)
            {
                int miscMod = actor.StatCollection.GetValue<int>(ModStats.MOD_MISC);
                initiativeBase += miscMod;
                string miscModColor = miscMod >= 0 ? "00FF00" : "FF0000";
                chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_COMPONENTS], new object[] { miscModColor, miscMod }).ToString());
            }

            int proneMod = actor.ProneInitModifier();
            if (proneMod != 0)
            {
                chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_CRIPPLED], new object[] { proneMod }).ToString());
                initiativeBase += proneMod;
            }

            int crippledMod = actor.CrippledInitModifier();
            if (crippledMod != 0)
            {
                chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_PRONE], new object[] { crippledMod }).ToString());
                initiativeBase += crippledMod;
            }

            int shutdownMod =  actor.ShutdownInitModifier();
            if (shutdownMod != 0)
            {
                chassisDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_SHUTDOWN], new object[] { shutdownMod }).ToString());
                initiativeBase += shutdownMod;
            }
            Mod.Log.Debug?.Write($"  proneMod: {proneMod}  crippledMod: {crippledMod}  shutdownMod: {shutdownMod}");

            // MODS SHOWN AS PILOT
            List<string> pilotDetails = new List<string> { };

            Pilot selectedPilot = actor.GetPilot();
            if (selectedPilot != null)
            {
                int tacticsMod = selectedPilot.SBITacticsMod();
                initiativeBase -= tacticsMod;
                pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_TACTICS], new object[] { tacticsMod }).ToString());
                Mod.Log.Debug?.Write($"  tacticsMod: {tacticsMod}");

                if (actor.StatCollection.GetValue<int>(ModStats.STATE_PILOT_TAGS) != 0)
                {
                    int pilotTagsMods = actor.StatCollection.GetValue<int>(ModStats.STATE_PILOT_TAGS);
                    initiativeBase += pilotTagsMods;
                    string pilotTagsColor = pilotTagsMods >= 0 ? "00FF00" : "FF0000";
                    pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_PILOT_TAGS], new object[] { pilotTagsColor, pilotTagsMods }).ToString());
                }

                int inspiredMod = selectedPilot.InspiredModifier(unitCfg);
                if (inspiredMod != 0)
                {
                    initiativeBase += inspiredMod;
                    pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_INSPIRED], new object[] { inspiredMod }).ToString());
                    Mod.Log.Debug?.Write($"  inspiredMod: {tacticsMod}");
                }

                if (actor.StatCollection.GetValue<int>(ModStats.MOD_INJURY) != 0)
                {
                    int injuryMod = actor.StatCollection.GetValue<int>(ModStats.MOD_INJURY);
                    initiativeBase += injuryMod;
                    string injuryModColor = injuryMod >= 0 ? "00FF00" : "FF0000";
                    pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_INJURY], new object[] { injuryModColor, injuryMod }).ToString());
                }
            }

            if (actor.StatCollection.GetValue<int>(ModStats.STATE_HESITATION) != 0)
            {
                int hestiateMod = actor.StatCollection.GetValue<int>(ModStats.STATE_HESITATION);
                Mod.Log.Debug?.Write($"  hestiationMod: {hestiateMod}");
                initiativeBase += hestiateMod;
                pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_HESITATION], new object[] { hestiateMod }).ToString());
            }

            if (actor.StatCollection.GetValue<int>(ModStats.STATE_CALLED_SHOT) != 0)
            {
                int calledShotMod = actor.StatCollection.GetValue<int>(ModStats.STATE_CALLED_SHOT);
                Mod.Log.Debug?.Write($"  hestiationMod: {calledShotMod}");
                initiativeBase += calledShotMod;
                pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_CALLED_SHOT], new object[] { calledShotMod }).ToString());
            }

            if (actor.StatCollection.GetValue<int>(ModStats.STATE_VIGILIANCE) != 0)
            {
                int vigilanceMod = actor.StatCollection.GetValue<int>(ModStats.STATE_VIGILIANCE);
                Mod.Log.Debug?.Write($"  hestiationMod: {vigilanceMod}");
                initiativeBase += vigilanceMod;
                pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_VIGILANCE], new object[] { vigilanceMod }).ToString());
            }

            int[] randomnessBounds = selectedPilot.RandomnessBounds(unitCfg.RandomnessMin, unitCfg.RandomnessMax);
            pilotDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_RANDOM],
                new object[] { randomnessBounds[0], randomnessBounds[1] }).ToString());

            Mod.Log.Debug?.Write($"randomnessBounds => Min: {randomnessBounds[0]}  Max: {randomnessBounds[1]}");

            // Finally, randomness bounds. These should be negatives
            int minRandomMod = randomnessBounds[0];
            int maxRandomMod = randomnessBounds[1];
            
            // Highest init = base + minimumRandomness => base + minRandom
            int maxPhase = Math.Min(initiativeBase + minRandomMod, Mod.MaxPhase);
            Mod.Log.Debug?.Write($"  initiativeBase: {initiativeBase} + minRandomMod: {minRandomMod} = maxInit: {maxPhase}");

            // Lowest init = base + maxRandom => base + maxRandom
            int minPhase = Math.Max(initiativeBase + maxRandomMod, Mod.MinPhase);
            Mod.Log.Debug?.Write($"  initiativeBase: {initiativeBase} + maxRandomMod: {maxRandomMod} = minInit: {minPhase}");

            List<string> toolTipDetails = new List<string> { };
            toolTipDetails.Add(String.Join(", ", chassisDetails.ToArray()));
            toolTipDetails.Add(String.Join(", ", pilotDetails.ToArray()));
            toolTipDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_EXPECTED], new object[] { maxPhase, minPhase }).ToString());
            toolTipDetails.Add(new Text(Mod.LocalizedText.Tooltip[ModText.LT_TT_HOVER], new object[] { }).ToString());

            string tooltipText = String.Join("\n", toolTipDetails.ToArray());
            return tooltipText;
        }
    }
}
