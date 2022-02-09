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

            Mod.Config.Pilot.PilotTagModifiers.Add("pilot_morale_high", +2);
            Mod.Config.Pilot.PilotTagModifiers.Add("pilot_morale_low", -2);

            Mod.Config.Pilot.PilotTagModifiers.Add("pilot_reckless", 1);
            Mod.Config.Pilot.PilotTagModifiers.Add("pilot_lucky", 1);
            Mod.Config.Pilot.PilotTagModifiers.Add("pilot_cautious", -1);
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

        [TestMethod]
        public void TestPilotTags()
        {
            Pilot pilot = TestHelper.BuildTestPilot();
            Assert.AreEqual(0, PilotHelper.GetTagsModifier(pilot));

            pilot.pilotDef.PilotTags.Add("pilot_morale_high");
            Assert.AreEqual(-2, PilotHelper.GetTagsModifier(pilot));
            pilot.pilotDef.PilotTags.Remove("pilot_morale_high");

            pilot.pilotDef.PilotTags.Add("pilot_morale_low");
            Assert.AreEqual(2, PilotHelper.GetTagsModifier(pilot));
            pilot.pilotDef.PilotTags.Remove("pilot_morale_low");

            pilot.pilotDef.PilotTags.Add("pilot_morale_high");
            pilot.pilotDef.PilotTags.Add("pilot_reckless");
            pilot.pilotDef.PilotTags.Add("pilot_lucky");
            Assert.AreEqual(-4, PilotHelper.GetTagsModifier(pilot));
            pilot.pilotDef.PilotTags.Remove("pilot_morale_high");
            pilot.pilotDef.PilotTags.Remove("pilot_reckless");
            pilot.pilotDef.PilotTags.Remove("pilot_lucky");

            pilot.pilotDef.PilotTags.Add("pilot_morale_low");
            pilot.pilotDef.PilotTags.Add("pilot_cautious");
            Assert.AreEqual(3, PilotHelper.GetTagsModifier(pilot));
            pilot.pilotDef.PilotTags.Remove("pilot_morale_low");
            pilot.pilotDef.PilotTags.Remove("pilot_cautious");

            pilot.pilotDef.PilotTags.Add("pilot_morale_low");
            pilot.pilotDef.PilotTags.Add("pilot_morale_high");
            pilot.pilotDef.PilotTags.Add("pilot_reckless");
            pilot.pilotDef.PilotTags.Add("pilot_lucky");
            pilot.pilotDef.PilotTags.Add("pilot_cautious");
            Assert.AreEqual(-1, PilotHelper.GetTagsModifier(pilot));

        }
    }


}
