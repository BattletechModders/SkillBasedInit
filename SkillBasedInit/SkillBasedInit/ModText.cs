using System.Collections.Generic;

namespace SkillBasedInit
{
    public class ModText
    {
        // Tooltips
        public const string LT_TT_TITLE = "TOOLTIP_TITLE_INITIATIVE";
        public const string LT_TT_MECH_TONNAGE = "TOOLTIP_MECH_TONNAGE";
        public const string LT_TT_VEHICLE_ROC = "TOOLTIP_VEHICLE_RULE_OF_COOL";
        public const string LT_TT_COMPONENTS = "TOOLTIP_COMPONENTS";
        public const string LT_TT_ENGINES = "TOOLTIP_ENGINES";
        public const string LT_TT_LEG_DESTROYED = "TOOLTIP_LEG_DESTROYED";
        public const string LT_TT_PRONE = "TOOLTIP_PRONE";
        public const string LT_TT_SHUTDOWN = "TOOLTIP_SHUTDOWN";
        public const string LT_TT_TACTICS = "TOOLTIP_TACTICS";
        public const string LT_TT_INSPIRED = "TOOLTIP_INSPIRED";
        public const string LT_TT_FRESH_INJURY = "TOOLTIP_FRESH_INJURY";
        public const string LT_TT_PAINFUL_INJURY = "TOOLTIP_PAINFUL_INJURY";
        public const string LT_TT_HESITATION = "TOOLTIP_HESITATION";
        public const string LT_TT_CALLED_SHOT_TARG = "TOOLTIP_CALLED_SHOT_TARGET";
        public const string LT_TT_VIGILANCE = "TOOLTIP_VIGILANCE";
        public const string LT_TT_RANDOM = "TOOLTIP_RANDOM";
        public const string LT_TT_EXPECTED = "TOOLTIP_EXPECTED";
        public const string LT_TT_HOVER = "TOOLTIP_HOVER";

        // Floaties
        public const string LT_FT_INJURY_NOW = "FLOATIE_INJURY_NOW";
        public const string LT_FT_INJURY_LATER = "FLOATIE_INJURY_LATER";
        public const string LT_FT_KNOCKDOWN_NOW = "FLOATIE_KNOCKDOWN_NOW";
        public const string LT_FT_KNOCKDOWN_LATER = "FLOATIE_KNOCKDOWN_LATER";
        public const string LT_FT_CALLED_SHOT_NOW = "FLOATIE_CALLED_SHOT_NOW";
        public const string LT_FT_CALLED_SHOT_LATER = "FLOATIE_CALLED_SHOT_LATER";
        public const string LT_FT_VIGILANCE = "FLOATIE_VIGILANCE";

        // Mech Bay
        public const string LT_MB_TONNAGE = "MECHBAY_TONNAGE";
        public const string LT_MB_VEHICLE_ROC = "MECHBAY_VEHICLE_RULE_OF_COOL";
        public const string LT_MB_COMPONENTS = "MECHBAY_COMPONENTS";
        public const string LT_MB_ENGINES = "MECHBAY_ENGINES";
        public const string LT_MB_LANCE = "MECHBAY_LANCE";
        public const string LT_MB_TACTICS = "MECHBAY_TACTICS";
        public const string LT_MB_TOTAL = "MECHBAY_TOTAL";
        public const string LT_MB_RANDOM = "MECHBAY_RANDOM";
        public const string LT_MB_EXPECTED_NO_PILOT = "MECHBAY_EXPECTED_NO_PILOT";
        public const string LT_MB_EXPECTED = "MECHBAY_EXPECTED";


