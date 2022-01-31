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
    [TestClass]
    public static class TestGlobalInit
    {
        [AssemblyInitialize]
        public static void TestInitialize(TestContext testContext)
        {
            Mod.Log = new DeferringLogger(testContext.TestResultsDirectory,
                "SBI_Tests", "SBITEST", true, true);

            Mod.Config = new ModConfig();

            CUSettings testCUSettings = new CustomUnits.CUSettings();
            testCUSettings.PartialMovementOnlyWalkByDefault = false;
            testCUSettings.AllowRotateWhileJumpByDefault = false;
            CustomUnits.Core.Settings = testCUSettings;

        }

        [AssemblyCleanup]
        public static void TearDown()
        {

        }
    }
}
