using BattleTech;
using BattleTech.UI;
using DG.Tweening;
using Harmony;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace SkillBasedInit {

    [HarmonyPatch(typeof(TurnDirector), MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
    public static class TurnDirector_ctor {
        public static void Postfix(TurnDirector __instance) {
            SkillBasedInit.Logger.Log($"TurnDirector::ctor::post - patching values");
            int ___FirstPhase = (int)Traverse.Create(__instance).Property("FirstPhase").GetValue();
            int ___LastPhase = (int)Traverse.Create(__instance).Property("LastPhase").GetValue();
            SkillBasedInit.Logger.Log($"TurnDirector::ctor::post - was initialized with {___FirstPhase} / {___LastPhase}");

            Traverse.Create(__instance).Property("FirstPhase").SetValue(1);
            Traverse.Create(__instance).Property("LastPhase").SetValue(SkillBasedInit.MaxPhase);
        }
    }

    [HarmonyPatch(typeof(TurnDirector), "OnCombatGameDestroyed")]
    public static class TurnDirector_OnCombatGameDestroyed {
        public static void Postfix(TurnDirector __instance) {
            SkillBasedInit.Logger.Log($"TurnDirector; Combat complete, destroying initiative map.");
            ActorInitiativeHolder.OnCombatComplete();
        }
    }

    //[HarmonyPatch(typeof(TurnDirector))]
    //[HarmonyPatch("FirstPhase", MethodType.Getter)]
    //public static class TurnDirector_FirstPhase {
    //    public static void Postfix(TurnDirector __instance, ref int __result) {
    //        SkillBasedInit.Logger.Log($"TurnDirector::LastPhase::post - returning 1");
    //        __result = 1;
    //    }
    //}

    //[HarmonyPatch(typeof(TurnDirector))]
    //[HarmonyPatch("LastPhase", MethodType.Getter)]
    //public static class TurnDirector_LastPhase {
    //    public static void Postfix(TurnDirector __instance, ref int __result) {
    //        SkillBasedInit.Logger.Log($"TurnDirector::LastPhase::post - returning {SkillBasedInit.MaxPhase}");
    //        __result = SkillBasedInit.MaxPhase;
    //    }
    //}


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

    [HarmonyPatch(typeof(Mech), "CompleteKnockdown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    public static class Mech_CompleteKnockdown {

        public static string KnockdownSourceID;
        public static int KnockdownStackItemUID;

        public static bool Prefix(Mech __instance, string sourceID, int stackItemUID) {
            SkillBasedInit.Logger.Log($"Mech:CompleteKnockdown:prefix - Recording sourceID:{sourceID} stackItemUID:{stackItemUID}");
            KnockdownSourceID = sourceID;
            KnockdownStackItemUID = stackItemUID;            
            return true;
        }

        public static void Postfix(Mech __instance) {
            SkillBasedInit.Logger.Log($"Mech:CompleteKnockdown:prefix - Removing record.");
            KnockdownSourceID = null;
            KnockdownStackItemUID = -1;
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "ForceUnitOnePhaseDown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public static class AbstractActor_ForceUnitOnePhaseDown {
        public static bool Prefix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - Init - sourceID:{sourceID} vs actor: {__instance.GUID}");
            bool shouldReturn;
            if (sourceID != Mech_CompleteKnockdown.KnockdownSourceID || stackItemUID != Mech_CompleteKnockdown.KnockdownStackItemUID) {
                SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - Not from knockdown, deferring to original call");
                shouldReturn = true;
            } else {
                SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - From knockdown, executing changed logic");
                shouldReturn = false;
                if (__instance.Combat.TurnDirector.IsInterleaved && __instance.Initiative != SkillBasedInit.MaxPhase) {
                    ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                    int knockDownMod = SkillBasedInit.Random.Next(actorInit.injuryBounds[0], actorInit.injuryBounds[1]);
                    SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown modifying unit initiative by {knockDownMod} due to knockdown!");
                    __instance.Initiative = __instance.Initiative + knockDownMod;
                    __instance.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(__instance.GUID));
                    __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, $"-{knockDownMod} INITIATIVE", FloatieMessage.MessageNature.Debuff));
                    string statName = (!addedBySelf) ? "PhaseModifier" : "PhaseModifierSelf";
                    __instance.StatCollection.ModifyStat<int>(sourceID, stackItemUID, statName, StatCollection.StatOperation.Int_Add, knockDownMod, -1, true);
                }
            }            
            return shouldReturn;
        }
    }

    [HarmonyPatch(typeof(Mech), "DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
    public static class Mech_DamageLocation {
        public static void Postfix(Mech __instance, WeaponHitInfo hitInfo, Weapon weapon, AttackImpactQuality impactQuality, DamageType damageType) {
            if (weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.Logger.Log($"Mech:DamageLocation:post - mech {__instance.DisplayName} has suffered a melee attack.");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                float delta = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                if (delta != 0) {
                    // DamageType DFA increases the shock for mechs
                    if (damageType == DamageType.DFA) {
                        int deltaWithDFA = (int)Math.Floor(delta * SkillBasedInit.settings.MechMeleeDFAMulti);
                        SkillBasedInit.Logger.Log($"DFA attack inficting additional slowdown - from {delta} to {deltaWithDFA}");
                        delta = deltaWithDFA;
                    }
                }

                SkillBasedInit.Logger.Log($"Impact on actor:{__instance.DisplayName} from:{weapon.parent.DisplayName} will inflict {delta} init slowdown!");
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

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                float delta = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                SkillBasedInit.Logger.Log($"Impact on actor:{__instance.DisplayName} from:{weapon.parent.DisplayName} will inflict {delta} init slowdown!");
                actorInit.AddMeleeImpact(delta);
            }                
        }
    }

    [HarmonyPatch(typeof(Turret), "DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(BuildingLocation), typeof(Weapon), typeof(float) })]
    public static class Turret_DamageLocation {
        public static void Postfix(Turret __instance, WeaponHitInfo hitInfo, BuildingLocation bLoc, Weapon weapon) {
            if (weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.Logger.Log($"Turret:DamageLocation:post - turret {__instance.DisplayName} has suffered a melee attack.");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                float delta = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                SkillBasedInit.Logger.Log($"Impact on actor:{__instance.DisplayName} from:{weapon.parent.DisplayName} will inflict {delta} init slowdown!");                
                actorInit.AddMeleeImpact(delta);
            }
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
