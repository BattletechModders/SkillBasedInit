﻿using CustAmmoCategories;
using CustomUnits;
using HBS.Collections;
using IRBTModUtils.Extension;
using System;
using System.Collections.Generic;

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
                if (customInfo == null) return false;

                return customInfo.FakeVehicle;
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

        public static int GetBaseInitByTonnage(this AbstractActor actor)
        {
            int unitTonnage = (int)Math.Ceiling(actor.GetUnitTonnage());
            UnitCfg opts = actor.GetUnitConfig();
            int initPhase = 0;
            foreach (KeyValuePair<int, int> kvp in opts.InitBaseByTonnage)
            {
                Mod.Log.Debug?.Write($"  -- key: {kvp.Key}  value: {kvp.Value}");
                if (unitTonnage <= kvp.Key)
                {
                    initPhase = kvp.Value;
                    break;
                }
            }
            // Now invert to an initiative modifier
            int baseInit = InitiativeHelper.PhaseToInitiative(initPhase);
            Mod.Log.Debug?.Write($"Using initPhase: {initPhase} => baseInit: {baseInit} for actor: {actor.DistinctId()} with tonnage: {unitTonnage}");

            return baseInit;
        }

        public static int GetBaseInitByTonnage(this MechDef mechDef)
        {
            int unitTonnage = (int)Math.Ceiling(mechDef.GetUnitTonnage());
            UnitCfg opts = mechDef.GetUnitConfig();
            int initPhase = 0;
            foreach (KeyValuePair<int, int> kvp in opts.InitBaseByTonnage)
            {
                if (unitTonnage >= kvp.Key)
                {
                    initPhase = kvp.Value;
                }
                else
                {
                    break;
                }
            }

            // Now invert to an initiative modifier
            int baseInit = InitiativeHelper.PhaseToInitiative(initPhase);
            Mod.Log.Debug?.Write($"Using initPhase: {initPhase} => baseInit: {baseInit} for mechDef: {mechDef.Name} with tonnage: {unitTonnage}");

            return baseInit;
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

            // Typemod is a phase modifier in config, so invert for init
            return typeMod * -1;
        }

        public static int GetTypeModifier(this MechDef mechDef)
        {
            if (mechDef == null || mechDef.Chassis == null) return 0;
            Mod.Log.Debug?.Write($"Calculating type modifier for mechDef: {mechDef.Name}");

            int typeMod = Mod.Config.Mech.TypeMod;

            UnitCustomInfo customInfo = mechDef.GetCustomInfo();
            if (customInfo == null)
            {
                Mod.Log.Debug?.Write("Mechdef has no customInfo, using Mech config.");
            }
            else
            {
                if (customInfo.FakeVehicle)
                {
                    Mod.Log.Debug?.Write("Mechdef GetTypeMod returning vehicle typeMod.");
                    typeMod = Mod.Config.Vehicle.TypeMod;
                }
                else if (customInfo.Naval)
                {
                    Mod.Log.Debug?.Write("Mechdef GetTypeMod returning naval typeMod.");
                    typeMod = Mod.Config.Naval.TypeMod;
                }
                else if (customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1)
                {
                    Mod.Log.Debug?.Write("Mechdef GetTypeMod returning trooper typeMod.");
                    typeMod = Mod.Config.Trooper.TypeMod;
                }
                else
                {
                    Mod.Log.Debug?.Write("Mechdef GetTypeMod fallthrough, returning mech typeMod.");
                }
            }

            // Typemod is a phase modifier in config, so invert for init
            return typeMod * -1;
        }

    }

    public static class UnitHelper
    {

        // Prone only applies to mechs and quads
        public static int ProneInitMod(this AbstractActor actor)
        {
            if (!(actor is Mech)) return 0;

            Mech mech = actor as Mech;
            if (!mech.IsProne) return 0;

            UnitCustomInfo customInfo = mech.GetCustomInfo();
            if (customInfo != null && (customInfo.Naval || customInfo.FakeVehicle)) return 0;
            if (customInfo != null && customInfo.SquadInfo != null && customInfo.SquadInfo.Troopers > 1) return 0;

            // Config is a phase mod, so invert
            int boundsMin = Mod.Config.Mech.ProneModifierMin * -1;
            int boundsMax = Mod.Config.Mech.ProneModifierMax * -1;
            Mod.Log.Debug?.Write($"Unit: {mech.DistinctId()} is prone, raw bounds => min: {boundsMin} to max: {boundsMax}");

            Pilot pilot = actor.GetPilot();
            int pilotMod = pilot.SBIPilotingMod();
            int adjustedMin = boundsMin - pilotMod;
            if (adjustedMin < 0)
                adjustedMin = 0;

            int adjustedMax = boundsMax - pilotMod;
            if (adjustedMax <= adjustedMin)
                adjustedMax = adjustedMin + 1;

            // Add +1 to max BECUASE MICROSOFT SUCKS (see https://docs.microsoft.com/en-us/dotnet/api/system.random.next?view=net-6.0#system-random-next(system-int32-system-int32)
            int finalMod = Mod.Random.Next(adjustedMin, adjustedMax + 1);
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

            // Config is a phase modifier, so invert
            int invertedMin = unitCfg.CrippledModifierMin * -1;
            int invertedMax = unitCfg.CrippledModifierMax * -1;

            Mod.Log.Debug?.Write($"Unit: {actor.DistinctId()} is crippled, raw bounds => min: {invertedMin} " +
                $"to max: {invertedMax}");

            Pilot pilot = actor.GetPilot();
            int pilotMod = pilot.SBIPilotingMod();

            int adjustedMin = invertedMin - pilotMod;
            if (adjustedMin < 0)
                adjustedMin = 0;
            int adjustedMax = invertedMax - pilotMod;
            if (adjustedMax <= adjustedMin)
                adjustedMax = adjustedMin + 1;

            // Add +1 to max BECUASE MICROSOFT SUCKS (see https://docs.microsoft.com/en-us/dotnet/api/system.random.next?view=net-6.0#system-random-next(system-int32-system-int32)
            int finalMod = Mod.Random.Next(adjustedMin, adjustedMax + 1);
            Mod.Log.Debug?.Write($"  finalMod: {finalMod}");
            return finalMod;
        }


        // Shutdown only applies to mechs, quads, and troopers
        public static int ShutdownInitMod(this AbstractActor actor)
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

            // BoundsMax are phase modifiers, so invert
            boundsMin *= -1;
            boundsMax *= -1;
            Mod.Log.Debug?.Write($"Unit: {mech.DistinctId()} is shutdown, raw bounds => min: {boundsMin} to max: {boundsMax}");

            Pilot pilot = actor.GetPilot();
            int pilotMod = pilot.SBIPilotingMod();

            int adjustedMin = boundsMin - pilotMod;
            if (adjustedMin < 0)
                adjustedMin = 0;

            int adjustedMax = boundsMax - pilotMod;
            if (adjustedMax <= adjustedMin)
                adjustedMax = adjustedMin + 1;
            Mod.Log.Debug?.Write($"  adjustedMax: {adjustedMax} = max: {boundsMax} - pilotMod: {pilotMod}");

            // Add +1 to max BECUASE MICROSOFT SUCKS (see https://docs.microsoft.com/en-us/dotnet/api/system.random.next?view=net-6.0#system-random-next(system-int32-system-int32)
            int finalMod = Mod.Random.Next(adjustedMin, adjustedMax + 1);
            Mod.Log.Debug?.Write($"  finalMod: {finalMod} = Random({adjustedMin}, {adjustedMax})");
            return finalMod;
        }

        public static int GetHesitationPenalty(this AbstractActor actor)
        {
            if (actor == null) return 0;

            // Hesitation is a phase modifier, so invert
            UnitCfg unitCfg = actor.GetUnitConfig();
            int invertedMax = unitCfg.HesitationMax * -1;
            int invertedMin = unitCfg.HesitationMin * -1;
            Mod.Log.Debug?.Write($"Unit: {actor.DistinctId()} hestiation raw bounds => min: {invertedMin} to max: {invertedMax}");

            // Invert because we assume the range is negative
            // Add +1 to max BECUASE MICROSOFT SUCKS (see https://docs.microsoft.com/en-us/dotnet/api/system.random.next?view=net-6.0#system-random-next(system-int32-system-int32)
            int rawMod = Mod.Random.Next(invertedMin, invertedMax + 1);

            // Assume the stat is a phase mod, and invert
            int actorMod = actor.StatCollection.GetValue<int>(ModStats.MOD_HESITATION) * -1;

            int finalMod = rawMod + actorMod;
            Mod.Log.Debug?.Write($"Hesitation penalty: {finalMod} = rawMod: {rawMod} + actorMod: {actorMod}");
            if (finalMod < 0)
            {
                Mod.Log.Debug?.Write("Normalizing penalty < 0 to 0");
                finalMod = 0;
            }
            return finalMod;
        }

        public static int CalledShotInitMod(this AbstractActor target, AbstractActor attacker)
        {

            // Get attacker bonus, get target bonus, combine
            if (target == null || attacker == null) return 0;

            // Config is phase modifiers, so invert
            UnitCfg unitCfg = target.GetUnitConfig();
            int invertedMin = unitCfg.CalledShotRandMin * -1;
            int invertedMax = unitCfg.CalledShotRandMax * -1;
            Mod.Log.Debug?.Write($"Unit: {target.DistinctId()} raw called shot raw bounds => min: {invertedMin} to max: {invertedMax}");

            // Assume the stat is a phase modifier, so invert
            int targetCSMod = target.StatCollection.GetValue<int>(ModStats.MOD_CALLED_SHOT_TARGET) * -1;
            int targetSkillMod = target.GetPilot().AverageGutsAndTacticsMod();
            int targetMod = targetCSMod + targetSkillMod;
            Mod.Log.Debug?.Write($"Target calledShotMod: {targetMod} = calledShotMod: {targetCSMod} + skillMod: {targetSkillMod}");

            // Assume the stat is a phase modifier, so invert
            int attackerCSMod = attacker.StatCollection.GetValue<int>(ModStats.MOD_CALLED_SHOT_ATTACKER) * -1;
            int attackerSkillMod = attacker.GetPilot().AverageGunneryAndTacticsMod();
            int attackerMod = attackerCSMod + attackerSkillMod;
            Mod.Log.Debug?.Write($"Attacker calledShotMod: {attackerMod} = calledShotMod: {attackerCSMod} + skillMod: {attackerSkillMod}");

            int actorDelta = attackerMod - targetMod;
            if (actorDelta < 0)
                actorDelta = 0;
            Mod.Log.Debug?.Write($"actor delta: {actorDelta} = attackerMod: {attackerMod} - targetMod: {targetMod}");

            int adjustedMin = invertedMin + actorDelta;
            int adjustedMax = invertedMax + actorDelta;
            if (adjustedMax <= adjustedMin)
                adjustedMax = adjustedMin + 1;

            // Add +1 to max BECUASE MICROSOFT SUCKS (see https://docs.microsoft.com/en-us/dotnet/api/system.random.next?view=net-6.0#system-random-next(system-int32-system-int32)
            int finalMod = Mod.Random.Next(adjustedMin, adjustedMax + 1);
            Mod.Log.Debug?.Write($"Called shot penalty: {finalMod} from Rand({adjustedMin}, {adjustedMax}) ");

            if (finalMod < 0)
            {
                Mod.Log.Debug?.Write("Normalizing penalty < 0 to 0");
                finalMod = 0;
            }

            return finalMod;
        }


        public static int VigilanceInitMod(this AbstractActor actor)
        {
            if (actor == null || actor.GetPilot() == null) return 0;

            // Config is a phase modifier, so invert. These will be negatives presumably
            UnitCfg unitCfg = actor.GetUnitConfig();
            int invertedMin = unitCfg.VigilanceRandMin * -1;
            int invertedMax = unitCfg.VigilanceRandMax * -1;

            //  Assume this is a phase modifier, and invert
            int actorMod = actor.StatCollection.GetValue<int>(ModStats.MOD_VIGILANCE) * -1;

            // Invert the value so it reduces the outcome
            int skillMod = actor.GetPilot().AverageGutsAndTacticsMod() * -1;

            int adjustedMin = invertedMin + actorMod + skillMod;
            Mod.Log.Debug?.Write($"  adjustedMin: {adjustedMin} = invertedMin: {invertedMin} + actorMod: {actorMod} + skillMod: {skillMod})");
            int adjustedMax = invertedMax + actorMod + skillMod;
            Mod.Log.Debug?.Write($"  adjustedMax: {adjustedMax} = invertedMin: {invertedMax} + actorMod: {actorMod} + skillMod: {skillMod})");

            // Add +1 to max BECUASE MICROSOFT SUCKS (see https://docs.microsoft.com/en-us/dotnet/api/system.random.next?view=net-6.0#system-random-next(system-int32-system-int32)
            int finalMod = Mod.Random.Next(adjustedMax, adjustedMin + 1);
            Mod.Log.Debug?.Write($"Vigilance bonus: {finalMod} = min:{adjustedMin} to max: {adjustedMax}");
            if (finalMod > 0)
            {
                Mod.Log.Debug?.Write("Normalizing bonus > 0 to 0");
                finalMod = 0;
            }
            return finalMod;
        }

    }
}
