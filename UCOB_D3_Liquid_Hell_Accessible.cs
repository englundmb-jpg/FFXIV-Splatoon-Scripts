using Splatoon.SplatoonScripting;
using System.Collections.Generic;

namespace MaggieScripts.Duties.Stormblood;

// Retired: Presence-driven repeating markers can extend beyond the arena; no Liquid Hell target detection.
public sealed class UCOB_D3_Liquid_Hell_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(100, "Maggie");
}
