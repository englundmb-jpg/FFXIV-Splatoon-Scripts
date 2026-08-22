using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Dawntrail;

public sealed class M9S_Sanguine_Scratch_Accessible : SplatoonScript
{
    private const uint SanguineScratchCastId = 45989;
    private const uint BreakdownDropCastId = 45992;
    private const uint BreakwingBeatCastId = 45994;

    private static readonly Vector3 Center =
        new(100.0f, 0.0f, 100.0f);

    private const float SafeStep = MathF.PI / 8.0f;     // 22.5 degrees
    private const float ProteanStep = MathF.PI / 4.0f; // 45 degrees

    private bool active;
    private float baseDangerRotation;
    private long startedAt;
    private int stage;

    public override HashSet<uint>? ValidTerritories { get; } =
        [1321];

    public override Metadata? Metadata =>
        new(2, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Sanguine_Current",
            """{"Name":"CURRENT","Enabled":false,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"tether":true}"""
        );

        Controller.RegisterElementFromCode(
            "Sanguine_Next",
            """{"Name":"NEXT","Enabled":false,"radius":2.2,"Donut":0.35,"color":4294967040,"thicc":8.0,"tether":true}"""
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == BreakdownDropCastId ||
            castId == BreakwingBeatCastId)
        {
            OnReset();
            return;
        }

        if (castId != SanguineScratchCastId || active)
            return;

        var caster = Svc.Objects
            .FirstOrDefault(x => x.EntityId == source);

        if (caster == null)
            return;

        active = true;
        startedAt = Environment.TickCount64;
        stage = 0;
        baseDangerRotation = caster.Rotation;

        ShowMarkers(stage);
    }

    public override void OnUpdate()
    {
        if (!active)
            return;

        var elapsed = Environment.TickCount64 - startedAt;

        // Conservative timing progression for the five rotating scratches.
        // First live test is only to verify the script loads and CURRENT
        // appears in the correct safe lane. Timing can then be tuned exactly.
        var newStage = stage;

        if (elapsed >= 1800) newStage = 1;
        if (elapsed >= 3600) newStage = 2;
        if (elapsed >= 5400) newStage = 3;
        if (elapsed >= 7200) newStage = 4;

        if (newStage != stage)
        {
            stage = newStage;
            ShowMarkers(stage);
        }

        if (elapsed >= 9000)
            OnReset();
    }

    public override void OnReset()
    {
        active = false;
        baseDangerRotation = 0.0f;
        startedAt = 0;
        stage = 0;

        DisableMarkers();
    }

    private void ShowMarkers(int currentStage)
    {
        var player = Svc.ClientState.LocalPlayer;

        if (player == null)
        {
            DisableMarkers();
            return;
        }

        var playerPosition = player.Position;
        var radius = Math.Clamp(
            HorizontalDistance(playerPosition, Center),
            8.0f,
            18.0f
        );

        var currentFamily =
            baseDangerRotation +
            (currentStage + 1) * SafeStep;

        var nextFamily =
            baseDangerRotation +
            (currentStage + 2) * SafeStep;

        var currentPosition =
            NearestLanePosition(
                currentFamily,
                radius,
                playerPosition
            );

        var nextPosition =
            NearestLanePosition(
                nextFamily,
                radius,
                playerPosition
            );

        if (Controller.TryGetElementByName(
                "Sanguine_Current",
                out var current))
        {
            current.SetRefPosition(currentPosition);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "Sanguine_Next",
                out var next))
        {
            next.SetRefPosition(nextPosition);
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

            var candidate = new Vector3(
                Center.X + MathF.Sin(angle) * radius,
                playerPosition.Y,
                Center.Z + MathF.Cos(angle) * radius
            );

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

    private void DisableMarkers()
    {
        if (Controller.TryGetElementByName(
                "Sanguine_Current",
                out var current))
            current.Enabled = false;

        if (Controller.TryGetElementByName(
                "Sanguine_Next",
                out var next))
            next.Enabled = false;
    }
}
