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
    private const uint TowerCast = 9951;
    private const uint BahamutDataId = 0x1FE8;
    private uint? selectedTower;

    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(100, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("Your_Tower", """
            {
              "Name":"YOUR TOWER — FOURTH COUNTERCLOCKWISE",
              "Enabled":false,
              "radius":0.0,
              "color":4294967040,
              "thicc":8.0,
              "tether":true,
              "LegacyFill":false
            }
            """);
        OnReset();
    }

    public override void OnUpdate()
    {
        if (!Controller.TryGetElementByName("Your_Tower", out var line))
            return;
        line.Enabled = false;

        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            selectedTower = null;
            return;
        }

        // Published Splatoon resolver: exactly eight actors casting Megaflare Tower.
        var towers = Svc.Objects.OfType<IBattleChara>()
            .Where(x => x.IsCasting && x.CastActionId == TowerCast).ToArray();
        if (towers.Length != 8)
        {
            selectedTower = null;
            return;
        }

        if (selectedTower == null)
        {
            var bosses = Svc.Objects.OfType<IBattleChara>()
                .Where(x => x.DataId == BahamutDataId).ToArray();
            if (bosses.Length != 1)
                return;
            var bahamut = Floor(bosses[0].Position);
            if (bahamut.LengthSquared() < 1f)
                return;
            var nearest = towers.OrderBy(x => Vector2.DistanceSquared(Floor(x.Position), bahamut)).ToArray();
            // Do not choose arbitrarily if two towers are effectively equally close.
            if (MathF.Abs(Vector2.DistanceSquared(Floor(nearest[0].Position), bahamut)
                - Vector2.DistanceSquared(Floor(nearest[1].Position), bahamut)) < 0.01f)
                return;
            // X points east and Z points south: descending angle runs counterclockwise.
            var ordered = towers.OrderByDescending(x => MathF.Atan2(x.Position.Z, x.Position.X)).ToArray();
            var first = Array.IndexOf(ordered, nearest[0]);
            selectedTower = ordered[(first + 3) % 8].EntityId;
        }

        var tower = towers.FirstOrDefault(x => x.EntityId == selectedTower);
        if (tower == null)
        {
            selectedTower = null;
            return;
        }
        line.SetRefPosition(tower.Position);
        line.Enabled = true;
    }

    public override void OnReset()
    {
        selectedTower = null;
        if (Controller.TryGetElementByName("Your_Tower", out var line))
            line.Enabled = false;
    }

    private static Vector2 Floor(Vector3 position) => new(position.X, position.Z);
}
