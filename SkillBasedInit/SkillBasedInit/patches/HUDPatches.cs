using BattleTech;
using BattleTech.UI;
using DG.Tweening;
using Harmony;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillBasedInit {

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    public static class CombatHUDPhaseTrack_Init
    {

        public static void Postfix(CombatHUDPhaseTrack __instance, CombatGameState combat, CombatHUD HUD, List<CombatHUDPhaseIcons> ___PhaseIcons, CombatHUDReserveButton ___reserveButton)
        {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack::Init - entered");

            __instance.OnCombatGameDestroyed();

            combat.MessageCenter.Subscribe(MessageCenterMessageType.OnRoundBegin, new ReceiveMessageCenterMessage(__instance.OnRoundBegin), true);
            combat.MessageCenter.Subscribe(MessageCenterMessageType.OnPhaseBegin, new ReceiveMessageCenterMessage(__instance.OnPhaseBegin), true);

            ___reserveButton.DisableButton();
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnRoundBegin")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class CombatHUDPhaseTrack_OnRoundBegin
    {
        public static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message,
            TextMeshProUGUI ___roundCounterText, List<CombatHUDIconTracker> ___IconTrackers,
            GameObject ___phaseTrack, GameObject[] ___phaseBarHolders)
        {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack::OnRoundBegin::post - Init");
            for (int i = 0; i < ___IconTrackers.Count; i++)
            {
                ___IconTrackers[i].Visible = false;
            }
            ___phaseTrack.SetActive(false);

            for (int i = 0; i < ___phaseBarHolders.Length; i++)
            {
                ___phaseBarHolders[i].SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnPhaseBegin")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class CombatHUDPhaseTrack_OnPhaseBegin
    {
        public static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message, TextMeshProUGUI ___roundCounterText)
        {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack::OnPhaseBegin::post - Init");
            PhaseBeginMessage phaseBeginMessage = message as PhaseBeginMessage;
            string phaseText = string.Format("{0} - Phase {1}", phaseBeginMessage.Round, 31 - phaseBeginMessage.Phase);
            ___roundCounterText.SetText(phaseText, new object[0]);
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack::OnPhaseBegin::post - for {phaseText}");
        }
    }

    // Prevents the init tracker line at the top from loading
    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "SetTrackerPhase")]
    [HarmonyPatch(new Type[] { typeof(CombatHUDIconTracker), typeof(int) })]
    public static class CombatHUDPhaseTrack_SetTrackerPhase
    {
        public static bool Prefix(CombatHUDPhaseTrack __instance, CombatHUDIconTracker tracker, int phase)
        {
            return false;
        }
    }

    // Corrects the init overlay displayed on the Mechwarrior
    [HarmonyPatch(typeof(CombatHUDPortrait), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD), typeof(UnityEngine.UI.LayoutElement), typeof(HBSDOTweenToggle) })]
    public static class CombatHUDPortrait_Init
    {
        public static void Postfix(CombatHUDPortrait __instance, ref TextMeshProUGUI ___ioText, ref DOTweenAnimation ___initiativeOverlay)
        {
            //SkillBasedInit.Logger.Log($"CombatHUDPortrait::Init::post - Init");
            ___ioText.enableWordWrapping = false;
            ___initiativeOverlay.isActive = false;
        }
    }

    // TODO: Dead code? 
    // Manipulates the icon to the upper left of the mech panel
    //[HarmonyPatch(typeof(CombatHUDPhaseDisplay), "Init")]
    //[HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    //public static class CombatHUDPhaseDisplay_Init {
    //    public static void Postfix(CombatHUDPhaseDisplay __instance, ref TextMeshProUGUI ___NumText) {
    //        //SkillBasedInit.Logger.Log($"CombatHUDPhaseDisplay::Init::post - Init");
    //        ___NumText.enableWordWrapping = false;
    //        ___NumText.fontSize = 18;
    //    }
    //}

    // Manipulates the badge icon to the left of the mech's floating damage/status bars
    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "RefreshInfo")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPhaseDisplay_RefreshInfo
    {
        public static void Postfix(CombatHUDPhaseDisplay __instance, ref TextMeshProUGUI ___NumText)
        {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseDisplay::RefreshInfo::post - Init");
            ___NumText.enableWordWrapping = false;
            ___NumText.fontSize = 18;
        }
    }

}
