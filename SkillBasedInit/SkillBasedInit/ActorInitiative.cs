using BattleTech;
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

        readonly public int meleeAttackMod;
        readonly public int meleeDefenseMod;

        readonly public bool pilotHasTagReckless;
        readonly public bool pilotHasTagCautious;
        readonly public bool pilotHasTagLucky;
        readonly public bool pilotHasTagJinxed;
        readonly public bool pilotHasTagKlutz;
        /*
            Reckless: Bonuses to-hit in combat, but easier to be hit.
            Cautious: Penalty to-hit in combat, but harder to be hit.
            Lucky: Chance to avoid getting wounded.
            Jinxed: Easier for this pilot to-be-hit, harder for them to-hit.
            Klutz: Reduced stability from piloting.
        */

        readonly public int[] randomnessBounds = new int[2];
        readonly public int[] injuryBounds = new int[2];

        // If an injury was suffered on a previous round, but not handled, take the full impact on the coming round.
        public int deferredInjuryMod = 0;
        public int deferredMeleeMod = 0;
        public int deferredReserveMod = 0;

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
            // Static initiative from tonnage
            float tonnage = UnitHelper.GetUnitTonnage(actor);
            int tonnageMod = UnitHelper.GetTonnageModifier(tonnage);

            // Any special modifiers by type
            int typeMod = UnitHelper.GetTypeModifier(actor);

            // Any modifiers that come from the chassis/mech/vehicle defs
            int componentsMod = UnitHelper.GetNormalizedComponentModifier(actor);

            // Modifier from the engine
            int engineMod = UnitHelper.GetEngineModifier(actor);

            // --- PILOT IMPACTS ---
            Pilot pilot = actor.GetPilot();
            PilotHelper.LogPilotStats(pilot);

            // Normalize skills so that values above 10 don't screw the system
            this.gutsEffectMod = PilotHelper.GetGutsModifier(pilot);
            this.injuryBounds = PilotHelper.GetInjuryBounds(pilot);

            this.pilotingEffectMod = PilotHelper.GetPilotingModifier(pilot);
            this.randomnessBounds = PilotHelper.GetRandomnessBounds(pilot);

            this.tacticsEffectMod = PilotHelper.GetTacticsModifier(pilot);

            int pilotTagsMod = PilotHelper.GetTagsModifier(pilot);

            // --- COMBO IMPACTS --
            // Determine the melee modifier
            int[] meleeMods = PilotHelper.GetMeleeModifiers(pilot, tonnage);
            this.meleeAttackMod = meleeMods[0];
            this.meleeDefenseMod = meleeMods[1];
            SkillBasedInit.LogDebug($"Actor:{actor.DisplayName}_{pilot.Name} has meleeAttackMod:{meleeAttackMod} meleeDefenseMod:{meleeDefenseMod}");

            // Log the full view for testing
            roundInitBase = tonnageMod;
            roundInitBase += typeMod;
            roundInitBase += componentsMod;
            roundInitBase += engineMod;
            roundInitBase += tacticsEffectMod;
            roundInitBase += pilotTagsMod;

            SkillBasedInit.Logger.Log($"Actor:{actor.DisplayName}_{pilot.Name} has " +
                $"roundInitBase:{roundInitBase} = (tonnage:{tonnageMod} + typeMod:{typeMod} + components:{componentsMod} + engine:{engineMod} " +
                $"tactics:{tacticsEffectMod} + pilotTags:{pilotTagsMod}) " +
                $"randomness:({randomnessBounds[0]}-{randomnessBounds[1]}) " +
                $"injuryBounds:({injuryBounds[0]}-{injuryBounds[1]}) " +
                $"gutsMod:{gutsEffectMod} pilotingMod:{pilotingEffectMod} tacticsMod:{tacticsEffectMod}");
        }

        public void CalculateRoundInit(AbstractActor actor) {
            // If the actor is dead, skip them
            if (actor.IsDead || actor.IsFlaggedForDeath) {
                actor.Initiative = SkillBasedInit.MaxPhase;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) is dead, skipping init.");
                return;
            }

            // Generate a random element            
            int roundVariance = SkillBasedInit.Random.Next(this.randomnessBounds[0], this.randomnessBounds[1]);
            int roundInitiative = this.roundInitBase - roundVariance;
            SkillBasedInit.LogDebug($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has roundInit:{roundInitiative} = (roundInitbase:{roundInitBase} - roundVariance:{roundVariance})");

            // Check for inspired status
            if (actor.IsMoraleInspired || actor.IsFuryInspired) {
                int bonus = SkillBasedInit.Random.Next(1, 3);
                roundInitiative += bonus;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) is inspired, added {bonus} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"INSPIRED! +{bonus} INITIATIVE", FloatieMessage.MessageNature.Buff));
            }

            // Check for injuries. If there injuries on the previous round, apply them in full force. Otherwise, reduce them.
            if (deferredInjuryMod != 0) {
                roundInitiative -= deferredInjuryMod;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) was injured on a previous round! Subtracted {deferredInjuryMod} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"INJURED! -{deferredInjuryMod} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            } else if (actor.GetPilot().Injuries != 0) {
                // Only apply 1/2 of the modifier for 'old wounds'
                int rawPenalty = this.CalculateInjuryPenalty(0, actor.GetPilot().Injuries);
                int penalty = (int)Math.Ceiling(rawPenalty / 2.0);
                roundInitiative -= penalty;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has existing injuries! Subtracted {penalty} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"PAIN! -{penalty} INITIATIVE", FloatieMessage.MessageNature.Debuff));
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
                int rawMod = SkillBasedInit.Settings.CrippledMovementModifier + this.pilotingEffectMod;
                SkillBasedInit.LogDebug($"  Crippled Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has rawMod:{rawMod} = ({SkillBasedInit.Settings.CrippledMovementModifier} - {this.pilotingEffectMod})");

                int penalty = Math.Min(-1, rawMod);
                roundInitiative += penalty;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has crippled movement! Subtracted {penalty} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"CRIPPLED! {penalty} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }

            // Check for prone 
            if (actor.IsProne) {
                int rawMod = SkillBasedInit.Settings.ProneModifier + this.pilotingEffectMod;
                SkillBasedInit.LogDebug($"  Prone Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has rawMod:{rawMod} = ({SkillBasedInit.Settings.ProneModifier} - {this.pilotingEffectMod})");

                int penalty = Math.Min(-1, rawMod);
                roundInitiative += penalty;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) is prone! Subtracted {penalty} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"PRONE! {penalty} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }

            // Check for shutdown
            if (actor.IsShutDown) {
                int rawMod = SkillBasedInit.Settings.ShutdownModifier + this.pilotingEffectMod;
                SkillBasedInit.LogDebug($"  Shutdown Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has rawMod:{rawMod} = ({SkillBasedInit.Settings.ShutdownModifier} - {this.pilotingEffectMod})");

                int penalty = Math.Min(-1, rawMod);
                roundInitiative += penalty;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) is shutdown! Subtracted {penalty} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"SHUTDOWN! {penalty} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }

            // Check for melee impacts        
            if (this.deferredMeleeMod > 0) {
                roundInitiative -= deferredMeleeMod;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) was meleed after activation! Subtracted {this.deferredMeleeMod} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"MELEED! -{deferredMeleeMod} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }

            // Check for an overly cautious player
            if (this.deferredReserveMod > 0) {
                roundInitiative -= deferredReserveMod;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) deferred last round! Subtracted {this.deferredReserveMod} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"HESITATED! -{deferredReserveMod} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }
            
            if (roundInitiative <= 0) {
                roundInitiative = SkillBasedInit.MinPhase;
                SkillBasedInit.LogDebug($"  Round init {roundInitiative} less than 0, setting to 1.");
            } else if (roundInitiative > 30) {
                roundInitiative = SkillBasedInit.MaxPhase;
                SkillBasedInit.LogDebug($"  Round init {roundInitiative} greater than 30, setting to 30.");
            }

            // Init is flipped... 1 acts in first phase, then 2, etc.
            actor.Initiative = (SkillBasedInit.MaxPhase + 1) - roundInitiative;
            SkillBasedInit.Logger.Log($"== Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has init:({roundInitiative}) from base:{roundInitBase} - variance:{roundVariance} plus modifiers.");
        }

        public static int CalculateMeleeDelta(ActorInitiative attacker, ActorInitiative target) {
            // Always return 1 or more
            int rawDelta = Math.Max(1, attacker.meleeAttackMod - target.meleeDefenseMod);
            SkillBasedInit.LogDebug($"Melee rawDelta:{rawDelta} = (attackerMod:{attacker.meleeAttackMod} - targetMod:{target.meleeDefenseMod})");
            int modifiedDelta = Math.Max(1, rawDelta - target.gutsEffectMod);
            SkillBasedInit.LogDebug($"MeleeMod reduced to modifiedDelta:{modifiedDelta} = (rawDelta:{rawDelta} - gutsEffectMod:{target.gutsEffectMod})");
            return modifiedDelta;
        }

        public void ResolveMeleeImpact(AbstractActor target, int impact) {
        
            if (target.HasActivatedThisRound) {
                SkillBasedInit.Logger.Log($"Melee impact will slow Actor:({target.DisplayName}_{target.GetPilot().Name}) by {impact} init on next activation!");
                this.deferredMeleeMod += impact;
                target.Combat.MessageCenter.PublishMessage(new FloatieMessage(target.GUID, target.GUID, $"CLANG! -{impact} INITIATIVE NEXT ROUND", FloatieMessage.MessageNature.Debuff));
            } else {
                SkillBasedInit.Logger.Log($"Melee impact immediately slows Actor:({target.DisplayName}_{target.GetPilot().Name}) by {impact} init!");
                // Add to the target's initiative. Remember higher init -> higher phase
                target.Initiative += impact;
                if (target.Initiative > SkillBasedInit.MaxPhase) {
                    target.Initiative = SkillBasedInit.MaxPhase;
                }
                target.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(target.GUID));
                target.Combat.MessageCenter.PublishMessage(new FloatieMessage(target.GUID, target.GUID, $"CLANG! -{impact} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }
        }

        public int CalculateInjuryPenalty(int damageTaken, int existingInjuries) {
            int injuryMax = this.injuryBounds[1] + existingInjuries + damageTaken;
            int injuryMin = this.injuryBounds[0] + existingInjuries;
            int injuryPenalty = SkillBasedInit.Random.Next(injuryMin, injuryMax);
            SkillBasedInit.LogDebug($"  InjuryPenalty:-{injuryPenalty} from (injuryMin:{this.injuryBounds[0]} + existingInjuries:{existingInjuries}) to (injuryMax:{this.injuryBounds[1]} + existingInjuries:{existingInjuries} + damageTaken:{damageTaken}).");
            return injuryPenalty;
        }
    }

    public static class ActorInitiativeHolder {

        private static readonly Dictionary<string, ActorInitiative> actorInitMap = new Dictionary<string, ActorInitiative>();
        public static Dictionary<string, ActorInitiative> ActorInitMap { get => actorInitMap; }

        public static void OnRoundBegin(AbstractActor actor) {
            if (actor != null) {
                // If the actor already exists, don't change them
                ActorInitiative actorInit = GetOrCreate(actor);

                // Recalculate the random part of their initiative for the round
                actorInit.CalculateRoundInit(actor);

                actorInit.deferredInjuryMod = 0;
                actorInit.deferredMeleeMod = 0;
                actorInit.deferredReserveMod = 0;
            }
        }

        public static void OnCombatComplete() {
            ActorInitMap.Clear();
        }

        // Units can spawn before they get onRoundBegin called, and so may not be present. This ensures they get added if they are missing.
        public static ActorInitiative GetOrCreate(AbstractActor actor) {
            if (!ActorInitMap.ContainsKey(actor.GUID)) {
                var actorInit = new ActorInitiative(actor);
                ActorInitMap[actor.GUID] = actorInit;
            }
            return ActorInitMap[actor.GUID];
        }

    }
}
