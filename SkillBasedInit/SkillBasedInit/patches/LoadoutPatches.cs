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
        }
    }

}
