using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using us.frostraptor.modUtils;

namespace SkillBasedInit.Helper
{
    public static class PilotExtensions
    {
        public static int RandomnessModifier(this Pilot pilot, UnitCfg config)
        {
            if (pilot == null) return 0;

            Mod.Log.Debug?.Write($"Calculating randomness modifier for pilot: {pilot.Name}");

            // Randomness is reduced by piloting
            int skillMod = CurrentSkillMod(pilot, pilot.Piloting, ModStats.MOD_SKILL_PILOT);
            int boundsMod = pilot.StatCollection.GetValue<int>(ModStats.BOUNDS_MOD_RANDOMNESS);
            int totalBoundsMod = skillMod + boundsMod;
            Mod.Log.Debug?.Write($"  totalBoundsMod: {totalBoundsMod} = skillMod: {skillMod} + pilotMod: {boundsMod}");

            int adjustedBound = config.RandomnessBoundsMaximum - totalBoundsMod;
            Mod.Log.Debug?.Write($"  adjustedBound: {adjustedBound} = config.RanBoundsMax: {config.RandomnessBoundsMaximum} - totalBounds: {totalBoundsMod}");
            if (adjustedBound < config.RandomnessBoundsMinimum)
                adjustedBound = config.RandomnessBoundsMinimum;

            int modifier = Mod.Random.Next(0, adjustedBound);
            Mod.Log.Debug?.Write($"  modifier: {modifier} => Math.Rand(0, {adjustedBound}");

            return modifier;
            ;
        }

        public static int InspiredModifier(this Pilot pilot, UnitCfg config)
        {
            if (pilot == null) return 0;
            if (pilot.ParentActor == null) return 0;
            if (!pilot.ParentActor.IsMoraleInspired && !pilot.ParentActor.IsFuryInspired) return 0;

            Mod.Log.Debug?.Write($"Calculating inspired modifier for pilot: {pilot.Name}");

            // Randomness is reduced by piloting
            int pilotMod = CurrentSkillMod(pilot, pilot.Piloting, ModStats.MOD_SKILL_PILOT);
            int adjustedMin = Mod.Config.Pilot.InspirationBoundsMinimum + pilotMod;
            if (adjustedMin < Mod.Config.Pilot.InspirationBoundsMinimum)
                adjustedMin = Mod.Config.Pilot.InspirationBoundsMinimum;
            
            int adjustedMax = Mod.Config.Pilot.InspirationBoundsMinimum + pilotMod;
            if (adjustedMax < Mod.Config.Pilot.InspirationBoundsMinimum)
                adjustedMax = Mod.Config.Pilot.InspirationBoundsMinimum;

            Mod.Log.Debug?.Write($"  adjusted bounds => min: {adjustedMin} to max: {adjustedMax}");

            int modifier = Mod.Random.Next(adjustedMin, adjustedMax);
            Mod.Log.Debug?.Write($"  modifier: {modifier} => Math.Rand({adjustedMin}, {adjustedMax}");

            return modifier;
        }
        public static int CurrentSkillMod(this Pilot pilot, int skillLevel, string statName)
        {
            int normedMod = SkillUtils.GetModifier(pilot, skillLevel, new Dictionary<int, int>(), new List<string>());
            int skillMod = statName != null ? pilot.StatCollection.GetValue<int>(statName) : 0;
            return normedMod + skillMod;
        }

        public static int CalledShotModifier(this Pilot pilot)
        {
            int gunneryMod = CurrentSkillMod(pilot, pilot.Gunnery, ModStats.MOD_SKILL_GUTS); 
            int tacticsMod = CurrentSkillMod(pilot, pilot.Tactics, ModStats.MOD_SKILL_TACTICS);
            int average = (int)Math.Floor((gunneryMod + tacticsMod) / 2.0);
            return average;
        }

        public static int VigilanceModifier(this Pilot pilot)
        {
            int gutsMod = CurrentSkillMod(pilot, pilot.Guts, ModStats.MOD_SKILL_GUTS);
            int tacticsMod = CurrentSkillMod(pilot, pilot.Tactics, ModStats.MOD_SKILL_TACTICS);
            int average = (int)Math.Floor((gutsMod + tacticsMod) / 2.0);
            return average;
        }

        public static void LogPilotStats(this Pilot pilot)
        {
            if (Mod.Config.Debug)
            {
                Dictionary<int, int> emptyDict = new Dictionary<int, int>();
                List<string> emptyList = new List<string>();
                int gunneryMod = SkillUtils.GetModifier(pilot, pilot.Gunnery, emptyDict, emptyList);
                int gutsMod = SkillUtils.GetModifier(pilot, pilot.Guts, emptyDict, emptyList);
                int pilotingMod = SkillUtils.GetModifier(pilot, pilot.Piloting, emptyDict, emptyList);
                int tacticsMod = SkillUtils.GetModifier(pilot, pilot.Tactics, emptyDict, emptyList);

                Mod.Log.Debug?.Write($"{pilot.Name} profile is " +
                    $"gunMod: {gunneryMod}  gutsMod: {gutsMod}  pilotMod: {pilotingMod}  tacMod: {tacticsMod}  " +
                    $""
                    );
            }
        }
    }

    public class PilotHelper
    {

        // Process any tags that provide flat bonuses
        public static int GetTagsModifier(Pilot pilot)
        {
            if (pilot == null) return 0;

            int mod = 0;
            foreach (string tag in pilot.pilotDef.PilotTags.Distinct())
            {
                if (Mod.Config.PilotTagModifiers.ContainsKey(tag))
                {
                    int tagMod = Mod.Config.PilotTagModifiers[tag];
                    //Mod.Log.Debug?.Write($"Pilot {pilot.Name} has tag:{tag}, applying modifier:{tagMod}");
                    mod += tagMod;
                }
            }
            return mod;
        }

        // Generates tooltip details for tags that provide modifiers
        public static List<string> GetTagsModifierDetails(Pilot pilot, int space = 2)
        {
            List<string> details = new List<string>();

            foreach (string tag in pilot.pilotDef.PilotTags.Distinct())
            {
                if (Mod.Config.PilotTagModifiers.ContainsKey(tag))
                {
                    int tagMod = Mod.Config.PilotTagModifiers[tag];
                    if (tagMod > 0)
                    {
                        details.Add($"<space={space}em><color=#00FF00>{tag}: {tagMod:+0}</color>");
                    }
                    else if (tagMod < 0)
                    {
                        details.Add($"<space={space}em><color=#FF0000>{tag}: {tagMod}</color>");
                    }
                }
            }

            return details;
        }
    }


}
