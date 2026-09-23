using Splatoon.SplatoonScripting;
using System.Collections.Generic;

namespace MaggieScripts.Duties.Stormblood;

// Retired: Incomplete port of preset: actor-relative preview expires after 0.25 seconds without frozen position.
public sealed class UCOB_Exaflare_Accessible_v2 : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(100, "Maggie");
}
