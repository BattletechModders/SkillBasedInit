using BattleTech;
using Harmony;
using SkillBasedInit.Helper;
using System;

namespace SkillBasedInit {

   
    [HarmonyPatch(typeof(Mech), "CompleteKnockdown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    public static class Mech_CompleteKnockdown {

        public static string KnockdownSourceID;
        public static int KnockdownStackItemUID;
        // We temporarily set the actorInit to PhaseAssault to trick the original method into not firing, then do our work in a postfix.
        //   We have to persist the actorInit during this time.
        public static int ActorInitiative;

        public static void Prefix(Mech __instance, string sourceID, int stackItemUID) {
            KnockdownSourceID = sourceID;
            KnockdownStackItemUID = stackItemUID;
            ActorInitiative = __instance.Initiative;
            SkillBasedInit.LogDebug($"Mech:CompleteKnockdown:prefix - Recording sourceID:{sourceID} stackItemUID:{stackItemUID}");            
        }

        public static void Postfix(Mech __instance) {
            SkillBasedInit.LogDebug($"Mech:CompleteKnockdown:postfix - Removing sourceID:{KnockdownSourceID} stackItemUID:{KnockdownStackItemUID}.");
            KnockdownSourceID = null;
            KnockdownStackItemUID = -1;
            __instance.Initiative = -1;
        }
    }

    [HarmonyPatch(typeof(AttackStackSequence), "OnAdded")]
    [HarmonyPatch(new Type[] { })]
    public static class AttackStackSequence_OnAdded {

        public static bool IsMoraleAttack = false;

        public static void Prefix(AttackStackSequence __instance) {
            IsMoraleAttack = __instance.isMoraleAttack;
            SkillBasedInit.LogDebug($"AttackStackSequence:OnAdded:prefix - Recording IsMoraleAttack: {IsMoraleAttack}");
        }

        public static void Postfix(AttackStackSequence __instance) {
            IsMoraleAttack = false;
            SkillBasedInit.LogDebug($"AttackStackSequence:OnAdded:prefix - Resetting IsMoraleAttack");
        }
    }

    // Required to apply the negative as soon as it happens in the knockdown case
    [HarmonyPatch(typeof(AbstractActor), "ForceUnitOnePhaseDown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public static class AbstractActor_ForceUnitOnePhaseDown {

        static bool InvokeIsKnockdown = false;
        static bool InvokeIsCalledShot = false;

        public static void Prefix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - sourceID:{sourceID} vs actor: {__instance.GUID}");
            if (sourceID == Mech_CompleteKnockdown.KnockdownSourceID && stackItemUID == Mech_CompleteKnockdown.KnockdownStackItemUID) {
                InvokeIsKnockdown = true;
                InvokeIsCalledShot = false;
            } else if (AttackStackSequence_OnAdded.IsMoraleAttack) {
                InvokeIsKnockdown = false;
                InvokeIsCalledShot = true;
            } else {
                InvokeIsKnockdown = false;
                InvokeIsCalledShot = false;
            }

            // In either of these cases, we want our logic to apply, not default. Fake out the call temporarily, then restore init at the end.
            if (InvokeIsKnockdown || InvokeIsCalledShot) {                
                __instance.Initiative = __instance.Combat.Constants.Phase.PhaseAssault;
            }
        }

        public static void Postfix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            if (InvokeIsKnockdown || InvokeIsCalledShot) {
                SkillBasedInit.LogDebug($"AbstractActor:ForceUnitOnePhaseDown:Postfix - executing custom logic.");
                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];

                int penalty = 0;
                if (InvokeIsKnockdown) {
                    penalty = Math.Max(0, (-1 * SkillBasedInit.Settings.ProneModifier) - actorInit.pilotingEffectMod);                    
                } else if (InvokeIsCalledShot) {
                    penalty = 1;
                }
                SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:Postfix modifying Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) " +
                    $"initiative by {penalty} due to knockdown:{InvokeIsKnockdown} / calledShot:{InvokeIsCalledShot}!");                
                
                if (__instance.HasActivatedThisRound || __instance.Initiative >= SkillBasedInit.MaxPhase) {
                    SkillBasedInit.Logger.Log($"Penalty {penalty} will apply to Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) on next activation!");

                    if (InvokeIsCalledShot) {
                        actorInit.deferredCalledShotMod += penalty;
                    }

                    string floatieMsg = InvokeIsKnockdown ? $"GOING DOWN! -{penalty} INITIATIVE NEXT ROUND" : $"CALLED SHOT! -{penalty} INITIATIVE NEXT ROUND";
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, floatieMsg, FloatieMessage.MessageNature.Debuff));
                } else {
                    SkillBasedInit.Logger.Log($"Penalty {penalty} immediately applies to Actor:({__instance.DisplayName}_{__instance.GetPilot().Name})");

                    // The prefix call from CompleteKnockdown will have the actual initiative value, do not use the actor initiative
                    __instance.Initiative = Mech_CompleteKnockdown.ActorInitiative + penalty;
                    if (__instance.Initiative > SkillBasedInit.MaxPhase) {
                        __instance.Initiative = SkillBasedInit.MaxPhase;
                    }
                    __instance.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(__instance.GUID));
                    string floatieMsg = InvokeIsKnockdown ? $"GOING DOWN! -{penalty} INITIATIVE" : $"CALLED SHOT! -{penalty} INITIATIVE";
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, floatieMsg, FloatieMessage.MessageNature.Debuff));
                }
            }
        }

    }

}
