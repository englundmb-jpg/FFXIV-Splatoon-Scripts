// FRU physical ranged, R1/G1. Companion to FRU_P1_R1_Cyclonic.
// Strategy and diagrams: https://ultistrats.com/guides/fru/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon;
using Splatoon.SplatoonScripting;

namespace MaggieSplatoon.FRU;

public sealed class FRU_R1_All_Phases : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 1238 };
    public override Metadata Metadata => new(3, "Maggie accessibility");
    private enum Part { Idle, Sky, Faith, Dust, Mirrors, Rampant, Relativity, Apoc, Darklit, Time, Blades, Wings, Polarizing }
    private Part part;
    private static readonly Vector2 Center = new(100,100);
    private static readonly string[] Compass = {"north","northeast","east","southeast","south","southwest","west","northwest"};
    private readonly HashSet<int> axes = new(), yellow = new();
    private readonly HashSet<uint> targets = new(), stones = new(), chainPlayers = new();
    private readonly HashSet<(uint,uint)> edges = new();
    private readonly Dictionary<uint,(Vector2 pos,int size)> towers = new();
    private readonly Dictionary<string,long> spoken = new();
    private readonly Dictionary<uint,Vector2> skyTethers = new();
    private readonly Dictionary<uint,Vector2> redMirrors = new();
    private HttpClient? voice;
    private volatile string connection = "";
    private string caption = "", cleanse = "";
    private long markUntil, textUntil, statusStart, nextPoll;
    private bool skyFire, clockwiseCardinals, iceKnown, blueKnown, dustSaid, solved, waterSaid, redQueued;
    private int smallSide, polarHit, fireOrder, ctDir;
    private readonly HashSet<uint> puddles = new();
    private readonly Dictionary<uint,Vector2> tidalOrigins = new();
    private bool flexWater, rewindShown, rewindPlaced;
    private int dpsSafe = -1, apocPulses;
    private long lastApocPulse;
    private Vector2 blue, firstTower;
    private bool firstTowerKnown;
    private int glory, fireWave, apocAxis = -1, apocSpin, exaWave;
    private long fireAt, exaAt;
    private Vector2? exaFirst;
    private Vector2[]? lines;
    private readonly HashSet<int> purple = new();
    private bool purpleCalled;
    private int urClock = -1;
    private static long Now => Environment.TickCount64;

    public override void OnSetup() => Controller.RegisterElement("R1",new Element(0)
    { Enabled=false,radius=1f,thicc=5f,tether=true,overlayFScale=1.4f });

    public override void OnEnable() => voice=CreateVoiceClient();
    private static HttpClient CreateVoiceClient() => new HttpClient(new HttpClientHandler{UseProxy=false})
    { Timeout=TimeSpan.FromSeconds(3) };
    private static Vector2? Position(uint id)
    {
        var a=Svc.Objects.SearchById(id);
        return a==null ? null : new Vector2(a.Position.X,a.Position.Z);
    }
    private static int Dir(Vector2 p) => ((int)Math.Round(Math.Atan2(p.X-100,100-p.Y)*4/Math.PI)+8)%8;
    private static Vector2 Spot(int d,float radius)
    {
        double a=d*Math.PI/4;
        return Center+new Vector2((float)Math.Sin(a),-(float)Math.Cos(a))*radius;
    }
    private void Ring(Vector2 pos,string label,bool current,int ms)
    {
        if (!Controller.TryGetElementByName("R1",out var e)) return;
        e.refX=pos.X; e.refY=pos.Y; e.overlayText=label; e.color=current?0xFF00FF00:0xFFFFFF00;
        e.tether=current; e.Enabled=true; markUntil=Now+ms;
    }
    private void Hide()
    {
        if(Controller.TryGetElementByName("R1",out var e)) e.Enabled=false;
        markUntil=0;
    }
    private void Call(string text)
    {
        if(spoken.TryGetValue(text,out var at) && Now-at<1700) return;
        spoken[text]=Now; caption=text; textUntil=Now+6500;
        if(voice!=null) _=Send(voice,text);
    }
    private async Task Send(HttpClient client,string text)
    {
        try {
            using var body=new StringContent("FRU_R1_ALL|"+text,Encoding.UTF8,"text/plain");
            using var res=await client.PostAsync("http://localhost:51423/",body).ConfigureAwait(false);
            connection=res.IsSuccessStatusCode?"Triggernometry received the message.":"Check Triggernometry endpoint";
        } catch(HttpRequestException){connection="Check Triggernometry endpoint";}
          catch(OperationCanceledException){connection="Voice request timed out";}
    }
    private void Start(Part p)
    {
        part=p; solved=waterSaid=redQueued=false; statusStart=Now;
        Hide(); caption=""; nextPoll=0;
    }
    public override void OnStartingCast(uint src,uint id)
    {
        switch(id)
        {
            case 40154: case 40155:
                Start(Part.Sky); axes.Clear(); skyTethers.Clear(); smallSide=0; skyFire=id==40154;
                Call(skyFire?"Northwest clone. Group one stack next.":"Northwest clone. Spread next."); break;
            case 40150: case 40151:
                if(part==Part.Sky) Call(id==40150?"Lightning circles are safe.":"Fire circles are safe."); break;
            case 40152:
                if(part==Part.Sky && Position(src) is {} orb && Math.Abs(orb.X-100)>1)
                { smallSide=Math.Sign(orb.X-100); SolveSkyTethers(); } break;
            case 40165: if(part==Part.Sky) Call("Tethers. Short south, long north."); break;
            case 40170: // Burnished Glory
                if(++glory==1) {Start(Part.Faith);targets.Clear();Call("Raidwide. Next, conga far west.");}
                break;
            case 40137: case 40140:
                if(part!=Part.Faith) {Start(Part.Faith); targets.Clear(); Call("Fall of Faith. Start far west.");} break;
            case 40129: Call("Fire. Line up for knockback into your tower."); break;
            case 40133: Call("Lightning. Avoid the wide line into your tower."); break;
            case 40135: case 40131: case 40125: case 40122: case 40126: case 40123: case 40124: case 40127:
                if(Position(src) is {} tower)
                {
                    towers[src]=(tower,id switch {40135 or 40131=>1,40125 or 40122=>2,40126 or 40123=>3,_=>4});
                    if(towers.Count==3 && towers.Values.Sum(t=>t.size)==6)
                    {
                        var sorted=towers.Values.OrderBy(t=>t.pos.Y).ToArray();
                        int index=Array.FindLastIndex(sorted,t=>t.size>1);
                        Call(new[]{"North tower.","Middle tower.","South tower."}[index]);
                        Ring(sorted[index].pos,"YOUR TOWER",false,10500);
                    }
                } break;
            case 40197: Start(Part.Dust); stones.Clear(); iceKnown=dustSaid=false; Call("Diamond Dust. Find your color buddy."); break;
            case 40198:
                if(part==Part.Dust && !iceKnown && Position(src) is {} ice)
                {iceKnown=true; clockwiseCardinals=Dir(ice)%2==0; DustAssignment();} break;
            case 40202: Call("Axe. Out."); break;
            case 40203: if(part==Part.Dust) Call("Scythe. In."); break;
            case 40208: Call("Rotate with group one. Four puddles, then look away."); break;
            case 40193: Call("Behind Shiva, then front."); break;
            case 40194: Call("In front of Shiva, then behind."); break;
            case 40210: Call("Line stack."); break;
            case 40179: Start(Part.Mirrors); blueKnown=false; redMirrors.Clear(); Call("Blue mirror. Group one left facing the wall."); break;
            case 40205:
                if(part==Part.Mirrors && Position(src) is {} red)
                {redMirrors[src]=red; SolveRed();} break;
            case 40220: Call("Pair up."); break;
            case 40221: Call("Spread."); break;
            case 40212: Start(Part.Rampant); puddles.Clear(); Call("Light Rampant. DPS line up south."); break;
            case 40189: Call("Clock spots. Bait your cone."); break;
            case 40224: Start(Part.Idle); Call("Intermission next. Ranged inside, intercardinal."); break;
            case 40259: Call("Attack the light crystal clockwise from you."); break;
            case 40266: Start(Part.Relativity); yellow.Clear(); fireOrder=fireWave=0;fireAt=0;urClock=-1; Call("Ultimate Relativity. Orient to the yellow Y."); break;
            case 40286: Call("Stack middle."); break;
            case 40270: Start(Part.Apoc);apocAxis=-1;apocSpin=0; dpsSafe=-1; apocPulses=0; lastApocPulse=0; flexWater=false; Call("Dark Water. Start east."); break;
            case 40296: Call("Apocalypse. Find the safe side for your group."); break;
            case 40273: Call(part==Part.Apoc && dpsSafe>=0?"Ranged spread at wall. "+Compass[dpsSafe]+" side.":"Spread for eruptions."); break;
            case 40288: Call(part==Part.Time?"Far wall. Group one left.":"Spread for Spirit Taker."); break;
            case 40181: Call(part==Part.Apoc && waterSaid?(flexWater?"Tank bait. Return to support group for water.":"Tank bait. Return to DPS group for water."):"Leave the far bait to the tank. Prepare knockback."); break;
            case 40246: Start(Part.Idle); Call("Center. Move southwest when wings appear."); break;
            case 40239: Start(Part.Darklit); edges.Clear(); chainPlayers.Clear(); Call("Darklit. Ranged southwest."); break;
            case 40227: Call("Right of Shiva. Stack with your water group."); break;
            case 40228: Call("Left of Shiva. Stack with your water group."); break;
            case 40247: case 40302: Call("Light party stack."); break;
            case 40249: case 40304: Call("Full party stack."); break;
            case 40298: Start(Part.Time); cleanse=""; ctDir=-1;purple.Clear();purpleCalled=false; tidalOrigins.Clear(); rewindShown=rewindPlaced=false; Call("Crystallize Time. Early pop."); break;
            case 40251: if(part==Part.Time) Call("Watch the exa waves. Prepare your rewind."); break;
            case 40252:
                if(part==Part.Time && Position(src) is {} origin) tidalOrigins[src]=origin;
                break;
            case 40306: Start(Part.Blades); lines=null;exaFirst=null;exaWave=0;exaAt=0; Call("Fulgent Blade. Face the three small triangles."); break;
            case 40307: case 40118: if(part==Part.Blades) exaFirst??=Position(src);break;
            case 40310: Call("Face Pandora. Stack left after your last dodge."); break;
            case 40319: Start(Part.Wings); firstTowerKnown=false; Call("First tower is relative south. Your tower is left with melee one."); break;
            case 40233: case 40313: if(part==Part.Wings) Call("Wait for the first tank cleave."); break;
            case 40316: Start(Part.Polarizing); polarHit=0; Call("Group one. Third hit is yours."); break;
            case 40234:
                if(part==Part.Polarizing && ++polarHit==2) Call("Front of your line. Your hit.");
                else if(part==Part.Polarizing && polarHit==3) Call("Opposite line. Behind the healer."); break;
            case 40326: Call("Pandora's Box. Party mitigation."); break;
            case 40327: Call("Final enrage. Finish Pandora."); break;
        }
    }

    public override void OnActorControl(uint id,uint cmd,uint p1,uint p2,uint p3,uint p4,uint p5,uint p6,uint p7,uint p8,ulong target,byte replaying)
    {
        if(part==Part.Sky && cmd==63 && p1==4 && !solved && Position(id) is {} pos &&
            Svc.Objects.SearchById(id)?.BaseId==0x459C && Vector2.Distance(pos,Center)>12)
        {
            axes.Add(Dir(pos)%4);
            if(axes.Count==3)
            {
                int d=new[]{0,7,6,5}.Single(i=>!axes.Contains(i%4)); solved=true;
                Call(Compass[d]+(skyFire?". Group one stack.":". Group one spread."));
                Ring(Spot(d,16),"SAFE AXIS",false,6200);
            }
        }
        if(cmd!=502) return;
        if(part==Part.Dust && p1==345) {stones.Add(id);DustAssignment();}
        if(part==Part.Rampant && p1==375 && puddles.Add(id) && puddles.Count==2)
            SolveRampant();
    }
    private void SolveRampant()
    {
        if(BasePlayer==null) return;
        var party=Svc.Party.ToArray();
        if(party.Length!=8) return;
        // Snapshot the two prepositioned rows at the second puddle marker.
        var positions=party.ToDictionary(m=>m.EntityId,m=>Position(m.EntityId));
        if(positions.Values.Any(p=>!p.HasValue)) return;
        var north=party.Where(m=>m.ClassJob.Value.Role is 1 or 4)
            .OrderBy(m=>positions[m.EntityId]!.Value.X).Select(m=>m.EntityId).ToList();
        var south=party.Where(m=>m.ClassJob.Value.Role is 2 or 3)
            .OrderBy(m=>positions[m.EntityId]!.Value.X).Select(m=>m.EntityId).ToList();
        if(north.Count!=4 || south.Count!=4 || !puddles.All(positions.ContainsKey)) return;
        uint me=BasePlayer.EntityId;
        if(!south.Contains(me)) return;
        if(puddles.Contains(me))
        {
            uint other=puddles.Single(id=>id!=me);
            int column=south.IndexOf(me);
            int otherColumn=north.Contains(other)?north.IndexOf(other):south.IndexOf(other);
            bool west=column<=otherColumn;
            Call(west?"Your puddles. West.":"Your puddles. East.");
            Ring(Spot(west?6:2,8),"FIRST PUDDLE",true,4500);
            return;
        }
        north.RemoveAll(puddles.Contains);
        south.RemoveAll(puddles.Contains);
        if(north.Count==2) {north.Insert(0,south[0]);south.RemoveAt(0);}
        else if(north.Count==4) {south.Add(north[3]);north.RemoveAt(3);}
        // North row (west to east) crosses to NE,S,NW. South crosses to SW,N,SE.
        int index=north.IndexOf(me);
        int degrees=index>=0?new[]{60,180,300}[index]:new[]{240,0,120}[south.IndexOf(me)];
        double angle=degrees*Math.PI/180;
        var point=Center+16*new Vector2((float)Math.Sin(angle),-(float)Math.Cos(angle));
        Call(Compass[Dir(point)]+" tower."); Ring(point,"YOUR FIRST TOWER",true,10500);
    }
    private void DustAssignment()
    {
        if(dustSaid || !iceKnown || stones.Count!=4 || BasePlayer==null) return;
        bool mine=stones.Contains(BasePlayer.EntityId);
        Call(mine ? (clockwiseCardinals?"Drop ice on intercardinal.":"Drop ice on cardinal.") :
                    (clockwiseCardinals?"Bait cone on cardinal.":"Bait cone on intercardinal."));
        dustSaid=true;
    }
    private void SolveSkyTethers()
    {
        if(smallSide==0 || skyTethers.Count!=2 || BasePlayer==null || redQueued) return;
        bool north=skyTethers.TryGetValue(BasePlayer.EntityId,out var p) && Math.Sign(p.X-100)!=smallSide;
        Call(north?"Long tether. North stack.":"South stack."); redQueued=true;
    }
    private void SolveRed()
    {
        if(!blueKnown || redMirrors.Count!=2 || redQueued) return;
        int from=Dir(blue);
        var mirror=redMirrors.Values.OrderBy(p=>Math.Min((Dir(p)-from+8)%8,(from-Dir(p)+8)%8))
            .ThenBy(p=>(Dir(p)-from+8)%8).First();
        Call("Red mirror "+Compass[Dir(mirror)]+".");
        Ring(mirror,"RED MIRROR",false,9000); redQueued=true;
    }
    public override void OnMapEffect(uint index,ushort data1,ushort data2)
    {
        if(data1!=1 || data2!=2) return;
        if(part==Part.Mirrors && index>=1 && index<=8)
        {
            blue=Spot((int)index-1,20); blueKnown=true;
            Call("Blue mirror "+Compass[Dir(blue)]+"."); Ring(blue,"BLUE MIRROR",false,18500); SolveRed();
        }
        if(part==Part.Wings && index is >=51 and <=53 && !firstTowerKnown)
        {
            firstTowerKnown=true;
            firstTower=Center+(index switch {51=>new Vector2(-6.06218f,-3.5f),52=>new Vector2(6.06218f,-3.5f),_=>new Vector2(0,7)});
            Ring(LeftTower(),"YOUR LATER TOWER",false,17000);
        }
    }
    private Vector2 LeftTower()
    {
        var v=firstTower-Center;
        return Center+new Vector2(-0.5f*v.X-0.8660254f*v.Y,0.8660254f*v.X-0.5f*v.Y);
    }
    public override void OnTetherCreate(uint src,uint dst,uint d2,uint type,uint d5)
    {
        if(part==Part.Sky && type==249 && Position(src) is {} p && Svc.Party.Any(m=>m.EntityId==dst))
        {skyTethers[dst]=p;SolveSkyTethers();}
        if(part==Part.Faith && type is 249 or 287 && Svc.Party.Any(m=>m.EntityId==dst) && targets.Add(dst) && BasePlayer!=null)
        {
            if(dst==BasePlayer.EntityId)
            {
                int number=Svc.Objects.SearchById(src)?.BaseId==0x459B?1:
                    Position(src) is {} clone ? Dir(clone) switch {7=>2,0=>3,1=>4,_=>0}:0;
                if(number!=0) Call("Your tether "+number+". "+(number%2==1?"West.":"East."));
                else Call("Your tether. Check order.");
            }
            else if(targets.Count==4 && !targets.Contains(BasePlayer.EntityId)) Call("West filler. Bait north for lightning.");
        }
        if(part==Part.Relativity && type==134 && Position(src) is {} glass) yellow.Add(Dir(glass));
        if(part==Part.Time && type==133 && Position(src) is {} slow) purple.Add(Dir(slow));
        if(part==Part.Darklit && type==110)
        {
            edges.Add((Math.Min(src,dst),Math.Max(src,dst))); chainPlayers.Add(src);chainPlayers.Add(dst);
            if(edges.Count==4 && chainPlayers.Count==4 && BasePlayer!=null)
                DarklitAssignment();
        }
    }
    private void DarklitAssignment()
    {
        var me=BasePlayer;
        if(me==null) return;
        var party=Svc.Party.ToArray();
        if(party.Length!=8) return;
        if(!chainPlayers.Contains(me.EntityId))
        {
            var other=party.Where(m=>m.EntityId!=me.EntityId && !chainPlayers.Contains(m.EntityId)
                && m.ClassJob.Value.Role is 2 or 3).ToArray();
            if(other.Length==1) Call(other[0].ClassJob.Value.Role==3?"No chain. Flex east for cone.":"No chain. Bait west cone.");
            else Call("No chain. Bait a close cone.");
            return;
        }
        var links=party.Where(m=>edges.Contains((Math.Min(m.EntityId,me.EntityId),Math.Max(m.EntityId,me.EntityId)))).ToArray();
        if(links.Length!=2) return;
        bool tank=links.Any(m=>m.ClassJob.Value.Role==1), healer=links.Any(m=>m.ClassJob.Value.Role==4);
        Call(tank&&!healer?"Swap with tank. North tower.":"South tower. Keep bowtie.");
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if(set.Action is not {} a) return;
        switch(a.RowId)
        {
            case 40276:
                if(part==Part.Relativity && Now-fireAt>2000)
                {
                    fireAt=Now;fireWave++;
                    if(fireWave==1 && (fireOrder==3||fireOrder==4)) Call("Bait first beam.");
                    else if(fireWave==1 && BasePlayer!=null)
                    {
                        if(BasePlayer.StatusList.Any(v=>v.StatusId==2460)) Call("Place rewind out.");
                        else if(BasePlayer.StatusList.Any(v=>v.StatusId==2461)) Call("Place rewind tight in middle.");
                    }
                    if(fireWave==2 && fireOrder==1) Call("Bait second beam.");
                    if(fireWave==2 && (fireOrder==3||fireOrder==4)) Call("Place eye rewind in.");
                    if(fireWave==3) Call(fireOrder==2?"Bait last beam. Face out.":"Face out for rewind.");
                } break;
            case 40307: case 40118: case 40308: case 40309:
                if(part==Part.Blades && Now-exaAt>1000)
                { exaAt=Now;exaWave++; DrawBlade(); }
                break;
            case 40157: if(part==Part.Sky) Hide(); break;
            case 40135: case 40131: case 40125: case 40122: case 40126: case 40123: case 40124: case 40127: Hide(); towers.Clear(); break;
            case 40213:
                if(part==Part.Rampant) Hide();
                break;
            case 40297:
                if(part==Part.Apoc && waterSaid && Now-lastApocPulse>1000)
                {
                    lastApocPulse=Now;
                    if(++apocPulses==2) Call(flexWater?"In. Cross for support water.":"In. DPS water.");
                }
                break;
            case 40281:
                if(part==Part.Time && !rewindShown && tidalOrigins.Count==2)
                {
                    var sum=tidalOrigins.Values.Aggregate(Vector2.Zero,(v,p)=>v+p-Center);
                    if(Math.Abs(sum.X)<5 || Math.Abs(sum.Y)<5) break;
                    var corner=Center+sum/2;
                    var outward=Vector2.Normalize(sum);
                    var left=new Vector2(outward.Y,-outward.X);
                    var edge=Vector2.Normalize(outward+left);
                    var offset=edge+2*new Vector2(edge.Y,-edge.X);
                    Ring(corner+offset,"G1 REWIND",true,8000);
                    Call(Compass[Dir(corner)]+" corner. Rewind left facing wall.");
                    rewindShown=true;
                }
                break;
            case 40219:
                if(part==Part.Rampant && !waterSaid && BasePlayer!=null)
                {
                    var light=BasePlayer.StatusList.FirstOrDefault(s=>s.StatusId==2257);
                    if(light!=null) {waterSaid=true;Call(light.Param==2?"Take middle tower.":"Stay out of middle tower.");}
                } break;
            case 40258: Call("Dodge your crystal bait."); break;
            case 40277:
                if(part==Part.Time && cleanse.Length>0 && !waterSaid)
                {waterSaid=true;Call("Next cleanse "+cleanse+".");Ring(Spot(ctDir,15),"CLEANSE AREA",false,12000);} break;
            case 40314: case 40315:
                if(part==Part.Wings && firstTowerKnown && !solved)
                {solved=true;Call("Left tower with melee one.");Ring(LeftTower(),"YOUR TOWER",true,7500);} break;
        }
    }
    private void ReadRelativity()
    {
        var me=BasePlayer;
        if(me==null) return;
        if(fireOrder==0 && Now-statusStart<15500)
        {
            var status=me.StatusList.FirstOrDefault(s=>s.StatusId is 2455 or 2462);
            if(status!=null)
            {
                fireOrder=status.StatusId==2462?4:status.RemainingTime<13?1:status.RemainingTime<24?2:3;
                Call(new[]{"","Short fire. Spread first.","Medium fire. Spread second.","Long fire. Spread third.","Ice. Stack middle for ice."}[fireOrder]);
            }
        }
        if(solved || fireOrder==0 || yellow.Count!=3) return;
        int north=Enumerable.Range(0,8).FirstOrDefault(i=>yellow.Contains((i+7)%8)&&yellow.Contains((i+1)%8)&&yellow.Contains((i+4)%8),-1);
        if(north<0) return;
        int offset=fireOrder==2?2:4;
        if(fireOrder==1)
        {
            var other=Svc.Party.Where(m=>m.EntityId!=me.EntityId && m.GameObject is IBattleChara c && c.ClassJob.Value.Role>=2 &&
                c.ClassJob.Value.Role!=4 && c.StatusList.Any(s=>s.StatusId==2455 && s.RemainingTime<13 && s.RemainingTime>0)).ToArray();
            if(other.Length!=1) return;
            offset=other[0].ClassJob.Value.Role==2?5:3;
        }
        int dir=(north+offset)%8; urClock=dir; solved=true;
        Call("Your clock "+Compass[dir]+".");Ring(Spot(dir,11),"YOUR CLOCK",false,10000);
    }
    private void ReadTime()
    {
        if(solved || BasePlayer==null) return;
        var s=BasePlayer.StatusList.Select(v=>v.StatusId).ToHashSet();
        if(s.Contains(3264))
        {
            if(s.Contains(2462)) {cleanse="southwest";ctDir=5;}
            else if(s.Contains(2454)){cleanse="east";ctDir=2;}
            else if(s.Contains(2461)){cleanse="southeast";ctDir=3;}
            else if(s.Contains(2460)){cleanse="west";ctDir=6;}
            else return;
            Call(ctDir==6?"Blue eruption. North purple glass.":"Blue. South purple glass.");
        }
        else if(s.Contains(3263) && (s.Contains(2462)||s.Contains(2463)))
        {
            uint debuff=s.Contains(2462)?2462u:2463u;
            var other=Svc.Party.Where(m=>m.EntityId!=BasePlayer.EntityId && m.GameObject is IBattleChara c &&
                c.StatusList.Any(v=>v.StatusId==3263) && c.StatusList.Any(v=>v.StatusId==debuff)).ToArray();
            if(other.Length!=1) return;
            // Supports west, DPS east. For double DPS, group one (R1) adjusts west.

            if(other[0].ClassJob.Value.Role==3 || other[0].ClassJob.Value.Role==2) {
                // If both DPS are G1, the diagram's precedence must break the tie.
                Call(debuff==2462?"Red ice. Check east west flex.":"Red aero. Check southern flex.");
            } else {
                int dir=debuff==2462?2:3;
                Call(debuff==2462?"Red ice. East.":"Red aero. Southeast.");Ring(Spot(dir,13),"START AREA",false,8500);
            }
        }
        else return;
        solved=true;
    }
    public override void OnUpdate()
    {
        if(markUntil!=0 && Now>=markUntil) Hide();
        if(BasePlayer==null || BasePlayer.CurrentHp==0) {if(part!=Part.Idle || markUntil!=0) OnReset();return;}
        if(Now<textUntil) Controller.DisplayAttentionWindowLine(new Vector4(0,1,0,1),caption);
        if(Now<nextPoll) return;
        nextPoll=Now+150;
        if(part==Part.Relativity) ReadRelativity();
        if(part==Part.Time)
        {
            ReadTime();
            if(rewindShown && !rewindPlaced && BasePlayer.StatusList.Any(s=>s.StatusId==2452))
            {rewindPlaced=true;Hide();Call("Ranged spread. Far wall, group one left.");}
            if(!purpleCalled && cleanse.Length!=0 && purple.Count==2)
            {
                var d=purple.Where(x=>ctDir==6?x is 1 or 7:x is 3 or 5).ToArray();
                if(d.Length==1) {purpleCalled=true;Call("Purple glass "+Compass[d[0]]+".");Ring(Spot(d[0],12),"PURPLE GLASS AREA",false,6500);}
            }
        }
        if(part==Part.Blades && lines==null && exaFirst!=null && exaWave==0) CollectBlades();
        if(part==Part.Apoc && !solved) ReadApocalypse();
        if(part==Part.Relativity && urClock>=0 && !waterSaid && BasePlayer!=null)
        {
            var fire=BasePlayer.StatusList.FirstOrDefault(v=>v.StatusId==2455 && v.RemainingTime>0 && v.RemainingTime<4);
            if(fire!=null)
            {
                waterSaid=true;Call("Fire out "+Compass[urClock]+".");
                Ring(Spot(urClock,16),"YOUR FIRE",true,3700);
            }
        }
        if(part==Part.Apoc && !waterSaid && Now-statusStart<12000)
        {
            var members=Svc.Party.ToArray();
            if(members.Length!=8 || members.Any(m=>m.GameObject is not IBattleChara)) return;
            var timers=members.ToDictionary(m=>m.EntityId,m=>
            {
                var status=((IBattleChara)m.GameObject!).StatusList.FirstOrDefault(v=>v.StatusId==2461);
                return status==null?0:status.RemainingTime<15?1:status.RemainingTime<34?2:3;
            });
            if(timers.Values.GroupBy(order=>order).Count(g=>g.Count()==2)!=4) return;
            int mine=timers[BasePlayer.EntityId];
            // Only R2 has lower flex priority than R1 among the DPS.
            flexWater=members.Any(m=>m.EntityId!=BasePlayer.EntityId && m.ClassJob.Value.Role==3 && timers[m.EntityId]==mine);
            waterSaid=true;
            Call((flexWater?"Flex west. Support water group. ":"Stay east. DPS water group. ")+
                new[]{"No water.","Your water first.","Your water second.","Your water third."}[mine]);
        }
    }
    private void ReadApocalypse()
    {
        foreach(var a in Svc.Objects.Where(a=>a.BaseId==0x1EB0FF))
        {
            var pos=new Vector2(a.Position.X,a.Position.Z);
            int facing=Dir(Center+new Vector2(MathF.Sin(a.Rotation),MathF.Cos(a.Rotation))*10);
            if(Vector2.Distance(pos,Center)<1) apocAxis=facing;
            else
            {
                int delta=(facing-Dir(pos)+8)%8;
                if(delta is >0 and <4) apocSpin=1;
                if(delta is >4 and <8) apocSpin=-1;
            }
        }
        if(apocAxis<0 || apocSpin==0) return;
        solved=true;
        int d=(apocAxis-apocSpin+8)%8;
        dpsSafe=d is >=1 and <=4?d:(d+4)%8;
        Call("DPS spread side "+Compass[dpsSafe]+".");
    }
    private static float Cross(Vector2 a,Vector2 b)=>a.X*b.Y-a.Y*b.X;
    private void CollectBlades()
    {
        var set=Svc.Objects.Where(a=>a.BaseId==0x1EBBF7).Select(a=>new Vector2(a.Position.X,a.Position.Z)).ToArray();
        if(set.Length!=6 || set.Any(p=>Vector2.Distance(p,Center)<1) || exaFirst==null) return;
        Vector2 axis=Vector2.Zero;
        foreach(var p in set) axis-=Vector2.Normalize(p-Center);
        if(axis.LengthSquared()<0.01f) return;
        var ordered=set.OrderBy(p=>Cross(Vector2.Normalize(p-Center),axis)).ToArray();
        int first=Array.FindIndex(ordered,p=>Vector2.Distance(p,exaFirst.Value)<1);
        if(first>=4) Array.Reverse(ordered);
        else if(first<0 || first>1) return;
        lines=ordered; DrawBlade();
    }
    private void DrawBlade()
    {
        if(lines==null || exaWave>6) {Hide();return;}
        int pair=exaWave<3?0:exaWave<5?2:4;
        float distance=exaWave is 2 or 4 or 6?9:11;
        Vector2 a=lines[pair],b=lines[pair+1];
        Vector2 na=Vector2.Normalize(a-Center),nb=Vector2.Normalize(b-Center);
        Vector2 va=new(-na.Y,na.X),vb=new(-nb.Y,nb.X);
        float det=Cross(va,vb);
        if(Math.Abs(det)<0.05f) {Hide();return;}
        a-=distance*na;b-=distance*nb;
        var position=a+va*(Cross(b-a,vb)/det);
        if(Vector2.Distance(position,Center)>19.5f){Hide();return;}
        Ring(position,"DIAMOND",true,exaWave==0?8500:exaWave==6?1200:3500);
        if(exaWave is 2 or 4 or 6) Call("Into the diamond.");
    }
    private async Task TestVoice()
    {
        connection="Sending voice test...";
        using var client=CreateVoiceClient();
        await Send(client,"F R U all phases ready.").ConfigureAwait(false);
    }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("R1, Group 1. Green is current. Cyan marks a reference or later destination.");
        if(ImGui.Button("Test FRU all phases voice")) _=TestVoice();
        ImGui.TextUnformatted(connection);
    }
    public override void OnReset()
    {
        Start(Part.Idle);textUntil=0;spoken.Clear(); towers.Clear();targets.Clear();yellow.Clear();axes.Clear();
        glory=0;firstTowerKnown=false; redMirrors.Clear(); skyTethers.Clear(); stones.Clear();edges.Clear();chainPlayers.Clear();
    }
    public override void OnCombatEnd()=>OnReset();
    public override void OnDisable(){OnReset();voice?.Dispose();voice=null;}
}
