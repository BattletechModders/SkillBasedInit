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
    public class UnitTests
    {

        [TestMethod]
        public void TestTonnageMod()
        {
            Mech mech5 = TestHelper.BuildTestMech(tonnage: 5);
            InitiativeHelper.UpdateInitiative(mech5);
            // Tonnage = phase 19 => init 12, +1 type, -3 tactics (6), +0 random => 10 init / 21 phase
            Assert.AreEqual(10, mech5.Initiative);

            Mech mech30 = TestHelper.BuildTestMech(tonnage: 30);
            InitiativeHelper.UpdateInitiative(mech30);
            // Tonnage = phase 16 => init 15, +1 type, -3 tactics (6), +0 random => 13 init / 18 phase
            Assert.AreEqual(13, mech30.Initiative);

            Mech mech50 = TestHelper.BuildTestMech(tonnage: 50);
            InitiativeHelper.UpdateInitiative(mech50);
            // Tonnage = phase 14 => init 17, +1 type, -3 tactics (6), +0 random => 15 init / 16 phase
            Assert.AreEqual(15, mech50.Initiative);

            Mech mech75 = TestHelper.BuildTestMech(tonnage: 75);
            InitiativeHelper.UpdateInitiative(mech75);
            // Tonnage = phase 12 => init 19, +1 type, -3 tactics (6), +0 random => 17 init / 18 phase
            Assert.AreEqual(17, mech75.Initiative);

            Mech mech100 = TestHelper.BuildTestMech(tonnage: 100);
            InitiativeHelper.UpdateInitiative(mech100);
            // Tonnage = phase 9 => init 22, +1 type, -3 tactics (6), +0 random => 20 init / 11 phase
            Assert.AreEqual(20, mech100.Initiative);

            Mech mech150 = TestHelper.BuildTestMech(tonnage: 150);
            InitiativeHelper.UpdateInitiative(mech150);
            // Tonnage = phase 6 => init 25, +1 type, -3 tactics (6), +0 random => 23 init / 8 phase
            Assert.AreEqual(23, mech150.Initiative);
        }
    }
}
