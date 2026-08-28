using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaggieScripts.Duties.Stormblood;

// Clean Nael-only replacement for the old bundle.
// The old bundle contained seven scripts that all activated from boss presence.
public sealed class UCOB_Accessible_Clean_Bundle : SplatoonScript
{
    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(3, "Maggie");

    public override void OnSetup()
    {
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
              "color":4294902015,
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
        if (!Svc.Condition[
                Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        if (nael == null)
        {
            if (active)
                OnReset();

            return;
        }

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
