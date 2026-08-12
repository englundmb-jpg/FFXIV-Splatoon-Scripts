using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UCOB_Heavensfall_Towers_Accessible : SplatoonScript
{
    private static readonly Vector3 Tower1 =
        new(99.0f, 0.0f, 99.0f);

    private static readonly Vector3 Tower2 =
        new(101.0f, 0.0f, 101.0f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Heavensfall_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":3.0,
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
            "Heavensfall_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":3.0,
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
        // VERIFIED layout trigger:
        // "The Brobdingnagian is consumed by"
        //
        // Use the actual Brobdingnagian actor disappearing
        // as the activation condition.

        var brobdingnagian = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Brobdingnagian",
                    StringComparison.OrdinalIgnoreCase));

        if (!active)
        {
            if (brobdingnagian != null)
                return;

            // Do not activate simply because the actor is absent
            // at the beginning of the instance.
            return;
        }

        if (Environment.TickCount64 - activatedAt > 15000)
            OnReset();
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

        ShowRoute(
            Tower1,
            Tower2
        );
    }

    private void ShowRoute(
        Vector3 currentPosition,
        Vector3 nextPosition)
    {
        if (Controller.TryGetElementByName(
                "Heavensfall_Current",
                out var current))
        {
            current.SetOffPosition(currentPosition);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "Heavensfall_Next",
                out var next))
        {
            next.SetOffPosition(nextPosition);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "Heavensfall_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "Heavensfall_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}
