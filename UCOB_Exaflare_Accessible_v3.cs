/* Encounter event handling adapted from awgil/ffxiv_bossmod.
Modified for Maggie: Splatoon rendering, accessibility state and conservative validation.
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
// Review build: API-compiled; not verified in an in-game UCOB replay.
// Mechanic references and limits: validation/UCOB-accessibility.md.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon.SplatoonScripting;
namespace MaggieScripts.Duties.Stormblood;
public sealed class UCOB_Exaflare_Accessible_v3 : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata Metadata => new(1, "Maggie");
    private const bool TestGreen = true;
    private sealed class Line
    {
        public Vector3 Next, Advance;
        public int Left = 6;
        public long Expected;
    }
    private readonly List<Line> lines = new();
    private readonly List<(Vector3 Point, long Time)> cleared = new();
    private readonly HashSet<uint> casters = new();
    private Vector3? chosen;
    private bool active, lost;
    private long expires;
    public override void OnSetup() { SetupVisuals(); OnReset(); }
    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == 9967) { OnReset(); active = true; expires = Environment.TickCount64 + 25000; }
        if (!active || castId != 9968 || !casters.Add(source)) return;
        if (Actor(source) is not IBattleChara caster) { lost = true; return; }
        // Game rotation: (sin(rotation), cos(rotation)), unlike north-based display angles.
        lines.Add(new Line { Next = caster.Position,
            Advance = new Vector3(MathF.Sin(caster.Rotation), 0, MathF.Cos(caster.Rotation)) * 8,
            Expected = Environment.TickCount64 + (long)((caster.TotalCastTime - caster.CurrentCastTime) * 1000) });
        chosen = null;
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (!active || set.Action?.RowId is not (9968 or 9969)) return;
        if (set.Source == null) { lost = true; return; }
        var point = set.Source.Position;
        var line = lines.FirstOrDefault(x => x.Left > 0 && Distance(x.Next, point) < 1);
        if (line == null) { lost = true; return; }
        cleared.Add((point, Environment.TickCount64));
        line.Next = point + line.Advance; --line.Left;
        line.Expected = Environment.TickCount64 + 1500;
    }
    private bool IsClear(Vector3 point)
    {
        if (Distance(point, Vector3.Zero) > 19.5f) return false;
        // Whole 0.6y circle plus 0.2y tolerance outside EVERY remaining known blast.
        foreach (var line in lines)
            for (int i = 0; i < line.Left; ++i)
                if (Distance(point, line.Next + i * line.Advance) < 6.8f) return false;
        return true;
    }
    private bool ClearPath(Vector3 from, Vector3 to)
    {
        // Conservative: require the entire direct path to avoid each next blast's circle.
        var a = new Vector2(from.X, from.Z); var b = new Vector2(to.X, to.Z); var d = b - a;
        foreach (var line in lines.Where(x => x.Left > 0))
        {
            var q = new Vector2(line.Next.X, line.Next.Z);
            var t = d.LengthSquared() < 0.001f ? 0 : Math.Clamp(Vector2.Dot(q - a, d) / d.LengthSquared(), 0, 1);
            if (Vector2.Distance(q, a + t * d) < 6.8f) return false;
        }
        return true;
    }
    public override void OnUpdate()
    {
        if (DisplayTest()) return;
        HideVisuals();
        if (!active) return;
        if (Environment.TickCount64 > expires) { OnReset(); return; }
        if (lost || lines.Any(x => x.Left > 0 && Environment.TickCount64 > x.Expected + 750))
        { Draw("EXA TRACKING LOST - DODGE VISUALLY"); return; }
        // Three pairs spawn in sequence. Never call a point safe using only an early pair.
        if (casters.Count != 6) { Draw("EXA - DODGE FIRST PAIR"); return; }
        if (lines.All(x => x.Left == 0)) { OnReset(); return; }
        var player = Svc.Objects.LocalPlayer;
        if (player == null) return;
        if (chosen is { } old && (!IsClear(old) || !ClearPath(player.Position, old))) chosen = null;
        if (chosen == null)
            foreach (var item in cleared.OrderBy(x => Distance(x.Point, player.Position)))
                if (Environment.TickCount64 - item.Time >= 250 && IsClear(item.Point) && ClearPath(player.Position, item.Point))
                { chosen = item.Point; break; }
        Draw(chosen.HasValue ? "CLEARED EXA SPOT" : "EXA - KEEP DODGING", chosen, true);
    }
    public override void OnReset()
    { active = lost = false; lines.Clear(); cleared.Clear(); casters.Clear(); chosen = null; expires = testUntil = 0; HideVisuals(); }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Green circle at a cleared blast position, only after all three pairs are observed and the remaining tracked blasts leave it clear. This is a conservative follow-up aid, NOT an opening dodge solver. No green circle means dodge visually. Unknown/missing events hide the circle. Untested in combat.");
        TestButton();
    }

    private long testUntil;
    private void SetupVisuals()
    {
        Controller.RegisterElementFromCode("Arrow", """{"Enabled":false,"type":2,"radius":0,"thicc":8,"color":4278190335,"LineEndB":1}""");
        Controller.RegisterElementFromCode("Spot", """{"Enabled":false,"type":0,"radius":0.6,"thicc":8,"color":4278255360}""");
        Controller.RegisterElementFromCode("Text", """{"Enabled":false,"type":0,"radius":0,"thicc":0,"overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":2,"overlayVOffset":2}""");
    }
    private void HideVisuals()
    {
        foreach (var key in new[] { "Arrow", "Spot", "Text" })
            if (Controller.TryGetElementByName(key, out var e)) e.Enabled = false;
    }
    private void Draw(string text, Vector3? destination = null, bool green = false)
    {
        HideVisuals();
        var player = Svc.Objects.LocalPlayer;
        if (player == null) return;
        if (Controller.TryGetElementByName("Text", out var label))
        {
            label.SetRefPosition(player.Position); label.SetOffPosition(Vector3.Zero);
            label.overlayText = text; label.Enabled = true;
        }
        if (destination is not { } pos) return;
        pos.Y = player.Position.Y;
        if (green && Controller.TryGetElementByName("Spot", out var spot))
        {
            spot.SetRefPosition(pos); spot.SetOffPosition(Vector3.Zero); spot.Enabled = true;
        }
        if (!green && Distance(player.Position, pos) > 0.5f && Controller.TryGetElementByName("Arrow", out var arrow))
        {
            arrow.SetRefPosition(player.Position); arrow.SetOffPosition(pos); arrow.Enabled = true;
        }
    }
    private bool DisplayTest()
    {
        if (testUntil == 0) return false;
        if (Environment.TickCount64 >= testUntil || Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        { OnReset(); return false; }
        if (Svc.Objects.LocalPlayer is { } p) Draw("DISPLAY TEST ONLY", p.Position + new Vector3(3, 0, 0), TestGreen);
        return true;
    }
    private void TestButton()
    {
        if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat]) return;
        if (ImGui.Button("Show display test for 5 seconds"))
        { OnReset(); testUntil = Environment.TickCount64 + 5000; }
    }
    public override void OnCombatEnd() => OnReset();
    public override void OnDisable() => OnReset();
    private static IGameObject? Actor(uint id) => Svc.Objects.FirstOrDefault(x => x.EntityId == id);
    private static float Distance(Vector3 a, Vector3 b) => Vector2.Distance(new(a.X, a.Z), new(b.X, b.Z));
    private static float Angle(Vector3 p) => MathF.Atan2(p.X, -p.Z); // north=0, positive=CW
    private static Vector3 At(float a, float radius) => new(MathF.Sin(a) * radius, 0, -MathF.Cos(a) * radius);
}
