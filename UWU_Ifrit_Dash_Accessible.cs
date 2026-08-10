using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UWU_Ifrit_Dash_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        [Raids.the_Weapons_Refrain_Ultimate];

    public override Metadata? Metadata =>
        new(1, "Maggie Ifrit Dash accessibility build");

    private sealed class NailInfo
    {
        public uint EntityId;
        public Vector3 Position;
        public bool Dead;
    }

    private readonly List<NailInfo> nails = new();
    private readonly List<Vector3> nailDeathOrder = new();

    private bool nailsCaptured;
    private bool nailOrderComplete;
    private int dashCount;

    private static readonly Vector3 Center =
        new(100.0f, 0.0f, 100.0f);

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "IfritDash_Current",
            "{\"Name\":\"CURRENT\",\"Enabled\":false,\"radius\":2.5,\"Donut\":0.35,\"color\":4278255360,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true}"
        );

        Controller.RegisterElementFromCode(
            "IfritDash_Next",
            "{\"Name\":\"NEXT\",\"Enabled\":false,\"radius\":2.2,\"Donut\":0.35,\"color\":4294967040,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true}"
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        if (!nailsCaptured)
        {
            TryCaptureNails();
            return;
        }

        if (!nailOrderComplete)
            TrackNailDeaths();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        // 2B5F = Crimson Cyclone.
        if (castId != 0x2B5F)
            return;

        if (!nailOrderComplete)
            return;

        var ifrit = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x => x.EntityId == source);

        if (ifrit == null)
            return;

        var nailIndex = FindClosestNail(ifrit.Position);

        if (nailIndex < 0)
            return;

        // Only accept the Crimson Cyclones in the actual
        // nail-death order from this pull.
        if (nailIndex != dashCount)
            return;

        var start = nailDeathOrder[dashCount];
        var end = OppositePoint(start);

        ShowRoute(start, end);

        dashCount++;

        if (dashCount >= 4)
            Controller.ScheduleReset(5000);
    }

    public override void OnReset()
    {
        nails.Clear();
        nailDeathOrder.Clear();

        nailsCaptured = false;
        nailOrderComplete = false;
        dashCount = 0;

        DisableMarkers();
    }

    private void TryCaptureNails()
    {
        var found = Svc.Objects
            .OfType<IBattleNpc>()
            .Where(x =>
                x.Name.TextValue.Equals(
                    "Infernal Nail",
                    StringComparison.OrdinalIgnoreCase))
            .Where(x => x.CurrentHp > 0)
            .ToList();

        if (found.Count != 4)
            return;

        nails.Clear();

        foreach (var nail in found)
        {
            nails.Add(new NailInfo
            {
                EntityId = nail.EntityId,
                Position = nail.Position,
                Dead = false
            });
        }

        nailsCaptured = true;
    }

    private void TrackNailDeaths()
    {
        foreach (var nail in nails)
        {
            if (nail.Dead)
                continue;

            var actor = Svc.Objects
                .OfType<IBattleNpc>()
                .FirstOrDefault(x => x.EntityId == nail.EntityId);

            if (actor != null)
            {
                nail.Position = actor.Position;

                if (actor.CurrentHp > 0)
                    continue;
            }

            nail.Dead = true;
            nailDeathOrder.Add(nail.Position);
        }

        if (nailDeathOrder.Count == 4)
        {
            nailOrderComplete = true;
            dashCount = 0;
        }
    }

    private int FindClosestNail(Vector3 position)
    {
        var closest = -1;
        var closestDistance = float.MaxValue;

        for (var i = 0; i < nailDeathOrder.Count; i++)
        {
            var distance = Distance(position, nailDeathOrder[i]);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = i;
            }
        }

        // Don't accept an unrelated Ifrit object.
        if (closestDistance > 8.0f)
            return -1;

        return closest;
    }

    private static Vector3 OppositePoint(Vector3 start)
    {
        return new Vector3(
            Center.X * 2.0f - start.X,
            start.Y,
            Center.Z * 2.0f - start.Z
        );
    }

    private static float Distance(Vector3 a, Vector3 b)
    {
        var x = a.X - b.X;
        var z = a.Z - b.Z;

        return MathF.Sqrt(x * x + z * z);
    }

    private void ShowRoute(Vector3 currentPosition, Vector3 nextPosition)
    {
        if (!Controller.TryGetElementByName("IfritDash_Current", out var current) ||
            !Controller.TryGetElementByName("IfritDash_Next", out var next))
            return;

        current.SetRefPosition(currentPosition);
        next.SetRefPosition(nextPosition);

        current.Enabled = true;
        next.Enabled = true;
    }

    private void DisableMarkers()
    {
        if (Controller.TryGetElementByName("IfritDash_Current", out var current))
            current.Enabled = false;

        if (Controller.TryGetElementByName("IfritDash_Next", out var next))
            next.Enabled = false;
    }
}
