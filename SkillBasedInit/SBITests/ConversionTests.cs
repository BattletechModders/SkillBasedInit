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
    public class ConversionTests
    {

        [TestMethod]
        public void TestPhaseToInit()
        {
            Assert.AreEqual(1, InitiativeHelper.PhaseToInitiative(30));
            Assert.AreEqual(6, InitiativeHelper.PhaseToInitiative(25));
            Assert.AreEqual(8, InitiativeHelper.PhaseToInitiative(23));
            Assert.AreEqual(14, InitiativeHelper.PhaseToInitiative(17));
            Assert.AreEqual(22, InitiativeHelper.PhaseToInitiative(9));
            Assert.AreEqual(26, InitiativeHelper.PhaseToInitiative(5));
            Assert.AreEqual(30, InitiativeHelper.PhaseToInitiative(1));
        }

        [TestMethod]
        public void TestInitToPhase()
        {
            Assert.AreEqual(30, InitiativeHelper.InitiativeToPhase(1));
            Assert.AreEqual(27, InitiativeHelper.InitiativeToPhase(4));
            Assert.AreEqual(23, InitiativeHelper.InitiativeToPhase(8));
            Assert.AreEqual(18, InitiativeHelper.InitiativeToPhase(13));
            Assert.AreEqual(15, InitiativeHelper.InitiativeToPhase(16));
            Assert.AreEqual(12, InitiativeHelper.InitiativeToPhase(19));
            Assert.AreEqual(8, InitiativeHelper.InitiativeToPhase(23));
            Assert.AreEqual(4, InitiativeHelper.InitiativeToPhase(27));
            Assert.AreEqual(1, InitiativeHelper.InitiativeToPhase(30));
        }
    }
}
