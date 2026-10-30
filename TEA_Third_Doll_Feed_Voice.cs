// Third individual doll feed AFTER its second Exhaust; below 24% HP required.
// Feed event IDs: awgil/ffxiv_bossmod TEAEnums.cs. Live validation pending.
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
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
    public sealed class TEA_Third_Doll_Feed_Voice : SplatoonScript
    {
        public override HashSet<uint> ValidTerritories => new() { 887 };
        public override Metadata Metadata => new(2, "Maggie accessibility");
        private HttpClient? voice;
        private volatile string status = "Not tested";
        private readonly HashSet<uint> fed = new();
        private readonly Dictionary<uint, (int Count, long Last)> exhausts = new();
        private bool secondAnnounced;
        private uint myDoll;
        private bool called, stopped;
        private string current = "";
        private long until;
        private static bool IsDoll(IBattleNpc b) => b.DataId == 0x2C4A;
        private IBattleNpc? Doll(uint id) => Svc.Objects.OfType<IBattleNpc>()
            .FirstOrDefault(x => x.EntityId == id && IsDoll(x));

        public override void OnEnable()
        {
            OnReset();
            voice = new HttpClient(new HttpClientHandler { UseProxy = false })
                { Timeout = TimeSpan.FromMilliseconds(700) };
        }

        public override void OnTetherCreate(uint source, uint target, uint data2, uint data3, uint data5)
        {
            if (stopped || BasePlayer == null) return;
            uint candidate = target == BasePlayer.EntityId ? source
                : source == BasePlayer.EntityId ? target : 0;
            if (candidate != 0 && Doll(candidate) != null)
            {
                if (myDoll != 0 && myDoll != candidate) { stopped = true; current = "DOLL CHANGED - CHECK MANUALLY"; return; }
                myDoll = candidate;
            }
        }

        public override void OnActionEffectEvent(ActionEffectSet set)
        {
            if (stopped) return;
            if (set.Action is { RowId: 18465 })
            {
                stopped = true;
                current = "";
                return;
            }
            if (set.Action is { RowId: 18462 } && set.Source is IBattleNpc caster && IsDoll(caster))
            {
                long now = Environment.TickCount64;
                if (!exhausts.TryGetValue(caster.EntityId, out var previous))
                    exhausts[caster.EntityId] = (1, now);
                else if (now - previous.Last >= 1000)
                    exhausts[caster.EntityId] = (previous.Count + 1, now);
                return;
            }
            if (set.Action is not { RowId: 18464 } || set.Source is not IBattleNpc source || !IsDoll(source)) return;
            // Unique source IDs prevent one raidwide packet per target being counted as several feeds.
            fed.Add(source.EntityId);
            if (source.EntityId == myDoll) { stopped = true; current = ""; }
        }

        public override void OnUpdate()
        {
            if (BasePlayer == null || BasePlayer.CurrentHp == 0) { OnReset(); return; }
            var doll = myDoll == 0 ? null : Doll(myDoll);
            if (!stopped && doll != null && (doll.IsDead || doll.CurrentHp == 0))
            {
                stopped = true;
                current = "";
            }
            bool secondDone = myDoll != 0 && exhausts.TryGetValue(myDoll, out var casts) && casts.Count >= 2;
            bool ready = !stopped && !called && secondDone && fed.Count == 2 && doll != null
                && doll.IsTargetable && doll.MaxHp > 0 && doll.CurrentHp > 0
                && (double)doll.CurrentHp / doll.MaxHp < 0.24;
            if (ready)
            {
                called = true;
                // Combine simultaneous cues so TTS does not interrupt Second Exhaust.
                Show(secondAnnounced ? "FEED" : "SECOND_FEED", "SECOND EXHAUST DONE - THIRD FEED - FEED YOUR DOLL");
                secondAnnounced = true;
            }
            else if (!stopped && !secondAnnounced && secondDone && fed.Count <= 2)
            {
                secondAnnounced = true;
                Show("SECOND", "SECOND EXHAUST DONE - WAIT FOR FEED CALL");
            }
            // Do not issue a late third-feed instruction after someone else has taken that slot.
            if (!stopped && fed.Count > 2) { stopped = true; current = ""; }
            if (current != "" && Environment.TickCount64 < until)
                Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), current);
        }

        private void Show(string code, string text)
        {
            current = text;
            until = Environment.TickCount64 + 5000;
            _ = SayAsync(code);
        }
        private async Task SayAsync(string code)
        {
            var client = voice;
            if (client == null) return;
            try
            {
                using var body = new StringContent("MAGGIE_TEA_DOLL|" + code, Encoding.UTF8, "text/plain");
                using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                status = "Sent to Triggernometry";
            }
            catch (Exception) { status = "Check Triggernometry endpoint: localhost:51423"; }
        }


        public override void OnReset()
        {
            fed.Clear();
            exhausts.Clear();
            secondAnnounced = false;
            myDoll = 0;
            called = stopped = false;
            current = "";
            until = 0;
        }
        public override void OnCombatStart() => OnReset();
        public override void OnCombatEnd() => OnReset();
        public override void OnDisable()
        {
            OnReset();
            voice?.Dispose();
            voice = null;
        }
        public override void OnSettingsDraw()
        {
            ImGui.TextWrapped("VERSION 2: Calls your doll's second Exhaust. Feed call requires ALL THREE: second Exhaust resolved, two other dolls fed, and your doll below 24% HP. Does not check healer readiness or your route. Enable before dolls tether and first Exhaust.");
            ImGui.TextWrapped("Import Triggernometry_TEA_Third_Doll_Feed_Voice.xml. Uses localhost:51423. No targeting or movement automation. Live validation pending.");
            ImGui.TextWrapped($"Tracked doll: {myDoll:X}; confirmed feeds: {fed.Count}; stopped: {stopped}. Voice: {status}");
            if (ImGui.Button("TEST VOICE")) Show("TEST", "DOLL VOICE TEST");
        }
    }
}
