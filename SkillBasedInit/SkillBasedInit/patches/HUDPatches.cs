using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using DG.Tweening;
using Harmony;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SkillBasedInit {

    [HarmonyPatch(typeof(CombatHUDMechTray), "Init")]
    [HarmonyPatch(new Type[] { typeof(MessageCenter), typeof(CombatHUD) })]
    public static class CombatHUDMechTray_Init {

        public static HBSTooltip CombatInitTooltip;

        public static void Postfix(CombatHUDMechTray __instance, MessageCenter messageCenter, CombatHUD HUD) {
            //SkillBasedInit.Logger.Log($"CombatHUDMechTray:Init - entered");

            CombatInitTooltip = __instance.gameObject.AddComponent(typeof(HBSTooltip)) as HBSTooltip;
            CombatInitTooltip.enabled = true;
            CombatInitTooltip.AllowRightClickExpansion = false;
            CombatInitTooltip.Show = true;
            CombatInitTooltip.gameObject.SetActive(true);

            BaseDescriptionDef initiativeData = new BaseDescriptionDef("CHUDMT_INIT", "TEST", $"{__instance}", null);
            CombatInitTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
        }

    }

    [HarmonyPatch(typeof(CombatHUDMechTray))]
    [HarmonyPatch("DisplayedActor", MethodType.Setter)]
    public static class CombatHUDMechTray_DisplayedActor_Setter {

        public static void Postfix(CombatHUDMechTray __instance) {
            //SkillBasedInit.Logger.Log($"CombatHUDMechTray:DisplayedActor:post - entered");

            if (__instance != null && __instance.DisplayedActor != null) {
                AbstractActor actor = __instance.DisplayedActor;

                ActorInitiative actorInit = ActorInitiativeHolder.GetOrCreate(actor);
                List<string> details = new List<string> {
                    $"Static Initiative: {actorInit.roundInitBase}",
                    $"<space=2em><color=#FF0000>-{actorInit.randomnessBounds[0]} to -{actorInit.randomnessBounds[1]} randomness</color> (piloting)"
                };

                int expectedInitMax = actorInit.roundInitBase;
                int expectedInitRandMin = actorInit.randomnessBounds[0];
                int expectedInitRandMax = actorInit.randomnessBounds[1];

                // Check for inspired status                
                if (actor.IsMoraleInspired || actor.IsFuryInspired) {
                    details.Add($"<space=2em><color=#00FF00>+1 to +3 Inspired</color>");
                    expectedInitRandMin += 1;
                    expectedInitRandMin += 3;
                }

                // Check for injuries. If there injuries on the previous round, apply them in full force. Otherwise, reduce them.
                if (actorInit.deferredInjuryMod != 0) {
                    details.Add($"<space=2em><color=#FF0000>-{actorInit.deferredInjuryMod} Deferred Injury</color>");
                    expectedInitMax -= actorInit.deferredInjuryMod;
                } else if (actor.GetPilot().Injuries != 0) {
                    // TODO: fold this in
                    int rawPenalty = actorInit.CalculateInjuryPenalty(0, actor.GetPilot().Injuries);
                    int penalty = (int)Math.Ceiling(rawPenalty / 2.0);
                    details.Add($"<space=2em><color=#FF0000>-{penalty} Previous Injuries</color>");
                    expectedInitMax -= penalty;
                }

                // Check for leg / side loss
                Mech mech = (Mech)actor;
                if (mech.IsLocationDestroyed(ChassisLocations.LeftLeg) || mech.IsLocationDestroyed(ChassisLocations.RightLeg)) {
                    int rawMod = SkillBasedInit.Settings.CrippledMovementModifier + actorInit.pilotingEffectMod;
                    int penalty = Math.Min(-1, rawMod);
                    details.Add($"<space=2em><color=#FF0000>{penalty} Crippled Movement</color>");
                    expectedInitMax += penalty;
                }
               
                // Check for prone 
                if (actor.IsProne) {
                    int rawMod = SkillBasedInit.Settings.ProneModifier + actorInit.pilotingEffectMod;               
                    int penalty = Math.Min(-1, rawMod);
                    details.Add($"<space=2em><color=#FF0000>{penalty} Prone</color>");
                    expectedInitMax += penalty;
                }

                // Check for shutdown
                if (actor.IsShutDown) {
                    int rawMod = SkillBasedInit.Settings.ShutdownModifier + actorInit.pilotingEffectMod;
                    int penalty = Math.Min(-1, rawMod);
                    details.Add($"<space=2em><color=#FF0000>{penalty} Shutdown</color>");
                    expectedInitMax += penalty;
                }

                // Check for melee impacts        
                if (actorInit.deferredMeleeMod > 0) {
                    expectedInitMax -= actorInit.deferredMeleeMod;
                    details.Add($"<space=2em><color=#FF0000>-{actorInit.deferredMeleeMod} Shutdown</color>");
                }

                // Check for an overly cautious player
                if (actorInit.deferredReserveMod > 0) {
                    expectedInitMax -= actorInit.deferredReserveMod;
                    details.Add($"<space=2em><color=#FF0000>-{actorInit.deferredReserveMod} Hesitation</color>");
                }

                int maxInit = Math.Max(expectedInitMax - expectedInitRandMin, SkillBasedInit.MinPhase);
                int minInit = Math.Max(expectedInitMax - expectedInitRandMax, SkillBasedInit.MinPhase);
                details.Add($"\nExpected Phase: <b>{maxInit} to {minInit}</b>");

                string tooltipTitle = $"{actor.DisplayName}: {actor.GetPilot().Name}";
                string tooltipText = String.Join("\n", details.ToArray());

                BaseDescriptionDef initiativeData = new BaseDescriptionDef("CHUDMT_INIT", tooltipTitle, tooltipText, null);
                CombatHUDMechTray_Init.CombatInitTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
            }
        }
    }


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
                    nameImg.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
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
                        nameImg.color = SkillBasedInit.Settings.FriendlyAlreadyActivated;
                    } else {
                        nameImg.color = SkillBasedInit.Settings.FriendlyUnactivated;
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
                    imgByCmp.color = SkillBasedInit.Settings.FriendlyUnactivated;

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
        }
    }

}
