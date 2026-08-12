using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UCOB_Nael_Dive_Accessible : SplatoonScript
{
    private const uint NaelNpcId = 2617;

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        // VERIFIED layout:
        // CURRENT = +15,+15 relative to Nael's rotation.
        Controller.RegisterElementFromCode(
            "NaelDive_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "type":3,
              "offX":15.0,
              "offY":15.0,
              "radius":2.5,
              "color":4278255360,
              "thicc":8.0,
              "refActorNPCID":2617,
              "refActorComparisonType":4,
              "includeRotation":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        // VERIFIED layout:
        // NEXT = -15,+15 relative to Nael's rotation.
        Controller.RegisterElementFromCode(
            "NaelDive_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "type":3,
              "offX":-15.0,
              "offY":15.0,
              "radius":2.2,
              "color":4294967040,
              "thicc":8.0,
              "refActorNPCID":2617,
              "refActorComparisonType":4,
              "includeRotation":true,
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
        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.DataId == NaelNpcId);

        if (nael == null)
        {
            if (active)
                OnReset();

            return;
        }

        // First implementation test:
        // use the verified Nael actor itself to prove the
        // actor-relative CURRENT/NEXT geometry in script form.
        if (!active)
            Activate();

        if (active &&
            Environment.TickCount64 - activatedAt > 10000)
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

        if (Controller.TryGetElementByName(
                "NaelDive_Current",
                out var current))
        {
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "NaelDive_Next",
                out var next))
        {
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "NaelDive_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "NaelDive_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}
