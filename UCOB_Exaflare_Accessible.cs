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

public sealed class UCOB_Exaflare_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        // VERIFIED UCOB First Exaflare Safe Line.
        // Exact actor-relative geometry from the working preset.
        Controller.RegisterElementFromCode(
            "Exaflare_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":true,
              "type":3,
              "refY":-2.0,
              "offY":60.0,
              "radius":2.0,
              "color":4278255360,
              "thicc":8.0,
              "refActorNPCNameID":3210,
              "refActorRequireCast":true,
              "refActorCastId":[9968],
              "refActorUseCastTime":true,
              "refActorCastTimeMax":0.25,
              "FillStep":2.0,
              "refActorComparisonType":6,
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

        OnReset();
    }

    public override void OnReset()
    {
    }
}
