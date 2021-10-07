using System.Collections.Generic;
using UnityEngine;

namespace SkillBasedInit
{

    public class UnitCfg
    {
        public const int DEFAULT_TYPE_MOD = -99;

        public int TypeMod = DEFAULT_TYPE_MOD;

        // The maximum value used for randomness, from 0 to this value. Offset by piloting mod.
        public int RandomnessMax = 10;
        public int RandomnessMin = 2;

        public int HesitationMax = 6;
        public int HesitationMin = 2;

        public int CalledShotRandMax = 6;
        public int CalledShotRandMin = 2;

        public int VigilanceRandMax = 6;
        public int VigilanceRandMin = 2;

        public int InspiredMax = 3;
        public int InspiredMin = 1;

        public float DefaultTonnage = 120f;

        public Dictionary<int, int> InitBaseByTonnage = new Dictionary<int, int>();
    }

    public class MechCfg : UnitCfg
    {
        // The init malus when a unit starts the round prone
        public int ProneModifierMin = -2;
        public int ProneModifierMax = -9;

        // The malus for a unit that starts the round shutdown
        public int ShutdownModifierMin = -2;
        public int ShutdownModifierMax = -8;

        // The init malus when a unit has lost a movement system
        public int CrippledModifierMin = -5;
        public int CrippledModifierMax = -13;
    }

    public class TrooperCfg : UnitCfg
    {
        // The malus for a unit that starts the round shutdown
        public int ShutdownModifierMin = -2;
        public int ShutdownModifierMax = -8;
    }

    public class NavalCfg : UnitCfg
    {
        // The init malus when a unit has lost a movement system
        public int CrippledModifierMin = -5;
        public int CrippledModifierMax = -13;
    }

    public class TurretCfg : UnitCfg
    {
        // Turrets don't have tonnage; supply a tonnage based upon unit tags
        public float LightTonnage = 60.0f;
        public float MediumTonnage = 80.0f;
        public float HeavyTonnage = 100.0f;
    }

    public class VehicleCfg : UnitCfg
    {
        // The init malus when a unit has lost a movement system
        public int CrippledModifierMin = -5;
        public int CrippledModifierMax = -13;
    }


    public class PilotCfg
    {
        public int InspirationMax = 4;
        public int InspirationMin = 1;

    }

    public class IconCfg
    {
        public string Stopwatch = "sbi_stopwatch";
    }

    public class ColorCfg
    {
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

    public class ModConfig
    {
        public bool Debug = false;
        public bool Trace = false;

        public MechCfg Mech = new MechCfg();
        public TrooperCfg Trooper = new TrooperCfg();
        public NavalCfg Naval = new NavalCfg();
        public VehicleCfg Vehicle = new VehicleCfg();
        public TurretCfg Turret = new TurretCfg();

        public PilotCfg Pilot = new PilotCfg();

        public IconCfg Icons = new IconCfg();

        public ColorCfg Colors = new ColorCfg();

        // Definition of any tags that should result in a flat initiative modifier
        public Dictionary<string, int> PilotTagModifiers = new Dictionary<string, int> {
            { "pilot_morale_high", 2 },
            { "pilot_morale_low", -2 }
        };

        public void Init()
        {
            InitTypeDefaults();
            InitializeColors();
        }

        private void InitTypeDefaults()
        {
            Dictionary<int, int> DefaultInitBaseByTonnage = new Dictionary<int, int>
            {
                {  5, 19 }, // 0-5
                {  15, 18 }, // 10-15
                {  25, 17 }, // 20-25
                {  35, 16 }, // 30-35
                {  45, 15 }, // 40-45
                {  55, 14 }, // 50-55
                {  65, 13 }, // 60-65
                {  75, 12 }, // 70-75
                {  85, 11 }, // 80-85
                {  95, 10 }, // 90-95
                { 100, 9 }, // 100
                { 999, 6 } // 105+
            };

            if (Mod.Config.Mech.InitBaseByTonnage.Count == 0)
                Mod.Config.Mech.InitBaseByTonnage = new Dictionary<int, int>(DefaultInitBaseByTonnage);
            if (Mod.Config.Mech.TypeMod == UnitCfg.DEFAULT_TYPE_MOD)
                Mod.Config.Mech.TypeMod = 0;

            if (Mod.Config.Naval.InitBaseByTonnage.Count == 0)
                Mod.Config.Naval.InitBaseByTonnage = new Dictionary<int, int>(DefaultInitBaseByTonnage);
            if (Mod.Config.Naval.TypeMod == UnitCfg.DEFAULT_TYPE_MOD)
                Mod.Config.Naval.TypeMod = -2;

            if (Mod.Config.Trooper.InitBaseByTonnage.Count == 0)
                Mod.Config.Trooper.InitBaseByTonnage = new Dictionary<int, int>(DefaultInitBaseByTonnage);
            if (Mod.Config.Trooper.TypeMod == UnitCfg.DEFAULT_TYPE_MOD)
                Mod.Config.Trooper.TypeMod = +2;

            if (Mod.Config.Turret.InitBaseByTonnage.Count == 0)
                Mod.Config.Turret.InitBaseByTonnage = new Dictionary<int, int>(DefaultInitBaseByTonnage);
            if (Mod.Config.Turret.TypeMod == UnitCfg.DEFAULT_TYPE_MOD)
                Mod.Config.Turret.TypeMod = -4;

            if (Mod.Config.Vehicle.InitBaseByTonnage.Count == 0)
                Mod.Config.Vehicle.InitBaseByTonnage = new Dictionary<int, int>(DefaultInitBaseByTonnage);
            if (Mod.Config.Vehicle.TypeMod == UnitCfg.DEFAULT_TYPE_MOD)
                Mod.Config.Vehicle.TypeMod = -2;

        }

