using Harmony;
using HBS.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SkillBasedInit {
    public class SkillBasedInit {

        public static ILog Logger;
        public static string Path { get; private set; }

        public static int MaxPhase = 30;

        public static Settings Settings;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            try {
                SetupLogger(modDirectory);

                SkillBasedInit.Logger.Log($"Reading settings from {settingsJSON}");
                SkillBasedInit.Settings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
            } catch (Exception e) {
                Logger.LogError(e);
                Logger.Log("Error loading mod settings - using defaults.");
                Settings = new Settings();
            }

            var harmony = HarmonyInstance.Create("us.frostraptor.SkillBasedInit");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
        }

        // Shamelessly stolen from https://github.com/CWolfs/MissionControl/blob/master/src/Main.cs
        public static void SetupLogger(string modDirectory) {
            Dictionary<string, LogLevel> logLevels = new Dictionary<string, LogLevel> {
                ["SkillBasedInit"] = LogLevel.Debug
            };
            LogManager.Setup(modDirectory + "/initiative.log", logLevels);
            Logger = LogManager.GetLogger("SkillBasedInit");
            Path = modDirectory;
        }

        public static void LogDebug(string message) {
            if (SkillBasedInit.Settings.Debug) {
                SkillBasedInit.Logger.LogDebug(message);
            }
        }

        public static void LogDebugWarning(string message) {
            if (SkillBasedInit.Settings.Debug) {
                SkillBasedInit.Logger.LogWarning(message);
            }
        }

    }
}
