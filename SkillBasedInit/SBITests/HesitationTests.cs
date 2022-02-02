using BattleTech;
using Harmony;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkillBasedInit;
using SkillBasedInit.Helper;
using System;

namespace SBITests
{
    [TestClass]
    public class HesitationTests
    {
        [TestInitialize]
        public void ClassInitialize()
        {
            Mod.Config.Mech.TypeMod = 0;

            Mod.Config.Mech.HesitationMax = -6;
            Mod.Config.Mech.HesitationMin= -2;
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestHesitation()
        {
            Mech mech = TestHelper.BuildTestMech(tonnage: 50);
            
            mech.StatCollection.Set<int>(ModStats.MOD_HESITATION, 0);
            Console.WriteLine("Expected: [2,6]");
            for (int i = 0; i < 30; i++)
            {
                int mod = mech.GetHesitationPenalty();
                Console.WriteLine($"HesitationMod: {mod}");
                Assert.IsTrue(mod <= 6);
                Assert.IsTrue(mod >= 2);
            }
            Console.WriteLine("");

            mech.StatCollection.Set<int>(ModStats.MOD_HESITATION, -2);
            Console.WriteLine("Expected: [4,8]");
            for (int i = 0; i < 30; i++)
            {
                int mod = mech.GetHesitationPenalty();
                Console.WriteLine($"HesitationMod: {mod}");
                Assert.IsTrue(mod <= 8);
                Assert.IsTrue(mod >= 4);
            }
            Console.WriteLine("");

            mech.StatCollection.Set<int>(ModStats.MOD_HESITATION, 3);
            Console.WriteLine("Expected: [0,3]");
            for (int i = 0; i < 30; i++)
            {
                int mod = mech.GetHesitationPenalty();
                Console.WriteLine($"HesitationMod: {mod}");
                Assert.IsTrue(mod <= 3);
                Assert.IsTrue(mod >= 0);
            }
            Console.WriteLine("");
        }

    }
}
