using Harmony;
using HBS.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace SkillBasedInit {
    public class SkillBasedInit {

        public static ILog Logger;
        public static string Path { get; private set; }

        public static int MaxPhase = 30;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            try {
                SetupLogger(modDirectory);
            } catch (Exception e) {
                Logger.LogError(e);
                Logger.Log("Error loading mod settings - using defaults.");
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

    }
}
