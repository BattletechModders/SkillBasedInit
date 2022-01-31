using BattleTech;
using Harmony;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using SkillBasedInit.Helper;
using System;

namespace SBITests
{
    [TestClass]
    public class CalledShotTests
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
        public void TestCalledShotBounds()
        {

            Mech attacker = TestHelper.BuildTestMech(tonnage: 50);
            attacker.GetPilot().StatCollection.Set<int>("Gunnery", 1);
            attacker.GetPilot().StatCollection.Set<int>("Tactics", 1);
            attacker.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_ATTACKER, 0);
            // (gunMod 0 + tacMod 0) => avg 0, mod 0

            Mech target = TestHelper.BuildTestMech(tonnage: 50);
            target.GetPilot().StatCollection.Set<int>("Guts", 1);
            target.GetPilot().StatCollection.Set<int>("Tactics", 1);
            target.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_TARGET, 0);
            // (gunMod 0 + tacMod 0) => avg 0, mod 0

            // Range should be min, max + target 0 + attacker 0
            for (int i = 0; i < 30; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 2);
                Assert.IsTrue(calledShotMod <= 6);
            }

            Mod.Config.Mech.CalledShotRandMax = -10;
            Mod.Config.Mech.CalledShotRandMin = -3;
            // Range should be min, max + target 0 + attacker 0
            for (int i = 0; i < 30; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 3);
                Assert.IsTrue(calledShotMod <= 10);
            }

        }

        [TestMethod]
        public void TestAttackerBiasSkills()
        {
            Mod.Config.Mech.CalledShotRandMax = -6;
            Mod.Config.Mech.CalledShotRandMin = -2;

            Mech attacker = TestHelper.BuildTestMech(tonnage: 50);
            attacker.GetPilot().StatCollection.Set<int>("Gunnery", 1);
            attacker.GetPilot().StatCollection.Set<int>("Tactics", 1);
            attacker.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_ATTACKER, 0);
            // (gunMod 0 + tacMod 0) => avg 0, mod 0

            Mech target = TestHelper.BuildTestMech(tonnage: 50);
            target.GetPilot().StatCollection.Set<int>("Guts", 1);
            target.GetPilot().StatCollection.Set<int>("Tactics", 1);
            target.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_TARGET, 0);
            // (gunMod 0 + tacMod 0) => avg 0, mod 0

            // Range should be min, max + target 0 + attacker 0
            Console.WriteLine($"Attacker bias: 0");
            for (int i = 0; i < 30; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 2);
                Assert.IsTrue(calledShotMod <= 6);
            }
            Console.WriteLine($"");

            attacker.GetPilot().StatCollection.Set<int>("Gunnery", 1);
            attacker.GetPilot().StatCollection.Set<int>("Tactics", 10);
            // (gunMod 0 + tacMod 5) => avg 3, mod 0
            // Range should be min, max, + (attacker 3 - target 0)
            Console.WriteLine($"Attacker bias: 3");
            for (int i = 0; i < 100; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 5);
                Assert.IsTrue(calledShotMod <= 9);
            }
            Console.WriteLine($"");

            attacker.GetPilot().StatCollection.Set<int>("Gunnery", 10);
            attacker.GetPilot().StatCollection.Set<int>("Tactics", 10);
            // (gunMod 5 + tacMod 5) => avg 5, mod 0
            // Range should be min, max, + (attacker 3 - target 0)
            Console.WriteLine($"Attacker bias: 3");
            for (int i = 0; i < 100; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 7);
                Assert.IsTrue(calledShotMod <= 11);
            }
            Console.WriteLine($"");

            attacker.GetPilot().StatCollection.Set<int>("Gunnery", 1);
            attacker.GetPilot().StatCollection.Set<int>("Tactics", 1);
            attacker.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_ATTACKER, -3);
            // (gunMod 0 + tacMod 0) => avg , mod 3
            // Range should be min, max, + (attacker 3 - target 0)
            Console.WriteLine($"Attacker bias: 3");
            for (int i = 0; i < 100; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 5);
                Assert.IsTrue(calledShotMod <= 9);
            }
            Console.WriteLine($"");

        }

        [TestMethod]
        public void TestAttackerBiasMods()
        {
            Mod.Config.Mech.CalledShotRandMax = -6;
            Mod.Config.Mech.CalledShotRandMin = -2;

            Mech attacker = TestHelper.BuildTestMech(tonnage: 50);
            attacker.GetPilot().StatCollection.Set<int>("Gunnery", 1);
            attacker.GetPilot().StatCollection.Set<int>("Tactics", 1);
            attacker.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_ATTACKER, 0);
            // (gunMod 0 + tacMod 0) => avg 0, mod 0

            Mech target = TestHelper.BuildTestMech(tonnage: 50);
            target.GetPilot().StatCollection.Set<int>("Guts", 1);
            target.GetPilot().StatCollection.Set<int>("Tactics", 1);
            target.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_TARGET, 0);
            // (gunMod 0 + tacMod 0) => avg 0, mod 0

            // Attacker mod is interpreted as a phase modifier, so inverted
            attacker.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_ATTACKER, -3);
            // (gunMod 0 + tacMod 0) => avg , mod 3
            // Range should be min, max, + (attacker 3 - target 0)
            Console.WriteLine($"Attacker bias: -3");
            for (int i = 0; i < 100; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 5);
                Assert.IsTrue(calledShotMod <= 9);
            }
            Console.WriteLine($"");

            attacker.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_ATTACKER, 3);
            // (gunMod 0 + tacMod 0) => avg , mod -3
            // Range should be min, max, + (attacker -3 - target 0) => 0
            Console.WriteLine($"Attacker bias: 3");
            for (int i = 0; i < 100; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 2);
                Assert.IsTrue(calledShotMod <= 6);
            }
            Console.WriteLine($"");
        }

        [TestMethod]
        public void TestTargetBias()
        {
            Mod.Config.Mech.CalledShotRandMax = -6;
            Mod.Config.Mech.CalledShotRandMin = -2;

            Mech attacker = TestHelper.BuildTestMech(tonnage: 50);
            attacker.GetPilot().StatCollection.Set<int>("Gunnery", 10);
            attacker.GetPilot().StatCollection.Set<int>("Tactics", 10);
            attacker.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_ATTACKER, 0);
            // (gunMod 0 + tacMod 0) => avg 0, mod 0

            Mech target = TestHelper.BuildTestMech(tonnage: 50);
            target.GetPilot().StatCollection.Set<int>("Guts", 1);
            target.GetPilot().StatCollection.Set<int>("Tactics", 1);
            target.StatCollection.Set<int>(ModStats.MOD_CALLED_SHOT_TARGET, 0);
            // (gunMod 0 + tacMod 0) => avg 0, mod 0

            // Range should be min, max + target 0 + attacker 0
            Console.WriteLine($"Attacker bias: 5");
            for (int i = 0; i < 30; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 7);
                Assert.IsTrue(calledShotMod <= 11);
            }
            Console.WriteLine($"");

            target.GetPilot().StatCollection.Set<int>("Guts", 6);
            target.GetPilot().StatCollection.Set<int>("Tactics", 6);
            // (gunMod 3 + tacMod 3) => avg 3, mod 0
            // Range should be min, max, + (attacker 5 - target 3)
            Console.WriteLine($"Attacker bias: 2");
            for (int i = 0; i < 100; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 4);
                Assert.IsTrue(calledShotMod <= 8);
            }
            Console.WriteLine($"");

            target.GetPilot().StatCollection.Set<int>("Guts", 10);
            target.GetPilot().StatCollection.Set<int>("Tactics", 10);
            // (gunMod 5 + tacMod 5) => avg 5, mod 0
            // Range should be min, max, + (attacker 3 - target 5)
            Console.WriteLine($"Attacker bias: 0");
            for (int i = 0; i < 100; i++)
            {
                int calledShotMod = target.CalledShotInitMod(attacker);
                Console.WriteLine($"CalledShotMod: {calledShotMod}");
                Assert.IsTrue(calledShotMod >= 2);
                Assert.IsTrue(calledShotMod <= 6);
            }
            Console.WriteLine($"");
        }
    }


}
