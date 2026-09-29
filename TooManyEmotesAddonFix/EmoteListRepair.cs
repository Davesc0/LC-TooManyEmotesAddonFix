using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using HarmonyLib;
using TooManyEmotes;
using TooManyEmotes.Props;
using UnityEngine;

namespace TooManyEmotesAddonFix
{
    /*  Addons call BuildEmotesList again to add their emotes, which breaks some base emotes:
        -the first build renames clips (praise_the_sun_pose -> praise_the_sun), so the second one
        misses the _pose / .random. / .layer_N. markers -> broken poses, random emotes, band emotes
        - all emotes get recreated, so props/music TME set up after its build are lost */

    // Fix: restore the original clip names, build again, rerun TME's setup, put back addon settings.
    [HarmonyPatch]
    internal static class EmoteListRepair
    {
        static bool ready;
        static bool rebuilding;
        static Dictionary<AnimationClip, AddonEmoteSettings> addonSettings;

        static readonly AccessTools.FieldRef<UnlockableEmote, bool> isBoomboxAudio =
            AccessTools.FieldRefAccess<UnlockableEmote, bool>("_isBoomboxAudio");

        static readonly FieldInfo emotePropsData =
            AccessTools.Field(AccessTools.TypeByName("TooManyEmotes.Props.EmotePropManager"), "emotePropsData");

        static readonly MethodInfo buildEmotePropList =
            AccessTools.Method(AccessTools.TypeByName("TooManyEmotes.Props.EmotePropManager"), "BuildEmotePropList");

        internal static void AtStartup()
        {
            var timer = Stopwatch.StartNew();
            BaseBundles.Find();
            ready = BaseBundles.Found;
            if (!ready)
                return;

            addonSettings = SaveAddonSettings();
            Repair();
            Plugin.mlg.LogInfo($"Emote list repaired and tiered in {timer.ElapsedMilliseconds} ms.");
        }

        // in case an addon loading after this mod rebuilds the list too.
        // grab addon settings before the old emote objects are thrown away
        [HarmonyPatch(typeof(EmotesManager), nameof(EmotesManager.BuildEmotesList))]
        [HarmonyPrefix]
        static void BeforeBuild()
        {
            if (ready && !rebuilding)
                addonSettings = SaveAddonSettings();
        }

        [HarmonyPatch(typeof(EmotesManager), nameof(EmotesManager.BuildEmotesList))]
        [HarmonyPostfix]
        static void AfterBuild()
        {
            if (ready && !rebuilding)
                Repair();
        }

        static void Repair()
        {
            try
            {
                RestoreClipNames();
                rebuilding = true;
                try { EmotesManager.BuildEmotesList(); }
                finally { rebuilding = false; }
                RerunBaseSetup();
                RestoreAddonSettings();
            }
            catch (Exception e)
            {
                Plugin.mlg.LogError($"Could not repair the emote list: {e}");
            }
            AddonTiers.Apply();
        }

        // only touch actually renamed clips (case-only diff = not renamed, e.g. "get_HOT")
        static void RestoreClipNames()
        {
            foreach ((AnimationClip clip, string originalName) in BaseBundles.Clips)
            {
                if (clip != null && !string.Equals(clip.name, originalName, StringComparison.OrdinalIgnoreCase))
                    clip.name = originalName;
            }
        }

        // same steps TME runs in Awake after its build.
        // props still point to the old emote objects, clear those first
        static void RerunBaseSetup()
        {
            if (emotePropsData?.GetValue(null) is IEnumerable props)
            {
                foreach (EmotePropData prop in props)
                    prop?.parentEmotes?.Clear();
            }
            buildEmotePropList?.Invoke(null, null);
            AdditionalEmoteData.SetAdditionalEmoteData();
            AdditionalEmoteData.SetAdditionalPropData();
            AdditionalEmoteData.SetAdditionalMusicData();
        }

        static Dictionary<AnimationClip, AddonEmoteSettings> SaveAddonSettings()
        {
            var saved = new Dictionary<AnimationClip, AddonEmoteSettings>();
            if (EmotesManager.allUnlockableEmotes == null)
                return saved;

            foreach (UnlockableEmote emote in EmotesManager.allUnlockableEmotes)
            {
                if (emote.animationClip != null && !BaseBundles.AllClips.Contains(emote.animationClip))
                    saved[emote.animationClip] = new AddonEmoteSettings(emote);
            }
            return saved;
        }

        static void RestoreAddonSettings()
        {
            if (addonSettings == null)
                return;
            foreach (UnlockableEmote emote in EmotesManager.allUnlockableEmotes)
            {
                if (emote.animationClip != null && addonSettings.TryGetValue(emote.animationClip, out AddonEmoteSettings settings))
                    settings.ApplyTo(emote);
            }
        }

        // stuff an addon might set on its emotes after building
        readonly struct AddonEmoteSettings
        {
            readonly bool boomboxAudio;
            readonly bool canMoveWhileEmoting;
            readonly string overrideAudioClipName;
            readonly string overrideAudioLoopClipName;
            readonly float recordSongLoopValue;
            readonly bool requiresHeldProp;
            readonly GameObject requiredHeldPropPrefab;
            readonly List<string> propNamesInEmote;

            internal AddonEmoteSettings(UnlockableEmote emote)
            {
                boomboxAudio = isBoomboxAudio(emote);
                canMoveWhileEmoting = emote.canMoveWhileEmoting;
                overrideAudioClipName = emote.overrideAudioClipName;
                overrideAudioLoopClipName = emote.overrideAudioLoopClipName;
                recordSongLoopValue = emote.recordSongLoopValue;
                requiresHeldProp = emote.requiresHeldProp;
                requiredHeldPropPrefab = emote.requiredHeldPropPrefab;
                propNamesInEmote = emote.propNamesInEmote;
            }

            internal void ApplyTo(UnlockableEmote emote)
            {
                isBoomboxAudio(emote) = boomboxAudio;
                emote.canMoveWhileEmoting = canMoveWhileEmoting;
                emote.overrideAudioClipName = overrideAudioClipName;
                emote.overrideAudioLoopClipName = overrideAudioLoopClipName;
                emote.recordSongLoopValue = recordSongLoopValue;
                emote.requiresHeldProp = requiresHeldProp;
                emote.requiredHeldPropPrefab = requiredHeldPropPrefab;
                emote.propNamesInEmote ??= propNamesInEmote;
            }
        }
    }
}
