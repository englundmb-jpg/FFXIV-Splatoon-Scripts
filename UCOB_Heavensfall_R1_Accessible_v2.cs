using Splatoon.SplatoonScripting;
using System.Collections.Generic;

namespace MaggieScripts.Duties.Stormblood;

// Retired: marker logic moved to UCOB_Nael_Two_Markers.
public sealed class UCOB_Heavensfall_R1_Accessible_v2 : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(99, "Maggie");
}
