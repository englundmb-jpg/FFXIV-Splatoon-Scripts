using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
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
    private bool knockbackDone;
    private bool heavensfallActive;
    private Vector2? bahamutDivePosition;

    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(103, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Your_Tower",
            """
            {
              "Name":"YOUR TOWER — FOURTH COUNTERCLOCKWISE",
              "Enabled":false,
              "radius":0.7,
              "Donut":0.25,
              "FillStep":1.0,
              "color":4294967040,
              "thicc":8.0,
              "tether":true,
              "LegacyFill":true
            }
            """
        );
        Controller.RegisterElementFromCode(
            "Knockback_Stand",
            """
            {
              "Name":"KNOCKBACK — STAND HERE",
              "Enabled":false,
              "radius":0.5,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "tether":true,
              "FillStep":1.0,
              "LegacyFill":true
            }
            """
        );
        Controller.RegisterElementFromCode(
            "Instruction",
            """
            {
              "Name":"HEAVENSFALL INSTRUCTION",
              "Enabled":false,
              "type":1,
              "refActorType":1,
              "radius":0.0,
              "color":0,
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":2.0,
              "overlayVOffset":2.5
            }
            """
        );
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == 9957)
        {
            OnReset();
            heavensfallActive = true;
        }
        else if (heavensfallActive && castId == 9953)
        {
            var bahamut = Svc.Objects.OfType<IBattleChara>()
                .FirstOrDefault(x => x.EntityId == source && x.DataId == BahamutDataId);
            if (bahamut != null)
                bahamutDivePosition = Floor(bahamut.Position);
        }
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (heavensfallActive && set.Action is { RowId: 9912 })
            knockbackDone = true;
        if (set.Action is { RowId: TowerCast })
            OnReset();
    }

    public override void OnUpdate()
    {
        if (!Controller.TryGetElementByName("Your_Tower", out var line))
            return;
        SetEnabled("Your_Tower", false);
        SetEnabled("Knockback_Stand", false);
        SetEnabled("Instruction", false);
        Controller.TryGetElementByName("Knockback_Stand", out var stand);
        Controller.TryGetElementByName("Instruction", out var instruction);

        if (!Svc.Condition[ConditionFlag.InCombat])
        {
            OnReset();
            return;
        }

        if (!heavensfallActive)
            return;

        // Published Splatoon resolver: exactly eight actors casting Megaflare Tower.
        var towers = Svc.Objects.OfType<IBattleChara>()
            .Where(x => x.IsCasting && x.CastActionId == TowerCast).ToArray();
        if (towers.Length != 8)
            return;

        if (selectedTower == null)
        {
            if (bahamutDivePosition is not Vector2 bahamut || bahamut.LengthSquared() < 1f)
            {
                if (instruction != null)
                {
                    instruction.overlayText = "TOWER NOT IDENTIFIED — FOLLOW PARTY";
                    instruction.Enabled = true;
                }
                return;
            }
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
            OnReset();
            return;
        }
        var position = Floor(tower.Position);
        if (position.LengthSquared() < 1f)
            return;
        // The tower has radius 3; mark its inward-facing edge.
        var front = position - Vector2.Normalize(position) * 3f;
        line.SetOffPosition(new Vector3(front.X, tower.Position.Y, front.Y));
        line.color = knockbackDone ? 4278255360u : 4294967040u;
        line.Enabled = true;
        if (!knockbackDone && stand != null)
        {
            // BossMod P3HeavensfallTowers uses radius 9 toward the assigned tower.
            var spot = Vector2.Normalize(position) * 9f;
            stand.SetOffPosition(new Vector3(spot.X, 0f, spot.Y));
            stand.Enabled = true;
        }
        if (instruction != null)
        {
            instruction.overlayText = knockbackDone
                ? "ENTER YOUR TOWER — GREEN CIRCLE"
                : "STAND ON GREEN — CYAN CIRCLE MARKS YOUR TOWER";
            instruction.Enabled = true;
        }
    }

    public override void OnReset()
    {
        selectedTower = null;
        knockbackDone = false;
        heavensfallActive = false;
        bahamutDivePosition = null;
        SetEnabled("Your_Tower", false);
        SetEnabled("Knockback_Stand", false);
        SetEnabled("Instruction", false);
    }

    private void SetEnabled(string name, bool enabled)
    {
        if (Controller.TryGetElementByName(name, out var element))
            element.Enabled = enabled;
    }

    private static Vector2 Floor(Vector3 position) => new(position.X, position.Z);
}
