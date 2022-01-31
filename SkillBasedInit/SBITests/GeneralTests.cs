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
    public class GeneralTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            // Patch isMoraleInspired to return true
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsFuryInspired, prefix: TestGlobalInit.HM_AlwaysFalse);
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsMoraleInspired, prefix: TestGlobalInit.HM_AlwaysFalse);

            Mod.Config.Mech.TypeMod = 0;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsFuryInspired, HarmonyPatchType.Prefix);
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsMoraleInspired, HarmonyPatchType.Prefix);
        }
        

        [TestMethod]
        public void TestTonnageMod()
        {
            Mech mech5 = TestHelper.BuildTestMech(tonnage: 5);
            InitiativeHelper.UpdateInitiative(mech5);
            // Tonnage = phase 19 => init 12, +0 type, -0 tactics, +0 random => 12 init
            Assert.AreEqual(12, mech5.Initiative);

            Mech mech30 = TestHelper.BuildTestMech(tonnage: 30);
            InitiativeHelper.UpdateInitiative(mech30);
            // Tonnage = phase 16 => init 15, +0 type, -0 tactics, +0 random => 15 init
            Assert.AreEqual(15, mech30.Initiative);

            Mech mech50 = TestHelper.BuildTestMech(tonnage: 50);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, +0 type, -0 tactics, +0 random => 17 init
            Assert.AreEqual(17, mech50.Initiative);

            Mech mech75 = TestHelper.BuildTestMech(tonnage: 75);
            InitiativeHelper.UpdateInitiative(mech75);
            // Tonnage = phase 12 => init 19, +0 type, -0 tactics, +0 random => 19 init
            Assert.AreEqual(19, mech75.Initiative);

            Mech mech100 = TestHelper.BuildTestMech(tonnage: 100);
            InitiativeHelper.UpdateInitiative(mech100);
            // Tonnage = phase 9 => init 22, +0 type, -0 tactics, +0 random => 22 init
            Assert.AreEqual(22, mech100.Initiative);

            Mech mech150 = TestHelper.BuildTestMech(tonnage: 150);
            InitiativeHelper.UpdateInitiative(mech150);
            // Tonnage = phase 6 => init 25, +0 type, -0 tactics, +0 random => 25 init
            Assert.AreEqual(25, mech150.Initiative);
        }

        [TestMethod]
        public void TestTypeMod()
        {
            // Typemod is a phase mod, so make sure it inverts
            Mod.Config.Mech.TypeMod = 0;

            Mech mech = TestHelper.BuildTestMech(tonnage: 50);
            InitiativeHelper.UpdateInitiative(mech);
            // Tonnage = phase 14 => init 17, +0 type, -0 tactics, +0 random => 17 init
            Assert.AreEqual(17, mech.Initiative);

            Mod.Config.Mech.TypeMod = -4;
            // Have to rebuild the mech each time as typeMod is set as stat on creation
            mech = TestHelper.BuildTestMech(tonnage: 50);
            InitiativeHelper.UpdateInitiative(mech);
            // Tonnage = phase 14 => init 17, -4 type => +4, -0 tactics, +0 random => 21 init
            Assert.AreEqual(21, mech.Initiative);

            Mod.Config.Mech.TypeMod = 3;
            mech = TestHelper.BuildTestMech(tonnage: 50);
            InitiativeHelper.UpdateInitiative(mech);
            // Tonnage = phase 14 => init 17, 3 type => -3, -0 tactics, +0 random => 14 init
            Assert.AreEqual(14, mech.Initiative);
        }

        [TestMethod]
        public void TestInjuryMod()
        {
            Mech mech50 = TestHelper.BuildTestMech(tonnage: 50);

            mech50.StatCollection.Set<int>(ModStats.MOD_INJURY, 0);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, -0 tactics, +0 random, +0 injury => 17 init 
            Assert.AreEqual(17, mech50.Initiative);

            mech50.StatCollection.Set<int>(ModStats.MOD_INJURY, 2);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, -0 tactics, +0 random, +2 injury -> -2 => 15 init 
            Assert.AreEqual(15, mech50.Initiative);

            mech50.StatCollection.Set<int>(ModStats.MOD_INJURY, -2);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, -0 tactics, +0 random, -2 injury -> +2 => 19 init
            Assert.AreEqual(19, mech50.Initiative);

        }

        [TestMethod]
        public void TestMiscMod()
        {
            Mech mech50 = TestHelper.BuildTestMech(tonnage: 50);

            mech50.StatCollection.Set<int>(ModStats.MOD_MISC, 0);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, -0 tactics, +0 random, 0 misc => 17 init 
            Assert.AreEqual(17, mech50.Initiative);

            mech50.StatCollection.Set<int>(ModStats.MOD_MISC, 2);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, -0 tactics, +0 random, +2 misc -> -2 => 15 init
            Assert.AreEqual(15, mech50.Initiative);

            mech50.StatCollection.Set<int>(ModStats.MOD_MISC, -2);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, -0 tactics, +0 random, -2 misc -> +2, => 19 init
            Assert.AreEqual(19, mech50.Initiative);

        }

        // TODO: FIX ME
        [TestMethod]
        public void TestTacticsMod()
        {
            Mech mech50 = TestHelper.BuildTestMech(tonnage: 50);
            mech50.GetPilot().StatCollection.Set<int>("Tactics", 1);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, tactics 1 = -0, +0 random => 17 init
            Assert.AreEqual(17, mech50.Initiative);

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 2);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, tactics 2 = -1, +0 random => 16 init
            Assert.AreEqual(16, mech50.Initiative);

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 4);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, tactics 4 = -2, +0 random => 15 init
            Assert.AreEqual(15, mech50.Initiative);

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 6);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, +0 type, tactics 6 = -3, +0 random => 14 init
            Assert.AreEqual(14, mech50.Initiative);

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 8);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, tactics 8 = -4, +0 random => 13 init
            Assert.AreEqual(13, mech50.Initiative);

            mech50.GetPilot().StatCollection.Set<int>("Tactics", 10);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, 0 type, tactics 10 = -5, +0 random => 12 init
            Assert.AreEqual(12, mech50.Initiative);
        }


        // TODO: FIX ME
        [TestMethod]
        public void TestHesitation()
        {
            Assert.IsTrue(false);

        }
    }


}
