using BattleTech;
using Harmony;
using HBS.Collections;
using IRBTModUtils;
using SkillBasedInit.Helper;
using System;
using System.Reflection;

namespace SBITests
{
    public static class TestHelper
    {
        public static Mech BuildTestMech(float tonnage)
        {
            Mech mech = new Mech();


            DescriptionDef descriptionDef = new DescriptionDef("foo", "bar", "raboof", "", 100, 0, true, "", "", "");
            ChassisDef chassisDef = new ChassisDef(descriptionDef, "", "", "", "", "", tonnage, tonnage, WeightClass.ASSAULT,
                0, 0, 0, 0, 0, 0, new float[] { 0 }, 0, 0, 0, 0, 0,
                true, 0, 0, 0, 0, 0, 0, 0, 0, new LocationDef[] { }, new MechComponentRef[] { },
                new HBS.Collections.TagSet());
            Traverse tonnageT = Traverse.Create(chassisDef).Property("Tonnage");
            tonnageT.SetValue(tonnage);

            MechDef mechDef = new MechDef();
            Traverse chassisT = Traverse.Create(mechDef).Field("_chassisDef");
            chassisT.SetValue(chassisDef);

            // Necessary for CU integration
            Traverse chassisIDT = Traverse.Create(mechDef).Field("chassisID");
            chassisIDT.SetValue(descriptionDef.Id);

            Traverse mechDefT = Traverse.Create(mech).Property("MechDef");
            mechDefT.SetValue(mechDef);

            mech = (Mech)InitAbstractActor(mech);

            mech.StatCollection.AddStatistic("Head.Structure", 1f);
            mech.StatCollection.AddStatistic("CenterTorso.Structure", 1f);
            mech.StatCollection.AddStatistic("LeftTorso.Structure", 1f);
            mech.StatCollection.AddStatistic("RightTorso.Structure", 1f);
            mech.StatCollection.AddStatistic("LeftArm.Structure", 1f);
            mech.StatCollection.AddStatistic("RightArm.Structure", 1f);
            mech.StatCollection.AddStatistic("LeftLeg.Structure", 1f);
            mech.StatCollection.AddStatistic("RightLeg.Structure", 1f);

            Traverse isPilotableT = Traverse.Create(mech).Field("isPilotable");
            isPilotableT.SetValue(true);
    
            InitModStats(mech);

            Pilot pilot = BuildTestPilot();
            Traverse pilotT = Traverse.Create(mech).Property("pilot");
            pilotT.SetValue(pilot);

            Traverse pilotParentT = Traverse.Create(pilot).Field("_parentActor");
            pilotParentT.SetValue(mech);

            return mech;
        }

        public static Turret BuildTestTurret(float tonnage)
        {
            Turret turret = new Turret();
            return (Turret)InitAbstractActor(turret);
        }

        public static Vehicle BuildTestVehicle(float tonnage)
        {
            Vehicle vehicle = new Vehicle();

            Traverse tonnageT = Traverse.Create(vehicle.VehicleDef.Chassis).Property("Tonnage");
            tonnageT.SetValue(vehicle);

            return (Vehicle)InitAbstractActor(vehicle);
        }

        // Do this after initialization to allow CU to bootstrap CustomInfo
        private static void InitModStats(AbstractActor actor)
        {
            // ModStats - should follow AbstractActorPatches::InitEffectStats
            int tonnageMod = actor.GetBaseInitByTonnage();
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.STATE_TONNAGE, tonnageMod);

