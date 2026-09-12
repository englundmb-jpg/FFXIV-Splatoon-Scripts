using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaggieScripts.Duties.Endwalker;

public sealed class P12S_R1_R2_Role_Accessible : SplatoonScript
{
    private long nextCheck;
    private string detectedRole = "WAITING";

    public override HashSet<uint>? ValidTerritories { get; } =
        [1154];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "P12S_Role",
            """
            {
              "Name":"P12S ROLE",
              "Enabled":false,
              "radius":0.0,
              "thicc":0.0,
              "refActorType":1,
              "overlayText":"P12S ROLE",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayVOffset":2.7,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        if (Environment.TickCount64 < nextCheck)
            return;

        nextCheck = Environment.TickCount64 + 2000;
        detectedRole = DetectRole();

        if (Controller.TryGetElementByName(
            "P12S_Role",
            out var role))
        {
            role.overlayText =
                $"P12S: {detectedRole}";

            role.Enabled = true;
        }
    }

    public override void OnReset()
    {
        nextCheck = 0;
        detectedRole = "WAITING";

        if (Controller.TryGetElementByName(
            "P12S_Role",
            out var role))
        {
            role.Enabled = false;
        }
    }

    private static string DetectRole()
    {
        var player = Svc.ClientState.LocalPlayer;

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

        return "NOT RANGED";
    }
}
