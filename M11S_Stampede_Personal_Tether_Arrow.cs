using System.Collections.Generic;
using Splatoon.SplatoonScripting;

namespace MaggieScripts.Duties.Dawntrail;

public class M11S_Stampede_Personal_Tether_Arrow : SplatoonScript
{
    public override Metadata Metadata => new(1, "NightmareXIV / Maggie accessibility");
    public override HashSet<uint>? ValidTerritories => [1325];

    public override void OnSetup()
    {
        Controller.RegisterLayoutFromCode("PersonalTether", """~Lv2~{"Name":"M11S Stampede Personal Tether Arrow","Group":"M11S (2)","ZoneLockH":[1325],"ConditionalAnd":true,"ElementsL":[{"Name":"","type":1,"offX":19.0,"offY":44.0,"radius":1.0,"Donut":0.49,"color":3355508527,"fillIntensity":0.5,"refActorDataID":19178,"refActorComparisonType":3,"includeRotation":true,"tether":true,"LimitDistance":true,"DistanceSourceX":125.0,"DistanceSourceY":100.0,"DistanceMax":0.5,"refActorTether":true,"refActorIsTetherLive":true,"refActorTetherConnectedWithPlayer":[],"Conditional":true,"Nodraw":true},{"Name":"","type":1,"offX":19.0,"offY":44.0,"radius":1.0,"Donut":0.49,"color":3355508527,"fillIntensity":0.5,"refActorDataID":19178,"refActorComparisonType":3,"includeRotation":true,"tether":true,"LimitDistance":true,"DistanceSourceX":100.0,"DistanceSourceY":75.0,"DistanceMax":0.5,"refActorTether":true,"refActorIsTetherLive":true,"refActorTetherConnectedWithPlayer":[],"Conditional":true,"Nodraw":true},{"Name":"","type":1,"offX":19.0,"offY":44.0,"radius":1.0,"Donut":0.49,"color":3355508527,"fillIntensity":0.5,"refActorDataID":19178,"refActorComparisonType":3,"includeRotation":true,"tether":true,"LimitDistance":true,"DistanceSourceX":75.0,"DistanceSourceY":100.0,"DistanceMax":0.5,"refActorTether":true,"refActorIsTetherLive":true,"refActorTetherConnectedWithPlayer":[],"Conditional":true,"Nodraw":true},{"Name":"","type":1,"offX":19.0,"offY":44.0,"radius":1.0,"Donut":0.49,"color":3355508527,"fillIntensity":0.5,"refActorDataID":19178,"refActorComparisonType":3,"includeRotation":true,"tether":true,"LimitDistance":true,"DistanceSourceX":100.0,"DistanceSourceY":125.0,"DistanceMax":0.5,"refActorTether":true,"refActorIsTetherLive":true,"refActorTetherConnectedWithPlayer":[],"Conditional":true,"Nodraw":true},{"Name":"Your tether destination","type":1,"offX":19.0,"offY":44.0,"radius":1.0,"Donut":0.49,"color":4278255360,"fillIntensity":0.25,"refActorDataID":19178,"refActorComparisonType":3,"includeRotation":true,"tether":true,"refActorTether":true,"refActorIsTetherLive":true,"refActorTetherConnectedWithPlayer":["<me>"],"thicc":7.0,"LineEndA":1},{"Name":"Stretching left","type":1,"Enabled":false,"offX":-19.0,"offY":44.0,"radius":1.0,"Donut":0.49,"color":3355508527,"fillIntensity":0.5,"refActorDataID":19178,"refActorComparisonType":3,"includeRotation":true,"tether":true,"refActorTether":true,"refActorIsTetherLive":true,"refActorTetherConnectedWithPlayer":["<me>"]}]}""");
    }
}
