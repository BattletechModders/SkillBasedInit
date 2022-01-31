using System;
using System.Collections.Generic;
using System.Linq;
using BattleTech;
using us.frostraptor.modUtils;

namespace SkillBasedInit.Helper
{
    public static class PilotExtensions
    {
        // Max and min will be negative here, as they represent phase modifiers
        //  invert them for calculation purposes
        public static int[] RandomnessBounds(this Pilot pilot, int min, int max)
        {
            if (pilot == null) return new int[]{ 0, 0 };

            Mod.Log.Debug?.Write($"Calculating randomness bounds for pilot: {pilot.Name}");

            int invertedMin = -1 * min;
            int invertedMax = -1 * max;

            // skillMod reduces the maximum randomness
            int skillMod = pilot.SBIPilotingMod();
            int adjustedMax = invertedMax - skillMod;

            Mod.Log.Debug?.Write($"  adjustedBound: {adjustedMax} = config.RanBoundsMax: {invertedMax} - skillMod: {skillMod}");
            if (adjustedMax < min)
                adjustedMax = min;

            return new int[] { invertedMin, adjustedMax };
        }

        public static int RandomnessModifier(this Pilot pilot, UnitCfg config)
        {
            if (pilot == null) return 0;

            Mod.Log.Debug?.Write($"Calculating randomness modifier for pilot: {pilot.Name}");

            int[] bounds = RandomnessBounds(pilot, config.RandomnessMin, config.RandomnessMax);

            // Add +1 to max BECUASE MICROSOFT SUCKS (see https://docs.microsoft.com/en-us/dotnet/api/system.random.next?view=net-6.0#system-random-next(system-int32-system-int32)
            int modifier = Mod.Random.Next(bounds[0], bounds[1] + 1);
            Mod.Log.Debug?.Write($"  modifier: {modifier} => Math.Rand({bounds[0]}, {bounds[1]})");

            return modifier;
        }

        public static int InspiredModifier(this Pilot pilot)
        {
            if (pilot == null) return 0;
            if (pilot.ParentActor == null) return 0;
            if (!pilot.ParentActor.IsMoraleInspired && !pilot.ParentActor.IsFuryInspired) return 0;

            Mod.Log.Debug?.Write($"Calculating inspired modifier for pilot: {pilot.Name}");

            // Config are phase modifiers, so invert
            UnitCfg unitCfg = pilot.ParentActor.GetUnitConfig();
            int invertedMin = unitCfg.InspiredMin * -1;
            int invertedMax = unitCfg.InspiredMax * -1;

            // Invert, because we expect this to be a bonus
            int skillMod = pilot.SBITacticsMod() * -1;

            int adjustedMin = invertedMin + skillMod; 
            int adjustedMax = invertedMax + skillMod;

            // Add +1 to max BECUASE MICROSOFT SUCKS (see https://docs.microsoft.com/en-us/dotnet/api/system.random.next?view=net-6.0#system-random-next(system-int32-system-int32)
            int modifier = Mod.Random.Next(adjustedMax, adjustedMin + 1);
            Mod.Log.Debug?.Write($"  modifier: {modifier} => Math.Rand({adjustedMax}, {adjustedMin})");

            return modifier;
        }

        public static int SBIGunneryMod(this Pilot pilot)
        {
            int normedMod = SkillUtils.GetGunneryModifier(pilot);
            int skillMod = pilot.StatCollection.GetValue<int>(ModStats.MOD_SKILL_GUNNERY);
            Mod.Log.Debug?.Write($"For pilot: {pilot.Name} with skill: {pilot.Gunnery}  normedMod: {normedMod} + skillMod: {skillMod}");
            return normedMod + skillMod;
        }

        public static int SBIGutsMod(this Pilot pilot)
        {
            int normedMod = SkillUtils.GetGutsModifier(pilot);
            int skillMod = pilot.StatCollection.GetValue<int>(ModStats.MOD_SKILL_GUTS);
            Mod.Log.Debug?.Write($"For pilot: {pilot.Name} with skill: {pilot.Guts}  normedMod: {normedMod} + skillMod: {skillMod}");
            return normedMod + skillMod;
        }

        public static int SBIPilotingMod(this Pilot pilot)
        {
            int normedMod = SkillUtils.GetPilotingModifier(pilot);
            int skillMod = pilot.StatCollection.GetValue<int>(ModStats.MOD_SKILL_PILOT);
            Mod.Log.Debug?.Write($"For pilot: {pilot.Name} with skill: {pilot.Piloting}  normedMod: {normedMod} + skillMod: {skillMod}");
            return normedMod + skillMod;
        }

        public static int SBITacticsMod(this Pilot pilot)
        {
            int normedMod = SkillUtils.GetTacticsModifier(pilot);
            int skillMod = pilot.StatCollection.GetValue<int>(ModStats.MOD_SKILL_TACTICS);
            Mod.Log.Debug?.Write($"For pilot: {pilot.Name} with skill: {pilot.Tactics}  normedMod: {normedMod} + skillMod: {skillMod}");
            return normedMod + skillMod;
        }

        public static int AverageGunneryAndTacticsMod(this Pilot pilot)
        {
            return (int)Math.Ceiling((pilot.SBIGunneryMod() + pilot.SBITacticsMod()) / 2.0);
        }

        public static int AverageGutsAndTacticsMod(this Pilot pilot)
        {
            return (int)Math.Ceiling((pilot.SBIGutsMod() + pilot.SBITacticsMod()) / 2.0);
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
                if (Mod.Config.Pilot.PilotTagModifiers.ContainsKey(tag))
                {
                    int tagMod = Mod.Config.Pilot.PilotTagModifiers[tag];
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
                if (Mod.Config.Pilot.PilotTagModifiers.ContainsKey(tag))
                {
                    int tagMod = Mod.Config.Pilot.PilotTagModifiers[tag];
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
