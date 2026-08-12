using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Splatoon.Data;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UCOB_Grand_Octet_Accessible : SplatoonScript
{
    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Octet_Nael",
            """
            {
              "Name":"CURRENT",
              "Enabled":true,
              "type":3,
              "offY":50.0,
              "radius":4.0,
              "color":4278255360,
              "thicc":8.0,
              "refActorNPCID":2612,
              "refActorRequireCast":true,
              "refActorCastId":[9923],
              "FillStep":1.0,
              "refActorComparisonType":4,
              "includeRotation":true,
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
            "Octet_Dragon1",
            """
            {
              "Name":"NEXT",
              "Enabled":true,
              "type":3,
              "offY":47.14,
              "radius":4.0,
              "color":4294967040,
              "thicc":8.0,
              "refActorNPCID":6958,
              "refActorRequireCast":true,
              "refActorCastId":[9931,9932,9933,9934,9935],
              "refActorComparisonType":4,
              "includeRotation":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "Octet_Dragon2",
            """
            {
              "Name":"NEXT",
              "Enabled":true,
              "type":3,
              "offY":47.14,
              "radius":4.0,
              "color":4294967040,
              "thicc":8.0,
              "refActorNPCID":6957,
              "refActorRequireCast":true,
              "refActorCastId":[9931,9932,9933,9934,9935],
              "refActorComparisonType":4,
              "includeRotation":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "Octet_Dragon3",
            """
            {
              "Name":"NEXT",
              "Enabled":true,
              "type":3,
              "offY":47.14,
              "radius":4.0,
              "color":4294967040,
              "thicc":8.0,
              "refActorNPCID":2630,
              "refActorRequireCast":true,
              "refActorCastId":[9931,9932,9933,9934,9935],
              "refActorComparisonType":4,
              "includeRotation":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "Octet_Dragon4",
            """
            {
              "Name":"NEXT",
              "Enabled":true,
              "type":3,
              "offY":47.14,
              "radius":4.0,
              "color":4294967040,
              "thicc":8.0,
              "refActorNPCID":2632,
              "refActorRequireCast":true,
              "refActorCastId":[9931,9932,9933,9934,9935],
              "refActorComparisonType":4,
              "includeRotation":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "Octet_Dragon5",
            """
            {
              "Name":"NEXT",
              "Enabled":true,
              "type":3,
              "offY":47.14,
              "radius":4.0,
              "color":4294967040,
              "thicc":8.0,
              "refActorNPCID":2631,
              "refActorRequireCast":true,
              "refActorCastId":[9931,9932,9933,9934,9935],
              "refActorComparisonType":4,
              "includeRotation":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "Octet_Bahamut",
            """
            {
              "Name":"CURRENT",
              "Enabled":true,
              "type":3,
              "offY":50.0,
              "radius":6.0,
              "color":4278255360,
              "thicc":8.0,
              "refActorNPCID":3210,
              "refActorRequireCast":true,
              "refActorCastId":[9953],
              "FillStep":1.0,
              "refActorComparisonType":4,
              "includeRotation":true,
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
            "Octet_Twintania",
            """
            {
              "Name":"NEXT",
              "Enabled":true,
              "type":3,
              "offY":50.0,
              "radius":4.0,
              "color":4294967040,
              "thicc":8.0,
              "refActorNPCID":1482,
              "refActorRequireCast":true,
              "refActorCastId":[9906],
              "FillStep":1.0,
              "refActorComparisonType":4,
              "includeRotation":true,
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

    public override void OnUpdate()
    {
        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        var bahamut = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Bahamut",
                    StringComparison.OrdinalIgnoreCase));

        var twin = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Twintania",
                    StringComparison.OrdinalIgnoreCase));

        if (nael == null ||
            bahamut == null ||
            twin == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active &&
            Environment.TickCount64 - activatedAt > 30000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }
}
