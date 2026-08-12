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

public sealed class UCOB_Quickmarch_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        // VERIFIED Quickmarch dive geometry.
        // Twintania.
        Controller.RegisterElementFromCode(
            "Quickmarch_Twin",
            """
            {
              "Name":"CURRENT",
              "Enabled":true,
              "type":3,
              "offY":67.52,
              "radius":2.5,
              "color":4278255360,
              "thicc":8.0,
              "refActorNPCNameID":1482,
              "refActorRequireCast":true,
              "refActorCastId":[9906,27531],
              "refActorComparisonType":6,
              "includeHitbox":true,
              "includeOwnHitbox":true,
              "includeRotation":true,
              "onlyVisible":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        // Bahamut Prime.
        Controller.RegisterElementFromCode(
            "Quickmarch_Bahamut",
            """
            {
              "Name":"CURRENT",
              "Enabled":true,
              "type":3,
              "offY":67.52,
              "radius":2.5,
              "color":4278255360,
              "thicc":8.0,
              "refActorNPCNameID":3210,
              "refActorRequireCast":true,
              "refActorCastId":[3008,9953,23378,24676],
              "refActorComparisonType":6,
              "includeHitbox":true,
              "includeOwnHitbox":true,
              "includeRotation":true,
              "onlyVisible":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        // Nael.
        Controller.RegisterElementFromCode(
            "Quickmarch_Nael",
            """
            {
              "Name":"CURRENT",
              "Enabled":true,
              "type":3,
              "offY":67.52,
              "radius":2.5,
              "color":4278255360,
              "thicc":8.0,
              "refActorNPCNameID":2612,
              "refActorRequireCast":true,
              "refActorCastId":[9923],
              "refActorComparisonType":6,
              "includeHitbox":true,
              "includeOwnHitbox":true,
              "includeRotation":true,
              "onlyVisible":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );
    }

    public override void OnReset()
    {
    }
}
