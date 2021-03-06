﻿using BattleTech;
using BattleTech.UI;
using Harmony;
using SkillBasedInit.Helper;
using System;
using us.frostraptor.modUtils;

namespace SkillBasedInit {

    [HarmonyPatch(typeof(Mech), "OnActivationEnd")]
    public static class AbstractActor_OnActivationEnd {
        public static void Prefix(Mech __instance, string sourceID, int stackItemID) {
            Mod.Log.Trace?.Write($"M:OAE:pre - entered for '{CombatantUtils.Label(__instance)}' with sourceID: '{sourceID}' and stackItemId: '{stackItemID}'");
        }
        public static void Postfix(Mech __instance, string sourceID, int stackItemID) {
            Mod.Log.Trace?.Write($"M:OAE:post - entered for '{CombatantUtils.Label(__instance)}' with sourceID: '{sourceID}' and stackItemId: '{stackItemID}'");
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "OnNewRound")]
    public static class AbstractActor_OnNewRound {
        public static void Postfix(AbstractActor __instance, int round) {
            Mod.Log.Trace?.Write("AA:ONR - entered.");
            Mod.Log.Debug?.Write($"  AbstractActor starting new round {round}, recalculating initiative element for actor:{__instance.DisplayName}");
            ActorInitiativeHolder.OnRoundBegin(__instance);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "DeferUnit")]
    public static class AbstractActor_DeferUnit {
        public static void Postfix(AbstractActor __instance) {
            Mod.Log.Trace?.Write($"AA:DU - entered.");
            int reservePenalty = Mod.Random.Next(Mod.Config.ReservedPenaltyBounds[0], Mod.Config.ReservedPenaltyBounds[1]);
            Mod.Log.Debug?.Write($"  Deferring Actor:({CombatantUtils.Label(__instance)}) " +
                $"initiative:{__instance.Initiative} by:{reservePenalty} to:{__instance.Initiative + reservePenalty}");
            __instance.Initiative += reservePenalty;
            if (__instance.Initiative > Mod.MaxPhase) {
                __instance.Initiative = Mod.MaxPhase;
            }

            // Save some part of the reserve surplus as a penalty for the next round
            ActorInitiative actorInit = ActorInitiativeHolder.GetOrCreate(__instance);
            actorInit.reservedCount++;
            Mod.Log.Debug?.Write($"  Actor:({CombatantUtils.Label(__instance)}) reservedCount incremented to:{actorInit.reservedCount}");
        }

    }

    [HarmonyPatch(typeof(AbstractActor), "InitiativeToString")]
    public static class AbstractActor_InitiativeToString {
        public static void Postfix(AbstractActor __instance, ref string __result, int initiative) {
            Mod.Log.Trace?.Write($"AA:ITS - entered for init:{initiative}.");
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
                    if (initiative > Mod.MaxPhase) {
                        // This looks weird, but it's the only place we can intercept a negative init that I've found.
                        if (__instance != null) { __instance.Initiative = Mod.MaxPhase; }
                        Mod.Log.Info?.Write($"AbstractActor:InitiativeToString - ERROR - Bad initiative of {initiative} detected!");
                        __result = "1";
                    } else if (initiative < Mod.MinPhase) {
                        // This looks weird, but it's the only place we can intercept a negative init that I've found.
                        if (__instance != null) { __instance.Initiative = Mod.MinPhase; }
                        Mod.Log.Info?.Write($"AbstractActor:InitiativeToString - ERROR - Bad initiative of {initiative} detected!");
                        __result = "30";
                    }
                    break;
            }
            Mod.Log.Trace?.Write($" returning {__result} for initiative {initiative}");
        }
    }

    // TODO: This is likely inlined and doesn't work- confirm
    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("HasValidInitiative", MethodType.Getter)]
    public static class AbstractActor_HasValidInitiative {
        public static void Postfix(AbstractActor __instance, bool __result) {
            Mod.Log.Debug?.Write("AA:HVI - entered.");
            bool isValid = __instance.Initiative >= Mod.MinPhase && __instance.Initiative <= Mod.MaxPhase;
            if (!isValid) {
                Mod.Log.Info?.Write($"Actor:{CombatantUtils.Label(__instance)} has invalid initiative {__instance.Initiative}!");
            }
            __result = isValid;
            Mod.Log.Debug?.Write($"AbstractActor:HasValidInitiative returning {__result} for {__instance.Initiative}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("BaseInitiative", MethodType.Getter)]
    public static class AbstractActor_BaseInitiative {
        public static void Postfix(AbstractActor __instance, ref int __result, StatCollection ___statCollection) {
            Mod.Log.Trace?.Write("AA:BI - entered.");

            if (__instance.Combat.TurnDirector.IsInterleaved) {
                int baseInit = ___statCollection.GetValue<int>("BaseInitiative");
                int phaseMod = ___statCollection.GetValue<int>("PhaseModifier");
                int modifiedInit = baseInit + phaseMod;

                if (modifiedInit < Mod.MinPhase) {
                    Mod.Log.Info?.Write($"Actor:({CombatantUtils.Label(__instance)}) being set to {Mod.MinPhase} due to BaseInit:{baseInit} + PhaseMod:{phaseMod}");
                    __result = Mod.MinPhase;
                } else if (modifiedInit > Mod.MaxPhase) {
                    Mod.Log.Info?.Write($"Actor:({CombatantUtils.Label(__instance)}) being set to {Mod.MaxPhase} due to BaseInit:{baseInit} + PhaseMod:{phaseMod}");
                    __result = Mod.MaxPhase;
                } else {
                    __result = modifiedInit;
                    Mod.Log.Info?.Write($"Actor:({CombatantUtils.Label(__instance)}) has stats BaseInit:{baseInit} + PhaseMod:{phaseMod} = modifiedInit:{modifiedInit}.");
                }
            } else {
                Mod.Log.Info?.Write($"Actor:({CombatantUtils.Label(__instance)}) is non-interleaved, returning phase: {Mod.MaxPhase}.");
                __result = Mod.MaxPhase;
            }

        }
    }

}
