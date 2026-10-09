# TEA — NAUR, Dancer / R1

Reviewed 2026-10-09. These are personal Splatoon helpers for Maggie's NAUR progression. R1 means physical ranged; numbered mechanics still follow the number/debuff actually assigned to you.

**Status: source reviewed and compiled; not tested in a TEA replay or live duty.** This is a repair of the existing eight scripts plus four optional voice reminders, not a complete encounter solver. Keep the group's NAUR plan as the authority for movement.

## Install / update

1. Disable the older copies of these TEA helpers before importing the replacements. The repaired scripts use `MaggieSplatoon.TEA` to distinguish them from official/upstream copies; importing may create another entry instead of replacing the old one. Do not run both.
2. Use **1256** for NAUR. Leave **1211 disabled**. Its bugs were repaired for completeness, but it remains an alternative strategy.
3. Import each desired standalone C# file using its Raw link. Check Splatoon's compile/error panel. These scripts do not require each other.
4. For voice, import the companion XML into Triggernometry and enable its local HTTP endpoint at `http://localhost:51423/`, as with UCOB Blackfire. Enable `TEA_NAUR_Core_Cues`, then use **TEST VOICE** in its settings. Text works without the endpoint. Avoid duplicate spoken reminders from another pack.
5. Check Stasis settings after importing: red = `SouthRightBossSide`, no debuff = `SouthLeftBossSide`, aggravated assault = `JusticeSide`. Blue defaults to locking your nearest east/west side. That is a personal FFA suggestion: coordinate with the other blue player; it does not negotiate their side.

