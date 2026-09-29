# TooManyEmotes Addon Fix

TooManyEmotes has no way for addons to add emotes, so addons like
[Alone's TooManyEmotes](https://thunderstore.io/c/lethal-company/p/aloneluvsya/Alones_TooManyEmotes/)
work around it, and that causes two problems. This mod fixes both.

**Addon emotes are all free Commons.** The addon registers them as free extras: unlocked for
everyone from the start, never in the store, all shown as Common. Here they become normal
emotes, each with a rarity tier, turning up in the store rotation and bought like the rest.

**Some of TooManyEmotes' own emotes break.** The addon adds its emotes by rebuilding the emote
list, and that rebuild loses part of what TooManyEmotes set up. Props go missing (sax, bat,
dumbbell, jug band), poses stop being poses, random emotes (death pose, rock paper scissors)
stop picking, and the band emotes lose their instrument order. Here the list is rebuilt
properly and everything the addon set on its own emotes is kept.

- **Tier from animation length.** How long the dance runs before it loops: under 7 seconds is
  Common, under 14 Rare, under 28 Epic, anything longer Legendary.
- **No list to keep up.** Nothing is tied to emote names. Emotes an addon adds in a later update
  are tiered the same way, with no update needed here.
- **Names keep their casing.** "Moves Like Jagger" instead of "Moves like jagger".
- **Base emotes stay as they were.** TooManyEmotes' own free emotes stay free.
- **Works with menus that read the store.** Y4NGZUpgrades lists the emotes under their tiers.

## Compatibility

Tested with Alone's TooManyEmotes. Other addons that add their emotes the same way (as free
extras through a rebuild of the emote list) should work too, since nothing here is specific to
Alone's. With no addon installed the mod has nothing to tier and changes nothing that matters.

Depends on [TooManyEmotes](https://thunderstore.io/c/lethal-company/p/FlipMods/TooManyEmotes/).
Every player needs the mod: the store rotation is worked out on each machine, and it only
matches when everyone sees the same tiers.

## Notes

AI-assisted mod. Tested in a lightweight profile and in a ~260-mod modpack.
Found a bug? Open an issue.
