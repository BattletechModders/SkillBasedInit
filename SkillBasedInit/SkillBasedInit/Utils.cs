
using UnityEngine;

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

        // Colors for the UI elements
        /* No affiliation
         default color is: UILookAndColorConstants.PhaseCurrentFill.color
            RGBA(0.843, 0.843, 0.843, 1.000) -> 215, 215, 215 (gray)
            still to activate: 59, 177, 67 -> 0.231, 0.694, 0.262
            already activated: 11, 102, 35 -> 0.043, 0.4, 0.137
        */
        public readonly Color FriendlyUnactivated = new Color(0.231f, 0.694f, 0.262f, 1.0f);
        public readonly Color FriendlyAlreadyActivated = new Color(0.043f, 0.4f, 0.137f, 1.0f);

        /* Allied
         default color is: UILookAndColorConstants.AlliedUI.color            
            RGBA(0.522, 0.859, 0.965, 1.000) -> 133, 219, 246 (light blue)
            still to activate: 133, 219, 246 -> 0.521, 0.858, 0.964
            already activated: 88, 139, 174 -> 0.345, 0.545, 0.682
        */
        public readonly Color AlliedUnactivated = new Color(0.521f, 0.858f, 0.964f, 1.0f);
        public readonly Color AlliedAlreadyActivated = new Color(0.345f, 0.545f, 0.682f, 1.0f);

        /* Neutral
         default color is: UILookAndColorConstants.NeutralUI.color            
            RGBA(0.631, 0.631, 0.631, 1.000) -> 161, 161, 161 (gray)
            still to activate: 217, 221, 220 -> 0.850, 0.866, 0.862
            already activated: 119, 123, 126 -> 0.466, 0.482, 0.494    
        */
        public readonly Color NeutralUnactivated = new Color(0.850f, 0.866f, 0.862f, 1.0f);
        public readonly Color NeutralAlreadyActivated = new Color(0.466f, 0.482f, 0.494f, 1.0f);

        /* Enemy
         default color is: UILookAndColorConstants.EnemyUI.color            
            RGBA(0.941, 0.259, 0.157, 1.000) -> 240, 66, 40 (light red)
            still to activate: 255, 8, 0 -> 1, 0.031, 0
            already activated: 124, 10, 2 -> 0.486, 0.039, 0.007
        */
        public readonly Color EnemyUnactivated = new Color(1.0f, 0.031f, 0f, 1.0f);
        public readonly Color EnemyAlreadyActivated = new Color(0.486f, 0.039f, 0.007f, 1.0f);

    }
}
