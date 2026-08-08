using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UWU_Ifrit_CastID_Logger : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [777];

    public override Metadata? Metadata =>
        new(1, "Maggie UWU Ifrit CastID Logger");

    public override void OnStartingCast(uint source, uint castId)
    {
        var obj = source.GetObject();
        if (obj == null)
            return;

        if (!obj.Name.TextValue.Contains("Ifrit"))
            return;

        PluginLog.Information(
            $"[IfritCastLog] source={obj.Name.TextValue} " +
            $"entityId={source} castId=0x{castId:X} ({castId})"
        );
    }
}
