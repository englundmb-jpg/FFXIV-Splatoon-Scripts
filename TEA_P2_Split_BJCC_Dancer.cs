// Split BJ/CC, Dancer path: https://ff14.toolboxgaming.space/?id=340414049443951&preview=1
using FFXIVClientStructs.FFXIV.Client.Game.UI;
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
    public sealed class TEA_P2_Split_BJCC_Dancer : SplatoonScript
    {
        public override HashSet<uint> ValidTerritories => new() { 887 };
        public override Metadata Metadata => new(1, "Maggie accessibility");
        private const uint Water = 2142, Lightning = 2143;
        private const uint Green = 0xFF00FF00, Cyan = 0xFFFFFF00;
        private readonly HashSet<uint> enumerationTargets = new();
        private bool minesPlaced;
        private string enumerationSide = "";
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
        private bool pass2Shown, waterCueShown, freshWater, shieldDead;
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
            Cue("CLOCKS");
        }

        public override void OnStartingCast(uint source, uint castId)
        {
            if (castId == 18522) { End(); return; } // Temporal Stasis: never guide P3.
            if (castId == 18494) Begin(); // Judgment Nisi: safe P2 activation fallback.
            if (!active || failed || !Once(casts, castId)) return;
            switch (castId)
            {
                case 18486:
                    Cue(photons == 0 ? "PHOTON" : "TANK_PHOTON");
                    break;
                case 19058:
                    Cue("SPIN");
                    break;
                case 18509:
                    Cue("MINES");
                    break;
                case 18513:
                    minesPlaced = true;
                    CallEnumeration();
                    break;
                case 18491:
                    // Verdict starts BEFORE second water resolves; don't erase the stack cue.
                    if (waters >= 2) Cue("VERDICT");
                    break;
                case 18483:
                    Cue("SHIELD");
                    break;
                case 18501:
                    Cue("FLARE");
                    break;
                case 18482:
                    Cue("LOS");
                    break;
                case 18492:
                    Cue("GAVEL");
                    break;
                case 18503:
                    Cue("JUMP");
                    break;
                case 18505:
                    Cue("JUMP");
                    break;
                case 18518:
                case 18490:
                    Cue("ENRAGE");
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
                    if (waters == 0) Cue("OUT");
                    break;
                case 18479:
                    Cue("IN");
                    break;
                case 18486:
                    photons++;
                    if (photons == 1) Cue("PASS1");
                    else Cue("TANK_PHOTON");
                    break;
                case 18496: // Crashing Wave only: Thunder must NOT advance water count.
                    waters++;
                    freshWater = false;
                    waterCueShown = false;
                    if (waters == 1) Cue("MINES");
                    else if (waters == 2) Cue("VERDICT");
                    else if (waters == 3) Cue(FinalLineup());
                    else Fail();
                    break;
                case 18512:
                    enumerationDone = true;
                    if (mineSources.Count < 2) Cue("WAIT_MINES");
                    CheckSecondPass();
                    break;
                case 18502:
                    flareDone = true;
                    break;
                case 18492:
                    Cue("GAVEL_DONE");
                    break;
                case 18506:
                    Cue("RAY");
                    break;
                case 18508:
                    Cue("RAY");
                    break;
                case 18882:
                    if (waters == 3 && photons >= 2) Cue("FINISH");
                    break;
                case 18515: // Failed mine, don't advance to a purported safe route.
                case 18498:
                case 18499:
                    Fail();
                    break;
            }
        }

        private string FinalLineup()
        {
            var me = BasePlayer;
            if (me == null) return "PASS4";
            foreach (var status in me.StatusList)
            {
                switch (status.StatusId)
                {
                    case 2224: return "PASS4_BLUE";
                    case 2139: return "PASS4_PURPLE";
                    case 2225: return "PASS4_ORANGE";
                    case 2140: return "PASS4_GREEN";
                }
            }
            return "PASS4";
        }

        private void CheckSecondPass()
        {
            if (pass2Shown || waters != 1 || !enumerationDone || mineSources.Count < 2) return;
            pass2Shown = true;
            Cue("PASS2");
        }

        // 65 is TEA's Enumeration headmarker. Each three-person group needs one marked DPS.
        public override void OnActorControl(uint id, uint cmd, uint p1, uint p2, uint p3,
            uint p4, uint p5, uint p6, uint p7, uint p8, ulong target, byte replaying)
        {
            if (!active || failed || waters != 1 || enumerationDone || cmd != 502 || p1 != 65) return;
            enumerationTargets.Add(id);
            if (minesPlaced) CallEnumeration();
        }

        private void CallEnumeration()
        {
            if (enumerationDone || enumerationSide.Length != 0) return;
            var party = Svc.Party.ToArray();
            var casters = party.Where(p => p.ClassJob.RowId is 25 or 27 or 35 or 42).ToArray();
            if (BasePlayer == null || casters.Length != 1 || enumerationTargets.Count != 2 ||
                !enumerationTargets.All(id => party.Any(p => p.EntityId == id)))
            {
                Cue("ENUM");
                return;
            }
            // Start on the caster side (right). Swap if that pair has zero or two marks.
            bool swap = enumerationTargets.Contains(BasePlayer.EntityId) == enumerationTargets.Contains(casters[0].EntityId);
            enumerationSide = swap ? "ENUM_LEFT" : "ENUM_RIGHT";
            Cue(enumerationSide);
        }

        private void ReadHolders()
        {
            if (Now - lastStatusRead < 100) return;
            lastStatusRead = Now;
            var players = Svc.Objects.OfType<IBattleChara>().Where(p => p.CurrentHp > 0).ToArray();
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
            int count = 0;
            float remaining = -1;
            foreach (var player in Svc.Objects.OfType<IBattleChara>())
            {
                if (player.CurrentHp == 0) continue;
                foreach (var status in player.StatusList)
                {
                    if (status.StatusId != Water || status.RemainingTime <= 0) continue;
                    count++;
                    remaining = status.RemainingTime;
                }
            }
            return count == 1 ? remaining : -1;
        }

        private void CheckShield()
        {
            if (waters != 2 || thirdPassShown) return;
            var shield = Svc.Objects.OfType<IBattleChara>().FirstOrDefault(x => x.BaseId == 0x2C4F);
            if (shield != null && shield.CurrentHp > 0 && shield.IsTargetable)
            {
                shieldSeen = true;
                shieldId = shield.EntityId;
            }
            // Require an observed dead shield. Disappearance/untargetability alone is not proof.
            if (shieldSeen && shield != null && shield.EntityId == shieldId && shield.CurrentHp == 0) shieldDead = true;
            if (shieldDead && flareDone)
            {
                thirdPassShown = true;
                Cue("PASS3");
            }
        }

        public override void OnUpdate()
        {
            HideMarkers();
            if (Now < previewUntil && !active)
            {
                Waymark("Next", preview == "D" ? 3 : 1, "PREVIEW " + preview);
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
            if (left > 5) freshWater = true;
            bool imminent = freshWater && left > 0 && left <= 5 && waters < 3;
            if (imminent && !waterCueShown)
            {
                if (waters == 0)
                    Cue(!initialLightningKnown ? "CHECK1" : initialLightningMe ? "LIGHTNING" : "WATER1");
                else if (waters == 1)
                    Cue("WATER2");
                else
                    Cue(!secondWaterKnown ? "CHECK3" : secondWaterMe ? "SKIP3" : "WATER3");
                waterCueShown = true;
            }

            string marker = imminent ? "Current" : "Next";
            if (waters == 0 && photons == 1 && initialLightningKnown && !initialLightningMe)
                Waymark(marker, 3, "FIRST WATER - D");
            else if (waters == 1 && pass2Shown)
                Waymark(marker, 1, "SECOND WATER - B");
            else if (waters == 2 && thirdPassShown && secondWaterKnown && !secondWaterMe)
            {
                var holder = Svc.Party.FirstOrDefault(p => p.GameObject is IBattleChara c &&
                    c.CurrentHp > 0 && c.StatusList.Any(st => st.StatusId == Water && st.RemainingTime > 0));
                if (holder != null && holder.EntityId != BasePlayer.EntityId)
                    Marker(marker, new Vector2(holder.Position.X, holder.Position.Z), "THIRD WATER HOLDER");
            }
            if (lastCode == "JUMP") Waymark("Next", 1, "BAIT FAR EAST NEAR B");
            if (current != "") Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), current);
        }

        private unsafe void Waymark(string element, int index, string label)
        {
            var controller = MarkingController.Instance();
            if (controller == null) return;
            var mark = controller->FieldMarkers[index];
            if (mark.Active) Marker(element, new Vector2(mark.Position.X, mark.Position.Z), label);
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
        private static readonly Dictionary<string, string> Instructions = new()
        {
            ["CLOCKS"] = "Southwest close clock. Keep Nisi separate.",
            ["PHOTON"] = "Photon. Next, Chaser tank.",
            ["TANK_PHOTON"] = "Tank Photon. Prepare to bait far east.",
            ["PASS1"] = "First Nisi. Chaser tank.",
            ["PASS2"] = "Both mines gone. Second Nisi with Chaser tank.",
            ["MINES"] = "Bait your mine south, to the right of the Chaser tank.",
            ["ENUM"] = "Enumeration. Flex with the other DPS.",
            ["WAIT_MINES"] = "Wait for Chaser tank to take both mines.",
            ["SHIELD"] = "Break the shield from the front.",
            ["FLARE"] = "Avoid east fire. Finish shield.",
            ["PASS3"] = "Third Nisi. Match at the tank and healer line.",
            ["PASS4"] = "Match your color behind ice. Blue, purple, orange, green.",
            ["SKIP3"] = "Skip third water. Prepare your color lineup.",
            ["WATER3"] = "Third water. Stack with the water holder.",
            ["JUMP"] = "Far east near B. Bait jump. Be farthest.",
            ["CHECK1"] = "Check lightning. If yours, Brute tank. Otherwise D.",
            ["WATER1"] = "First water. West at D.",
            ["WATER2"] = "Second water. Northeast at B.",
            ["LIGHTNING"] = "Your lightning. Brute Justice tank.",
            ["IN"] = "Back to southwest close clock.",
            ["TEST"] = "Split Dancer voice ready.",
            ["ENUM_LEFT"] = "Enumeration left. White Mage side.",
            ["ENUM_RIGHT"] = "Enumeration right. Scholar side.",
            ["PASS4_BLUE"] = "Blue. First in line behind ice.",
            ["PASS4_PURPLE"] = "Purple. Second in line behind ice.",
            ["PASS4_ORANGE"] = "Orange. Third in line behind ice.",
            ["PASS4_GREEN"] = "Green. Fourth in line behind ice.",
            ["SPIN"] = "Avoid Chaser front. Next, water or lightning.",
            ["VERDICT"] = "Check required Nisi. Next, shield and fire.",
            ["LOS"] = "Hide behind ice from Chaser.",
            ["GAVEL"] = "Correct Nisi. Stay behind ice.",
            ["ENRAGE"] = "Enrage. Finish bosses.",
            ["OUT"] = "Move out. Dodge Chakrams.",
            ["GAVEL_DONE"] = "Nisi finished. Next, tank busters.",
            ["RAY"] = "Watch and dodge Apocalyptic Ray.",
            ["FINISH"] = "Raidwide. Finish bosses.",
            ["CHECK"] = "Check mechanics. Follow group calls.",
            ["CHECK3"] = "Check if you carried second water before stacking.",
        };

        private void Cue(string code)
        {
            current = Instructions[code];
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
                using var body = new StringContent("MAGGIE_TEA_SPLIT_DNC|" + code, Encoding.UTF8, "text/plain");
                using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                voiceStatus = "Sent to Triggernometry";
            }
            catch (Exception) { voiceStatus = "Check Triggernometry endpoint: localhost:51423"; }
        }
        private void Fail() { failed = true; HideMarkers(); Cue("CHECK"); }
        private void End() { active = false; ended = true; current = ""; previewUntil = 0; HideMarkers(); }
        public override void OnReset()
        {
            active = ended = failed = false;
            waters = photons = 0;
            freshWater = shieldDead = false;
            initialLightningKnown = initialLightningMe = secondWaterKnown = secondWaterMe = false;
            enumerationDone = shieldSeen = flareDone = thirdPassShown = pass2Shown = waterCueShown = false;
            current = lastCode = preview = "";
            began = lastStatusRead = previewUntil = 0;
            shieldId = 0;
            minesPlaced = false; enumerationTargets.Clear(); enumerationSide = "";
            effects.Clear(); casts.Clear(); mineSources.Clear();
            HideMarkers();
        }
        public override void OnCombatEnd() => OnReset();
        public override void OnDisable() { OnReset(); voice?.Dispose(); voice = null; }
        public override void OnSettingsDraw()
        {
            ImGui.TextWrapped("Split BJ/CC, Dancer. First two passes with the Cruise Chaser tank. Enumeration starts right with the solo caster and swaps when needed. Without both marks or a unique caster, listen for the group flex call.");
            ImGui.TextWrapped("Water rings use the placed D and B waymarks. Third water tracks its holder when eligible. Cyan is next, green is current. B is a reference for the far-east jump bait.");
            ImGui.Checkbox("Voice (this session)", ref enableVoice);
            ImGui.TextWrapped("Voice: " + voiceStatus);
            if (ImGui.Button("TEST D3 VOICE")) _ = SayAsync("TEST");
            if (!active)
            {
                if (ImGui.Button("Preview D")) { preview = "D"; previewUntil = Now + 5000; }
                if (ImGui.Button("Preview B")) { preview = "B"; previewUntil = Now + 5000; }
            }
        }
    }
}
