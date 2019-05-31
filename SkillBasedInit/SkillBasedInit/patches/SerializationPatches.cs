using BattleTech;
using BattleTech.Data;
using BattleTech.Save.Test;
using Harmony;
using System;
using System.Collections.Generic;

namespace SkillBasedInit {

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
            //SkillBasedInit.Logger.Log($"AbstractActor:Dehydrate - preventing native call to allow serialization of initiative");

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
            if (! (__instance.Initiative > 0 && __instance.Initiative < (Mod.MaxPhase + 1)) ) {
                Mod.Log.Info(string.Format("Saving an AbstractActor of type {0} with an invalid initiative of {1}", __instance.ClassName, __instance.Initiative));
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
            //SkillBasedInit.Logger.Log($"AbstractActor:Hydrate - preventing native call to allow deserialization of initiative");

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
                Traverse requestResources = Traverse.Create(__instance).Method("RequestResources", new Type[] { typeof(DataManager), typeof(Action) });
                requestResources.GetValue(new object[] { loadedState.DataManager, true });
            }

            // OriginalLogic: Initiative > 0 && Initiative < 6;
            if (!(__instance.Initiative > 0 && __instance.Initiative <= Mod.MaxPhase)) {
                Mod.Log.Info(string.Format("Loading an AbstractActor of type {0} with an invalid initiative of {1}, Reverting to BaseInitiative", __instance.ClassName, __instance.Initiative));
                __instance.Initiative = 1;
            }

            return false;
        }
    }
}
