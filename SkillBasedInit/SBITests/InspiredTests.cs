using BattleTech;
using Harmony;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using SkillBasedInit.Helper;
using System;

namespace SBITests
{
    [TestClass]
    public class InspiredTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysTrue);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysTrue);

            Mod.Config.Mech.TypeMod = 0;

            Mod.Config.Mech.InspiredMax = 3;
            Mod.Config.Mech.InspiredMin = 1;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsMoraleInspired, HarmonyPatchType.Prefix);
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsFuryInspired, HarmonyPatchType.Prefix);
        }


        // TODO: FIX ME
        [TestMethod]
        public void TestInspired()
        {
            Mech mech50 = TestHelper.BuildTestMech(tonnage: 50);
            mech50.GetPilot().StatCollection.Set<int>("Tactics", 1);
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.GetPilot().InspiredModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= -1);
                Assert.IsTrue(mod >= -3);
            }

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 3);
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.GetPilot().InspiredModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= -2);
                Assert.IsTrue(mod >= -4);
            }

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 5);
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.GetPilot().InspiredModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= -3);
                Assert.IsTrue(mod >= -5);
            }

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 7);
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.GetPilot().InspiredModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= -4);
                Assert.IsTrue(mod >= -6);
            }

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 9);
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.GetPilot().InspiredModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= -5);
                Assert.IsTrue(mod >= -7);
            }

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 10);
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.GetPilot().InspiredModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= -6);
                Assert.IsTrue(mod >= -8);
            }
        }

    }

}
