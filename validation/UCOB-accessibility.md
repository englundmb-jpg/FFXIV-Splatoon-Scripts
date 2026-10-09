# UCOB repairs — 2026-10-06

BossMod 3D was disabled on the user's PC. That explains some missing in-game drawings; this GitHub review cannot establish which remaining scripts were installed or enabled. Triggevent is not required by these scripts.

Changes:
- Recovered Grand Octet v3, Tenstrike v3 and Exaflare v3 from commit 2e8052d. Preserved the requested Tank LB3 strategy and 22.5-degree CCW Twin bait. Older retired scripts stay retired; enabling them will not provide equivalent guidance.
- Thunder v1/v2, metadata revision 3: yellow 5-yalm ring; match any actor with status 466, as in the official Splatoon Thunderstruck preset, instead of only the local player. Status filtering still controls visibility. Install only one Thunder version.
- Heavensfall revision 107: explicit green arrow from the radius-9 knockback spot to the assigned tower. Preserves fifth clockwise from Nael, including Nael's tower. Eight tower casts can recover tower selection after a missed opener; the knockback standing cue is withheld if the opening cast was not observed, because knockback timing is then unknown.
- Nael Two Markers revision 6: visible Nael name-ID lookup and red line during targetable Nael phase, in addition to existing Phase 3 markers.
- Nael Dive revision 4: repaired invalid literal backslash-n in both embedded JSON definitions. Its old positioning strategy was not newly validated; it is not part of the recommended clean installation.

Current validation:
- Compiled all eight restored/modified script files against Splatoon 3.9.2.28 and current Dalamud API 15 assemblies with .NET 10.0.100: zero errors, zero warnings.
- Recovered-script test harness: 85 assertions pass. Updated the stale normal-stack expectation to match the previously requested Tank LB3 tower prompt. This uses test adapters, not a game replay.
- All embedded UCOB marker JSON parsed successfully.
- No live fight or ACT replay test was performed. A compile does not prove encounter correctness or visibility on the user's PC.

Recommended standalone scripts (install their raw GitHub URLs in Splatoon; avoid duplicate versions):
1. UCOB_Thunder_Accessible_v2.cs
2. UCOB_Nael_Two_Markers.cs
3. UCOB_Heavensfall_Towers_Accessible.cs
4. UCOB_Grand_Octet_Accessible_v3.cs
5. UCOB_Tenstrike_Accessible_v3.cs — arrows require the saved personal positions described below.
6. UCOB_Exaflare_Accessible_v3.cs — conservative follow-up circle, not a complete opening solver.

Sources checked: PunishXIV/Splatoon main, `Presets/Stormblood/Duties/Ultimate - The Unending Coil of Bahamud.md`, `Splatoon/Utility/LayoutUtils.cs`, DirectX11Renderer.cs, and official Heavensfall Trio Towers script. BossMod UCOB enums and Earthshaker/Grand Octet/Heavensfall handlers were also reviewed.

The following is historical documentation; the validation above supersedes earlier build-status statements.

---

# UCOB accessibility review builds — 2026-09-27

These are standalone Splatoon scripts for territory 733. They compile against Splatoon 3.9.2.25 and Dalamud API 15. They have not been exercised in an in-game replay or live pull. Disable older copies covering the same mechanic before testing these. UWU Annihilation is unchanged.

## Grand Octet revision 2 — 2026-10-01 (supersedes Grand Octet notes below)

Maggie confirmed the Tank LB3 strategy and requested a bait spot halfway between N and NW when Twintania is north. The bait destination now rotates 22.5 degrees counterclockwise from Twintania, retaining the previous radius of 20. This is a user-selected location, not a newly measured or log-verified safe coordinate.

The personal bait arrow remains until Twintania's icon locks the dive. The normal stack branch is removed: other players receive FILL AN OPEN TOWER, regardless of stack icons. After the bait locks, the bait player also receives that prompt. Tower positions are not drawn or assigned. The script assumes the party uses Tank LB3; it does not detect that mitigation. Existing encounter triggers, early Octet guidance, death/missing-icon safeguards and tower-resolution Twister prompt are unchanged.

Validation for this revision: reviewed the narrow diff and checked the 22.5-degree CCW geometry in all eight orientations. No current compilation, ACT replay or in-game validation was possible. The older compilation and 85 assertions below apply only to the original revision; the old normal-stack test expectations have not been updated. This remains a draft, not a verified release.

## Install / first check

Install each v3 `.cs` raw GitHub URL through Splatoon's script installer. Open the script's settings while out of combat and press **Show display test for 5 seconds**. Grand Octet and Tenstrike show a red arrow; Exaflare shows a green circle. Each also shows large text over your character. This only checks rendering, not encounter correctness.

## Grand Octet

Implements the normal resolution described in Tessan's linked guide, not the Tank LB3 alternative:

