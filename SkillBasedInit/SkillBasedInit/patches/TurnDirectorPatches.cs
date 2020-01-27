using BattleTech;
using Harmony;
using System;

namespace SkillBasedInit.patches {

    [HarmonyPatch(typeof(TurnDirector), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
    public static class TurnDirector_ctor {
        public static void Postfix(TurnDirector __instance) {
            Mod.Log.Debug("TD:ctor:post - entered.");
            Mod.Log.Debug($" TurnDirector init with phases: {__instance.FirstPhase} / {__instance.LastPhase}");
            
            Traverse firstT = Traverse.Create(__instance).Property("FirstPhase");
            firstT.SetValue(1);

            Traverse lastT = Traverse.Create(__instance).Property("LastPhase");
            lastT.SetValue(30);

            Mod.Log.Debug($" TurnDirector updated to phases: {__instance.FirstPhase} / {__instance.LastPhase}");
        }
    }

    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Postfix(TurnDirector __instance) {
            Mod.Log.Trace("TD:OCGD:post - entered.");
            Mod.Log.Debug($" TurnDirector - Combat complete, destroying initiative map.");
            ActorInitiativeHolder.OnCombatComplete();
        }
    }

    //[HarmonyPatch(typeof(TurnDirector), "BeginNewPhase")]
    //public static class TurnDirector_BeginNewPhase {
    //    public static void Postfix(TurnDirector __instance, int newPhase) {
    //        Mod.Log.Trace($"TD:BNP - for phase: {newPhase}  currentPhase:{__instance.CurrentPhase}  nonInterleavedPhase:{__instance.NonInterleavedPhase}  phaseIncrement:{__instance.PhaseIncrement}");
    //    }
    //}

    //[HarmonyPatch(typeof(TurnDirector), "EndCurrentPhase")]
    //public static class TurnDirector_EndCurrentPhase {
    //    public static void Postfix(TurnDirector __instance) {
    //        Mod.Log.Trace($"TD:ECP - ending phase: {__instance.CurrentPhase}  nonInterleavedPhase:{__instance.NonInterleavedPhase}  phaseIncrement:{__instance.PhaseIncrement}");
    //    }
    //}

    //[HarmonyPatch(typeof(TurnDirector), "EndCurrentPhaseComplete")]
    //public static class TurnDirector_EndCurrentPhaseComplete {
    //    public static void Postfix(TurnDirector __instance) {
    //        Mod.Log.Trace($"TD:ECPC - ending phase: {__instance.CurrentPhase} complete  nonInterleavedPhase:{__instance.NonInterleavedPhase}  phaseIncrement:{__instance.PhaseIncrement}");
    //    }
    //}

    //[HarmonyPatch(typeof(TurnDirector), "NotifyTurnEvents")]
    //public static class TurnDirector_NotifyTurnEvents {
    //    public static void Postfix(TurnDirector __instance) {
    //        Mod.Log.Trace($"TD:NTE - phase: {__instance.CurrentPhase} has {__instance.TurnEvents.Count} turn events.  nonInterleavedPhase:{__instance.NonInterleavedPhase}  phaseIncrement:{__instance.PhaseIncrement}");
    //    }
    //}

    //[HarmonyPatch(typeof(TurnDirector), "BeginNewRound")]
    //public static class TurnDirector_BeginNewRound {
    //    public static void Postfix(TurnDirector __instance, int round) {
    //        Mod.Log.Trace($"TD:BNR - for round: {round}");
    //    }
    //}

    //[HarmonyPatch(typeof(TurnDirector), "IncrementActiveTurnActor")]
    //public static class TurnDirector_IncrementActiveTurnActor {
    //    public static void Postfix(TurnDirector __instance) {
    //        Mod.Log.Trace($"TD:IATA - entered with isInterleaved:{__instance.IsInterleaved}  isInterleavedPending:{__instance.IsInterleavePending}  isNonInterleavePending:{__instance.IsNonInterleavePending}");
    //    }
    //}

    //[HarmonyPatch(typeof(TurnDirector), "NotifyContact")]
    //public static class TurnDirector_NotifyContact {
    //    public static void Postfix(TurnDirector __instance, VisibilityLevel contactLevel) {
    //        Mod.Log.Trace($"TD:NC - notifying contact due to visibilityLevel:{contactLevel}");
    //    }
    //}
}
