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

public sealed class UCOB_Heavensfall_R1_Accessible_v2 : SplatoonScript
{
    private static readonly Vector3 Center = new(0.0f, 0.0f, 0.0f);
    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(2, "Maggie");

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
              "overlayText":"OPPOSITE NAEL",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != 0x26E5) return;
        active = true;
        activatedAt = Environment.TickCount64;
    }

    public override void OnUpdate()
    {
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active) OnReset();
            return;
        }
        if (!active) return;

        var nael = Svc.Objects.OfType<IBattleNpc>().FirstOrDefault(x =>
            x.Name.TextValue.Contains("Nael", StringComparison.OrdinalIgnoreCase));
        if (nael != null)
        {
            var direction = nael.Position - Center;
            direction.Y = 0.0f;
            if (direction.LengthSquared() > 0.001f &&
                Controller.TryGetElementByName("Heavensfall_Current", out var current))
            {
                direction = Vector3.Normalize(direction);
                current.SetOffPosition(Center - direction * 15.5f);
                current.Enabled = true;
            }
        }
        if (Environment.TickCount64 - activatedAt > 19000) OnReset();
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;
        if (Controller.TryGetElementByName("Heavensfall_Current", out var current))
            current.Enabled = false;
    }
}
