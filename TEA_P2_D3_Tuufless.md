# D3 — Tuufless Tollgate draft

This is the Tuufless BJ/CC strategy, not the earlier split Toolbox or NAUR helper pack.
User tank labels: **MT = Brute Justice; OT = Cruise Chaser**. Tuufless calls these ST and MT respectively.

## Imports

- Splatoon: `TEA_P2_D3_Tuufless.cs`
- Triggernometry: `Triggernometry_TEA_P2_D3_Tuufless.xml`, using the existing `localhost:51423` endpoint.
- Nisi identity helper: updated `TEA_P2_Nisi.cs` v105 plus the existing `Triggernometry_TEA_Nisi_Personal_Voice.xml`.
- Keep Limit Cut 1256 separately. This new helper starts with J Kick / Judgment Nisi and stops before Temporal Stasis.

The new helper does not modify in-game waymarks or control movement. It does not replace all TEA scripts.
Disable duplicate Photon/Verdict triggers from older voice packs if they overlap. The Nisi helper's old timer-based partner marker is now optional and off by default; identity display/voice remains enabled. If enabled manually, that partner ring is cyan, not a move-now signal.

## Exact floor coordinates

These come from the preset on Tuufless's TEA index. The script uses X and Z; Splatoon stores world Z in element `refY`.

| Location | X | Z | Use |
|---|---:|---:|---|
| A | 100 | 88 | First water, if D3 does not have initial Lightning |
| C | 100 | 116 | Second water, all DPS |
| 3 | 107.8 | 107.8 | Third water, if D3 did not carry second water |
| C | 100 | 116 | South Super Jump reference; D3 must still be the farthest player |

The guide prescribes a south jump bait, not a precise C-center jump coordinate. Reusing C is an explicit staging choice, not a claim that the guide specifies that exact jump point. Do not stop short if another player remains farther from Brute Justice.

Only water destinations and the south jump reference have floor markers in this version. West Enumeration is near the middle, not at D. No guessed floor positions are drawn for clocks, Enumeration, Nisi exchange routes, shield front, or hiding behind ice.

## Implemented behavior

- Short event-driven text/voice cues for the P2 sequence, including Shield + Flarethrower before third Nisi.
- First two exchanges say H1. No CCW blanket rule, BPOG lineup, mine bait for D3, or D3 Enumeration flex.
- First Lightning holder is read from the actual status; if it is D3, says to share with MT (Brute Justice). Missing holder data produces a check cue, not a water marker.
- Water cycles advance only on Crashing Wave, deduplicated against multi-target packets. Crashing Thunder never increments them.
- Cyan water marker is a later destination. It turns green when the observed Water debuff has five seconds remaining. This is a configurable-in-code lead time, not a validated personal safe-route window.
- Second-water carrier is remembered from the actual status before resolution. That person is excluded from the third-water marker. Unknown history produces a check cue instead.
- Second Nisi cue requires Enumeration resolution and two distinct mine-soak helpers. Third Nisi cue requires resolved Flarethrower and a shield observed alive and subsequently dead. If these packets/objects are not observable in the installed plugin build, the cue may be absent; there is no timer fallback.
- First/second Nisi cues are reminders to exchange, not automatic confirmation the exchange succeeded. Final matching is still shown by the separate identity helper; this new script does not solve the third/fourth partner or the ice lineup.
- P2 activation, Temporal Stasis cutoff, death cutoff, combat/reset cleanup, and 240-second timeout. After death the route stays off until the next reset rather than guessing a recovery state.

## Still needs logs and testing

No C# compiler with the game's assemblies or live FFXIV client is available in this session. Syntax/voice wiring/coordinate checks do not establish plugin compatibility or in-game correctness. Import and check Splatoon's compile panel before relying on it.

Checks completed: C# syntax trees parsed without errors for the route helper and v105 Nisi helper; all 32 emitted voice codes have exactly one matching XML trigger; other helpers' endpoint messages cannot match; all three water coordinates match the live Tuufless preset. Reviewed P2/death cutoff, duplicate mine counting, missing-holder handling, water-only cycle progression, and shield/fire gates. No replay or compiled behavior test was run.

Replay/live verification is needed for the action-event timing, mine-source counting, shield death visibility, water status sampling, actual movement windows, and voice delivery. Full personal Nisi destinations and exact clock/Enumeration/ice routes remain unfinished pending logs and assignment validation.

Do not treat the line to a marker as a safe path through other players or hazards. The helper never checks route collisions or confirms a completed Nisi transfer.

## Sources checked 2026-10-30

- https://ffxiv.tuufless.com/elemental/tea/ — exact waymark preset
- https://ffxiv.tuufless.com/elemental/tea/02b_bjcc/ — D3 Tollgate route and diagrams
- https://github.com/quisquous/cactbot/blob/main/ui/raidboss/data/05-shb/ultimate/the_epic_of_alexander.txt — action sequence
- https://github.com/quisquous/cactbot/blob/main/ui/raidboss/data/05-shb/ultimate/the_epic_of_alexander.ts — status IDs and event cues
- https://github.com/awgil/ffxiv_bossmod/blob/master/BossMod.Ultimate/Shadowbringers/Ultimate/TEA/TEAEnums.cs — action/status/object IDs
- https://github.com/awgil/ffxiv_bossmod/blob/master/BossMod.Ultimate/Shadowbringers/Ultimate/TEA/TEAStates.cs — cast/resolution ordering (Verdict begins before second water resolves)
- https://github.com/NightmareXIV/ECommons/blob/master/ECommons/Hooks/ActionEffectTypes/ActionEffectSet.cs — nullable Action and Source API
