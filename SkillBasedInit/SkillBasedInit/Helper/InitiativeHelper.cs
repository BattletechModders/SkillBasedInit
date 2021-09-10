using BattleTech;
using CustomUnits;
using IRBTModUtils.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillBasedInit.Helper
{
    public static class InitiativeHelper
    {

        public static void UpdateInitiative(AbstractActor actor)
        {

            Mod.Log.Info?.Write($"Updating initiative for actor: {actor.DistinctId()}");
            // If the actor is dead, skip them
            if (actor.IsDead || actor.IsFlaggedForDeath)
            {
                actor.Initiative = Mod.MaxPhase;
                Mod.Log.Info?.Write($"  actor is dead, setting init to MaxPhase: {Mod.MaxPhase}");
                actor.StatCollection.Set<int>(ModStats.ROUND_INIT, Mod.MaxPhase);
                return;
            }

            UnitCfg unitConfig = UnitHelper.GetUnitConfig(actor);

            // Set the base init by tonnage
            int roundInitiative = actor.StatCollection.GetValue<int>(ModStats.STATE_TONNAGE);
            roundInitiative += actor.StatCollection.GetValue<int>(ModStats.STATE_UNIT_TYPE);
            roundInitiative += actor.StatCollection.GetValue<int>(ModStats.STATE_PILOT_TAGS);
            Mod.Log.Info?.Write(
                $"  tonnageBase: {UnitHelper.GetTonnageModifier(actor)}  " +
                $"unitType: {actor.StatCollection.GetValue<int>(ModStats.STATE_UNIT_TYPE)}  " +
                $"unitType: {actor.StatCollection.GetValue<int>(ModStats.STATE_PILOT_TAGS)}"
                );


            // Check non-consumable modifiers - they apply without change
            roundInitiative += actor.StatCollection.GetValue<int>(ModStats.MOD_INJURY);
            roundInitiative += actor.StatCollection.GetValue<int>(ModStats.MOD_MISC);
            Mod.Log.Info?.Write(
                $"  injuryMod: {actor.StatCollection.GetValue<int>(ModStats.MOD_INJURY)}  " +
                $"miscMod: {actor.StatCollection.GetValue<int>(ModStats.MOD_MISC)}"
                );

            // Check for consumable modifiers - these get reset to 0 when we recalculate 
            if (actor.StatCollection.GetValue<int>(ModStats.MOD_CALLED_SHOT) != 0)
            {
                // Actor was hit by a called shot sometime after its turn, apply the penalty
                roundInitiative = actor.StatCollection.GetValue<int>(ModStats.MOD_CALLED_SHOT);
                Mod.Log.Info?.Write($"  calledShotMod: {actor.StatCollection.GetValue<int>(ModStats.MOD_CALLED_SHOT)}");
                actor.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT, 0);
            }

            if (actor.StatCollection.GetValue<int>(ModStats.MOD_VIGILANCE) != 0)
            {
                roundInitiative = actor.StatCollection.GetValue<int>(ModStats.MOD_VIGILANCE);
                Mod.Log.Info?.Write($"  vigilanceMod: {actor.StatCollection.GetValue<int>(ModStats.MOD_VIGILANCE)}");
                actor.StatCollection.Set<int>(ModStats.MOD_VIGILANCE, 0);
            }

            if (actor.StatCollection.GetValue<int>(ModStats.MOD_HESITATION) != 0)
            {
                roundInitiative = actor.StatCollection.GetValue<int>(ModStats.MOD_HESITATION);
                Mod.Log.Info?.Write($"  hesitationMod: {actor.StatCollection.GetValue<int>(ModStats.MOD_HESITATION)}");
                actor.StatCollection.Set<int>(ModStats.MOD_HESITATION, 0);
            }

            Pilot pilot = actor.GetPilot();

            // Generate the random element
            roundInitiative += pilot.RandomnessModifier(unitConfig);
            roundInitiative += pilot.InspiredModifier(unitConfig);

            // Check for leg / side loss
            var isMovementCrippled = false;
            if (this.type == ActorType.Mech)
            {
                var mech = (Mech)actor;
                isMovementCrippled = mech.IsLocationDestroyed(ChassisLocations.LeftLeg) || mech.IsLocationDestroyed(ChassisLocations.RightLeg) ? true : false;
            }
            else if (this.type == ActorType.Vehicle)
            {
                // TODO: This is pretty unlikely; vehicles rarely get crippled before they are destroyed. Find another solution?
                var vehicle = (Vehicle)actor;
                isMovementCrippled = vehicle.IsLocationDestroyed(VehicleChassisLocations.Left) || vehicle.IsLocationDestroyed(VehicleChassisLocations.Right) ? true : false;
            }

            if (isMovementCrippled) //  mechs, vehicles
            {
                int rawMod = Mod.Config.CrippledMovementModifier + this.pilotingEffectMod;
                Mod.Log.Debug?.Write($"  Crippled Actor: {actor.DistinctId()} has rawMod:{rawMod} = ({Mod.Config.CrippledMovementModifier} - {this.pilotingEffectMod})");

                int penalty = Math.Min(-1, rawMod);
                roundInitiative += penalty;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} has crippled movement! Subtracted {penalty} = roundInit:{roundInitiative}");
            }

            // Check for prone -> mechs
            if (actor.IsProne)
            {
                int rawMod = Mod.Config.ProneModifier + this.pilotingEffectMod;
                Mod.Log.Debug?.Write($"  Prone Actor: {actor.DistinctId()} has rawMod:{rawMod} = ({Mod.Config.ProneModifier} - {this.pilotingEffectMod})");

                int penalty = Math.Min(0, rawMod);
                roundInitiative += penalty;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} is prone! Subtracted {penalty} = roundInit:{roundInitiative}");
            }

            // Check for shutdown - mechs, battlearmor
            if (actor is Mech mech)
            {
                UnitCustomInfo customInfo = mech.GetCustomInfo();
                if (customInfo != null && customInfo.ArmsCountedAsLegs)
                {
                    // Quad - check shutdown, prone, crippled movement
                }
                else if (customInfo != null && customInfo?.SquadInfo?.Troopers > 1)
                {
                    // BattleArmor - check shutdown
                }
                else if (customInfo != null && customInfo.Naval)
                {
                    // Naval asset
                }
                else if (customInfo != null && customInfo.FakeVehicle)
                {
                    // Fake vehicle
                }
                else
                {
                    // base game Mech - check shutdown, prone, crippled movement

                }
            }
            else if (actor is Vehicle vehicle)
            {

            }
            else if (actor is Turret turret)
            {
                // No special processing
            }

            roundInitiative += actor.ShutdownInitModifier();

            if (actor.IsShutDown)
            {
                int rawMod = Mod.Config.ShutdownModifier + this.pilotingEffectMod;
                Mod.Log.Debug?.Write($"  Shutdown Actor: {actor.DistinctId()} has rawMod:{rawMod} = ({Mod.Config.ShutdownModifier} - {this.pilotingEffectMod})");
                int penalty = Math.Min(0, rawMod);
                roundInitiative += penalty;
                Mod.Log.Info?.Write($"  Actor: {actor.DistinctId()} is shutdown! Subtracted {penalty} = roundInit:{roundInitiative}");
            }


            // Normalize values
            if (roundInitiative <= 0)
            {
                roundInitiative = Mod.MinPhase;
                Mod.Log.Debug?.Write($"  Round init {roundInitiative} less than 0, setting to 1.");
            }
            else if (roundInitiative > 30)
            {
                roundInitiative = Mod.MaxPhase;
                Mod.Log.Debug?.Write($"  Round init {roundInitiative} greater than 30, setting to 30.");
            }

            // Init is flipped... 1 acts in first phase, then 2, etc.
            if (!actor.Combat.TurnDirector.IsInterleaved)
            {
                actor.Initiative = actor.Combat.TurnDirector.NonInterleavedPhase;
            }
            else
            {
                actor.Initiative = (Mod.MaxPhase + 1) - roundInitiative;
            }

            Mod.Log.Info?.Write($"== Actor: {actor.DistinctId()} has init:({roundInitiative}) from base:{roundInitBase} - variance:{roundVariance} plus modifiers.");
        }


        // Calculate the left and right phase boundaries *as initiative* 
        //   Will calculate 
        // 30, 29, 28, 27, 26 (red 30)
        // 30, 29, 28, 27, 26 (red 29)
        // 30, 29, 28, 27, 26 (red 28)
        // 29, 28, 27, 26, 25 (red 27)
        // 28, 27, 26, 25, 24 (red 26)
        // ...
        //  7,  6,  5,  4,  3 (red 5)
        //  6,  5,  4,  3,  2 (red 4)
        //  5,  4,  3,  2,  1 (red 3)
        //  5,  4,  3,  2,  1 (red 2)
        //  5,  4,  3,  2,  1 (red 1)
        public static int[] CalcPhaseIconBounds(int currentPhase)
        {

            // Normalize phase to initiative values
            int currentInit = (Mod.MaxPhase + 1) - currentPhase;

            int midPoint = currentInit;
            if (midPoint + 2 > Mod.MaxPhase || midPoint + 1 > Mod.MaxPhase)
            {
                midPoint = Mod.MaxPhase - 2;
            }
            else if (midPoint - 2 < Mod.MinPhase || midPoint - 1 < Mod.MinPhase)
            {
                midPoint = Mod.MinPhase + 2;
            }

            int[] bounds = new int[] { midPoint + 2, midPoint + 1, midPoint, midPoint - 1, midPoint - 2 };
            //Mod.Log.Info?.Write($"For phase {currentPhase}, init bounds are: {bounds[0]} to {bounds[4]}");
            return bounds;
        }
    }
}
