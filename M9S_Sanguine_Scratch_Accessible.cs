using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon.Data;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MaggieScripts.Duties.Dawntrail;

public sealed class M9S_Sanguine_Scratch_Accessible : SplatoonScript
{
    private const uint SanguineScratchCastId = 45989;
    private const uint SanguineScratchHitId = 45991;
    private const uint BreakdownDropCastId = 45992;
    private const uint BreakwingBeatCastId = 45994;

    private static readonly Vector3 Center =
        new(100.0f, 0.0f, 100.0f);

    private const float SafeStep = MathF.PI / 8.0f;      // 22.5 degrees
    private const float ProteanStep = MathF.PI / 4.0f;  // 45 degrees

    private bool active;
    private float baseDangerRotation;
    private int hitsResolved;
    private long lastWaveEventAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [1321];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Sanguine_Current",
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
            "Sanguine_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":2.2,
              "Donut":0.35,
              "color":4294967040,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
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

    public override unsafe void OnStartingCast(
        uint sourceId,
        PacketActorCast* packet)
    {
        var action = packet->ActionDescriptor;

        if (action == new ActionDescriptor(
                FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action,
                BreakdownDropCastId) ||
            action == new ActionDescriptor(
                FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action,
                BreakwingBeatCastId))
        {
            OnReset();
            return;
        }

        if (action != new ActionDescriptor(
                FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action,
                SanguineScratchCastId))
            return;

        // Sanguine Scratch begins with eight simultaneous 30-degree
        // proteans spaced 45 degrees apart. The official Splatoon script
        // records each cast rotation, then rotates the entire pattern by
        // 22.5 degrees after each hit. One cast rotation is enough to
        // establish the complete eight-way pattern.
        if (active)
            return;

        active = true;
        hitsResolved = 0;
        lastWaveEventAt = 0;
        baseDangerRotation = packet->Rotation;

        ShowMarkers();
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (!active)
            return;

        var actionId = set.Action?.RowId;

        if (actionId != SanguineScratchCastId &&
            actionId != SanguineScratchHitId)
            return;

        // Each wave produces several action-effect events. Count only one
        // event per wave, matching the throttle used by the official script.
        var now = Environment.TickCount64;

        if (lastWaveEventAt != 0 &&
            now - lastWaveEventAt < 250)
            return;

        lastWaveEventAt = now;
        hitsResolved++;

        if (hitsResolved >= 5)
        {
            OnReset();
            return;
        }

        ShowMarkers();
    }

    public override void OnReset()
    {
        active = false;
        baseDangerRotation = 0.0f;
        hitsResolved = 0;
        lastWaveEventAt = 0;

        HideMarkers();
    }

    private void ShowMarkers()
    {
        var player = Svc.ClientState.LocalPlayer;

        if (player == null)
        {
            HideMarkers();
            return;
        }

        var playerPosition = player.Position;
        var radius = HorizontalDistance(playerPosition, Center);

        // Keep the marker at approximately the player's current distance
        // from center so the script asks for a lateral dodge, not an
        // unnecessary run inward or outward.
        radius = Math.Clamp(radius, 8.0f, 18.0f);

        // Current safe lanes are centered halfway between the current
        // 30-degree danger cones. Every resolved hit rotates the danger
        // pattern by another 22.5 degrees.
        var currentFamily =
            baseDangerRotation +
            (hitsResolved + 1) * SafeStep;

        var nextFamily =
            baseDangerRotation +
            (hitsResolved + 2) * SafeStep;

        var currentPosition =
            NearestLanePosition(
                currentFamily,
                radius,
                playerPosition);

        var nextPosition =
            NearestLanePosition(
                nextFamily,
                radius,
                playerPosition);

        if (Controller.TryGetElementByName(
                "Sanguine_Current",
                out var current))
        {
            current.SetOffPosition(currentPosition);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "Sanguine_Next",
                out var next))
        {
            next.SetOffPosition(nextPosition);
            next.Enabled = true;
        }
    }

    private static Vector3 NearestLanePosition(
        float familyRotation,
        float radius,
        Vector3 playerPosition)
    {
        var best = Center;
        var bestDistance = float.MaxValue;

        for (var i = 0; i < 8; i++)
        {
            var angle =
                familyRotation +
                i * ProteanStep;

            // FFXIV actor rotation 0 points along +Z; +rotation turns toward +X.
            var candidate = new Vector3(
                Center.X + MathF.Sin(angle) * radius,
                playerPosition.Y,
                Center.Z + MathF.Cos(angle) * radius);

            var dx = candidate.X - playerPosition.X;
            var dz = candidate.Z - playerPosition.Z;
            var distance = dx * dx + dz * dz;

            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }

    private static float HorizontalDistance(
        Vector3 a,
        Vector3 b)
    {
        var dx = a.X - b.X;
        var dz = a.Z - b.Z;

        return MathF.Sqrt(dx * dx + dz * dz);
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "Sanguine_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "Sanguine_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}
