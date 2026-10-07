using Dalamud.Bindings.ImGui;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

// Personal assignment: SW for Earthshaker; north/1 to wait and regroup.
// Action IDs and wave order checked against Network_30301_20261006.log.
// VFX path from PunishXIV/Splatoon UCOB Earthshakers.cs.
// This script guides Earthshakers only, not hatches or Meteor Stream.
public sealed class UCOB_Tenstrike_SW_North : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata Metadata => new(1, "Maggie");

    private const uint Tenstrike = 0x26E6;
    private const uint EarthshakerResolve = 0x26D9;
    private const string EarthshakerVfx = "vfx/lockon/eff/m0117_earth_shake_01s.avfx";
    private static readonly Vector3 Southwest = new(-14.14f, 0f, 14.14f);
    private static readonly Vector3 North = new(0f, 0f, -8f);
    private readonly HashSet<uint> firstTargets = new();
    private readonly HashSet<uint> secondTargets = new();
    private long started;
    private long firstMarkerAt;
    private long lastResolveAt;
    private long finishedAt;
    private long previewUntil;
    private int resolvedWaves;
    private bool active;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("Current",
            """
            {"Name":"Tenstrike CURRENT","Enabled":false,"type":0,"radius":1.0,"Filled":true,"fillIntensity":0.3,"color":4278255360,"thicc":8.0,"tether":true,"overlayText":"CURRENT","overlayBGColor":4278190080,"overlayTextColor":4278255360,"overlayFScale":2.0}
            """);
        Controller.RegisterElementFromCode("Next",
            """
            {"Name":"Tenstrike NEXT","Enabled":false,"type":0,"radius":1.3,"Filled":false,"color":4294967040,"thicc":5.0,"tether":false,"overlayText":"NEXT - NORTH","overlayBGColor":4278190080,"overlayTextColor":4294967040,"overlayFScale":1.5}
            """);
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == Tenstrike)
            Begin();
        else if (active && castId == 0x26E7)
            OnReset();
    }

    private void Begin()
    {
        OnReset();
        active = true;
        started = Environment.TickCount64;
    }

    public override void OnVFXSpawn(uint target, string vfxPath)
    {
        if (!active || vfxPath != EarthshakerVfx || resolvedWaves >= 2)
            return;
        var now = Environment.TickCount64;
        if (firstMarkerAt == 0)
            firstMarkerAt = now;
        // Logs show approximately 5.17 seconds between marker sets.
        // Do not classify by hit count: wave-two markers precede wave-one's hit.
        if (now - firstMarkerAt < 2500)
            firstTargets.Add(target);
        else
            secondTargets.Add(target);
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        var action = set.Action?.RowId ?? 0;
        if (action == Tenstrike && !active)
            Begin();
        if (!active || action != EarthshakerResolve || resolvedWaves >= 2)
            return;
        var now = Environment.TickCount64;
        // 26D9 is the once-per-wave control action, not each cone's 26DA hit.
        if (lastResolveAt != 0 && now - lastResolveAt < 1000)
            return;
        lastResolveAt = now;
        resolvedWaves++;
        if (resolvedWaves == 2)
            finishedAt = now;
    }

    public override void OnUpdate()
    {
        Hide();
        var now = Environment.TickCount64;
        var player = BasePlayer;
        if (player == null)
            return;
        if (previewUntil > now)
        {
            ShowCurrent(player.Position, true, "TEST - SAFE NORTH");
            return;
        }
        if (!active)
            return;
        if (player.CurrentHp == 0 || now - started > 45000 ||
            (finishedAt != 0 && now - finishedAt > 6000))
        {
            OnReset();
            return;
        }
        if (firstMarkerAt == 0 || now - firstMarkerAt < 150)
            return;

        var first = firstTargets.Contains(player.EntityId);
        var second = secondTargets.Contains(player.EntityId);
        // Once all four wave-one targets are known, everyone else waits north.
        // An incomplete set must not turn a missed local marker into a safe call.
        var knownSecond = second || (!first && firstTargets.Count == 4);
        if (!first && !knownSecond)
            return;

        bool spread;
        if (resolvedWaves >= 2)
            spread = false;
        else if (first)
            spread = resolvedWaves == 0;
        else
            spread = resolvedWaves == 1 && second;

        // At the first resolve, wait for the actual local second-wave marker
        // if it has not arrived yet; avoid presenting a false safe instruction.
        if (resolvedWaves == 1 && !first && !second)
            return;

        ShowCurrent(spread ? Southwest : North, !spread,
            spread ? "CURRENT - SW" : "CURRENT - SAFE NORTH");
        if (spread && Controller.TryGetElementByName("Next", out var next))
        {
            next.SetRefPosition(North);
            next.SetOffPosition(Vector3.Zero);
            next.Enabled = true;
        }
    }

    private void ShowCurrent(Vector3 position, bool safe, string label)
    {
        if (!Controller.TryGetElementByName("Current", out var element))
            return;
        element.SetRefPosition(position);
        element.SetOffPosition(Vector3.Zero);
        element.radius = safe ? 1.8f : 1.0f;
        element.thicc = safe ? 10f : 8f;
        element.fillIntensity = safe ? 0.45f : 0.3f;
        element.overlayText = label;
        element.Enabled = true;
    }

    private void Hide()
    {
        if (Controller.TryGetElementByName("Current", out var current))
            current.Enabled = false;
        if (Controller.TryGetElementByName("Next", out var next))
            next.Enabled = false;
    }

    public override void OnReset()
    {
        active = false;
        started = firstMarkerAt = lastResolveAt = finishedAt = previewUntil = 0;
        resolvedWaves = 0;
        firstTargets.Clear();
        secondTargets.Clear();
        Hide();
    }

    public override void OnCombatEnd() => OnReset();
    public override void OnDisable() => OnReset();

    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("SW Earthshaker assignment; north/1 waiting and regroup spot. Green = CURRENT. Cyan = NEXT. Follow the green destination. Earthshakers only; hatch and Meteor Stream positions are not shown.");
        ImGui.TextWrapped($"Active: {active}. Marker targets: {firstTargets.Count}/{secondTargets.Count}. Resolved waves: {resolvedWaves}.");
        ImGui.TextWrapped("Log timing and API signatures reviewed. Not compiled or tested in-game.");
        if (!active && ImGui.Button("TEST BIG GREEN MARKER (5 seconds)"))
            previewUntil = Environment.TickCount64 + 5000;
        if (ImGui.Button("HIDE / RESET"))
            OnReset();
    }
}
