using ECommons.DalamudServices;
using Splatoon.Data;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UCOB_Tenstrike_Accessible_v2 : SplatoonScript
{
    private static readonly Vector3 SouthThree = new(0.0f, 0.0f, 8.75f);
    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(2, "Maggie");

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
              "overlayText":"SOUTH 3 SAFE",
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
        if (castId != 0x26E6) return;
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

        if (Controller.TryGetElementByName("Tenstrike_Current", out var current))
        {
            current.SetOffPosition(SouthThree);
            current.Enabled = true;
        }
        if (Environment.TickCount64 - activatedAt > 19000) OnReset();
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;
        if (Controller.TryGetElementByName("Tenstrike_Current", out var current))
            current.Enabled = false;
    }
}
