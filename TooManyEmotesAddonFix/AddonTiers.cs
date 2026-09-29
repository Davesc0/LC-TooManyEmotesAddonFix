using System.Collections.Generic;
using HarmonyLib;
using TooManyEmotes;

namespace TooManyEmotesAddonFix
{
    // Addons (like Alone's) dump their clips into TME's complementary list, so they end up
    // free and unlocked for everyone. This turns them into normal store emotes with a tier.
    // No emote names hardcoded, so new emotes from addon updates just work.
    internal static class AddonTiers
    {
        // clip length in seconds: <7 common, <14 rare, <28 epic, rest legendary
        static readonly float[] tierFloors = { 7f, 14f, 28f };

        // internal in TME, new sessions copy the free list from this
        static readonly AccessTools.FieldRef<List<UnlockableEmote>> complementaryEmotesDefault =
            AccessTools.StaticFieldRefAccess<List<UnlockableEmote>>(AccessTools.Field(typeof(EmotesManager), "complementaryEmotesDefault"));

        internal static void Apply()
        {
            if (EmotesManager.allUnlockableEmotes == null || !BaseBundles.Found)
                return;

            int[] counts = new int[4];
            foreach (UnlockableEmote emote in EmotesManager.allUnlockableEmotes)
            {
                if (!emote.complementary || emote.animationClip == null || BaseBundles.FreeClips.Contains(emote.animationClip))
                    continue;

                emote.complementary = false;
                emote.rarity = TierFor(emote);
                emote.displayName = OriginalCaseName(emote);
                TierList(emote.rarity).Add(emote);
                complementaryEmotesDefault()?.Remove(emote);
                EmotesManager.complementaryEmotes?.Remove(emote);
                counts[emote.rarity]++;
            }

            int total = counts[0] + counts[1] + counts[2] + counts[3];
            if (total > 0)
                Plugin.mlg.LogInfo($"Tiered {total} addon emotes: {counts[0]} Common, {counts[1]} Rare, {counts[2]} Epic, {counts[3]} Legendary.");
        }

        static int TierFor(UnlockableEmote emote)
        {
            float length = emote.animationClip.length;
            if (emote.transitionsToClip != null)
                length += emote.transitionsToClip.length;

            int tier = 0;
            while (tier < tierFloors.Length && length >= tierFloors[tier])
                tier++;
            return tier;
        }

        // TME lowercases everything after the first letter ("Moves like jagger"), use the clip name instead.
        // list is sorted case-insensitive so this doesn't mess up the order
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
