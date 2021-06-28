using Harmony;
using IRBTModUtils.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SkillBasedInit {
    public class Mod {

        public const string HarmonyPackage = "us.frostraptor.SkillBasedInit";
        public const string LogName = "skill_based_init";
        public const string LogLabel = "SBI";

        public static DeferringLogger Log;
        public static string ModDir;
        public static ModConfig Config;
        public static ModText LocalizedText;
        public static readonly Random Random = new Random();

        public const int MaxPhase = 30;
        public const int MinPhase = 1;

        public static void Init(string modDirectory, string settingsJSON) {
            ModDir = modDirectory;

            Log.Debug?.Write($"ModDir is:{modDirectory}");
            Log.Debug?.Write($"mod.json settings are:({settingsJSON})");

            Exception configE;
            try {
                Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            } catch (Exception e) {
                configE = e;
                Config = new ModConfig();
            } finally {
                Config.Init();
            }

            Log = new DeferringLogger(modDirectory, "skill_based_init", Config.Debug, Config.Trace);
            Mod.Config.LogConfig();

            // Read localization
            string localizationPath = Path.Combine(ModDir, "./mod_localized_text.json");
            try
            {
                string jsonS = File.ReadAllText(localizationPath);
                Mod.LocalizedText = JsonConvert.DeserializeObject<ModText>(jsonS);
            }
            catch (Exception e)
            {
                Mod.LocalizedText = new ModText();
                Log.Error?.Write(e, $"Failed to read localizations from: {localizationPath} due to error!");
            }

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info?.Write($"Assembly version: {fvi.ProductVersion}");


            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());            
        }

    }
}
