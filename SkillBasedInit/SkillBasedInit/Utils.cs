using System.Linq;
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

        public void InitializeColors() {
            FriendlyUnactivated = new Color(ColorFriendlyUnactivated[0], ColorFriendlyUnactivated[1], ColorFriendlyUnactivated[2], ColorFriendlyUnactivated[3]);
            FriendlyAlreadyActivated = new Color(ColorFriendlyAlreadyActivated[0], ColorFriendlyAlreadyActivated[1], ColorFriendlyAlreadyActivated[2], ColorFriendlyAlreadyActivated[3]);

            AlliedUnactivated = new Color(ColorAlliedUnactivated[0], ColorAlliedUnactivated[1], ColorAlliedUnactivated[2], ColorAlliedUnactivated[3]);
            AlliedAlreadyActivated = new Color(ColorAlliedAlreadyActivated[0], ColorAlliedAlreadyActivated[1], ColorAlliedAlreadyActivated[2], ColorAlliedAlreadyActivated[3]);

            NeutralUnactivated = new Color(ColorNeutralUnactivated[0], ColorNeutralUnactivated[1], ColorNeutralUnactivated[2], ColorNeutralUnactivated[3]);
            NeutralAlreadyActivated = new Color(ColorNeutralAlreadyActivated[0], ColorNeutralAlreadyActivated[1], ColorNeutralAlreadyActivated[2], ColorNeutralAlreadyActivated[3]);

            EnemyUnactivated = new Color(ColorEnemyUnactivated[0], ColorEnemyUnactivated[1], ColorEnemyUnactivated[2], ColorEnemyUnactivated[3]);
            EnemyAlreadyActivated = new Color(ColorEnemyAlreadyActivated[0], ColorEnemyAlreadyActivated[1], ColorEnemyAlreadyActivated[2], ColorEnemyAlreadyActivated[3]);
        }

        // Colors for the UI elements
        /* No affiliation
         default color is: UILookAndColorConstants.PhaseCurrentFill.color
            RGBA(0.843, 0.843, 0.843, 1.000) -> 215, 215, 215 (gray)
            still to activate: 59, 177, 67 -> 0.231, 0.694, 0.262
            already activated: 11, 102, 35 -> 0.043, 0.4, 0.137
        */
        public float[] ColorFriendlyUnactivated = new float[] { 0.231f, 0.694f, 0.262f, 1.0f };
        public float[] ColorFriendlyAlreadyActivated = new float[] { 0.043f, 0.4f, 0.137f, 1.0f };
        public Color FriendlyUnactivated;
        public Color FriendlyAlreadyActivated;

        /* Allied
         default color is: UILookAndColorConstants.AlliedUI.color            
            RGBA(0.522, 0.859, 0.965, 1.000) -> 133, 219, 246 (light blue)
            still to activate: 133, 219, 246 -> 0.521, 0.858, 0.964
            already activated: 88, 139, 174 -> 0.345, 0.545, 0.682
        */
        public float[] ColorAlliedUnactivated = new float[] { 0.521f, 0.858f, 0.964f, 1.0f };
        public float[] ColorAlliedAlreadyActivated = new float[] { 0.345f, 0.545f, 0.682f, 1.0f };
        public Color AlliedUnactivated;
        public Color AlliedAlreadyActivated;

        /* Neutral
         default color is: UILookAndColorConstants.NeutralUI.color            
            RGBA(0.631, 0.631, 0.631, 1.000) -> 161, 161, 161 (gray)
            still to activate: 217, 221, 220 -> 0.850, 0.866, 0.862
            already activated: 119, 123, 126 -> 0.466, 0.482, 0.494    
        */
        public float[] ColorNeutralUnactivated = new float[] { 0.850f, 0.866f, 0.862f, 1.0f };
        public float[] ColorNeutralAlreadyActivated = new float[] { 0.466f, 0.482f, 0.494f, 1.0f };
        public Color NeutralUnactivated;
        public Color NeutralAlreadyActivated;

        /* Enemy
         default color is: UILookAndColorConstants.EnemyUI.color            
            RGBA(0.941, 0.259, 0.157, 1.000) -> 240, 66, 40 (light red)
            still to activate: 255, 8, 0 -> 1, 0.031, 0
            already activated: 124, 10, 2 -> 0.486, 0.039, 0.007
        */
        public float[] ColorEnemyUnactivated = new float[] { 1.0f, 0.031f, 0f, 1.0f };
        public float[] ColorEnemyAlreadyActivated = new float[] { 0.486f, 0.039f, 0.007f, 1.0f };
        public Color EnemyUnactivated;
        public Color EnemyAlreadyActivated;

    }
}
