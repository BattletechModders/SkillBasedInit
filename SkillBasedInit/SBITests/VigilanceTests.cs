using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using SkillBasedInit.Helper;
using System;

namespace SBITests
{
    [TestClass]
    public class VigilanceTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            // Patch isMoraleInspired to return true
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysFalse);

            Mod.Config.Mech.TypeMod = 0;

            Mod.Config.Mech.VigilanceRandMax = 6;
            Mod.Config.Mech.VigilanceRandMin = 2;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsFuryInspired, HarmonyPatchType.Prefix);
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsMoraleInspired, HarmonyPatchType.Prefix);
        }


        [TestMethod]
        public void TestCalledShotBounds()
        {

            Mech mech = TestHelper.BuildTestMech(tonnage: 50);
            mech.GetPilot().StatCollection.Set<int>("Guts", 1);
            mech.GetPilot().StatCollection.Set<int>("Tactics", 1);
            mech.StatCollection.Set<int>(ModStats.MOD_VIGILANCE, 0);
            // (guts 0 + tacMod 0) => avg 0, mod 0

            // Range should be min, max + target 0 + attacker 0
            Console.WriteLine($"Expected: -2 to -6");
            for (int i = 0; i < 30; i++)
            {
                int vigilanceMod = mech.VigilanceInitMod();
                Console.WriteLine($"VigilanceInitMod: {vigilanceMod}");
                Assert.IsTrue(vigilanceMod >= -6);
                Assert.IsTrue(vigilanceMod <= -2);
            }
            Console.WriteLine($"");

            Mod.Config.Mech.VigilanceRandMax = 8;
            Mod.Config.Mech.VigilanceRandMin = 6;
            Console.WriteLine($"Expected: -6 to -8");
            for (int i = 0; i < 30; i++)
            {
                int vigilanceMod = mech.VigilanceInitMod();
                Console.WriteLine($"VigilanceInitMod: {vigilanceMod}");
                Assert.IsTrue(vigilanceMod >= -8);
                Assert.IsTrue(vigilanceMod <= -6);
            }
            Console.WriteLine($"");

            Mod.Config.Mech.VigilanceRandMax = 3;
            Mod.Config.Mech.VigilanceRandMin = 1;
            Console.WriteLine($"Expected: -1 to -3");
            for (int i = 0; i < 30; i++)
            {
                int vigilanceMod = mech.VigilanceInitMod();
                Console.WriteLine($"VigilanceInitMod: {vigilanceMod}");
                Assert.IsTrue(vigilanceMod >= -3);
                Assert.IsTrue(vigilanceMod <= -1);
            }
            Console.WriteLine($"");

        }

        [TestMethod]
        public void TestPilotSkills()
        {

            Mech mech = TestHelper.BuildTestMech(tonnage: 50);
            mech.GetPilot().StatCollection.Set<int>("Guts", 1);
            mech.GetPilot().StatCollection.Set<int>("Tactics", 1);
            mech.StatCollection.Set<int>(ModStats.MOD_VIGILANCE, 0);
            // (guts 0 + tacMod 0) => avg 0, mod 0

            // Range should be min, max + target 0 + attacker 0
            mech.GetPilot().StatCollection.Set<int>("Guts", 4);
            mech.GetPilot().StatCollection.Set<int>("Tactics", 6);
            // (guts 2 + tacMod 3) => avg 3, mod 0
            Console.WriteLine($"Expected: -5 to -9");
            for (int i = 0; i < 30; i++)
            {
                int vigilanceMod = mech.VigilanceInitMod();
                Console.WriteLine($"VigilanceInitMod: {vigilanceMod}");
                Assert.IsTrue(vigilanceMod >= -9);
                Assert.IsTrue(vigilanceMod <= -5);
            }
            Console.WriteLine($"");

            mech.GetPilot().StatCollection.Set<int>("Guts", 8);
            mech.GetPilot().StatCollection.Set<int>("Tactics", 6);
            // (guts 4 + tacMod 3) => avg 4, mod 0
            Console.WriteLine($"Expected: -6 to -10");
            for (int i = 0; i < 30; i++)
            {
                int vigilanceMod = mech.VigilanceInitMod();
                Console.WriteLine($"VigilanceInitMod: {vigilanceMod}");
                Assert.IsTrue(vigilanceMod >= -10);
                Assert.IsTrue(vigilanceMod <= -6);
            }
            Console.WriteLine($"");

            mech.GetPilot().StatCollection.Set<int>("Guts", 10);
            mech.GetPilot().StatCollection.Set<int>("Tactics", 10);
            // (guts 5 + tacMod 5) => avg 5, mod 0
            Console.WriteLine($"Expected: -7 to -11");
            for (int i = 0; i < 30; i++)
            {
                int vigilanceMod = mech.VigilanceInitMod();
                Console.WriteLine($"VigilanceInitMod: {vigilanceMod}");
                Assert.IsTrue(vigilanceMod >= -11);
                Assert.IsTrue(vigilanceMod <= -7);
            }
            Console.WriteLine($"");

        }

        [TestMethod]
        public void TestPilotMod()
        {

            Mech mech = TestHelper.BuildTestMech(tonnage: 50);
            mech.GetPilot().StatCollection.Set<int>("Guts", 1);
            mech.GetPilot().StatCollection.Set<int>("Tactics", 1);
            mech.StatCollection.Set<int>(ModStats.MOD_VIGILANCE, 0);
            // (guts 0 + tacMod 0) => avg 0, mod 0

            mech.StatCollection.Set<int>(ModStats.MOD_VIGILANCE, 2);
            Console.WriteLine($"Expected: -4 to -8");
            for (int i = 0; i < 30; i++)
            {
                int vigilanceMod = mech.VigilanceInitMod();
                Console.WriteLine($"VigilanceInitMod: {vigilanceMod}");
                Assert.IsTrue(vigilanceMod >= -8);
                Assert.IsTrue(vigilanceMod <= -4);
            }
            Console.WriteLine($"");

            mech.StatCollection.Set<int>(ModStats.MOD_VIGILANCE, -4);
            Console.WriteLine($"Expected: -0 to -2");
            for (int i = 0; i < 30; i++)
            {
                int vigilanceMod = mech.VigilanceInitMod();
                Console.WriteLine($"VigilanceInitMod: {vigilanceMod}");
                Assert.IsTrue(vigilanceMod >= -2);
                Assert.IsTrue(vigilanceMod <= 0);
            }
            Console.WriteLine($"");

        }

    }
}
