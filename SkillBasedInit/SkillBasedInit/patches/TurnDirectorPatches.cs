using BattleTech;
using Harmony;
using System;

namespace SkillBasedInit.patches {
    [HarmonyPatch(typeof(TurnDirector), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
    public static class TurnDirector_ctor {
        public static void Postfix(TurnDirector __instance) {
            int ___FirstPhase = (int)Traverse.Create(__instance).Property("FirstPhase").GetValue();
            int ___LastPhase = (int)Traverse.Create(__instance).Property("LastPhase").GetValue();
            //SkillBasedInit.Logger.Log($"TurnDirector::ctor::post - was initialized with {___FirstPhase} / {___LastPhase}");

            Traverse.Create(__instance).Property("FirstPhase").SetValue(1);
            Traverse.Create(__instance).Property("LastPhase").SetValue(SkillBasedInit.MaxPhase);
        }
    }

    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Postfix(TurnDirector __instance) {
            //SkillBasedInit.Logger.Log($"TurnDirector; Combat complete, destroying initiative map.");
            ActorInitiativeHolder.OnCombatComplete();
        }
    }
}
