using BattleTech;
using Harmony;

namespace SkillBasedInit.patches {

    //[HarmonyPatch(typeof(Team), "OnTurnActorActivationComplete")]
    //public static class Team_OnTurnActorActivationComplete {

    //    public static void Prefix(Team __instance, string actorGUID, bool isDeferringAction) {
    //        Mod.Log.Trace($"T:OTAAC - entered for actorGUID: '{actorGUID}'  isDeferringAction: {isDeferringAction}.");

    //        Traverse shouldCompleteT = Traverse.Create(__instance).Property("ShouldCompleteActivationSequence");
    //        Mod.Log.Debug($" Team - ShouldCompleteActivationSequence: {shouldCompleteT.GetValue()}");
    //    }
    //}
}
