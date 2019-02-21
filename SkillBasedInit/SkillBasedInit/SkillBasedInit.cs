using Harmony;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Reflection;

namespace SkillBasedInit {
    public class SkillBasedInit {

        public const string HarmonyPackage = "us.frostraptor.SkillBasedInit";

        public static string Path { get; private set; }

        public static int MaxPhase = 30;
        public static int MinPhase = 1;

        public static Logger Logger;
        public static string ModDir;
        public static ModConfig ModConfig;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            ModDir = modDirectory;

            Exception configE;
            try {
                ModConfig = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            } catch (Exception e) {
                configE = e;
                ModConfig = new ModConfig();
            }

            ModConfig.InitializeColors();

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm?.Location);
            //Logger.Log($"Assembly version: {fvi?.ProductVersion}");

            Logger = new Logger(modDirectory, "skill_based_init");
            Logger.LogIfDebug($"mod.json settings are:({settingsJSON})");
            Logger.Log($"mergedConfig is:{ModConfig}");

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
        }

    }
}
