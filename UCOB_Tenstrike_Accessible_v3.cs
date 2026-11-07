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
public sealed class UCOB_Tenstrike_Accessible_v3 : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata Metadata => new(1, "Maggie");
    private const bool TestGreen = false;
    public class Config
    {
        public Vector3 Waiting, First, Second;
        public bool HasWaiting, HasFirst, HasSecond;
    }
    private Config C => Controller.GetConfig<Config>();
    private bool active;
    private int resolved;
    private long expires;
    private readonly HashSet<uint> first = new(), second = new();
    private HashSet<uint> Wave => resolved == 0 ? first : second;
    public override void OnSetup() { SetupVisuals(); OnReset(); }
    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == 9958) { OnReset(); active = true; expires = Environment.TickCount64 + 90000; }
        if (active && castId == 9942) OnReset();
    }
    public override void OnActorControl(uint sourceId, uint command, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, uint p7, uint p8, ulong targetId, byte replaying)
    {
        if (!active || command != 34 || p1 != 40) return;
        if (first.Contains(sourceId) || second.Contains(sourceId)) return;
        (first.Count < 4 && resolved == 0 ? first : second).Add(sourceId);
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (!active || set.Action?.RowId != 9945) return;
        ++resolved;
        if (resolved >= 2) OnReset();
    }
    public override void OnUpdate()
    {
        if (DisplayTest()) return;
        HideVisuals();
        if (!active) return;
        if (Environment.TickCount64 > expires) { OnReset(); return; }
        var wave = Wave;
        if (wave.Count != 4)
        { if (resolved == 1 && first.Contains(Svc.Objects.LocalPlayer?.EntityId ?? 0)) Draw("RETURN TO WAITING SPOT", C.HasWaiting ? C.Waiting : null); return; }
        if (!wave.Contains(Svc.Objects.LocalPlayer?.EntityId ?? 0))
        { Draw(resolved == 0 ? "WAIT - CLAIM SECOND SHAKER SPOT" : "RETURN TO WAITING SPOT", C.HasWaiting ? C.Waiting : null); return; }
        bool configured = resolved == 0 ? C.HasFirst : C.HasSecond;
        var target = resolved == 0 ? C.First : C.Second;
        var player = Svc.Objects.LocalPlayer;
        // Do not point into a cone already aimed at another marked player.
        bool conflict = player != null && wave.Where(x => x != player.EntityId).Select(Actor).Any(x => x != null &&
            MathF.Abs(MathF.IEEERemainder(Angle(x.Position) - Angle(target), 2 * MathF.PI)) < MathF.PI / 4);
        Draw(!configured ? "EARTHSHAKER - TAKE YOUR CLAIMED SPOT" : conflict ? "EARTHSHAKER - ADJUST, SPOT OCCUPIED" : resolved == 0 ? "FIRST SHAKER - OUT" : "SECOND SHAKER - IN FRONT OF PUDDLE",
            configured && !conflict ? target : null);
    }
    public override void OnReset()
    { active = false; resolved = 0; first.Clear(); second.Clear(); expires = testUntil = 0; HideVisuals(); }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Setup required for arrows: agree your first-wave spread spot, second-wave spread spot and waiting/corner claim with your party. Stand at each agreed spot and save it below while out of combat. The video uses flexible claims; this script does not assign a fixed D3 spot. Without saved points it gives text only. Adjust if another player claims your spot.");
        if (Svc.ClientState.TerritoryType == 733 && !Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat] && Svc.Objects.LocalPlayer is { } p)
        {
            if (ImGui.Button("Save waiting / claim position here")) { C.Waiting = p.Position; C.HasWaiting = true; }
            if (ImGui.Button("Save first Earthshaker position here")) { C.First = p.Position; C.HasFirst = true; }
            if (ImGui.Button("Save second Earthshaker position here")) { C.Second = p.Position; C.HasSecond = true; }
            if (ImGui.Button("Clear saved positions")) C.HasWaiting = C.HasFirst = C.HasSecond = false;
        }
        ImGui.TextWrapped($"Saved: waiting={C.HasWaiting}, first={C.HasFirst}, second={C.HasSecond}");
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
