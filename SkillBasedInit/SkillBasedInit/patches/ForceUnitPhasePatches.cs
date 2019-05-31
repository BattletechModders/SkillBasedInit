﻿using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;

namespace SkillBasedInit {
   
    // --- FORCE UNIT PHASE DOWN EFECTS ---
    // Handle knockdowns
    [HarmonyPatch(typeof(Mech), "CompleteKnockdown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    public static class Mech_CompleteKnockdown {

        public static string KnockdownSourceID;
        public static int KnockdownStackItemUID;

        public static void Prefix(Mech __instance, string sourceID, int stackItemUID) {
            KnockdownSourceID = sourceID;
            KnockdownStackItemUID = stackItemUID;
            Mod.Log.LogIfDebug($"Mech:CompleteKnockdown:prefix - Recording sourceID:{sourceID} stackItemUID:{stackItemUID}");            
        }

        public static void Postfix(Mech __instance) {
            Mod.Log.LogIfDebug($"Mech:CompleteKnockdown:postfix - Removing sourceID:{KnockdownSourceID} stackItemUID:{KnockdownStackItemUID}.");
            KnockdownSourceID = null;
            KnockdownStackItemUID = -1;
        }
    }

    // Handle morale attacks (aka called shots)
    [HarmonyPatch(typeof(AttackStackSequence), "OnAdded")]
    [HarmonyPatch(new Type[] { })]
    public static class AttackStackSequence_OnAdded {

        public static bool IsMoraleAttack = false;
        public static int MoraleAttackMod = 0;       

        public static Dictionary<string, int> AttackModifiers = new Dictionary<string, int>();

        public static void Prefix(AttackStackSequence __instance) {
            IsMoraleAttack = __instance.isMoraleAttack;
            Mod.Log.LogIfDebug($"AttackStackSequence:OnAdded:prefix - Recording IsMoraleAttack: {IsMoraleAttack}");

            AbstractActor attacker = __instance.owningActor;
            ActorInitiative actorInit = ActorInitiativeHolder.GetOrCreate(attacker);
            MoraleAttackMod = actorInit.calledShotMod;
        }

        public static void Postfix(AttackStackSequence __instance) {
            IsMoraleAttack = false;
            MoraleAttackMod = 0;            
            Mod.Log.LogIfDebug($"AttackStackSequence:OnAdded:prefix - Resetting IsMoraleAttack");
        }
    }

    // Required to apply the negative as soon as it happens in the knockdown case
    [HarmonyPatch(typeof(AbstractActor), "ForceUnitOnePhaseDown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public static class AbstractActor_ForceUnitOnePhaseDown {

        static bool InvokeIsKnockdown = false;
        static bool InvokeIsCalledShot = false;

        // We temporarily set the actorInit to PhaseAssault to trick the original method into not firing, then do our work in a postfix.
        //   We have to persist the actorInit during this time.
        public static int PreInvokeInitiative;

        public static void Prefix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            Mod.Log.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - sourceID:{sourceID} vs actor: {__instance.GUID}");
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
                PreInvokeInitiative = __instance.Initiative;
                __instance.Initiative = __instance.Combat.Constants.Phase.PhaseAssault;
            }
        }

        public static void Postfix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            if (InvokeIsKnockdown || InvokeIsCalledShot) {
                Mod.Log.LogIfDebug($"AbstractActor:ForceUnitOnePhaseDown:Postfix - executing custom logic.");
                ActorInitiative actorInit = ActorInitiativeHolder.GetOrCreate(__instance);

                int penalty = 0;
                if (InvokeIsKnockdown) {
                    penalty = Math.Max(0, (-1 * Mod.Config.ProneModifier) - actorInit.pilotingEffectMod);                    
                } else if (InvokeIsCalledShot) {
                    int randVal = Mod.Random.Next(0, 2);
                    penalty = Math.Max(0, AttackStackSequence_OnAdded.MoraleAttackMod + randVal - actorInit.pilotingEffectMod);
                    Mod.Log.LogIfDebug($"AbstractActor:ForceUnitOnePhaseDown:Postfix - moraleAttackMod:{AttackStackSequence_OnAdded.MoraleAttackMod} pilotingEffect:{actorInit.pilotingEffectMod}");
                }
                Mod.Log.Log($"AbstractActor:ForceUnitOnePhaseDown:Postfix modifying Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) " +
                    $"initiative by {penalty} due to knockdown:{InvokeIsKnockdown} / calledShot:{InvokeIsCalledShot}!");                
                
                if (__instance.HasActivatedThisRound || __instance.Initiative >= Mod.MaxPhase) {
                    Mod.Log.Log($"Penalty {penalty} will apply to Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) on next activation!");

                    if (InvokeIsCalledShot) {
                        actorInit.deferredCalledShotMod += penalty;
                    }

                    string floatieMsg = InvokeIsKnockdown ? $"GOING DOWN! -{penalty} INITIATIVE NEXT ROUND" : $"CALLED SHOT! -{penalty} INITIATIVE NEXT ROUND";
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, floatieMsg, FloatieMessage.MessageNature.Debuff));

                    // Reset their initiative on this round
                    __instance.Initiative = PreInvokeInitiative;
                } else {
                    Mod.Log.Log($"Penalty {penalty} immediately applies to Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) with init:{PreInvokeInitiative}");

                    // The prefix call from CompleteKnockdown will have the actual initiative value, do not use the actor initiative
                    __instance.Initiative = PreInvokeInitiative + penalty;
                    if (__instance.Initiative > Mod.MaxPhase) {
                        __instance.Initiative = Mod.MaxPhase;
                    }
                    __instance.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(__instance.GUID));
                    string floatieMsg = InvokeIsKnockdown ? $"GOING DOWN! -{penalty} INITIATIVE" : $"CALLED SHOT! -{penalty} INITIATIVE";
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, floatieMsg, FloatieMessage.MessageNature.Debuff));
                }
            }
        }

    }

    // --- FORCE UNIT PHASE UP EFFECTS ---
    // Handle knockdowns
    [HarmonyPatch(typeof(Mech), "ApplyMoraleDefendEffects")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    public static class Mech_ApplyMoraleDefendEffects {

        public static string MoraleDefendSourceId;
        public static int MoraleDefendStackItemId;

        public static void Prefix(Mech __instance, string sourceID, int stackItemID) {
            MoraleDefendSourceId = sourceID;
            MoraleDefendStackItemId = stackItemID;            
            Mod.Log.LogIfDebug($"Mech:ApplyMoraleDefendEffects:prefix - Recording sourceID:{MoraleDefendSourceId} stackItemUID:{MoraleDefendStackItemId}");
        }

        public static void Postfix(Mech __instance) {
            Mod.Log.LogIfDebug($"Mech:ApplyMoraleDefendEffects:postfix - Removing sourceID:{MoraleDefendSourceId} stackItemUID:{MoraleDefendStackItemId}.");
            MoraleDefendSourceId = null;
            MoraleDefendStackItemId = -1;            
        }
    }

    // Required to apply the negative as soon as it happens in the knockdown case
    [HarmonyPatch(typeof(AbstractActor), "ForceUnitOnePhaseUp")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public static class AbstractActor_ForceUnitOnePhaseUp {

        static bool InvokeIsVigilance = false;
        static int PreInvokeInitiative = 0;

        // ForceUnitOnePhaseUp(string sourceID, int stackItemUID, bool addedBySelf)
        public static void Prefix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            Mod.Log.Log($"AbstractActor:ForceUnitOnePhaseUp:prefix - sourceID:{sourceID} vs actor: {__instance.GUID}");
            if (sourceID == Mech_ApplyMoraleDefendEffects.MoraleDefendSourceId && stackItemUID == Mech_ApplyMoraleDefendEffects.MoraleDefendStackItemId) {
                InvokeIsVigilance = true;
                PreInvokeInitiative = __instance.Initiative;
                __instance.Initiative = __instance.Combat.Constants.Phase.PhaseSpecial;
            } else {
                InvokeIsVigilance = false;
            }
        }

        public static void Postfix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            if (InvokeIsVigilance) {
                Mod.Log.LogIfDebug($"AbstractActor:ForceUnitOnePhaseUp:Postfix - executing custom logic.");
                ActorInitiative actorInit = ActorInitiativeHolder.GetOrCreate(__instance);
               
                actorInit.deferredVigilanceMod = actorInit.vigilianceMod + Mod.Random.Next(0, 2);
                Mod.Log.LogIfDebug($"AbstractActor:ForceUnitOnePhaseUp:Postfix - actor {__instance.DisplayName}_{__instance.GetPilot().Name} will gain  {actorInit.deferredVigilanceMod} init next round.");
                string floatieMsg = $"VIGILANCE! +{actorInit.vigilianceMod} INITIATIVE NEXT ROUND!";
                __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, floatieMsg, FloatieMessage.MessageNature.Debuff));

                // TODO: Test that this eliminates the portrait remaining activated
                __instance.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(__instance.GUID));

                // Reset the actor's initiative
                Mod.Log.LogIfDebug($"AbstractActor:ForceUnitOnePhaseUp:Postfix - actor {__instance.DisplayName}_{__instance.GetPilot().Name} init of {__instance.Initiative} being restored to {PreInvokeInitiative}");
                __instance.Initiative = PreInvokeInitiative;
            }
        }

    }

}
