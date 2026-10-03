# UWU repair candidates — 27 September 2026

These are compiled review builds, not a certification that every mechanic has been replayed or tested in game. Do not describe this set as a complete repair of all UWU scripts.

## Scope

- Annihilation is unchanged, byte for byte (blob `21f2b2903f12790a66d8ef3e74456903deb47b62`).
- Ifrit Dash v4: tracks nail-death action effects, rejects invalid kill orders, requires Awoken status and the pre-dash Flaming Crush, calculates short/long and CW/CCW movement, and advances on the first dash's action effect. This module covers the four-dash sequence, not the entrance or earlier cardinal clone dashes. Nearest valid side is chosen from the player's current position; the party must already be together. Sprint is required for the fast route.
- Predation v2: reads actual boss positions/rotations and checks both waves of hazards. Uses a 19y sampled circle with a 0.75y hazard margin, rather than treating an arbitrary cardinal point as safe. No route is drawn when actor identification or geometry is ambiguous. If one computed location is safe for both waves, only CURRENT appears. Otherwise NEXT becomes CURRENT on Crimson Cyclone's action effect. This is a dynamic geometric solution and can differ from the old cardinal bait route.
- Roulette v3: activates only on Ultima's triple Viscous Aetheroplasm action effect (`2B8F`). Reacts to the three actual summon effects, then the mechanic casts/effects. Removes the early-Ifrit behavior from this module so it cannot overlap the Ifrit phase or Annihilation. Titan alternates north/NW/north/NW from actual puddle cast volleys. Garuda's former A position was inside Wicked Wheel; the new position is 9.7y north of the actual caster and moves in only after Wheel resolves. Ifrit moves north to NW on the first Eruption puddle cast.
- Suppression is unchanged and remains incomplete. Its existing fixed timer route must not be presented as assignment-aware or verified. A reliable early mapping for Eruption and Light Pillar targets, plus the exact strategy/waymark layout, is still missing. A replay/ACT log from a complete Suppression sequence is needed to validate that mapping before coding it. No made-up marker/VFX identifiers have been added.
- The empty Ifrit CastID Logger is unchanged.

## Display test

Each changed script has `Show display test for 5 seconds` in its settings. Outside combat it draws green CURRENT at the player, cyan NEXT two yalms east, and large `DISPLAY TEST ONLY` text over the player. It stops after five seconds and clears its state. This validates visibility only; it does not simulate or certify the fight. The button is unavailable during combat. Install one changed script at a time and do this test before relying on its fight markers.

## Event and position audit

| Script | Start / prerequisite | Position and variation handling | Advance | Clear |
|---|---|---|---|---|
| Ifrit | Nail death `2B58` action effects; Flaming Crush `2B5D` action effect; first `2B5F` cast from the first nail axis; real Ifrit status 1529 | Actual nail positions; valid CW/CCW axis sequence; real Awoken Ifrit axis selects 45° or 90°; closest equivalent side | First caster's `2B5F` action effect | Four distinct dash sources plus `2B60` cross; Titan `2CFD` cast; Ultima casts; reset/combat end; 15s watchdog |
| Predation | Ultima `2B76` cast; position sample after 10s, matching cactbot's delay | Known boss entity IDs, or unique/Woken fallback; actual shapes/rotations; 19y circle; first and second endpoints checked separately | Tracked Ifrit `2B5F` action effect | `2B60` cross effect after first wave; reset/combat end; 26s watchdog |
| Roulette | Ultima `2B8F` action effect, then `2CD3/4/5` summon effects | Three summon variants handled by events, independent of order; NAUR north/NW coordinates; Garuda position relative to caster | Wheel `2B4E` effect; Eruption `2B5A` ground cast; Weight `2B65` ground casts clustered into volleys | Hellfire/Earthen Fury return north; Feather Rain hides current marker and warns; enrage `2B8C`; reset/combat end; 90s watchdog |

Watchdog times and the 1.5s Titan volley debounce are software safeguards, not invented movement deadlines. No timed safety transition depends on those watchdogs. Cast starts and action effects are separate callbacks.

## Validation performed

- Compiled the changed source files against the actual Splatoon 3.9.2.25 distribution and Dalamud staging assemblies with .NET SDK 10.0.401. Zero errors; compatibility warnings include the still-supported DataId-to-BaseId rename. This is not a build against Maggie's exact local plugin installation.
- Pure C# checks: 64 valid Ifrit first-nail/rotation/Awoken combinations, invalid nail-order rejection, the public cactbot Predation ACT example and its four rotational equivalents, geometric hazard exclusions, a travel-distance bound for those samples, and invalid-geometry rejection. The source snippets under test are copied from the repair build. Run `dotnet run --project PureTests.csproj` to reproduce them.
- Inspected event IDs and event types against the sources below. **Maggie's own UWU ACT log was not available for replay.** Roulette's actor/event delivery, all real Predation permutations, and the full Ifrit route timing remain untested in game. No complete-fight test is requested before the display test succeeds.

## Sources

- [cactbot UWU implementation](https://github.com/quisquous/cactbot/blob/main/ui/raidboss/data/04-sb/ultimate/ultima_weapon_ultimate.ts): nail death order, Awoken rotation algorithm, Predation sample log, and summon events.
- [BossMod UWU](https://github.com/awgil/ffxiv_bossmod/tree/8349c384c845cf598e862f34a5a05635e04eab1d/BossMod.Ultimate/Stormblood/Ultimate/UWU): P4UltimatePredation hazard geometry, P2CrimsonCyclone, P1WickedWheel, and UWUEnums. Predation geometry is adapted from Andrew Gilewsky's BSD-3-Clause implementation; the license is included alongside this note.
- [Splatoon API and examples](https://github.com/PunishXIV/Splatoon/tree/fdf762432d0e74ed122495f64e42f32477404838): callback definitions, element positioning, action-effect event usage.
- [NAUR UWU](https://naurffxiv.com/ultimate/uwu): north/NW waymark coordinates.
- [Tuufless Roulette guide and diagrams](https://ffxiv.tuufless.com/elemental/uwu/04d_primal_roulette/): out/in, north-to-NW and alternating Titan movements.
- [Tuufless Suppression](https://ffxiv.tuufless.com/elemental/uwu/04c_suppression/): distinct randomized assignments, demonstrating why a universal timer route is insufficient.
