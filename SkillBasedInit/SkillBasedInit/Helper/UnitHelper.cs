using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using CustomComponents;
using HBS.Collections;
using MechEngineer;

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

        public UnitHelper() {
            // Normalized BaseInitiative stats - should this be broken down by component in the tooltip?
            // Base init from tonnage
            // Engine mod
        }

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
                    tonnage = SkillBasedInit.Settings.TurretTonnageTagUnitLight;
                } else if (actorTags != null && actorTags.Contains("unit_medium")) {
                    tonnage = SkillBasedInit.Settings.TurretTonnageTagUnitMedium;
                } else if (actorTags != null && actorTags.Contains("unit_heavy")) {
                    tonnage = SkillBasedInit.Settings.TurretTonnageTagUnitHeavy;
                } else {
                    tonnage = SkillBasedInit.Settings.TurretTonnageTagUnitNone;
                }
            }
            return tonnage;
        }

        // Any modifier from the unit's tonnage
        public static int GetTonnageModifier(float tonnage) {
            int tonnageMod = 0;

            int tonnageRange = GetTonnageRange(tonnage);
            if (tonnageRange > 10) {
                tonnageMod = InitBaseByTonnage[SuperHeavyTonnage];
            } else {
                tonnageMod = InitBaseByTonnage[tonnageRange];
            }

            return tonnageMod;
        }

        private static int GetTonnageRange(float tonnage) {
            return (int)Math.Floor(tonnage / 10.0);
        }

        /*
         * Take the BaseInitiative value from all components on the unit, remove the 
         *  normal phase modifiers HBS applies from SimGameConstants (-2 for light, etc),
         *  then invert the value. Because HBS defines bonuses as negative modifiers,
         *  invert this value to have it make sense elsewhere in the code.        
         */
        public static int GetNormalizedComponentModifier(AbstractActor actor) {
            int unitInit = 0;

            WeightClass weightClass;
            if (actor.GetType() == typeof(Mech)) {
                weightClass = ((Mech)actor).weightClass;
            } else if (actor.GetType() == typeof(Vehicle)) {
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
                        SkillBasedInit.LogDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has unknown or undefined weight class:{weightClass}!");
                        break;
                }

                // Because HBS init values were from 2-5, bonuses will be negative at this point and penalties positive. Invert these.
                unitInit = baseMod * -1;
                SkillBasedInit.LogDebug($"Normalized BaseInit from {actor.StatCollection.GetValue<int>("BaseInitiative")} to unitInit:{unitInit}");
            }

            return unitInit;
        }

        public static int GetEngineModifier(AbstractActor actor) {
            int engineMod = 0;
            if (actor.GetTags().Contains("unit_powerarmor")) {
                SkillBasedInit.LogDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} is PowerArmor, skipping engine bonus.");
            } else {
                var mainEngineComponent = actor?.allComponents?.FirstOrDefault(c => c?.componentDef?.GetComponent<EngineCoreDef>() != null);
                if (mainEngineComponent != null) {
                    var engine = mainEngineComponent?.componentDef?.GetComponent<EngineCoreDef>();
                    SkillBasedInit.LogDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has engine: {engine?.ToString()} with rating: {engine?.Rating}");

                    float tonnage = GetUnitTonnage(actor);
                    float engineRatio = engine.Rating / tonnage;

                    int tonnageRange = GetTonnageRange(tonnage);
                    int ratioMidpoint = EngineMidpointByTonnage[tonnageRange];
                    SkillBasedInit.LogDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has engineRatio:{engineRatio} against midpoint:{ratioMidpoint}");
                    if (engineRatio > ratioMidpoint) {
                        int oneSigma = (int)Math.Ceiling(ratioMidpoint * 1.2);
                        int twoSigma = (int)Math.Ceiling(ratioMidpoint * 1.4);
                        int threeSigma = (int)Math.Ceiling(ratioMidpoint * 1.7);
                        if (engineRatio < oneSigma) {
                            engineMod = 0;
                        } else if (engineRatio < twoSigma) {
                            engineMod = 1;
                        } else if (engineRatio < threeSigma) {
                            engineMod = 2;
                        } else {
                            engineMod = 3;
                        }
                        SkillBasedInit.LogDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has an oversized engine, gets engineMod:{engineMod}");
                    } else if (engineRatio < ratioMidpoint) {
                        int oneSigma = (int)Math.Floor(ratioMidpoint * 0.8);
                        int twoSigma = (int)Math.Floor(ratioMidpoint * 0.6);
                        int threeSigma = (int)Math.Floor(ratioMidpoint * 0.3);
                        if (engineRatio > oneSigma) {
                            engineMod = 0;
                        } else if (engineRatio > twoSigma) {
                            engineMod = -1;
                        } else if (engineRatio > threeSigma) {
                            engineMod = -2;
                        } else {
                            engineMod = -3;
                        }
                        SkillBasedInit.LogDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has an undersized engine, gets engineMod:{engineMod}");
                    } else {
                        SkillBasedInit.LogDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has balanced engine, +0 modifier.");
                        engineMod = 0;
                    }

                } else {
                    SkillBasedInit.Logger.Log($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has no engine - is this expected?");
                }
            }

            return engineMod;
        }
    }
}
