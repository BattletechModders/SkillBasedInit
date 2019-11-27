using BattleTech;
using Harmony;
using us.frostraptor.modUtils;

namespace SkillBasedInit.patches {

    //[HarmonyPatch(typeof(Mech), "InitStats")]
    //public static class Mech_InitStats {
    //    public static void Postfix(Mech __instance) {
    //        Mod.Log.Trace($"M:IS - entered for {CombatantUtils.Label(__instance)}");
    //        WeightClass weightClass = __instance.MechDef.Chassis.weightClass;
    //        if (weightClass == WeightClass.ASSAULT) {
    //            __instance.Initiative = Mod.MaxPhase;
    //        }
    //        __instance.StatCollection.Set("BaseInitiative", Mod.MaxPhase);
    //    }
    //}
}
