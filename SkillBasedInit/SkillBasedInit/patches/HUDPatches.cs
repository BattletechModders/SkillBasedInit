﻿using BattleTech;
using BattleTech.UI;
using DG.Tweening;
using Harmony;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkillBasedInit {

    // ========== CombatHUDPhaseTrack ==========
    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    public static class CombatHUDPhaseTrack_Init {

        public static void Postfix(CombatHUDPhaseTrack __instance, CombatGameState combat, CombatHUD HUD, 
            List<CombatHUDPhaseIcons> ___PhaseIcons, CombatHUDReserveButton ___reserveButton) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack:Init - entered");

            __instance.OnCombatGameDestroyed();

            combat.MessageCenter.Subscribe(MessageCenterMessageType.OnRoundBegin, new ReceiveMessageCenterMessage(__instance.OnRoundBegin), true);
            combat.MessageCenter.Subscribe(MessageCenterMessageType.OnPhaseBegin, new ReceiveMessageCenterMessage(__instance.OnPhaseBegin), true);

            ___reserveButton.DisableButton();

            __instance.Visible = false;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnRoundBegin")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class CombatHUDPhaseTrack_OnRoundBegin {
        public static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message,
            TextMeshProUGUI ___roundCounterText, List<CombatHUDIconTracker> ___IconTrackers,
            GameObject ___phaseTrack, GameObject[] ___phaseBarHolders) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack:OnRoundBegin:post - Init");
            for (int i = 0; i < ___IconTrackers.Count; i++) {
                ___IconTrackers[i].Visible = false;
            }
            ___phaseTrack.SetActive(false);

            for (int i = 0; i < ___phaseBarHolders.Length; i++) {
                ___phaseBarHolders[i].SetActive(false);
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "OnPhaseBegin")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class CombatHUDPhaseTrack_OnPhaseBegin {
        public static void Postfix(CombatHUDPhaseTrack __instance, MessageCenterMessage message, TextMeshProUGUI ___roundCounterText) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack:OnPhaseBegin:post - Init");
            PhaseBeginMessage phaseBeginMessage = message as PhaseBeginMessage;
            string phaseText = string.Format("{0} - Phase {1}", phaseBeginMessage.Round, 31 - phaseBeginMessage.Phase);
            ___roundCounterText.SetText(phaseText, new object[0]);
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack:OnPhaseBegin:post - for {phaseText}");
        }
    }

    // Prevents the init tracker line at the top from loading
    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "SetTrackerPhase")]
    [HarmonyPatch(new Type[] { typeof(CombatHUDIconTracker), typeof(int) })]
    public static class CombatHUDPhaseTrack_SetTrackerPhase {
        public static bool Prefix(CombatHUDPhaseTrack __instance, CombatHUDIconTracker tracker, int phase) {
            return false;
        }
    }

    // === CombatHUDPortrait === : The mechwarrior picture in the bottom tray

    // Corrects the init overlay displayed on the Mechwarrior
    [HarmonyPatch(typeof(CombatHUDPortrait), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD), typeof(UnityEngine.UI.LayoutElement), typeof(HBSDOTweenToggle) })]
    public static class CombatHUDPortrait_Init {
        public static void Postfix(CombatHUDPortrait __instance, ref TextMeshProUGUI ___ioText, ref DOTweenAnimation ___initiativeOverlay) {
            //SkillBasedInit.Logger.Log($"CombatHUDPortrait:Init:post - Init");
            ___ioText.enableWordWrapping = false;
            ___initiativeOverlay.isActive = false;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPortrait), "IndicatePastPhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPortrait_IndicatePastPhase {
        public static void Postfix(CombatHUDPortrait __instance, TextMeshProUGUI ___ioText) {
            __instance.NumberFlagFill.color = Color.white;
            __instance.NumberFlagText.color = Color.white;
            __instance.NumberFlagOutline.color = Color.white;
            ___ioText.color = Color.white;

            Transform frameT = __instance.FilledHolder.transform.Find("mw_Frame");
            if (frameT != null) {
                GameObject frame = frameT.gameObject;
                Transform nameRectT = frameT.transform.Find("mw_NameRect");
                if (nameRectT != null) {
                    GameObject nameRect = nameRectT.gameObject;
                    Image nameImg = nameRect.GetComponent<Image>();
                    nameImg.color = Mod.Config.FriendlyAlreadyActivated;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDPortrait), "IndicateCurrentPhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPortrait_IndicateCurrentPhase {
        public static void Postfix(CombatHUDPortrait __instance, TextMeshProUGUI ___ioText) {
            __instance.NumberFlagFill.color = Color.white;
            __instance.NumberFlagOutline.color = Color.white;
            __instance.NumberFlagText.color = Color.white;
            ___ioText.color = Color.white;

            Transform frameT = __instance.FilledHolder.transform.Find("mw_Frame");
            if (frameT != null) {
                GameObject frame = frameT.gameObject;
                Transform nameRectT = frameT.transform.Find("mw_NameRect");
                if (nameRectT != null) {
                    GameObject nameRect = nameRectT.gameObject;
                    Image nameImg = nameRect.GetComponent<Image>();

                    if (__instance.DisplayedActor.HasActivatedThisRound) {
                        nameImg.color = Mod.Config.FriendlyAlreadyActivated;
                    } else {
                        nameImg.color = Mod.Config.FriendlyUnactivated;
                    }

                }
            }
        }
    }

    [HarmonyPatch(typeof(CombatHUDPortrait), "IndicateFuturePhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPortrait_IndicateFuturePhase {
        public static void Postfix(CombatHUDPortrait __instance, TextMeshProUGUI ___ioText) {
            __instance.NumberFlagFill.color = Color.white;
            __instance.NumberFlagOutline.color = Color.white;
            __instance.NumberFlagText.color = Color.white;
            ___ioText.color = Color.white;

            Transform frameT = __instance.FilledHolder.transform.Find("mw_Frame");
            if (frameT != null) {
                GameObject frame = frameT.gameObject;
                Transform nameRectT = frameT.transform.Find("mw_NameRect");
                if (nameRectT != null) {
                    GameObject nameRect = nameRectT.gameObject;
                    Image imgByCmp = nameRect.GetComponent<Image>();
                    imgByCmp.color = Mod.Config.FriendlyUnactivated;

                } 
            } 
        }
    }

    // === CombatHUDPhaseDisplay === : The floating badget next to a mech
    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "RefreshInfo")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPhaseDisplay_RefreshInfo {
        public static void Postfix(CombatHUDPhaseDisplay __instance, ref TextMeshProUGUI ___NumText) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseDisplay:RefreshInfo:post - Init");
            ___NumText.enableWordWrapping = false;
            ___NumText.fontSize = 18;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "IndicatePastPhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPhaseDisplay_IndicatePastPhase {
        public static void Postfix(CombatHUDPhaseDisplay __instance) {
            //SkillBasedInit.Logger.LogIfDebug($"CombatHUDPhaseDisplay:IndicatePastPhase:post - init");
            __instance.FlagOutline.color = Color.white;
            __instance.NumText.color = Color.white;

            Hostility hostility = __instance.Combat.HostilityMatrix.GetHostility(__instance.DisplayedActor.team, __instance.Combat.LocalPlayerTeam);
            bool isPlayer = __instance.DisplayedActor.team == __instance.Combat.LocalPlayerTeam;

            Color color = Mod.Config.FriendlyAlreadyActivated;
            if (hostility == Hostility.ENEMY) {
                color = Mod.Config.EnemyAlreadyActivated;
            } else {
                if (!isPlayer) {
                    switch (hostility) {
                        case Hostility.FRIENDLY:
                            color = Mod.Config.AlliedAlreadyActivated;
                            break;
                        case Hostility.NEUTRAL:
                            color = Mod.Config.NeutralAlreadyActivated;
                            break;
                    }
                }
            }
            __instance.FlagFillImage.color = color;

        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "IndicateCurrentPhase")]
    [HarmonyPatch(new Type[] { typeof(bool), typeof(Hostility) })]
    public static class CombatHUDPhaseDisplay_IndicateCurrentPhase {
        public static void Postfix(CombatHUDPhaseDisplay __instance, bool isPlayer, Hostility hostility) {
            //SkillBasedInit.Logger.LogIfDebug($"CombatHUDPhaseDisplay:IndicateCurrentPhase:post - init");
            __instance.FlagOutline.color = Color.white;
            __instance.NumText.color = Color.white;

            Color color = Mod.Config.FriendlyUnactivated;
            if (hostility == Hostility.ENEMY) {
                color = Mod.Config.EnemyUnactivated;
            } else {
                if (!isPlayer) {
                    switch (hostility) {
                        case Hostility.FRIENDLY:
                            color = Mod.Config.AlliedUnactivated;
                            break;
                        case Hostility.NEUTRAL:
                            color = Mod.Config.NeutralUnactivated;
                            break;
                    }
                }
            }
            __instance.FlagFillImage.color = color;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "IndicateFuturePhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPhaseDisplay_IndicateFuturePhase {
        public static void Postfix(CombatHUDPhaseDisplay __instance) {
            //SkillBasedInit.Logger.LogIfDebug($"CombatHUDPhaseDisplay:IndicateFuturePhase:post - init");

            __instance.FlagOutline.color = Color.white;
            __instance.NumText.color = Color.white;

            Hostility hostility = __instance.Combat.HostilityMatrix.GetHostility(__instance.DisplayedActor.team, __instance.Combat.LocalPlayerTeam);
            bool isPlayer = __instance.DisplayedActor.team == __instance.Combat.LocalPlayerTeam;

            Color color = Mod.Config.FriendlyUnactivated;
            if (hostility == Hostility.ENEMY) {
                color = Mod.Config.EnemyUnactivated;
            } else {
                if (!isPlayer) {
                    switch (hostility) {
                        case Hostility.FRIENDLY:
                            color = Mod.Config.AlliedUnactivated;
                            break;
                        case Hostility.NEUTRAL:
                            color = Mod.Config.NeutralUnactivated;
                            break;
                    }
                }
            }
            __instance.FlagFillImage.color = color;
        }
    }

}
