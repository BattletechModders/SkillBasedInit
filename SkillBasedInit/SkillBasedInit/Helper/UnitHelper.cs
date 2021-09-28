using System;
using System.Collections.Generic;
using BattleTech;
using CustAmmoCategories;
using CustomUnits;
using HBS.Collections;
using IRBTModUtils.Extension;

namespace SkillBasedInit.Helper
{
    public static class UnitHelper
    {
        
        public static bool IsQuadMech(ICombatant combatant)
        {
            if (combatant is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                return customInfo != null && customInfo.ArmsCountedAsLegs;
            }
            return false;
        }


        public static bool IsQuadMech(MechDef mechDef)
        {
            if (mechDef == null) return false;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return false;

            return customInfo.ArmsCountedAsLegs;
        }

        public static bool IsTrooper(ICombatant combatant)
        {
            if (combatant is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                return customInfo != null && customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1;
            }
            return false;

        }
        public static bool IsTrooper(MechDef mechDef)
        {
            if (mechDef == null) return false;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return false;

            return customInfo?.SquadInfo?.Troopers > 1;
        }

        public static bool IsNaval(ICombatant combatant)
        {
            if (combatant is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                return customInfo != null && customInfo.Naval;
            }
            return false;

        }

        public static bool IsNaval(MechDef mechDef)
        {
            if (mechDef == null) return false;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return false;

            return customInfo.Naval;
        }

        public static bool IsVehicle(ICombatant combatant)
        {
            if (combatant is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                return customInfo != null && customInfo.FakeVehicle;
            }
            else if (combatant is Vehicle)
            {
                return true;
            }
            return false;

        }

        public static bool IsVehicle(MechDef mechDef)
        {
            if (mechDef == null) return false;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return false;

            return customInfo.FakeVehicle;
        }


        public static UnitCfg GetUnitConfig(AbstractActor actor)
        {
            if (actor is Turret)
            {
                return Mod.Config.Turret;
            }
            else if (actor is Vehicle)
            {
                return Mod.Config.Vehicle;
            }
            else if (actor is Mech mech)
            {
                if (mech.FakeVehicle())
                {
                    return Mod.Config.Vehicle;
                }
                else if (mech.NavalUnit())
                {
                    return Mod.Config.Naval;

                }
                else if (mech.TrooperSquad())
                {
                    return Mod.Config.Trooper;
                }
                else
                {
                    return Mod.Config.Mech;
                }
            }
            else
            {
                return Mod.Config.Mech;
            }
        }

        public static float GetUnitTonnage(AbstractActor actor)
        {

            Mod.Log.Debug?.Write($"Calculating unit tonnage for actor: {actor.DistinctId()}");
            float tonnage;
            if (actor is Turret)
            {
                TagSet actorTags = actor.GetTags();
                if (actorTags != null && actorTags.Contains("unit_light"))
                {
                    tonnage = Mod.Config.Turret.LightTonnage;
                    Mod.Log.Debug?.Write($" -- unit is a unit_light turret, using tonnage: {tonnage}");
                }
                else if (actorTags != null && actorTags.Contains("unit_medium"))
                {
                    tonnage = Mod.Config.Turret.MediumTonnage;
                    Mod.Log.Debug?.Write($" -- unit is unit_medium turret, using tonnage: {tonnage}");
                }
                else if (actorTags != null && actorTags.Contains("unit_heavy"))
                {
                    tonnage = Mod.Config.Turret.HeavyTonnage;
                    Mod.Log.Debug?.Write($" -- unit is a unit_heavy turret, using tonnage: {tonnage}");
                }
                else
                {
                    tonnage = Mod.Config.Turret.DefaultTonnage;
                    Mod.Log.Debug?.Write($" -- unit is tagless turret, using tonnage: {tonnage}");
                }
            }
            else if (actor is Vehicle vehicle)
            {
                tonnage = vehicle.tonnage;
                Mod.Log.Debug?.Write($" -- unit is a vehicle, using tonnage: {tonnage}");
            }
            else if (actor is Mech mech)
            {
                if (mech.FakeVehicle())
                {
                    tonnage = mech.tonnage;
                    Mod.Log.Debug?.Write($" -- unit is a fake vehicle, using tonnage: {tonnage}");
                }
                else if (mech.NavalUnit())
                {
                    tonnage = mech.tonnage;
                    Mod.Log.Debug?.Write($" -- unit is a naval unit, using tonnage: {tonnage}");
                }
                else if (mech.TrooperSquad())
                {
                    TrooperSquad squad = mech as TrooperSquad;
                    tonnage = (float)Math.Ceiling(squad.tonnage / squad.info.SquadInfo.Troopers);
                    Mod.Log.Debug?.Write($" -- unit is a trooper squad, using tonnage: {tonnage}");
                }
                else
                {
                    tonnage = mech.tonnage;
                    Mod.Log.Debug?.Write($" -- unit is a mech, using tonnage: {tonnage}");
                }

            }
            else
            {
                UnitCfg unitConfig = UnitHelper.GetUnitConfig(actor);
                tonnage = unitConfig.DefaultTonnage;
                Mod.Log.Debug?.Write($" -- unit tonnage is unknown, using tonnage: {tonnage}");
            }

            return tonnage;
        }

