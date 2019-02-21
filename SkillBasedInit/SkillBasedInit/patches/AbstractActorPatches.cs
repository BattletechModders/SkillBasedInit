using BattleTech;
using BattleTech.UI;
using Harmony;
using SkillBasedInit.Helper;
using System;

namespace SkillBasedInit {

    [HarmonyPatch(typeof(AbstractActor), "OnNewRound")]
    public static class AbstractActor_OnNewRound {
        public static void Postfix(AbstractActor __instance, int round) {
            //SkillBasedInit.Logger.Log($"AbstractActor: Starting new round {round}, recalculating initiative element for actor:{__instance.DisplayName}");
            ActorInitiativeHolder.OnRoundBegin(__instance);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "DeferUnit")]
    public static class AbstractActor_DeferUnit {
        public static void Postfix(AbstractActor __instance) {
            //SkillBasedInit.Logger.Log($"AbstractActor:DeferUnit:");
            int reservePenalty = SkillBasedInit.Random.Next(SkillBasedInit.ModConfig.ReservedPenaltyBounds[0], SkillBasedInit.ModConfig.ReservedPenaltyBounds[1]);
            SkillBasedInit.Logger.LogIfDebug($"  Deferring Actor:({CombatantHelper.LogLabel(__instance)}) " +
                $"initiative:{__instance.Initiative} by:{reservePenalty} to:{__instance.Initiative + reservePenalty}");
            __instance.Initiative += reservePenalty;
            if (__instance.Initiative > SkillBasedInit.MaxPhase) {
                __instance.Initiative = SkillBasedInit.MaxPhase;
            }

            // Save some part of the reserve surplus as a penalty for the next round
            ActorInitiative actorInit = ActorInitiativeHolder.GetOrCreate(__instance);
            actorInit.reservedCount++;
            SkillBasedInit.Logger.LogIfDebug($"  Actor:({CombatantHelper.LogLabel(__instance)}) reservedCount incremented to:{actorInit.reservedCount}");
        }

    }

    [HarmonyPatch(typeof(AbstractActor), "InitiativeToString")]
    public static class AbstractActor_InitiativeToString {
        public static void Postfix(AbstractActor __instance, ref string __result, int initiative) {
            switch (initiative) {
                case 1:
                    __result = "30";
                    break;
                case 2:
                    __result = "29";
                    break;
                case 3:
                    __result = "28";
                    break;
                case 4:
                    __result = "27";
                    break;
                case 5:
                    __result = "26";
                    break;
                case 6:
                    __result = "25";
                    break;
                case 7:
                    __result = "24";
                    break;
                case 8:
                    __result = "23";
                    break;
                case 9:
                    __result = "22";
                    break;
                case 10:
                    __result = "21";
                    break;
                case 11:
                    __result = "20";
                    break;
                case 12:
                    __result = "19";
                    break;
                case 13:
                    __result = "18";
                    break;
                case 14:
                    __result = "17";
                    break;
                case 15:
                    __result = "16";
                    break;
                case 16:
                    __result = "15";
                    break;
                case 17:
                    __result = "14";
                    break;
                case 18:
                    __result = "13";
                    break;
                case 19:
                    __result = "12";
                    break;
                case 20:
                    __result = "11";
                    break;
                case 21:
                    __result = "10";
                    break;
                case 22:
                    __result = "9";
                    break;
                case 23:
                    __result = "8";
                    break;
                case 24:
                    __result = "7";
                    break;
                case 25:
                    __result = "6";
                    break;
                case 26:
                    __result = "5";
                    break;
                case 27:
                    __result = "4";
                    break;
                case 28:
                    __result = "3";
                    break;
                case 29:
                    __result = "2";
                    break;
                case 30:
                    __result = "1";
                    break;
                default:                    
                    if (initiative > SkillBasedInit.MaxPhase) {
                        // This looks weird, but it's the only place we can intercept a negative init that I've found.
                        if (__instance != null) { __instance.Initiative = SkillBasedInit.MaxPhase; }
                        SkillBasedInit.Logger.Log($"AbstractActor:InitiativeToString - ERROR - Bad initiative of {initiative} detected!");
                        __result = "1";
                    } else if (initiative < SkillBasedInit.MinPhase) {
                        // This looks weird, but it's the only place we can intercept a negative init that I've found.
                        if (__instance != null) { __instance.Initiative = SkillBasedInit.MinPhase; }
                        SkillBasedInit.Logger.Log($"AbstractActor:InitiativeToString - ERROR - Bad initiative of {initiative} detected!");
                        __result = "30";
                    }                    
                    break;
            }
            //SkillBasedInit.Logger.Log($"AbstractActor:InitiativeToString returning {__result} for {initiative}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("HasValidInitiative", MethodType.Getter)]
    public static class AbstractActor_HasValidInitiative {
        public static void Postfix(AbstractActor __instance, bool __result) {
            bool isValid = __instance.Initiative >= SkillBasedInit.MinPhase && __instance.Initiative <= SkillBasedInit.MaxPhase;
            if (!isValid) {
                SkillBasedInit.Logger.Log($"Actor:{CombatantHelper.LogLabel(__instance)} has invalid initiative {__instance.Initiative}!");
            }
            __result = isValid;
            SkillBasedInit.Logger.LogIfDebug($"AbstractActor:HasValidInitiative returning {__result} for {__instance.Initiative}");
        }
    }

    // TODO: This looks to be unused - can we confirm?
    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("BaseInitiative", MethodType.Getter)]
    public static class AbstractActor_BaseInitiative {
        public static void Postfix(AbstractActor __instance, ref int __result, StatCollection ___statCollection) {
            CombatGameState ___Combat = (CombatGameState)Traverse.Create(__instance).Property("Combat").GetValue();
            if (___Combat.TurnDirector.IsInterleaved) {
                CombatHUD ___HUD = (CombatHUD)Traverse.Create(__instance).Property("HUD").GetValue();

                int baseInit = ___statCollection.GetValue<int>("BaseInitiative");
                int phaseMod = ___statCollection.GetValue<int>("PhaseModifier");
                int modifiedInit = baseInit + phaseMod;
                //SkillBasedInit.Logger.LogIfDebug($"Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has stats BaseInit:{baseInit} / PhaseMod:{phaseMod}");

                if (modifiedInit < SkillBasedInit.MinPhase) {
                    SkillBasedInit.Logger.Log($"Actor:({CombatantHelper.LogLabel(__instance)}) being set to {SkillBasedInit.MinPhase} due to BaseInit:{baseInit} + PhaseMod:{phaseMod}");
                    __result = SkillBasedInit.MinPhase;
                } else if (modifiedInit > SkillBasedInit.MaxPhase) {
                    SkillBasedInit.Logger.Log($"Actor:({CombatantHelper.LogLabel(__instance)}) being set to {SkillBasedInit.MaxPhase} due to BaseInit:{baseInit} + PhaseMod:{phaseMod}");
                    __result = SkillBasedInit.MaxPhase;
                } else {
                    __result = modifiedInit;
                    //SkillBasedInit.Logger.Log($"Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has stats BaseInit:{baseInit} + PhaseMod:{phaseMod} = modifiedInit:{modifiedInit}.");
                }
            }            
        }
    }

}
