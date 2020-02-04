using BattleTech;
using BattleTech.Data;
using BattleTech.UI;
using Harmony;
using HBS;
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
            Mod.Log.Debug("___ CombatHUDStatusPanel:ShowActorStatuses:post - entered.");

            if (__instance.DisplayedCombatant != null) {

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {
                    Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                    Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                    DataManager dm = __instance.DisplayedCombatant.Combat.DataManager;
                    SVGAsset icon = dm.GetObjectOfType<SVGAsset>(Mod.Config.Icons.Stopwatch, BattleTechResourceType.SVGAsset);
                    showBuffIconMethod.GetValue(new object[]
                        { icon, new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TITLE]), new Text(BuildTooltipText(actor)), __instance.effectIconScale, false }
                    );
                }

            }
        }

        private static string BuildTooltipText(AbstractActor actor) {

            ActorInitiative actorInit = ActorInitiativeHolder.GetOrCreate(actor);

            // -- Mech
            List<string> mechDetails = new List<string> { };
            Mech actorMech = actor as Mech;
            float tonnage = actorMech.MechDef.Chassis.Tonnage;
            int tonnageMod = UnitHelper.GetTonnageModifier(tonnage);
            mechDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_MECH_TONNAGE], new object[] { tonnageMod }).ToString());
            int expectedInitMax = tonnageMod;

            // Any modifiers that come from the chassis/mech/vehicle defs
            int componentsMod = UnitHelper.GetNormalizedComponentModifier(actor);
            string compColor = componentsMod >= 0 ? "00FF00" : "FF0000";
            mechDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_COMPONENTS], new object[] { compColor, componentsMod }).ToString());
            expectedInitMax += componentsMod;

            // Modifier from the engine
            int engineMod = UnitHelper.GetEngineModifier(actor);
            string engineColor = engineMod >= 0 ? "00FF00" : "FF0000";
            mechDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_ENGINES], new object[] { engineColor, engineMod }).ToString());
            expectedInitMax += engineMod;

            // Check for leg / side loss
            Mech mech = (Mech)actor;
            if (mech.IsLocationDestroyed(ChassisLocations.LeftLeg) || mech.IsLocationDestroyed(ChassisLocations.RightLeg)) {
                int rawMod = Mod.Config.CrippledMovementModifier + actorInit.pilotingEffectMod;
                int penalty = Math.Min(-1, rawMod);
                mechDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_LEG_DESTROYED], new object[] { penalty }).ToString());
                expectedInitMax += penalty;
            }

            // Check for prone 
            if (actor.IsProne) {
                int rawMod = Mod.Config.ProneModifier + actorInit.pilotingEffectMod;
                int penalty = Math.Min(-1, rawMod);
                mechDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_PRONE], new object[] { penalty }).ToString());
                expectedInitMax += penalty;
            }

            // Check for shutdown
            if (actor.IsShutDown) {
                int rawMod = Mod.Config.ShutdownModifier + actorInit.pilotingEffectMod;
                int penalty = Math.Min(-1, rawMod);
                mechDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_SHUTDOWN], new object[] { penalty }).ToString());
                expectedInitMax += penalty;
            }

            // Check for melee impacts        
            if (actorInit.lastRoundMeleeMod > 0) {
                expectedInitMax -= actorInit.lastRoundMeleeMod;
                mechDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_MELEE_IMPACT], new object[] { actorInit.lastRoundMeleeMod }).ToString());
            }

            // --- PILOT ---
            List<string> pilotDetails = new List<string> { };

            Pilot selectedPilot = actor.GetPilot();
            int tacticsMod = PilotHelper.GetTacticsModifier(selectedPilot);
            pilotDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_TACTICS], new object[] { tacticsMod }).ToString());
            expectedInitMax += tacticsMod;

            int pilotTagsMod = PilotHelper.GetTagsModifier(selectedPilot);
            pilotDetails.AddRange(PilotHelper.GetTagsModifierDetails(selectedPilot));
            expectedInitMax += pilotTagsMod;

            int[] randomnessBounds = PilotHelper.GetRandomnessBounds(selectedPilot);
            int expectedInitRandMin = randomnessBounds[0];
            int expectedInitRandMax = randomnessBounds[1];

            // Check for inspired status                
            if (actor.IsMoraleInspired || actor.IsFuryInspired) {
                pilotDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_INSPIRED]).ToString());
                expectedInitRandMin += 1;
                expectedInitRandMin += 3;
            }

            // Check for injuries. If there injuries on the previous round, apply them in full force. Otherwise, reduce them.
            if (actorInit.lastRoundInjuryMod != 0) {
                pilotDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_FRESH_INJURY], new object[] { actorInit.lastRoundInjuryMod }).ToString());
                expectedInitMax -= actorInit.lastRoundInjuryMod;
            } else if (actor.GetPilot().Injuries != 0) {
                // TODO: fold this in
                int rawPenalty = actorInit.CalculateInjuryPenalty(0, actor.GetPilot().Injuries);
                int penalty = (int)Math.Ceiling(rawPenalty / 2.0);
                pilotDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_PAINFUL_INJURY], new object[] { penalty }).ToString());
                expectedInitMax -= penalty;
            }

            // Check for an overly cautious player
            if (actorInit.lastRoundHesitationPenalty > 0) {
                expectedInitMax -= actorInit.lastRoundHesitationPenalty;
                pilotDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_HESITATION], new object[] { actorInit.lastRoundHesitationPenalty }).ToString());
            }

            // Check for called shot
            if (actorInit.lastRoundCalledShotMod > 0) {
                expectedInitMax -= actorInit.lastRoundCalledShotMod;
                pilotDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_CALLED_SHOT_TARG], new object[] { actorInit.lastRoundCalledShotMod }).ToString());
            }

            // Check for vigilance
            if (actorInit.lastRoundVigilanceMod > 0) {
                expectedInitMax -= actorInit.lastRoundVigilanceMod;
                pilotDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_VIGILANCE], new object[] { actorInit.lastRoundVigilanceMod }).ToString());
            }

            // Finally, randomness bounds
            pilotDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_RANDOM], 
                new object[] { actorInit.randomnessBounds[0], actorInit.randomnessBounds[1] }).ToString());

            int maxInit = Math.Max(expectedInitMax - expectedInitRandMin, Mod.MinPhase);
            int minInit = Math.Max(expectedInitMax - expectedInitRandMax, Mod.MinPhase);

            List<string> toolTipDetails = new List<string> { };
            toolTipDetails.Add(String.Join(", ", mechDetails.ToArray()));
            toolTipDetails.Add(String.Join(", ", pilotDetails.ToArray()));
            toolTipDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_EXPECTED], new object[] { maxInit, minInit }).ToString());
            toolTipDetails.Add(new Text(Mod.Config.LocalizedText[ModConfig.LT_TT_HOVER], new object[] { }).ToString());

            string tooltipText = String.Join("\n", toolTipDetails.ToArray());
            return tooltipText;
        }
    }
}
