using BattleTech.UI;
using Harmony;
using System;
using TMPro;
using UnityEngine;

namespace SkillBasedInit { 

    // Sets the initiative value in the mech-bay
    [HarmonyPatch(typeof(MechBayMechInfoWidget), "SetInitiative")]
    [HarmonyPatch(new Type[] { })]
    public static class MechBayMechInfoWidget_SetInitiative {
        public static void Postfix(MechBayMechInfoWidget __instance, GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText) {
            //SkillBasedInit.Logger.Log($"MechBayMechInfoWidget::SetInitiative::post - disabling text");
            if (___initiativeObj.activeSelf) {
                ___initiativeText.SetText("{0}", new object[] { "-" });
            }
        }
    }

    // Sets the initiative value in the lance loading screen
    [HarmonyPatch(typeof(LanceLoadoutSlot), "RefreshInitiativeData")]
    [HarmonyPatch(new Type[] { })]
    public static class LanceLoadoutSlot_RefreshInitiativeData {
        public static void Postfix(LanceLoadoutSlot __instance, GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText) {
            //SkillBasedInit.Logger.Log($"LanceLoadoutSlot::RefreshInitiativeData::post - disabling text");
            if (___initiativeObj != null && ___initiativeText != null && ___initiativeObj.activeSelf) {
                ___initiativeText.SetText("{0}", new object[] { "-" });
            }

            if (__instance.SelectedMech != null) {

            }

            if (__instance.SelectedPilot != null) {

            }
        }
    }

    /*
        if (!((Object)initiativeObj == (Object)null) && !((Object)initiativeText == (Object)null) && !((Object)initiativeColor == (Object)null)) {
                if ((Object)SelectedMech == (Object)null || SelectedMech.MechDef == null || SelectedMech.MechDef.Chassis == null) {
                    initiativeObj.SetActive (false);
                } else if ((Object)SelectedPilot == (Object)null || SelectedPilot.Pilot == null || SelectedPilot.Pilot.pilotDef == null) {
                    initiativeObj.SetActive (false);
                } else {
                    initiativeObj.SetActive (true);
                    int num = 0;
                    int num2 = 0;
                    switch (SelectedMech.MechDef.Chassis.weightClass) {
                    case WeightClass.LIGHT:
                        num = 4;
                        break;
                    case WeightClass.MEDIUM:
                        num = 3;
                        break;
                    case WeightClass.HEAVY:
                        num = 2;
                        break;
                    case WeightClass.ASSAULT:
                        num = 1;
                        break;
                    }
                    if (SelectedPilot.Pilot.pilotDef.AbilityDefs != null) {
                        foreach (AbilityDef abilityDef in SelectedPilot.Pilot.pilotDef.AbilityDefs) {
                            foreach (EffectData effectDatum in abilityDef.EffectData) {
                                if (MechStatisticsRules.GetInitiativeModifierFromEffectData (effectDatum, true, null) == 0) {
                                    num2 += MechStatisticsRules.GetInitiativeModifierFromEffectData (effectDatum, false, null);
                                }
                            }
                        }
                    }
                    if (selectedMech.MechDef.Inventory != null) {
                        MechComponentRef[] inventory = SelectedMech.MechDef.Inventory;
                        foreach (MechComponentRef mechComponentRef in inventory) {
                            if (mechComponentRef.Def != null && mechComponentRef.Def.statusEffects != null) {
                                EffectData[] statusEffects = mechComponentRef.Def.statusEffects;
                                foreach (EffectData effect in statusEffects) {
                                    if (MechStatisticsRules.GetInitiativeModifierFromEffectData (effect, true, null) == 0) {
                                        num2 += MechStatisticsRules.GetInitiativeModifierFromEffectData (effect, false, null);
                                    }
                                }
                            }
                        }
                    }
                    int num3 = 0;
                    if ((Object)LC != (Object)null) {
                        num3 = LC.lanceInitiativeModifier;
                    }
                    num2 += num3;
                    int num4 = Mathf.Clamp (num + num2, 1, 5);
                    initiativeText.SetText ($"{num4}");
                    if ((Object)initiativeTooltip != (Object)null) {
                        BaseDescriptionDef initiativeData = GetInitiativeData (SelectedMech.MechDef.Chassis.weightClass);
                        if (initiativeData != null) {
                            initiativeTooltip.enabled = true;
                            initiativeTooltip.SetDefaultStateData (TooltipUtilities.GetStateDataFromObject (initiativeData));
                        }
                    }
                    initiativeColor.SetUIColor ((num2 <= 0) ? ((num2 >= 0) ? UIColor.White : UIColor.Red) : UIColor.Gold);
                }
            }
   
     */

}
