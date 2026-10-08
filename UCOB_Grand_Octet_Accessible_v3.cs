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
// Revision 2: user-selected bait offset and Tank LB3 instructions; not compiled or replay-tested.
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
public sealed class UCOB_Grand_Octet_Accessible_v3 : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata Metadata => new(2, "Maggie");
    private const bool TestGreen = false;
    private enum Step { Off, Center, Opposite, Run, BahaCenter, Twin, Twisters }
    private Step step;
    private Vector3? baha, nael, twin, start;
    private int direction;
    private bool skip, sprint, twinLocked, uncertain;
    private readonly HashSet<uint> dives = new(), stacks = new();
    private readonly HashSet<uint> seenTimeline = new();
    private long expires;
    private string RunText => direction > 0 ? "CLOCKWISE" : "COUNTERCLOCKWISE";
    public override void OnSetup() { SetupVisuals(); OnReset(); }
    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == 9959)
        { OnReset(); step = Step.Center; expires = Environment.TickCount64 + 90000; }
        if (castId == 9942 && step != Step.Off) OnReset();
    }
    public override void OnActorControl(uint sourceId, uint command, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, uint p7, uint p8, ulong targetId, byte replaying)
    {
        if (step == Step.Off) return;
        if (command == 407 && (p1 == 0x1E43 || p1 == 0x1E44)) seenTimeline.Add(sourceId);
        if (command != 34) return;
        if (p1 is 119 or 20 or 41)
        {
            dives.Add(sourceId);
            if (p1 == 119 && step == Step.Center) { CapturePositions(); step = Step.Opposite; }
            if (p1 == 41) step = Step.BahaCenter;
        }
        if (p1 == 39) stacks.Add(sourceId);
        if (p1 == 42) { twinLocked = true; step = Step.Twin; }
    }
    private void CapturePositions()
    {
        foreach (var id in seenTimeline)
        {
            var actor = Actor(id);
            if (actor == null || Distance(actor.Position, Vector3.Zero) < 18) continue;
            if (actor.BaseId == 0x1FE8) baha = actor.Position;
            if (actor.BaseId == 0x1FE1) nael = actor.Position;
            if (actor.BaseId == 0x1FDF) twin = actor.Position;
        }
        if (baha is not { } b || nael is not { } n) return;
        var sector = (int)MathF.Round(Angle(b) / (MathF.PI / 4));
        direction = (sector & 1) == 0 ? -1 : 1;
        var opposite = Angle(b) + MathF.PI;
        skip = Vector3.Dot(At(opposite, 1), At(Angle(n), 1)) > 0.99f;
        start = At(opposite + (skip ? direction * MathF.PI / 4 : 0), 20);
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (step == Step.Off) return;
        var id = set.Action?.RowId;
        if (id == 9923 && step == Step.Opposite) step = Step.Run;
        if (id is >= 9931 and <= 9935) sprint = true;
        if (id == 9953) step = Step.Twin;
        if (id == 9951) { step = Step.Twisters; expires = Environment.TickCount64 + 3000; }
    }
    public override void OnUpdate()
    {
        if (DisplayTest()) return;
        HideVisuals();
        if (step == Step.Off) return;
        if (Environment.TickCount64 > expires) { OnReset(); return; }
        var player = Svc.Objects.LocalPlayer;
        if (player == null) return;
        // Deaths invalidate the guide's one-dive-per-player assumption.
        if (Controller.GetPartyMembers().Any(x => x.IsDead)) uncertain = true;
        if (step is Step.Center or Step.Opposite) CapturePositions();
        if (step == Step.Center) { Draw("CENTER - WAIT FOR NAEL MARK", Vector3.Zero); return; }
        if (step == Step.Opposite)
        {
            Draw(start.HasValue ? $"OPPOSITE BAHAMUT{(skip ? " + SKIP NAEL" : "")}\nTHEN {RunText}" : "FIND BAHAMUT - POSITION UNKNOWN", start);
            return;
        }
        if (step == Step.Run)
        {
            // A short direction cue, NOT a certified safe destination or timed route.
            Draw(direction == 0 ? "FOLLOW PARTY - DIRECTION UNKNOWN" : $"{(sprint && !skip ? "SPRINT - " : "RUN - ")}{RunText}",
                direction == 0 ? null : At(Angle(player.Position) + direction * MathF.PI / 12, 20));
            return;
        }
        if (step == Step.BahaCenter) { Draw("BAHAMUT LOCKED - CENTER", Vector3.Zero); return; }
        if (step == Step.Twisters) { Draw("MOVE - TWISTERS"); return; }
        if (uncertain || dives.Count != 7) { Draw("DIVE ASSIGNMENT UNKNOWN - FOLLOW PARTY"); return; }
        if (twin is not { } t) { Draw("FIND TWINTANIA - POSITION UNKNOWN"); return; }
        bool bait = !dives.Contains(player.EntityId);
        if (bait && !twinLocked)
        { Draw("BAIT TWINTANIA - CCW WALL", At(Angle(t) - MathF.PI / 8, 20)); return; }
        // User-selected Tank LB3 strategy: stack markers do not change the instruction.
        // Tower selection stays manual; no tower is claimed as personally assigned.
        Draw(bait ? "DIVE LOCKED - FILL AN OPEN TOWER\nMOVE FOR TWISTERS" : "TANK LB3 STRATEGY - FILL AN OPEN TOWER\nMOVE FOR TWISTERS");
    }
    public override void OnReset()
    {
        step = Step.Off; baha = nael = twin = start = null; direction = 0;
        skip = sprint = twinLocked = uncertain = false;
        dives.Clear(); stacks.Clear(); seenTimeline.Clear(); expires = testUntil = 0; HideVisuals();
    }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Tessan PF Tank LB3 strategy. Twin bait arrow: 22.5 degrees counterclockwise of Twin at the existing radius 20 (user-selected placement). With Twin north, this is halfway between north and northwest. Stack markers are ignored; fill open towers manually. Requires the party to use Tank LB3; this script does not detect LB use. Run arrow is a direction cue. This revision is not compiled or combat-tested.");
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
