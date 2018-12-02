using Harmony;
using HBS.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SkillBasedInit {
    public class SkillBasedInit {

        public static ILog Logger;
        public static string Path { get; private set; }

        public static int MaxPhase = 30;

        public static Settings settings;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            try {
                SetupLogger(modDirectory);

                using (StreamReader r = new StreamReader(settingsJSON)) {
                    string json = r.ReadToEnd();
                    SkillBasedInit.settings = JsonConvert.DeserializeObject<Settings>(json);
                }
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
