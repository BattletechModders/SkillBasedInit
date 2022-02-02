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
    public class CombinedTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            // Patch isMoraleInspired to return true


            Mod.Config.Mech.CrippledModifierMax = -6;
            Mod.Config.Mech.CrippledModifierMin = -2;
            Mod.Config.Mech.InspiredMax = 3;
            Mod.Config.Mech.InspiredMin = 1;
            Mod.Config.Mech.ProneModifierMax = -6;
            Mod.Config.Mech.ProneModifierMin = -2;
            Mod.Config.Mech.RandomnessMax = -4;
            Mod.Config.Mech.RandomnessMin = -1;
            Mod.Config.Mech.ShutdownModifierMax = -6;
            Mod.Config.Mech.ShutdownModifierMin = -2;

            Mod.Config.Mech.TypeMod = 0;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsFuryInspired, HarmonyPatchType.Prefix);
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsMoraleInspired, HarmonyPatchType.Prefix);
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsLegged, HarmonyPatchType.Prefix);
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsProne, HarmonyPatchType.Prefix);
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsShutdown, HarmonyPatchType.Prefix);
        }
        

        [TestMethod]
        public void TestCombined_NoState()
        {
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsLegged, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsProne, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsShutdown, prefix: TestGlobalInit.HM_AlwaysFalse);

            Mod.Config.Mech.TypeMod = 2;
            Mech mech30 = TestHelper.BuildTestMech(tonnage: 30);

            mech30.StatCollection.Set<int>(ModStats.MOD_INJURY, 2);
            mech30.StatCollection.Set<int>(ModStats.MOD_MISC, 2);
            mech30.GetPilot().StatCollection.Set<int>("Tactics", 10);

            //Tonnage = phase 16 => init 15
            // type => -2
            // tactics => -5
            // injury => -2
            // misc => -2
            // random => +[1,4]
            // ==> 4 + [1,4]
            Console.WriteLine("Combined test 1");
            for (int i = 0; i < 30; i++)
            {
                InitiativeHelper.UpdateInitiative(mech30);
                Console.WriteLine($"Mech init: {mech30.Initiative}");
                Assert.IsTrue(mech30.Initiative >= 5);
                Assert.IsTrue(mech30.Initiative <= 8);
            }
        }

        [TestMethod]
        public void TestCombined_NoMods()
        {
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsLegged, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsProne, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsShutdown, prefix: TestGlobalInit.HM_AlwaysFalse);

            Mod.Config.Mech.TypeMod = 2;
            Mech mech30 = TestHelper.BuildTestMech(tonnage: 30);

            mech30.StatCollection.Set<int>(ModStats.MOD_INJURY, 0);
            mech30.StatCollection.Set<int>(ModStats.MOD_MISC, 0);
            mech30.GetPilot().StatCollection.Set<int>("Tactics", 4);

            //Tonnage = phase 16 => init 15
            // type => -2
            // tactics => -2
            // injury => 0
            // misc => 0
            // random => +[1,4]
            // ==> 11 + [1,4]
            Console.WriteLine("Combined test 1");
            for (int i = 0; i < 30; i++)
            {
                InitiativeHelper.UpdateInitiative(mech30);
                Console.WriteLine($"Mech init: {mech30.Initiative}");
                Assert.IsTrue(mech30.Initiative >= 12);
                Assert.IsTrue(mech30.Initiative <= 15);
            }
        }

        [TestMethod]
        public void TestCombined_Inspired()
        {
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysTrue);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsLegged, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsProne, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsShutdown, prefix: TestGlobalInit.HM_AlwaysFalse);

            Mod.Config.Mech.TypeMod = 2;
            Mech mech30 = TestHelper.BuildTestMech(tonnage: 30);

            mech30.StatCollection.Set<int>(ModStats.MOD_INJURY, 2);
            mech30.StatCollection.Set<int>(ModStats.MOD_MISC, 2);
            mech30.GetPilot().StatCollection.Set<int>("Tactics", 10);

            //Tonnage = phase 16 => init 15
            // type => -2
            // tactics => -5
            // injury => -2
            // misc => -2
            // random => +[1,4]
            // inspired => -[1,3] + tactics: 5 => -[6,8]
            // ==> 4 + [1,4] - [6,8]
            Console.WriteLine("Combined test 1");
            for (int i = 0; i < 30; i++)
            {
                InitiativeHelper.UpdateInitiative(mech30);
                Console.WriteLine($"Mech init: {mech30.Initiative}");
                Assert.IsTrue(mech30.Initiative >= 0);
                Assert.IsTrue(mech30.Initiative <= 2);
            }
        }

        [TestMethod]
        public void TestCombined_Hesitation()
        {
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsLegged, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsProne, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsShutdown, prefix: TestGlobalInit.HM_AlwaysFalse);

            Mod.Config.Mech.TypeMod = 2;
            Mech mech30 = TestHelper.BuildTestMech(tonnage: 30);
            mech30.GetPilot().StatCollection.Set<int>("Tactics", 10);

            mech30.StatCollection.Set<int>(ModStats.MOD_INJURY, 0);
            mech30.StatCollection.Set<int>(ModStats.MOD_MISC, 0);

            //Tonnage = phase 16 => init 15
            // type => -2
            // tactics => -5
            // hesitation => 8 - (tactics 5 + 1) => 2
            // random => +[1,4]
            // ==> 10 + [1,4] - [11,14]
            Console.WriteLine("Combined test 1");
            for (int i = 0; i < 30; i++)
            {
                // Hesitation
                mech30.StatCollection.Set<int>(ModStats.STATE_HESITATION, 8);
                InitiativeHelper.UpdateInitiative(mech30);

                Console.WriteLine($"Mech init: {mech30.Initiative}");
                Assert.IsTrue(mech30.Initiative >= 11);
                Assert.IsTrue(mech30.Initiative <= 14);
            }
        }

        [TestMethod]
        public void TestCombined_Hesitation_Reduction()
        {
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsLegged, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsProne, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsShutdown, prefix: TestGlobalInit.HM_AlwaysFalse);

            Mod.Config.Mech.TypeMod = 2;
            Mech mech30 = TestHelper.BuildTestMech(tonnage: 30);
            mech30.GetPilot().StatCollection.Set<int>("Tactics", 1);

            mech30.StatCollection.Set<int>(ModStats.MOD_INJURY, 0);
            mech30.StatCollection.Set<int>(ModStats.MOD_MISC, 0);

            mech30.StatCollection.Set<int>(ModStats.STATE_HESITATION, 8);

            //Tonnage = phase 16 => init 15
            // type => -2
            // tactics => -0
            // hesitation => 8 - (tactics 0 + 1) => 7
            // random => +[1,4]
            // ==> 20 + [1,4] - [21,25]
            InitiativeHelper.UpdateInitiative(mech30);
            Console.WriteLine($"Mech init: {mech30.Initiative}");
            Assert.IsTrue(mech30.Initiative >= 21);
            Assert.IsTrue(mech30.Initiative <= 25);

            // hestiation => 7 - (tactics 0 + 1) => 6
            InitiativeHelper.UpdateInitiative(mech30);
            Console.WriteLine($"Mech init: {mech30.Initiative}");
            Assert.IsTrue(mech30.Initiative >= 20);
            Assert.IsTrue(mech30.Initiative <= 24);

            // hestiation => 6 - (tactics 0 + 1) => 5
            InitiativeHelper.UpdateInitiative(mech30);
            Console.WriteLine($"Mech init: {mech30.Initiative}");
            Assert.IsTrue(mech30.Initiative >= 19);
            Assert.IsTrue(mech30.Initiative <= 23);

            // hestiation => 5 - (tactics 0 + 1) => 4
            InitiativeHelper.UpdateInitiative(mech30);
            Console.WriteLine($"Mech init: {mech30.Initiative}");
            Assert.IsTrue(mech30.Initiative >= 18);
            Assert.IsTrue(mech30.Initiative <= 22);
        }
    }


}