        public Dictionary<string, string> Floaties = new Dictionary<string, string>() 
        {
            // Floaties
            { LT_FT_INJURY_LATER, "OUCH! -{0} INITIATIVE" },
            { LT_FT_INJURY_NOW, "OUCH! -{0} INITIATIVE NEXT ROUND" },
            { LT_FT_KNOCKDOWN_NOW, "GOING DOWN! -{0} INITIATIVE" },
            { LT_FT_KNOCKDOWN_LATER, "GOING DOWN! -{0} INITIATIVE NEXT ROUND" },
            { LT_FT_CALLED_SHOT_NOW, "CALLED SHOT! -{0} INITIATIVE" },
            { LT_FT_CALLED_SHOT_LATER, "CALLED SHOT! -{0} INITIATIVE NEXT ROUND" },
            { LT_FT_VIGILANCE, "VIGILANCE! +{0} INITIATIVE NEXT ROUND!" },
        };

        public Dictionary<string, string> MechBay = new Dictionary<string, string>() 
        {
            // Mech Bay
            { LT_MB_TONNAGE, "<b>BASE</b>: {0} (<i>from tonnage</i>)" },
            { LT_MB_VEHICLE_ROC, "<space=2em><color=#{0}>{1:+0;-#} vehicle</color>" },
            { LT_MB_COMPONENTS, "<space=2em><color=#{0}>{1:+0;-#} components</color>" },
            { LT_MB_ENGINES, "<space=2em><color=#{0}>{1:+0;-#} engine</color>" },
            { LT_MB_LANCE, "<space=2em><color=#{0}>{1:+0;-#} lance</color>" },
            { LT_MB_TACTICS, "<space=2em>{0:+0;-#} tactics" },
            { LT_MB_TOTAL, "<b>TOTAL</b>: {0}" },
            { LT_MB_RANDOM, "<space=2em><color=#FF0000>-{0} to -{1} random</color> (<i>reduced by tactics</i>)" },
            { LT_MB_EXPECTED_NO_PILOT, "<b>EXPECTED PHASE</b>: {0}" },
            { LT_MB_EXPECTED, "<b>EXPECTED PHASE</b>: {0} to {1}" },
        };

        public Dictionary<string, string> Tooltip = new Dictionary<string, string>() 
        {
            // Tooltip
            { LT_TT_TITLE, "INITIATIVE" },
            { LT_TT_MECH_TONNAGE, "CHASSIS => tonnage: {0}" },
            { LT_TT_VEHICLE_ROC, "<color=#{0}>{1:+0;-#} vehicle</color>" },
            { LT_TT_COMPONENTS, "<color=#{0}>{1:+0;-#} components</color>" },
            { LT_TT_ENGINES, "<color=#{0}>{1:+0;-#} engine</color>" },
            { LT_TT_LEG_DESTROYED, "<color=#FF0000>{0} Leg Destroyed</color>" },
            { LT_TT_PRONE, "<color=#FF0000>{0} Prone</color>" },
            { LT_TT_SHUTDOWN, "<color=#FF0000>{0} Shutdown</color>" },
            { LT_TT_TACTICS, "PILOT => Tactics: <color=#00FF00>{0:+0}</color>" },
            { LT_TT_INSPIRED, "<color=#00FF00>+1 to +3 Inspired</color>" },
            { LT_TT_FRESH_INJURY, "<color=#FF0000>-{0} Fresh Injury</color>" },
            { LT_TT_PAINFUL_INJURY, "<color=#FF0000>-{0} Painful Injury</color>" },
            { LT_TT_HESITATION, "<color=#FF0000>-{0} Hesitation</color>" },
            { LT_TT_CALLED_SHOT_TARG, "<color=#FF0000>-{0} Called Shot Target</color>" },
            { LT_TT_VIGILANCE, "<color=#00FF00>-{0} Vigilance</color>" },
            { LT_TT_RANDOM, "\nRANDOM => <color=#FF0000>-{0} to -{1}</color> <i>(reduced by tactics)</i>" },
            { LT_TT_EXPECTED, "EXPECTED PHASE: <b>{0} to {1}</b>" },
            { LT_TT_HOVER, "<i>Hover initiative in Mechlab & Deploy for details.</i>" },
        };
    }
}
