
namespace SkillBasedInit {
    public class Settings {
        // If true, extra logging will be printed
        public bool debug = false;

        // The init malus per point of difference in tonnage
        public float MeleeTonnageMalus = 1;
        // The init malus multiplier used if the pilot has the Juggernaught skill
        public float MeleeJuggerMulti = 1.5f;

        // The init malus when a unit starts the round prone or shutdown
        public int ProneOrShutdownMalus = 6;

        // The init malus when a unit has lost a leg (mechs) or side (vehicles)
        public int MovementCrippledMalus = 13;

        // For each point of piloting, reduce negative effects by this percentage
        public float PilotingMultiplier = 0.05f;

        public int PilotSpiritsModifier = 2;

        public int VehicleROCModifier = 2;
        public float VehicleMeleeMulti = 2.0f;

        public int TurretROCModifier = 4;
        public float TurretMeleeMulti = 1.5f;
    }
}
