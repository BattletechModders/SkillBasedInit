using BattleTech;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SkillBasedInit {

    public enum ActorType { Mech, Vehicle, Turret };

    public class ActorInitiative {
        readonly string pilotId;
        readonly string pilotName;
        public ActorType type = ActorType.Mech;
        readonly public float tonnage;
        public int priorRoundInit = -1; // Tracks the previous round init
        readonly public int chassisBaseMod; // base initiative value from the chassis before any effects
        readonly public int[] roundInitBounds;
        readonly public double pilotingEffectMulti;        

        readonly public int[] injuryBounds;
        readonly public int ignoredInjuries;

        public float meleeImpact;

        // Const values
        private const double PilotingCrippledMultiplier = 0.05;
        private const int MeleedModifier = 6;
        private const int ProneOrShutdownModifier = -6;
        private const double CrippledMovementModifier = -13.0;        
        private const float TurretTonnage = 100.0f;

        private static readonly Dictionary<int, int[]> GutsInjuryBounds = new Dictionary<int, int[]> {
            {  1, new[] { 4, 7 } }, // -1
            {  2, new[] { 4, 7 } }, // -1
            {  3, new[] { 4, 7 } }, // -1
            {  4, new[] { 4, 7 } }, // -1
            {  5, new[] { 3, 6 } }, // -2
            {  6, new[] { 3, 6 } }, // -2
            {  7, new[] { 3, 6 } }, // -2
            {  8, new[] { 3, 6 } }, // -2
            {  9, new[] { 2, 5 } }, // -3
            { 10, new[] { 1, 4 } }  // -4
        };

        private static readonly int SuperHeavyTonnage = 11;
        private static readonly Dictionary<int, double> InitMultiBaseByTonnage = new Dictionary<int, double> {
            {  0, 1.6 }, // 0-15
            {  1, 1.5 }, // 10-15
            {  2, 1.4 }, // 20-25
            {  3, 1.3 }, // 30-35
            {  4, 1.2 }, // 40-45
            {  5, 1.1 }, // 50-55
            {  6, 1.0 }, // 60-65
            {  7, 0.9 }, // 70-75
            {  8, 0.8 }, // 80-85
            {  9, 0.7 }, // 90-95
            { 10, 0.6 }, // 100
            { SuperHeavyTonnage, 0.2 }, // 105+
        };

        public ActorInitiative(AbstractActor actor) {

            // Mech and vehicles has weightclass, tonnage. Turrets do not.
            WeightClass weightClass;
            if (actor.GetType() == typeof(Mech)) {
                this.type = ActorType.Mech;
                this.tonnage = ((Mech)actor).tonnage;
                weightClass = ((Mech)actor).weightClass;
            } else if (actor.GetType() == typeof(Vehicle)) {
                this.type = ActorType.Vehicle;
                this.tonnage = ((Vehicle)actor).tonnage;
                weightClass = ((Vehicle)actor).weightClass;

                // TODO: Temporary malus for testing purposes
            } else {
                this.type = ActorType.Turret;
                this.tonnage = TurretTonnage;
                
                TagSet actorTags = actor.GetTags();
                if (actorTags.Contains("unit_light")) {
                    weightClass = WeightClass.LIGHT;
                } else if (actorTags.Contains("unit_medium")) {
                    weightClass = WeightClass.MEDIUM;
                } else if (actorTags.Contains("unit_heavy")) {
                    weightClass = WeightClass.HEAVY;
                } else {
                    weightClass = WeightClass.ASSAULT;
                }

                // TODO: Apply turret init malus
            }

            // Determine if the mech has any special tags on it that grant bonus init
            // TODO: This must be compared against the 'base' value to see what the modifier is
            if (actor.StatCollection.ContainsStatistic("BaseInitiative")) {
                var rawModifier = actor.StatCollection.GetValue<int>("BaseInitiative");
                // Normalize the value
                if (weightClass == WeightClass.LIGHT) {
                    rawModifier -= 2;
                } else if (weightClass == WeightClass.MEDIUM) {
                    rawModifier -= 3;
                } else if (weightClass == WeightClass.HEAVY) {
                    rawModifier -= 4;
                } else if (weightClass == WeightClass.ASSAULT) {
                    rawModifier -= 5;
                }
                this.chassisBaseMod = rawModifier;
            } else {
                this.chassisBaseMod = 0;
            }

            // Check morale status
            if (actor.HasHighMorale) {
                this.chassisBaseMod += 2;
            } else if (actor.HasLowMorale) {
                this.chassisBaseMod -= 2;
            }

            // TODO: Temporary mods for testing 
            if (actor.GetType() == typeof(Vehicle)) {
                chassisBaseMod -= 2;
            } else if (actor.GetType() == typeof(Turret)) {
                chassisBaseMod -= 4;
            }

            var pilot = actor.GetPilot();
            this.pilotId = pilot.GUID;
            this.pilotName = pilot.Name;

            double skillsInitModifier = Math.Floor((pilot.Tactics * 2.0 + pilot.Piloting) / 3.0) / 10.0;
            this.roundInitBounds = RoundInitFromTonnage(this.tonnage, skillsInitModifier);

            this.pilotingEffectMulti = 1.0 - (pilot.Piloting * PilotingCrippledMultiplier);

            this.injuryBounds = GutsInjuryBounds[pilot.Guts];
            this.ignoredInjuries = pilot.BonusHealth;       
            SkillBasedInit.Logger.Log($"Pilot {pilot.GUID} with name: {pilot.Name} has " +
                $"skillsMod:{skillsInitModifier} (from piloting:{pilot.Piloting} tactics:{pilot.Tactics}) " +
                $"injuryBounds: {injuryBounds[0]}-{injuryBounds[1]} ignoredInjuries:{ignoredInjuries} " +
                $"roundInitBounds:{roundInitBounds[0]}-{roundInitBounds[1]} chassisBaseMod: {chassisBaseMod} " +
                $"crippledMulti:{pilotingEffectMulti}");
        }

        private int[] RoundInitFromTonnage(float tonnage, double skillInitModifier) {
            int tonnageRange = (int)Math.Floor(tonnage / 10.0);

            double initBaseMulti = 0.0;
            if (tonnageRange <= 10) {
                initBaseMulti = InitMultiBaseByTonnage[tonnageRange];
            } else if (tonnageRange > 10) {
                initBaseMulti = InitMultiBaseByTonnage[SuperHeavyTonnage];
            }

            double initBoundMulti = initBaseMulti + skillInitModifier;
            int roundMin = (int)Math.Floor(6 * initBoundMulti);
            int roundMax = (int)Math.Ceiling(12 * initBoundMulti);
            SkillBasedInit.Logger.Log($"For skillMod:{skillInitModifier} + baseMod:{initBaseMulti} yielded bounds: {roundMin} - {roundMax}");
            return new int[] { roundMin, roundMax };
        }

        public void AddMeleeImpact(float impactDelta) {
            if (impactDelta >= this.meleeImpact) {
                this.meleeImpact = impactDelta;
            }
        }

        public void CalculateRoundInit(AbstractActor actor) {                  

            // Generate a random element            
            int roundVariance = SkillBasedInit.Random.Next(this.roundInitBounds[0], this.roundInitBounds[1]);
            int roundInitiative = this.chassisBaseMod + roundVariance;

            // Check for inspired status
            if (actor.IsMoraleInspired || actor.IsFuryInspired) {
                int inspiredBonus = SkillBasedInit.Random.Next(1, 3);
                SkillBasedInit.Logger.Log($"Pilot {this.pilotName} is inspired, adding {inspiredBonus} to init.");
                roundInitiative += inspiredBonus;
            }

            // Check for injuries
            var slowingInjuries = actor.GetPilot().Injuries - this.ignoredInjuries;
            if (slowingInjuries > 0) {
                int injuryModifier = SkillBasedInit.Random.Next(this.injuryBounds[0], this.injuryBounds[1]);
                SkillBasedInit.Logger.Log($"Pilot {this.pilotName} has injuries! Reduced {roundInitiative} by {injuryModifier}");
                roundInitiative -= injuryModifier;
            }

            // Check for leg / side loss
            var isMovementCrippled = false;
            if (this.type == ActorType.Mech) {
                var mech = (Mech)actor;
                isMovementCrippled = mech.IsLocationDestroyed(ChassisLocations.LeftLeg) || mech.IsLocationDestroyed(ChassisLocations.RightLeg) ? true : false;
            } else if (this.type == ActorType.Vehicle) {
                var vehicle = (Vehicle)actor;
                isMovementCrippled = vehicle.IsLocationDestroyed(VehicleChassisLocations.Left) || vehicle.IsLocationDestroyed(VehicleChassisLocations.Right) ? true : false;
            }
            if (isMovementCrippled) {
                int crippledLoss = (int)Math.Floor(CrippledMovementModifier * pilotingEffectMulti);
                SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} has crippled movement! Reduced {roundInitiative} by {crippledLoss}");
                roundInitiative += crippledLoss;
            }
            // TODO: Check for 'solid' impacts

            // Check for melee impacts
            // TODO: Compare weights on impact to determine modifer? Or just make it part of a knockdown mod?            
            if (actor.WasMeleedSinceLastActivation) {
                SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} was meleed! Reduced {roundInitiative} by {MeleedModifier}");
                roundInitiative -= MeleedModifier;
            }

            // Check for knockdown / prone / shutdown
            // TODO: Piloting skill impacts this
            if (actor.IsProne || actor.IsShutDown) {
                int initDelay = (int)Math.Floor(ProneOrShutdownModifier * pilotingEffectMulti);
                SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} is prone or shutdown! Reduced {roundInitiative} by {ProneOrShutdownModifier}");
                roundInitiative += ProneOrShutdownModifier;
            }

            // Check to see if the actor's initative changed in the prior round
            int delta = 0;
            if (this.priorRoundInit != -1) {
                delta = actor.Initiative - this.priorRoundInit;
                roundInitiative += delta;
                SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} has init:{actor.Initiative} but priorInit:{this.priorRoundInit} - applying delta:{delta} to roundInit:{roundInitiative} reflect init changes during round.");
                /* 
                 * TODO: Should this be instead                 
                    string statName = (!addedBySelf) ? "PhaseModifier" : "PhaseModifierSelf";
                    __instance.StatCollection.ModifyStat<int>(sourceID, stackItemUID, statName, StatCollection.StatOperation.Int_Add, 1, -1, true);
                */
            }
            


            if (roundInitiative < 0) {
                roundInitiative = 1;
                SkillBasedInit.Logger.Log($"Round init {roundInitiative} less than 0.");
            } else if (roundInitiative > 30) {
                roundInitiative = 30;
                SkillBasedInit.Logger.Log($"Round init {roundInitiative} greater than 30.");
            }

            // Init is flipped... 1 acts in first phase, then 2, etc.
            actor.Initiative = 31 - roundInitiative;

        SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} ends up with roundInitiative {roundInitiative} from bounds {roundInitBounds[0]}-{roundInitBounds[1]}");
        }

    }

    public static class ActorInitiativeHolder {

        private static readonly Dictionary<string, ActorInitiative> actorInitMap = new Dictionary<string, ActorInitiative>();
        public static Dictionary<string, ActorInitiative> ActorInitMap { get => actorInitMap; }

        public static void OnRoundBegin(AbstractActor actor) {
            if (actor != null) {
                // If the actor already exists, don't change them
                if (!ActorInitMap.ContainsKey(actor.GUID)) {
                    AddActor(actor);
                }
                var actorInit = ActorInitMap[actor.GUID];

                // Recalculate the random part of their initiative for the round
                actorInit.CalculateRoundInit(actor);

                actorInit.meleeImpact = 0.0f;

            }
        }

        public static void OnCombatComplete() {
            ActorInitMap.Clear();
        }

        private static void AddActor(AbstractActor actor) {
            var actorInit = new ActorInitiative(actor);

            // Recalc init in case we're coming from a save
            //actorInit.CalculateRoundInit(actor);

            ActorInitMap[actor.GUID] = actorInit;
        }

    }
}