        public static int GetTonnageModifier(AbstractActor actor)
        {
            int unitTonnage = (int) Math.Ceiling(GetUnitTonnage(actor));

            UnitCfg opts = GetUnitConfig(actor);
            int initBase = 0;
            foreach (KeyValuePair<int, int> kvp in opts.InitBaseByTonnage)
            {
                if (kvp.Key > unitTonnage)
                {
                    initBase = kvp.Value;
                }
                else
                {
                    break;
                }
            }

            Mod.Log.Debug?.Write($"Using tonnageMod: {initBase} for actor: {actor.DistinctId()} with tonnage: {unitTonnage}");
            return initBase;
        }

        public static int GetTypeModifier(AbstractActor actor)
        {
            int typeMod;
            if (actor is Turret)
            {
                typeMod = Mod.Config.Turret.TypeMod;
                Mod.Log.Debug?.Write($" -- unit is type turret, using typeMod: {typeMod}");
            }
            else if (actor is Vehicle)
            {
                typeMod = Mod.Config.Vehicle.TypeMod;
                Mod.Log.Debug?.Write($" -- unit is type vehicle, using typeMod: {typeMod}");
            }
            else if (actor is Mech mech)
            {
                if (mech.FakeVehicle())
                {
                    typeMod = Mod.Config.Vehicle.TypeMod;
                    Mod.Log.Debug?.Write($" -- unit is type vehicle, using typeMod: {typeMod}");
                }
                else if (mech.NavalUnit())
                {
                    typeMod = Mod.Config.Naval.TypeMod;
                    Mod.Log.Debug?.Write($" -- unit is type naval, using typeMod: {typeMod}");

                }
                else if (mech.TrooperSquad())
                {
                    typeMod = Mod.Config.Trooper.TypeMod;
                    Mod.Log.Debug?.Write($" -- unit is type troopers, using typeMod: {typeMod}");
                }
                else
                {
                    typeMod = Mod.Config.Mech.TypeMod;
                    Mod.Log.Debug?.Write($" -- unit is type mech, using typeMod: {typeMod}");
                }
            }
            else
            {
                typeMod = 0;
                Mod.Log.Debug?.Write(" -- unit is unknown type, using 0 typeMod.");
            }

            return typeMod;
        }

        // Prone only applies to mechs and quads
        public static int ProneInitModifier(this AbstractActor actor)
        {
            if (!(actor is Mech)) return 0;

            Mech mech = actor as Mech;
            if (!mech.IsProne) return 0;

            UnitCustomInfo customInfo = mech.GetCustomInfo();
            if (customInfo != null && (customInfo.Naval || customInfo.FakeVehicle)) return 0;
            if (customInfo != null && customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1) return 0;

            int boundsMin = Mod.Config.Mech.ProneModifierMin;
            int boundsMax = Mod.Config.Mech.ProneModifierMax;
            Mod.Log.Debug?.Write($"Unit: {mech.DistinctId()} is prone, raw bounds => min: {boundsMin} to max: {boundsMax}");

            Pilot pilot = actor.GetPilot();
            int pilotMod = pilot.CurrentSkillMod(pilot.Piloting, ModStats.MOD_SKILL_PILOT);
            int adjustedMax = boundsMax - pilotMod;

            int finalMod = adjustedMax;
            if (adjustedMax <= boundsMin)
            {
                finalMod = boundsMin;
                Mod.Log.Debug?.Write($"  adjustedMax < boundsMin, returning {boundsMin}");
            }

            Mod.Log.Debug?.Write($"  finalMod: {finalMod}");
            return finalMod;
        }

