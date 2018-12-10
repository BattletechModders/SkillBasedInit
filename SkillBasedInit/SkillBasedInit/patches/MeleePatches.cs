using BattleTech;
using Harmony;
using System;

namespace SkillBasedInit {

    // TODO: If an attack damages multiple locations, this gets applied multiple times! Fix this!
    // TODO: Should embraced/guarded reduce this as well?

    [HarmonyPatch(typeof(Mech), "TakeWeaponDamage")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(Weapon), typeof(float), typeof(int), typeof(DamageType) })]
    //public override void TakeWeaponDamage (WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType)
    public static class Mech_TakeWeaponDamage {
        public static void Postfix(Mech __instance, WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType) {
            if (weapon != null && weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.LogDebug($"Mech:TakeWeaponDamage:post - Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has suffered a melee attack from:{weapon.parent.DisplayName}.");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                int deltaMod = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                if (damageType == DamageType.DFA) {
                    // TODO: Prevent the multiple initiative warnings
                    SkillBasedInit.LogDebug($"DFA attack - was melee mod: {deltaMod} applied twice?");
                }

                actorInit.ResolveMeleeImpact(__instance, deltaMod);
            }
        }
    }

    // TODO: OLD APPROACH 
    //[HarmonyPatch(typeof(Mech), "DamageLocation")]
    //[HarmonyPatch(new Type[] { typeof(int), typeof(WeaponHitInfo), typeof(ArmorLocation), typeof(Weapon), typeof(float), typeof(int), typeof(AttackImpactQuality), typeof(DamageType) })]
    //public static class Mech_DamageLocation {
    //    public static void Postfix(Mech __instance, WeaponHitInfo hitInfo, Weapon weapon, AttackImpactQuality impactQuality, DamageType damageType) {
    //        if (weapon != null && weapon.Category == WeaponCategory.Melee) {
    //            SkillBasedInit.LogDebug($"Mech:DamageLocation:post - Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has suffered a melee attack from:{weapon.parent.DisplayName}.");

    //            ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
    //            int deltaMod = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

    //            if (damageType == DamageType.DFA) {
    //                // TODO: Prevent the multiple initiative warnings
    //                SkillBasedInit.LogDebug($"DFA attack - melee mod: {deltaMod} will be applied twice!");
    //            }

    //            actorInit.ResolveMeleeImpact(__instance, deltaMod);
    //        }
    //    }
    //}
    // public override void TakeWeaponDamage (WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType)

    [HarmonyPatch(typeof(Vehicle), "TakeWeaponDamage")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(Weapon), typeof(float), typeof(int), typeof(DamageType) })]
    public static class Vehicle_TakeWeaponDamage {
        public static void Postfix(Vehicle __instance, WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType) {
            if (weapon != null && weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.LogDebug($"Vehicle:TakeWeaponDamage:post - Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has suffered a melee attack from:{weapon.parent.DisplayName}.");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                int deltaMod = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                actorInit.ResolveMeleeImpact(__instance, deltaMod);
            }
        }
    }

    // TODO: OLD APPROACH 
    //[HarmonyPatch(typeof(Vehicle), "DamageLocation")]
    //[HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(VehicleChassisLocations), typeof(Weapon), typeof(float), typeof(AttackImpactQuality) })]
    //public static class Vehicle_DamageLocation
    //{
    //    public static void Postfix(Vehicle __instance, WeaponHitInfo hitInfo, VehicleChassisLocations vLoc, Weapon weapon, AttackImpactQuality impactQuality)
    //    {
    //        if (weapon != null && weapon.Category == WeaponCategory.Melee)
    //        {
    //            SkillBasedInit.LogDebug($"Vehicle:DamageLocation:post - Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has suffered a melee attack from:{weapon.parent.DisplayName}.");

    //            ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
    //            int deltaMod = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

    //            actorInit.ResolveMeleeImpact(__instance, deltaMod);
    //        }
    //    }
    //}

    // public override void TakeWeaponDamage (WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType)
    [HarmonyPatch(typeof(Turret), "TakeWeaponDamage")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(Weapon), typeof(float), typeof(int), typeof(DamageType) })]
    public static class Turret_TakeWeaponDamage {
        public static void Postfix(Turret __instance, WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType) {
            if (weapon != null && weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.LogDebug($"Turret:TakeWeaponDamage:post - Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has suffered a melee attack from:{weapon.parent.DisplayName}.");

                ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
                int deltaMod = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

                actorInit.ResolveMeleeImpact(__instance, deltaMod);
            }
        }
    }

    // TODO: OLD APPROACH
    //[HarmonyPatch(typeof(Turret), "DamageLocation")]
    //[HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(BuildingLocation), typeof(Weapon), typeof(float) })]
    //public static class Turret_DamageLocation
    //{
    //    public static void Postfix(Turret __instance, WeaponHitInfo hitInfo, BuildingLocation bLoc, Weapon weapon)
    //    {
    //        if (weapon != null && weapon.Category == WeaponCategory.Melee)
    //        {
    //            SkillBasedInit.LogDebug($"Turret:DamageLocation:post - Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has suffered a melee attack from:{weapon.parent.DisplayName}.");

    //            ActorInitiative actorInit = ActorInitiativeHolder.ActorInitMap[__instance.GUID];
    //            int deltaMod = ActorInitiative.CalculateMeleeDelta(actorInit.tonnage, weapon);

    //            actorInit.ResolveMeleeImpact(__instance, deltaMod);
    //        }
    //    }
    //}


}
