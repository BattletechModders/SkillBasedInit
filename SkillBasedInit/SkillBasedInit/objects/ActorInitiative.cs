using BattleTech;
using IRBTModUtils.Extension;
using Localize;
using SkillBasedInit.Helper;
using System;
using System.Collections.Generic;

namespace SkillBasedInit {

    public enum ActorType { Mech, Vehicle, Turret };

    public class ActorInitiative {
        public ActorType type = ActorType.Mech;

        /*
         * Static initiative each round
         */        
        readonly public int roundInitBase;

        readonly public int gutsEffectMod;
        readonly public int pilotingEffectMod;
        readonly public int tacticsEffectMod;

        readonly public int calledShotMod;
        readonly public int vigilianceMod;

        readonly public int[] randomnessBounds = new int[2];
        readonly public int[] injuryBounds = new int[2];

        // Values carried over from a previous round
        public int deferredInjuryMod = 0;
        public int deferredCalledShotMod = 0;
        public int deferredVigilanceMod = 0;
        public int reservedCount = 0;

        // Values preserved for UI display
        public int lastRoundInjuryMod = 0;
        public int lastRoundCalledShotMod = 0;
        public int lastRoundVigilanceMod = 0;
        public int lastRoundHesitationPenalty = 0;
        public int lastRoundReservedCount = 0;

        public ActorInitiative(AbstractActor actor) {
            //SkillBasedInit.Logger.Log($"Initializing ActorInitiative for {actor.DisplayName} with GUID {actor.GUID}.");

            if (actor.GetType() == typeof(Mech)) {
                this.type = ActorType.Mech;
            } else if (actor.GetType() == typeof(Vehicle)) {
                this.type = ActorType.Vehicle;
            } else {
                this.type = ActorType.Turret;
            }

            // --- UNIT IMPACTS ---


            // --- PILOT IMPACTS ---
            Pilot pilot = actor.GetPilot();
            PilotHelper.LogPilotStats(pilot);

            // Normalize skills so that values above 10 don't screw the system
            this.gutsEffectMod = PilotHelper.GetGutsModifier(pilot);
            this.injuryBounds = PilotHelper.GetInjuryBounds(pilot);

            this.pilotingEffectMod = PilotHelper.GetPilotingModifier(pilot);
            this.randomnessBounds = PilotHelper.GetRandomnessBounds(pilot);

            this.tacticsEffectMod = PilotHelper.GetTacticsModifier(pilot);

            this.calledShotMod = PilotHelper.GetCalledShotModifier(pilot);
            this.vigilianceMod = PilotHelper.GetVigilanceModifier(pilot);

            // --- COMBO IMPACTS --

            // Log the full view for testing
            roundInitBase = tonnageMod;
            roundInitBase += typeMod;
            roundInitBase += componentsMod;
            roundInitBase += engineMod;
            roundInitBase += tacticsEffectMod;
            roundInitBase += pilotTagsMod;

            Mod.Log.Info?.Write($"Actor:{actor.DisplayName}_{pilot.Name} has " +
                $"roundInitBase:{roundInitBase} = (tonnage:{tonnageMod} + typeMod:{typeMod} + components:{componentsMod} + engine:{engineMod} " +
                $"tactics:{tacticsEffectMod} + pilotTags:{pilotTagsMod}) " +
                $"randomness:({randomnessBounds[0]}-{randomnessBounds[1]}) " +
                $"injuryBounds:({injuryBounds[0]}-{injuryBounds[1]}) " +
                $"gutsMod:{gutsEffectMod} pilotingMod:{pilotingEffectMod} tacticsMod:{tacticsEffectMod}");
        }

        public void CalculateRoundInit(AbstractActor actor) {
            // If the actor is dead, skip them
            if (actor.IsDead || actor.IsFlaggedForDeath) {
                actor.Initiative = Mod.MaxPhase;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} is dead, skipping init.");
                return;
            }

            // Generate a random element            
            int roundVariance = Mod.Random.Next(this.randomnessBounds[0], this.randomnessBounds[1]);
            int roundInitiative = this.roundInitBase - roundVariance;
            Mod.Log.Debug?.Write($"  Actor: {actor.DistinctId()} has roundInit:{roundInitiative} = (roundInitbase:{roundInitBase} - roundVariance:{roundVariance})");

            // Check for inspired status
            if (actor.IsMoraleInspired || actor.IsFuryInspired) {
                int bonus = Mod.Random.Next(1, 3);
                roundInitiative += bonus;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} is inspired, added {bonus} = roundInit:{roundInitiative}");                
            }

