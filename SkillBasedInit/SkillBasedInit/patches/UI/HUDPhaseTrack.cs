using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;

namespace SkillBasedInit {

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "RefreshPhaseColors")]
    public static class CombatHUDPhaseTrack_RefreshPhaseColors {
        public static bool Prefix(CombatHUDPhaseTrack __instance, bool isPlayer, Hostility hostility, int ___currentPhase, CombatHUDPhaseBar[] ___phaseBars) {
            Mod.Log.Debug("CHUDPT::RPC - entered.");

            if (__instance == null || ___phaseBars == null) { return true; }
            if (!ModState.Combat.TurnDirector.IsInterleaved) { return true; }

            // TODO: FIX HARDCODED VALUE
            // Reconcile phase (from 1 - X) with display (X to 1)
            int initNum = (Mod.MaxPhase + 1) - ___currentPhase;
            int[] phaseBounds = PhaseHelper.CalcPhaseIconBounds(___currentPhase);
            Mod.Log.Debug($" For currentPhase: {___currentPhase}  phaseBounds are: [ {phaseBounds[0]} {phaseBounds[1]} {phaseBounds[2]} {phaseBounds[3]} {phaseBounds[4]} ]");

            for (int i = 0; i < 5; i++) {
                if (phaseBounds[i] > initNum) {
                    Mod.Log.Debug($" Setting phase: {phaseBounds[i]} as past phase.");
                    ___phaseBars[i].IndicatePastPhase();
                } else if (phaseBounds[i] == initNum) {
                    Mod.Log.Debug($" Setting phase: {phaseBounds[i]} as current phase.");
                    ___phaseBars[i].IndicateCurrentPhase(isPlayer, hostility);
                } else {
                    Mod.Log.Debug($" Setting phase: {phaseBounds[i]} as future phase.");
                    ___phaseBars[i].IndicateFuturePhase(isPlayer, hostility);
                }
                ___phaseBars[i].Text.SetText($"{phaseBounds[i]}");

            }

            if (phaseBounds[0] != Mod.MaxPhase) {
                ___phaseBars[0].Text.SetText("P");
            }
            if (phaseBounds[4] != Mod.MinPhase) {
                ___phaseBars[4].Text.SetText("F");
            }
                        
            return false;
        }
    }


    // ========== CombatHUDPhaseTrack ==========
    //[HarmonyPatch(typeof(CombatHUDPhaseTrack), "Init")]
    //[HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    //public static class CombatHUDPhaseTrack_Init {

    //    public static void Postfix(CombatHUDPhaseTrack __instance, CombatGameState combat, CombatHUD HUD,
    //        List<CombatHUDPhaseIcons> ___PhaseIcons, CombatHUDReserveButton ___reserveButton) {

    //        Mod.Log.Info($"CHUDPT:I:post - Init");

    //        __instance.OnCombatGameDestroyed();

    //        combat.MessageCenter.Subscribe(MessageCenterMessageType.OnRoundBegin, new ReceiveMessageCenterMessage(__instance.OnRoundBegin), true);
    //        combat.MessageCenter.Subscribe(MessageCenterMessageType.OnPhaseBegin, new ReceiveMessageCenterMessage(__instance.OnPhaseBegin), true);

    //        ___reserveButton.DisableButton();

    //        __instance.Visible = false;
    //    }
    //}

    //[HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnRoundBegin")]
    //[HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    //public static class CombatHUDPhaseTrack_OnRoundBegin {
    //    public static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message,
    //        TextMeshProUGUI ___roundCounterText, List<CombatHUDIconTracker> ___IconTrackers,
    //        GameObject ___phaseTrack, GameObject[] ___phaseBarHolders) {

    //        RoundBeginMessage roundBeginMessage = message as RoundBeginMessage;
    //        Mod.Log.Info($"CHUDPT:ORB:post - init for round: {roundBeginMessage.Round}");
    //        for (int i = 0; i < ___IconTrackers.Count; i++) {
    //            ___IconTrackers[i].Visible = false;
    //        }
    //        ___phaseTrack.SetActive(false);

    //        for (int i = 0; i < ___phaseBarHolders.Length; i++) {
    //            ___phaseBarHolders[i].SetActive(false);
    //        }
    //    }
    //}

    //[HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnPhaseBegin")]
    //[HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    //public static class CombatHUDPhaseTrack_OnPhaseBegin {
    //    public static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message, TextMeshProUGUI ___roundCounterText,
    //        CombatHUDPhaseBar[] ___phaseBars, int ___currentPhase) {

    //        //Mod.Log.Info($"CHUDPT:OPB:post - init for phase: {___currentPhase}");
    //        //PhaseBeginMessage phaseBeginMessage = message as PhaseBeginMessage;
    //        //string phaseText = string.Format("{0} - Phase {1}", phaseBeginMessage.Round, 31 - phaseBeginMessage.Phase);
    //        //___roundCounterText.SetText(phaseText);

    //        //CombatHUDPhaseTrack_SetPhaseTexts.UpdatePhaseTexts(___phaseBars, ___currentPhase);
    //    }
    //}

    //[HarmonyPatch(typeof(CombatHUDPhaseTrack), "SetPhaseTexts")]
    //public static class CombatHUDPhaseTrack_SetPhaseTexts {
    //    public static bool Prefix(CombatHUDPhaseTrack __instance, CombatHUDPhaseBar[] ___phaseBars, int ___currentPhase) {
    //        Mod.Log.Debug($"CHUDPT:SPT - entered for currentPhase: {___currentPhase}");
    //        UpdatePhaseTexts(___phaseBars, ___currentPhase);
    //        return false;
    //    }

    //    public static void UpdatePhaseTexts(CombatHUDPhaseBar[] phaseBars, int currentPhase) {

    //        int[] bounds = PhaseHelper.CalcPhaseIconBounds(currentPhase);
    //        Mod.Log.Info($"CHUDPT:STP - updating phase texts to be {bounds[0]}-{bounds[1]}.");
    //        for (int i = 0; i < 5; i++) {
    //            phaseBars[i].Text.SetText(AbstractActor.InitiativeToString(bounds[0] - i), Array.Empty<object>());
    //        }
    //    }
    //}

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "SetTrackerPhase")]
    [HarmonyPatch(new Type[] { typeof(CombatHUDIconTracker), typeof(int) })]
    public static class CombatHUDPhaseTrack_SetTrackerPhase {
        public static bool Prefix(CombatHUDPhaseTrack __instance, CombatHUDIconTracker tracker, int phase, int ___currentPhase, List<CombatHUDPhaseIcons> ___PhaseIcons) {
            Mod.Log.Trace($"CHUDPT:STP - entered at phase: {phase}.");

            int[] bounds = PhaseHelper.CalcPhaseIconBounds(___currentPhase);
            int phaseAsInit = (Mod.MaxPhase + 1) - phase;
            Mod.Log.Info($"Phase {phase} is init {phaseAsInit} within currentPhase: {___currentPhase} with bounds: {bounds[0]}-{bounds[4]}");

            if (phaseAsInit > bounds[1]) {
                Mod.Log.Info($"  -- Phase icon is higher than {bounds[1]}, setting to P phase.");
                ___PhaseIcons[0].AddIconTrackerToPhase(tracker);
            } else if (phaseAsInit < bounds[3]) {
                Mod.Log.Info($"  -- Phase icon is higher than {bounds[3]}, setting to F phase.");
                ___PhaseIcons[4].AddIconTrackerToPhase(tracker);
            } else {
                for (int i = 0; i < 5; i++) {
                    if (bounds[i] == phaseAsInit) {
                        Mod.Log.Info($"  -- Setting phase icon for phaseAsInit: {phaseAsInit} / bounds: {bounds[i]} at index {i}");
                        ___PhaseIcons[i].AddIconTrackerToPhase(tracker);
                    }
                }
            }

            return false;
        }
    }

    //[HarmonyPatch(typeof(CombatHUDIconTracker), "RefreshColor")]
    //public static class CombatHUDIconTracker_RefreshColor_Prefix {
    //    public static bool Prefix(CombatHUDIconTracker __instance) {
    //        Mod.Log.Debug("CHUDIT:RC entered.");

    //        return false;
    //    }
    //}

}
