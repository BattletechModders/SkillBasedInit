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

        public static void Postfix(Mech __instance, string sourceID, int stackItemUID)
        {
            Mod.Log.Trace?.Write("M:CK:post - entered.");
            Mod.Log.Debug?.Write($"Completing knockdown for actor: {__instance.DistinctId()} from sourceID:{sourceID} stackItemUID:{stackItemUID}.");

            bool applyNextTurn = __instance.HasActivatedThisRound || __instance.Initiative >= Mod.MaxPhase;
            if (applyNextTurn)
            {
                Mod.Log.Debug?.Write("Actor has already acted, applying prone modifier next turn.");
                return;
            }

            int penalty = __instance.ProneInitModifier(isKnockdown: true);
            if (penalty == 0) return; // Nothing to do

            Mod.Log.Info?.Write($"Applying penalty: {penalty} to {__instance.DistinctId()} with init: {__instance.Initiative} immediately.");

            __instance.Initiative += penalty;
            if (__instance.Initiative > Mod.MaxPhase)
            {
                __instance.Initiative = Mod.MaxPhase;
            }
            __instance.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(__instance.GUID));

            string floatieMsg = new Text(Mod.LocalizedText.Floaties[ModText.LT_FT_KNOCKDOWN], new object[] { penalty }).ToString();
            __instance.Combat.MessageCenter.PublishMessage(
                    new FloatieMessage(__instance.GUID, __instance.GUID, floatieMsg, FloatieMessage.MessageNature.Debuff)
                    );
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

        static bool InvokeIsCalledShot = false;

        // We temporarily set the actorInit to PhaseAssault to trick the original method into not firing, then do our work in a postfix.
        //   We have to persist the actorInit during this time.
        public static int PreInvokeInitiative;

        public static void Prefix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf)
        {
            Mod.Log.Trace?.Write("AA:FUOPD:pre - entered.");

            InvokeIsCalledShot = AttackStackSequence_OnAdded.IsMoraleAttack ? true : false;

            // In either of these cases, we want our logic to apply, not default. Fake out the call temporarily, then restore init at the end.
            if (InvokeIsCalledShot)
            {
                PreInvokeInitiative = __instance.Initiative;
                __instance.Initiative = __instance.Combat.Constants.Phase.PhaseAssault;
            }
        }

        public static void Postfix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf)
        {
            Mod.Log.Trace?.Write("AA:FUOPD:post - entered.");
            if (!InvokeIsCalledShot) return;

            bool applyNextTurn = __instance.HasActivatedThisRound || __instance.Initiative >= Mod.MaxPhase;
            int penalty = __instance.CalledShotPenalty(AttackStackSequence_OnAdded.Attacker);
            if (penalty == 0) return; // nothing to do

            if (applyNextTurn)
            {
                Mod.Log.Info?.Write($"Applying penalty: {penalty} to {__instance.DistinctId()} with init: {PreInvokeInitiative} next turn.");
                __instance.StatCollection.ModifyStat<int>("self", -1, ModStats.STATE_CALLED_SHOT, StatCollection.StatOperation.Int_Add, penalty);

                // Leave their init untouched on this round
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

            string floatieMsg = applyNextTurn ?
                new Text(Mod.LocalizedText.Floaties[ModText.LT_FT_CALLED_SHOT_LATER], new object[] { penalty }).ToString() :
                new Text(Mod.LocalizedText.Floaties[ModText.LT_FT_CALLED_SHOT_NOW], new object[] { penalty }).ToString();
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
