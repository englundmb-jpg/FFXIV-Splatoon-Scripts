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

public sealed class UCOB_Tenstrike_Accessible : SplatoonScript
{
    private static readonly Vector3 Center =
        new(0.0f, 0.0f, 0.0f);

    private static readonly Vector3 SafeOne =
        new(0.0f, 0.0f, -8.0f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Tenstrike_Current",
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
            "Tenstrike_Next",
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

        var twin = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Twintania",
                    StringComparison.OrdinalIgnoreCase));

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        var bahamut = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Bahamut",
                    StringComparison.OrdinalIgnoreCase));

        if (twin == null ||
            nael == null ||
            bahamut == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active)
            DrawPositions();

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

    private void DrawPositions()
    {
        // NAUR Tenstrike:
        // Waymark 1 is always safe for Earthshakers.
        //
        // CURRENT = safe Earthshaker position.
        // NEXT = return toward center / waymark 4.

        if (Controller.TryGetElementByName(
                "Tenstrike_Current",
                out var current))
        {
            current.SetOffPosition(SafeOne);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "Tenstrike_Next",
                out var next))
        {
            next.SetOffPosition(Center);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "Tenstrike_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "Tenstrike_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}
