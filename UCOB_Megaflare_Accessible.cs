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

public sealed class UCOB_Megaflare_Accessible : SplatoonScript
{
    private const uint MegaflareCastId = 9953;

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Megaflare_Safe",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":8.0,
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
        if (!active)
            return;

        if (Environment.TickCount64 - activatedAt > 12000)
        {
            OnReset();
        }
    }

    public override unsafe void OnStartingCast(
        uint sourceId,
        PacketActorCast* packet)
    {
        if (packet->ActionDescriptor !=
            new ActionDescriptor(
                FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action,
                MegaflareCastId))
            return;

        var bahamut = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x => x.EntityId == sourceId);

        if (bahamut == null)
            return;

        if (!bahamut.Name.TextValue.Contains(
                "Bahamut",
                StringComparison.OrdinalIgnoreCase))
            return;

        Activate();
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

        if (Controller.TryGetElementByName(
                "Megaflare_Safe",
                out var safe))
        {
            safe.Enabled = true;
        }
    }

    private void HideMarker()
    {
        if (Controller.TryGetElementByName(
                "Megaflare_Safe",
                out var safe))
        {
            safe.Enabled = false;
        }
    }
}
