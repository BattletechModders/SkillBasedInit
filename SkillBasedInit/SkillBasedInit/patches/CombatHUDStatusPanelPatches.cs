using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using Localize;
using SkillBasedInit.Helper;
using SVGImporter;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SkillBasedInit.patches {
    [HarmonyPatch()]
    public static class CombatHUDStatusPanel_ShowActorStatuses {

        // Private method can't be patched by annotations, so use MethodInfo
        public static MethodInfo TargetMethod() {
            return AccessTools.Method(typeof(CombatHUDStatusPanel), "ShowActorStatuses", new Type[] { typeof(AbstractActor) });
        }

        // Display the initiative modifiers for the current unit as a buff that folks can hover over for details.
        public static void Postfix(CombatHUDStatusPanel __instance) {
            Mod.Log.LogIfDebug("___ CombatHUDStatusPanel:ShowActorStatuses:post - entered.");

            if (__instance.DisplayedCombatant != null) {

                AbstractActor actor = __instance.DisplayedCombatant as AbstractActor;
                bool isPlayer = actor.team == actor.Combat.LocalPlayerTeam;
                if (isPlayer) {
                    Type[] iconMethodParams = new Type[] { typeof(SVGAsset), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                    Traverse showBuffIconMethod = Traverse.Create(__instance).Method("ShowBuff", iconMethodParams);

                    Type[] stringMethodParams = new Type[] { typeof(string), typeof(Text), typeof(Text), typeof(Vector3), typeof(bool) };
                    Traverse showBuffStringMethod = Traverse.Create(__instance).Method("ShowBuff", stringMethodParams);

                    showBuffIconMethod.GetValue(new object[] {
                            LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.StatusSensorLockIcon,
                            new Text("INITIATIVE", new object[0]),
                            new Text(BuildTooltipText(actor)),
                            __instance.effectIconScale,
                            false
                        });

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
            mechDetails.Add($"MECH => tonnage:{tonnageMod}");
            int expectedInitMax = tonnageMod;

            // Any modifiers that come from the chassis/mech/vehicle defs
            int componentsMod = UnitHelper.GetNormalizedComponentModifier(actor);
            if (componentsMod > 0) {
                mechDetails.Add($"<color=#00FF00>{componentsMod:+0} components</color>");
            } else if (componentsMod < 0) {
                mechDetails.Add($"<color=#FF0000>{componentsMod:0} components</color>");
            }
            expectedInitMax += componentsMod;

            // Modifier from the engine
            int engineMod = UnitHelper.GetEngineModifier(actor);
            if (engineMod > 0) {
                mechDetails.Add($"<color=#00FF00>{engineMod:+0} engine</color>");
            } else if (engineMod < 0) {
                mechDetails.Add($"<color=#FF0000>{engineMod:0} engine</color>");
            }
            expectedInitMax += engineMod;

            // Check for leg / side loss
            Mech mech = (Mech)actor;
            if (mech.IsLocationDestroyed(ChassisLocations.LeftLeg) || mech.IsLocationDestroyed(ChassisLocations.RightLeg)) {
                int rawMod = Mod.Config.CrippledMovementModifier + actorInit.pilotingEffectMod;
                int penalty = Math.Min(-1, rawMod);
                mechDetails.Add($"<color=#FF0000>{penalty} Leg Destroyed</color>");
                expectedInitMax += penalty;
            }

            // Check for prone 
            if (actor.IsProne) {
                int rawMod = Mod.Config.ProneModifier + actorInit.pilotingEffectMod;
                int penalty = Math.Min(-1, rawMod);
                mechDetails.Add($"<color=#FF0000>{penalty} Prone</color>");
                expectedInitMax += penalty;
            }

            // Check for shutdown
            if (actor.IsShutDown) {
                int rawMod = Mod.Config.ShutdownModifier + actorInit.pilotingEffectMod;
                int penalty = Math.Min(-1, rawMod);
                mechDetails.Add($"<color=#FF0000>{penalty} Shutdown</color>");
                expectedInitMax += penalty;
            }

            // Check for melee impacts        
            if (actorInit.lastRoundMeleeMod > 0) {
                expectedInitMax -= actorInit.lastRoundMeleeMod;
                mechDetails.Add($"<color=#FF0000>-{actorInit.lastRoundMeleeMod} Melee Impact</color>");
            }

            // --- PILOT ---
            List<string> pilotDetails = new List<string>{};

            Pilot selectedPilot = actor.GetPilot();
            int tacticsMod = PilotHelper.GetTacticsModifier(selectedPilot);
            pilotDetails.Add($"PILOT => Tactics: <color=#00FF00>{tacticsMod:+0}</color>");
            expectedInitMax += tacticsMod;

            int pilotTagsMod = PilotHelper.GetTagsModifier(selectedPilot);
            pilotDetails.AddRange(PilotHelper.GetTagsModifierDetails(selectedPilot));
            expectedInitMax += pilotTagsMod;

            int[] randomnessBounds = PilotHelper.GetRandomnessBounds(selectedPilot);
            int expectedInitRandMin = randomnessBounds[0];
            int expectedInitRandMax = randomnessBounds[1];

            // Check for inspired status                
            if (actor.IsMoraleInspired || actor.IsFuryInspired) {
                pilotDetails.Add($"<color=#00FF00>+1 to +3 Inspired</color>");
                expectedInitRandMin += 1;
                expectedInitRandMin += 3;
            }

            // Check for injuries. If there injuries on the previous round, apply them in full force. Otherwise, reduce them.
            if (actorInit.lastRoundInjuryMod != 0) {
                pilotDetails.Add($"<color=#FF0000>-{actorInit.lastRoundInjuryMod} Fresh Injury</color>");
                expectedInitMax -= actorInit.lastRoundInjuryMod;
            } else if (actor.GetPilot().Injuries != 0) {
                // TODO: fold this in
                int rawPenalty = actorInit.CalculateInjuryPenalty(0, actor.GetPilot().Injuries);
                int penalty = (int)Math.Ceiling(rawPenalty / 2.0);
                pilotDetails.Add($"<color=#FF0000>-{penalty} Painful Injury</color>");
                expectedInitMax -= penalty;
            }

            // Check for an overly cautious player
            if (actorInit.lastRoundHesitationPenalty > 0) {
                expectedInitMax -= actorInit.lastRoundHesitationPenalty;
                pilotDetails.Add($"<color=#FF0000>-{actorInit.lastRoundHesitationPenalty} Hesitation</color>");
            }

            // Check for called shot
            if (actorInit.lastRoundCalledShotMod > 0) {
                expectedInitMax -= actorInit.lastRoundCalledShotMod;
                pilotDetails.Add($"<color=#FF0000>-{actorInit.lastRoundCalledShotMod} Called Shot Target</color>");
            }

            // Check for vigilance
            if (actorInit.lastRoundVigilanceMod > 0) {
                expectedInitMax -= actorInit.lastRoundVigilanceMod;
                pilotDetails.Add($"<color=#00FF00>-{actorInit.lastRoundVigilanceMod} Vigilance</color>");
            }

            // Finally, randomness bounds
            pilotDetails.Add($"\nRandom (tactics): <color=#FF0000>-{actorInit.randomnessBounds[0]} to -{actorInit.randomnessBounds[1]}</color>");

            int maxInit = Math.Max(expectedInitMax - expectedInitRandMin, Mod.MinPhase);
            int minInit = Math.Max(expectedInitMax - expectedInitRandMax, Mod.MinPhase);

            List<string> toolTipDetails = new List<string> { };
            toolTipDetails.Add(String.Join(", ", mechDetails.ToArray()));
            toolTipDetails.Add(String.Join(", ", pilotDetails.ToArray()));
            toolTipDetails.Add($"Expected Phase: <b>{maxInit} to {minInit}</b>");
            toolTipDetails.Add($"Hover over init in Mechlab/Deploy for details.");

            string tooltipText = String.Join("\n", toolTipDetails.ToArray());
            return tooltipText;
        }
    }
}