| File | Raw import | Current scope |
|---|---|---|
| [Doll helper](TEA_P1_Untarget_Doll.cs) | [Raw](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/TEA_P1_Untarget_Doll.cs) | Retargets off a low-health doll; optional |
| [1256 Limit Cut](TEA_P2_Transition_1256.cs) | [Raw](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/TEA_P2_Transition_1256.cs) | Blast prediction and bait options, not a complete personal route |
| [Nisi](TEA_P2_Nisi.cs) | [Raw](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/TEA_P2_Nisi.cs) | Partner identification after the first exchange; final symbol matching |
| [Temporal Stasis](TEA_P2_Temporal_Stasis.cs) | [Raw](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/TEA_P2_Temporal_Stasis.cs) | NAUR DPS defaults and personal destination |
| [Wormhole](TEA_P3_Wormhole_Formation.cs) | [Raw](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/TEA_P3_Wormhole_Formation.cs) | Personal number, soak order, observed soak position |
| [Fate Alpha](TEA_P4_Fate_Projection_Alpha.cs) | [Raw](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/TEA_P4_Fate_Projection_Alpha.cs) | Preview order, personal assignment, clone-relative destination |
| [Fate Beta](TEA_P4_Fate_Projection_Beta.cs) | [Raw](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/TEA_P4_Fate_Projection_Beta.cs) | Initial position, jump/stack reminder, donut preview and release |
| [Core reminders](TEA_NAUR_Core_Cues.cs) | [Raw](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/TEA_NAUR_Core_Cues.cs) | Ordinary Motion/Stillness, Photon partner check, Verdict symbol check |
| [Voice companion](Triggernometry_TEA_NAUR_Core_Cues.xml) | [Raw XML](https://raw.githubusercontent.com/englundmb-jpg/FFXIV-Splatoon-Scripts/main/Triggernometry_TEA_NAUR_Core_Cues.xml) | Triggernometry endpoint triggers, including a test |

## What changed

- **Fate Alpha:** removed the incorrect mapping of action `18597` (aggravated assault) to second Stillness. Second Stillness is `18586`. First/second previews now have explicit slots; unrelated packets cannot consume a generic throttle and drop the second instruction.
- **Both Fate helpers:** replaced tether-arrival ordering with descending clone IDs, matching cactbot's assignment method. Require eight distinct owners and eight distinct nonzero clones. Missing data produces no inferred assignment. Reset cancels queued callbacks; death hides the helper.
- **Fate Beta:** clarifies east/west/south jump assignments and the north stack group. Keeps the NAUR light beacon northwest of A. The donut destination remains cyan NEXT until the actual stack/spread resolves, instead of turning green solely because 63 seconds elapsed. Missing donut data cannot produce a destination at the arena origin. Other inherited display timers still need replay validation.
- **Temporal Stasis:** uses the cast event instead of an English/Japanese chat string, waits for debuff/tether evidence, uses NAUR DPS defaults, and clears on resolution or timeout. Debuff priority is preserved; blue-side coordination remains manual.
- **Nisi:** guards incomplete pair information, absent/dead players, and reset state. Changes “pass” wording to **NISI PARTNER** because an expiring debuff alone does not establish that it is safe to pass now. The initial exchange still needs your assigned partner.
- **1256:** reads your number from the actual local-player VFX callback, adds null/reset guards and language-independent Hawk Blaster activation. Opposite bait candidates no longer both tether to you as if each were the chosen personal destination. Even-number text says to go behind the odd partner. Candidate markers are cyan options, not a solved route.
- **1211 alternative:** fixes number 5 pointing to number 6's bait, southeast angle/sign and settings labels, local VFX parsing, and unnecessary marker clutter. Keep disabled for NAUR.
- **Wormhole:** removes the inherited fixed path table and object-effect counter that could advance beyond its array bounds. Soak stages now follow actual Chakram/Repentance action events; duplicate packets do not increment them. Only your number and relevant soak area appear; stand toward its wall edge, not necessarily its center. Third-soak guidance stays cyan and reminds 1/2 to wait for their vulnerability to clear. No universal jump/ray/Limit Cut movement route is claimed.
- **Dolls:** keeps existing retarget behavior and adds a zero-max-HP guard. This does **not** stop damage, disable autorotation, prevent an already queued action, or protect dolls from Dancer splash damage.
- **New core cues:** four brief event-driven reminders using the same local voice bridge as UCOB Blackfire. Photon says “check partner,” never “pass now.” Fate preview actions cannot fire ordinary “move now”/“stop now” voice cues.

## What you need as R1

The main additions to your preparation are assignment reminders, not more rotation micromanagement:

| Priority | Reminder / agreement | Coverage now |
|---|---|---|
| 1 | Identify the CC tank/OT for the first two Nisi exchanges; second exchange waits for their mines | Partner helper plus check reminder; first exchange and safe pass timing remain manual |
| 2 | R1 and M1 flex when BJ/CC Enumerations require it | No automatic flex solver in this pack |
| 3 | Learn numbered Wormhole jump/ray/Limit Cut duties as well as your soak | Number/soak helper only; use the NAUR simulator for the full route |
| 4 | Post-Wormhole Enumeration: follow marked/unmarked assignment, not a permanent R1 side | No personal solver added |
| 5 | Final Word and Fate movement restrictions; stop actions as well as movement when required | Ordinary cast voice cues and Fate visual previews; no automatic action cancellation |
| 6 | Agree party mitigation, Limit Break duties and phase-based burst holds | No rotation/burst scheduler in this repair |

Next useful custom work would be a verified **BJ/CC Enumeration flex cue**, **Nisi pass-window cue**, and **Final Word / Exatrine personal cues**. Those are coverage gaps, not implemented features. They need a representative TEA log/replay and the group's role assignments before adding precise “go now” arrows. The supplied strategy plans are sufficient for this review; a replay would address the remaining event timing and movement questions.

The pasted 0:00 / 2:00 / 4:05 Technical Step plan is not a validated Dancer schedule and mixes encounter events. Do not install it as a repeating burst timer. Agree actual phase openings and holds with this group's kill timings. Also check how your chosen rotation setup handles Stillness and dolls; these visual/voice helpers do not control it.

## Validation and limits

All nine C# scripts compiled together against the available real Splatoon, ECommons and Dalamud assemblies using .NET 10: **zero compiler errors**. Existing deprecated-API and Windows-platform warnings remain; this does not establish compatibility with every installed plugin build.

Focused checks invoked methods from the compiled scripts: both Fate helpers produced identical assignments for all 40,320 packet-order permutations; incomplete/duplicate/zero clone maps were rejected; the Wormhole soak order and ordinary-cast versus Fate-preview voice separation passed. Companion XML parses, and its endpoint patterns match the messages emitted by the script. No real game events, client rendering, audio endpoint, or full encounter replay were exercised.

Remaining limitations: initial Nisi pairing is learned only after a complete first exchange; Stasis blue side is not party-wide negotiation; 1256 does not choose between its two bait candidates; Wormhole no longer supplies unverified full-route arrows; Fate clone-relative coordinates and remaining display timings require in-game checking. Importing these files does not install or configure them on your PC automatically.

## Sources

- [NAUR TEA hub](https://naurffxiv.com/ultimate/tea), including its correction to the video's Fate Beta light-beacon position.
- NAUR-linked Toolboxes: [Living Liquid](https://ff14.toolboxgaming.space/?id=725383877116761&preview=1), [1256](https://ff14.toolboxgaming.space/?id=803293127441961&preview=1), [BJ/CC](https://ff14.toolboxgaming.space/?id=492297437831961&preview=1), [Stasis/Inception](https://ff14.toolboxgaming.space/?id=860745463802461&preview=1), [Wormhole](https://ff14.toolboxgaming.space/?id=537197026169861&preview=1), [Perfect Alexander](https://ff14.toolboxgaming.space/?id=170875560147661&preview=1).
- [cactbot TEA triggers](https://github.com/quisquous/cactbot/blob/main/ui/raidboss/data/05-shb/ultimate/the_epic_of_alexander.ts) for clone ordering and action IDs.
- [BossMod TEA source](https://github.com/awgil/ffxiv_bossmod/tree/master/BossMod/Modules/Shadowbringers/Ultimate/TEA) for mechanic event identities and resolution behavior; [PunishXIV/Splatoon](https://github.com/PunishXIV/Splatoon) for the original helpers/API.
- [Tuufless Elemental TEA](https://ffxiv.tuufless.com/elemental/tea/) was considered as a comparison, not the position authority after NAUR was selected. The supplied YouTube video and malformed repeated playlist URL could not be retrieved reliably; this review does not claim to have watched them.

Original helper authors remain credited in each script. This review found upstream-style logic in the existing copies; not every issue can be attributed to a previous model.