- Stay center until Nael's marker; move opposite Bahamut. If Nael occupies that spot, skip one 45-degree sector in the movement direction.
- Bahamut cardinal: counterclockwise. Intercardinal: clockwise. When Nael dives, show a short red direction arrow along the rim and a large direction label. The arrow is a direction cue, not a guarantee that its endpoint or the entire route is clear.
- If the initial Nael skip was not needed, show SPRINT after the first dragon dive resolves.
- Bahamut's red marker switches to a red arrow to center immediately.
- After Bahamut dives, exactly seven different observed dive targets identify the remaining Twin bait player. Deaths or missing markers suppress that inference.
- Twin bait goes CCW of Twin, even if also marked for stack. On Twin's marker, the bait can leave: stack goes CW, otherwise text directs them to their closest tower. Other non-stack players get an open-tower reminder; towers are not assigned automatically.
- Tower resolution shows MOVE — TWISTERS.

The two Twin wall targets use the adjacent 45-degree sectors at radius 20, a geometric implementation choice consistent with the requested CW/CCW sides; they are not claimed to be coordinates measured from the video. Adapt to the party's precise placement. If boss timeline/position events are missing, the script gives text instead of guessing positions.

## Tenstrike: setup required

The guide's Earthshakers use flexible player claims, not a fixed D3 assignment. No spread coordinates are prefilled or invented. Without setup, the script provides personal first/second-wave prompts only.

Before combat, in UCOB, agree your spots and stand at each to save:

1. Waiting / corner-claim position.
2. First Earthshaker position.
3. Second Earthshaker position, in front of the first puddle.

These saved points are personal preferences, not automatic party assignments. If a marked player aims within 45 degrees of the selected spread direction, the arrow is withheld and ADJUST is shown. This does not predict where that player will move. A different party/claim requires updating the saved positions. Both possible event orders are supported: second-wave icons before or after the first Earthshaker resolution. Hatch is outside this script's scope.

## Golden Bahamut Exaflare: conservative follow-up aid

Green marks a confirmed cleared blast position only after all six lines (three pairs) have been observed. It checks the whole green circle against every remaining tracked blast, requires the direct approach to miss all next blast circles, and hides when event tracking is incomplete. It does not solve the opening dodge or promise a green circle for every pattern; dodge the first pair normally. This is deliberately narrower than a complete Exaflare route solver.

Encounter facts used: radius 6; advances 8 yalms; six blasts per line; subsequent interval 1.5 seconds. The 0.6-yalm marker, 0.2-yalm extra clearance, radius-19.5 placement bound, 250 ms display delay, and 750 ms missing-event tolerance are conservative implementation choices, not encounter timings or positions sourced from logs. Progression follows received action effects; it never advances blasts on a timer. An unrecognized blast hides the marker. This aid considers Exaflares only, not tank-buster positioning or another player's mechanics.

## Sources

- Tessan Twintails, The Unending Coil of Bahamut (Ultimate) — Visual Guide [2024]: https://www.youtube.com/watch?v=EG9NxD6bxWs
  - Retrieved English automatic captions, not a frame-by-frame visual verification. Tenstrike ~31:18–31:54; Grand Octet ~32:52–35:49; Exaflares ~42:20–42:55. Video frame retrieval was unavailable.
- NAUR current PF references: https://naurffxiv.com/ultimate/ucob
- BossMod pinned source: https://github.com/awgil/ffxiv_bossmod/tree/8349c384c845cf598e862f34a5a05635e04eab1d/BossMod.Ultimate/Stormblood/Ultimate/UCOB
  - UCOBEnums.cs: IDs; P3GrandOctet.cs: positioning timeline and dive icon tracking; P3EarthShaker.cs: queued waves; P5Exaflare.cs and UCOBStates.cs: moving blasts and three pairs.
- Splatoon API/rendering source: https://github.com/PunishXIV/Splatoon/tree/fdf762432d0e74ed122495f64e42f32477404838
  - ActorControlProcessor passes TargetIcon (34) and timeline (407) events through. LineEndB=Arrow points to the destination. Element SetRefPosition/SetOffPosition handle world coordinates.

## Validation

- Real plugin assembly compile: .NET 10.0.401, Splatoon 3.9.2.25, Dalamud API 15; zero errors and zero warnings.
- Test adapters compile the actual three script source files. They are not a mock implementation of the scripts. Run `dotnet run --project validation/ucob-tests/Tests.csproj` with .NET 10.
- Tests cover all eight Bahamut sectors, Nael-opposite exceptions, red arrow destination, center timing, bait+stack order, missing icons/death handling, Tenstrike event ordering, no unconfigured arrows, future Exaflare collision rejection, green after confirmed clearance, tracking loss and reset.
- No ACT log replay, live fight validation, rendered in-game visibility check, or empirical coordinate calibration has been performed. Review builds, not a guarantee of a clear.
