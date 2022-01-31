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
    public class InitTests
    {
        [TestMethod]
        public void TestNonInterleaved()
        {
            Mech mech = TestHelper.BuildTestMech(tonnage: 50);

            Traverse isInterleavedT = Traverse.Create(mech.Combat.TurnDirector).Field("_isInterleaved");
            isInterleavedT.SetValue(true);

            InitiativeHelper.UpdateInitiative(mech);

            Assert.AreEqual(30, mech.Initiative);
        }
    }
}
