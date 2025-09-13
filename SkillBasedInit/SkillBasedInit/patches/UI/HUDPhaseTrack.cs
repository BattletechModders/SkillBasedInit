using IRBTModUtils;
using SkillBasedInit.Helper;
using System;
using System.Collections.Generic;

namespace SkillBasedInit
{

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "RefreshPhaseColors")]
    public static class CombatHUDPhaseTrack_RefreshPhaseColors
    {
        public static void Prefix(ref bool __runOriginal, CombatHUDPhaseTrack __instance, bool isPlayer, Hostility hostility, int ___currentPhase, CombatHUDPhaseBar[] ___phaseBars)
        {
            if (!__runOriginal) return;

            Mod.Log.Debug?.Write("CHUDPT::RPC - entered.");

            if (__instance == null || ___phaseBars == null || !SharedState.Combat.TurnDirector.IsInterleaved) { return; }

            // TODO: FIX HARDCODED VALUE
            // Reconcile phase (from 1 - X) with display (X to 1)
            int initNum = (Mod.MaxPhase + 1) - ___currentPhase;
            int[] phaseBounds = InitiativeHelper.CalcPhaseIconBounds(___currentPhase);
            Mod.Log.Debug?.Write($" For currentPhase: {___currentPhase}  phaseBounds are: [ {phaseBounds[0]} {phaseBounds[1]} {phaseBounds[2]} {phaseBounds[3]} {phaseBounds[4]} ]");

            for (int i = 0; i < 5; i++)
            {
                if (phaseBounds[i] > initNum)
                {
                    Mod.Log.Debug?.Write($" Setting phase: {phaseBounds[i]} as past phase.");
                    ___phaseBars[i].IndicatePastPhase();
                }
                else if (phaseBounds[i] == initNum)
                {
                    Mod.Log.Debug?.Write($" Setting phase: {phaseBounds[i]} as current phase.");
                    ___phaseBars[i].IndicateCurrentPhase(isPlayer, hostility);
                }
                else
                {
                    Mod.Log.Debug?.Write($" Setting phase: {phaseBounds[i]} as future phase.");
                    ___phaseBars[i].IndicateFuturePhase(isPlayer, hostility);
                }
                ___phaseBars[i].Text.SetText($"{phaseBounds[i]}");

            }

            if (phaseBounds[0] != Mod.MaxPhase)
            {
                ___phaseBars[0].Text.SetText("P");
            }
            if (phaseBounds[4] != Mod.MinPhase)
            {
                ___phaseBars[4].Text.SetText("F");
            }

            __runOriginal = false;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "SetTrackerPhase")]
    [HarmonyPatch(new Type[] { typeof(CombatHUDIconTracker), typeof(int) })]
    public static class CombatHUDPhaseTrack_SetTrackerPhase
    {
        public static void Prefix(ref bool __runOriginal, CombatHUDPhaseTrack __instance, CombatHUDIconTracker tracker, int phase, int ___currentPhase, List<CombatHUDPhaseIcons> ___PhaseIcons)
        {
            if (!__runOriginal) return;

            Mod.Log.Trace?.Write($"CHUDPT:STP - entered at phase: {phase}.");

            if (__instance == null || tracker == null || ___PhaseIcons == null)
            {
                Mod.Log.Warn?.Write("Invalid state detect, __instance, tracker, or ___PhaseIcons were null. This should not happen!");
                return;
            }
            if (___PhaseIcons.Count < 5)
            {
                Mod.Log.Warn?.Write("___PhaseIcons has less than 5 selections, this should not happen!");
                return;
            }
            if (!SharedState.Combat.TurnDirector.IsInterleaved)
            {
                Mod.Log.Warn?.Write("Asked to SetTrackerPhase, but combat is not interleaved - this should not happen!");
                return;
            }

            // Incoming phase value is Actor.Initiative, need to convert it figure out what pahse is represents
            int phaseAsInit = (Mod.MaxPhase + 1) - phase;
            if (phaseAsInit > Mod.MaxPhase)
            {
                phaseAsInit = Mod.MaxPhase;
                Mod.Log.Info?.Write($"Invalid phase {phase} supplied, greater than MaxPhase: {Mod.MaxPhase}. Normalizing.");
            }
            else if (phaseAsInit < Mod.MinPhase)
            {
                phaseAsInit = Mod.MinPhase;
                Mod.Log.Info?.Write($"Invalid phase {phase} supplied, less than MinPhase: {Mod.MinPhase}. Normalizing.");
            }
            
            int[] bounds = InitiativeHelper.CalcPhaseIconBounds(___currentPhase);
            if (bounds == null || bounds.Length < 5)
            {
                Mod.Log.Warn?.Write($"Calculated bounds are null or less than 5 for phase {phase}, this should not happen!");
                return;
            }
            Mod.Log.Trace?.Write($"Phase {phase} is init {phaseAsInit} within currentPhase: {___currentPhase} with bounds: {bounds[0]}-{bounds[4]}");
            
            if (phaseAsInit > bounds[1])
            {
                Mod.Log.Trace?.Write($"  -- Phase icon is higher than {bounds[1]}, setting to P phase.");
                ___PhaseIcons[0].AddIconTrackerToPhase(tracker);
            }
            else if (phaseAsInit < bounds[3])
            {
                Mod.Log.Trace?.Write($"  -- Phase icon is higher than {bounds[3]}, setting to F phase.");
                ___PhaseIcons[4].AddIconTrackerToPhase(tracker);
            }
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    if (bounds[i] == phaseAsInit)
                    {
                        Mod.Log.Trace?.Write($"  -- Setting phase icon for phaseAsInit: {phaseAsInit} / bounds: {bounds[i]} at index {i}");
                        ___PhaseIcons[i].AddIconTrackerToPhase(tracker);
                    }
                }
            }

            __runOriginal = false;
        }
    }


}
