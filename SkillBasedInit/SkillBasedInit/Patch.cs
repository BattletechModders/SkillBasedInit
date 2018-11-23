using BattleTech;
using BattleTech.UI;
using DG.Tweening;
using Harmony;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillBasedInit {

    [HarmonyPatch(typeof(AbstractActor), "OnNewRound")]
    public static class AbstractActor_OnNewRound {
        public static void Postfix(AbstractActor __instance, int round) {
            SkillBasedInit.Logger.Log($"AbstractActor: Starting new round {round}, recalculating initiative element for actor:{__instance.DisplayName}");
            ActorInitiativeHolder.OnRoundBegin(__instance);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "DeferUnit")]
    public static class AbstractActor_DeferUnit {
        public static void Postfix(AbstractActor __instance) {
            //SkillBasedInit.Logger.Log($"AbstractActor:DeferUnit:");
            var deferJump = SkillBasedInit.Random.Next(0, 5);
            __instance.Initiative += deferJump;
            SkillBasedInit.Logger.Log($"AbstractActor:DeferUnit - Reducing actorInit by an additional {deferJump} to {__instance.Initiative} ");
            if (__instance.Initiative > SkillBasedInit.MaxPhase) {
                __instance.Initiative = SkillBasedInit.MaxPhase;
            }
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
                    __result = "ERROR";
                    break;
            }
            //SkillBasedInit.Logger.Log($"AbstractActor:InitiativeToString returning {__result} for {initiative}");
        }
    }

    [HarmonyPatch(typeof(AbstractActor))]
    [HarmonyPatch("HasValidInitiative", MethodType.Getter)]
    public static class AbstractActor_HasValidInitiative {
        public static void Postfix(AbstractActor __instance, bool __result) {
            __result = __instance.Initiative > 0 && __instance.Initiative < 31;
            //SkillBasedInit.Logger.Log($"AbstractActor:HasValidInitiative returning {__result} for {__instance.Initiative}");
        }
    }

    [HarmonyPatch(typeof(Mech), "DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
    public static class Mech_DamageLocation {
        public static void Postfix(Mech __instance, WeaponHitInfo hitInfo, Weapon weapon, AttackImpactQuality impactQuality, DamageType damageType) {            
            if (weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.Logger.Log($"Mech:DamageLocation:post - mech {__instance.DisplayName} has suffered a melee attack.");
                float attackerTonnage = 0.0f;
                if (weapon.parent.GetType() == typeof(Mech)) {
                    Mech parent = (Mech)weapon.parent;
                    attackerTonnage = parent.tonnage;
                } else if (weapon.parent.GetType() == typeof(Vehicle)) {
                    Vehicle parent = (Vehicle)weapon.parent;
                    attackerTonnage = parent.tonnage;
                }
                float targetTonnage = __instance.tonnage;
                int targetTonnageMod = (int)Math.Floor(targetTonnage / 5.0);

                int attackerTonnageMod = (int) Math.Floor(attackerTonnage / 5.0);
                SkillBasedInit.Logger.Log($"Raw attackerTonnageMod:{attackerTonnageMod} vs targetTonnageMod:{targetTonnageMod}");

                // DamageType DFA increases the shock
                if (damageType == DamageType.DFA) {
                    int attackerTonnageWithDFA = (int)Math.Floor(attackerTonnageMod * 1.5);
                    SkillBasedInit.Logger.Log($"DFA attack inficting additional slowdown - from {attackerTonnageMod} to {attackerTonnageWithDFA}");
                    attackerTonnageMod = attackerTonnageWithDFA;
                }

                // Check for juggernaut
                foreach (Ability ability in weapon.parent.GetPilot().Abilities) {
                    if (ability.Def.Id == "AbilityDefGu5") {
                        attackerTonnageMod = attackerTonnageMod * 2;
                        SkillBasedInit.Logger.Log($"Pilot {weapon.parent.GetPilot()} has the Juggernaught skill, doubled their impact to {attackerTonnageMod}!");
                    }
                }
                
                float delta = Math.Min(1, attackerTonnageMod - targetTonnageMod);
                SkillBasedInit.Logger.Log($"Impact on actor:{__instance.DisplayName} from:{weapon.parent.DisplayName} will inflict {delta} init slowdown!");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                actorInit.AddMeleeImpact(delta);
            }

        }
    }

    [HarmonyPatch(typeof(Vehicle), "DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(VehicleChassisLocations), typeof(Weapon), typeof(float), typeof(AttackImpactQuality) })]
    public static class Vehicle_DamageLocation {
        public static void Postfix(Vehicle __instance, WeaponHitInfo hitInfo, VehicleChassisLocations vLoc, Weapon weapon, AttackImpactQuality impactQuality) {
            if (weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.Logger.Log($"Vehicle:DamageLocation:post - vehicle {__instance.DisplayName} has suffered a melee attack.");                
            }                
        }
    }

    [HarmonyPatch(typeof(Turret), "DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(BuildingLocation), typeof(Weapon), typeof(float) })]
    public static class Turret_DamageLocation {
        public static void Postfix(Turret __instance, WeaponHitInfo hitInfo, BuildingLocation bLoc, Weapon weapon) {
            if (weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.Logger.Log($"Turret:DamageLocation:post - turret {__instance.DisplayName} has suffered a melee attack.");
            }
        }
    }

    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Postfix(TurnDirector __instance) {
            //SkillBasedInit.Logger.Log($"TurnDirector; Combat complete, destroying initiative map.");
            ActorInitiativeHolder.OnCombatComplete();
        }
    }

    [HarmonyPatch(typeof(TurnDirector))]
    [HarmonyPatch("FirstPhase", MethodType.Getter)]
    public static class TurnDirector_FirstPhase {
        public static void Postfix(TurnDirector __instance, ref int __result) {
            __result = 1;
        }
    }

    [HarmonyPatch(typeof(TurnDirector))]
    [HarmonyPatch("LastPhase", MethodType.Getter)]
    public static class TurnDirector_LastPhase {
        [HarmonyPostfix]
        public static void Postfix(TurnDirector __instance, ref int __result) {
            __result = SkillBasedInit.MaxPhase;
        }
    }


    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    public static class CombatHUDPhaseTrack_Init {

        public static void Postfix(CombatHUDPhaseTrack __instance, CombatGameState combat, CombatHUD HUD, List<CombatHUDPhaseIcons> ___PhaseIcons, CombatHUDReserveButton ___reserveButton) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack::Init - entered");

            __instance.OnCombatGameDestroyed();

            combat.MessageCenter.Subscribe(MessageCenterMessageType.OnRoundBegin, new ReceiveMessageCenterMessage(__instance.OnRoundBegin), true);
            combat.MessageCenter.Subscribe(MessageCenterMessageType.OnPhaseBegin, new ReceiveMessageCenterMessage(__instance.OnPhaseBegin), true);

            ___reserveButton.DisableButton();
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnRoundBegin")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class CombatHUDPhaseTrack_OnRoundBegin {
        public static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message, 
            TextMeshProUGUI ___roundCounterText, List<CombatHUDIconTracker> ___IconTrackers, 
            GameObject ___phaseTrack, GameObject[] ___phaseBarHolders ) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack::OnRoundBegin::post - Init");
            for (int i = 0; i < ___IconTrackers.Count; i++) {
                ___IconTrackers[i].Visible = false;
            }
            ___phaseTrack.SetActive(false);

            for (int i = 0; i < ___phaseBarHolders.Length; i++) {
                ___phaseBarHolders[i].SetActive(false);
            }            
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnPhaseBegin")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage)} )]
    public static class CombatHUDPhaseTrack_OnPhaseBegin {
        public static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message, TextMeshProUGUI ___roundCounterText) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack::OnPhaseBegin::post - Init");
            PhaseBeginMessage phaseBeginMessage = message as PhaseBeginMessage;
            string phaseText = string.Format("{0} - Phase {1}", phaseBeginMessage.Round, 31 - phaseBeginMessage.Phase);
            ___roundCounterText.SetText(phaseText, new object[0]);
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack::OnPhaseBegin::post - for {phaseText}");
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "SetTrackerPhase")]
    [HarmonyPatch(new Type[] { typeof(CombatHUDIconTracker), typeof(int) })]
    public static class CombatHUDPhaseTrack_SetTrackerPhase {
        public static bool Prefix(CombatHUDPhaseTrack __instance, CombatHUDIconTracker tracker, int phase) {      
            return false;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPortrait), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD), typeof(UnityEngine.UI.LayoutElement), typeof(HBSDOTweenToggle)})]
    public static class CombatHUDPortrait_Init {
        public static void Postfix(CombatHUDPortrait __instance, TextMeshProUGUI ___ioText, DOTweenAnimation ___initiativeOverlay) {
            //SkillBasedInit.Logger.Log($"CombatHUDPortrait::Init::post - Init");
            ___ioText.enableWordWrapping = false;
            ___initiativeOverlay.isActive = false;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    public static class CombatHUDPhaseDisplay_Init {
        public static void Postfix(CombatHUDPhaseDisplay __instance, TextMeshProUGUI ___NumText) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseDisplay::Init::post - Init");
            ___NumText.enableWordWrapping = false;
        }
    }

    [HarmonyPatch(typeof(MechBayMechInfoWidget), "SetInitiative")]
    [HarmonyPatch(new Type[] { })]
    public static class MechBayMechInfoWidget_SetInitiative {
        public static void Postfix(MechBayMechInfoWidget __instance, GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText) {
            //SkillBasedInit.Logger.Log($"MechBayMechInfoWidget::SetInitiative::post - disabling text");
            if (___initiativeObj.activeSelf) {
                ___initiativeText.SetText("{0}", new object[] { "-" });
            }
        }
    }

    [HarmonyPatch(typeof(LanceLoadoutSlot), "RefreshInitiativeData")]
    [HarmonyPatch(new Type[] { })]
    public static class LanceLoadoutSlot_RefreshInitiativeData {
        public static void Postfix(LanceLoadoutSlot __instance, GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText) {
            //SkillBasedInit.Logger.Log($"LanceLoadoutSlot::RefreshInitiativeData::post - disabling text");
            if (___initiativeObj != null && ___initiativeText != null && ___initiativeObj.activeSelf) {
                ___initiativeText.SetText("{0}", new object[] { "-" });
            }
        }
    }

    // Uncomment to prevent deferring
    //
    //[HarmonyPatch(typeof(AbstractActor))]
    //[HarmonyPatch("CanDeferUnit", MethodType.Getter)]
    //public static class AbstractActor_CanDeferUnit {
    //    public static void Postfix(AbstractActor __instance, ref bool __result) {
    //        SkillBasedInit.Logger.Log($"AbstractActor:CanDeferUnit:post - Preventing unit from deferring.");
    //        __result = false;
    //    }
    //}
    //
    //[HarmonyPatch(typeof(CombatSelectionHandler), "ShowReserveOrSelect")]
    //[HarmonyPatch(new Type[] { })]
    //public static class CombatSelectionHandler_ShowReserveOrSelect {
    //    public static void Postfix(CombatSelectionHandler __instance) {            
    //        CombatHUD ___HUD = (CombatHUD)Traverse.Create(__instance).Property("HUD").GetValue();
    //        CombatGameState ___Combat = (CombatGameState)Traverse.Create(__instance).Property("Combat").GetValue();

    //        SkillBasedInit.Logger.Log($"CombatSelectionHandler::ShowReserveOrSelect::post - HUD null:{___HUD == null} CGS null:{___Combat == null}");
    //        if (!___Combat.TurnDirector.IsInterleaved || ___Combat.TurnDirector.CurrentPhase == ___Combat.TurnDirector.LastPhase) {
    //            if (___Combat.Constants.CombatUIConstants.ShowDoneWithAllButton) {
    //                ___HUD.HideFireButton();
    //            }
    //            ___HUD.MechWarriorTray.ShowAvailableChevrons();
    //        } else {
    //            if (!___HUD.MechWarriorTray.IsReserveButtonSuppressed) {
    //                ___HUD.HideFireButton();
    //            }
    //            ___HUD.MechWarriorTray.ShowAvailableChevrons();
    //        }
    //    }
    //}

}
