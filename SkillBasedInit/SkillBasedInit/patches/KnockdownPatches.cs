using BattleTech;
using Harmony;
using System;

namespace SkillBasedInit {

   
    [HarmonyPatch(typeof(Mech), "CompleteKnockdown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    public static class Mech_CompleteKnockdown {

        public static string KnockdownSourceID;
        public static int KnockdownStackItemUID;

        public static bool Prefix(Mech __instance, string sourceID, int stackItemUID) {
            KnockdownSourceID = sourceID;
            KnockdownStackItemUID = stackItemUID;
            SkillBasedInit.LogDebug($"Mech:CompleteKnockdown:prefix - Recording sourceID:{sourceID} stackItemUID:{stackItemUID}");
            return true;
        }

        public static void Postfix(Mech __instance) {
            SkillBasedInit.LogDebug($"Mech:CompleteKnockdown:postfix - Removing sourceID:{KnockdownSourceID} stackItemUID:{KnockdownStackItemUID}.");
            KnockdownSourceID = null;
            KnockdownStackItemUID = -1;
        }
    }

    // Required to apply the negative as soon as it happens in the knockdown case
    [HarmonyPatch(typeof(AbstractActor), "ForceUnitOnePhaseDown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public static class AbstractActor_ForceUnitOnePhaseDown {
        public static bool Prefix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            //SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - Init - sourceID:{sourceID} vs actor: {__instance.GUID}");
            bool shouldReturn;
            if (sourceID != Mech_CompleteKnockdown.KnockdownSourceID || stackItemUID != Mech_CompleteKnockdown.KnockdownStackItemUID) {
                //SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - Not from knockdown, deferring to original call");
                shouldReturn = true;
            } else {
                SkillBasedInit.LogDebug($"AbstractActor:ForceUnitOnePhaseDown:prefix - Source is knockdown, executing changed logic");
                shouldReturn = false;

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                int knockDownMod = SkillBasedInit.Random.Next(actorInit.injuryBounds[0], actorInit.injuryBounds[1]);
                SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown modifying Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) initiative by {knockDownMod} due to knockdown!");

                if (__instance.HasActivatedThisRound || __instance.Initiative != SkillBasedInit.MaxPhase) {
                    SkillBasedInit.Logger.Log($"Knockdown will slow Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) by {knockDownMod} init on next activation!");
                } else {
                    SkillBasedInit.Logger.Log($"Knockdown immediately slows Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) by {knockDownMod} init!");
                    if (__instance.Combat.TurnDirector.IsInterleaved && __instance.Initiative != SkillBasedInit.MaxPhase) {
                        __instance.Initiative = __instance.Initiative + knockDownMod;
                        if (__instance.Initiative > SkillBasedInit.MaxPhase) {
                            __instance.Initiative = SkillBasedInit.MaxPhase;
                        } 
                        __instance.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(__instance.GUID));
                    }
                }
                __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, $"THUD! -{knockDownMod} INITIATIVE", FloatieMessage.MessageNature.Debuff));

                // TODO: Is this causing the lockup?
                string statName = (!addedBySelf) ? "PhaseModifier" : "PhaseModifierSelf";
                __instance.StatCollection.ModifyStat<int>(sourceID, stackItemUID, statName, StatCollection.StatOperation.Int_Add, knockDownMod, -1, true);

            }            
            return shouldReturn;
        }
    }

}
