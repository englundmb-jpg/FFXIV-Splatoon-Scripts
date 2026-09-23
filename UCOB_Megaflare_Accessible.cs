using Splatoon.SplatoonScripting;
using System.Collections.Generic;

namespace MaggieScripts.Duties.Stormblood;

// Retired: 9953 is Megaflare Dive; a fixed center circle is not a calculated safe spot.
public sealed class UCOB_Megaflare_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(100, "Maggie");
}
