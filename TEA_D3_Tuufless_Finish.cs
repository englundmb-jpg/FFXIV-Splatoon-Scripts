// Tuufless TEA, D3. Use alongside the existing Alpha visual and Final Word helper.
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.TEA;

public sealed class TEA_D3_Tuufless_Finish : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 887 };
    public override Metadata Metadata => new(1, "Maggie accessibility");
    public sealed class Settings
    {
        public string H1 = "";
        public string D1 = "", MT = "";
        public int Tower = 5;
        public bool HigherStays = true;
    }
    private Settings C => Controller.GetConfig<Settings>();
    private enum Part { Idle, Waves, Optical, Alpha, Beta, Trines, Prison }
    private Part part;
    private long until;
    private int killStep, role = -1;
    private bool betaSightKnown, stack, march, jumped, donutNow, moved, grace;
    private string last = "";
    private readonly Dictionary<uint,uint> clones = new();
    private readonly HashSet<uint> targets = new(), prisoners = new();
    private Vector2? donut, prisonOffset;
    private static readonly Vector2 Middle = new(100,100);
    // Four red positions from Tuufless, in path order. Adjacent entries share an axis.
    private static readonly Vector2[] TrineSlots = { new(92,108), new(100,108), new(108,108), new(108,100) };
    private readonly int[] trineWave = new int[4];
    private long waveTime;
    private int wave, start = -1, finish = -1;
    private bool ccSeen, bjSeen;
    private HttpClient? voice;
    public override void OnSetup()
    {
        foreach (string n in new[]{"Current","Next"})
            Controller.RegisterElement(n,new Element(0){Enabled=false,radius=0.8f,thicc=6,
                color=n=="Current"?0xFF00FF00u:0xFFFFFF00u,tether=n=="Current",overlayFScale=1.5f});
    }
    private static HttpClient Client() => new(new HttpClientHandler{UseProxy=false}){Timeout=TimeSpan.FromSeconds(1)};
    public override void OnEnable() => voice=Client();
    private void Begin(Part p,int seconds)
    {
        OnReset(); part=p; until=Environment.TickCount64+1000L*seconds;
    }
    public override void OnStartingCast(uint source,uint id)
    {
        switch(id)
        {
            case 19075:
                if(part!=Part.Waves) { Begin(Part.Waves,120); Say("HOLIES"); }
                break;
            case 19029: if(part!=Part.Waves) Begin(Part.Waves,100); break;
            case 18570: case 18571:
                Begin(Part.Optical,10); stack=id==18571;
                if(!stack) Say("SPREAD");
                break;
            case 18555: Begin(Part.Alpha,65); break;
            case 19219: Begin(Part.Beta,85); break;
            case 18574: Begin(Part.Trines,30); break;
            case 18575 when part==Part.Trines:
                if(Svc.Objects.SearchById(source) is {} h) Preview(XZ(h));
                break;
            case 18582: case 18583: if(part!=Part.Prison) Begin(Part.Prison,60); break;
        }
    }
    private static Vector2 XZ(IGameObject o) => new(o.Position.X,o.Position.Z);
    public override void OnActorControl(uint src,uint cmd,uint p1,uint p2,uint p3,uint p4,uint p5,uint p6,uint p7,uint p8,ulong target,byte replaying)
    {
        if(part==Part.Optical && stack && cmd==502 && p1==62) targets.Add(src);
    }
    public override void OnTetherCreate(uint src,uint target,uint d2,uint d3,uint d5)
    {
        if((part==Part.Alpha || part==Part.Beta) && d3==98 && src!=0 && target!=0) clones[src]=target;
    }
    private void Preview(Vector2 pos)
    {
        long now=Environment.TickCount64;
        if(wave==0 || now-waveTime>900) {wave++; waveTime=now;}
        for(int i=0;i<4;i++)
            if(Vector2.Distance(pos,TrineSlots[i])<2 && trineWave[i]==0) trineWave[i]=wave;
        if(wave!=3 || start>=0 || trineWave.Any(w=>w==0)) return;
        for(int i=0;i<4;i++)
            for(int j=0;j<4;j++)
                if(trineWave[i]==3 && trineWave[j]==1 && Math.Abs(i-j)==1)
                {start=i;finish=j;return;}
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        uint id=set.Action?.RowId??0;
        if(id==18550)
        {
            if(part!=Part.Waves) Begin(Part.Waves,100);
            if(killStep==0) {killStep=1;Say("CHASER");}
        }
        else if(part==Part.Waves && id==18553) OnReset();
        else if(part==Part.Optical && id is 18572 or 18573) OnReset();
        else if(part==Part.Alpha && id==18569) OnReset();
        else if(part==Part.Beta)
        {
            if(id is 18592 or 18593) { betaSightKnown=true; stack=id==18593; }
            if(id==18590 && set.Source!=null) donut=XZ(set.Source);
            if(id==18565) jumped=true;
            if(id is 18572 or 18573) donutNow=true;
            if(id==18566) OnReset();
        }
        else if(part==Part.Trines)
        {
            if(id==18576 && start>=0 && finish>=0 && set.Source!=null &&
                Vector2.Distance(XZ(set.Source),TrineSlots[finish])<2) moved=true;
            // Third wave: the stack resolves about a second later; stay at the end spot.
            if(id==18576 && start>=0 && set.Source!=null &&
                Vector2.Distance(XZ(set.Source),TrineSlots[start])<2) grace=true;
            if(id==18580) OnReset();
        }
    }
    public override void OnUpdate()
    {
        if(part==Part.Idle) return;
        if(BasePlayer==null || BasePlayer.CurrentHp==0 || Environment.TickCount64>=until){OnReset();return;}
        switch(part)
        {
            case Part.Waves: Waves(); break;
            case Part.Optical: Optical(); break;
            case Part.Alpha: case Part.Beta: Calibration(); break;
            case Part.Trines: Trines(); break;
            case Part.Prison: Prison(); break;
        }
    }
    private static IBattleNpc? Boss(uint id) => Svc.Objects.OfType<IBattleNpc>().FirstOrDefault(o=>o.BaseId==id && o.CurrentHp>0 && o.IsTargetable);
    private void Waves()
    {
        if(killStep==0) return;
        var cc=Boss(0x2C4E); var bj=Boss(0x2C4C);
        ccSeen|=cc!=null; bjSeen|=bj!=null;
        if(killStep==1 && ccSeen && cc==null && bj!=null){killStep=2;Say("JUSTICE");}
        if(killStep<=2 && ccSeen && bjSeen && cc==null && bj==null && Boss(0x2C53)!=null){killStep=3;Say("PRIME");}
    }
    private void Optical()
    {
        if(!stack || targets.Count!=2) return;
        bool east=false;
        if(targets.Contains(BasePlayer.EntityId))
        {
            var other=Svc.Objects.SearchById(targets.First(x=>x!=BasePlayer.EntityId)) as IPlayerCharacter;
            if(other==null) return;
            // H1 > D3 > D1 > MT. East-group players always stay east when D3 has the other mark.
            string name=other.Name.TextValue;
            if ((!C.HigherStays && (C.D1.Length==0 || C.MT.Length==0)) ||
                (C.HigherStays && C.H1.Length==0 && other.ClassJob.Value.Role==4))
            { Say("CHECK_STACK");return; }
            east=C.HigherStays ? Same(name,C.H1) : Same(name,C.D1)||Same(name,C.MT);
        }
        Mark("Current",new(east?107:93,100),east?"EAST":"WEST");Say(east?"STACK_E":"STACK_W");
    }
    private static bool Same(string name,string configured) => name.Equals(configured.Trim(),StringComparison.OrdinalIgnoreCase);
    private void Calibration()
    {
        if(role<0 && clones.Count==8 && clones.Values.Distinct().Count()==8 && clones.TryGetValue(BasePlayer.EntityId,out var mine))
            role=Array.IndexOf(clones.Values.OrderByDescending(v=>v).ToArray(),mine);
        if(role<0)return;
        if(part==Part.Alpha)
        {
            Say(role==0?"ALPHA_SHARED":role==1?"ALPHA_DEF":role<=4?"ALPHA_ASSAULT":"ALPHA_NONE");
            return;
        }
        bool light=(role&1)==1;
        if(!march) march=BasePlayer.StatusList.Any(s=>s.StatusId is >=3055 and <=3058);
        if(donutNow)
        {
            if(donut.HasValue) Mark("Current",donut.Value,"DONUT");
            else Hide("Current");
            Say("DONUT");return;
        }
        if(jumped)
        {
            if(!betaSightKnown) { Hide("Current"); Say("CHECK_SIGHT"); return; }
            if(stack) Mark("Current",light?Middle:new Vector2(100,115),light?"MID":"SOUTH");
            else Hide("Current");
            Say(stack?(light?"BETA_MID":"BETA_SOUTH"):"SPREAD");return;
        }
        if(march)
        {
            var next=role switch {0=>new Vector2(118,100),2=>new Vector2(82,100),6=>new Vector2(100,118),_=>new Vector2(100,84)};
            Mark("Current",next,"AFTER MARCH");
            Say(role switch {0=>"JUMP_E",2=>"JUMP_W",6=>"JUMP_S",_=>"GROUP_N"});return;
        }
        var pos=role switch{0=>new Vector2(114,100),1=>new Vector2(92,84),2=>new Vector2(111,100),6=>new Vector2(111,102),_=>new Vector2(111,98)};
        Mark("Current",pos,"SETUP");
        Say(role switch{0=>"DARK_E",1=>"LIGHT_NNW",2=>"DARK_W",6=>"DARK_SW",4=>"DARK_NW",_=>"LIGHT_E"});
    }
    private void Trines()
    {
        if(start<0 || finish<0)return;
        if(!moved)
        {
            Mark("Current",TrineSlots[start],"START");Mark("Next",TrineSlots[finish],"NEXT");Say("START_"+start);
        }
        else
        {
            Hide("Next");Mark("Current",TrineSlots[finish],grace?"STACK":"MOVE HERE");Say(grace?"GRACE":"MOVE_"+finish);
        }
    }
    private void Prison()
    {
        int tower=Math.Clamp(C.Tower,1,8);
        foreach(var p in Svc.Objects.OfType<IPlayerCharacter>().Where(p=>p.StatusList.Any(s=>s.StatusId==2165)))
        {
            if(prisoners.Add(p.EntityId) && prisoners.Count==1)
            {
                var off=XZ(p)-Middle;
                if(off.X>1 && off.Y< -1 && off.Length()>4) prisonOffset=off;
            }
        }
        if(tower==8 || prisoners.Count>=tower){Hide("Current");return;}
        if(prisoners.Count!=tower-1)return;
        float angle=(tower-1)*MathF.PI/4;
        if(prisonOffset is Vector2 first)
        {
            var rotated=Vector2.Transform(first,Matrix3x2.CreateRotation(angle));
            Mark("Current",Middle+rotated,"YOUR TOWER");
        }
        Say("TOWER_"+tower);
    }
    private void Mark(string n,Vector2 p,string label)
    {
        if(Controller.TryGetElementByName(n,out var e)){e.refX=p.X;e.refY=p.Y;e.overlayText=label;e.Enabled=true;}
    }
    private void Hide(string n){if(Controller.TryGetElementByName(n,out var e))e.Enabled=false;}
    private void Say(string code)
    {
        if(last==code)return;
        last=code;
        if(voice!=null)_=Send(voice,code);
    }
    private static async Task Send(HttpClient c,string code)
    {
        try{using var body=new StringContent("MAGGIE_TEA_FINISH|"+code,Encoding.UTF8,"text/plain");
            using var res=await c.PostAsync("http://localhost:51423/",body).ConfigureAwait(false);}
        catch(Exception ex) when(ex is OperationCanceledException or HttpRequestException or ObjectDisposedException){}
    }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("D3 west group: H1, D3, D1, MT. Enter your H1's character name for same-side stack markers.");
        ImGui.InputText("H1 name",ref C.H1,80);
        ImGui.Checkbox("Higher priority stays on its home side",ref C.HigherStays);
        if(!C.HigherStays){ImGui.InputText("D1 name",ref C.D1,80);ImGui.InputText("MT name",ref C.MT,80);}
        ImGui.SliderInt("Your prison tower (8 = last DPS, no tower)",ref C.Tower,1,8);
        if(ImGui.Button("Test finish voice"))_=Test();
    }
    private static async Task Test(){using var client=Client();await Send(client,"TEST");}
    public override void OnReset()
    {
        part=Part.Idle;until=waveTime=0;wave=killStep=0;role=start=finish=-1;
        ccSeen=bjSeen=betaSightKnown=stack=march=jumped=donutNow=moved=grace=false;
        last="";donut=prisonOffset=null;clones.Clear();targets.Clear();prisoners.Clear();
        Array.Clear(trineWave);Hide("Current");Hide("Next");
    }
    public override void OnCombatEnd()=>OnReset();
    public override void OnDisable(){OnReset();voice?.Dispose();voice=null;}
}
