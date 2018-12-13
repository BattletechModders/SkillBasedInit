using BattleTech;
using CustomComponents;
using HBS.Collections;
using MechEngineer;
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
        readonly public int staticMod; // static modifiers from chassis, pilot that won't change during combat
        readonly public int roundInitBase; // Base initiative each round; chassisMod + tacticsMod + staticMod

        readonly public int gutsEffectMod;
        readonly public int pilotingEffectMod;
        readonly public int tacticsEffectMod;

        readonly public int meleeAttackMod;

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

        readonly public int[] randomBounds = new int[2];
        readonly public int[] injuryBounds = new int[2];

        // Track any changes in the previous round init
        public int priorRoundInit = -1;

        // If an injury was suffered on a previous round, but not handled, take the full impact on the coming round.
        public int deferredInjuryMod = 0;
        public int deferredMeleeMod = 0;
        public int deferredReserveMod = 0;

        // Const values 
        private const float TurretTonnage = 100.0f;

        private static readonly Dictionary<int, int[]> InjuryBoundsByGuts = new Dictionary<int, int[]> {
            {  1, new[] { 6, 9 } },
            {  2, new[] { 5, 8 } },
            {  3, new[] { 5, 8 } },
            {  4, new[] { 4, 7 } },
            {  5, new[] { 4, 7 } },
            {  6, new[] { 3, 6 } },
            {  7, new[] { 3, 6 } },
            {  8, new[] { 2, 5 } },
            {  9, new[] { 2, 5 } },
            { 10, new[] { 1, 4 } },
            { 11, new[] { 1, 3 } },
            { 12, new[] { 1, 3 } },
            { 13, new[] { 1, 2 } } 
        };

        private static readonly Dictionary<int, int[]> RandomBoundsByPiloting = new Dictionary<int, int[]> {
            {  1, new[] { 3, 9 } },
            {  2, new[] { 2, 8 } },
            {  3, new[] { 2, 8 } },
            {  4, new[] { 1, 7 } },
            {  5, new[] { 1, 7 } },
            {  6, new[] { 0, 6 } },
            {  7, new[] { 0, 6 } },
            {  8, new[] { 0, 5 } },
            {  9, new[] { 0, 5 } },
            { 10, new[] { 0, 4 } },
            { 11, new[] { 0, 3 } },
            { 12, new[] { 0, 3 } },
            { 13, new[] { 0, 2 } }
        };

        private static readonly Dictionary<int, int> ModifierBySkill = new Dictionary<int, int> {
            { 1, 0 },
            { 2, 1 },
            { 3, 1 },
            { 4, 2 },
            { 5, 2 },
            { 6, 3 },
            { 7, 3 },
            { 8, 4 },
            { 9, 4 },
            { 10, 5 },
            { 11, 6 },
            { 12, 7 },
            { 13, 8 }
        };

        private static readonly int SuperHeavyTonnage = 11;
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

        public ActorInitiative(AbstractActor actor) {
            //SkillBasedInit.Logger.Log($"Initializing ActorInitiative for {actor.DisplayName} with GUID {actor.GUID}.");

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
                staticMod -= SkillBasedInit.Settings.VehicleROCModifier;
            } else {
                this.type = ActorType.Turret;
                this.tonnage = TurretTonnage;
                
                TagSet actorTags = actor.GetTags();                
                if (actorTags != null && actorTags.Contains("unit_light")) {
                    weightClass = WeightClass.LIGHT;
                    this.tonnage = SkillBasedInit.Settings.TurretTonnageTagUnitLight;
                } else if (actorTags != null && actorTags.Contains("unit_medium")) {
                    weightClass = WeightClass.MEDIUM;
                    this.tonnage = SkillBasedInit.Settings.TurretTonnageTagUnitMedium;
                } else if (actorTags != null && actorTags.Contains("unit_heavy")) {
                    weightClass = WeightClass.HEAVY;
                    this.tonnage = SkillBasedInit.Settings.TurretTonnageTagUnitHeavy;
                } else {
                    weightClass = WeightClass.ASSAULT;
                    this.tonnage = SkillBasedInit.Settings.TurretTonnageTagUnitNone;
                }

                // TODO: Temporary malus for testing
                staticMod -= SkillBasedInit.Settings.TurretROCModifier;
            }

            // Determine if the mech has any special tags on it that grant bonus init
            // TODO: This must be compared against the 'base' value to see what the modifier is
            if (actor.StatCollection != null && actor.StatCollection.ContainsStatistic("BaseInitiative")) {
                int statCollectionVal = actor.StatCollection.GetValue<int>("BaseInitiative");
                int rawModifier = actor.StatCollection.GetValue<int>("BaseInitiative");
                
                // Normalize the value
                switch (weightClass) {
                    case WeightClass.LIGHT:
                        rawModifier -= 2;
                        break;
                    case WeightClass.MEDIUM:
                        rawModifier -= 3;
                        break;
                    case WeightClass.HEAVY:
                        rawModifier -= 4;
                        break;
                    case WeightClass.ASSAULT:
                        rawModifier -= 5;
                        break;
                    default:
                        SkillBasedInit.LogDebug($"Actor:{actor.DisplayName}_{actor.GetPilot().Name} has unknown or undefined weight class:{weightClass}!");
                        break;
                }

                // Because HBS init values were from 2-5, bonuses will be negative at this point and penalties positive. Invert these.
                this.staticMod = rawModifier * -1;
                SkillBasedInit.LogDebug($"Normalized BaseInit from {statCollectionVal} to rawModifier:{rawModifier} to {this.staticMod}");
            } else {
                this.staticMod = 0;
            }

            // Check morale status
            if (actor.HasHighMorale) {
                this.staticMod += SkillBasedInit.Settings.PilotSpiritsModifier;
            } else if (actor.HasLowMorale) {
                this.staticMod -= SkillBasedInit.Settings.PilotSpiritsModifier;
            }

            Pilot pilot = actor.GetPilot();
            this.pilotId = pilot.GUID;
            this.pilotName = pilot.Name;

            // Normalize skills so that values above 10 don't screw the system
            int gutsNormd = NormalizeSkill(pilot.Guts);
            this.gutsEffectMod = ModifierBySkill[gutsNormd];
            InjuryBoundsByGuts[gutsNormd].CopyTo(this.injuryBounds, 0);

            int pilotingNormd = NormalizeSkill(pilot.Piloting);
            this.pilotingEffectMod = ModifierBySkill[pilotingNormd];
            RandomBoundsByPiloting[pilotingNormd].CopyTo(this.randomBounds, 0);

            int tacticsNormd = NormalizeSkill(pilot.Tactics);
            this.tacticsEffectMod = ModifierBySkill[tacticsNormd];

            SkillBasedInit.LogDebug($"Actor:{actor.DisplayName}_{pilot.Name} skill profile is " +
            	$"p:{pilot.Piloting}->{pilotingNormd}={pilotingEffectMod} " +
            	$"t:{pilot.Tactics}->{tacticsNormd}={tacticsEffectMod} " +
            	$"g:{pilot.Guts}->{gutsNormd}={gutsEffectMod}");

            // Determine the static init value from chassis + tacctics
            int tonnageRange = (int)Math.Floor(tonnage / 10.0);
            int chassisBase;
            if (tonnageRange > 10) {
                chassisBase = InitBaseByTonnage[SuperHeavyTonnage];
            } else {
                chassisBase = InitBaseByTonnage[tonnageRange];
            }

            // Determine the melee modifier
            bool pilotIsJugger = false;
            foreach (Ability ability in pilot.Abilities) {
                if (ability.Def.Id == "AbilityDefGu5") {
                    //SkillBasedInit.LogDebug($"Actor:{actor.DisplayName}_{pilot.Name} has Juggernaught.");
                    pilotIsJugger = true;
                }
            }
            float meleeAttackTonnage = pilotIsJugger ? tonnage * SkillBasedInit.Settings.MeleeAttackerJuggerMulti : tonnage;
            this.meleeAttackMod = (int)Math.Ceiling(meleeAttackTonnage / 5.0);
            SkillBasedInit.LogDebug($"Actor:{actor.DisplayName}_{pilot.Name} has meleeAttackMod:{meleeAttackMod} from rawTonnage:{tonnage} / meleeTonnage:{meleeAttackTonnage} / isJugger:{pilotIsJugger}");

            int engineMod = 0;
            if (actor.GetTags().Contains("unit_powerarmor")) {
                SkillBasedInit.LogDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} is PowerArmor, skipping engine bonus.");
            } else {
                var mainEngineComponent = actor?.allComponents?.FirstOrDefault(c => c?.componentDef?.GetComponent<EngineCoreDef>() != null);
                if (mainEngineComponent != null) {
                    var engine = mainEngineComponent?.componentDef?.GetComponent<EngineCoreDef>();
                    SkillBasedInit.LogDebug($"  Actor:{actor.DisplayName}_{actor.GetPilot().Name} has engine: {engine?.ToString()} with rating: {engine?.Rating}");

                    float engineRatio = engine.Rating / tonnage;
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

            // Finally wrap together the static init we'll use each round
            roundInitBase = chassisBase + engineMod + tacticsEffectMod + staticMod;

            SkillBasedInit.Logger.Log($"Actor:{actor.DisplayName}_{pilot.Name} has " +
                $"roundInitBase:{roundInitBase} = (chassisBase:{chassisBase} + tacticsMod:{tacticsEffectMod} + staticMod:{staticMod}) " +
                $"randomBounds:({randomBounds[0]}-{randomBounds[1]}) " +
                $"injuryBounds:({injuryBounds[0]}-{injuryBounds[1]}) " +
                $"gutsMod:{gutsEffectMod} pilotingMod:{pilotingEffectMod} tacticsMod:{tacticsEffectMod}");
        }

        private int NormalizeSkill(int rawValue) {
            int normalizedVal = rawValue;
            if (rawValue >= 11 && rawValue <= 14) {
                // 11, 12, 13, 14 normalizes to 11
                normalizedVal = 11;
            } else if (rawValue >= 15 && rawValue <= 18) {
                // 15, 16, 17, 18 normalizes to 14
                normalizedVal = 12;
            } else if (rawValue == 19 || rawValue == 20) {
                // 19, 20 normalizes to 13
                normalizedVal = 13;
            } else if (rawValue <= 0) {
                normalizedVal = 1;
            } else if (rawValue > 20) {
                normalizedVal = 13;
            }
            return normalizedVal;
        }

        public void CalculateRoundInit(AbstractActor actor) {
            // If the actor is dead, skip them
            if (actor.IsDead || actor.IsFlaggedForDeath) {
                actor.Initiative = SkillBasedInit.MaxPhase;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) is dead, skipping init.");
                return;
            }

            // Generate a random element            
            int roundVariance = SkillBasedInit.Random.Next(this.randomBounds[0], this.randomBounds[1]);
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
                var vehicle = (Vehicle)actor;
                isMovementCrippled = vehicle.IsLocationDestroyed(VehicleChassisLocations.Left) || vehicle.IsLocationDestroyed(VehicleChassisLocations.Right) ? true : false;
            }

            if (isMovementCrippled) {
                int rawMod = SkillBasedInit.Settings.MovementCrippledMalus - this.pilotingEffectMod;
                SkillBasedInit.LogDebug($"  Crippled Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has rawMod:{rawMod} = ({SkillBasedInit.Settings.MovementCrippledMalus} - {this.pilotingEffectMod})");

                int penalty = Math.Min(-1, -1 * rawMod);
                roundInitiative += penalty;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has crippled movement! Subtracted {penalty} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"CRIPPLED! -{penalty} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }

            // Check for knockdown / prone 
            if (actor.IsProne) {
                int rawMod = SkillBasedInit.Settings.ProneMalus - this.pilotingEffectMod;
                SkillBasedInit.LogDebug($"  Prone Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has rawMod:{rawMod} = ({SkillBasedInit.Settings.ProneMalus} - {this.pilotingEffectMod})");

                int penalty = Math.Min(-1, -1 * rawMod);
                roundInitiative += penalty;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) is prone! Subtracted {penalty} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"PRONE! -{penalty} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }

            if (actor.IsShutDown) {
                int rawMod = SkillBasedInit.Settings.ShutdownMalus - this.pilotingEffectMod;
                SkillBasedInit.LogDebug($"  Shutdown Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has rawMod:{rawMod} = ({SkillBasedInit.Settings.ShutdownMalus} - {this.pilotingEffectMod})");

                int penalty = Math.Min(-1, -1 * rawMod);
                roundInitiative += penalty;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) is shutdown! Subtracted {penalty} = roundInit:{roundInitiative}");
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"SHUTDOWN! -{penalty} INITIATIVE", FloatieMessage.MessageNature.Debuff));

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
                actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"HESITATION! -{deferredReserveMod} INITIATIVE", FloatieMessage.MessageNature.Debuff));
            }

            // Check to see if the actor's initative changed in the prior round
            int delta = 0;
            if (this.priorRoundInit != -1) {
                delta = actor.Initiative - this.priorRoundInit;
                roundInitiative += delta;
                SkillBasedInit.Logger.Log($"  Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has init:{actor.Initiative} but priorInit:{this.priorRoundInit} - applying delta:{delta} to roundInit:{roundInitiative} reflect init changes during round.");
                /* 
                 * TODO: Should this be instead                 
                    string statName = (!addedBySelf) ? "PhaseModifier" : "PhaseModifierSelf";
                    __instance.StatCollection.ModifyStat<int>(sourceID, stackItemUID, statName, StatCollection.StatOperation.Int_Add, 1, -1, true);
                */
            }
            
            if (roundInitiative <= 0) {
                roundInitiative = 1;
                SkillBasedInit.Logger.Log($"  Round init {roundInitiative} less than 0, setting to 1.");
            } else if (roundInitiative > 30) {
                roundInitiative = 30;
                SkillBasedInit.Logger.Log($"  Round init {roundInitiative} greater than 30, setting to 30.");
            }

            // Init is flipped... 1 acts in first phase, then 2, etc.
            actor.Initiative = 31 - roundInitiative;
            SkillBasedInit.Logger.Log($"== Actor:({actor.DisplayName}_{actor.GetPilot().Name}) has roundInitiative:({roundInitiative}) from initBase:{roundInitBase} - variance:{roundVariance} plus modifiers.");
        }

        public static int CalculateMeleeDelta(ActorInitiative attacker, ActorInitiative target) {
            // Always return 1 or more
            int rawDelta = Math.Max(1, attacker.meleeAttackMod - target.meleeAttackMod);
            SkillBasedInit.LogDebug($"Melee rawDelta:{rawDelta} = (attackerMod:{attacker.meleeAttackMod} - targetMod:{target.meleeAttackMod})");
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
