using System;
using System.Collections.Generic;
using BattleTech;
using CustAmmoCategories;
using CustomUnits;
using HBS.Collections;
using IRBTModUtils.Extension;

namespace SkillBasedInit.Helper
{
    public static class UnitExtensions
    {
        public static UnitCfg GetUnitConfig(this AbstractActor actor)
        {
            if (actor is Turret) return Mod.Config.Turret;
            else if (actor is Vehicle) return Mod.Config.Vehicle;
            else if (actor is Mech mech)
            {
                if (mech.FakeVehicle()) return Mod.Config.Vehicle;
                else if (mech.NavalUnit()) return Mod.Config.Naval;
                else if (mech.TrooperSquad()) return Mod.Config.Trooper;                
                else return Mod.Config.Mech;
            }
            else return Mod.Config.Mech;
        }

        public static UnitCfg GetUnitConfig(this MechDef mechDef)
        {
            if (mechDef == null) return null;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return Mod.Config.Mech;

            // Technically quads
            if (customInfo.ArmsCountedAsLegs) return Mod.Config.Mech;
            else if (customInfo.FakeVehicle) return Mod.Config.Vehicle;
            else if (customInfo.Naval) return Mod.Config.Naval;
            else if (customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1) return Mod.Config.Trooper;

            // NOTE: Turrets currently not included in this. I don't think they can drop currently
            else return Mod.Config.Mech;
        }

        public static bool IsQuadMech(this ICombatant combatant)
        {
            if (combatant is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                return customInfo != null && customInfo.ArmsCountedAsLegs;
            }
            return false;
        }


        public static bool IsQuadMech(this MechDef mechDef)
        {
            if (mechDef == null) return false;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return false;

            return customInfo.ArmsCountedAsLegs;
        }

        public static bool IsTrooper(this ICombatant combatant)
        {
            if (combatant is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                return customInfo != null && customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1;
            }
            return false;

        }
        public static bool IsTrooper(this MechDef mechDef)
        {
            if (mechDef == null) return false;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return false;

            return customInfo?.SquadInfo?.Troopers > 1;
        }

        public static bool IsNaval(this ICombatant combatant)
        {
            if (combatant is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                return customInfo != null && customInfo.Naval;
            }
            return false;

        }

        public static bool IsNaval(this MechDef mechDef)
        {
            if (mechDef == null) return false;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return false;

            return customInfo.Naval;
        }

        public static bool IsVehicle(this ICombatant combatant)
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

        public static bool IsVehicle(this MechDef mechDef)
        {
            if (mechDef == null) return false;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null) return false;

            return customInfo.FakeVehicle;
        }

