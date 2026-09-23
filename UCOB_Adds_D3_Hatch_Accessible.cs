using Splatoon.SplatoonScripting;
using System.Collections.Generic;

namespace MaggieScripts.Duties.Stormblood;

// Retired: Fixed ground positions and presence-only phase detection; no personal Hatch assignment.
public sealed class UCOB_Adds_D3_Hatch_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(100, "Maggie");
}
