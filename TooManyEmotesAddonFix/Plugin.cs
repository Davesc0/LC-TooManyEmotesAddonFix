using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TooManyEmotesAddonFix
{
    [BepInPlugin(Plugin.modGUID, Plugin.modName, Plugin.modVersion)]
    [BepInDependency("FlipMods.TooManyEmotes", BepInDependency.DependencyFlags.HardDependency)]
    // just for load order, so alone's emotes are in by the time this mod loads in
    [BepInDependency("luvsya.aloneemotes", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string modGUID = "dev.davesco.TooManyEmotesAddonFix";
        public const string modName = "TooManyEmotesAddonFix";
        public const string modVersion = "0.1.0";
        private Harmony _harmony = new Harmony(modGUID);
        internal static ManualLogSource mlg = BepInEx.Logging.Logger.CreateLogSource(modGUID);

        void Awake()
        {
            _harmony.PatchAll();
            EmoteListRepair.AtStartup();

            mlg.LogInfo($"Plugin {modName} is loaded!");
        }
    }
}
