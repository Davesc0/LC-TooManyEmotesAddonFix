using System.Collections.Generic;
using HarmonyLib;
using TooManyEmotes;

namespace TooManyEmotesAddonFix
{
    [HarmonyPatch]
    internal static class AddonTiers
    {
        static readonly List<UnlockableEmote> addonEmotes = new List<UnlockableEmote>();

        // new sessions copy the free list from this
        static readonly AccessTools.FieldRef<List<UnlockableEmote>> complementaryEmotesDefault =
            AccessTools.StaticFieldRefAccess<List<UnlockableEmote>>(AccessTools.Field(typeof(EmotesManager), "complementaryEmotesDefault"));

        internal static void Apply()
        {
            if (EmotesManager.allUnlockableEmotes == null || !BaseBundles.Found)
                return;

            float[] floors = Settings.TierFloors();
            Dictionary<string, int> overrides = Settings.ParseOverrides();
            addonEmotes.Clear();
            int[] counts = new int[4];
            foreach (UnlockableEmote emote in EmotesManager.allUnlockableEmotes)
            {
                // addon emotes = free emotes that didn't come from TME's own bundles
                if (!emote.complementary || emote.animationClip == null || BaseBundles.FreeClips.Contains(emote.animationClip))
                    continue;

                emote.complementary = false;
                emote.displayName = OriginalCaseName(emote);
                if (!overrides.TryGetValue(emote.displayName, out emote.rarity))
                    emote.rarity = TierFor(emote, floors);
                addonEmotes.Add(emote);
                TierList(emote.rarity).Add(emote);
                complementaryEmotesDefault()?.Remove(emote);
                EmotesManager.complementaryEmotes?.Remove(emote);
                counts[emote.rarity]++;
            }

            int total = counts[0] + counts[1] + counts[2] + counts[3];
            if (total > 0)
                Plugin.mlg.LogInfo($"Tiered {total} addon emotes: {counts[0]} Common, {counts[1]} Rare, {counts[2]} Epic, {counts[3]} Legendary.");
        }

        // every reset of the unlocked list ends up here, host and client
        [HarmonyPatch(typeof(SessionManager), nameof(SessionManager.ResetEmotesLocal))]
        [HarmonyPostfix]
        static void UnlockFreeAddonEmotes()
        {
            if (!Settings.MustBeBought.Value)
                SessionManager.UnlockEmotesLocal(addonEmotes);
        }

        static int TierFor(UnlockableEmote emote, float[] tierFloors)
        {
            float length = emote.animationClip.length;
            if (emote.transitionsToClip != null)
                length += emote.transitionsToClip.length;

            int tier = 0;
            while (tier < tierFloors.Length && length >= tierFloors[tier])
                tier++;
            return tier;
        }

        // TME lowercases names ("Moves like jagger"), so use the clip name
        static string OriginalCaseName(UnlockableEmote emote)
        {
            string name = emote.emoteSyncGroupName != "" ? emote.emoteSyncGroupName : emote.emoteName;
            name = name.Replace('_', ' ').Trim(' ');
            return name != "" ? name : emote.displayName;
        }

        static List<UnlockableEmote> TierList(int rarity) => rarity switch
        {
            1 => EmotesManager.allEmotesTier1,
            2 => EmotesManager.allEmotesTier2,
            3 => EmotesManager.allEmotesTier3,
            _ => EmotesManager.allEmotesTier0,
        };
    }
}
