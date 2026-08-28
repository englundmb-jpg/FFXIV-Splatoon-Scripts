using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

// Minimal D3 positional guide for the Tuufless Elemental UCoB strategy.
// One phase-triggered marker is shown at a time.  This intentionally replaces
// the older bundle whose separate scripts all activated from boss presence.
public sealed class UCOB_Elemental_D3_Clean : SplatoonScript
{
    private const uint Quickmarch = 0x26E2;
    private const uint Blackfire = 0x26E3;
    private const uint Fellruin = 0x26E4;
    private const uint Heavensfall = 0x26E5;
    private const uint Tenstrike = 0x26E6;
    private const uint GrandOctet = 0x26E7;

    private static readonly Vector3 Center = new(0.0f, 0.0f, 0.0f);
    private static readonly Vector3 SouthNeurolink = new(0.0f, 0.0f, 8.75f);

    private uint phase;
    private long phaseStarted;
    private bool markerShown;

    public override HashSet<uint>? ValidTerritories { get; } = [733];

    public override Metadata? Metadata =>
        new(2, "Maggie — Tuufless Elemental D3");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "D3_Destination",
            """
            {
              "Name":"D3 DESTINATION",
              "Enabled":false,
              "radius":2.5,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"D3",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId < Quickmarch || castId > GrandOctet)
            return;

        phase = castId;
        phaseStarted = Environment.TickCount64;
        markerShown = false;
        HideMarker();
    }

    public override void OnUpdate()
    {
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (phase != 0)
                OnReset();
            return;
        }

        if (phase == 0)
            return;

        var elapsed = Environment.TickCount64 - phaseStarted;

        switch (phase)
        {
            case Quickmarch:
                // Elemental: orient north toward the trio; D3/D4 spread opposite.
                ShowOppositeTrio("D3 — OPPOSITE TRIO", 14.5f);
                if (elapsed > 18000)
                    HideMarker();
                break;

            case Blackfire:
                // Elemental D3 shares the H1(stack) tower.  The exact tower is
                // player-relative, so this clean build marks only the safe regroup.
                ShowMarker(Center, "D3 — CENTER REGROUP");
                if (elapsed > 10500)
                    HideMarker();
                break;

            case Fellruin:
                // Elemental spread: D3/D4 are at the bottom, opposite Nael.
                ShowOppositeNael("D3 — OPPOSITE NAEL", 10.5f);
                if (elapsed > 19000)
                    HideMarker();
                break;

            case Heavensfall:
                // Elemental: both ranged are opposite Nael.
                ShowOppositeNael("D3 — OPPOSITE NAEL", 15.5f);
                if (elapsed > 19000)
                    HideMarker();
                break;

            case Tenstrike:
                // Tuufless Elemental uses the SOUTH 3-waymark as the safe Neurolink.
                ShowMarker(SouthNeurolink, "D3 — SOUTH SAFE");
                if (elapsed > 19000)
                    HideMarker();
                break;

            case GrandOctet:
                // Octet is marker/order dependent.  Do not display a guessed spot.
                HideMarker();
                break;
        }

        if (elapsed > 60000)
            OnReset();
    }

    public override void OnReset()
    {
        phase = 0;
        phaseStarted = 0;
        markerShown = false;
        HideMarker();
    }

    private void ShowOppositeNael(string label, float distance)
    {
        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x => x.Name.TextValue.Contains(
                "Nael", StringComparison.OrdinalIgnoreCase));

        if (nael == null)
            return;

        ShowMarker(Opposite(nael.Position, distance), label);
    }

    private void ShowOppositeTrio(string label, float distance)
    {
        var bosses = Svc.Objects
            .OfType<IBattleNpc>()
            .Where(x =>
                x.Name.TextValue.Contains("Twintania", StringComparison.OrdinalIgnoreCase) ||
                x.Name.TextValue.Contains("Nael", StringComparison.OrdinalIgnoreCase) ||
                x.Name.TextValue.Contains("Bahamut", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        if (bosses.Length == 0)
            return;

        var average = new Vector3(
            bosses.Average(x => x.Position.X),
            0.0f,
            bosses.Average(x => x.Position.Z));

        ShowMarker(Opposite(average, distance), label);
    }

    private static Vector3 Opposite(Vector3 source, float distance)
    {
        var direction = new Vector2(source.X - Center.X, source.Z - Center.Z);
        if (direction.LengthSquared() < 0.01f)
            return Center;

        direction = Vector2.Normalize(direction);
        return new Vector3(
            Center.X - direction.X * distance,
            0.0f,
            Center.Z - direction.Y * distance);
    }

    private void ShowMarker(Vector3 position, string label)
    {
        if (!Controller.TryGetElementByName("D3_Destination", out var marker))
            return;

        marker.SetOffPosition(position);
        marker.overlayText = label;
        marker.Enabled = true;
        markerShown = true;
    }

    private void HideMarker()
    {
        if (Controller.TryGetElementByName("D3_Destination", out var marker))
            marker.Enabled = false;

        markerShown = false;
    }
}
