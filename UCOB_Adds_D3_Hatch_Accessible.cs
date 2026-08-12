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

public sealed class UCOB_Adds_D3_Hatch_Accessible : SplatoonScript
{
    // Verified UCOB Waymark 3 / D3 Neurolink.
    private static readonly Vector3 D3Neurolink =
        new(0.0f, 0.0f, 8.75f);

    // Twister bait position just outside the back Neurolink.
    private static readonly Vector3 TwisterBait =
        new(0.0f, 0.0f, 11.75f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "AddsHatch_Current",
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
            "AddsHatch_Next",
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

        // Both bosses being present identifies Adds phase.
        if (twin == null ||
            nael == null)
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
            Environment.TickCount64 - activatedAt > 30000)
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
        // D3 / R1:
        //
        // CURRENT:
        // Stay outside the back Neurolink
        // so Twister is not dropped inside it.
        //
        // NEXT:
        // Enter the bottom Neurolink
        // after Twister resolves to take Hatch.

        if (Controller.TryGetElementByName(
                "AddsHatch_Current",
                out var current))
        {
            current.SetOffPosition(TwisterBait);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "AddsHatch_Next",
                out var next))
        {
            next.SetOffPosition(D3Neurolink);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "AddsHatch_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "AddsHatch_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}
