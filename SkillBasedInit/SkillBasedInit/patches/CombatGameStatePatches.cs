using BattleTech;
using Harmony;

namespace SkillBasedInit.patches {
    [HarmonyPatch(typeof(CombatGameState), "_Init")]
    public static class CombatGateState__Init {
        public static void Postfix(CombatGameState __instance) {
            Mod.Log.Debug($" TurnDirector initialized with phases: {__instance.TurnDirector.FirstPhase} / {__instance.TurnDirector.FirstPhase} " +
                $"and non-interleaved phase: {__instance.TurnDirector.NonInterleavedPhase}");
        }
    }
}
