using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using MaggieScripts.Duties.Stormblood;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using Dalamud.Game.ClientState.Objects.Types;

namespace Dalamud.Bindings.ImGui { public static class ImGui { public static bool Button(string s)=>false; public static void TextWrapped(string s) {} } }
namespace Dalamud.Game.ClientState.Conditions { public enum ConditionFlag { InCombat } }
namespace ECommons.Configuration { }
namespace Dalamud.Game.ClientState.Objects.Types {
 public interface IGameObject { uint EntityId {get;} uint BaseId {get;} Vector3 Position {get;} float Rotation {get;} bool IsDead {get;} }
 public interface IBattleChara:IGameObject { float TotalCastTime {get;} float CurrentCastTime {get;} }
 public class Obj:IBattleChara { public uint EntityId {get;set;} public uint BaseId {get;set;} public Vector3 Position {get;set;} public float Rotation {get;set;} public bool IsDead {get;set;} public float TotalCastTime {get;set;}=4; public float CurrentCastTime {get;set;} }
}
namespace ECommons.DalamudServices {
 public class Objects:List<Obj> { public Obj? LocalPlayer {get;set;} }
 public class Conditions { public bool this[Dalamud.Game.ClientState.Conditions.ConditionFlag x]=>false; }
 public class Client { public uint TerritoryType=>733; }
 public static class Svc { public static Objects Objects=new(); public static Conditions Condition=new(); public static Client ClientState=new(); }
}
namespace ECommons.Hooks.ActionEffectTypes {
 public class Action { public uint RowId; }
 public class ActionEffectSet { public Action? Action; public IGameObject? Source; }
}
namespace Splatoon.SplatoonScripting {
 public record Metadata(int Version,string Author);
 public class Element { public bool Enabled; public string overlayText=""; public Vector3 Ref,Off; public int LineEndB; public void SetRefPosition(Vector3 x)=>Ref=x; public void SetOffPosition(Vector3 x)=>Off=x; }
 public class Controller {
  public Dictionary<string,Element> Elements=new(); private Dictionary<Type,object> configs=new();
  public void RegisterElementFromCode(string key,string json) { var j=JsonDocument.Parse(json).RootElement; Elements[key]=new(){ LineEndB=j.TryGetProperty("LineEndB",out var v)?v.GetInt32():0}; }
  public bool TryGetElementByName(string key,out Element e)=>Elements.TryGetValue(key,out e!);
  public T GetConfig<T>() where T:new() { if(!configs.ContainsKey(typeof(T))) configs[typeof(T)]=new T(); return (T)configs[typeof(T)]; }
  public List<Obj> GetPartyMembers()=>Svc.Objects.Where(x=>x.EntityId<=8).ToList();
 }
 public class SplatoonScript {
  public Controller Controller=new(); public virtual HashSet<uint>? ValidTerritories=>null; public virtual Metadata? Metadata=>null;
  public virtual void OnSetup(){} public virtual void OnReset(){} public virtual void OnUpdate(){} public virtual void OnDisable(){} public virtual void OnCombatEnd(){} public virtual void OnSettingsDraw(){} public virtual void OnStartingCast(uint source,uint id){}
  public virtual void OnActionEffectEvent(ECommons.Hooks.ActionEffectTypes.ActionEffectSet s){}
  public virtual void OnActorControl(uint s,uint c,uint p1,uint p2,uint p3,uint p4,uint p5,uint p6,uint p7,uint p8,ulong t,byte r){}
 }
}
public static class Program {
 static int assertions;
 static void Assert(bool x,string label) { ++assertions;if(!x) throw new Exception(label); }
 static void Init() { Svc.Objects.Clear(); for(uint i=1;i<=8;++i) Svc.Objects.Add(new(){EntityId=i,Position=new(0,0,0)}); Svc.Objects.LocalPlayer=Svc.Objects[0]; }
 static void Icon(SplatoonScript s,uint who,uint icon)=>s.OnActorControl(who,34,icon,0,0,0,0,0,0,0,0,0);
 static void Effect(SplatoonScript s,uint id,Obj? o=null)=>s.OnActionEffectEvent(new(){Action=new(){RowId=id},Source=o});
 static void Tick(SplatoonScript s)=>s.OnUpdate();
 static string Text(SplatoonScript s)=>s.Controller.Elements["Text"].overlayText;
 static Vector3 Destination(SplatoonScript s)=>s.Controller.Elements["Arrow"].Off;
 static Vector3 At(float a,float r)=>new(MathF.Sin(a)*r,0,-MathF.Cos(a)*r);
 static UCOB_Grand_Octet_Accessible_v3 Grand(int sector,bool opposite=false) {
  Init(); var s=new UCOB_Grand_Octet_Accessible_v3();s.OnSetup();s.OnStartingCast(100,9959);
  foreach(var (id,oid,a,tl) in new[]{(100u,0x1FE8u,sector*MathF.PI/4,0x1E43u),(101u,0x1FE1u,sector*MathF.PI/4+(opposite?MathF.PI:MathF.PI/4),0x1E43u),(102u,0x1FDFu,MathF.PI,0x1E44u)}) {
   Svc.Objects.Add(new(){EntityId=id,BaseId=oid,Position=At(a,24)});s.OnActorControl(id,407,tl,0,0,0,0,0,0,0,0,0);
  } return s;
 }
 public static void Main() {
  for(int sector=0;sector<8;++sector) foreach(bool opposite in new[]{false,true}) {
   var s=Grand(sector,opposite);Icon(s,2,119);Tick(s);int d=sector%2==0?-1:1;
   Assert(Vector3.Distance(Destination(s),At(sector*MathF.PI/4+MathF.PI+(opposite?d*MathF.PI/4:0),20))<0.001,"opposite/skip");
   Effect(s,9923);Svc.Objects.LocalPlayer!.Position=new(0,0,-20);Tick(s);
   Assert(Math.Sign(Destination(s).X)==d,"CW/CCW run");Assert(s.Controller.Elements["Arrow"].LineEndB==1,"arrowhead destination");
   Icon(s,3,41);Tick(s);Assert(Destination(s)==Vector3.Zero,"center at red marker");
  }
  {
   var s=Grand(0);Icon(s,2,119);for(uint i=3;i<=7;++i) Icon(s,i,20);Icon(s,8,41);Effect(s,9953);Icon(s,1,39);Tick(s);
   Assert(Text(s).Contains("BAIT TWINTANIA"),"bait before stack");Assert(Destination(s).X>0,"CCW of south Twin");
   Icon(s,1,42);Tick(s);Assert(Text(s).StartsWith("STACK"),"stack after Twin lock");Assert(Destination(s).X<0,"CW of south Twin");
   Svc.Objects[1].IsDead=true;Tick(s);Assert(Text(s).Contains("UNKNOWN"),"death invalidates bait deduction");s.OnCombatEnd();Assert(!s.Controller.Elements["Arrow"].Enabled,"wipe clears");
  }
  {
   var s=Grand(1);Icon(s,2,119);Icon(s,8,41);Effect(s,9953);Tick(s);Assert(Text(s).Contains("UNKNOWN"),"missing dive markers fail closed");
  }
  foreach(bool earlySecond in new[]{false,true}) {
   Init();var s=new UCOB_Tenstrike_Accessible_v3();s.OnSetup();s.OnStartingCast(100,9958);
   for(uint i=2;i<=5;++i)Icon(s,i,40);Tick(s);Assert(Text(s).StartsWith("WAIT"),"second wave waits");
   if(earlySecond)foreach(uint i in new uint[]{1,6,7,8})Icon(s,i,40);
   Effect(s,9945);
   if(!earlySecond)foreach(uint i in new uint[]{1,6,7,8})Icon(s,i,40);
   Tick(s);Assert(Text(s).Contains("TAKE YOUR CLAIMED SPOT"),"second wave retained");Assert(!s.Controller.Elements["Arrow"].Enabled,"no invented coordinates");
   Effect(s,9945);Tick(s);Assert(!s.Controller.Elements["Text"].Enabled,"two waves finish");
  }
  {
   Init();var s=new UCOB_Exaflare_Accessible_v3();s.OnSetup();s.OnStartingCast(100,9967);
   var actors=new List<Obj>();foreach(float x in new float[]{-16,-8,0,8,16,24}) { var o=new Obj(){EntityId=(uint)(100+actors.Count),Position=new(x,0,-24),Rotation=0};actors.Add(o);Svc.Objects.Add(o);s.OnStartingCast(o.EntityId,9968); }
   Tick(s);Assert(!s.Controller.Elements["Spot"].Enabled,"no green before a blast clears");
   var method=s.GetType().GetMethod("IsClear",BindingFlags.NonPublic|BindingFlags.Instance)!;
   Assert(!(bool)method.Invoke(s,new object[]{new Vector3(0,0,0)})!,"future blast unsafe");
   Assert(!(bool)method.Invoke(s,new object[]{new Vector3(21,0,0)})!,"arena edge rejected");
   Effect(s,9968,actors[2]); actors[2].Position=new(0,0,-16);Effect(s,9969,actors[2]);
   var cleared=(List<(Vector3 Point,long Time)>)s.GetType().GetField("cleared",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(s)!;
   for(int i=0;i<cleared.Count;++i) cleared[i]=(cleared[i].Point,Environment.TickCount64-300);
   Svc.Objects.LocalPlayer!.Position=new(0,0,-16);Tick(s);
   Assert(s.Controller.Elements["Spot"].Enabled,"green after confirmed clear and all pairs known");
   Assert(Vector3.Distance(s.Controller.Elements["Spot"].Ref,new(0,0,-16))<0.01,"green at cleared location");
   Effect(s,9969,new Obj(){Position=new(2,0,3)});Tick(s);Assert(Text(s).Contains("TRACKING LOST"),"unmatched event fails closed");
  }
  Console.WriteLine($"PASS: {assertions} assertions; actual script source compiled with test-only game/render adapters.");
 }
}
