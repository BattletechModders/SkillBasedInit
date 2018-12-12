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

                ActorInitiative attacker = ActorInitiativeHolder.GetOrCreate(weapon.parent);
                ActorInitiative target = ActorInitiativeHolder.GetOrCreate(__instance);
                int deltaMod = ActorInitiative.CalculateMeleeDelta(attacker, target);
                target.ResolveMeleeImpact(__instance, deltaMod);
            }
        }
    }

    [HarmonyPatch(typeof(Vehicle), "TakeWeaponDamage")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(Weapon), typeof(float), typeof(int), typeof(DamageType) })]
    public static class Vehicle_TakeWeaponDamage {
        public static void Postfix(Vehicle __instance, WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType) {
            if (weapon != null && weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.LogDebug($"Vehicle:TakeWeaponDamage:post - Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has suffered a melee attack from:{weapon.parent.DisplayName}.");

                ActorInitiative attacker = ActorInitiativeHolder.GetOrCreate(weapon.parent);
                ActorInitiative target = ActorInitiativeHolder.GetOrCreate(__instance);
                int deltaMod = ActorInitiative.CalculateMeleeDelta(attacker, target);
                target.ResolveMeleeImpact(__instance, deltaMod);
            }
        }
    }

    // public override void TakeWeaponDamage (WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType)
    [HarmonyPatch(typeof(Turret), "TakeWeaponDamage")]
    [HarmonyPatch(new Type[] { typeof(WeaponHitInfo), typeof(int), typeof(Weapon), typeof(float), typeof(int), typeof(DamageType) })]
    public static class Turret_TakeWeaponDamage {
        public static void Postfix(Turret __instance, WeaponHitInfo hitInfo, int hitLocation, Weapon weapon, float damageAmount, int hitIndex, DamageType damageType) {
            if (weapon != null && weapon.Category == WeaponCategory.Melee) {
                SkillBasedInit.LogDebug($"Turret:TakeWeaponDamage:post - Actor:({__instance.DisplayName}_{__instance.GetPilot().Name}) has suffered a melee attack from:{weapon.parent.DisplayName}.");

                ActorInitiative attacker = ActorInitiativeHolder.GetOrCreate(weapon.parent);
                ActorInitiative target = ActorInitiativeHolder.GetOrCreate(__instance);
                int deltaMod = ActorInitiative.CalculateMeleeDelta(attacker, target);
                target.ResolveMeleeImpact(__instance, deltaMod);
            }
        }
    }

}
