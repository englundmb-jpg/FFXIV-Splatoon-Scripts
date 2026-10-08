using Dalamud.Bindings.ImGui;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieScripts.Duties.Stormblood;

// Blackfire only. Timing checked against four Blackfire sequences in the
// October 6 log. Second 26BF: ~20.06-20.18s after Blackfire cast start.
// Tower hits: ~22.32s. No personal Hypernova prediction or tower assignment.
// API: PunishXIV/Splatoon; voice: Triggernometry's local HTTP endpoint.
public sealed class UCOB_Blackfire_Tower_Now : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata Metadata => new(1, "Maggie");
    private const uint Blackfire = 0x26E3, Hypernova = 0x26BF, Tower = 0x26DF;
    private readonly HashSet<uint> stackTargets = new();
    private bool active, go, called, invalid;
    private int novaCount;
    private long started, lastNova, previewUntil;
    private HttpClient? voice;
    private volatile string voiceStatus = "Not tested";
    private bool enableVoice = true;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("Cue",
            """
            {"Name":"Blackfire tower timing","type":1,"Enabled":false,"radius":0.0,"thicc":0.0,"refActorType":1,"overlayText":"","overlayTextColor":4294967040,"overlayBGColor":4278190080,"overlayFScale":3.5,"overlayVOffset":2.0}
            """);
        OnReset();
    }

    public override void OnEnable()
    {
        voice = new HttpClient(new HttpClientHandler { UseProxy = false })
        {
            Timeout = TimeSpan.FromMilliseconds(700)
        };
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == Blackfire)
        {
            OnReset();
            active = true;
            started = Environment.TickCount64;
        }
        else if (active && castId is 0x26E2 or 0x26E4 or 0x26E5 or 0x26E6 or 0x26E7)
            OnReset();
    }

    public override void OnActorControl(uint sourceId, uint command,
        uint p1, uint p2, uint p3, uint p4, uint p5, uint p6,
        uint p7, uint p8, ulong targetId, byte replaying)
    {
        // TargetIcon 34, Megaflare stack marker 0x27.
        if (active && command == 34 && p1 == 0x27)
            stackTargets.Add(sourceId);
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (!active) return;
        var action = set.Action?.RowId ?? 0;
        if (action == Tower)
        {
            OnReset();
            return;
        }
        if (action != Hypernova || invalid || novaCount >= 2) return;
        var now = Environment.TickCount64;
        if (lastNova != 0 && now - lastNova < 500) return;
        var elapsed = now - started;
        // Fail closed if installed mid-mechanic or an event was missed.
        // These broad guard windows validate order; they never start the cue.
        if ((novaCount == 0 && (elapsed < 17500 || elapsed > 19500)) ||
            (novaCount == 1 && (elapsed < 19500 || elapsed > 21000 ||
                               now - lastNova < 1000 || now - lastNova > 2200)))
        {
            invalid = true;
            Hide();
            return;
        }
        lastNova = now;
        novaCount++;
        if (novaCount == 2 && IsTowerPlayer())
        {
            go = true;
            Show("TOWER NOW", true);
            if (!called)
            {
                called = true;
                if (enableVoice) _ = SayAsync();
            }
        }
    }

    private bool IsTowerPlayer()
    {
        var player = BasePlayer;
        return player != null && player.CurrentHp > 0 &&
            stackTargets.Count == 4 && !stackTargets.Contains(player.EntityId);
    }

    public override void OnUpdate()
    {
        Hide();
        var now = Environment.TickCount64;
        if (previewUntil > now)
        {
            Show("TEST - TOWER NOW", true);
            return;
        }
        if (!active) return;
        if (BasePlayer == null || BasePlayer.CurrentHp == 0 || now - started > 22500)
        {
            OnReset();
            return;
        }
        if (invalid || !IsTowerPlayer()) return;
        // A late marker packet must not cause a delayed voice instruction.
        if (go)
            Show("TOWER NOW", true);
        else if (novaCount < 2)
            Show("WAIT - BAIT PUDDLES", false);
    }

    private void Show(string text, bool ready)
    {
        if (!Controller.TryGetElementByName("Cue", out var cue)) return;
        cue.overlayText = text;
        cue.overlayTextColor = ready ? 0xFF00FF00u : 0xFFFFFF00u;
        cue.Enabled = true;
        Controller.DisplayAttentionWindowLine(
            ready ? new Vector4(0, 1, 0, 1) : new Vector4(0, 1, 1, 1), text);
    }

    private void Hide()
    {
        if (Controller.TryGetElementByName("Cue", out var cue))
            cue.Enabled = false;
    }

    private async Task SayAsync()
    {
        var client = voice;
        if (client == null) return;
        try
        {
            using var body = new StringContent("MAGGIE_UCOB_BLACKFIRE|TOWER_NOW",
                Encoding.UTF8, "text/plain");
            using var response = await client.PostAsync("http://localhost:51423/", body)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            voiceStatus = "Sent to Triggernometry";
        }
        catch (Exception)
        {
            voiceStatus = "No voice connection: check Triggernometry endpoint";
        }
    }

    public override void OnReset()
    {
        active = go = called = invalid = false;
        novaCount = 0;
        started = lastNova = previewUntil = 0;
        stackTargets.Clear();
        Hide();
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
        ImGui.TextWrapped("Blackfire tower timing only. Cyan WAIT, then green TOWER NOW on the second Hypernova event. No cue for Megaflare stack players. This does not choose your tower or check whether its floor is clear.");
        ImGui.TextWrapped("Voice needs the companion Triggernometry XML and its local HTTP endpoint enabled at http://localhost:51423/.");
        ImGui.Checkbox("Voice cue (this session)", ref enableVoice);
        ImGui.TextWrapped("Voice: " + voiceStatus);
        ImGui.TextWrapped($"Active: {active}; stack markers: {stackTargets.Count}/4; Hypernovas: {novaCount}; incomplete timing: {invalid}");
        ImGui.TextWrapped("Log and API checked. Not compiled or tested inside FFXIV.");
        if (!active && ImGui.Button("TEST GREEN TEXT + VOICE"))
        {
            previewUntil = Environment.TickCount64 + 4000;
            if (enableVoice) _ = SayAsync();
        }
        if (ImGui.Button("HIDE / RESET")) OnReset();
    }
}
