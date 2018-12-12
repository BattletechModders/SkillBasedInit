using BattleTech;
using BattleTech.UI;
using DG.Tweening;
using Harmony;
using HBS;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace SkillBasedInit {

    // ========== CombatHUDPhaseTrack ==========
    [HarmonyPatch(typeof(CombatHUDPhaseTrack), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    public static class CombatHUDPhaseTrack_Init {

        public static void Postfix(CombatHUDPhaseTrack __instance, 
            CombatGameState combat, CombatHUD HUD, 
            List<CombatHUDPhaseIcons> ___PhaseIcons, 
            CombatHUDReserveButton ___reserveButton) {
            //SkillBasedInit.Logger.Log($"CombatHUDPhaseTrack:Init - entered");

            __instance.OnCombatGameDestroyed();

            combat.MessageCenter.Subscribe(MessageCenterMessageType.OnRoundBegin, new ReceiveMessageCenterMessage(__instance.OnRoundBegin), true);
            combat.MessageCenter.Subscribe(MessageCenterMessageType.OnPhaseBegin, new ReceiveMessageCenterMessage(__instance.OnPhaseBegin), true);

            ___reserveButton.DisableButton();
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

    // ========== CombatHUDPortrait ==========

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
        public static void Postfix(CombatHUDPortrait __instance) {
            //SkillBasedInit.LogDebug($"CombatHUDPortrait:IndicatePastPhase:post - init");
            __instance.NumberFlagFill.color = Color.red;
            __instance.NumberFlagOutline.color = Color.red;
            __instance.NumberFlagText.color = Color.red;
            //__instance.Background.color = Color.red;
            //__instance.Portrait.color = Color.red;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPortrait), "IndicateCurrentPhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPortrait_IndicateCurrentPhase {
        public static void Postfix(CombatHUDPortrait __instance) {
            //SkillBasedInit.LogDebug($"CombatHUDPortrait:IndicateCurrentPhase:post - init");
            __instance.NumberFlagFill.color = Color.green;
            __instance.NumberFlagOutline.color = Color.green;
            __instance.NumberFlagText.color = Color.green;
            //__instance.Background.color = Color.green;
            //__instance.Portrait.color = Color.green;
        }
    }


    [HarmonyPatch(typeof(CombatHUDPortrait), "IndicateFuturePhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPortrait_IndicateFuturePhase {
        public static void Postfix(CombatHUDPortrait __instance) {
            //SkillBasedInit.LogDebug($"CombatHUDPortrait:IndicateFuturePhase:post - init");
            __instance.NumberFlagFill.color = Color.blue;
            __instance.NumberFlagOutline.color = Color.blue;
            __instance.NumberFlagText.color = Color.blue;
            //__instance.Background.color = Color.blue;
            //__instance.Portrait.color = Color.blue;

        }
    }

    /*
            public void OnActorHovered (MessageCenterMessage message) {
            if (displayedActor != null) {
                ActorHoveredMessage actorHoveredMessage = message as ActorHoveredMessage;
                if (actorHoveredMessage.affectedObjectGuid == displayedActor.GUID && !IsHovered) {
                    IsHovered = true;
                    Background.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ButtonBGHighlighted.color;
                }
            }
        }

        public void OnPointerExit (PointerEventData eventData)
        {
            if (displayedActor != null) {
                Combat.MessageCenter.PublishMessage (new ActorUnHoveredMessage (displayedActor.GUID));
            }
            MWStatusWindow.MouseHover = false;
        }

        public void OnActorUnHovered (MessageCenterMessage message)
        {
            if (displayedActor != null) {
                ActorUnHoveredMessage actorUnHoveredMessage = message as ActorUnHoveredMessage;
                if (actorUnHoveredMessage.affectedObjectGuid == displayedActor.GUID) {
                    IsHovered = false;
                    if (DisplayedActor.IsAvailableThisPhase) {
                        Background.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ButtonBGEnabled.color;
                    } else {
                        Background.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ButtonBGDisabled.color;
                    }
                }
                NotActiveText.color = Color.clear;
            }
        }
     */

    // ========== CombatHUDPhaseDisplay ==========

    // TODO: Dead code? 
    // Manipulates the icon to the upper left of the mech panel
    //[HarmonyPatch(typeof(CombatHUDPhaseDisplay), "Init")]
    //[HarmonyPatch(new Type[] { typeof(CombatGameState), typeof(CombatHUD) })]
    //public static class CombatHUDPhaseDisplay_Init {
    //    public static void Postfix(CombatHUDPhaseDisplay __instance, ref TextMeshProUGUI ___NumText) {
    //        //SkillBasedInit.Logger.Log($"CombatHUDPhaseDisplay:Init:post - Init");
    //        ___NumText.enableWordWrapping = false;
    //        ___NumText.fontSize = 18;
    //    }
    //}

    // Manipulates the badge icon to the left of the mech's floating damage/status bars
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
            //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicatePastPhase:post - init");
            __instance.FlagFillImage.color = Color.grey;
            __instance.FlagOutline.color = Color.grey;
            __instance.NumText.color = Color.white;
            /*
            FlagFillImage.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhasePastFill.color;
            FlagOutline.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhasePastOutline.color;
            NumText.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhasePastText.color;
            */
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "IndicateCurrentPhase")]
    [HarmonyPatch(new Type[] { typeof(bool), typeof(Hostility) })]
    public static class CombatHUDPhaseDisplay_IndicateCurrentPhase {
        public static void Postfix(CombatHUDPhaseDisplay __instance, bool isPlayer, Hostility hostility) {
            //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicateCurrentPhase:post - init");
            __instance.FlagFillImage.color = Color.green;
            __instance.FlagOutline.color = Color.white;
            __instance.NumText.color = Color.white;

            if (hostility == Hostility.ENEMY) {
                __instance.FlagFillImage.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.EnemyUI.color;
            } else {
                //Color color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseCurrentFill.color;
                Color color = Color.green;
                if (!isPlayer) {
                    switch (hostility) {
                    case Hostility.FRIENDLY:
                        color *= LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.AlliedUI.color;
                        break;
                    case Hostility.NEUTRAL:
                        color *= LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.NeutralUI.color;
                        break;
                    }
                }
                __instance.FlagFillImage.color = color;
            }

            /*
            FlagOutline.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseCurrentOutline.color;
            NumText.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseCurrentText.color;
            */           
        }
    }

    [HarmonyPatch(typeof(CombatHUDPhaseDisplay), "IndicateFuturePhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPhaseDisplay_IndicateFuturePhase {
        public static void Postfix(CombatHUDPhaseDisplay __instance) {
            //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicateFuturePhase:post - init");

            Hostility hostility = __instance.Combat.HostilityMatrix.GetHostility(__instance.DisplayedActor.team, __instance.Combat.LocalPlayerTeam);
            bool isPlayer = __instance.DisplayedActor.team == __instance.Combat.LocalPlayerTeam;        

            if (hostility == Hostility.ENEMY) {
                __instance.FlagFillImage.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.EnemyUI.color;
                //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicateFuturePhase:post - enemy color is: {__instance.FlagFillImage.color.ToString()}");
            } else {
                Color color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseCurrentFill.color;
                //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicateFuturePhase:post - color is: {color.ToString()}");
                Color color2 = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.AlliedUI.color;
                //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicateFuturePhase:post - allied color is: {color2.ToString()}");
                Color color3 = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.NeutralUI.color;
                //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicateFuturePhase:post - neutral color is: {color3.ToString()}");

                color = Color.green;

                if (!isPlayer) {
                    switch (hostility) {
                        case Hostility.FRIENDLY:
                            color *= LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.AlliedUI.color;
                            //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicateFuturePhase:post - allied color is: {color.ToString()}");
                            break;
                        case Hostility.NEUTRAL:
                            color *= LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.NeutralUI.color;
                            //SkillBasedInit.LogDebug($"CombatHUDPhaseDisplay:IndicateFuturePhase:post - neutral color is: {color.ToString()}");
                            break;
                    }
                }
                __instance.FlagFillImage.color = color;
            }
            //__instance.FlagFillImage.color = Color.blue;
            __instance.FlagOutline.color = Color.white;
            __instance.NumText.color = Color.white;

            /*
            FlagFillImage.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseFutureFill.color;
            FlagOutline.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseFutureOutline.color;
            NumText.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseFutureText.color;
            */
        }
    }

}
