using CustomUnits;
using IRBTModUtils.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBITests
{
    public static class TestConsts
    {
        public const int FirstPhase = 1;
        public const int LastPhase = 30;
        public static UnitCfg DefaultUnitCfg = new UnitCfg()
        {
            TypeMod = 0,
            RandomnessMax = 0,
            RandomnessMin = 0,
            HesitationMax = -6,
            HesitationMin = -2,
            CalledShotRandMax = -6,
            CalledShotRandMin = -2,
            VigilanceRandMax = 6,
            VigilanceRandMin = 2,
            InspiredMax = 3,
            InspiredMin = 1,
            ProneModifierMax = 6,
            ProneModifierMin = 2,
            ShutdownModifierMax = 6,
            ShutdownModifierMin = 2,
            CrippledModifierMax = 6,
            CrippledModifierMin = 2,
            DefaultTonnage = 120f,
            InitBaseByTonnage = new Dictionary<int, int>
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
                }
        };
        public static TurretCfg DefaultTurretCfg = new TurretCfg()
        {
            TypeMod = 0,
            RandomnessMax = 0,
            RandomnessMin = 05,
            HesitationMax = -6,
            HesitationMin = -2,
            CalledShotRandMax = -6,
            CalledShotRandMin = -2,
            VigilanceRandMax = 6,
            VigilanceRandMin = 2,
            InspiredMax = 3,
            InspiredMin = 1,
            ProneModifierMax = 6,
            ProneModifierMin = 2,
            ShutdownModifierMax = 6,
            ShutdownModifierMin = 2,
            CrippledModifierMax = 6,
            CrippledModifierMin = 2,
            DefaultTonnage = 120f,
            LightTonnage = 60f,
            MediumTonnage = 80f,
            HeavyTonnage = 100f,
            InitBaseByTonnage = new Dictionary<int, int>
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
                }
        };

    }

    [TestClass]
    public static class TestGlobalInit
    {
        [AssemblyInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            Mod.Log = new DeferringLogger(testContext.TestResultsDirectory,
                "SBI_Tests", "SBITEST", true, true);

            Mod.Config = new ModConfig();

            Mod.Config.Mech = TestConsts.DefaultUnitCfg.ShallowCopy();
            Mod.Config.Mech.TypeMod = 1;

            Mod.Config.Trooper = TestConsts.DefaultUnitCfg.ShallowCopy();
            Mod.Config.Trooper.TypeMod = 2;

            Mod.Config.Naval = TestConsts.DefaultUnitCfg.ShallowCopy();
            Mod.Config.Naval.TypeMod = 3;

            Mod.Config.Vehicle = TestConsts.DefaultUnitCfg.ShallowCopy();
            Mod.Config.Vehicle.TypeMod = 4;

            Mod.Config.Turret = TestConsts.DefaultTurretCfg;
            Mod.Config.Turret.TypeMod = 5;

            CUSettings testCUSettings = new CustomUnits.CUSettings();
            testCUSettings.PartialMovementOnlyWalkByDefault = false;
            testCUSettings.AllowRotateWhileJumpByDefault = false;
            CustomUnits.Core.Settings = testCUSettings;

            IRBTModUtils.Mod.Log = new DeferringLogger(testContext.TestResultsDirectory,
                "IRBTMODUTIL_Tests", "IRBTMODUTILTEST", false, false);

            IRBTModUtils.Mod.Config = new IRBTModUtils.ModConfig();
            IRBTModUtils.Mod.Config.Init();
        }

        [AssemblyCleanup]
        public static void TearDown()
        {

        }
    }
}
