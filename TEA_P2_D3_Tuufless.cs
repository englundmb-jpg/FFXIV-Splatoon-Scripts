// D3, Tuufless Tollgate. MT = Brute Justice; OT = Cruise Chaser (user labels).
// Source-reviewed draft; no replay/live validation. See TEA_P2_D3_Tuufless.md.
// No automatic movement. Exact waymark coordinates are from Tuufless's TEA preset.
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.TEA
{
    public sealed class TEA_P2_D3_Tuufless : SplatoonScript
    {
        public override HashSet<uint> ValidTerritories => new() { 887 };
        public override Metadata Metadata => new(1, "Maggie accessibility");
        private const uint Water = 2142, Lightning = 2143;
        private const uint Green = 0xFF00FF00, Cyan = 0xFFFFFF00;
        // X/Z, not screen coordinates. No guessed marker positions.
        private static readonly Vector2 NorthA = new(100f, 88f);
        private static readonly Vector2 SouthC = new(100f, 116f);
        private static readonly Vector2 Southeast3 = new(107.8f, 107.8f);
        private readonly Dictionary<uint, long> effects = new();
        private readonly Dictionary<uint, long> casts = new();
        private HttpClient? voice;
        private volatile string voiceStatus = "Not tested";
        private bool active, ended, failed, enableVoice = true;
        private int waters, photons;
        private bool initialLightningKnown, initialLightningMe;
        private bool secondWaterKnown, secondWaterMe;
        private bool enumerationDone, shieldSeen, flareDone, thirdPassShown;
        private readonly HashSet<uint> mineSources = new();
        private bool pass2Shown, waterCueShown;
        private string current = "", lastCode = "", preview = "";
        private long began, lastStatusRead, previewUntil;
        private uint shieldId;

        public override void OnSetup()
        {
            Controller.RegisterElement("Current", new Element(0)
            {
                Enabled = false, radius = 1.2f, thicc = 6f,
                color = Green, tether = true, overlayFScale = 1.7f
            });
            Controller.RegisterElement("Next", new Element(0)
            {
                Enabled = false, radius = 1.2f, thicc = 4f,
                color = Cyan, tether = false, overlayFScale = 1.5f
            });
        }

        public override void OnEnable()
        {
            OnReset();
            voice = new HttpClient(new HttpClientHandler { UseProxy = false })
                { Timeout = TimeSpan.FromMilliseconds(700) };
        }

        private static long Now => Environment.TickCount64;
        private bool Once(Dictionary<uint, long> seen, uint id, int gap = 1500)
        {
            if (seen.TryGetValue(id, out long at) && Now - at < gap) return false;
            seen[id] = Now;
            return true;
        }

        private void Begin()
        {
            if (active || ended || BasePlayer == null || BasePlayer.CurrentHp == 0) return;
            active = true;
            began = Now;
            Cue("CLOCKS", "D3 CLOSE CLOCK - KEEP NISI SEPARATE");
        }

        public override void OnStartingCast(uint source, uint castId)
        {
            if (castId == 18522) { End(); return; } // Temporal Stasis: never guide P3.
            if (castId == 18494) Begin(); // Judgment Nisi: safe P2 activation fallback.
            if (!active || failed || !Once(casts, castId)) return;
            switch (castId)
            {
                case 18486:
                    Cue(photons == 0 ? "PHOTON" : "TANK_PHOTON",
                        photons == 0 ? "PHOTON - WAIT; NEXT NISI WITH H1" : "TANK PHOTON - KEEP CLEAR");
                    break;
                case 19058:
                    Cue("SPIN", "AVOID CC FRONT - NEXT WATER / LIGHTNING");
                    break;
                case 18509:
                    Cue("MINES", "STAY INWARD, BEHIND CC TANK (OT) - HEALERS BAIT MINES");
                    break;
                case 18513:
                    Cue("ENUM", "WEST ENUMERATION WITH H1 - D1/D2 FLEX; YOU STAY WEST");
                    break;
                case 18491:
                    // Verdict starts BEFORE second water resolves; don't erase the stack cue.
                    if (waters >= 2) Cue("VERDICT", "CHECK REQUIRED NISI - NEXT SHIELD + FIRE");
                    break;
                case 18483:
                    Cue("SHIELD", "KILL PLASMA SHIELD FROM FRONT - MT AIMS FIRE SOUTH");
                    break;
                case 18501:
                    Cue("FLARE", "SHIELD + FLARETHROWER - STAY OUT OF SOUTH FIRE");
                    break;
                case 18482:
                    Cue("LOS", "HIDE BEHIND ICE FROM CC - CHECK FINAL NISI");
                    break;
                case 18492:
                    Cue("GAVEL", "GAVEL CHECK - CORRECT NISI; STAY BEHIND ICE");
                    break;
                case 18503:
                    Cue("JUMP", "D3 SOUTH: BAIT SUPER JUMP - BE FARTHEST; AVOID TANKS");
                    break;
                case 18505:
                    Cue("JUMP", "D3 SOUTH: BAIT SUPER JUMP - BE FARTHEST");
                    break;
                case 18518:
                case 18490:
                    Cue("ENRAGE", "ENRAGE - FINISH BOSSES");
                    break;
            }
        }

        public override void OnActionEffectEvent(ActionEffectSet set)
        {
            if (set.Action is not { } action) return;
            uint id = action.RowId;
            if (id == 18516) Begin(); // J Kick ends Limit Cut intermission.
            if (!active || failed) return;
            // One mine can produce more than one target packet; count unique helpers.
            if (id == 18514)
            {
                if (set.Source != null) mineSources.Add(set.Source.EntityId);
                CheckSecondPass();
                return;
            }
            if (!Once(effects, id, id == 18496 ? 10000 : 1500)) return;
            switch (id)
            {
                case 18495:
                    if (waters == 0) Cue("OUT", "MOVE OUT - DODGE CHAKRAMS");
                    break;
                case 18479:
                    Cue("IN", "MOVE IN - RETURN TO YOUR CLOCK; KEEP NISI SEPARATE");
                    break;
                case 18486:
                    photons++;
                    if (photons == 1) Cue("PASS1", "FIRST NISI WITH H1 - THEN SEPARATE");
                    else Cue("TANK_PHOTON", "TANK PHOTON - KEEP CLEAR; NEXT SOUTH JUMP BAIT");
                    break;
                case 18496: // Crashing Wave only: Thunder must NOT advance water count.
                    waters++;
                    waterCueShown = false;
                    if (waters == 1) Cue("MINES", "STAY INWARD, BEHIND CC TANK (OT) - NO MINE BAIT");
                    else if (waters == 2) Cue("VERDICT", "CHECK REQUIRED NISI - NEXT SHIELD + FIRE");
                    else if (waters == 3) Cue("PASS4", "FOURTH NISI: MATCH REQUIRED TYPE - THEN HIDE BEHIND ICE");
                    else Fail();
                    break;
                case 18512:
                    enumerationDone = true;
                    Cue("WAIT_MINES", "ENUMERATION DONE - WAIT FOR TANKS TO TAKE BOTH MINES");
                    CheckSecondPass();
                    break;
                case 18502:
                    flareDone = true;
                    break;
                case 18492:
                    Cue("GAVEL_DONE", "NISI FINISHED - NEXT TANK PHOTON AND TANKBUSTER");
                    break;
                case 18506:
                    Cue("RAY", "JUMP LANDED - WATCH AND DODGE APOCALYPTIC RAY");
                    break;
                case 18508:
                    Cue("RAY", "DODGE APOCALYPTIC RAY");
                    break;
                case 18882:
                    if (waters == 3 && photons >= 2) Cue("FINISH", "RAIDWIDE - FINISH BOSSES");
                    break;
                case 18515: // Failed mine, don't advance to a purported safe route.
                case 18498:
                case 18499:
                    Fail();
                    break;
            }
        }

        private void CheckSecondPass()
        {
            if (pass2Shown || waters != 1 || !enumerationDone || mineSources.Count < 2) return;
            pass2Shown = true;
            Cue("PASS2", "SECOND NISI WITH H1 - WATCH ICE AND FIRE ON THE WAY");
        }

        private void ReadHolders()
        {
            if (Now - lastStatusRead < 100) return;
            lastStatusRead = Now;
            var players = Svc.Objects.OfType<IPlayerCharacter>().Where(p => p.CurrentHp > 0).ToArray();
            if (waters == 0)
            {
                var holders = players.Where(p => p.StatusList.Any(s => s.StatusId == Lightning && s.RemainingTime > 0)).ToArray();
                if (holders.Length == 1)
                {
                    initialLightningKnown = true;
                    initialLightningMe = holders[0].EntityId == BasePlayer?.EntityId;
                }
            }
            if (waters == 1)
            {
                // Ignore the outgoing first debuff around its removal packet.
                var holders = players.Where(p => p.StatusList.Any(s => s.StatusId == Water && s.RemainingTime > 5)).ToArray();
                if (holders.Length == 1)
                {
                    secondWaterKnown = true;
                    secondWaterMe = holders[0].EntityId == BasePlayer?.EntityId;
                }
            }
        }

        private float WaterRemaining()
        {
            var times = Svc.Objects.OfType<IPlayerCharacter>()
                .SelectMany(p => p.StatusList)
                .Where(s => s.StatusId == Water && s.RemainingTime > 0)
                .Select(s => s.RemainingTime).ToArray();
            return times.Length == 1 ? times[0] : -1;
        }

        private void CheckShield()
        {
            if (waters != 2 || thirdPassShown) return;
            var shield = Svc.Objects.OfType<IBattleChara>().FirstOrDefault(x => x.DataId == 0x2C4F);
            if (shield != null && shield.CurrentHp > 0 && shield.IsTargetable)
            {
                shieldSeen = true;
                shieldId = shield.EntityId;
            }
            // Require an observed dead shield. Disappearance/untargetability alone is not proof.
            if (shieldSeen && flareDone && shield != null && shield.EntityId == shieldId && shield.CurrentHp == 0)
            {
                thirdPassShown = true;
                Cue("PASS3", "THIRD NISI: GIVE / COLLECT REQUIRED TYPE AT TANK-HEALER LINE");
            }
        }

        public override void OnUpdate()
        {
            HideMarkers();
            if (Now < previewUntil && !active)
            {
                var p = preview == "A" ? NorthA : preview == "C" ? SouthC : Southeast3;
                Marker("Next", p, "PREVIEW ONLY - " + preview);
                return;
            }
            if (!active) return;
            if (BasePlayer == null || BasePlayer.CurrentHp == 0 || Now - began > 240000) { End(); return; }
            if (failed)
            {
                Controller.DisplayAttentionWindowLine(new Vector4(1, 1, 0, 1), "ROUTE STOPPED - MISSING / FAILED MECHANIC; USE GROUP CALLS");
                return;
            }
            ReadHolders();
            CheckShield();
            float left = WaterRemaining();
            bool imminent = left > 0 && left <= 5 && waters < 3;
            if (imminent && !waterCueShown)
            {
                if (waters == 0)
                    Cue(!initialLightningKnown ? "CHECK1" : initialLightningMe ? "LIGHTNING" : "WATER1",
                        !initialLightningKnown ? "CHECK LIGHTNING - MT IF YOURS; OTHERWISE WATER NORTH A" :
                        initialLightningMe ? "LIGHTNING: SHARE WITH MT (BRUTE JUSTICE TANK)" : "FIRST WATER - NORTH A");
                else if (waters == 1)
                    Cue("WATER2", "SECOND WATER - SOUTH C; ALL DPS");
                else
                    Cue(!secondWaterKnown ? "CHECK3" : secondWaterMe ? "SKIP3" : "WATER3",
                        !secondWaterKnown ? "CHECK SECOND WATER HISTORY - THIRD WATER ONLY IF ELIGIBLE" :
                        secondWaterMe ? "SKIP THIRD WATER - RETURN WEST AFTER NISI" : "THIRD WATER - SOUTHEAST 3 WITH H2");
                waterCueShown = true;
            }

            // Cyan marks a later destination, NEVER permission to move or cross another player.
            if (waters == 0 && photons == 1 && initialLightningKnown && !initialLightningMe)
                Marker(imminent ? "Current" : "Next", NorthA, imminent ? "WATER NOW - A" : "NEXT WATER - A");
            else if (waters == 1 && enumerationDone)
                Marker(imminent ? "Current" : "Next", SouthC, imminent ? "WATER NOW - C" : "NEXT WATER - C");
            else if (waters == 2 && secondWaterKnown && !secondWaterMe)
                Marker(imminent ? "Current" : "Next", Southeast3, imminent ? "WATER NOW - 3" : "NEXT WATER - 3");
            if (lastCode == "JUMP")
                Marker("Current", SouthC, "SOUTH JUMP BAIT - BE FARTHEST");
            if (current != "") Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), current);
        }

        private void Marker(string name, Vector2 p, string label)
        {
            if (!Controller.TryGetElementByName(name, out var e)) return;
            e.refX = p.X;
            e.refY = p.Y; // Splatoon refY is world Z.
            e.overlayText = label;
            e.Enabled = true;
        }
        private void HideMarkers()
        {
            foreach (var e in Controller.GetRegisteredElements().Values) e.Enabled = false;
        }
        private void Cue(string code, string text)
        {
            current = text;
            if (lastCode == code) return;
            lastCode = code;
            if (enableVoice) _ = SayAsync(code);
        }
        private async Task SayAsync(string code)
        {
            var client = voice;
            if (client == null) return;
            try
            {
                using var body = new StringContent("MAGGIE_TEA_D3_TUUFLESS|" + code, Encoding.UTF8, "text/plain");
                using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                voiceStatus = "Sent to Triggernometry";
            }
            catch (Exception) { voiceStatus = "Check Triggernometry endpoint: localhost:51423"; }
        }
        private void Fail() { failed = true; HideMarkers(); Cue("CHECK", "CHECK MECHANICS - FOLLOW GROUP CALLS"); }
        private void End() { active = false; ended = true; current = ""; previewUntil = 0; HideMarkers(); }
        public override void OnReset()
        {
            active = ended = failed = false;
            waters = photons = 0;
            initialLightningKnown = initialLightningMe = secondWaterKnown = secondWaterMe = false;
            enumerationDone = shieldSeen = flareDone = thirdPassShown = pass2Shown = waterCueShown = false;
            current = lastCode = preview = "";
            began = lastStatusRead = previewUntil = 0;
            shieldId = 0;
            effects.Clear(); casts.Clear(); mineSources.Clear();
            HideMarkers();
        }
        public override void OnCombatEnd() => OnReset();
        public override void OnDisable() { OnReset(); voice?.Dispose(); voice = null; }
        public override void OnSettingsDraw()
        {
            ImGui.TextWrapped("D3 Tuufless Tollgate only. MT = Brute Justice; OT = Cruise Chaser. A north, C south, 3 southeast. Exact preset positions; this does not place your game's waymarks.");
            ImGui.TextWrapped("Draft: event IDs source-checked; compilation and live timing not verified. Floor markers cover water and a south jump reference. Clocks, Enumeration, Nisi routes, shield-front and ice hiding need your actual positioning; no automatic safe-path solver.");
            ImGui.TextWrapped("H1 for first two Nisi passes. Cyan = NEXT, not move now. Green water appears with 5 seconds left on the actual water debuff. The line indicates destination, not a safe route. Second Nisi waits for Enumeration and two distinct mine soaks; third cue needs an observed dead shield and resolved fire. Missing data produces no guessed pass cue.");
            ImGui.TextWrapped("Import Triggernometry_TEA_P2_D3_Tuufless.xml. Keep your Nisi identity helper. Disable overlapping Photon/Verdict calls in older voice packs if needed.");
            ImGui.Checkbox("Voice (this session)", ref enableVoice);
            ImGui.TextWrapped("Voice: " + voiceStatus);
            if (ImGui.Button("TEST D3 VOICE")) _ = SayAsync("TEST");
            if (!active)
            {
                if (ImGui.Button("Preview A")) { preview = "A"; previewUntil = Now + 5000; }
                if (ImGui.Button("Preview C")) { preview = "C"; previewUntil = Now + 5000; }
                if (ImGui.Button("Preview 3")) { preview = "3"; previewUntil = Now + 5000; }
            }
        }
    }
}
