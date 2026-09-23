# UCOB drawing audit and proposed cleanup

Audited all 28 UCOB C# scripts at repository commit `0558649a39aba94e267536aa962da3eefe5938c2`. The Bard timeline is rotation configuration and is outside this drawing cleanup.

## What this revision does

- Preserves `UCOB_Nael_Two_Markers.cs` byte-for-byte, including the user-confirmed red line, yellow marker, and center marker.
- Retires seven unreliable drawing implementations and the old duplicate Thunder implementation. Retirement preserves each class identity, raises its metadata version, and registers no drawings. It does not implement replacement safe spots for those mechanics.
- Keeps Thunder status detection (466), and preserves the original large green circle and THUNDER label as requested by the user. Triggernometry remains responsible for the existing speech callouts.
- Restores `UCOB_Heavensfall_Towers_Accessible.cs` with one cyan tether to the assigned tower, no additional circles. Counts the tower closest to Bahamut as 1 and takes the fourth counterclockwise. A tower directly at Bahamut is therefore 1. Identifies the real Bahamut by DataId 0x1FE8.

## Verified defects

1. Nael Dive has literal backslash-n sequences outside JSON strings in both raw drawing definitions, making those definitions invalid JSON. Its activation depends on Nael being targetable, not a dive event, and it reactivates after every ten-second reset. The offsets are not a personal dive assignment resolver.
2. Liquid Hell activates from Twintania's presence, resets and reactivates every thirty seconds, and computes points 17/21 yalms farther out from the boss along the center-to-boss ray. With a noncentral boss, its outer point exceeds the 21-yalm arena radius. It never determines the player's Liquid Hell targeting.
3. Megaflare uses 9953, which is Megaflare Dive, then enables a radius-eight circle at the default center. It never derives a safe position from the dive origin or direction.
4. Both Adds Hatch versions use fixed locations and do not identify the player's Hatch assignment. Boss presence alone is not proof of the adds phase because these actors are present during trios; v2 adds a Twister gate but still uses Nael presence as the phase check.
5. Both Exaflare versions duplicate an actor-relative drawing restricted to the first 0.25 seconds of cast 9968. The published preset includes a trigger and a freeze lifecycle that these scripts omit. Consequently the copied drawing is not equivalent to the original preset's persistent route.
6. Thunder v1/v2 duplicate the same personal ring.

## Tower lifecycle and scope

The published Splatoon tower resolver detects exactly eight actors casting 9951. This revision follows that condition. The line is hidden unless exactly eight such actors exist, combat is active, and the unique Bahamut actor is identifiable. Missing, centered, or ambiguous reference data produces no line. The assignment is locked while that group is casting so subsequent boss movement cannot change it. Reset, leaving combat, or loss of the eight-cast group hides the line and clears the selection. The line points to the tower destination; it is not a knockback start-position calculation and does not promise a collision-free route.

## Preserved items that still need encounter evidence

The working Nael script uses a fixed yellow south marker at Z=8.75 and timed center/phase windows. These are preserved because the user reports the script works; they are not newly verified as dynamic Neurolink tracking. The Bahamut transition script likewise uses a fixed south coordinate and seven-second display. That script is unchanged pending a replay/log check of the party's actual placement.

## Other copies outside this repository

The separately delivered `UCOB_Rest_D3_Accessible_v1.cs`, `UCOB_Rest_D3_Accessible_v2.cs`, and `UCOB_Nael_R1_Accessible_v1.cs` are not in this GitHub snapshot. They can continue drawing independently if installed. Updating GitHub cannot remove locally installed or separately downloaded copies. Do not run the old remainder tower resolver alongside the proposed dedicated tower script.

## Validation and limitations

Passed: all six remaining embedded JSON drawing definitions parse; the working Nael file is byte-identical; 24 tower direction simulations cover all eight bearings, nearest-tower offsets, shuffled object enumeration and angle wraparound; git diff whitespace checks.

