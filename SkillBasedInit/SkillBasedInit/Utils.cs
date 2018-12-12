
namespace SkillBasedInit {
    public class Settings {
        // If true, extra logging will be printed
        public bool Debug = false;

        // The init malus when a unit starts the round prone (from a knockdown)
        public int ProneMalus = 9;

        // The init malus when a unit starts the round shutdown
        public int ShutdownMalus = 6;

        // The init malus when a unit has lost a leg (mechs) or side (vehicles)
        public int MovementCrippledMalus = 13;

        // The modifier applied for High Spirits or Low Spirits tag
        public int PilotSpiritsModifier = 2;

        // The init malus per point of difference in tonnage
        public float MeleeTonnageMalus = 1;

        // The init malus multiplier used if the pilot has the Juggernaught skill
        public float MeleeAttackerJuggerMulti = 1.5f;

        // Modifier applied to make vehicles slower
        public int VehicleROCModifier = 2;

        // Modifier applied to make turrets slower
        public int TurretROCModifier = 4;

        // Turrets don't have tonnage; supply a tonnage based upon unit tags
        public float TurretTonnageTagUnitLight = 60.0f;
        public float TurretTonnageTagUnitMedium = 80.0f;
        public float TurretTonnageTagUnitHeavy = 100.0f;
        public float TurretTonnageTagUnitNone = 120.0f;
    }
}
