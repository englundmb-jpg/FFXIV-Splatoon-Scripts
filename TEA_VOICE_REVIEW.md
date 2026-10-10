# TEA voice-call repair — 2026-10-09, Arizona

Reviewed the two supplied Local triggers exports; their contents were identical. This repair covers the 16 enabled loose TEA callouts. The old 1211 entry was already disabled and is intentionally excluded. The supplied export contains no spoken “hold” call. Its two-minute Burst reminder is not changed.

## Import

Import this URL into Triggernometry under Local triggers:

https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/Triggernometry_TEA_NAUR_Reviewed_Calls.xml

It creates **Maggie - TEA NAUR Reviewed Calls**. Before using it, uncheck the old individual entries beginning **TEA -** directly under Local triggers, including the old Fate preview and Wormhole soak entries. Leave **Maggie - TEA NAUR Core Cues** enabled; that folder supplies different calls. Leave the user's preferred **Maggie - Two Minute Burst Reminder** unchanged. No UCOB/UWU/M9S files were modified.

Importing does not replace existing triggers automatically. Triggernometry's import code assigns new IDs to collisions, so retaining both sets would cause duplicate or conflicting calls. The new pack uses distinct IDs and one clearly named folder. No original triggers are deleted.

## Confirmed problems repaired

- Alpha second Stillness matched both `489A` and `48A5`. `48A5` is aggravated assault, not Stillness. The replacement matches only `489A`.
- Alpha/Beta calls describe clone previews explicitly. A preview of a later action must not sound like an immediate move/stop/stack command.
- The old first/second/third soak calls matched Repentance resolution while naming Alexander Prime as the source. Repentance is a helper event, and announcing the same soak at its resolution is too late for preparation.
- First-soak preparation now uses the Void of Repentance cast: “Five six first. Wait for chakrams.” First-soak resolution prepares 7/8; second-soak resolution prepares 1/2 and explicitly reminds them to wait for vulnerability expiry.
- Wormhole Sacrament preparation now uses cast start, not its damage resolution.
- Phase introductions use verified cast IDs instead of relying on English “uses/readies” combat text. Protean is a preparation reminder, not a solved R1 position.
- Matching supports the legacy ACT colon representation and raw pipe representation, including single-target and area action-effect records. A ten-second refire guard prevents multiple target packets from repeating the same call.
- The disabled “1211 Limit Cut” entry was actually bound to Verdict, an unrelated BJ/CC mechanic. It remains excluded rather than being renamed into a purported 1256 solver.

## Validation

The generated XML parses successfully. All 144 synthetic matching checks passed, covering the two log representations, timestamps, effect packet types, rejection of wrong cast/effect type, and rejection of aggravated assault as Stillness. All 16 triggers have distinct IDs; all actions are TTS only. These are structural and source-based checks, not a TEA replay or live ACT test.

This supplement does not add personal Enumeration, Nisi pass-window, Inception, Final Word or Exatrine position solvers. Those remain separate work. Existing phase introductions only name mechanics; they do not determine the player's assignment.

Sources:
- [cactbot TEA](https://github.com/quisquous/cactbot/blob/main/ui/raidboss/data/05-shb/ultimate/the_epic_of_alexander.ts), particularly Alpha preview IDs and their use as Ability events.
- [BossMod TEA](https://github.com/awgil/ffxiv_bossmod/tree/master/BossMod/Modules/Shadowbringers/Ultimate/TEA), mechanic cast/effect distinctions and Repentance order.
- [Triggernometry import handling](https://github.com/paissaheavyindustries/Triggernometry/blob/master/Source/Triggernometry/CustomControls/UserInterface.cs), `ImportResultsFromForm`, for duplicate-ID behavior.
- [NAUR TEA](https://naurffxiv.com/ultimate/tea) and its linked Wormhole plan for soak assignments and movement caveats.
