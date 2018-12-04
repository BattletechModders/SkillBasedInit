
namespace SkillBasedInit {
    public class Settings {
        // If true, extra logging will be printed
        public bool Debug = false;

        // The init malus per point of difference in tonnage
        public float MeleeTonnageMalus = 1;
        // The init malus multiplier used if the pilot has the Juggernaught skill
        public float MeleeAttackerJuggerMulti = 1.5f;

        // The init malus when a unit starts the round prone or shutdown
        public int ProneOrShutdownMalus = 6;

        // The init malus when a unit has lost a leg (mechs) or side (vehicles)
        public int MovementCrippledMalus = 13;

        // For each point of piloting, reduce negative effects by this percentage
        public float PilotingMultiplier = 0.05f;

        // For each point of guts, reduce negative effects by this percentage
        public float GutsMultiplier = 0.05f;

        public int PilotSpiritsModifier = 2;

        public float MechMeleeMulti = 0f;
        public float MechMeleeDFAMulti = 1.25f;

        public int VehicleROCModifier = 2;
        public float VehicleMeleeMulti = 1.0f;

        public int TurretROCModifier = 4;
        public float TurretMeleeMulti = 0f;
        public float TurretTonnageTagUnitLight = 60.0f;
        public float TurretTonnageTagUnitMedium = 80.0f;
        public float TurretTonnageTagUnitHeavy = 100.0f;
        public float TurretTonnageTagUnitNone = 120.0f;
    }
}
