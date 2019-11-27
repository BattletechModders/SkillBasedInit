using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillBasedInit {

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

    //        //UpdatePhaseTexts(___phaseBars, ___currentPhase);
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
        public static bool Prefix(CombatHUDPhaseTrack __instance, CombatHUDIconTracker tracker, int phase, List<CombatHUDPhaseIcons> ___PhaseIcons) {
            Mod.Log.Info($"CHUDPT:STP - entered at phase: {phase}.");
            //int[] bounds = PhaseHelper.CalcPhaseIconBounds(phase);
            //int phaseAsInit = (Mod.MaxPhase + 1) - phase;
            //Mod.Log.Info($"Phase {phase} is init {phaseAsInit} with bounds: {bounds[0]}-{bounds[1]}");

            //if (phaseAsInit > bounds[0] || phaseAsInit < bounds[1]) {
            //    Mod.Log.Info($" Phase outside of current bounds, skipping.");
            //} else {
            //    int iconIdx = bounds[0] - phaseAsInit;
            //    Mod.Log.Info($"Phase inside bounds, icon index is: {iconIdx}");
            //    ___PhaseIcons[iconIdx].AddIconTrackerToPhase(tracker);
            //}

            //tracker.CurrentlyDisplayedPhase = phase;
            Mod.Log.Info($"CHUDPT:STP - exiting.");
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
