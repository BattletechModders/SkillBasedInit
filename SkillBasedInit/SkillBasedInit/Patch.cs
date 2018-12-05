using BattleTech;
using BattleTech.Save.Test;
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
            //SkillBasedInit.Logger.Log($"AbstractActor: Starting new round {round}, recalculating initiative element for actor:{__instance.DisplayName}");
            ActorInitiativeHolder.OnRoundBegin(__instance);
        }
    }

    [HarmonyPatch(typeof(AbstractActor), "DeferUnit")]
    public static class AbstractActor_DeferUnit {
        public static void Postfix(AbstractActor __instance) {
            //SkillBasedInit.Logger.Log($"AbstractActor:DeferUnit:");
            var deferJump = SkillBasedInit.Random.Next(2, 7);
            SkillBasedInit.LogDebug($"AbstractActor:DeferUnit - Reducing actorInit:{__instance.Initiative} by :{deferJump} to {__instance.Initiative + deferJump}");

            __instance.Initiative += deferJump;
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

    // DANGER: We prevent the execution of the native method because it won't serialize properly. The HasValidInitiative method gets
    //  inlined, which prevents clean patching. We have to use the sledgehammer instead of the surgical knife unfortunately.
    [HarmonyPatch(typeof(AbstractActor), "Dehydrate")]
    [HarmonyPatch(new Type[] { typeof(SerializableReferenceContainer) })]
    public static class AbstractActor_Dehydrate {
        public static bool Prefix(AbstractActor __instance, SerializableReferenceContainer references,
            ref bool ___underscoreIsShutDown, bool ____isShutDown,
            ref string ___underscoreLanceID, string ____lanceId,
            ref string ___underscoreSpawnerID, string ____spawnerId,
            ref string ___underscoreTeamID, string ____teamId,
            ref bool ___serializableHasHandledDeath, 
            ref int ___serializableDeathLocation, int ____deathLocation,
            ref DeathMethod ___serializableDeathMethod, DeathMethod ____deathMethod,
            Team ____team, List<Weapon> ___weapons
            ) {
            SkillBasedInit.Logger.Log($"AbstractActor:Dehydrate - preventing native call to allow serialization of initiative");

            ___underscoreIsShutDown = ____isShutDown;
            ___underscoreLanceID = ____lanceId;
            ___underscoreSpawnerID = ____spawnerId;
            ___underscoreTeamID = ____teamId;
            ___serializableHasHandledDeath = __instance.HasHandledDeath;
            ___serializableDeathLocation = ____deathLocation;
            ___serializableDeathMethod = ____deathMethod;
            references.AddItem<Team>(__instance, "_team", ____team);
            if (__instance.BehaviorTree != null) {
                __instance.BehaviorTree.Dehydrate(references);
            }
            references.AddItemList<Weapon>(__instance, "Weapons", ___weapons);
            references.AddItemList<AmmunitionBox>(__instance, "AmmoBox", __instance.ammoBoxes);
            references.AddItemList<Jumpjet>(__instance, "JumpJets", __instance.jumpjets);
            references.AddItem<Weapon>(__instance, "ImaginaryLaser", __instance.ImaginaryLaserWeapon);
            if (! (__instance.Initiative > 0 && __instance.Initiative < (SkillBasedInit.MaxPhase + 1)) ) {
                SkillBasedInit.Logger.LogError(string.Format("Saving an AbstractActor of type {0} with an invalid initiative of {1}", __instance.ClassName, __instance.Initiative));
            }

            return false;
        }
    }

    // DANGER: We prevent the execution of the native method because it won't serialize properly. The HasValidInitiative method gets
    //  inlined, which prevents clean patching. We have to use the sledgehammer instead of the surgical knife unfortunately.
    [HarmonyPatch(typeof(AbstractActor), "Hydrate")]
    [HarmonyPatch(new Type[] { typeof(SerializableReferenceContainer), typeof(CombatGameState) })]
    public static class AbstractActor_Hydrate {
        public static bool Prefix(AbstractActor __instance, SerializableReferenceContainer references, CombatGameState loadedState,
            ref bool ____isShutDown, bool ___underscoreIsShutDown, 
            ref string ____lanceId, string ___underscoreLanceID, 
            ref string ____spawnerId, string ___underscoreSpawnerID, 
            ref string ____teamId, string ___underscoreTeamID, 
            ref Team ____team, 
            ref Lance ____lance, 
            ref bool ____hasHandledDeath, bool ___serializableHasHandledDeath,
            ref int ____deathLocation, int ___serializableDeathLocation,
            ref DeathMethod ____deathMethod, DeathMethod ___serializableDeathMethod,             
            List<Weapon> ___weapons
            ) {
            SkillBasedInit.Logger.Log($"AbstractActor:Hydrate - preventing native call to allow deserialization of initiative");

            Traverse.Create(__instance).Property("Combat").SetValue(loadedState);           

            ____isShutDown = ___underscoreIsShutDown;
            ____lanceId = ___underscoreLanceID;
            ____spawnerId = ___underscoreSpawnerID;
            ____teamId = ___underscoreTeamID;
            ____team = references.GetItem<Team>(__instance, "_team");
            ____lance = ____team.GetLanceByUID(____lanceId);
            ____hasHandledDeath = ___serializableHasHandledDeath;
            ____deathLocation = ___serializableDeathLocation;
            ____deathMethod = ___serializableDeathMethod;
            if (__instance.BehaviorTree != null) {
                __instance.BehaviorTree.Hydrate(loadedState, references);
            }
            ___weapons = references.GetItemList<Weapon>(__instance, "Weapons");
            foreach (Weapon weapon in __instance.Weapons) {
                weapon.Hydrate(references);
                __instance.allComponents.Add(weapon);
            }
            __instance.ammoBoxes = references.GetItemList<AmmunitionBox>(__instance, "AmmoBox");
            foreach (AmmunitionBox ammunitionBox in __instance.ammoBoxes) {
                ammunitionBox.Hydrate(references);
                __instance.allComponents.Add(ammunitionBox);
            }
            __instance.jumpjets = references.GetItemList<Jumpjet>(__instance, "JumpJets");
            foreach (Jumpjet jumpjet in __instance.jumpjets) {
                jumpjet.Hydrate(references);
                __instance.allComponents.Add(jumpjet);
            }
            __instance.ImaginaryLaserWeapon = references.GetItem<Weapon>(__instance, "ImaginaryLaser");
            if (__instance.ImaginaryLaserWeapon != null) {
                __instance.ImaginaryLaserWeapon.Hydrate(references);
            }
            if (__instance.CustomHeraldryDef != null) {
                __instance.CustomHeraldryDef.RequestResources(loadedState.DataManager, true);
            }
            if (!(__instance.Initiative > 0 && __instance.Initiative < (SkillBasedInit.MaxPhase + 1))) {
                SkillBasedInit.Logger.Log(string.Format("Loading an AbstractActor of type {0} with an invalid initiative of {1}, Reverting to BaseInitiative", __instance.ClassName, __instance.Initiative));
                __instance.Initiative = __instance.BaseInitiative;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(Mech), "CompleteKnockdown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int) })]
    public static class Mech_CompleteKnockdown {

        public static string KnockdownSourceID;
        public static int KnockdownStackItemUID;

        public static bool Prefix(Mech __instance, string sourceID, int stackItemUID) {
            //SkillBasedInit.Logger.Log($"Mech:CompleteKnockdown:prefix - Recording sourceID:{sourceID} stackItemUID:{stackItemUID}");
            KnockdownSourceID = sourceID;
            KnockdownStackItemUID = stackItemUID;            
            return true;
        }

        public static void Postfix(Mech __instance) {
            //SkillBasedInit.Logger.Log($"Mech:CompleteKnockdown:prefix - Removing record.");
            KnockdownSourceID = null;
            KnockdownStackItemUID = -1;
        }
    }

    // Required to apply the negative as soon as it happens in the knockdown case
    [HarmonyPatch(typeof(AbstractActor), "ForceUnitOnePhaseDown")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(bool) })]
    public static class AbstractActor_ForceUnitOnePhaseDown {
        public static bool Prefix(AbstractActor __instance, string sourceID, int stackItemUID, bool addedBySelf) {
            //SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - Init - sourceID:{sourceID} vs actor: {__instance.GUID}");
            bool shouldReturn;
            if (sourceID != Mech_CompleteKnockdown.KnockdownSourceID || stackItemUID != Mech_CompleteKnockdown.KnockdownStackItemUID) {
                //SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - Not from knockdown, deferring to original call");
                shouldReturn = true;
            } else {
                //SkillBasedInit.Logger.Log($"AbstractActor:ForceUnitOnePhaseDown:prefix - From knockdown, executing changed logic");
                shouldReturn = false;

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                int knockDownMod = SkillBasedInit.Random.Next(actorInit.injuryBounds[0], actorInit.injuryBounds[1]);
                SkillBasedInit.Logger.Log($"AbstractActor::ForceUnitOnePhaseDown modifying unit initiative by {knockDownMod} due to knockdown!");

                if (__instance.HasActivatedThisRound || __instance.Initiative != SkillBasedInit.MaxPhase) {
                    SkillBasedInit.Logger.Log($"Knockdown will slow actor:{__instance.DisplayName} by {knockDownMod} init on next activation!");
                } else {
                    SkillBasedInit.Logger.Log($"Knockdown immediately slows actor:{__instance.DisplayName} by {knockDownMod} init!");
                    if (__instance.Combat.TurnDirector.IsInterleaved && __instance.Initiative != SkillBasedInit.MaxPhase) {
                        __instance.Initiative = __instance.Initiative + knockDownMod;
                        if (__instance.Initiative + knockDownMod > SkillBasedInit.MaxPhase) {
                            __instance.Initiative = SkillBasedInit.MaxPhase;
                        } 
                        __instance.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(__instance.GUID));
                    }
                }
                __instance.Combat.MessageCenter.PublishMessage(new FloatieMessage(__instance.GUID, __instance.GUID, $"-{knockDownMod} INITIATIVE", FloatieMessage.MessageNature.Debuff));

                string statName = (!addedBySelf) ? "PhaseModifier" : "PhaseModifierSelf";
                __instance.StatCollection.ModifyStat<int>(sourceID, stackItemUID, statName, StatCollection.StatOperation.Int_Add, knockDownMod, -1, true);

            }            
            return shouldReturn;
        }
    }

    [HarmonyPatch(typeof(Mech), "DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
    public static class Mech_DamageLocation {
        public static void Postfix(Mech __instance, WeaponHitInfo hitInfo, Weapon weapon, AttackImpactQuality impactQuality, DamageType damageType) {
            if (weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.LogDebug($"Mech:DamageLocation:post - mech {__instance.DisplayName} has suffered a melee attack from:{weapon.parent.DisplayName}.");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                float delta = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                if (delta != 0) {
                    // DamageType DFA increases the shock for mechs
                    if (damageType == DamageType.DFA) {
                        int deltaWithDFA = (int)Math.Ceiling(delta * SkillBasedInit.Settings.MechMeleeDFAMulti);
                        SkillBasedInit.LogDebug($"DFA attack inficting additional slowdown - from {delta} to {deltaWithDFA}");
                        delta = deltaWithDFA;
                    }
                }

                actorInit.ResolveMeleeImpact(__instance, delta);
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle), "DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(VehicleChassisLocations), typeof(Weapon), typeof(float), typeof(AttackImpactQuality) })]
    public static class Vehicle_DamageLocation {
        public static void Postfix(Vehicle __instance, WeaponHitInfo hitInfo, VehicleChassisLocations vLoc, Weapon weapon, AttackImpactQuality impactQuality) {
            if (weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.LogDebug($"Vehicle:DamageLocation:post - vehicle {__instance.DisplayName} has suffered a melee attack from:{weapon.parent.DisplayName}.");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                float delta = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                actorInit.ResolveMeleeImpact(__instance, delta);
            }
        }
    }

    [HarmonyPatch(typeof(Turret), "DamageLocation")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(BuildingLocation), typeof(Weapon), typeof(float) })]
    public static class Turret_DamageLocation {
        public static void Postfix(Turret __instance, WeaponHitInfo hitInfo, BuildingLocation bLoc, Weapon weapon) {
            if (weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.LogDebug($"Turret:DamageLocation:post - turret {__instance.DisplayName} has suffered a melee attack from:{weapon.parent.DisplayName}.");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                float delta = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                actorInit.ResolveMeleeImpact(__instance, delta);
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

    // Prevents the init tracker line at the top from loading
    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "SetTrackerPhase")]
    [HarmonyPatch(new Type[] { typeof(CombatHUDIconTracker), typeof(int) })]
    public static class CombatHUDPhaseTrack_SetTrackerPhase {
        public static bool Prefix(CombatHUDPhaseTrack __instance, CombatHUDIconTracker tracker, int phase) {      
            return false;
        }
    }

    // Corrects the init overlay displayed on the Mechwarrior
    [HarmonyPatch(typeof(CombatHUDPortrait), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD), typeof(UnityEngine.UI.LayoutElement), typeof(HBSDOTweenToggle)})]
    public static class CombatHUDPortrait_Init {
        public static void Postfix(CombatHUDPortrait __instance, ref TextMeshProUGUI ___ioText, ref DOTweenAnimation ___initiativeOverlay) {
            //SkillBasedInit.Logger.Log($"CombatHUDPortrait::Init::post - Init");
            ___ioText.enableWordWrapping = false;
            ___initiativeOverlay.isActive = false;
        }
    }

    // TODO: Dead code? 
    // Manipulates the icon to the upper left of the mech panel
    //[HarmonyPatch(typeof(CombatHUDPhaseDisplay), "Init")]
    //[HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    //public static class CombatHUDPhaseDisplay_Init {
    //    public static void Postfix(CombatHUDPhaseDisplay __instance, ref TextMeshProUGUI ___NumText) {
    //        //SkillBasedInit.Logger.Log($"CombatHUDPhaseDisplay::Init::post - Init");
    //        ___NumText.enableWordWrapping = false;
    //        ___NumText.fontSize = 18;
    //    }
    //}

    // Manipulates the badge icon to the left of the mech's floating damage/status bars
    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "RefreshInfo")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPhaseDisplay_RefreshInfo {
        public static void Postfix(CombatHUDPhaseDisplay __instance, ref TextMeshProUGUI ___NumText) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseDisplay::RefreshInfo::post - Init");
            ___NumText.enableWordWrapping = false;
            ___NumText.fontSize = 18;
        }
    }

    // Sets the initiative value in the mech-bay
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

    // Sets the initiative value in the lance loading screen
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
