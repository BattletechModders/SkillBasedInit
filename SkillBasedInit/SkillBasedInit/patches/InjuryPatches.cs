using BattleTech;
using Harmony;
using System;

namespace SkillBasedInit {

    // Injure the pilot as soon as it happens
    [HarmonyPatch(typeof(Pilot), "InjurePilot")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(int), typeof(DamageType), typeof(Weapon), typeof(AbstractActor) })]
    public static class Pilot_InjurePilot {
        public static void Postfix(Pilot __instance, string sourceID, int stackItemUID, int dmg, DamageType damageType, Weapon sourceWeapon, AbstractActor sourceActor) {
            SkillBasedInit.Logger.Log($"Called after pilot injury on Actor:({__instance.ParentActor.DisplayName}_{__instance.Name}), but still NO-OP");
        }
    }
}
