using BattleTech;
using Harmony;
using Localize;
using System;
using System.Collections.Generic;
using SkillBasedInit.Helper;
using IRBTModUtils.Extension;

namespace SkillBasedInit.Patches
{

    // --- FORCE UNIT PHASE DOWN EFECTS ---
    // Handle knockdowns
    [HarmonyPatch(typeof(Mech), "CompleteKnockdown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    public static class Mech_CompleteKnockdown
    {

        public static string KnockdownSourceID;
        public static int KnockdownStackItemUID;

        public static void Prefix(Mech __instance, string sourceID, int stackItemUID)
        {
            Mod.Log.Trace?.Write("M:CK:pre - entered.");
            KnockdownSourceID = sourceID;
            KnockdownStackItemUID = stackItemUID;
            Mod.Log.Debug?.Write($"Mech:CompleteKnockdown:prefix - Recording sourceID:{sourceID} stackItemUID:{stackItemUID}");
        }

        public static void Postfix(Mech __instance)
        {
            Mod.Log.Trace?.Write("M:CK:post - entered.");
            Mod.Log.Debug?.Write($"Mech:CompleteKnockdown:postfix - Removing sourceID:{KnockdownSourceID} stackItemUID:{KnockdownStackItemUID}.");
            KnockdownSourceID = null;
            KnockdownStackItemUID = -1;
        }
    }

    // Handle morale attacks (aka called shots)
    [HarmonyPatch(typeof(AttackStackSequence), "OnAdded")]
    [HarmonyPatch(new Type[] { })]
    public static class AttackStackSequence_OnAdded
    {
        public static bool IsMoraleAttack = false;
        public static AbstractActor Attacker = null;

        public static void Prefix(AttackStackSequence __instance)
        {
            Mod.Log.Trace?.Write("ASS:OA:pre - entered.");
            IsMoraleAttack = __instance.isMoraleAttack;
            Attacker = __instance.owningActor;
        }

        public static void Postfix(AttackStackSequence __instance)
        {
            Mod.Log.Trace?.Write("ASS:OA:post - entered.");
            IsMoraleAttack = false;
            Attacker = null;
            Mod.Log.Debug?.Write($"AttackStackSequence:OnAdded:prefix - Resetting IsMoraleAttack");
        }
    }

    // Required to apply the negative as soon as it happens in the knockdown case
    [HarmonyPatch(typeof(AbstractActor), "ForceUnitOnePhaseDown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public static class AbstractActor_ForceUnitOnePhaseDown
    {

        static bool InvokeIsKnockdown = false;
        static bool InvokeIsCalledShot = false;

        // We temporarily set the actorInit to PhaseAssault to trick the original method into not firing, then do our work in a postfix.
        //   We have to persist the actorInit during this time.
        public static int PreInvokeInitiative;

        public static void Prefix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf)
        {
            Mod.Log.Trace?.Write("AA:FUOPD:pre - entered.");

            if (sourceID == Mech_CompleteKnockdown.KnockdownSourceID && stackItemUID == Mech_CompleteKnockdown.KnockdownStackItemUID)
            {
                InvokeIsKnockdown = true;
                InvokeIsCalledShot = false;
            }
            else if (AttackStackSequence_OnAdded.IsMoraleAttack)
            {
                InvokeIsKnockdown = false;
                InvokeIsCalledShot = true;
            }
            else
            {
                InvokeIsKnockdown = false;
                InvokeIsCalledShot = false;
            }

            // In either of these cases, we want our logic to apply, not default. Fake out the call temporarily, then restore init at the end.
            if (InvokeIsKnockdown || InvokeIsCalledShot)
            {
                PreInvokeInitiative = __instance.Initiative;
                __instance.Initiative = __instance.Combat.Constants.Phase.PhaseAssault;
            }
        }

        public static void Postfix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf)
        {
            Mod.Log.Trace?.Write("AA:FUOPD:post - entered.");
            bool applyNextTurn = __instance.HasActivatedThisRound || __instance.Initiative >= Mod.MaxPhase;
            int penalty = 0;
            string floatieMsg = "";
            if (InvokeIsKnockdown)
            {
                penalty = __instance.KnockdownPenalty();
                floatieMsg = applyNextTurn ?
                    new Text(Mod.LocalizedText.Floaties[ModText.LT_FT_KNOCKDOWN_LATER], new object[] { penalty }).ToString() :
                    new Text(Mod.LocalizedText.Floaties[ModText.LT_FT_KNOCKDOWN_NOW], new object[] { penalty }).ToString();
            }
            else if (InvokeIsCalledShot)
            {
                penalty = __instance.CalledShotPenalty(AttackStackSequence_OnAdded.Attacker);
                floatieMsg = applyNextTurn ?
                    new Text(Mod.LocalizedText.Floaties[ModText.LT_FT_CALLED_SHOT_LATER], new object[] { penalty }).ToString() :
                    new Text(Mod.LocalizedText.Floaties[ModText.LT_FT_CALLED_SHOT_NOW], new object[] { penalty }).ToString();
            }

            if (penalty == 0) return;
            
            if (applyNextTurn)
            {
                Mod.Log.Info?.Write($"Applying penalty: {penalty} to {__instance.DistinctId()} with init: {PreInvokeInitiative} next turn.");

                __instance.StatCollection.ModifyStat<int>("self", -1, 
                    InvokeIsKnockdown ? ModStats.STATE_KNOCKDOWN : ModStats.STATE_CALLED_SHOT, 
                    StatCollection.StatOperation.Int_Add, penalty);

                // Reset their initiative on this round
                __instance.Initiative = PreInvokeInitiative;
            }
            else
            {
                Mod.Log.Info?.Write($"Applying penalty: {penalty} to {__instance.DistinctId()} with init: {PreInvokeInitiative} immediately.");

                // The prefix call from CompleteKnockdown will have the actual initiative value, do not use the actor initiative
                __instance.Initiative = PreInvokeInitiative + penalty;
                if (__instance.Initiative > Mod.MaxPhase)
                {
                    __instance.Initiative = Mod.MaxPhase;
                }
                __instance.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(__instance.GUID));
            }

            __instance.Combat.MessageCenter.PublishMessage(
                new FloatieMessage(__instance.GUID, __instance.GUID, floatieMsg, FloatieMessage.MessageNature.Debuff)
                );
        }

    }

    // --- FORCE UNIT PHASE UP EFFECTS ---
    // Handle knockdowns
    [HarmonyPatch(typeof(Mech), "ApplyMoraleDefendEffects")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    public static class Mech_ApplyMoraleDefendEffects
    {

        public static string MoraleDefendSourceId;
        public static int MoraleDefendStackItemId;

        public static void Prefix(Mech __instance, string sourceID, int stackItemID)
        {
            Mod.Log.Trace?.Write("M:AMDE:post - entered.");

            MoraleDefendSourceId = sourceID;
            MoraleDefendStackItemId = stackItemID;
            Mod.Log.Debug?.Write($"Mech:ApplyMoraleDefendEffects:prefix - Recording sourceID:{MoraleDefendSourceId} stackItemUID:{MoraleDefendStackItemId}");
        }

        public static void Postfix(Mech __instance)
        {
            Mod.Log.Trace?.Write("M:AMDE:post - entered.");
            Mod.Log.Debug?.Write($"Mech:ApplyMoraleDefendEffects:postfix - Removing sourceID:{MoraleDefendSourceId} stackItemUID:{MoraleDefendStackItemId}.");
            MoraleDefendSourceId = null;
            MoraleDefendStackItemId = -1;
        }
    }

    // Required to apply the negative as soon as it happens in the knockdown case
    [HarmonyPatch(typeof(AbstractActor), "ForceUnitOnePhaseUp")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public static class AbstractActor_ForceUnitOnePhaseUp
    {

        static bool InvokeIsVigilance = false;
        static int PreInvokeInitiative = 0;

        public static void Prefix(AbstractActor __instance, string sourceID, int stackItemUID)
        {
            Mod.Log.Trace?.Write("AA:FUOPU:pre - entered.");
            Mod.Log.Info?.Write($"AbstractActor:ForceUnitOnePhaseUp:prefix - sourceID:{sourceID} vs actor: {__instance.GUID}");
            if (sourceID == Mech_ApplyMoraleDefendEffects.MoraleDefendSourceId && stackItemUID == Mech_ApplyMoraleDefendEffects.MoraleDefendStackItemId)
            {
                InvokeIsVigilance = true;
                PreInvokeInitiative = __instance.Initiative;
                __instance.Initiative = __instance.Combat.Constants.Phase.PhaseSpecial;
            }
            else
            {
                InvokeIsVigilance = false;
            }
        }

        public static void Postfix(AbstractActor __instance)
        {
            Mod.Log.Trace?.Write("AA:FUOPU:post - entered.");
            if (InvokeIsVigilance)
            {

                int bonus = __instance.VigilanceBonus();
                if (bonus != 0)
                {
                    __instance.StatCollection.ModifyStat<int>("self", -1, ModStats.STATE_VIGILIANCE, StatCollection.StatOperation.Int_Add, bonus);

                    string floatieMsg = new Text(Mod.LocalizedText.Floaties[ModText.LT_FT_VIGILANCE], new object[] { bonus }).ToString();
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, floatieMsg, FloatieMessage.MessageNature.Buff));

                }

                // Restore actor's initiative
                __instance.Initiative = PreInvokeInitiative;
            }
        }

    }

}
