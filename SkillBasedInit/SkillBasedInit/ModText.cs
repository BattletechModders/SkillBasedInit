using System.Collections.Generic;

namespace SkillBasedInit
{
    public class ModText
    {


        // Floaties
        public const string LT_FT_KNOCKDOWN = "FLOATIE_KNOCKDOWN";
        public const string LT_FT_CALLED_SHOT_NOW = "FLOATIE_CALLED_SHOT_NOW";
        public const string LT_FT_CALLED_SHOT_LATER = "FLOATIE_CALLED_SHOT_LATER";
        public const string LT_FT_VIGILANCE = "FLOATIE_VIGILANCE";

        // Mech Bay
        public const string LT_MB_TONNAGE = "MECHBAY_TONNAGE";
        public const string LT_MB_UNIT_TYPE = "MECHBAY_UNIT_TYPE";
        public const string LT_MB_LANCE = "MECHBAY_LANCE";
        public const string LT_MB_TACTICS = "MECHBAY_TACTICS";
        public const string LT_MB_TOTAL = "MECHBAY_TOTAL";
        public const string LT_MB_RANDOM = "MECHBAY_RANDOM";
        public const string LT_MB_EXPECTED_NO_PILOT = "MECHBAY_EXPECTED_NO_PILOT";
        public const string LT_MB_EXPECTED = "MECHBAY_EXPECTED";

        // Tooltips
        public const string LT_TT_TITLE = "TOOLTIP_TITLE_INITIATIVE";
        public const string LT_TT_MECH_TONNAGE = "TOOLTIP_MECH_TONNAGE";
        public const string LT_TT_UNIT_TYPE = "TOOLTIP_UNIT_TYPE";
        public const string LT_TT_COMPONENTS = "TOOLTIP_COMPONENTS";
        public const string LT_TT_CRIPPLED = "TOOLTIP_CRIPPLED";
        public const string LT_TT_PRONE = "TOOLTIP_PRONE";
        public const string LT_TT_SHUTDOWN = "TOOLTIP_SHUTDOWN";
        public const string LT_TT_TACTICS = "TOOLTIP_TACTICS";
        public const string LT_TT_PILOT_TAGS = "TOOLTIP_PILOT_TAGS";
        public const string LT_TT_INJURY = "TOOLTIP_INJURY";
        public const string LT_TT_INSPIRED = "TOOLTIP_INSPIRED";
        public const string LT_TT_HESITATION = "TOOLTIP_HESITATION";
        public const string LT_TT_CALLED_SHOT = "TOOLTIP_CALLED_SHOT";
        public const string LT_TT_VIGILANCE = "TOOLTIP_VIGILANCE";
        public const string LT_TT_RANDOM = "TOOLTIP_RANDOM";
        public const string LT_TT_EXPECTED = "TOOLTIP_EXPECTED";
        public const string LT_TT_HOVER = "TOOLTIP_HOVER";

        public Dictionary<string, string> Floaties = new Dictionary<string, string>() 
        {
            // Floaties
            { LT_FT_KNOCKDOWN, "GOING DOWN! -{0} INIT" },
            { LT_FT_CALLED_SHOT_NOW, "CALLED SHOT! -{0} INIT" },
            { LT_FT_CALLED_SHOT_LATER, "CALLED SHOT! -{0} INIT NEXT ROUND" },
            { LT_FT_VIGILANCE, "VIGILANCE! +{0} INIT NEXT ROUND!" },
        };

        public Dictionary<string, string> MechBay = new Dictionary<string, string>() 
        {
            // Mech Bay
            { LT_MB_TONNAGE, "<b>BASE</b>: {0} (<i>from tonnage</i>)" },
            { LT_MB_UNIT_TYPE, "<space=2em><color=#{0}>{1:+0;-#} unit type</color>" },
            { LT_MB_LANCE, "<space=2em><color=#{0}>{1:+0;-#} lance</color>" },
            { LT_MB_TACTICS, "<space=2em>{0:+0;-#} tactics" },
            { LT_MB_TOTAL, "<b>TOTAL</b>: {0}" },
            { LT_MB_RANDOM, "<space=2em><color=#FF0000>{0} to {1} random</color> (<i>reduced by tactics</i>)" },
            { LT_MB_EXPECTED_NO_PILOT, "<b>EXPECTED PHASE</b>: {0}" },
            { LT_MB_EXPECTED, "<b>EXPECTED PHASE</b>: {0} to {1}" },
        };

        public Dictionary<string, string> Tooltip = new Dictionary<string, string>() 
        {
            // Tooltip
            { LT_TT_TITLE, "INITIATIVE" },
            { LT_TT_MECH_TONNAGE, "CHASSIS => tonnage: {0}" },
            { LT_TT_UNIT_TYPE, "<color=#{0}>{1:+0;-#} unit type</color>" },
            { LT_TT_COMPONENTS, "<color=#{0}>{1:+0;-#} components</color>" },
            { LT_TT_CRIPPLED, "<color=#FF0000>{0} crippled</color>" },
            { LT_TT_PRONE, "<color=#FF0000>{0} prone</color>" },
            { LT_TT_SHUTDOWN, "<color=#FF0000>{0} shutdown</color>" },

            { LT_TT_TACTICS, "PILOT => Tactics: <color=#00FF00>{0:+0}</color>" },
            { LT_TT_PILOT_TAGS, "<color=#{0}>{1:+0;-#} pilot tags</color>" },
            { LT_TT_INSPIRED, "<color=#00FF00>{0}inspired</color>" },
            { LT_TT_INJURY, "<color=#{0}>{1:+0;-#} injury</color>" },

            { LT_TT_HESITATION, "<color=#FF0000>{0} hesitation</color>" },
            { LT_TT_CALLED_SHOT, "<color=#FF0000>{0} called shot</color>" },
            { LT_TT_VIGILANCE, "<color=#00FF00>{0} vigilance</color>" },
            
            { LT_TT_RANDOM, "\nRANDOM => <color=#FF0000>-{0} to -{1}</color> <i>(reduced by tactics)</i>" },
            { LT_TT_EXPECTED, "EXPECTED PHASE: <b>{0} to {1}</b>" },
            { LT_TT_HOVER, "<i>Hover initiative in Mechlab & Deploy for details.</i>" },
        };
    }
}
