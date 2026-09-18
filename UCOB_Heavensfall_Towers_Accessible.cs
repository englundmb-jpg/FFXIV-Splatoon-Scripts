using Splatoon.SplatoonScripting;
using System.Collections.Generic;

namespace MaggieScripts.Duties.Stormblood;

// Retired: marker logic moved to UCOB_Nael_Two_Markers.
public sealed class UCOB_Heavensfall_Towers_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(99, "Maggie");
}
