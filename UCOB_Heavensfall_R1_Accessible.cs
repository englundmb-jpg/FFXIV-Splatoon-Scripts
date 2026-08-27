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

public sealed class UCOB_Heavensfall_R1_Accessible : SplatoonScript
{
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

        if (active)
        {
            if (Controller.TryGetElementByName(
                    "Heavensfall_Current",
                    out var current))
            {
                current.SetOffPosition(nael.Position);
                current.Enabled = true;
            }
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

        HideMarker();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }

    private void HideMarker()
    {
        if (Controller.TryGetElementByName(
                "Heavensfall_Current",
                out var current))
        {
            current.Enabled = false;
        }
    }
}
