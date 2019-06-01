using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;

namespace SkillBasedInit.Helper {
    public class PilotHelper {

        // The penalty range for injuries, keyed by guts level.
        private static readonly Dictionary<int, int[]> InjuryBounds = new Dictionary<int, int[]> {
            {  1, new[] { 5, 7 } },
            {  2, new[] { 4, 6 } },
            {  3, new[] { 4, 6 } },
            {  4, new[] { 3, 6 } },
            {  5, new[] { 3, 6 } },
            {  6, new[] { 3, 5 } },
            {  7, new[] { 3, 5 } },
            {  8, new[] { 2, 5 } },
            {  9, new[] { 2, 5 } },
            { 10, new[] { 1, 4 } },
            { 11, new[] { 1, 3 } },
            { 12, new[] { 1, 3 } },
            { 13, new[] { 1, 2 } }
        };

        // The randomness each round, keyed by piloting level.
        private static readonly Dictionary<int, int[]> RandomnessBounds = new Dictionary<int, int[]> {
            {  1, new[] { 3, 9 } },
            {  2, new[] { 2, 8 } },
            {  3, new[] { 2, 8 } },
            {  4, new[] { 1, 7 } },
            {  5, new[] { 1, 7 } },
            {  6, new[] { 0, 6 } },
            {  7, new[] { 0, 6 } },
            {  8, new[] { 0, 5 } },
            {  9, new[] { 0, 5 } },
            { 10, new[] { 0, 4 } },
            { 11, new[] { 0, 3 } },
            { 12, new[] { 0, 3 } },
            { 13, new[] { 0, 2 } }
        };

        private static readonly Dictionary<int, int> ModifierBySkill = new Dictionary<int, int> {
            { 1, 0 },
            { 2, 1 },
            { 3, 1 },
            { 4, 2 },
            { 5, 2 },
            { 6, 3 },
            { 7, 3 },
            { 8, 4 },
            { 9, 4 },
            { 10, 5 },
            { 11, 6 },
            { 12, 7 },
            { 13, 8 }
        };

        // Process any tags that provide flat bonuses
        public static int GetTagsModifier(Pilot pilot) {
            int mod = 0;

            foreach (string tag in pilot.pilotDef.PilotTags.Distinct()) {
                if (Mod.Config.PilotTagModifiers.ContainsKey(tag)) {
                    int tagMod = Mod.Config.PilotTagModifiers[tag];
                    Mod.Log.Debug($"Pilot {pilot.Name} has tag:{tag}, applying modifier:{tagMod}");
                    mod += tagMod;
                }
            }

            return mod;
        }

        // Generates tooltip details for tags that provide modifiers
        public static List<string> GetTagsModifierDetails(Pilot pilot, int space=2) {
            List<string> details = new List<string>();

            foreach (string tag in pilot.pilotDef.PilotTags.Distinct()) {
                if (Mod.Config.PilotTagModifiers.ContainsKey(tag)) {
                    int tagMod = Mod.Config.PilotTagModifiers[tag];
                    if (tagMod > 0) {
                        details.Add($"<space={space}em><color=#00FF00>{tag}: {tagMod:+0}</color>");
                    } else if (tagMod < 0) {
                        details.Add($"<space={space}em><color=#FF0000>{tag}: {tagMod}</color>");
                    }
                }
            }

            return details;
        }

        // Generates tooltip details for tags that have a non-modifier effect.
        //  This would be for things like juggernaught ability, drunk, etc
        public static List<string> GetPilotSpecialsDetails(Pilot pilot, int space=2) {
            List<string> details = new List<string>();

            foreach (string tag in pilot.pilotDef.PilotTags.Distinct()) {
                if (Mod.Config.PilotSpecialTagsDetails.ContainsKey(tag)) {
                    string tagEffect = Mod.Config.PilotSpecialTagsDetails[tag];
                    details.Add($"<space={space}em>{tag}: <i>{tagEffect}</i>");
                }
            }

            return details;
        }

