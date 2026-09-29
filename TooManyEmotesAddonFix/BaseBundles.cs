using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TooManyEmotesAddonFix
{
    // Finds TME's own emote bundles among the loaded ones.
    internal static class BaseBundles
    {
        static readonly string[] emoteBundles =
            { "emotes_0", "emotes_1", "emotes_2", "emotes_3", "emotes_complementary", "emotes_special" };

        // base free emotes, leave these alone
        static readonly string[] freeBundles = { "emotes_complementary", "emotes_special" };

        // clip + its original name (asset path file name, lowercase)
        internal static List<(AnimationClip clip, string originalName)> Clips { get; private set; }
        internal static HashSet<AnimationClip> AllClips { get; private set; }
        internal static HashSet<AnimationClip> FreeClips { get; private set; }

        internal static bool Found => Clips != null;

        internal static void Find()
        {
            if (Found)
                return;

            var clips = new List<(AnimationClip, string)>();
            var all = new HashSet<AnimationClip>();
            var free = new HashSet<AnimationClip>();
            int found = 0;
            foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (Array.IndexOf(emoteBundles, bundle.name) < 0)
                    continue;
                found++;
                bool isFree = Array.IndexOf(freeBundles, bundle.name) >= 0;
                foreach (string path in bundle.GetAllAssetNames())
                {
                    if (!path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase))
                        continue;
                    AnimationClip clip = bundle.LoadAsset<AnimationClip>(path);
                    if (clip == null)
                        continue;
                    clips.Add((clip, Path.GetFileNameWithoutExtension(path)));
                    all.Add(clip);
                    if (isFree)
                        free.Add(clip);
                }
            }

            // if one is missing we can't tell base emotes from addon ones, so bail
            if (found < emoteBundles.Length)
            {
                Plugin.mlg.LogError($"Found {found} of TooManyEmotes' {emoteBundles.Length} emote bundles, emotes are left as they are.");
                return;
            }
            Clips = clips;
            AllClips = all;
            FreeClips = free;
        }
    }
}
