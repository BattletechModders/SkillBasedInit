using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
//using CustomComponents;
using HBS.Collections;
//using MechEngineer.Features.Engines;

namespace SkillBasedInit.Helper {
    public class UnitHelper {

        // Const values 
        private const float TurretTonnage = 100.0f;
        public const int DefaultTonnage = 100;
        private const int SuperHeavyTonnage = 11;

        private static readonly Dictionary<int, int> InitBaseByTonnage = new Dictionary<int, int> {
            {  0, 22 }, // 0-5
            {  1, 21 }, // 10-15
            {  2, 20 }, // 20-25
            {  3, 19 }, // 30-35
            {  4, 18 }, // 40-45
            {  5, 17 }, // 50-55
            {  6, 16 }, // 60-65
            {  7, 15 }, // 70-75
            {  8, 14 }, // 80-85
            {  9, 13 }, // 90-95
            { 10, 12 }, // 100
            { SuperHeavyTonnage, 9 }, // 105+
        };
        private static readonly Dictionary<int, int> EngineMidpointByTonnage = new Dictionary<int, int> {
            {  0, 10 }, // 0-5
            {  1, 9 }, // 10-15
            {  2, 8 }, // 20-25
            {  3, 7 }, // 30-35
            {  4, 6 }, // 40-45
            {  5, 5 }, // 50-55
            {  6, 4 }, // 60-65
            {  7, 4 }, // 70-75
            {  8, 3 }, // 80-85
            {  9, 3 }, // 90-95
            { 10, 3 }, // 100
            { SuperHeavyTonnage, 2 }, // 105+        
        };

        public static float GetUnitTonnage(AbstractActor actor) {
            float tonnage = DefaultTonnage;

            if (actor.GetType() == typeof(Mech)) {
                tonnage = ((Mech)actor).tonnage;
            } else if (actor.GetType() == typeof(Vehicle)) {
                tonnage = ((Vehicle)actor).tonnage;
            } else {
                tonnage = TurretTonnage;

                TagSet actorTags = actor.GetTags();
                if (actorTags != null && actorTags.Contains("unit_light")) {
                    tonnage = Mod.Config.TurretTonnageTagUnitLight;
                } else if (actorTags != null && actorTags.Contains("unit_medium")) {
                    tonnage = Mod.Config.TurretTonnageTagUnitMedium;
                } else if (actorTags != null && actorTags.Contains("unit_heavy")) {
                    tonnage = Mod.Config.TurretTonnageTagUnitHeavy;
                } else {
                    tonnage = Mod.Config.TurretTonnageTagUnitNone;
                }
            }
            return tonnage;
        }

        // Any modifier from the unit's tonnage
        public static int GetTonnageModifier(float tonnage) {
            int tonnageRange = GetTonnageRange(tonnage);
            return InitBaseByTonnage[tonnageRange];
        }

        private static int GetTonnageRange(float tonnage) {
            int tonnageRange = (int)Math.Floor(tonnage / 10.0);
            if (tonnageRange > 10) {
                tonnageRange = SuperHeavyTonnage;
            }
            Mod.Log.Debug($"for raw tonnage {tonnage} returning tonnageRange:{tonnageRange}");
            return tonnageRange;
        }

