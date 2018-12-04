using BattleTech;
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
        public int priorRoundInit = -1; // Tracks the previous round init
        readonly public int chassisBaseMod; // base initiative value from the chassis before any effects
        readonly public int[] roundInitBounds;
        readonly public double pilotingEffectMulti;
        readonly public double gutsEffectMulti;

        readonly public int[] injuryBounds;
        readonly public int ignoredInjuries;

        public float meleeImpact = 0.0f;

        // Const values 
        private const float TurretTonnage = 100.0f;

        private static readonly Dictionary<int, int[]> GutsInjuryBounds = new Dictionary<int, int[]> {
            {  1, new[] { 6, 9 } },
            {  2, new[] { 5, 8 } },
            {  3, new[] { 5, 8 } },
            {  4, new[] { 5, 8 } },
            {  5, new[] { 4, 7 } },
            {  6, new[] { 4, 7 } },
            {  7, new[] { 3, 6 } },
            {  8, new[] { 3, 6 } },
            {  9, new[] { 2, 5 } },
            { 10, new[] { 1, 4 } },
            { 11, new[] { 0, 4 } },
            { 12, new[] { 0, 3 } },
            { 13, new[] { 0, 2 } } 
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
                chassisBaseMod -= SkillBasedInit.Settings.VehicleROCModifier;
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
                chassisBaseMod -= SkillBasedInit.Settings.TurretROCModifier;
            }

            // Determine if the mech has any special tags on it that grant bonus init
            // TODO: This must be compared against the 'base' value to see what the modifier is
            if (actor.StatCollection != null && actor.StatCollection.ContainsStatistic("BaseInitiative")) {
                int statCollectionVal = actor.StatCollection.GetValue<int>("BaseInitiative");
                int rawModifier = statCollectionVal;
                
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
                SkillBasedInit.LogDebug($"Normalized BaseInit from {statCollectionVal} to {rawModifier}");
                this.chassisBaseMod = rawModifier;
            } else {
                this.chassisBaseMod = 0;
            }

            // Determine engine size
            //foreach (MechComponent mc in actor.allComponents) {
            //    Mech mech = (Mech)actor;
            //    mech.MechDef.Inventory.Any(c => c.Def.GetComponent<EngineHeatSinkDef>());
            //    SkillBasedInit.Logger.Log($"Actor:{actor.DisplayName} has component: {mc.Name} with def: {mc.componentDef.GetType()} / {mc.componentDef.ComponentType} / {mc.componentType}");
            //    if (mc.componentDef.GetType() == typeof(EngineCoreDef)) {
            //        SkillBasedInit.Logger.Log($"Actor:{actor.DisplayName} component: {mc.Name} is an EngineCoreDef!");
            //    }
            //}

            // Check morale status
            if (actor.HasHighMorale) {
                this.chassisBaseMod += SkillBasedInit.Settings.PilotSpiritsModifier;
            } else if (actor.HasLowMorale) {
                this.chassisBaseMod -= SkillBasedInit.Settings.PilotSpiritsModifier;
            }

            Pilot pilot = actor.GetPilot();
            this.pilotId = pilot.GUID;
            this.pilotName = pilot.Name;

            // Normalize skills so that values above 10 don't screw the system
            int pilotingNormd = NormalizeSkill(pilot.Piloting);
            int tacticsNormd = NormalizeSkill(pilot.Tactics);
            int gutsNormd = NormalizeSkill(pilot.Guts);
            SkillBasedInit.LogDebug($"Skill profile is - p:{pilot.Piloting}->{pilotingNormd} t:{pilot.Tactics}->{tacticsNormd} g:{pilot.Guts}->{gutsNormd}");

            // Set the round init modifier based upon the normalized tactics and piloting skill
            double skillsInitModifier = Math.Ceiling((tacticsNormd * 2.0 + pilotingNormd) / 3.0) / 10.0;
            this.roundInitBounds = RoundInitFromTonnage(this.tonnage, skillsInitModifier);

            this.pilotingEffectMulti = 1.0 - (pilotingNormd * SkillBasedInit.Settings.PilotingMultiplier);
            this.gutsEffectMulti = 1.0 - (gutsNormd * SkillBasedInit.Settings.GutsMultiplier);

            this.injuryBounds = GutsInjuryBounds[gutsNormd];
            this.ignoredInjuries = pilot.BonusHealth;

            SkillBasedInit.Logger.Log($"Pilot {pilot.GUID} with name: {pilot.Name} has " +
                $"skillsMod:{skillsInitModifier} (from piloting:{pilotingNormd} tactics:{tacticsNormd}) " +
                $"injuryBounds: {injuryBounds[0]}-{injuryBounds[1]} ignoredInjuries:{ignoredInjuries} " +
                $"roundInitBounds:{roundInitBounds[0]}-{roundInitBounds[1]} chassisBaseMod: {chassisBaseMod} " +
                $"pilotingEffectMulti:{pilotingEffectMulti} gutsEffectMulti:{gutsEffectMulti}");
        }

        private int NormalizeSkill(int rawValue) {
            int normalizedVal = rawValue;
            if (rawValue > 10 && rawValue < 14) {
                // 11, 12, 13, 14 normalizes to 11
                normalizedVal = 11;
            } else if (rawValue > 14 && rawValue < 19) {
                // 15, 16, 17, 18 normalizes to 14
                normalizedVal = 12;
            } else if (rawValue > 18 && rawValue < 21) {
                // 19, 20 normalizes to 13
                normalizedVal = 13;
            } else if (rawValue < 0) {
                normalizedVal = 1;
            } else if (rawValue > 20) {
                normalizedVal = 13;
            }
            return normalizedVal;
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
            SkillBasedInit.LogDebug($"For skillMod:{skillInitModifier} + baseMod:{initBaseMulti} yielded bounds: {roundMin} - {roundMax}");
            return new int[] { roundMin, roundMax };
        }

        public void AddMeleeImpact(float impactDelta) {
            // TODO: Should only the highest apply? This could make the messaging confusing.
            this.meleeImpact += impactDelta;
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
                int crippledLoss = (int)Math.Ceiling(SkillBasedInit.Settings.MovementCrippledMalus * pilotingEffectMulti);
                SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} has crippled movement! Reduced {roundInitiative} by {crippledLoss}");
                roundInitiative -= crippledLoss;
            }

            // Check for melee impacts        
            if (this.meleeImpact > 0) {
                int delay = (int)Math.Ceiling(this.meleeImpact * pilotingEffectMulti);
                SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} was meleed after activation! Impact of {this.meleeImpact} was reduced to {delay} by piloting. Reduced {roundInitiative} by {delay}");
                roundInitiative -= delay;
            }

            // Check for knockdown / prone / shutdown
            if (actor.IsProne || actor.IsShutDown) {
                int delay = (int)Math.Ceiling(SkillBasedInit.Settings.ProneOrShutdownMalus * pilotingEffectMulti);
                SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} is prone or shutdown! Reduced {roundInitiative} by {delay}");
                roundInitiative -= delay;
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
                SkillBasedInit.LogDebug($"Round init {roundInitiative} less than 0.");
            } else if (roundInitiative > 30) {
                roundInitiative = 30;
                SkillBasedInit.LogDebug($"Round init {roundInitiative} greater than 30.");
            }

            // Init is flipped... 1 acts in first phase, then 2, etc.
            actor.Initiative = 31 - roundInitiative;
            SkillBasedInit.Logger.Log($"Actor {actor.DisplayName} ends up with roundInitiative {roundInitiative} from bounds {roundInitBounds[0]}-{roundInitBounds[1]}");
        }

        public static float CalculateMeleeDelta(float targetTonnage, Weapon weapon) {
            float delta = 0.0f;

            int targetTonnageMod = (int)Math.Floor(targetTonnage / 5.0);

            float attackerTonnage = 0.0f;
            if (weapon.parent.GetType() == typeof(Mech)) {
                Mech parent = (Mech)weapon.parent;
                attackerTonnage = parent.tonnage;
            } else if (weapon.parent.GetType() == typeof(Vehicle)) {
                Vehicle parent = (Vehicle)weapon.parent;
                attackerTonnage = parent.tonnage;
            }
            int attackerTonnageMod = (int)Math.Ceiling(attackerTonnage / 5.0);
            SkillBasedInit.LogDebug($"Raw attackerTonnageMod:{attackerTonnageMod} vs targetTonnageMod:{targetTonnageMod}");

            // Check for juggernaut
            foreach (Ability ability in weapon.parent.GetPilot().Abilities) {
                if (ability.Def.Id == "AbilityDefGu5") {
                    attackerTonnageMod = (int)Math.Ceiling(attackerTonnageMod * SkillBasedInit.Settings.MeleeAttackerJuggerMulti);
                    SkillBasedInit.LogDebug($"Pilot {weapon.parent.GetPilot()} has the Juggernaught skill, increasing their impact to {attackerTonnageMod}!");
                }
            }

            delta = Math.Max(1, attackerTonnageMod - targetTonnageMod);

            return delta;
        }

        public void ResolveMeleeImpact(AbstractActor actor, float delta) {
            
            double modifiedDelta = Math.Max(1, Math.Floor(delta * this.gutsEffectMulti));
            SkillBasedInit.LogDebug($"Melee impact of {delta} reduced to {modifiedDelta} thanks to gutsModifier: {this.gutsEffectMulti}");
            
            if (actor.HasActivatedThisRound) {
                SkillBasedInit.Logger.Log($"Melee impact will slow actor:{actor.DisplayName} by {modifiedDelta} init on next activation!");                
                this.AddMeleeImpact(delta);
            } else {
                SkillBasedInit.Logger.Log($"Melee impact immediately slows actor:{actor.DisplayName} by {modifiedDelta} init!");
                // TODO: Should this add to the PhaseModifier stat?
                if (actor.Initiative + (int)Math.Ceiling(delta) > SkillBasedInit.MaxPhase) {
                    actor.Initiative = SkillBasedInit.MaxPhase;
                } else {
                    actor.Initiative = actor.Initiative + (int)Math.Ceiling(delta);
                }
            }
            actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"CLANG!", FloatieMessage.MessageNature.Debuff));
            actor.Combat.MessageCenter.PublishMessage(new FloatieMessage(actor.GUID, actor.GUID, $"-{modifiedDelta} INITIATIVE", FloatieMessage.MessageNature.Debuff));
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
            ActorInitMap[actor.GUID] = actorInit;
        }

    }
}
