using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Endwalker;

public sealed class P12S_Superchain1_R1_R2_Accessible : SplatoonScript
{
    private const uint MasterSphere = 16176;
    private const uint PointBlank = 16177;
    private const uint Donut = 16178;
    private const uint Protean = 16179;
    private const uint Pairs = 16180;

    private static readonly Vector3 Center =
        new(100.0f, 0.0f, 100.0f);

    private readonly Dictionary<uint, HashSet<uint>> attachments = new();

    public override HashSet<uint>? ValidTerritories { get; } =
        [1154];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Superchain_Current",
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
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "Superchain_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":2.2,
              "Donut":0.35,
              "color":4294967040,
              "thicc":8.0,
              "FillStep":1.0,
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

    public override void OnTetherCreate(
        uint source,
        uint target,
        uint data2,
        uint data3,
        uint data5)
    {
        var sourceObject = GetBattleNpc(source);
        var targetObject = GetBattleNpc(target);

        if (sourceObject == null ||
            targetObject == null ||
            targetObject.DataId != MasterSphere)
            return;

        if (sourceObject.DataId != PointBlank &&
            sourceObject.DataId != Donut &&
            sourceObject.DataId != Protean &&
            sourceObject.DataId != Pairs)
            return;

        if (!attachments.TryGetValue(
            target,
            out var children))
        {
            children = new HashSet<uint>();
            attachments[target] = children;
        }

        children.Add(source);
    }

    public override void OnUpdate()
    {
        var mechanics = FindMechanics();

        if (mechanics.Count == 0)
        {
            HideMarkers();

            if (!Svc.Objects.Any(x =>
                x.DataId == MasterSphere))
            {
                attachments.Clear();
            }

            return;
        }

        DrawMarker(
            "Superchain_Current",
            mechanics[0],
            "CURRENT");

        if (mechanics.Count > 1)
        {
            DrawMarker(
                "Superchain_Next",
                mechanics[1],
                "NEXT");
        }
        else if (Controller.TryGetElementByName(
            "Superchain_Next",
            out var next))
        {
            next.Enabled = false;
        }
    }

    public override void OnReset()
    {
        attachments.Clear();
        HideMarkers();
    }

    private List<MechanicInfo> FindMechanics()
    {
        var result = new List<MechanicInfo>();

        foreach (var attachment in attachments)
        {
            var master = GetBattleNpc(attachment.Key);

            if (master == null ||
                master.DataId != MasterSphere)
                continue;

            var children = attachment.Value
                .Select(GetBattleNpc)
                .Where(x => x != null)
                .Cast<IBattleNpc>()
                .ToList();

            if (children.Count == 0)
                continue;

            var distance = children.Min(x =>
                Vector3.Distance(
                    master.Position,
                    x.Position));

            var movement = children.Any(x =>
                x.DataId == Donut)
                ? "IN"
                : children.Any(x =>
                    x.DataId == PointBlank)
                    ? "OUT"
                    : "";

            var formation = children.Any(x =>
                x.DataId == Pairs)
                ? "PAIR"
                : children.Any(x =>
                    x.DataId == Protean)
                    ? "SPREAD"
                    : "";

            result.Add(new MechanicInfo(
                master.Position,
                distance,
                movement,
                formation));
        }

        return result
            .OrderBy(x => x.Distance)
            .ToList();
    }

    private void DrawMarker(
        string elementName,
        MechanicInfo mechanic,
        string prefix)
    {
        if (!Controller.TryGetElementByName(
            elementName,
            out var element))
            return;

        var position = GetPersonalPosition(mechanic);

        element.SetRefPosition(position);
        element.overlayText =
            $"{prefix}: {mechanic.Movement} {mechanic.Formation} {DetectRole()}";
        element.Enabled = true;
    }

    private static Vector3 GetPersonalPosition(
        MechanicInfo mechanic)
    {
        var safe = mechanic.Position;

        if (mechanic.Movement == "OUT")
        {
            var towardCenter = Center - safe;
            towardCenter.Y = 0.0f;

            if (towardCenter.LengthSquared() > 0.001f)
            {
                safe += Vector3.Normalize(towardCenter) * 8.5f;
            }
        }

        var role = DetectRole();
        safe.X += role == "R2" ? 2.0f : -2.0f;
        safe.Z += 2.0f;

        return safe;
    }

    private static string DetectRole()
    {
        var player = Player.Object;

        if (player == null)
            return "WAITING";

        uint[] rangedJobs =
            [23, 25, 27, 31, 35, 38, 42];

        var rangedPlayers = Svc.Party
            .Where(x =>
                x.GameObject is IPlayerCharacter member &&
                rangedJobs.Contains(member.ClassJob.RowId))
            .Select(x =>
                (IPlayerCharacter)x.GameObject!)
            .ToList();

        var index = rangedPlayers.FindIndex(x =>
            x.EntityId == player.EntityId);

        if (index == 0)
            return "R1";

        if (index == 1)
            return "R2";

        return "RANGED";
    }

    private static IBattleNpc? GetBattleNpc(uint entityId)
    {
        return Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.EntityId == entityId);
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
            "Superchain_Current",
            out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
            "Superchain_Next",
            out var next))
        {
            next.Enabled = false;
        }
    }

    private sealed record MechanicInfo(
        Vector3 Position,
        float Distance,
        string Movement,
        string Formation);
}