        // Crippled applies to mechs, quads, vehicles, naval
        public static int CrippledInitModifier(this AbstractActor actor)
        {
            if (!((actor is Vehicle) || (actor is Mech))) return 0;

            int boundsMin = 0, boundsMax = 0;
            if (actor is Vehicle vehicle &&
                (vehicle.IsLocationDestroyed(VehicleChassisLocations.Left) || vehicle.IsLocationDestroyed(VehicleChassisLocations.Right))
                )
            {
                boundsMin = Mod.Config.Vehicle.CrippledModifierMin;
                boundsMax = Mod.Config.Vehicle.CrippledModifierMax;
            }
            else if (actor is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                if (customInfo != null && customInfo.FakeVehicle)
                {
                    // CU treats legs as vehicle sides. 
                    if (mech.IsLegged)
                    {
                        boundsMin = Mod.Config.Vehicle.CrippledModifierMin;
                        boundsMax = Mod.Config.Vehicle.CrippledModifierMax;
                    }

                }
                else if (customInfo != null && customInfo.Naval)
                {
                    // CU treats legs as vehicle sides. 
                    if (mech.IsLegged)
                    {
                        boundsMin = Mod.Config.Naval.CrippledModifierMin;
                        boundsMax = Mod.Config.Naval.CrippledModifierMax;
                    }
                }
                else if (customInfo.SquadInfo == null || customInfo.SquadInfo.Troopers < 1)
                {
                    // Cannot be crippled, do nothing
                    boundsMin = 0;
                    boundsMax = 0;
                }
                // Should be a mech, or a CU that is mech-like
                else if (mech.IsLegged)
                {
                    boundsMin = Mod.Config.Mech.CrippledModifierMin;
                    boundsMax = Mod.Config.Mech.CrippledModifierMax;
                }
            }

            if (boundsMax == 0) return 0;

            Pilot pilot = actor.GetPilot();
            int pilotMod = pilot.CurrentSkillMod(pilot.Piloting, ModStats.MOD_SKILL_PILOT);
            int adjustedMax = boundsMax - pilotMod;

            int finalMod = adjustedMax;
            if (adjustedMax <= boundsMin)
            {
                finalMod = boundsMin;
                Mod.Log.Debug?.Write($"  adjustedMax < boundsMin, returning {boundsMin}");
            }

            Mod.Log.Debug?.Write($"  finalMod: {finalMod}");
            return finalMod;
        }


        // Shutdown only applies to mechs, quads, and troopers
        public static int ShutdownInitModifier(this AbstractActor actor)
        {
            if (!(actor is Mech)) return 0;
            
            Mech mech = actor as Mech;
            if (!mech.IsShutDown) return 0;

            UnitCustomInfo customInfo = mech.GetCustomInfo();
            if (customInfo != null && (customInfo.Naval || customInfo.FakeVehicle)) return 0;

            int boundsMin, boundsMax;
            if (customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1)
            {
                boundsMin = Mod.Config.Trooper.ShutdownModifierMin;
                boundsMax = Mod.Config.Trooper.ShutdownModifierMax;
            }
            else
            {
                boundsMin = Mod.Config.Mech.ShutdownModifierMin;
                boundsMax = Mod.Config.Mech.ShutdownModifierMax;
            }
            Mod.Log.Debug?.Write($"Unit: {mech.DistinctId()} is shutdown, raw bounds => min: {boundsMin} to max: {boundsMax}");

            Pilot pilot = actor.GetPilot();
            int pilotMod = pilot.CurrentSkillMod(pilot.Piloting, ModStats.MOD_SKILL_PILOT);
            int adjustedMax = boundsMax - pilotMod;
            Mod.Log.Debug?.Write($"  adjustedMax: {adjustedMax} = max: {boundsMax} - pilotMod: {pilotMod}");

            int finalMod = adjustedMax;
            if (adjustedMax <= boundsMin)
            {
                finalMod = boundsMin;
                Mod.Log.Debug?.Write($"  adjustedMax < boundsMin, returning {boundsMin}");
            }

            Mod.Log.Debug?.Write($"  finalMod: {finalMod}");
            return finalMod;
        }
     
    }
}
