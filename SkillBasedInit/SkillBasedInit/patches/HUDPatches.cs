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
        public static void Postfix(CombatHUDPortrait __instance, TextMeshProUGUI ___ioText) {
            //SkillBasedInit.LogDebug($"CombatHUDPortrait:IndicatePastPhase:post - init");
            //__instance.NumberFlagFill.color = Color.red;
            //__instance.NumberFlagOutline.color = Color.red;
            //__instance.NumberFlagText.color = Color.red;

            //__instance.Frame.color = Color.red;
            //__instance.PilotName.color = Color.red;
            //__instance.PilotIcon.color = Color.red;

            __instance.Frame.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
            __instance.Background.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
            __instance.NumberFlagFill.color = Color.white;
            __instance.NumberFlagText.color = Color.white;
            __instance.NumberFlagOutline.color = Color.white;

            ___ioText.color = Color.white;


            //__instance.Background.color = Color.red;
            //__instance.Portrait.color = Color.red;
        }
    }

    [HarmonyPatch(typeof(CombatHUDPortrait), "IndicateCurrentPhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPortrait_IndicateCurrentPhase {
        public static void Postfix(CombatHUDPortrait __instance, TextMeshProUGUI ___ioText) {
            //SkillBasedInit.LogDebug($"CombatHUDPortrait:IndicateCurrentPhase:post - init");
            //__instance.NumberFlagFill.color = Color.green;
            //__instance.NumberFlagOutline.color = Color.green;
            //__instance.NumberFlagText.color = Color.green;

            //__instance.Frame.color = Color.green;
            //__instance.PilotName.color = Color.green;
            //__instance.PilotIcon.color = Color.green;

            //__instance.Background.color = Color.green;
            //__instance.NotActiveText.color = Color.blue;
            //__instance.SelectedOutline.color = Color.red;

            if (__instance.DisplayedActor.HasActivatedThisRound) {
                __instance.Frame.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
                __instance.Background.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
            } else {
                __instance.Frame.color = SkillBasedInit.Settings.FriendlyUnactivated;
                __instance.Background.color = SkillBasedInit.Settings.FriendlyUnactivated;
            }
            __instance.NumberFlagFill.color = Color.white;
            __instance.NumberFlagOutline.color = Color.white;
            __instance.NumberFlagText.color = Color.white;

            ___ioText.color = Color.white;

            //__instance.Background.color = Color.green;
            //__instance.Portrait.color = Color.green;
        }
    }


    [HarmonyPatch(typeof(CombatHUDPortrait), "IndicateFuturePhase")]
    [HarmonyPatch(new Type[] { })]
    public static class CombatHUDPortrait_IndicateFuturePhase {
        public static void Postfix(CombatHUDPortrait __instance, TextMeshProUGUI ___ioText) {
            //SkillBasedInit.LogDebug($"CombatHUDPortrait:IndicateFuturePhase:post - init");
            //__instance.NumberFlagFill.color = Color.blue;
            //__instance.NumberFlagOutline.color = Color.blue;
            //__instance.NumberFlagText.color = Color.blue;

            //SkillBasedInit.Logger.Log($"CombatHUDPortrait:IndicateFuturePhase:post - looking at components");
            //foreach (GameObject component in __instance.FilledHolder.GetComponents<GameObject>()) {
            //    SkillBasedInit.Logger.Log($"CombatHUDPortrait:IndicateFuturePhase:post - Found component: {component.name}");
            //}

            __instance.Frame.color = SkillBasedInit.Settings.FriendlyUnactivated;
            __instance.Background.color = SkillBasedInit.Settings.FriendlyUnactivated;
            __instance.NumberFlagFill.color = Color.white;
            __instance.NumberFlagOutline.color = Color.white;
            __instance.NumberFlagText.color = Color.white;

            ___ioText.color = Color.white;

            //__instance.PilotName.color = Color.blue;
            //__instance.PilotIcon.color = Color.blue;

            //__instance.Background.color = Color.blue;
            //__instance.Portrait.color = Color.blue;

        }
    }

    [HarmonyPatch(typeof(CombatHUDPortrait), "OnActorHovered")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class CombatHUDPortrait_OnActorHovered {
        public static void Postfix(CombatHUDPortrait __instance) {
            if (__instance.DisplayedActor.HasActivatedThisRound) {
                __instance.Frame.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
                __instance.Background.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
            } else {
                __instance.Frame.color = SkillBasedInit.Settings.FriendlyUnactivated;
                __instance.Background.color = SkillBasedInit.Settings.FriendlyUnactivated;
            }

        }
    }

    [HarmonyPatch(typeof(CombatHUDPortrait), "OnActorUnHovered")]
    [HarmonyPatch(new Type[] { typeof(MessageCenterMessage) })]
    public static class CombatHUDPortrait_OnActorUnHovered {
        public static void Postfix(CombatHUDPortrait __instance) {
            if (__instance.DisplayedActor.HasActivatedThisRound) {
                __instance.Frame.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
                __instance.Background.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
            } else {
                __instance.Frame.color = SkillBasedInit.Settings.FriendlyUnactivated;
                __instance.Background.color = SkillBasedInit.Settings.FriendlyUnactivated;
            }
        }
    }


    /*
            public void OnActorHovered (MessageCenterMessage message)
        {
            if (displayedActor != null) {
                ActorHoveredMessage actorHoveredMessage = message as ActorHoveredMessage;
                if (actorHoveredMessage.affectedObjectGuid == displayedActor.GUID && !IsHovered) {
                    IsHovered = true;
                    Background.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.ButtonBGHighlighted.color;
                }
            }
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
    // Manipulates the badge icon to the left of the mech's floating damage/status bars

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
            __instance.FlagOutline.color = Color.white;
            __instance.NumText.color = Color.white;

            Hostility hostility = __instance.Combat.HostilityMatrix.GetHostility(__instance.DisplayedActor.team, __instance.Combat.LocalPlayerTeam);
            bool isPlayer = __instance.DisplayedActor.team == __instance.Combat.LocalPlayerTeam;

            Color color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
            if (hostility == Hostility.ENEMY) {
                color = SkillBasedInit.Settings.EnemyAlreadyActivated;
            } else {
                if (!isPlayer) {
                    switch (hostility) {
                        case Hostility.FRIENDLY:
                            color = SkillBasedInit.Settings.AlliedAlreadyActivated;
                            break;
                        case Hostility.NEUTRAL:
                            color = SkillBasedInit.Settings.NeutralAlreadyActivated;
                            break;
                    }
                }
            }
            __instance.FlagFillImage.color = color;

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
            __instance.FlagOutline.color = Color.white;
            __instance.NumText.color = Color.white;

            Color color = SkillBasedInit.Settings.FriendlyUnactivated;
            if (hostility == Hostility.ENEMY) {
                color = SkillBasedInit.Settings.EnemyUnactivated;
            } else {
                if (!isPlayer) {
                    switch (hostility) {
                        case Hostility.FRIENDLY:
                            color = SkillBasedInit.Settings.AlliedUnactivated;
                            break;
                        case Hostility.NEUTRAL:
                            color = SkillBasedInit.Settings.NeutralUnactivated;
                            break;
                    }
                }
            }
            __instance.FlagFillImage.color = color;

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

            __instance.FlagOutline.color = Color.white;
            __instance.NumText.color = Color.white;

            Hostility hostility = __instance.Combat.HostilityMatrix.GetHostility(__instance.DisplayedActor.team, __instance.Combat.LocalPlayerTeam);
            bool isPlayer = __instance.DisplayedActor.team == __instance.Combat.LocalPlayerTeam;

            Color color = SkillBasedInit.Settings.FriendlyUnactivated;
            if (hostility == Hostility.ENEMY) {
                color = SkillBasedInit.Settings.EnemyUnactivated;
            } else {
                if (!isPlayer) {
                    switch (hostility) {
                        case Hostility.FRIENDLY:
                            color = SkillBasedInit.Settings.AlliedUnactivated;
                            break;
                        case Hostility.NEUTRAL:
                            color = SkillBasedInit.Settings.NeutralUnactivated;
                            break;
                    }
                }
            }
            __instance.FlagFillImage.color = color;

            /*
            FlagFillImage.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseFutureFill.color;
            FlagOutline.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseFutureOutline.color;
            NumText.color = LazySingletonBehavior<UIManager>.Instance.UILookAndColorConstants.PhaseFutureText.color;
            */
        }
    }

}