        public static int GetGunneryModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Gunnery, "AbilityDefG5", "AbilityDefG8");
        }

        public static int GetGutsModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Guts, "AbilityDefGu5", "AbilityDefGu8");
        }

        public static int GetPilotingModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Piloting, "AbilityDefP5", "AbilityDefP8");
        }

        public static int GetTacticsModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Tactics, "AbilityDefT5A", "AbilityDefT8A");
        }

        public static int GetModifier(Pilot pilot, int skillValue, string abilityDefIdL5, string abilityDefIdL8) {
            int normalizedVal = NormalizeSkill(skillValue);
            int mod = ModifierBySkill[normalizedVal];

            bool hasL5 = false;
            bool hasL8 = false;
            foreach (Ability ability in pilot.Abilities) {
                Mod.Log.Trace($"Pilot {pilot.Name} has ability:{ability.Def.Id}.");
                if (ability.Def.Id.ToLower().Equals(abilityDefIdL5.ToLower())) {
                    Mod.Log.Debug($"Pilot {pilot.Name} has L5 ability:{ability.Def.Id}.");
                    hasL5 = true;
                }

                if (ability.Def.Id.ToLower().Equals(abilityDefIdL8.ToLower())) {
                    Mod.Log.Debug($"Pilot {pilot.Name} has L8 ability:{ability.Def.Id}.");
                    hasL5 = true;
                }
            }
            if (hasL5) mod++;
            if (hasL8) mod++;

            return mod;
        }

        public static int[] GetMeleeModifiers(Pilot pilot, float tonnage) {
            float[] meleeMultiplier = PilotHelper.GetMeleeMultipliers(pilot);
            float attackTonnage = tonnage * meleeMultiplier[0];
            float defenseTonnage = tonnage * meleeMultiplier[1];
            Mod.Log.Debug($"Pilot:{pilot.Name} with tonnage:{tonnage} counts as attack:{attackTonnage} defense:{defenseTonnage}");

            int gutsMod = GetGutsModifier(pilot);
            int attackTonnageMod = (int)Math.Ceiling(attackTonnage / 5.0);
            int defenseTonnageMod = (int)Math.Ceiling(defenseTonnage / 5.0);
            Mod.Log.Debug($"Pilot:{pilot.Name} has attackMod:{attackTonnageMod} + {gutsMod} defenseMod:{defenseTonnageMod} + {gutsMod}");

            return new int[] { attackTonnageMod + gutsMod, defenseTonnageMod + gutsMod};
        }

        public static int GetCalledShotModifier(Pilot pilot) {
            int gunneryMod = GetGunneryModifier(pilot);
            int tacticsMod = GetTacticsModifier(pilot);
            int average = (int) Math.Floor((gunneryMod + tacticsMod) / 2.0);
            return average;
        }

        public static int GetVigilanceModifier(Pilot pilot) {
            int gutsMod = GetGutsModifier(pilot);
            int tacticsMod = GetTacticsModifier(pilot);
            int average = (int)Math.Floor((gutsMod + tacticsMod) / 2.0);
            return average;
        }

        // Returns a multiplier for unit tonnage for attacker / defense cases
        private static float[] GetMeleeMultipliers(Pilot pilot) {
            float[] multipliers = new float[] { 1.0f, 1.0f };

            foreach (string tag in pilot.pilotDef.PilotTags.Distinct()) {
                if (Mod.Config.PilotTagMeleeMultipliers.ContainsKey(tag)) {
                    float[] tagMultis = Mod.Config.PilotTagMeleeMultipliers[tag];
                    Mod.Log.Debug($"Pilot {pilot.Name} has tag:{tag}, applying melee multipliers attack:{tagMultis[0]} defense:{tagMultis[1]}");
                    multipliers[0] += tagMultis[0];
                    multipliers[1] += tagMultis[1];
                }
            }
            return multipliers;
        }

        public static int[] GetInjuryBounds(Pilot pilot) {
            int normalizedVal = NormalizeSkill(pilot.Guts);
            int[] bounds = new int[2];
            InjuryBounds[normalizedVal].CopyTo(bounds, 0);
            return bounds;
        }

        public static int[] GetRandomnessBounds(Pilot pilot) {
            int normalizedVal = NormalizeSkill(pilot.Piloting);
            int[] bounds = new int[2];
            RandomnessBounds[normalizedVal].CopyTo(bounds, 0);
            return bounds;
        }

        private static int NormalizeSkill(int rawValue) {
            int normalizedVal = rawValue;
            if (rawValue >= 11 && rawValue <= 14) {
                // 11, 12, 13, 14 normalizes to 11
                normalizedVal = 11;
            } else if (rawValue >= 15 && rawValue <= 18) {
                // 15, 16, 17, 18 normalizes to 14
                normalizedVal = 12;
            } else if (rawValue == 19 || rawValue == 20) {
                // 19, 20 normalizes to 13
                normalizedVal = 13;
            } else if (rawValue <= 0) {
                normalizedVal = 1;
            } else if (rawValue > 20) {
                normalizedVal = 13;
            }
            return normalizedVal;
        }

        public static void LogPilotStats(Pilot pilot) {
            if (Mod.Config.Debug) {
                int normedGuts = NormalizeSkill(pilot.Guts);
                int gutsMod = GetGutsModifier(pilot);
                int normdPilot = NormalizeSkill(pilot.Piloting);
                int pilotingMod = GetPilotingModifier(pilot);
                int normedTactics = NormalizeSkill(pilot.Tactics);
                int tacticsMod = GetTacticsModifier(pilot);

                Mod.Log.Debug($"{pilot.Name} skill profile is " +
                    $"g:{pilot.Guts}->{normedGuts}={gutsMod}" +
                    $"p:{pilot.Piloting}->{normdPilot}={pilotingMod} " +
                    $"t:{pilot.Tactics}->{normedTactics}={tacticsMod} "
                    );
            }
        }
    }
}
