// NAUR 1256 intermission only. Requires companion Triggernometry XML.
// Event IDs checked against cactbot's TEA timeline; live validation pending.
using Dalamud.Bindings.ImGui;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.TEA
{
    public sealed class TEA_Limit_Cut_Personal_Voice : SplatoonScript
    {
        public override HashSet<uint> ValidTerritories => new() { 887 };
        public override Metadata Metadata => new(4, "Maggie accessibility");
        private HttpClient? voice;
        private int number, pairs, centers;
        private long numberAt, started, lastDash, lastCenter, until;
        private bool active, finished;
        private string current = "";
        private volatile string status = "Not tested";

        private Element? startMarker;
        private Vector2 openingAxis;
        private long markerUntil;

        public override void OnSetup()
        {
            startMarker = new Element(0)
            {
                Enabled = false,
                radius = 0.9f,
                thicc = 5f,
                color = 0xFF00FF00,
                tether = true
            };
            Controller.RegisterElement("PersonalLimitCutStart", startMarker, true);
        }

        // Opposite endpoints of the same blaster pair produce the same axis.
        private static Vector2 OpeningAxis(Vector3 position)
        {
            var offset = new Vector2(position.X - 100f, position.Z - 100f);
            if (offset.LengthSquared() < 144f || offset.LengthSquared() > 256f)
                return Vector2.Zero;
            if (Math.Abs(offset.Y) < 2f ? offset.X > 0f : offset.Y > 0f)
                offset = -offset;
            return Vector2.Normalize(offset);
        }

        private void MarkOpening(Vector2 axis, long now)
        {
            if (startMarker == null || axis == Vector2.Zero) return;
            openingAxis = axis;
            Vector2 side = number is 1 or 2 or 5 or 6 ? axis : -axis;
            // Trail the cleared blast center, leaving odd players ahead clockwise.
            float angle = (number % 2 == 1 ? -8f : -20f) * MathF.PI / 180f;
            float radius = number <= 4 ? 13.5f : 18f;
            var clockwise = new Vector2(-side.Y, side.X);
            var spot = radius * (MathF.Cos(angle) * side + MathF.Sin(angle) * clockwise);
            startMarker.refX = 100f + spot.X;
            startMarker.refY = 100f + spot.Y;
            startMarker.refZ = 0f;
            startMarker.Enabled = true;
            markerUntil = now + 3500;
        }

        public override void OnEnable()
        {
            OnReset();
            voice = CreateVoiceClient();
        }

        public override void OnVFXSpawn(uint target, string path)
        {
            const string prefix = "vfx/lockon/eff/m0361trg_a";
            if (finished || active || BasePlayer == null || target != BasePlayer.EntityId
                || !path.StartsWith(prefix) || path.Length <= prefix.Length) return;
            if (int.TryParse(path.Substring(prefix.Length, 1), out int n) && n >= 1 && n <= 8)
            {
                number = n;
                numberAt = Environment.TickCount64;
            }
        }

        public override void OnActionEffectEvent(ActionEffectSet set)
        {
            if (finished || BasePlayer == null || BasePlayer.CurrentHp == 0) return;
            long now = Environment.TickCount64;
            // 4830: intermission Hawk Blaster. Wormhole uses different mechanics.
            if (set.Action is { RowId: 0x4830 })
            {
                if (!active)
                {
                    if (number == 0 || now - numberAt > 10000) return;
                    active = true;
                    started = now;
                    MarkOpening(OpeningAxis(set.Position), now);
                    Show("NUMBER_" + number, $"{number}: START {(number <= 4 ? "INNER" : "OUTER")} - {(number % 2 == 1 ? "LEAD" : "FOLLOW")}");
                }
                else if (startMarker?.Enabled == true &&
                    MathF.Abs(Vector2.Dot(openingAxis, OpeningAxis(set.Position))) < 0.95f)
                {
                    startMarker.Enabled = false;
                }
                // Count center explosions, not the two simultaneous outside packets.
                if (Math.Abs(set.Position.X - 100) < 3 && Math.Abs(set.Position.Z - 100) < 3
                    && now - lastCenter > 1000)
                {
                    lastCenter = now;
                    centers++;
                    FinishIfReady();
                }
                return;
            }
            // 4B4F is the intermission dash; 49C3 is Wormhole and is ignored.
            if (!active || set.Action is not { RowId: 0x4B4F } || now - lastDash < 1000) return;
            lastDash = now;
            pairs++;
            int myPair = (number + 1) / 2;
            if (pairs == myPair && !(pairs == 4 && centers >= 2))
                Show("OUTER", "MOVE OUTER - KEEP DODGING CLOCKWISE");
            else if (number >= 5 && pairs == myPair - 2)
                Show(number % 2 == 1 ? "INNER_LEAD" : "INNER_FOLLOW",
                    number % 2 == 1 ? "MOVE INNER - LEAD, FACE CLOCKWISE" : "MOVE INNER - FOLLOW YOUR ODD PARTNER");
            FinishIfReady();
        }

        private void FinishIfReady()
        {
            if (pairs < 4 || centers < 2) return;
            active = false;
            finished = true;
            Show("CENTER", "GO CENTER FOR HEALING");
        }

        private void Show(string code, string text)
        {
            current = text;
            until = Environment.TickCount64 + 4500;
            if (voice is { } client) _ = SayAsync(client, code);
        }

        private static HttpClient CreateVoiceClient() => new(new HttpClientHandler { UseProxy = false })
            { Timeout = TimeSpan.FromSeconds(3) };

        private async Task TestVoiceAsync()
        {
            status = "Sending test...";
            using var client = CreateVoiceClient();
            await SayAsync(client, "TEST");
        }

        private async Task SayAsync(HttpClient client, string code)
        {
            try
            {
                using var body = new StringContent("MAGGIE_TEA_LC|" + code, Encoding.UTF8, "text/plain");
                using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                status = "Sent to Triggernometry";
            }
            catch (Exception) { status = "Check Triggernometry endpoint: localhost:51423"; }
        }

        public override void OnUpdate()
        {
            if (BasePlayer == null || BasePlayer.CurrentHp == 0) { OnReset(); return; }
            if (startMarker?.Enabled == true && Environment.TickCount64 >= markerUntil)
                startMarker.Enabled = false;
            if (active && Environment.TickCount64 - started > 35000)
            {
                active = false;
                finished = true;
                current = "";
            }
            if (active)
                Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), $"YOUR NUMBER: {number}");
            if (current != "" && (active || Environment.TickCount64 < until))
                Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), current);
        }

        public override void OnReset()
        {
            if (startMarker != null) startMarker.Enabled = false;
            number = pairs = centers = 0;
            numberAt = started = lastDash = lastCenter = until = 0;
            active = finished = false;
            current = "";
        }
        public override void OnCombatEnd() => OnReset();
        public override void OnDisable()
        {
            OnReset();
            voice?.Dispose();
            voice = null;
        }
        public override void OnSettingsDraw()
        {
            ImGui.TextWrapped("NAUR 1256 personal voice. First call is at the first Hawk Blaster. Follow the green tether into the first cleared blast on your numbered side, then continue clockwise. Inner means the inner outside lane, NOT center. Lead = odd; follow = even, spaced behind odd.");
            ImGui.TextWrapped("Import Triggernometry_TEA_Limit_Cut_Personal_Voice.xml. Uses your existing localhost:51423 endpoint. The personal green tether marks the opening only. Live validation pending.");
            ImGui.TextWrapped("Voice: " + status);
            if (ImGui.Button("TEST VOICE")) _ = TestVoiceAsync();
        }
    }
}
