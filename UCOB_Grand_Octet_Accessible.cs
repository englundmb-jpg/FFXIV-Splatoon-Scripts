using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UCOB_Grand_Octet_Accessible : SplatoonScript
{
    private bool active;
    private bool calculated;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(2, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("Octet_Current", """
        {"Name":"CURRENT","Enabled":false,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"FillStep":1.0,"tether":true,"LegacyFill":true,"overlayText":"CURRENT","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}
        """);
        Controller.RegisterElementFromCode("Octet_Next", """
        {"Name":"NEXT","Enabled":false,"radius":2.2,"Donut":0.35,"color":4294902015,"thicc":8.0,"FillStep":1.0,"tether":false,"LegacyFill":true,"overlayText":"NEXT","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}
        """);
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != 0x26E7) return;
        active = true;
        calculated = false;
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

        var elapsed = Environment.TickCount64 - activatedAt;
        if (!calculated && elapsed >= 4800) CalculatePositions();
        if (elapsed > 20000) OnReset();
    }

    public override void OnReset()
    {
        active = false;
        calculated = false;
        activatedAt = 0;
        if (Controller.TryGetElementByName("Octet_Current", out var current))
            current.Enabled = false;
        if (Controller.TryGetElementByName("Octet_Next", out var next))
            next.Enabled = false;
    }

    private void CalculatePositions()
    {
        var bosses = Svc.Objects.OfType<IBattleNpc>().ToArray();
        var bahamut = bosses.FirstOrDefault(x => x.Name.TextValue.Contains(
            "Bahamut", StringComparison.OrdinalIgnoreCase));
        var nael = bosses.FirstOrDefault(x => x.Name.TextValue.Contains(
            "Nael", StringComparison.OrdinalIgnoreCase));
        if (bahamut == null || nael == null) return;

        var bahamutSector = Sector(bahamut.Position);
        var naelSector = Sector(nael.Position);
        var cardinal = (bahamutSector & 1) == 0;
        var step = cardinal ? -1 : 1;
        var start = (bahamutSector + 4) & 7;
        if (start == naelSector) start = (start + step + 8) & 7;
        var nextSector = (start + step + 8) & 7;

        if (Controller.TryGetElementByName("Octet_Current", out var current))
        {
            current.SetOffPosition(Point(start));
            current.overlayText = cardinal ? "ROTATE CCW" : "ROTATE CW";
            current.Enabled = true;
        }
        if (Controller.TryGetElementByName("Octet_Next", out var next))
        {
            next.SetOffPosition(Point(nextSector));
            next.Enabled = true;
        }
        calculated = true;
    }

    private static int Sector(Vector3 position)
    {
        var radians = Math.Atan2(position.X, -position.Z);
        return ((int)Math.Round(radians / (Math.PI / 4.0)) + 8) & 7;
    }

    private static Vector3 Point(int sector)
    {
        var radians = sector * Math.PI / 4.0;
        return new Vector3((float)Math.Sin(radians) * 17.5f,
            0.0f, (float)-Math.Cos(radians) * 17.5f);
    }
}