        private void InitializeColors()
        {
            this.Colors.FriendlyUnactivated = new Color(this.Colors.ColorFriendlyUnactivated[0], this.Colors.ColorFriendlyUnactivated[1], 
                this.Colors.ColorFriendlyUnactivated[2], this.Colors.ColorFriendlyUnactivated[3]);
            this.Colors.FriendlyAlreadyActivated = new Color(this.Colors.ColorFriendlyAlreadyActivated[0], this.Colors.ColorFriendlyAlreadyActivated[1], 
                this.Colors.ColorFriendlyAlreadyActivated[2], this.Colors.ColorFriendlyAlreadyActivated[3]);

            this.Colors.AlliedUnactivated = new Color(this.Colors.ColorAlliedUnactivated[0], this.Colors.ColorAlliedUnactivated[1], 
                this.Colors.ColorAlliedUnactivated[2], this.Colors.ColorAlliedUnactivated[3]);
            this.Colors.AlliedAlreadyActivated = new Color(this.Colors.ColorAlliedAlreadyActivated[0], this.Colors.ColorAlliedAlreadyActivated[1], 
                this.Colors.ColorAlliedAlreadyActivated[2], this.Colors.ColorAlliedAlreadyActivated[3]);

            this.Colors.NeutralUnactivated = new Color(this.Colors.ColorNeutralUnactivated[0], this.Colors.ColorNeutralUnactivated[1], 
                this.Colors.ColorNeutralUnactivated[2], this.Colors.ColorNeutralUnactivated[3]);
            this.Colors.NeutralAlreadyActivated = new Color(this.Colors.ColorNeutralAlreadyActivated[0], this.Colors.ColorNeutralAlreadyActivated[1],
                this.Colors.ColorNeutralAlreadyActivated[2], this.Colors.ColorNeutralAlreadyActivated[3]);

            this.Colors.EnemyUnactivated = new Color(this.Colors.ColorEnemyUnactivated[0], this.Colors.ColorEnemyUnactivated[1],
                this.Colors.ColorEnemyUnactivated[2], this.Colors.ColorEnemyUnactivated[3]);
            this.Colors.EnemyAlreadyActivated = new Color(this.Colors.ColorEnemyAlreadyActivated[0], this.Colors.ColorEnemyAlreadyActivated[1],
                this.Colors.ColorEnemyAlreadyActivated[2], this.Colors.ColorEnemyAlreadyActivated[3]);
        }

        public void LogConfig()
        {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG:{this.Debug} Trace:{this.Trace}");

            Mod.Log.Info?.Write($"  == Pilot Tag Modifiers");
            foreach (KeyValuePair<string, int> kvp in PilotTagModifiers)
            {
                Mod.Log.Info?.Write($"    tag:{kvp.Key} modifier:{kvp.Value}");
            }
            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write($"  == MECH ==");
            Mod.Log.Info?.Write($"  TypeMod: {this.Mech.TypeMod} DefaultTonnage: {this.Mech.DefaultTonnage}" +
                $" CrippledMod (max: {this.Mech.CrippledModifierMax} min: {this.Mech.CrippledModifierMin})" +
                $" Hesitation: (max: {this.Mech.HesitationMax} min: {this.Mech.HesitationMin}) " +
                $" Prone: (max: {this.Mech.ProneModifierMax} min: {this.Mech.ProneModifierMin}) " +
                $" Randomness: (max: {this.Mech.RandomnessMax} min: {this.Mech.RandomnessMin}) " +
                $" Shutdown: (max: {this.Mech.ShutdownModifierMax} min: {this.Mech.ShutdownModifierMin})");
            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write($"  == TROOPER ==");
            Mod.Log.Info?.Write($"  TypeMod: {this.Trooper.TypeMod}");
            Mod.Log.Info?.Write(""); 

            Mod.Log.Info?.Write($"  == VEHICLE ==");
            Mod.Log.Info?.Write($"  TypeMod: {this.Vehicle.TypeMod}" +
                $" CrippledMod (max: {this.Mech.CrippledModifierMax} min: {this.Mech.CrippledModifierMin})"
                );

            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write($"  == NAVAL ==");
            Mod.Log.Info?.Write($"  TypeMod: {this.Vehicle.TypeMod}" +
                $" CrippledMod (max: {this.Mech.CrippledModifierMax} min: {this.Mech.CrippledModifierMin})"
                );
            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write($"  == TURRET ==");
            Mod.Log.Info?.Write($"  TypeMod: {this.Turret.TypeMod}  " +
                $"Tonnages (Light:{this.Turret.LightTonnage}  Medium:{this.Turret.MediumTonnage}  Heavy:{this.Turret.HeavyTonnage}  Default:{this.Turret.DefaultTonnage}) ");
            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write("=== MOD CONFIG END ===");
        }
    }
}
