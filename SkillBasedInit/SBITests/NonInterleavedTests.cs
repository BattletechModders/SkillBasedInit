using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit.Helper;

namespace SBITests
{
    [TestClass]
    public class NonInterleavedTests
    {
        //[TestMethod]
        public void TestNonInterleaved()
        {
            Mech mech = TestHelper.BuildTestMech(tonnage: 50);

            Traverse isInterleavedT = Traverse.Create(mech.Combat.TurnDirector).Property("_isInterleaved");
            isInterleavedT.SetValue(false);

            InitiativeHelper.UpdateInitiative(mech);

            Assert.AreEqual(TestConsts.LastPhase, mech.Initiative);
        }
    }
}
