using BattleTech;
using Harmony;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using SkillBasedInit.Helper;
using System;

namespace SBITests
{
    [TestClass]
    public class ProneTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsProne, prefix: TestGlobalInit.HM_AlwaysTrue);

            Mod.Config.Mech.TypeMod = 0;

            Mod.Config.Mech.ProneModifierMax = -6;
            Mod.Config.Mech.ProneModifierMin = -2;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsProne, HarmonyPatchType.Prefix);
        }

        [TestMethod]
        public void TestProne()
        {

            Mech attacker = TestHelper.BuildTestMech(tonnage: 50);
            attacker.GetPilot().StatCollection.Set<int>("Piloting", 1);
            attacker.GetPilot().StatCollection.Set<int>(ModStats.MOD_SKILL_PILOT, 0);

            // Range should be min, max - 0
            Console.WriteLine("Expected value: [2,6]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ProneInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 2);
                Assert.IsTrue(mod <= 6);
            }

            attacker.GetPilot().StatCollection.Set<int>("Piloting", 4);
            // Range should be min, max - 2
            Console.WriteLine("Expected value: [0,4]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ShutdownInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 0);
                Assert.IsTrue(mod <= 4);
            }

            attacker.GetPilot().StatCollection.Set<int>("Piloting", 6);
            // Range should be min, max - 3
            Console.WriteLine("Expected value: [0,3]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ShutdownInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 0);
                Assert.IsTrue(mod <= 3);
            }

            attacker.GetPilot().StatCollection.Set<int>("Piloting", 9);
            // Range should be min, max - 4
            Console.WriteLine("Expected value: [0,2]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ShutdownInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 0);
                Assert.IsTrue(mod <= 2);
            }

            attacker.GetPilot().StatCollection.Set<int>("Piloting", 10);
            // Range should be min, max - 5
            Console.WriteLine("Expected value: [0,1]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ShutdownInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 0);
                Assert.IsTrue(mod <= 1);
            }
        }

        [TestMethod]
        public void TestProneMods()
        {

            Mech attacker = TestHelper.BuildTestMech(tonnage: 50);
            attacker.GetPilot().StatCollection.Set<int>("Piloting", 1);
            attacker.GetPilot().StatCollection.Set<int>(ModStats.MOD_SKILL_PILOT, 0);

            // Range should be min, max - 0
            Console.WriteLine("Expected value: [2,6]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ProneInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 2);
                Assert.IsTrue(mod <= 6);
            }

            attacker.GetPilot().StatCollection.Set<int>(ModStats.MOD_SKILL_PILOT, 2);
            // Range should be min, max - 2
            Console.WriteLine("Expected value: [0,4]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ProneInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 0);
                Assert.IsTrue(mod <= 4);
            }

            attacker.GetPilot().StatCollection.Set<int>(ModStats.MOD_SKILL_PILOT, -2);
            // Range should be min, max - (0 + 2)
            Console.WriteLine("Expected value: [2,6]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ProneInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 4);
                Assert.IsTrue(mod <= 8);
            }

            attacker.GetPilot().StatCollection.Set<int>("Piloting", 10);
            attacker.GetPilot().StatCollection.Set<int>(ModStats.MOD_SKILL_PILOT, -2);
            // Range should be min, max - 3
            Console.WriteLine("Expected value: [0,3]");
            for (int i = 0; i < 30; i++)
            {
                int mod = attacker.ProneInitMod();
                Console.WriteLine($"ProneMod: {mod}");
                Assert.IsTrue(mod >= 0);
                Assert.IsTrue(mod <= 3);
            }


        }
    }


}
