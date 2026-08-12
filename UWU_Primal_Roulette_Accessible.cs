using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Splatoon.Data;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UWU_Primal_Roulette_Accessible : SplatoonScript
{
    private static readonly Vector3 Center =
        new(100.0f, 0.0f, 100.0f);

    // NAUR current UWU waymarks.
    private static readonly Vector3 A =
        new(100.0f, 0.0f, 93.3f);

    private static readonly Vector3 Two =
        new(100.0f, 0.0f, 81.0f);

    private static readonly Vector3 Three =
        new(100.0f, 0.0f, 100.0f);

    private static readonly Vector3 Four =
        new(87.0f, 0.0f, 87.0f);

    private enum Primal
    {
        None,
        Garuda,
        Ifrit,
        Titan
    }

    private Primal currentPrimal = Primal.None;

    private uint lastPrimalSource;
    private long lastPrimalSeenAt;

    private int titanWeightCount;

    // Before Ultima:
    // Garuda OFF
    // Ifrit ON
    // Titan OFF
    //
    // After targetable Ultima has appeared:
    // Garuda / Ifrit / Titan are all enabled for Primal Roulette.
    private bool sawTargetableUltima;

    public override HashSet<uint>? ValidTerritories { get; } =
        [777];

    public override Metadata? Metadata =>
        new(2, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Roulette_Current",
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
            "Roulette_Next",
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

    public override void OnUpdate()
    {
        // Once the real targetable Ultima Weapon has appeared,
        // enable all three primals for the later Primal Roulette.
        var ultima = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Ultima",
                    StringComparison.OrdinalIgnoreCase) &&
                x.IsTargetable);

        if (ultima != null)
            sawTargetableUltima = true;

        // Read the actual live primal actor rather than predict the order.
        var primal = FindLiveRoulettePrimal();

        if (primal == null)
            return;

        var detected = GetPrimal(primal);

        if (detected == Primal.None)
            return;

        // BEFORE ULTIMA:
        // keep Ifrit markers only.
        // Suppress original Garuda and Titan markers.
        if (!sawTargetableUltima &&
            detected != Primal.Ifrit)
        {
            HideMarkers();
            return;
        }

        // Only react when a new primal actor becomes the active one.
        if (primal.EntityId == lastPrimalSource &&
            detected == currentPrimal)
            return;

        lastPrimalSource = primal.EntityId;
        lastPrimalSeenAt = Environment.TickCount64;

        SetPrimal(detected);
    }

    public override unsafe void OnStartingCast(
        uint sourceId,
        PacketActorCast* packet)
    {
        var source = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x => x.EntityId == sourceId);

        if (source == null)
            return;

        var primal = GetPrimal(source);

        if (primal == Primal.None)
            return;

        // Same early-phase filter for cast detection.
        // Before Ultima, only Ifrit is allowed.
        if (!sawTargetableUltima &&
            primal != Primal.Ifrit)
            return;

        // If actor detection did not catch the new primal first,
        // the real cast event will.
        if (primal != currentPrimal)
        {
            lastPrimalSource = sourceId;
            SetPrimal(primal);
        }

        // Titan's Roulette mechanic is three Weight of the Land sets.
        if (currentPrimal == Primal.Titan &&
            packet->ActionDescriptor ==
            new ActionDescriptor(
                FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action,
                0x2B65))
        {
            titanWeightCount++;

            DrawTitanStep(titanWeightCount);
        }
    }

    public override void OnReset()
    {
        currentPrimal = Primal.None;
        lastPrimalSource = 0;
        lastPrimalSeenAt = 0;

        titanWeightCount = 0;
        sawTargetableUltima = false;

        HideMarkers();
    }

    private IBattleNpc? FindLiveRoulettePrimal()
    {
        var primals = Svc.Objects
            .OfType<IBattleNpc>()
            .Where(x => x.IsTargetable == false)
            .Where(x =>
                x.Name.TextValue.Equals(
                    "Garuda",
                    StringComparison.OrdinalIgnoreCase) ||
                x.Name.TextValue.Equals(
                    "Ifrit",
                    StringComparison.OrdinalIgnoreCase) ||
                x.Name.TextValue.Equals(
                    "Titan",
                    StringComparison.OrdinalIgnoreCase))
            .Where(x =>
                HorizontalDistance(x.Position, Center) < 25.0f)
            .ToList();

        if (primals.Count != 1)
            return null;

        return primals[0];
    }

    private static Primal GetPrimal(IBattleNpc actor)
    {
        if (actor.Name.TextValue.Equals(
                "Garuda",
                StringComparison.OrdinalIgnoreCase))
            return Primal.Garuda;

        if (actor.Name.TextValue.Equals(
                "Ifrit",
                StringComparison.OrdinalIgnoreCase))
            return Primal.Ifrit;

        if (actor.Name.TextValue.Equals(
                "Titan",
                StringComparison.OrdinalIgnoreCase))
            return Primal.Titan;

        return Primal.None;
    }

    private void SetPrimal(Primal primal)
    {
        currentPrimal = primal;
        titanWeightCount = 0;

        switch (primal)
        {
            case Primal.Garuda:
                DrawGaruda();
                break;

            case Primal.Ifrit:
                DrawIfrit();
                break;

            case Primal.Titan:
                DrawTitanStep(0);
                break;
        }
    }

    private void DrawGaruda()
    {
        ShowRoute(
            A,
            Three,
            "GARUDA"
        );
    }

    private void DrawIfrit()
    {
        ShowRoute(
            Two,
            Three,
            "IFRIT"
        );
    }

    private void DrawTitanStep(int step)
    {
        if (step <= 0)
        {
            ShowRoute(
                Two,
                A,
                "TITAN"
            );

            return;
        }

        if (step == 1)
        {
            ShowRoute(
                A,
                Four,
                "TITAN"
            );

            return;
        }

        if (step == 2)
        {
            ShowRoute(
                Four,
                A,
                "TITAN"
            );

            return;
        }

        ShowRoute(
            A,
            Two,
            "TITAN"
        );
    }

    private void ShowRoute(
        Vector3 currentPosition,
        Vector3 nextPosition,
        string primalName)
    {
        if (Controller.TryGetElementByName(
                "Roulette_Current",
                out var current))
        {
            current.SetOffPosition(currentPosition);
            current.overlayText = primalName;
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "Roulette_Next",
                out var next))
        {
            next.SetOffPosition(nextPosition);
            next.overlayText = "NEXT";
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "Roulette_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "Roulette_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }

    private static float HorizontalDistance(
        Vector3 a,
        Vector3 b)
    {
        var dx = a.X - b.X;
        var dz = a.Z - b.Z;

        return MathF.Sqrt(dx * dx + dz * dz);
    }
}
