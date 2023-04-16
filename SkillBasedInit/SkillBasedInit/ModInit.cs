using IRBTModUtils.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace SkillBasedInit
{
    public class Mod
    {

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

        public static void Init(string modDirectory, string settingsJSON)
        {
            ModDir = modDirectory;

            Exception configE;
            try
            {
                Mod.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            }
            catch (Exception e)
            {
                configE = e;
                Mod.Config = new ModConfig();
            }
            finally
            {
                Mod.Config.Init();
            }

            Log = new DeferringLogger(modDir: modDirectory, logFilename: Mod.LogName, logLabel: Mod.LogLabel, isDebug: Config.Debug, isTrace: Config.Trace);
            Log.Debug?.Write($"ModDir is:{modDirectory}");
            Log.Debug?.Write($"mod.json settings are:({settingsJSON})");
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

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), HarmonyPackage);
        }

    }
}
