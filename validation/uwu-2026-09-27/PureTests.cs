using System; using System.Linq; using System.Collections.Generic; using System.Numerics;
class Test {
private static readonly Vector3 Center = new(100,0,100);
private static float Distance(Vector3 a,Vector3 b)=>Vector2.Distance(new(a.X,a.Z),new(b.X,b.Z));
    private static bool Solve(Vector3 garuda, Vector3 titan, float titanRotation,
        Vector3 ifrit, float ifritRotation, Vector3 ultima, out Vector3 first, out Vector3 next)
    {
        first = next = default;
        var a = new List<(int index, Vector3 point)>();
        var b = new List<(int index, Vector3 point)>();
        const float margin = 0.75f;
        for (var i = 0; i < 720; i++)
        {
            float angle = i * MathF.PI / 360;
            var p = Center + new Vector3(MathF.Sin(angle), 0, MathF.Cos(angle)) * 19;
            if (Distance(p, garuda) <= 20 + margin) continue;
            if (LineDistance(p, titan, titanRotation) > 3 + margin &&
                LineDistance(p, titan, titanRotation + MathF.PI / 4) > 3 + margin &&
                LineDistance(p, titan, titanRotation - MathF.PI / 4) > 3 + margin &&
                LineDistance(p, ifrit, ifritRotation) > 9 + margin) a.Add((i, p));
            if (Distance(p, ultima) > 14 + margin && MathF.Abs(p.X - 100) > 5 + margin &&
                MathF.Abs(p.Z - 100) > 5 + margin &&
                LineDistance(p, titan, titanRotation + MathF.PI / 8) > 3 + margin &&
                LineDistance(p, titan, titanRotation - MathF.PI / 8) > 3 + margin &&
                LineDistance(p, titan, titanRotation + MathF.PI / 2) > 3 + margin) b.Add((i, p));
        }
        if (a.Count == 0 || b.Count == 0) return false;
        float best = float.MaxValue;
        foreach (var x in a) foreach (var y in b)
        {
            int diff = Math.Abs(x.index - y.index);
            float score = Math.Min(diff, 720 - diff);
            if (score >= best) continue;
            best = score; first = x.point; next = y.point;
        }
        return true;
    }
    private static float LineDistance(Vector3 p, Vector3 origin, float rotation) =>
        MathF.Abs((p.X - origin.X) * MathF.Cos(rotation) - (p.Z - origin.Z) * MathF.Sin(rotation));

static Vector3 Rotate(Vector3 p, float r) {var x=p.X-100;var z=p.Z-100;return new(100+x*MathF.Cos(r)+z*MathF.Sin(r),0,100-x*MathF.Sin(r)+z*MathF.Cos(r));}
static void Check(bool ok,string message) {if(!ok)throw new Exception(message);}
private readonly Dictionary<uint,Vector3> nailPositions = new();
private readonly List<uint> deathOrder = new();
    private bool TryNailOrder(out int first, out int rotation)
    {
        first = rotation = 0;
        if (deathOrder.Count != 4 || deathOrder.Any(x => !nailPositions.ContainsKey(x))) return false;
        var directions = deathOrder.Select(x => Direction(nailPositions[x])).ToArray();
        first = directions[0];
        for (int i = 1; i < directions.Length; i++)
        {
            var delta = (directions[i] - directions[i - 1] + 8) % 4;
            int r = delta == 1 ? 1 : delta == 3 ? -1 : 0;
            if (r == 0 || (rotation != 0 && r != rotation)) return false;
            rotation = r;
        }
        return true;
    }
    private static int Direction(Vector3 p) =>
        ((int)MathF.Round(MathF.Atan2(p.X - 100, 100 - p.Z) / (MathF.PI / 4)) + 8) % 8;
    // The 19y dodge circle stays inside the 20y arena and outside the dash edges.
    private static Vector3 Point(int direction)
    {
        var angle = direction * MathF.PI / 4;
        return Center + new Vector3(MathF.Sin(angle), 0, -MathF.Cos(angle)) * 19;
    }
static void Main() {
int patterns=0;
foreach (int first in Enumerable.Range(0,8)) foreach(int rotation in new[]{-1,1}) {
 var h=new Test();
 for(uint i=0;i<4;i++) {h.deathOrder.Add(i+1);h.nailPositions[i+1]=Point((first+(int)i*rotation+16)%8);}
 Check(h.TryNailOrder(out var actualFirst,out var actualRotation) && actualFirst==first && actualRotation==rotation,"Nail rotation mismatch");
 for(int wokenDash=1;wokenDash<=4;wokenDash++) {
  int start=(first-rotation+8)%8;
  int end=(start+rotation*(wokenDash%2==1?1:2)+16)%8;
  int woken=(start+wokenDash*rotation+16)%8;
  Check(LineDistance(Point(end),Center,woken*MathF.PI/4+MathF.PI/4)>5,"Endpoint in Awoken cross");
  Check(LineDistance(Point(end),Center,woken*MathF.PI/4-MathF.PI/4)>5,"Endpoint in Awoken cross");
  patterns++;
 }
}
var invalid=new Test();
foreach(var pair in new[]{(1u,0),(2u,2),(3u,1),(4u,3)}) {invalid.deathOrder.Add(pair.Item1);invalid.nailPositions[pair.Item1]=Point(pair.Item2);}
Check(!invalid.TryNailOrder(out _,out _),"Invalid nail order accepted");
Console.WriteLine($"PASS: {patterns} Ifrit nail-order/Awoken combinations and invalid-order rejection.");
for(int i=0;i<4;i++) {
 float r=i*MathF.PI/2;
 var g=Rotate(new(103,0,103),r);var t=Rotate(new(117,0,97),r);var f=Rotate(new(113.7f,0,113.7f),r);var u=Rotate(new(88,0,112),r);
 float tr=-MathF.PI/2+r,fr=-3*MathF.PI/4+r;
 Check(Solve(g,t,tr,f,fr,u,out var a,out var b),"No route for rotated public ACT sample");
 Check(MathF.Abs(Distance(a,Center)-19)<.001f && MathF.Abs(Distance(b,Center)-19)<.001f,"Arena radius");
 Check(Distance(a,g)>20.74 && Distance(b,g)>20.74 && Distance(b,u)>14.74,"Circle overlap");
 foreach(var d in new[]{0f,MathF.PI/4,-MathF.PI/4}) Check(LineDistance(a,t,tr+d)>3.74,"First Landslide overlap");
 foreach(var d in new[]{MathF.PI/8,-MathF.PI/8,MathF.PI/2}) Check(LineDistance(b,t,tr+d)>3.74,"Second Landslide overlap");
 Check(LineDistance(a,f,fr)>9.74 && MathF.Abs(b.X-100)>5.74 && MathF.Abs(b.Z-100)>5.74,"Ifrit overlap");
 Check(Distance(a,b)<12,"Movement exceeds normal-speed two-second window");
 Console.WriteLine($"ACT sample rotation {i}: first {a}, next {b}, move {Distance(a,b):F2}y");
}
Check(!Solve(Center,Center,0,Center,0,Center,out _,out _),"Invalid geometry should fail closed");
Console.WriteLine("PASS: four rotations, geometry exclusions, travel-distance bound, invalid-geometry rejection.");
}}
