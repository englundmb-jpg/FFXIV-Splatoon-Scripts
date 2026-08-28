using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UCOB_Quickmarch_Accessible : SplatoonScript
{
    private static readonly Vector3 Center = new(0.0f, 0.0f, 0.0f);
    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(2, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("Quickmarch_Current", """
        {"Name":"CURRENT","Enabled":false,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"FillStep":1.0,"tether":true,"LegacyFill":true,"overlayText":"D3 BOTTOM","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}
        """);
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != 0x26E2) return;
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

        var bosses = Svc.Objects.OfType<IBattleNpc>().Where(x =>
            x.Name.TextValue.Contains("Twintania", StringComparison.OrdinalIgnoreCase) ||
            x.Name.TextValue.Contains("Nael", StringComparison.OrdinalIgnoreCase) ||
            x.Name.TextValue.Contains("Bahamut", StringComparison.OrdinalIgnoreCase)).ToArray();

        if (bosses.Length > 0)
        {
            var north = new Vector3(
                bosses.Average(x => x.Position.X), 0.0f,
                bosses.Average(x => x.Position.Z));
            var direction = north - Center;
            direction.Y = 0.0f;
            if (direction.LengthSquared() > 0.001f &&
                Controller.TryGetElementByName("Quickmarch_Current", out var current))
            {
                direction = Vector3.Normalize(direction);
                var right = new Vector3(-direction.Z, 0.0f, direction.X);
                current.SetOffPosition(Center - direction * 14.5f - right * 1.8f);
                current.Enabled = true;
            }
        }
        if (Environment.TickCount64 - activatedAt > 18000) OnReset();
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;
        if (Controller.TryGetElementByName("Quickmarch_Current", out var current))
            current.Enabled = false;
    }
}
