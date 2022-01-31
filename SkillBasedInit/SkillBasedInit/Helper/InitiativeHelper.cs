using BattleTech;
using IRBTModUtils.Extension;

namespace SkillBasedInit.Helper
{
    public static class InitiativeHelper
    {

        // Init and phases are flipped. ForceUnitOnePhaseDown is initiative++, ForceUnitOnePhaseUp is initiative--.
        // Init of 1 = phase 30, 6 = 24, 10 = 21, 30 = 1
        public static int PhaseToInitiative(int phase)
        {
            int initiative = (Mod.MaxPhase + 1) - phase;
            return initiative;
        }

        // Init and phases are flipped. ForceUnitOnePhaseDown is initiative++, ForceUnitOnePhaseUp is initiative--.
        // Init of 1 = phase 30, 6 = 24, 10 = 21, 30 = 1
        public static int InitiativeToPhase(int initiative)
        {
            int phase = (Mod.MaxPhase + 1) - initiative;
            return phase;
        }

        public static void UpdateInitiative(AbstractActor actor)
        {

            Mod.Log.Info?.Write($"Updating initiative for actor: {actor.DistinctId()}");

            if (!actor.Combat.TurnDirector.IsInterleaved)
            {
                actor.Initiative = actor.Combat.TurnDirector.NonInterleavedPhase;
                Mod.Log.Info?.Write($"  combat is not interleaved, using phase: {actor.Initiative})");
                return;
            }

            // If the actor is dead, skip them
            if (actor.IsDead || actor.IsFlaggedForDeath)
            {
                actor.Initiative = Mod.MaxPhase;
                Mod.Log.Info?.Write($"  actor is dead, setting init to MaxPhase: {Mod.MaxPhase}");
                actor.StatCollection.Set<int>(ModStats.ROUND_INIT, Mod.MaxPhase);
                return;
            }

            UnitCfg unitConfig = actor.GetUnitConfig();

            // Set the base init by tonnage
            int tonnageMod = actor.StatCollection.GetValue<int>(ModStats.STATE_TONNAGE);
            int roundInitiative = tonnageMod;

            int typeMod = actor.StatCollection.GetValue<int>(ModStats.STATE_UNIT_TYPE);
            roundInitiative += typeMod;

            int pilotTagsMod = actor.StatCollection.GetValue<int>(ModStats.STATE_PILOT_TAGS);
            roundInitiative += pilotTagsMod;
            Mod.Log.Info?.Write(
                $"roundInit: {roundInitiative} <=" +
                $"  tonnageBase: {tonnageMod}" +
                $"  unitType: {typeMod}" +
                $"  pilotTags: {pilotTagsMod}"
                );


            // Check non-consumable modifiers - they apply without change

            // Injury mod is a phase mod, so invert it
            int injuryMod = actor.StatCollection.GetValue<int>(ModStats.MOD_INJURY) * -1;
            roundInitiative += injuryMod;

            // Misc mod is a phase mod, so invert it.
            int miscMod = actor.StatCollection.GetValue<int>(ModStats.MOD_MISC) * -1;
            roundInitiative += miscMod;
            Mod.Log.Info?.Write(
                $"roundInit: {roundInitiative} <=" +
                $"  injuryMod: {injuryMod}" +
                $"  miscMod: {miscMod}"
                );

            // Check for consumable modifiers - these get reset to 0 when we recalculate 
            int calledShotMod = actor.StatCollection.GetValue<int>(ModStats.STATE_CALLED_SHOT);
            if (calledShotMod != 0)
            {
                // Actor was hit by a called shot sometime after its turn, apply the penalty
                roundInitiative += calledShotMod;
                Mod.Log.Info?.Write($"roundInit: {roundInitiative} <=  calledShotState: {calledShotMod}");
                actor.StatCollection.Set<int>(ModStats.STATE_CALLED_SHOT, 0);
            }

            int vigilanceMod = actor.StatCollection.GetValue<int>(ModStats.STATE_VIGILIANCE);
            if (vigilanceMod != 0)
            {
                roundInitiative += vigilanceMod;
                Mod.Log.Info?.Write($"roundInit: {roundInitiative} <=  vigilanceState: {vigilanceMod}");
                actor.StatCollection.Set<int>(ModStats.STATE_VIGILIANCE, 0);
            }

            int proneMod = actor.ProneInitModifier();
            roundInitiative += proneMod;
            int crippledMod = actor.CrippledInitModifier();
            roundInitiative += crippledMod;
            int shutdownMod = actor.ShutdownInitMod();
            roundInitiative += shutdownMod;
            Mod.Log.Info?.Write($"roundInit: {roundInitiative} <=  proneInitModifier: {proneMod}  " +
                $"crippledInitModifier: {crippledMod}  " +
                $"shutdownInitModifier: {shutdownMod}");

            Pilot pilot = actor.GetPilot();

            // Tactics modifier are positive, so reduce init
            int tacticsMod = pilot.SBITacticsMod();
            roundInitiative -= tacticsMod;

            // Inspired mods are positive phases, so reduce init 
            int inspiredMod = pilot.InspiredModifier(unitConfig);
            roundInitiative -= inspiredMod;

            // Generate the random element
            int randomnessMod = pilot.RandomnessModifier(unitConfig);
            roundInitiative += randomnessMod;

            Mod.Log.Info?.Write($"roundInit: {roundInitiative} <=  tacticsMod: {tacticsMod}  inspiredMod: {inspiredMod} randomnessMod: {randomnessMod}");

            if (actor.StatCollection.GetValue<int>(ModStats.STATE_HESITATION) != 0)
            {
                int reducedHesitation = actor.StatCollection.GetValue<int>(ModStats.STATE_HESITATION) + tacticsMod;
                Mod.Log.Info?.Write($"  hesitationMod: {actor.StatCollection.GetValue<int>(ModStats.STATE_HESITATION)} - tacticsMod: {tacticsMod} => {reducedHesitation}");
                if (reducedHesitation > 0) reducedHesitation = 0;
                roundInitiative += reducedHesitation;
                actor.StatCollection.Set<int>(ModStats.STATE_HESITATION, reducedHesitation);
            }

            // Normalize values
            if (roundInitiative <= 0)
            {
                roundInitiative = Mod.MinPhase;
                Mod.Log.Info?.Write($"  Round init {roundInitiative} less than 0, setting to 1.");
            }
            else if (roundInitiative > 30)
            {
                roundInitiative = Mod.MaxPhase;
                Mod.Log.Info?.Write($"  Round init {roundInitiative} greater than 30, setting to 30.");
            }

            actor.Initiative = roundInitiative;
            int newPhase = InitiativeHelper.InitiativeToPhase(roundInitiative);
            Mod.Log.Info?.Write($"  actor.Initiative: {actor.Initiative} => phase: {newPhase}");
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
