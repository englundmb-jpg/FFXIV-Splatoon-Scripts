// Adapted from PunishXIV/Splatoon; original authors retained in Metadata.
// NAUR review 2026-10-09. See TEA_NAUR_R1_README.md for coverage and validation.
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.ImGuiMethods;
using Splatoon;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using ECommons.DalamudServices.Legacy;

namespace MaggieSplatoon.TEA;

public class TEA_P2_Temporal_Stasis : SplatoonScript
{
    public enum BaitType
    {
        West,
        East,
        NorthLeftBossSide,
        SouthLeftBossSide,
        NorthRightBossSide,
        SouthRightBossSide,
        JusticeSide
    }

    private enum CruiseChaserSide
    {
        East,
        West
    }

    private const uint LightningDebuffId = 1121;
    private const uint RedTetherDebuffId = 1123;
    private const uint BlueTetherDebuffId = 1124;
    private bool _isStartTemporalStasis;
    private bool _shouldDisplayElement;
    private BaitType? _blueSide;
    private long _expires;

    public override HashSet<uint>? ValidTerritories => [887];
    public override Metadata? Metadata => new(103, "Garume; Maggie accessibility repair");

    private IBattleNpc? CruiseChaser =>
        Svc.Objects.OfType<IBattleNpc>()
            .FirstOrDefault(x => x.DataId == 0x2C4E);

    private Config C => Controller.GetConfig<Config>();

    public override void OnSetup()
    {
        var element = new Element(0)
        {
            radius = 0.8f,
            thicc = 5f,
            color = 0xFF00FF00,
            overlayText = "CURRENT",
            overlayVOffset = 2f,
            overlayFScale = 2f,
            tether = true
        };

        Controller.RegisterElement(
            "TEA_P2_Temporal_Stasis_Bait",
            element,
            true);
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if(castId != 18522) return; // Temporal Stasis, verified in BossMod TEAEnums.
        OnReset();
        _isStartTemporalStasis = true;
        _expires = Environment.TickCount64 + 20000;
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if(_isStartTemporalStasis && set.Action is { RowId: 18531 }) OnReset();
    }

    public override void OnTetherCreate(uint source, uint target, uint data2, uint data3, uint data5)
    {
        if(_isStartTemporalStasis && data3 is 28 or 29)
            _shouldDisplayElement = true;
    }

    public override void OnUpdate()
    {
        Controller.GetRegisteredElements()
            .Each(x => x.Value.Enabled = false);

        var cruiseChaser = CruiseChaser;

        if(cruiseChaser == null || !_isStartTemporalStasis || Player.Object == null)
            return;
        if(Environment.TickCount64 >= _expires) { OnReset(); return; }
        // Wait for debuffs to actually exist before treating this player as "no debuff".
        if(Svc.Objects.OfType<IBattleChara>().Any(p => p.StatusList.Any(s => s.StatusId is 1121 or 1123 or 1124)))
            _shouldDisplayElement = true;
        if(!_shouldDisplayElement)
            return;

        var statuses = Player.Status;
        var baitType = GetBaitType(statuses);
        var cruiseChaserSide =
            GetCruiseChaserSide(cruiseChaser);

        var baitPosition =
            GetBaitPosition(baitType, cruiseChaserSide);

        if(Controller.TryGetElementByName(
            "TEA_P2_Temporal_Stasis_Bait",
            out var element))
        {
            element.Enabled = _shouldDisplayElement;
            element.refX = baitPosition.X;
            element.refY = baitPosition.Y;
        }
    }

    public override void OnReset()
    {
        _isStartTemporalStasis = false;
        _shouldDisplayElement = false;
        _blueSide = null;
        _expires = 0;
        Controller.GetRegisteredElements().Each(x => x.Value.Enabled = false);
    }

    private BaitType GetBaitType(
        IEnumerable<IStatus> statuses)
    {
        foreach(var status in statuses)
        {
            switch(status.StatusId)
            {
                case LightningDebuffId:
                    return C.LightningBaitPosition;

                case RedTetherDebuffId:
                    return C.RedTetherBaitPosition;

                case BlueTetherDebuffId:
                    if(C.NaurBlueChooseNearest)
                    {
                        var local = Player.Object;
                        if(local == null) return C.BlueTetherBaitPosition;
                        _blueSide ??= local.Position.X < 100f ? BaitType.West : BaitType.East;
                        return _blueSide.Value;
                    }
                    return C.BlueTetherBaitPosition;
            }
        }

        return C.NothingBaitPosition;
    }

    private CruiseChaserSide GetCruiseChaserSide(
        IGameObject cruiseChaser)
    {
        return cruiseChaser.Position.X > 100f
            ? CruiseChaserSide.East
            : CruiseChaserSide.West;
    }

    private Vector2 GetBaitPosition(
        BaitType baitType,
        CruiseChaserSide cruiseChaserSide)
    {
        return (baitType, cruiseChaserSide) switch
        {
            (BaitType.NorthRightBossSide, _) =>
                new Vector2(106f, 98f),

            (BaitType.SouthRightBossSide, _) =>
                new Vector2(106f, 102f),

            (BaitType.NorthLeftBossSide, _) =>
                new Vector2(94f, 98f),

            (BaitType.SouthLeftBossSide, _) =>
                new Vector2(94f, 102f),

            (BaitType.East, CruiseChaserSide.East) =>
                new Vector2(113f, 100f),

            (BaitType.East, CruiseChaserSide.West) =>
                new Vector2(118f, 100f),

            (BaitType.West, CruiseChaserSide.East) =>
                new Vector2(82f, 100f),

            (BaitType.West, CruiseChaserSide.West) =>
                new Vector2(87f, 100f),

            (BaitType.JusticeSide, CruiseChaserSide.East) =>
                new Vector2(82f, 100f),

            (BaitType.JusticeSide, CruiseChaserSide.West) =>
                new Vector2(118f, 100f),

            _ => Vector2.Zero
        };
    }

    public override void OnSettingsDraw()
    {
        ImGuiEx.Text("NAUR R1/DPS: red EAST boss south; no debuff WEST boss south.");
        Dalamud.Bindings.ImGui.ImGui.Checkbox("Blue: lock nearest side (NAUR FFA)", ref C.NaurBlueChooseNearest);
        ImGuiEx.Text("Blue Tether Bait Position (when nearest-side is off)");
        ImGuiEx.EnumCombo(
            "##BlueTetherBaitPosition",
            ref C.BlueTetherBaitPosition);

        ImGuiEx.Text("Red Tether Bait Position");
        ImGuiEx.EnumCombo(
            "##RedTetherBaitPosition",
            ref C.RedTetherBaitPosition);

        ImGuiEx.Text("Lightning Bait Position");
        ImGuiEx.EnumCombo(
            "##LightningBaitPosition",
            ref C.LightningBaitPosition);

        ImGuiEx.Text("Nothing Bait Position");
        ImGuiEx.EnumCombo(
            "##NothingBaitPosition",
            ref C.NothingBaitPosition);
    }

    private class Config : IEzConfig
    {
        public bool NaurBlueChooseNearest = true;
        public BaitType BlueTetherBaitPosition = BaitType.East;
        public BaitType LightningBaitPosition = BaitType.JusticeSide;
        public BaitType NothingBaitPosition = BaitType.SouthLeftBossSide;
        public BaitType RedTetherBaitPosition = BaitType.SouthRightBossSide;
    }
}

