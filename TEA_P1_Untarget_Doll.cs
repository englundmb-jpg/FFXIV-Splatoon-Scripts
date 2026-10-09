// Adapted from PunishXIV/Splatoon; original authors retained in Metadata.
// NAUR review 2026-10-09. See TEA_NAUR_R1_README.md for coverage and validation.
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;
using System.Linq;

using ECommons.DalamudServices.Legacy;

namespace MaggieSplatoon.TEA;

public class TEA_P1_Untarget_Doll : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        [Raids.The_Epic_of_Alexander_Ultimate];

    public override Metadata Metadata =>
        new(101, "NightmareXIV; Maggie accessibility repair");

    public override void OnUpdate()
    {
        if(Svc.Targets.Target is IBattleNpc b
           && b.NameId.EqualsAny<uint>(3759, 9214)
           && b.MaxHp > 0
           && ((float)b.CurrentHp / (float)b.MaxHp) < 0.24f)
        {
            Svc.Targets.Target =
                Svc.Objects
                    .OfType<IBattleNpc>()
                    .FirstOrDefault(x =>
                        x.IsTargetable
                        && !x.IsDead
                        && x.NameId.EqualsAny<uint>(
                            3765,
                            9211,
                            9212));
        }
    }
}

