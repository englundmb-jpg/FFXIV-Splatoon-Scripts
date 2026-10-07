# UCOB Bard reaction calls v1

Import Triggernometry_UCOB_Bard_Reaction_Calls.xml into ACT > Plugins > Triggernometry > Local triggers (right-click, Import). Disable the old Maggie - Two Minute Burst Reminder folder. Keep your mechanic triggers enabled.

These are proposed phase-based personal cues, not an independently verified raid-leader burst plan. No actions or songs are pressed for you. Continue regular attacks when asked to hold burst. Hold means save Raging Strikes, Battle Voice and Barrage.

## What it says

- "Burst in five", then "Three", "Two", "One", "Burst". It checks your recorded Raging Strikes and Battle Voice uses before starting the countdown. The next window is skipped if either will still be unavailable. Your actual use of either buff cancels remaining countdown speech.
- "Hold burst" before the next predicted Bahamut disappearance, approximately 20 seconds beforehand.
- "Hold burst. Spend Pitch if you have it" at a trio cast, approximately six seconds before disappearance. Use Pitch Perfect only when available; the XML does not read your Pitch stacks or change songs.
- "Boss low. Hold burst. Keep attacking" at 10% HP on Twintania in phase 1 or Nael in phase 2. This is a conservative configurable HP heuristic, NOT a prediction of exact remaining uptime.
- "Hold burst for Golden" at Teraflare. Golden opening countdown uses the glowing-ball action as its timing anchor. A subsequent countdown is anchored to your actual Raging Strikes use in Golden, not to pull time.

There is no automatic opening-pull burst instruction: use your normal opener. This version does not offer a complete Nael dive-phase song plan, low-HP adds holding, or an optimized party-specific buff schedule. It does not require Radiant Finale (unavailable at level 70).

## Planned window timings

All timings below are the final "Burst" cue; preparation starts five seconds earlier. They are target-uptime opportunities, subject to the two cooldown checks, not a demand to use buffs on every return.

| Anchor | Burst cue after anchor |
| --- | ---: |
| Nael-entry Ragnarok Heavensfall action 26B8, only in phase 1 | 14 s |
| Seventh Umbral Era action 26D1 | 12 s |
| Quickmarch cast 26E2 | 18 s |
| Blackfire cast 26E3 | 23 s |
| Fellruin cast 26E4 | 26 s |
| Heavensfall cast 26E5 | 34 s |
| Tenstrike cast 26E6 | 32 s |
| Adds Bahamut's Favor action 26E8 | 6 s |
| Golden glowing-ball action 2707 | 15 s |

## Validation and limits

XML parsed successfully. Regexes, captured HP fields, phase guards, cumulative action delays, cancellation and cooldown gating were checked with a local event-queue simulation using Network_30301_20261006.log. This is not a live Triggernometry import or audio test. The source log reaches adds but not Golden; Golden timing is based on the cactbot encounter timeline. Triggernometry's actual scheduling/TTS can add latency. This version assumes the English client and the normal level-70 120-second Raging Strikes/Battle Voice recasts. It cannot infer party members' future decisions or recover missed cooldown-use events from before import. Import before the pull; Engage initializes the state.

Wipe/recommence, clear, new countdown and zone-change events cancel queued actions. The whole folder is restricted to UCOB territory 733. Your player name is resolved automatically; no name edit is required.

For an out-of-combat sound check, add a temporary simple TTS test using Triggernometry's UI; do not forcibly execute phase triggers during a pull.

Sources:
- https://github.com/quisquous/cactbot/blob/main/ui/raidboss/data/04-sb/ultimate/unending_coil_ultimate.txt
- https://github.com/paissaheavyindustries/Triggernometry/blob/master/Source/Triggernometry/RealPlugin.cs (QueueActions uses cumulative delays)
- https://github.com/paissaheavyindustries/Triggernometry/blob/master/Source/Triggernometry/Folder.cs (territory restriction)
- https://github.com/paissaheavyindustries/Triggernometry/blob/master/Source/Triggernometry/Context.cs (local player and variable expressions)
