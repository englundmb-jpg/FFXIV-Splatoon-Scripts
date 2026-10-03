using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Dalamud.Bindings.ImGui;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;
public sealed class UWU_Primal_Roulette_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [777];
    public override Metadata Metadata => new(3, "Maggie");
    private const string Current = "Roulette_Current", Next = "Roulette_Next";
    private static readonly Vector3 Center = new(100, 0, 100);
    private static readonly Vector3 North = new(100, 0, 81);
    private static readonly Vector3 Northwest = new(87, 0, 87);
    private bool finale, ifritMoved;
    private uint ultimaId;
    private int primal, weightCount;
    private long expires, lastWeight;
    public override void OnSetup() { SetupMarkers(Current, Next); OnReset(); }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        var action = set.Action?.RowId;
        if (action == 0x2B8F && set.Source?.DataId == 0x221E)
        {
            Controller.Reset(); finale = true; ultimaId = set.Source.EntityId;
            expires = Environment.TickCount64 + 90000;
            return;
        }
        if (!finale) return;
        if (set.Source?.EntityId == ultimaId)
        {
            if (action == 0x2B8C) { Controller.Reset(); return; }
            if (action is 0x2CD3 or 0x2CD4 or 0x2CD5)
            {
                Hide(Current); Hide(Next); weightCount = 0; lastWeight = 0;
                primal = action == 0x2CD3 ? 1 : action == 0x2CD4 ? 2 : 3;
                ifritMoved = false;
                if (primal == 1)
                {
                    // Wait for Garuda's actual cast position; do not place the old A marker inside Wheel.
                    Say("GARUDA: OUT, THEN IN");
                }
                else { Place(Current, North); Place(Next, Northwest); }
            }
        }
        if (primal == 1 && action == 0x2B4E && set.Source?.DataId == 0x2212)
        {
            Place(Current, set.Source.Position); Hide(Next); Say("IN", 3000);
        }
        if ((primal == 2 && action == 0x2B5E && set.Source?.DataId == 0x221A) ||
            (primal == 3 && action == 0x2B90 && set.Source?.DataId == 0x2217))
        {
            Place(Current, North); Hide(Next); primal = 0; Say("REGROUP NORTH");
        }
    }
    public override void OnStartingCast(uint source, uint castId)
    {
        if (!finale) return;
        var actor = Actor(source);
        if (primal == 1 && castId == 0x2B4E && actor?.DataId == 0x2212)
        {
            // Verified Wheel radius 8.7y; place outside it with 1y clearance.
            Place(Current, actor.Position + new Vector3(0, 0, -9.7f));
            Place(Next, actor.Position); Say("OUT — WAIT FOR WHEEL");
        }
        if (primal == 1 && castId == 0x2B4D)
        {
            Hide(Current); Hide(Next); primal = 0;
            Say("MOVE: FEATHER RAIN");
        }
        if (primal == 2 && castId == 0x2B5A && !ifritMoved)
        {
            ifritMoved = true; Place(Current, Northwest); Hide(Next); Say("NORTHWEST");
        }
        // 2B64 is Titan's visual; 2B65 is each actual ground puddle's cast.
        // Puddles in one volley cast together, so count the volley, not each helper.
        if (primal == 3 && castId == 0x2B65)
        {
            var now = Environment.TickCount64;
            if (now - lastWeight < 1500) return;
            lastWeight = now; weightCount++;
            if (weightCount > 3) return;
            Place(Current, weightCount % 2 == 1 ? Northwest : North);
            if (weightCount < 3) Place(Next, weightCount % 2 == 1 ? North : Northwest);
            else Hide(Next);
        }
    }
    public override void OnUpdate()
    {
        if (UpdateTest(Current, Next)) return;
        UpdateNotice();
        if (finale && Environment.TickCount64 > expires) Controller.Reset();
    }
    public override void OnReset()
    {
        finale = ifritMoved = false; ultimaId = 0; primal = weightCount = 0;
        expires = lastWeight = noticeUntil = testUntil = 0;
        Hide(Current); Hide(Next); Hide("PlayerNotice");
    }

    private long testUntil;
    private string notice = "";
    private long noticeUntil;
    private void SetupMarkers(string currentKey, string nextKey)
    {
        Controller.RegisterElementFromCode(currentKey,
            """{"Name":"CURRENT","Enabled":false,"type":0,"radius":0.6,"color":4278255360,"thicc":8.0,"tether":true,"overlayText":"CURRENT","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}""");
        Controller.RegisterElementFromCode(nextKey,
            """{"Name":"NEXT","Enabled":false,"type":0,"radius":0.6,"color":4294967040,"thicc":8.0,"tether":false,"overlayText":"NEXT","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}""");
        Controller.RegisterElementFromCode("PlayerNotice",
            """{"Name":"UWU instruction","Enabled":false,"type":0,"radius":0.0,"thicc":0.0,"overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":2.0,"overlayVOffset":2.0}""");
    }
    private void Place(string key, Vector3 position, bool enabled = true)
    {
        if (!Controller.TryGetElementByName(key, out var e)) return;
        e.SetRefPosition(position);
        e.SetOffPosition(Vector3.Zero);
        e.Enabled = enabled;
    }
    private void Hide(string key)
    {
        if (Controller.TryGetElementByName(key, out var e)) e.Enabled = false;
    }
    private void Say(string text, int milliseconds = 5000)
    {
        notice = text;
        noticeUntil = Environment.TickCount64 + milliseconds;
    }
    private bool UpdateTest(string currentKey, string nextKey)
    {
        if (testUntil == 0) return false;
        if (Environment.TickCount64 >= testUntil)
        {
            testUntil = 0;
            OnReset();
            return false;
        }
        if (Svc.Objects.LocalPlayer is { } player)
        {
            Place(currentKey, player.Position);
            Place(nextKey, player.Position + new Vector3(2, 0, 0));
            Say("DISPLAY TEST ONLY", 100);
        }
        UpdateNotice();
        return true;
    }
    private void UpdateNotice()
    {
        if (Environment.TickCount64 >= noticeUntil || Svc.Objects.LocalPlayer == null)
        {
            Hide("PlayerNotice");
            return;
        }
        if (Controller.TryGetElementByName("PlayerNotice", out var e))
        {
            e.overlayText = notice;
            Place("PlayerNotice", Svc.Objects.LocalPlayer.Position);
        }
    }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Green CURRENT, cyan NEXT. The display test checks visibility only, not fight correctness.");
        if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat]) return;
        if (ImGui.Button("Show display test for 5 seconds"))
        {
            Controller.Reset();
            testUntil = Environment.TickCount64 + 5000;
        }
    }
    public override void OnCombatEnd() => Controller.Reset();
    public override void OnDisable() => Controller.Reset();
    private static IGameObject? Actor(uint id) => Svc.Objects.FirstOrDefault(x => x.EntityId == id);
    private static float Distance(Vector3 a, Vector3 b) => Vector2.Distance(new(a.X, a.Z), new(b.X, b.Z));
}
