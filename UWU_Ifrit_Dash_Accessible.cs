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
    private const uint CrimsonCycloneCastId = 0x2B5F;
    private static readonly Vector3 Center = new(100f, 0f, 100f);

    private bool active;
    private long startedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [Raids.the_Weapons_Refrain_Ultimate];

    public override Metadata? Metadata =>
        new(1, "Maggie UWU Ifrit Dash Accessible");

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

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != CrimsonCycloneCastId)
            return;

        var ifrit = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x => x.EntityId == source);

        if (ifrit == null)
            return;

        Vector3 start = ifrit.Position;

        Vector3 end = new Vector3(
            Center.X * 2f - start.X,
            start.Y,
            Center.Z * 2f - start.Z
        );

        ShowRoute(start, end);

        active = true;
        startedAt = Environment.TickCount64;
    }

    public override void OnUpdate()
    {
        if (!active)
            return;

        if (Environment.TickCount64 - startedAt > 4500)
            OnReset();
    }

    public override void OnReset()
    {
        active = false;
        startedAt = 0;

        if (Controller.TryGetElementByName("IfritDash_Current", out var current))
            current.Enabled = false;

        if (Controller.TryGetElementByName("IfritDash_Next", out var next))
            next.Enabled = false;
    }

    private void ShowRoute(Vector3 currentPosition, Vector3 nextPosition)
    {
        if (!Controller.TryGetElementByName("IfritDash_Current", out var current))
            return;

        if (!Controller.TryGetElementByName("IfritDash_Next", out var next))
            return;

        current.SetRefPosition(currentPosition);
        next.SetRefPosition(nextPosition);

        current.Enabled = true;
        next.Enabled = true;
    }
}
