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

public sealed class UCOB_Fellruin_Accessible : SplatoonScript
{
    private static readonly Vector3 Center =
        new(100.0f, 0.0f, 100.0f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Fellruin_Current",
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
            "Fellruin_Next",
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
        // Never display positional guidance before the pull or after a wipe.
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var bahamut = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Bahamut",
                    StringComparison.OrdinalIgnoreCase));

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        if (bahamut == null ||
            nael == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active)
        {
            DrawPositions(
                bahamut.Position,
                nael.Position
            );
        }

        if (active &&
            Environment.TickCount64 - activatedAt > 15000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;

        HideMarkers();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }

    private void DrawPositions(
        Vector3 bahamutPosition,
        Vector3 naelPosition)
    {
        var bahamutDirection =
            bahamutPosition - Center;

        bahamutDirection.Y = 0.0f;

        if (bahamutDirection.LengthSquared() < 0.001f)
            return;

        bahamutDirection =
            Vector3.Normalize(bahamutDirection);

        var right =
            new Vector3(
                -bahamutDirection.Z,
                0.0f,
                bahamutDirection.X
            );

        // D3 / R1 Quickmarch-style spread position
        // with Bahamut treated as relative north.
        var currentPosition =
            Center -
            bahamutDirection * 5.5f +
            right * 3.5f;

        // Party Neurolink is opposite Bahamut.
        var nextPosition =
            Center -
            bahamutDirection * 10.0f;

        if (Controller.TryGetElementByName(
                "Fellruin_Current",
                out var current))
        {
            current.SetOffPosition(currentPosition);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "Fellruin_Next",
                out var next))
        {
            next.SetOffPosition(nextPosition);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "Fellruin_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "Fellruin_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}
