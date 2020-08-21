using Harmony;
using IRBTModUtils.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Reflection;

namespace SkillBasedInit {
    public class Mod {

        public const string HarmonyPackage = "us.frostraptor.SkillBasedInit";
        public const string LogName = "skill_based_init";

        public static DeferringLogger Log;
        public static string ModDir;
        public static ModConfig Config;

        public const int MaxPhase = 30;
        public const int MinPhase = 1;
        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            ModDir = modDirectory;

            Exception configE;
            try {
                Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            } catch (Exception e) {
                configE = e;
                Config = new ModConfig();
            } finally {
                Config.InitializeColors();
            }

            Log = new DeferringLogger(modDirectory, "skill_based_init", Config.Debug, Config.Trace);

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info?.Write($"Assembly version: {fvi.ProductVersion}");

            Log.Debug?.Write($"ModDir is:{modDirectory}");
            Log.Debug?.Write($"mod.json settings are:({settingsJSON})");
            Mod.Config.LogConfig();

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());            
        }

    }
}
