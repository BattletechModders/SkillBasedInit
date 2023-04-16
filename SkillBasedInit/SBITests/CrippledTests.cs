using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using SkillBasedInit.Helper;
using System;

namespace SBITests
{
    [TestClass]
    public class CrippledTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            TestGlobalInit.HarmonyInst.Patch(TestGlobalInit.MI_IsLegged, prefix: TestGlobalInit.HM_AlwaysTrue);

            Mod.Config.Mech.CrippledModifierMax = -6;
            Mod.Config.Mech.CrippledModifierMin = -2;
        }

        [TestCleanup]
        public void TestCleanup()
        {
            TestGlobalInit.HarmonyInst.Unpatch(TestGlobalInit.MI_IsLegged, HarmonyPatchType.Prefix);
        }


        // TODO: FIX ME
        [TestMethod]
        public void TestCrippled()
        {
            Mech mech50 = TestHelper.BuildTestMech(tonnage: 50);
            mech50.GetPilot().StatCollection.Set<int>("Piloting", 1);
            Console.WriteLine("Expected: [2,6]");
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.CrippledInitModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= 6);
                Assert.IsTrue(mod >= 2);
            }

            mech50.GetPilot().StatCollection.Set<int>("Piloting", 3);
            Console.WriteLine("Expected: [1,5]");
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.CrippledInitModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= 5);
                Assert.IsTrue(mod >= 1);
            }

            mech50.GetPilot().StatCollection.Set<int>("Piloting", 7);
            Console.WriteLine("Expected: [0,3]");
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.CrippledInitModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= 3);
                Assert.IsTrue(mod >= 0);
            }

            mech50.GetPilot().StatCollection.Set<int>("Piloting", 10);
            Console.WriteLine("Expected: [0,1]");
            for (int i = 0; i < 30; i++)
            {
                int mod = mech50.CrippledInitModifier();
                Console.WriteLine($"InspiredMod: {mod}");
                Assert.IsTrue(mod <= 1);
                Assert.IsTrue(mod >= 0);
            }
        }

    }

}
