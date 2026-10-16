using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace TooManyEmotesAddonFix
{
    internal static class Settings
    {
        internal static ConfigEntry<bool> MustBeBought;
        internal static ConfigEntry<float> RareFrom;
        internal static ConfigEntry<float> EpicFrom;
        internal static ConfigEntry<float> LegendaryFrom;
        internal static ConfigEntry<string> TierOverrides;

        static readonly string[] tierNames = { "common", "rare", "epic", "legendary" };

        internal static void Init(ConfigFile config)
        {
            const string note = "\nEveryone in the lobby needs the same value.";

            MustBeBought = config.Bind("Addon emotes", "MustBeBought", true,
                "If false, addon emotes still get a tier but are unlocked from the start." + note);
            RareFrom = config.Bind("Tiers", "RareFromSeconds", 7f,
                "Addon emotes are tiered by animation length. Shorter than this is Common." + note);
            EpicFrom = config.Bind("Tiers", "EpicFromSeconds", 14f, "Emotes at least this long are Epic." + note);
            LegendaryFrom = config.Bind("Tiers", "LegendaryFromSeconds", 28f, "Emotes at least this long are Legendary." + note);
            TierOverrides = config.Bind("Tiers", "Overrides", "",
                "Set the tier of single emotes by name, e.g.  Chicken Dance:Common, SCOOBY DOO:Epic" + note);
        }

        internal static float[] TierFloors()
        {
            float[] floors = { RareFrom.Value, EpicFrom.Value, LegendaryFrom.Value };
            Array.Sort(floors);
            return floors;
        }

        internal static Dictionary<string, int> ParseOverrides()
        {
            var overrides = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (string entry in TierOverrides.Value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int colon = entry.LastIndexOf(':');
                int tier = colon < 0 ? -1 : Array.IndexOf(tierNames, entry.Substring(colon + 1).Trim().ToLowerInvariant());
                if (tier < 0)
                {
                    Plugin.mlg.LogWarning($"Can't read tier override \"{entry.Trim()}\", expected Name:Common/Rare/Epic/Legendary.");
                    continue;
                }
                overrides[entry.Substring(0, colon).Trim()] = tier;
            }
            return overrides;
        }
    }
}