        public static float GetUnitTonnage(this AbstractActor actor)
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
                if (mech.IsVehicle())
                {
                    tonnage = mech.tonnage;
                    Mod.Log.Debug?.Write($" -- unit is a fake vehicle, using tonnage: {tonnage}");
                }
                else if (mech.IsNaval())
                {
                    tonnage = mech.tonnage;
                    Mod.Log.Debug?.Write($" -- unit is a naval unit, using tonnage: {tonnage}");
                }
                else if (mech.IsTrooper())
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
                UnitCfg unitConfig = actor.GetUnitConfig();
                tonnage = unitConfig.DefaultTonnage;
                Mod.Log.Debug?.Write($" -- unit tonnage is unknown, using tonnage: {tonnage}");
            }

            return tonnage;
        }

        public static float GetUnitTonnage(this MechDef mechDef)
        {

            if (mechDef == null || mechDef.Chassis == null) return 0;

            Mod.Log.Debug?.Write($"Calculating unit tonnage for mechDef: {mechDef.Name}");
            float tonnage;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            float chassisTonnage = mechDef.Chassis.Tonnage;
            if (customInfo == null) return chassisTonnage;

            if (customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1)
            {
                tonnage = (float)Math.Ceiling(chassisTonnage / customInfo.SquadInfo.Troopers);
            }
            else
            {
                tonnage = chassisTonnage;
            }

            return tonnage;
        }

        public static int GetTonnageModifier(this AbstractActor actor)
        {
            int unitTonnage = (int)Math.Ceiling(actor.GetUnitTonnage());
            UnitCfg opts = actor.GetUnitConfig();
            int initBase = 0;
            foreach (KeyValuePair<int, int> kvp in opts.InitBaseByTonnage)
            {
                Mod.Log.Debug?.Write($"  -- key: {kvp.Key}  value: {kvp.Value}");
                if (unitTonnage <= kvp.Key)
                {
                    initBase = kvp.Value;
                    break;
                }
            }

            Mod.Log.Debug?.Write($"Using tonnageMod: {initBase} for actor: {actor.DistinctId()} with tonnage: {unitTonnage}");
            return initBase;
        }

        public static int GetTonnageModifier(this MechDef mechDef)
        {
            int unitTonnage = (int)Math.Ceiling(mechDef.GetUnitTonnage());
            UnitCfg opts = mechDef.GetUnitConfig();
            int initBase = 0;
            foreach (KeyValuePair<int, int> kvp in opts.InitBaseByTonnage)
            {
                if (unitTonnage >= kvp.Key)
                {
                    initBase = kvp.Value;
                }
                else
                {
                    break;
                }
            }

            Mod.Log.Debug?.Write($"Using tonnageMod: {initBase} for mechDef: {mechDef.Name} with tonnage: {unitTonnage}");
            return initBase;

        }


        public static int GetTypeModifier(this AbstractActor actor)
        {
            int typeMod;
            if (actor is Turret)
            {
                typeMod = Mod.Config.Turret.TypeMod;
                Mod.Log.Debug?.Write($"  -- unit is type turret, using typeMod: {typeMod}");
            }
            else if (actor is Vehicle)
            {
                typeMod = Mod.Config.Vehicle.TypeMod;
                Mod.Log.Debug?.Write($"  -- unit is type vehicle, using typeMod: {typeMod}");
            }
            else if (actor is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                if (customInfo != null)
                {
                    if (customInfo.FakeVehicle)
                    {
                        typeMod = Mod.Config.Vehicle.TypeMod;
                        Mod.Log.Debug?.Write($"  -- unit is type vehicle, using typeMod: {typeMod}");
                    }
                    else if (customInfo.Naval)
                    {
                        typeMod = Mod.Config.Naval.TypeMod;
                        Mod.Log.Debug?.Write($"  -- unit is type naval, using typeMod: {typeMod}");
                    }
                    else if (customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1)
                    {
                        typeMod = Mod.Config.Trooper.TypeMod;
                        Mod.Log.Debug?.Write($"  -- unit is type troopers, using typeMod: {typeMod}");
                    }
                    else
                    {
                        typeMod = Mod.Config.Mech.TypeMod;
                        Mod.Log.Debug?.Write($"  -- unit is type mech, using typeMod: {typeMod}");
                    }
                }
                else
                {
                    typeMod = Mod.Config.Mech.TypeMod;
                    Mod.Log.Debug?.Write($"  -- unit is type mech, using typeMod: {typeMod}");
                }
            }
            else
            {
                typeMod = 0;
                Mod.Log.Debug?.Write("  -- unit is unknown type, using 0 typeMod.");
            }

            return typeMod;
        }

        public static int GetTypeModifier(this MechDef mechDef)
        {
            if (mechDef == null || mechDef.Chassis == null) return 0;
            Mod.Log.Debug?.Write($"Calculating type modifier for mechDef: {mechDef.Name}");

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null)
            {
                Mod.Log.Debug?.Write("Mechdef has no customInfo, using Mech config.");
                return Mod.Config.Mech.TypeMod;
            }

            if (customInfo.FakeVehicle)
            {
                Mod.Log.Debug?.Write("Mechdef GetTypeMod returning vehicle typeMod.");
                return Mod.Config.Vehicle.TypeMod;
            }
            else if (customInfo.Naval)
            {
                Mod.Log.Debug?.Write("Mechdef GetTypeMod returning naval typeMod.");
                return Mod.Config.Naval.TypeMod;
            }
            else if (customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1)
            {
                Mod.Log.Debug?.Write("Mechdef GetTypeMod returning trooper typeMod.");
                return Mod.Config.Trooper.TypeMod;
            }

            Mod.Log.Debug?.Write("Mechdef GetTypeMod fallthrough, returning mech typeMod.");
            return Mod.Config.Mech.TypeMod;
        }
    }

    public static class UnitHelper
    {
        
        // Prone only applies to mechs and quads
        public static int ProneInitModifier(this AbstractActor actor, bool isKnockdown=false)
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
            int pilotMod = pilot.SBIPilotingMod();
            int adjustedMax = boundsMax + pilotMod;
            if (adjustedMax >= boundsMin)
            {
                adjustedMax = boundsMin - 1;
            }

            // Invert the modifier, because Init is inverted. actor.Initiative = (Mod.MaxPhase + 1) - phaseModifier
            int finalMod = -1 * Mod.Random.Next(adjustedMax, boundsMin);

            Mod.Log.Debug?.Write($"  finalMod: {finalMod}");
            return finalMod;
        }

        // Crippled applies to mechs, quads, vehicles, naval
        public static int CrippledInitModifier(this AbstractActor actor)
        {
            if (actor is null) return 0;

            // Turrets cannot be crippled
            if (actor is Turret) return 0;

            UnitCustomInfo customInfo = actor.GetCustomInfo();
            
            // Troopers cannot be crippled
            if (customInfo != null && customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1) return 0;

            
            bool isCrippled = false;
            if (actor is Vehicle vehicle)
            {
                Mod.Log.Debug?.Write($"Checking true vehicle for crippled");
                if (vehicle.IsLocationDestroyed(VehicleChassisLocations.Left) || vehicle.IsLocationDestroyed(VehicleChassisLocations.Right))
                    isCrippled = true;
            }            
            else if (actor is Mech mech)
            {
                // CU treats legs as vehicle sides, so this works for our purposes
                // TODO: Quads may want to defer until mutiples are legged, but for now one leg - crippled
                isCrippled = mech.IsLegged;
            }

            if (!isCrippled) return 0; // Nothing to do

            UnitCfg unitCfg = actor.GetUnitConfig();
            if (unitCfg.CrippledModifierMin == 0) return 0; // Nothing to do

            Mod.Log.Debug?.Write($"Unit: {actor.DistinctId()} is crippled, raw bounds => min: {unitCfg.CrippledModifierMin} " +
                $"to max: {unitCfg.CrippledModifierMax}");

            Pilot pilot = actor.GetPilot();
            int pilotMod = pilot.SBIPilotingMod();
            int adjustedMax = unitCfg.CrippledModifierMax + pilotMod;

            int finalMod = adjustedMax;
            if (adjustedMax >= unitCfg.CrippledModifierMin)
            {
                finalMod = unitCfg.CrippledModifierMin;
                Mod.Log.Debug?.Write($"  adjustedMax >= boundsMin, setting to {unitCfg.CrippledModifierMin}");
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
            int pilotMod = pilot.SBIPilotingMod();
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

        public static int GetHesitationPenalty(this AbstractActor actor)
        {
            if (actor == null) return 0;

            UnitCfg unitCfg = actor.GetUnitConfig();
            Mod.Log.Debug?.Write($"Unit: {actor.DistinctId()} hestiation raw bounds => min: {unitCfg.HesitationMin} to max: {unitCfg.HesitationMax}");
            
            // Invert because we assume the range is negative
            int rawMod = Mod.Random.Next(unitCfg.HesitationMax, unitCfg.HesitationMin);
            int actorMod = actor.StatCollection.GetValue<int>(ModStats.MOD_HESITATION);
            int finalMod = rawMod + actorMod;
            Mod.Log.Debug?.Write($"Hesitation penalty: {finalMod} = rawMod: {rawMod} + actorMod: {actorMod}");
            if (finalMod > 0)
            {
                Mod.Log.Debug?.Write("Normalizing penalty > 0 to 0");
                finalMod = 0;
            }
            return finalMod;
        }

        public static int CalledShotPenalty(this AbstractActor target, AbstractActor attacker)
        {
            // Get attacker bonus, get target bonus, combine
            if (target == null || attacker == null) return 0;

            UnitCfg unitCfg = target.GetUnitConfig();
            Mod.Log.Debug?.Write($"Unit: {target.DistinctId()} called shot raw bounds => min: {unitCfg.CalledShotRandMin} to max: {unitCfg.CalledShotRandMax}");

            // Invert because we assume the range is negative
            int targetCSMod = target.StatCollection.GetValue<int>(ModStats.MOD_CALLED_SHOT_TARGET);
            int targetSkillMod = target.GetPilot().AverageGutsAndTactics();
            int targetMod = targetCSMod + targetSkillMod;
            Mod.Log.Debug?.Write($"Target calledShotMod: {targetMod} = calledShotMod: {targetCSMod} + skillMod: {targetSkillMod}");

            int attackerCSMod = attacker.StatCollection.GetValue<int>(ModStats.MOD_CALLED_SHOT_ATTACKER);
            int attackerSkillMod = attacker.GetPilot().AverageGunneryAndTactics();
            int attackerMod = attackerCSMod + attackerSkillMod;
            Mod.Log.Debug?.Write($"Attacker calledShotMod: {attackerMod} = calledShotMod: {attackerCSMod} + skillMod: {attackerSkillMod}");

            int adjustedMax = unitCfg.CalledShotRandMax + targetMod + attackerMod;
            if (adjustedMax >= unitCfg.CalledShotRandMin)
                adjustedMax = unitCfg.CalledShotRandMin - 1;

            int finalMod = Mod.Random.Next(adjustedMax, unitCfg.CalledShotRandMin);
            Mod.Log.Debug?.Write($"Called shot penalty: {finalMod} from Rand({adjustedMax} to {unitCfg.CalledShotRandMin}) ");
            if (finalMod > 0)
            {
                Mod.Log.Debug?.Write("Normalizing penalty > 0 to 0");
                finalMod = 0;
            }
            return finalMod;
        }


        public static int VigilanceBonus(this AbstractActor actor)
        {
            if (actor == null || actor.GetPilot() == null) return 0;

            UnitCfg unitCfg = actor.GetUnitConfig();
            int actorMod = actor.StatCollection.GetValue<int>(ModStats.MOD_VIGILANCE);
            int skillMod = actor.GetPilot().AverageGutsAndTactics();
            int adjustedMax = actorMod + skillMod + unitCfg.VigilanceRandMax;

            int finalMod = Mod.Random.Next(unitCfg.VigilanceRandMin, adjustedMax);
            Mod.Log.Debug?.Write($"Vigilance bonus: {finalMod} = min:{unitCfg.VigilanceRandMin} to max: {unitCfg.VigilanceRandMax} + actorMod: {actorMod} + skillMod: {skillMod}");
            if (finalMod < 0)
            {
                Mod.Log.Debug?.Write("Normalizing bonus < 0 to 0");
                finalMod = 0;
            }
            return finalMod;
        }

    }
}