        /*
         * Take the BaseInitiative value from all components on the unit, remove the 
         *  normal phase modifiers HBS applies from SimGameConstants (-2 for light, etc),
         *  then invert the value. Because HBS defines bonuses as negative modifiers,
         *  invert this value to have it make sense elsewhere in the code.        
         */
         // TODO: Should this use the MechDef call below? 
        public static int GetNormalizedComponentModifier(AbstractActor actor) {
            int unitInit = 0;

            WeightClass weightClass;
            if (actor.GetType() == typeof(Mech)) {
                weightClass = ((Mech)actor).weightClass;
            } else if (actor.GetType() == typeof(Vehicle)) {
                var vehicle = (Vehicle)actor;
                weightClass = ((Vehicle)actor).weightClass;
            } else { // turret
                TagSet actorTags = actor.GetTags();
                if (actorTags != null && actorTags.Contains("unit_light")) {
                    weightClass = WeightClass.LIGHT;
                } else if (actorTags != null && actorTags.Contains("unit_medium")) {
                    weightClass = WeightClass.MEDIUM;
                } else if (actorTags != null && actorTags.Contains("unit_heavy")) {
                    weightClass = WeightClass.HEAVY;
                } else {
                    weightClass = WeightClass.ASSAULT;
                }
            }

            // TODO: Validate that vehicles are normalized properly - looks like HBS adjusts the phases, may not be working properly
            // TODO: Validate that turret are normalized properly - looks like HBS adjusts the phases, may not be working properly
            /*
                HBS VALUES           
                "PhaseSpecial": 1,
                "PhaseLight": 2,
                "PhaseMedium": 3,
                "PhaseHeavy": 4,
                "PhaseAssault": 5,
                "PhaseLightVehicle": 3,
                "PhaseMediumVehicle": 4,
                "PhaseHeavyVehicle": 5,
                "PhaseAssaultVehicle": 5,
                "PhaseLightTurret": 5,
                "PhaseMediumTurret": 5,
                "PhaseHeavyTurret": 5,
                "PhaseAssaultTurret": 5

                RT VALUE
                "PhaseSpecial": 1,
                "PhaseLight": 2,
                "PhaseMedium": 3,
                "PhaseHeavy": 4,
                "PhaseAssault": 5,
                "PhaseLightVehicle": 2,
                "PhaseMediumVehicle": 3,
                "PhaseHeavyVehicle": 4,
                "PhaseAssaultVehicle": 5,
                "PhaseLightTurret": 3,
                "PhaseMediumTurret": 4,
                "PhaseHeavyTurret": 5,
                "PhaseAssaultTurret": 5
           */

            if (actor.StatCollection != null && actor.StatCollection.ContainsStatistic("BaseInitiative")) {
                int baseMod = actor.StatCollection.GetValue<int>("BaseInitiative");

                // Normalize the value
                // TODO: These should come from CombatGameDef.PhaseConstantsDef, but I can't find a reference to pull 
                //   those values. May have to load at mod start and cache them.
                switch (weightClass) {
                    case WeightClass.LIGHT:
                        baseMod -= 2;
                        break;
                    case WeightClass.MEDIUM:
                        baseMod -= 3;
                        break;
                    case WeightClass.HEAVY:
                        baseMod -= 4;
                        break;
                    case WeightClass.ASSAULT:
                        baseMod -= 5;
                        break;
                    default:
                        Mod.Log.Debug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name}" +
                            $" has unknown or undefined weight class:{weightClass}!");
                        break;
                }

                // Because HBS init values were from 2-5, bonuses will be negative at this point and penalties positive. Invert these.
                unitInit = baseMod * -1;
                Mod.Log.Debug($"Normalized BaseInit for Actor:{actor.DisplayName}_{actor.GetPilot().Name}" +
                    $" from {actor.StatCollection.GetValue<int>("BaseInitiative")} to unitInit:{unitInit}");
            }