            int typeMod = actor.GetTypeModifier();
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.STATE_UNIT_TYPE, typeMod);

            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_INJURY, 0);
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_MISC, 0);
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_CALLED_SHOT_ATTACKER, 0);
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_CALLED_SHOT_TARGET, 0);
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_VIGILANCE, 0);
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_HESITATION, 0);
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.STATE_CALLED_SHOT, 0);
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.STATE_VIGILIANCE, 0);
            actor.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.STATE_HESITATION, 0);
        }

        private static AbstractActor InitAbstractActor(AbstractActor actor)
        {
            // Init the combat ref for constants
            ConstructorInfo constantsCI = AccessTools.Constructor(typeof(CombatGameConstants), new Type[] { });
            CombatGameConstants constants = (CombatGameConstants)constantsCI.Invoke(new object[] { });

            CombatGameState cgs = new CombatGameState();
            Traverse constantsT = Traverse.Create(cgs).Property("Constants");
            constantsT.SetValue(constants);

            Traverse combatT = Traverse.Create(actor).Property("Combat");
            combatT.SetValue(cgs);

            // Init TurnDirector stats. Must use activator here, AccessTools can't find the right constructor
            TurnDirector td = (TurnDirector)Activator.CreateInstance(typeof(TurnDirector), nonPublic: true);

            Traverse firstPhaseT = Traverse.Create(td).Property("FirstPhase");
            firstPhaseT.SetValue(TestConsts.FirstPhase);

            Traverse lastPhaseT = Traverse.Create(td).Property("LastPhase");
            lastPhaseT.SetValue(TestConsts.LastPhase);

            Traverse tdCGST = Traverse.Create(td).Property("Combat");
            tdCGST.SetValue(cgs);

            Traverse isInterleavedT = Traverse.Create(td).Property("_isInterleaved");
            isInterleavedT.SetValue(true);

            Traverse cgsTDT = Traverse.Create(cgs).Property("TurnDirector");
            cgsTDT.SetValue(td);
         
            // Init any required stats
            actor.StatCollection = new StatCollection();

            return actor;
        }

        private static Pilot BuildTestPilot()
        {
            HumanDescriptionDef humanDescDef = new HumanDescriptionDef();
            Traverse callsignT = Traverse.Create(humanDescDef).Property("Callsign");
            callsignT.SetValue("foobar");

            PilotDef pilotDef = new PilotDef();

            Traverse humanDescDefT = Traverse.Create(pilotDef).Property("Description");
            humanDescDefT.SetValue(humanDescDef);

            Traverse pilotTagsT = Traverse.Create(pilotDef).Property("PilotTags");
            pilotTagsT.SetValue(new TagSet());

            Guid guid = new Guid();

            Pilot pilot = new Pilot(pilotDef, guid.ToString(), false);
            pilot.StatCollection.Set<int>("Health", 3);
            pilot.StatCollection.Set<int>("Injuries", 0);

            pilot.StatCollection.Set<int>("Gunnery", 1);
            pilot.StatCollection.Set<int>("Guts", 1);
            pilot.StatCollection.Set<int>("Piloting", 1);
            pilot.StatCollection.Set<int>("Tactics", 1);
            // Init any required stats
            int pilotTagsMod = PilotHelper.GetTagsModifier(pilot);
            pilot.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.STATE_PILOT_TAGS, pilotTagsMod);

            pilot.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_SKILL_GUNNERY, 0);
            pilot.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_SKILL_GUTS, 0);
            pilot.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_SKILL_PILOT, 0);
            pilot.StatCollection.AddStatistic<int>(SkillBasedInit.ModStats.MOD_SKILL_TACTICS, 0);

            return pilot;
        }

        public static Weapon BuildTestWeapon(float minRange = 0f, float shortRange = 0f,
            float mediumRange = 0f, float longRange = 0f, float maxRange = 0f)
        {
            Weapon weapon = new Weapon();

            StatCollection statCollection = new StatCollection();
            statCollection.AddStatistic("MinRange", minRange);
            statCollection.AddStatistic("MinRangeMultiplier", 1f);
            statCollection.AddStatistic("LongRangeModifier", 0f);
            statCollection.AddStatistic("MaxRange", maxRange);
            statCollection.AddStatistic("MaxRangeModifier", 0f);
            statCollection.AddStatistic("ShortRange", shortRange);
            statCollection.AddStatistic("MediumRange", mediumRange);
            statCollection.AddStatistic("LongRange", longRange);

            Traverse statCollectionT = Traverse.Create(weapon).Field("statCollection");
            statCollectionT.SetValue(statCollection);

            return weapon;
        }
    }
}
