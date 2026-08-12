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

public sealed class UCOB_Thunder_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Thunder_Personal",
            """
            {
              "Name":"THUNDER",
              "Enabled":true,
              "type":1,
              "radius":5.0,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "refActorType":1,
              "refActorRequireBuff":true,
              "refActorBuffId":[466],
              "refActorComparisonType":2,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"THUNDER",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.8
            }
            """
        );

        OnReset();
    }

    public override void OnReset()
    {
    }
}
