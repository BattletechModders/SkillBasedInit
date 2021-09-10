using BattleTech;
using BattleTech.Data;
using Harmony;
using SVGImporter;

namespace SkillBasedInit.patches
{

    [HarmonyPatch(typeof(CombatGameState), "_Init")]
    public static class CombatGameState__Init
    {
        public static void Postfix()
        {
            Mod.Log.Trace?.Write("CGS:_I entered.");
            Mod.Log.Debug?.Write("Caching CombatGameState");

            // Pre-load our required icons, otherwise DM will unload them as they aren't necessary
            DataManager dm = UnityGameInstance.BattleTechGame.DataManager;
            LoadRequest loadRequest = dm.CreateLoadRequest();

            // Need to load each unique icon
            Mod.Log.Info?.Write("LOADING EFFECT ICONS...");
            loadRequest.AddLoadRequest<SVGAsset>(BattleTechResourceType.SVGAsset, Mod.Config.Icons.Stopwatch, null);
            loadRequest.ProcessRequests();
            Mod.Log.Info?.Write("  ICON LOADING COMPLETE!");
        }
    }

}
