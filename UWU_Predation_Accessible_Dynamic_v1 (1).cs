/*
Predation hazard geometry adapted from BossMod P4UltimatePredation.
BSD 3-Clause License

Copyright (c) 2022-2024, Andrew Gilewsky

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its
   contributors may be used to endorse or promote products derived from
   this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Dalamud.Bindings.ImGui;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SplatoonScriptsOfficial.Duties.Stormblood;
public sealed class UWU_Predation_Accessible_Dynamic : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [777];
    public override Metadata Metadata => new(2, "Maggie");
    private const string Current = "Predation_Start";
    private const string Next = "Predation_Dodge";
    private static readonly Vector3 Center = new(100, 0, 100);
    private bool active, drawn, second, casting;
    private long started;
    private uint ultimaId, ifritId;
    private readonly Dictionary<uint, uint> knownBosses = new();
    private Vector3 destination;
    public override void OnSetup() { SetupMarkers(Current, Next); OnReset(); }
    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == 0x2B76 && Actor(source)?.DataId == 0x221E)
        {
            Hide(Current); Hide(Next); Hide("PlayerNotice");
            drawn = second = casting = false;
            active = true;
            ultimaId = source;
            started = Environment.TickCount64;
        }
        if (active && castId == 0x2B5F && source == ifritId) casting = true;
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (!active || !drawn) return;
        if (set.Action?.RowId == 0x2B5F && set.Source?.EntityId == ifritId && !second)
        {
            second = true;
            Place(Current, destination);
            Hide(Next);
            Say("CURRENT", 3000);
        }
        if (set.Action?.RowId == 0x2B60 && second) Controller.Reset();
    }
    public override void OnUpdate()
    {
        if (UpdateTest(Current, Next)) return;
        UpdateNotice();
        foreach (var boss in Svc.Objects.OfType<IBattleChara>().Where(x =>
                     x.IsTargetable && x.CurrentHp > 0 && (x.DataId == 0x2212 || x.DataId == 0x221A || x.DataId == 0x2217)))
            knownBosses[boss.DataId] = boss.EntityId;
        if (!active) return;
        var elapsed = Environment.TickCount64 - started;
        // Expiry is a fail-closed watchdog, never a movement trigger.
        if (elapsed > 26000) { Controller.Reset(); return; }
        if (drawn || casting || elapsed < 10000) return;
        if (elapsed > 14500)
        {
            Hide(Current); Hide(Next);
            Say("PREDATION: POSITION NOT CONFIRMED", 100);
            return;
        }
        var garudas = Candidates(0x2212, 2, 8);
        var ifrits = Candidates(0x221A, 15, 23);
        var titans = Candidates(0x2217, 10, 23);
        var ultima = Actor(ultimaId);
        if (garudas.Length != 1 || ifrits.Length != 1 || titans.Length != 1 || ultima == null ||
            Distance(ultima.Position, Center) < 8 || Distance(ultima.Position, Center) > 23) return;
        var g = garudas[0]; var f = ifrits[0]; var t = titans[0];
        if (!Solve(g.Position, t.Position, t.Rotation, f.Position, f.Rotation, ultima.Position,
                   out var first, out var next)) return;
        ifritId = f.EntityId;
        destination = next;
        Place(Current, first);
        if (Distance(first, next) > 0.1f) Place(Next, next); else Hide(Next);
        Say(Distance(first, next) > 0.1f ? "CURRENT — WAIT FOR DASH" : "HOLD CURRENT", 7000);
        drawn = true;
    }
    private IGameObject[] Candidates(uint dataId, float min, float max)
    {
        var candidates = Svc.Objects.Where(x => x.DataId == dataId &&
            Distance(x.Position, Center) >= min && Distance(x.Position, Center) <= max).ToArray();
        if (knownBosses.TryGetValue(dataId, out var id))
            return candidates.Where(x => x.EntityId == id).ToArray();
        if (candidates.Length == 1) return candidates;
        return candidates.OfType<IBattleChara>().Where(x => x.StatusList.Any(s => s.StatusId == 1529))
            .Cast<IGameObject>().ToArray();
    }
    // Both endpoints are checked against the complete two-wave geometry at radius 19.
    // A 0.75y clearance is a conservative margin, not an encounter timing or coordinate.
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
    public override void OnReset()
    {
        active = drawn = second = casting = false;
        started = 0; ultimaId = ifritId = 0; knownBosses.Clear();
        noticeUntil = testUntil = 0;
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
