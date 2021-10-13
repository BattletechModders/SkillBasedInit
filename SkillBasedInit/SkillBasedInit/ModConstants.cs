
namespace SkillBasedInit
{
    public class ModConsts
    {
    }

    public class ModTags
    {
        public const string TAG_UNIT_MECH = "unit_mech";
    }

    public class ModStats
    {
        // Base game values
        public const string HBS_BONUS_HEALTH = "BonusHeath";

        // Assumed to be provided by status effects, not consumed
        public const string MOD_INJURY = "SBI_MOD_INJURY";
        public const string MOD_MISC = "SBI_MOD_MISC";

        // Assumed to be provided by status effects, consumed on recalculation
        public const string MOD_CALLED_SHOT_ATTACKER = "SBI_MOD_CALLED_SHOT_ATTACKER"; // modifies called shot penalty for the defender
        public const string MOD_CALLED_SHOT_TARGET = "SBI_MOD_CALLED_SHOT_TARGET"; // modifies called shot penalty for the defender
        public const string STATE_CALLED_SHOT = "SBI_STATE_CALLED_SHOT"; //  current called shot penalty

        public const string MOD_VIGILANCE = "SBI_MOD_VIGILANCE"; // modifies generated vigilance
        public const string STATE_VIGILIANCE = "SBI_STATE_VIGILANCE"; // current vigilant bonus

        public const string MOD_HESITATION = "SBI_MOD_HESITATION"; // modifies generated hesitation
        public const string STATE_HESITATION = "SBI_STATE_HESITATION"; // current hesitation penalty

        // Calculated by SBI - do not modify directly!
        public const string ROUND_INIT = "SBI_ROUND_INIT";

        public const string STATE_TONNAGE = "SBI_STATE_TONNAGE";
        public const string STATE_UNIT_TYPE = "SBI_STATE_UNIT_TYPE";

        // Pilot specific vallues
        public const string STATE_PILOT_TAGS = "SBI_STATE_PILOT_TAGS";
        public const string MOD_SKILL_GUNNERY = "SBI_MOD_GUNNERY";
        public const string MOD_SKILL_GUTS = "SBI_MOD_GUTS";
        public const string MOD_SKILL_PILOT = "SBI_MOD_PILOTING";
        public const string MOD_SKILL_TACTICS = "SBI_MOD_TACTICS";
    }

}
