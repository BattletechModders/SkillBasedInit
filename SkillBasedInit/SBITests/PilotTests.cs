using BattleTech;
using Harmony;
using IRBTModUtils.Extension;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using SkillBasedInit.Helper;
using System;

namespace SBITests
{
    [TestClass]
    public class PilotTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            // Patch isMoraleInspired to return true
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysFalse);


            Mod.Config.Mech.TypeMod = 0;

            Mod.Config.Mech.CalledShotRandMax = -6;
            Mod.Config.Mech.CalledShotRandMin = -2;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsFuryInspired, HarmonyPatchType.Prefix);
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsMoraleInspired, HarmonyPatchType.Prefix);
        }


        [TestMethod]
        public void TestAvgGunneryAndTactics()
        {
            Pilot pilot = TestHelper.BuildTestPilot();
            pilot.StatCollection.Set<int>("Gunnery", 1);
            pilot.StatCollection.Set<int>("Tactics", 1);
            // guts 0 + tactics 0 => 0
            Assert.AreEqual(0, pilot.AverageGunneryAndTacticsMod());

            pilot.StatCollection.Set<int>("Gunnery", 1);
            pilot.StatCollection.Set<int>("Tactics", 10);
            // guts 0 + tactics 5 => 3
            Assert.AreEqual(3, pilot.AverageGunneryAndTacticsMod());

            pilot.StatCollection.Set<int>("Gunnery", 10);
            pilot.StatCollection.Set<int>("Tactics", 1);
            // guts 5 + tactics 0 => 3
            Assert.AreEqual(3, pilot.AverageGunneryAndTacticsMod());

            pilot.StatCollection.Set<int>("Gunnery", 10);
            pilot.StatCollection.Set<int>("Tactics", 10);
            // guts 5 + tactics 5 => 5
            Assert.AreEqual(5, pilot.AverageGunneryAndTacticsMod());

        }

    }


}
