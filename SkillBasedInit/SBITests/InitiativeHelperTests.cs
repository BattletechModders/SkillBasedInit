using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using SkillBasedInit.Helper;

namespace SBITests
{
    [TestClass]
    public class InitiativeHelperTests
    {
        // Phases are tracked from 1 - 30, but display to the user as 30 to 1 (inverted)
        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase1_UI30()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(1);
            int[] expected_label = { 30, 29, 28, 27, 26 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase2_UI29()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(2);
            int[] expected_label = { 30, 29, 28, 27, 26 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase3_UI28()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(3);
            int[] expected_label = { 30, 29, 28, 27, 26 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase4_UI27()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(4);
            int[] expected_label = { 29, 28, 27, 26, 25 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase5_UI26()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(5);
            int[] expected_label = { 28, 27, 26, 25, 24 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase26_UI5()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(26);
            int[] expected_label = {7, 6, 5, 4, 3};

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase27_UI4()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(27);
            int[] expected_label = { 6, 5, 4, 3, 2 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase28_UI3()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(28);
            int[] expected_label = { 5, 4, 3, 2, 1 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase29_UI2()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(29);
            int[] expected_label = { 5, 4, 3, 2, 1 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

        [TestMethod]
        public void TestCalcPhaseIconBounds_Phase30_UI1()
        {
            int[] result = InitiativeHelper.CalcPhaseIconBounds(30);
            int[] expected_label = { 5, 4, 3, 2, 1 };

            Assert.IsNotNull(result);
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(expected_label[0], result[0]);
            Assert.AreEqual(expected_label[1], result[1]);
            Assert.AreEqual(expected_label[2], result[2]);
            Assert.AreEqual(expected_label[3], result[3]);
            Assert.AreEqual(expected_label[4], result[4]);
        }

    }
}