Not performed: compilation against the user's installed Splatoon/Dalamud assemblies, replay of the user's ACT logs, and a live encounter test. Those assemblies and UCOB logs are not in this checkout. This is a review draft, not a fully verified replacement for every removed mechanic. A simple visible line test must precede asking the user to test a full encounter.

## Sources

- User repository baseline: https://github.com/englundmb-jpg/FFXIV-Splatoon-Scripts/tree/0558649a39aba94e267536aa962da3eefe5938c2
- BossMod IDs and arena at commit 076003c0510a6b82b5d7b1a1d12438bc55ee3a75: https://github.com/awgil/ffxiv_bossmod/blob/076003c0510a6b82b5d7b1a1d12438bc55ee3a75/BossMod.Ultimate/Stormblood/Ultimate/UCOB/UCOBEnums.cs and UCOB.cs in the same directory.
- Published Splatoon tower resolver: https://github.com/PunishXIV/Splatoon/blob/main/SplatoonScripts/Duties/Stormblood/UCOB%20Heavensfall%20Trio%20Towers.cs
- Published Exaflare preset with freeze lifecycle: https://github.com/NightmareXIV/Splatoon/blob/master/Presets/Stormblood%20content/Duties/Ultimate%20-%20The%20Unending%20Coil%20of%20Bahamud.md

## Complete C# inventory

| File | Proposed state |
| --- | --- |
| `UCOB_Accessible_Clean_Bundle.cs` | Retired; no drawings |
| `UCOB_Adds_D3_Hatch_Accessible.cs` | Retired; no drawings |
| `UCOB_Adds_D3_Hatch_Accessible_v2.cs` | Retired; no drawings |
| `UCOB_Bahamut_Transition_MCH_Accessible.cs` | Unchanged; fixed position remains unverified |
| `UCOB_Blackfire_Accessible.cs` | Retired; no drawings |
| `UCOB_Blackfire_Accessible_v2.cs` | Retired; no drawings |
| `UCOB_D3_Liquid_Hell_Accessible.cs` | Retired; no drawings |
| `UCOB_Elemental_D3_Clean.cs` | Retired; no drawings |
| `UCOB_Exaflare_Accessible.cs` | Retired; no drawings |
| `UCOB_Exaflare_Accessible_v2.cs` | Retired; no drawings |
| `UCOB_Fellruin_Accessible.cs` | Retired; no drawings |
| `UCOB_Fellruin_Accessible_v2.cs` | Retired; no drawings |
| `UCOB_Grand_Octet_Accessible.cs` | Retired; no drawings |
| `UCOB_Grand_Octet_Accessible_v2.cs` | Retired; no drawings |
| `UCOB_Heavensfall_Knockback_Accessible.cs` | Retired; no drawings |
| `UCOB_Heavensfall_R1_Accessible.cs` | Retired; no drawings |
| `UCOB_Heavensfall_R1_Accessible_v2.cs` | Retired; no drawings |
| `UCOB_Heavensfall_Towers_Accessible.cs` | One tower line; draft, not live-tested |
| `UCOB_Megaflare_Accessible.cs` | Retired; no drawings |
| `UCOB_Nael_Dive_Accessible.cs` | Retired; no drawings |
| `UCOB_Nael_Two_Markers.cs` | Preserved byte-for-byte; user-confirmed working |
| `UCOB_Phase3_D3_Accessible_v4.cs` | Retired; no drawings |
| `UCOB_Quickmarch_Accessible.cs` | Retired; no drawings |
| `UCOB_Quickmarch_Accessible_v2.cs` | Retired; no drawings |
| `UCOB_Tenstrike_Accessible.cs` | Retired; no drawings |
| `UCOB_Tenstrike_Accessible_v2.cs` | Retired; no drawings |
| `UCOB_Thunder_Accessible.cs` | Retired; no drawings |
| `UCOB_Thunder_Accessible_v2.cs` | Original large personal Thunder circle retained |
