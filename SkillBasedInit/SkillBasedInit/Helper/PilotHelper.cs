using System;
using System.Collections.Generic;
using BattleTech;

namespace SkillBasedInit.Helper {
    public class PilotHelper {

        private static readonly Dictionary<int, int[]> InjuryBounds = new Dictionary<int, int[]> {
            {  1, new[] { 6, 9 } },
            {  2, new[] { 5, 8 } },
            {  3, new[] { 5, 8 } },
            {  4, new[] { 4, 7 } },
            {  5, new[] { 4, 7 } },
            {  6, new[] { 3, 6 } },
            {  7, new[] { 3, 6 } },
            {  8, new[] { 2, 5 } },
            {  9, new[] { 2, 5 } },
            { 10, new[] { 1, 4 } },
            { 11, new[] { 1, 3 } },
            { 12, new[] { 1, 3 } },
            { 13, new[] { 1, 2 } }
        };

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

            foreach (string tag in pilot.pilotDef.PilotTags) {
                if (SkillBasedInit.Settings.PilotTagModifiers.ContainsKey(tag)) {
                    int tagMod = SkillBasedInit.Settings.PilotTagModifiers[tag];
                    SkillBasedInit.LogDebug($"Pilot {pilot.Name} has tag:{tag}, applying modifier:{tagMod}");
                    mod += tagMod;
                }
            }

            return mod;
        }

        // Generates tooltip details for tags that provide modifiers
        public static List<string> GetTagsModifierDetails(Pilot pilot) {
            List<string> details = new List<string>();

            foreach (string tag in pilot.pilotDef.PilotTags) {
                if (SkillBasedInit.Settings.PilotTagModifiers.ContainsKey(tag)) {
                    int tagMod = SkillBasedInit.Settings.PilotTagModifiers[tag];
                    if (tagMod > 0) {
                        details.Add($"<space=5em><color=#00FF00>{tag}: {tagMod}</color>");
                    } else if (tagMod < 0) {
                        details.Add($"<space=5em><color=#FF0000>{tag}: {tagMod}</color>");
                    }
                }
            }

            return details;
        }

        // Generates tooltip details for tags that have a non-modifier effect.
        //  This would be for things like juggernaught ability, drunk, etc
        public static List<string> GetPilotSpecialsDetails(Pilot pilot) {
            List<string> details = new List<string>();

            foreach (string tag in pilot.pilotDef.PilotTags) {
                if (SkillBasedInit.Settings.PilotTagModifiers.ContainsKey(tag)) {
                    int tagMod = SkillBasedInit.Settings.PilotTagModifiers[tag];

                }
            }

            return details;
        }

        public static int GetGutsModifier(Pilot pilot) {
            int mod = 0;

            int normalizedVal = NormalizeSkill(pilot.Guts);
            mod = ModifierBySkill[normalizedVal];

            return mod;
        }

        public static int GetPilotingModifier(Pilot pilot) {
            int mod = 0;

            int normalizedVal = NormalizeSkill(pilot.Piloting);
            mod = ModifierBySkill[normalizedVal];

            return mod;
        }

        public static int GetTacticsModifier(Pilot pilot) {
            int mod = 0;

            int normalizedVal = NormalizeSkill(pilot.Tactics);
            mod = ModifierBySkill[normalizedVal];

            return mod;
        }

        public static int[] GetMeleeModifiers(Pilot pilot, float tonnage) {
            float[] meleeMultiplier = PilotHelper.GetMeleeMultipliers(pilot);
            float attackTonnage = tonnage * meleeMultiplier[0];
            float defenseTonnage = tonnage * meleeMultiplier[1];
            SkillBasedInit.LogDebug($"Pilot:{pilot.Name} with tonnage:{tonnage} counts as attack:{attackTonnage} defense:{defenseTonnage}");

            return new int[] { (int)Math.Ceiling(attackTonnage / 5.0), (int)Math.Ceiling(defenseTonnage / 5.0) };
        }

        // Returns a multiplier for unit tonnage for attacker / defense cases
        private static float[] GetMeleeMultipliers(Pilot pilot) {
            float[] multipliers = new float[] { 1.0f, 1.0f };

            // If the pilot has the Juggernaugt skill, give them a bonus to attack
            foreach (Ability ability in pilot.Abilities) {
                if (ability.Def.Id == "AbilityDefGu5") {
                    multipliers[0] += SkillBasedInit.Settings.JuggernaughtBonus;
                }
            }

            foreach (string tag in pilot.pilotDef.PilotTags) {
                if (SkillBasedInit.Settings.PilotTagMeleeMultipliers.ContainsKey(tag)) {
                    float[] tagMultis = SkillBasedInit.Settings.PilotTagMeleeMultipliers[tag];
                    SkillBasedInit.LogDebug($"Pilot {pilot.Name} has tag:{tag}, applying melee multipliers attack:{tagMultis[0]} defense:{tagMultis[1]}");
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
            if (SkillBasedInit.Settings.Debug) {
                int normedGuts = NormalizeSkill(pilot.Guts);
                int gutsMod = GetGutsModifier(pilot);
                int normdPilot = NormalizeSkill(pilot.Piloting);
                int pilotingMod = GetPilotingModifier(pilot);
                int normedTactics = NormalizeSkill(pilot.Tactics);
                int tacticsMod = GetTacticsModifier(pilot);

                SkillBasedInit.LogDebug($"{pilot.Name} skill profile is " +
                    $"g:{pilot.Guts}->{normedGuts}={gutsMod}" +
                    $"p:{pilot.Piloting}->{normdPilot}={pilotingMod} " +
                    $"t:{pilot.Tactics}->{normedTactics}={tacticsMod} "
                    );
            }
        }
    }
}