            // Check for injuries. If there injuries on the previous round, apply them in full force. Otherwise, reduce them.
            if (deferredInjuryMod != 0) {
                roundInitiative -= deferredInjuryMod;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} was injured on a previous round! Subtracted {deferredInjuryMod} = roundInit:{roundInitiative}");                
            } else if (actor.GetPilot().Injuries != 0) {
                // Only apply 1/2 of the modifier for 'old wounds'
                int rawPenalty = this.CalculateInjuryPenalty(0, actor.GetPilot().Injuries);
                int penalty = (int)Math.Ceiling(rawPenalty / 2.0);
                roundInitiative -= penalty;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} has existing injuries! Subtracted {penalty} = roundInit:{roundInitiative}");                
            }

            // Check for leg / side loss
            var isMovementCrippled = false;
            if (this.type == ActorType.Mech) {
                var mech = (Mech)actor;
                isMovementCrippled = mech.IsLocationDestroyed(ChassisLocations.LeftLeg) || mech.IsLocationDestroyed(ChassisLocations.RightLeg) ? true : false;
            } else if (this.type == ActorType.Vehicle) {
                // TODO: This is pretty unlikely; vehicles rarely get crippled before they are destroyed. Find another solution?
                var vehicle = (Vehicle)actor;
                isMovementCrippled = vehicle.IsLocationDestroyed(VehicleChassisLocations.Left) || vehicle.IsLocationDestroyed(VehicleChassisLocations.Right) ? true : false;
            }

            if (isMovementCrippled) {
                int rawMod = Mod.Config.CrippledMovementModifier + this.pilotingEffectMod;
                Mod.Log.Debug?.Write($"  Crippled Actor: {actor.DistinctId()} has rawMod:{rawMod} = ({Mod.Config.CrippledMovementModifier} - {this.pilotingEffectMod})");

                int penalty = Math.Min(-1, rawMod);
                roundInitiative += penalty;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} has crippled movement! Subtracted {penalty} = roundInit:{roundInitiative}");                
            }

            // Check for prone 
            if (actor.IsProne) {
                int rawMod = Mod.Config.ProneModifier + this.pilotingEffectMod;
                Mod.Log.Debug?.Write($"  Prone Actor: {actor.DistinctId()} has rawMod:{rawMod} = ({Mod.Config.ProneModifier} - {this.pilotingEffectMod})");

                int penalty = Math.Min(0, rawMod);
                roundInitiative += penalty;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} is prone! Subtracted {penalty} = roundInit:{roundInitiative}");                
            }

            // Check for shutdown
            if (actor.IsShutDown) {
                int rawMod = Mod.Config.ShutdownModifier + this.pilotingEffectMod;
                Mod.Log.Debug?.Write($"  Shutdown Actor: {actor.DistinctId()} has rawMod:{rawMod} = ({Mod.Config.ShutdownModifier} - {this.pilotingEffectMod})");
                int penalty = Math.Min(0, rawMod);
                roundInitiative += penalty;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} is shutdown! Subtracted {penalty} = roundInit:{roundInitiative}");                
            }

            // Check for an overly cautious player
            if (this.reservedCount > 0) {
                int reserveRand = Mod.Random.Next(Mod.Config.HesitationPenaltyBounds[0], Mod.Config.HesitationPenaltyBounds[1]);
                int hesitationPenalty = reserveRand * this.reservedCount - this.tacticsEffectMod;
                this.lastRoundHesitationPenalty = hesitationPenalty;
                roundInitiative -= hesitationPenalty;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} deferred last round! " +
                    $"Random penalty:{reserveRand} * reserveCount:{this.reservedCount} - tacticsMod:{this.tacticsEffectMod} = penalty:{hesitationPenalty}. " +
                    $"roundInit currently:{roundInitiative}");
            }

            // Check for deferred called shot
            if (this.deferredCalledShotMod > 0) {
                roundInitiative -= deferredCalledShotMod;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} was targeted by called shot after activation! Subtracted {this.deferredCalledShotMod} = roundInit:{roundInitiative}");                
            }

            // Check for deferred vigilance bonus
            if (this.deferredVigilanceMod > 0) {
                roundInitiative += deferredVigilanceMod;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} did vigilance last round! Adding {this.deferredVigilanceMod} = roundInit:{roundInitiative}");
            }

            if (roundInitiative <= 0) {
                roundInitiative = Mod.MinPhase;
                Mod.Log.Debug?.Write($"  Round init {roundInitiative} less than 0, setting to 1.");
            } else if (roundInitiative > 30) {
                roundInitiative = Mod.MaxPhase;
                Mod.Log.Debug?.Write($"  Round init {roundInitiative} greater than 30, setting to 30.");
            }

            // Init is flipped... 1 acts in first phase, then 2, etc.
            if (!actor.Combat.TurnDirector.IsInterleaved) { actor.Initiative = actor.Combat.TurnDirector.NonInterleavedPhase;  }
            else { actor.Initiative = (Mod.MaxPhase + 1) - roundInitiative; }

            Mod.Log.Info?.Write($"== Actor: {actor.DistinctId()} has init:({roundInitiative}) from base:{roundInitBase} - variance:{roundVariance} plus modifiers.");
        }

        public int CalculateInjuryPenalty(int damageTaken, int existingInjuries) {
            int injuryMax = this.injuryBounds[1] + existingInjuries + damageTaken;
            int injuryMin = this.injuryBounds[0] + existingInjuries;
            int injuryPenalty = Mod.Random.Next(injuryMin, injuryMax);
            Mod.Log.Debug?.Write($"  InjuryPenalty:-{injuryPenalty} from (injuryMin:{this.injuryBounds[0]} + existingInjuries:{existingInjuries}) to (injuryMax:{this.injuryBounds[1]} + existingInjuries:{existingInjuries} + damageTaken:{damageTaken}).");
            return injuryPenalty;
        }
    }

}