            return unitInit;
        }

        // Calculate the initiative modifiers from all components based upon a MechDef. For whatever reason they 
        //  reverse the modifier right out of the gate, such that these values are positives automatically
        public static int GetNormalizedComponentModifier(MechDef mechDef) {
            int unitInit = 0;
            if (mechDef.Inventory != null) {
                MechComponentRef[] inventory = mechDef.Inventory;
                foreach (MechComponentRef mechComponentRef in inventory) {
                    if (mechComponentRef.Def != null && mechComponentRef.Def.statusEffects != null) {
                        EffectData[] statusEffects = mechComponentRef.Def.statusEffects;
                        foreach (EffectData effect in statusEffects) {
                            if (MechStatisticsRules.GetInitiativeModifierFromEffectData(effect, true, null) == 0) {
                                unitInit += MechStatisticsRules.GetInitiativeModifierFromEffectData(effect, false, null);
                            }
                        }
                    }
                }
            }

            Mod.Log.Debug($"Normalized BaseInit for mechDef:{mechDef.Name} is unitInit:{unitInit}");
            return unitInit;
        }

        // TODO: Re implement?
        //public static int GetEngineModifier(AbstractActor actor) {
        //    int engineMod = 0;
        //    if (actor.GetTags().Contains("unit_powerarmor")) {
        //        Mod.Log.Debug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} is PowerArmor, skipping engine bonus.");
        //    } else {
        //        MechComponent mainEngineComponent = actor?.allComponents?.FirstOrDefault(c => c?.componentDef?.GetComponent<EngineCoreDef>() != null);
        //        if (mainEngineComponent != null) {
        //            EngineCoreDef engine = mainEngineComponent?.componentDef?.GetComponent<EngineCoreDef>();
        //            float tonnage = GetUnitTonnage(actor);
        //            engineMod = CalculateEngineModifier(tonnage, engine.Rating);
        //            Mod.Log.Debug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} with engine rating: {engine?.Rating} has engineMod:{engineMod}");
        //        } else {
        //            Mod.Log.Info($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has no engine - is this expected?");
        //        }
        //    }

        //    return engineMod;
        //}

        //public static int GetEngineModifier(MechDef mechDef) {
        //    int engineMod = 0;

        //    // var mainEngineComponent = actor?.allComponents?.FirstOrDefault(c => c?.componentDef?.GetComponent<EngineCoreDef>() != null);
        //    MechComponentRef engineRef = mechDef.Inventory.FirstOrDefault(mcr => mcr?.GetComponent<EngineCoreDef>() != null);
        //    Mod.Log.Debug($"MechDef:{mechDef.Name} has engineComponent:{engineRef}?");
        //    if (engineRef != null) {
        //        EngineCoreDef engine = engineRef.Def.GetComponent<EngineCoreDef>();
        //        float tonnage = mechDef.Chassis.Tonnage;
        //        engineMod = CalculateEngineModifier(tonnage, engine.Rating);
        //    }

        //    return engineMod;
        //}

        private static int CalculateEngineModifier(float tonnage, int rating) {
            int engineMod = 0;

            float engineRatio = rating / tonnage;
            int tonnageRange = GetTonnageRange(tonnage);
            int ratioMidpoint = EngineMidpointByTonnage[tonnageRange];
            Mod.Log.Debug($"Comparing engineRatio:{engineRatio} from rating:{rating} / tonnage:{tonnage} vs midpoint:{ratioMidpoint}");
            if (engineRatio > ratioMidpoint) {
                int oneSigma = (int)Math.Ceiling(ratioMidpoint * 1.2);
                int twoSigma = (int)Math.Ceiling(ratioMidpoint * 1.4);
                int threeSigma = (int)Math.Ceiling(ratioMidpoint * 1.7);
                if (engineRatio < oneSigma) {
                    engineMod = 0;
                } else if (engineRatio < twoSigma) {
                    engineMod = 2;
                } else if (engineRatio < threeSigma) {
                    engineMod = 4;
                } else {
                    engineMod = 6;
                }
                Mod.Log.Debug($"Oversized engine, returning bonus engineMod:{engineMod}");
            } else if (engineRatio < ratioMidpoint) {
                int oneSigma = (int)Math.Floor(ratioMidpoint * 0.8);
                int twoSigma = (int)Math.Floor(ratioMidpoint * 0.6);
                int threeSigma = (int)Math.Floor(ratioMidpoint * 0.3);
                if (engineRatio > oneSigma) {
                    engineMod = 0;
                } else if (engineRatio > twoSigma) {
                    engineMod = -2;
                } else if (engineRatio > threeSigma) {
                    engineMod = -4;
                } else {
                    engineMod = -6;
                }
                Mod.Log.Debug($"Undersized engine, returning penalty engineMod:{engineMod}");
            } else {
                Mod.Log.Debug("Balanced engine, returning engineMod:0");
                engineMod = 0;
            }

            return engineMod;
        }

        public static int GetTypeModifier(AbstractActor actor) {
            int typeMod;
            if (actor.GetType() == typeof(Mech)) {
                typeMod = 0;
            } else if (actor.GetType() == typeof(Vehicle)) {
                typeMod = Mod.Config.VehicleROCModifier;
            } else {
                typeMod = Mod.Config.TurretROCModifier;
            }
            return typeMod;
        }
    }
}
