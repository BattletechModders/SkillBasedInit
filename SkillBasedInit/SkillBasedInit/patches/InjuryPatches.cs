using BattleTech;
using Harmony;
using System;

namespace SkillBasedInit {

    // Injure the pilot as soon as it happens
    [HarmonyPatch(typeof(Pilot), "InjurePilot")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(int), typeof(int), typeof(DamageType), typeof(Weapon), typeof(AbstractActor) })]
    public static class Pilot_InjurePilot {

        public static void Prefix(Pilot __instance, ref int __state) {
            __state = 0;
            if (__instance.StatCollection.GetStatistic("BonusHeath") != null) {
                __state = __instance.StatCollection.GetStatistic("BonusHeath").Value<int>();
            }
            SkillBasedInit.Logger.Log($"Pilot:InjurePilot:pre - Actor:({__instance.ParentActor.DisplayName}_{__instance.Name}) injured with bonusHealth:{__state}");
        }

        public static void Postfix(Pilot __instance, int __state, string sourceID, int stackItemUID, int dmg, DamageType damageType, Weapon sourceWeapon, AbstractActor sourceActor) {
            SkillBasedInit.Logger.Log($"Pilot:InjurePilot:post - Actor:({__instance.ParentActor.DisplayName}_{__instance.Name}) injured with initial bonusHealth:{__state}");

            int currentBonus = 0;
            if (__instance.StatCollection.GetStatistic("BonusHeath") != null) {
                currentBonus = __instance.StatCollection.GetStatistic("BonusHeath").Value<int>();
            }
            int bonusDelta = __state - currentBonus;
            int damageTaken = dmg - bonusDelta;
            SkillBasedInit.LogDebug($"  Actor:({__instance.ParentActor.DisplayName}_{__instance.Name}) lost bonusHealth:{bonusDelta}, while results in damage:{damageTaken} from the attack.");

            // If the attacker took any damage, apply it
            if (damageTaken > 0) {
                AbstractActor parent = __instance.ParentActor;
                ActorInitiative target = ActorInitiativeHolder.GetOrCreate(parent);
                int injuryPenalty = target.CalculateInjuryPenalty(damageTaken, __instance.Injuries);

                if (!parent.HasActivatedThisRound) {
                    // Apply penalty in current round. Remember high init -> higher phase
                    parent.Initiative += injuryPenalty;
                    if (parent.Initiative > SkillBasedInit.MaxPhase) {
                        parent.Initiative = SkillBasedInit.MaxPhase;
                    }
                    parent.Combat.MessageCenter.PublishMessage(new ActorPhaseInfoChanged(parent.GUID));
                    parent.Combat.MessageCenter.PublishMessage(new FloatieMessage(parent.GUID, parent.GUID, $"OUCH! -{injuryPenalty} INITIATIVE", FloatieMessage.MessageNature.Debuff));
                } else {
                    // Injuries are cumulative
                    target.deferredInjuryMod += injuryPenalty;
                    parent.Combat.MessageCenter.PublishMessage(new FloatieMessage(parent.GUID, parent.GUID, $"OUCH! -{injuryPenalty} INITIATIVE NEXT ROUND", FloatieMessage.MessageNature.Debuff));
                }

            }
        }
        /*
         *          
         * if (ParentActor != null && ParentActor.StatCollection.GetValue<bool> ("IgnorePilotInjuries")) {
                Combat.MessageCenter.PublishMessage (new FloatieMessage (sourceID, ParentActor.GUID, Strings.T ("INJURY IGNORED"), FloatieMessage.MessageNature.PilotInjury));
            } else {
                int bonusHealth = BonusHealth;
                if (bonusHealth > 0) {
                    int num = Mathf.Min (dmg, bonusHealth);
                    statCollection.ModifyStat (sourceID, stackItemUID, "BonusHealth", StatCollection.StatOperation.Int_Subtract, num, -1, true);
                    dmg -= num;
                }
                if (dmg > 0) {
                    if (!IsIncapacitated) {
                        SaveInjuryInfo (damageType, ParentActor, sourceWeapon, sourceActor);
                    }
                    StatCollection.ModifyStat (sourceID, stackItemUID, "Injuries", StatCollection.StatOperation.Int_Add, dmg, -1, true);
                    if (Combat != null) {
                        Combat.MessageCenter.PublishMessage (new PilotDamagedMessage (ParentActor.GUID, StatCollection.GetValue<int> ("Injuries")));
                    }
                    if (injuryLogger.IsLogEnabled) {
                        injuryLogger.Log ($"////// POWPOW (>_<*): {Description.Name} Injured! ///// Cause: {damageType.ToString ()} ///// Injuries: {Injuries}");
                    }
                }
                ApplyInjuryAbilities (sourceID, stackItemUID);
            }

public void ApplyInjuryAbilities (string sourceID, int stackItemUID)
{
    if (OnInjuredAbilities != null) {
        for (int i = 0; i < OnInjuredAbilities.Count; i++) {
            for (int j = 0; j < OnInjuredAbilities [i].Def.EffectData.Count; j++) {
                Combat.EffectManager.CreateEffect (OnInjuredAbilities [i].Def.EffectData [j], OnInjuredAbilities [i].Def.Id, stackItemUID, ParentActor, ParentActor, default(WeaponHitInfo), 0, false);
            }
            Combat.MessageCenter.PublishMessage (new FloatieMessage (sourceID, ParentActor.GUID, OnInjuredAbilities [i].Def.Description.Name, FloatieMessage.MessageNature.Buff));
            if (injuryLogger.IsLogEnabled) {
                injuryLogger.Log ($"////// ZAP (>n<!): {Description.Name} Gains {OnInjuredAbilities [i].Def.Description.Name}!");
            }
        }
    }
}
         * 
         */
    }
}
