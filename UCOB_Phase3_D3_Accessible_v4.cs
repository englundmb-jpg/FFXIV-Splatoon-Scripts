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
public sealed class UCOB_Phase3_D3_Accessible_v4 : SplatoonScript
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
    private bool octetCalculated;

    public override HashSet<uint>? ValidTerritories { get; } = [733];

    public override Metadata? Metadata =>
        new(4, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "CURRENT",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":2.5,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "NEXT",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":2.2,
              "Donut":0.35,
              "color":4294902015,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":false,
              "LegacyFill":true,
              "overlayText":"NEXT",
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
        octetCalculated = false;
        HideMarkers();
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
                ShowOppositeTrio("CURRENT — D3 BOTTOM", 14.5f);
                if (elapsed > 18000)
                    HideMarkers();
                break;

            case Blackfire:
                // Elemental D3 shares the H1(stack) tower.  The exact tower is
                // player-relative, so this clean build marks only the safe regroup.
                ShowMarker(Center, "CURRENT — CENTER / 4");
                if (elapsed > 10500)
                    HideMarkers();
                break;

            case Fellruin:
                // Elemental spread: D3/D4 are at the bottom, opposite Nael.
                ShowOppositeNael("CURRENT — D3 BOTTOM", 10.5f);
                if (elapsed > 19000)
                    HideMarkers();
                break;

            case Heavensfall:
                // Elemental: both ranged are opposite Nael.
                ShowOppositeNael("CURRENT — OPPOSITE NAEL", 15.5f);
                if (elapsed > 19000)
                    HideMarkers();
                break;

            case Tenstrike:
                // Tuufless Elemental uses the SOUTH 3-waymark as the safe Neurolink.
                ShowMarker(SouthNeurolink, "CURRENT — SOUTH 3 SAFE");
                if (elapsed > 19000)
                    HideMarkers();
                break;

            case GrandOctet:
                // Cactbot-confirmed rule: start opposite Bahamut.  Rotate CCW
                // when Bahamut is cardinal and CW when it is intercardinal.
                // If Nael occupies that start sector, shift one sector in the
                // rotation direction.  Boss positions settle about 4.8s in.
                if (elapsed >= 4800 && !octetCalculated)
                    CalculateGrandOctet();
                if (elapsed > 20000)
                    HideMarkers();
                break;
        }

        if (elapsed > 60000)
            OnReset();
    }

    public override void OnReset()
    {
        phase = 0;
        phaseStarted = 0;
        octetCalculated = false;
        HideMarkers();
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
        if (!Controller.TryGetElementByName("CURRENT", out var marker))
            return;

        marker.SetOffPosition(position);
        marker.overlayText = label;
        marker.Enabled = true;
    }

    private void CalculateGrandOctet()
    {
        var bosses = Svc.Objects.OfType<IBattleNpc>().ToArray();
        var bahamut = bosses.FirstOrDefault(x => x.Name.TextValue.Contains(
            "Bahamut", StringComparison.OrdinalIgnoreCase));
        var nael = bosses.FirstOrDefault(x => x.Name.TextValue.Contains(
            "Nael", StringComparison.OrdinalIgnoreCase));
        if (bahamut == null || nael == null)
            return;

        var bahamutSector = Sector(bahamut.Position);
        var naelSector = Sector(nael.Position);
        var cardinal = (bahamutSector & 1) == 0;
        var rotationStep = cardinal ? -1 : 1; // screen/world sectors: -1 CCW, +1 CW
        var startSector = (bahamutSector + 4) & 7;
        if (startSector == naelSector)
            startSector = (startSector + rotationStep + 8) & 7;

        var nextSector = (startSector + rotationStep + 8) & 7;
        ShowMarker(PointAtSector(startSector, 17.5f),
            cardinal ? "CURRENT — START / ROTATE CCW" : "CURRENT — START / ROTATE CW");

        if (Controller.TryGetElementByName("NEXT", out var next))
        {
            next.SetOffPosition(PointAtSector(nextSector, 17.5f));
            next.overlayText = "NEXT";
            next.Enabled = true;
        }

        octetCalculated = true;
    }

    private static int Sector(Vector3 position)
    {
        // 0=N, 1=NE, 2=E ... 7=NW.
        var radians = Math.Atan2(position.X, -position.Z);
        return ((int)Math.Round(radians / (Math.PI / 4.0)) + 8) & 7;
    }

    private static Vector3 PointAtSector(int sector, float radius)
    {
        var radians = sector * Math.PI / 4.0;
        return new Vector3(
            (float)Math.Sin(radians) * radius,
            0.0f,
            (float)-Math.Cos(radians) * radius);
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName("CURRENT", out var current))
            current.Enabled = false;
        if (Controller.TryGetElementByName("NEXT", out var next))
            next.Enabled = false;

    }
}
