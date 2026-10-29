// NAUR 1256 intermission only. Requires companion Triggernometry XML.
// Event IDs checked against cactbot's TEA timeline; live validation pending.
using Dalamud.Bindings.ImGui;
using ECommons.Hooks.ActionEffectTypes;
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
        public override Metadata Metadata => new(1, "Maggie accessibility");
        private HttpClient? voice;
        private int number, pairs, centers;
        private long numberAt, started, lastDash, lastCenter, until;
        private bool active, finished;
        private string current = "";
        private volatile string status = "Not tested";

        public override void OnEnable()
        {
            OnReset();
            voice = new HttpClient(new HttpClientHandler { UseProxy = false })
                { Timeout = TimeSpan.FromMilliseconds(700) };
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
                    Show("NUMBER_" + number, $"{number}: START {(number <= 4 ? "INNER" : "OUTER")} - {(number % 2 == 1 ? "LEAD" : "FOLLOW")}");
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
            _ = SayAsync(code);
        }

        private async Task SayAsync(string code)
        {
            var client = voice;
            if (client == null) return;
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
            if (active && Environment.TickCount64 - started > 35000)
            {
                active = false;
                finished = true;
                current = "";
            }
            if (current != "" && Environment.TickCount64 < until)
                Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), current);
        }

        public override void OnReset()
        {
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
            ImGui.TextWrapped("NAUR 1256 personal voice. First call is at the first Hawk Blaster. Choose your group's starting side and follow cleared explosions clockwise. Inner means the inner outside lane, NOT center. Lead = odd; follow = even, spaced behind odd.");
            ImGui.TextWrapped("Import Triggernometry_TEA_Limit_Cut_Personal_Voice.xml. Uses your existing localhost:51423 endpoint. Keep your 1256 ground markers enabled. Live validation pending.");
            ImGui.TextWrapped("Voice: " + status);
            if (ImGui.Button("TEST VOICE")) Show("TEST", "LIMIT CUT VOICE TEST");
        }
    }
}
