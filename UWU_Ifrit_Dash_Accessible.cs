/*
Nail-order and Awoken-direction logic adapted from cactbot UWU.

                                 Apache License
                           Version 2.0, January 2004
                        http://www.apache.org/licenses/

   TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION

   1. Definitions.

      "License" shall mean the terms and conditions for use, reproduction,
      and distribution as defined by Sections 1 through 9 of this document.

      "Licensor" shall mean the copyright owner or entity authorized by
      the copyright owner that is granting the License.

      "Legal Entity" shall mean the union of the acting entity and all
      other entities that control, are controlled by, or are under common
      control with that entity. For the purposes of this definition,
      "control" means (i) the power, direct or indirect, to cause the
      direction or management of such entity, whether by contract or
      otherwise, or (ii) ownership of fifty percent (50%) or more of the
      outstanding shares, or (iii) beneficial ownership of such entity.

      "You" (or "Your") shall mean an individual or Legal Entity
      exercising permissions granted by this License.

      "Source" form shall mean the preferred form for making modifications,
      including but not limited to software source code, documentation
      source, and configuration files.

      "Object" form shall mean any form resulting from mechanical
      transformation or translation of a Source form, including but
      not limited to compiled object code, generated documentation,
      and conversions to other media types.

      "Work" shall mean the work of authorship, whether in Source or
      Object form, made available under the License, as indicated by a
      copyright notice that is included in or attached to the work
      (an example is provided in the Appendix below).

      "Derivative Works" shall mean any work, whether in Source or Object
      form, that is based on (or derived from) the Work and for which the
      editorial revisions, annotations, elaborations, or other modifications
      represent, as a whole, an original work of authorship. For the purposes
      of this License, Derivative Works shall not include works that remain
      separable from, or merely link (or bind by name) to the interfaces of,
      the Work and Derivative Works thereof.

      "Contribution" shall mean any work of authorship, including
      the original version of the Work and any modifications or additions
      to that Work or Derivative Works thereof, that is intentionally
      submitted to Licensor for inclusion in the Work by the copyright owner
      or by an individual or Legal Entity authorized to submit on behalf of
      the copyright owner. For the purposes of this definition, "submitted"
      means any form of electronic, verbal, or written communication sent
      to the Licensor or its representatives, including but not limited to
      communication on electronic mailing lists, source code control systems,
      and issue tracking systems that are managed by, or on behalf of, the
      Licensor for the purpose of discussing and improving the Work, but
      excluding communication that is conspicuously marked or otherwise
      designated in writing by the copyright owner as "Not a Contribution."

      "Contributor" shall mean Licensor and any individual or Legal Entity
      on behalf of whom a Contribution has been received by Licensor and
      subsequently incorporated within the Work.

   2. Grant of Copyright License. Subject to the terms and conditions of
      this License, each Contributor hereby grants to You a perpetual,
      worldwide, non-exclusive, no-charge, royalty-free, irrevocable
      copyright license to reproduce, prepare Derivative Works of,
      publicly display, publicly perform, sublicense, and distribute the
      Work and such Derivative Works in Source or Object form.

   3. Grant of Patent License. Subject to the terms and conditions of
      this License, each Contributor hereby grants to You a perpetual,
      worldwide, non-exclusive, no-charge, royalty-free, irrevocable
      (except as stated in this section) patent license to make, have made,
      use, offer to sell, sell, import, and otherwise transfer the Work,
      where such license applies only to those patent claims licensable
      by such Contributor that are necessarily infringed by their
      Contribution(s) alone or by combination of their Contribution(s)
      with the Work to which such Contribution(s) was submitted. If You
      institute patent litigation against any entity (including a
      cross-claim or counterclaim in a lawsuit) alleging that the Work
      or a Contribution incorporated within the Work constitutes direct
      or contributory patent infringement, then any patent licenses
      granted to You under this License for that Work shall terminate
      as of the date such litigation is filed.

   4. Redistribution. You may reproduce and distribute copies of the
      Work or Derivative Works thereof in any medium, with or without
      modifications, and in Source or Object form, provided that You
      meet the following conditions:

      (a) You must give any other recipients of the Work or
          Derivative Works a copy of this License; and

      (b) You must cause any modified files to carry prominent notices
          stating that You changed the files; and

      (c) You must retain, in the Source form of any Derivative Works
          that You distribute, all copyright, patent, trademark, and
          attribution notices from the Source form of the Work,
          excluding those notices that do not pertain to any part of
          the Derivative Works; and

      (d) If the Work includes a "NOTICE" text file as part of its
          distribution, then any Derivative Works that You distribute must
          include a readable copy of the attribution notices contained
          within such NOTICE file, excluding those notices that do not
          pertain to any part of the Derivative Works, in at least one
          of the following places: within a NOTICE text file distributed
          as part of the Derivative Works; within the Source form or
          documentation, if provided along with the Derivative Works; or,
          within a display generated by the Derivative Works, if and
          wherever such third-party notices normally appear. The contents
          of the NOTICE file are for informational purposes only and
          do not modify the License. You may add Your own attribution
          notices within Derivative Works that You distribute, alongside
          or as an addendum to the NOTICE text from the Work, provided
          that such additional attribution notices cannot be construed
          as modifying the License.

      You may add Your own copyright statement to Your modifications and
      may provide additional or different license terms and conditions
      for use, reproduction, or distribution of Your modifications, or
      for any such Derivative Works as a whole, provided Your use,
      reproduction, and distribution of the Work otherwise complies with
      the conditions stated in this License.

   5. Submission of Contributions. Unless You explicitly state otherwise,
      any Contribution intentionally submitted for inclusion in the Work
      by You to the Licensor shall be under the terms and conditions of
      this License, without any additional terms or conditions.
      Notwithstanding the above, nothing herein shall supersede or modify
      the terms of any separate license agreement you may have executed
      with Licensor regarding such Contributions.

   6. Trademarks. This License does not grant permission to use the trade
      names, trademarks, service marks, or product names of the Licensor,
      except as required for reasonable and customary use in describing the
      origin of the Work and reproducing the content of the NOTICE file.

   7. Disclaimer of Warranty. Unless required by applicable law or
      agreed to in writing, Licensor provides the Work (and each
      Contributor provides its Contributions) on an "AS IS" BASIS,
      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
      implied, including, without limitation, any warranties or conditions
      of TITLE, NON-INFRINGEMENT, MERCHANTABILITY, or FITNESS FOR A
      PARTICULAR PURPOSE. You are solely responsible for determining the
      appropriateness of using or redistributing the Work and assume any
      risks associated with Your exercise of permissions under this License.

   8. Limitation of Liability. In no event and under no legal theory,
      whether in tort (including negligence), contract, or otherwise,
      unless required by applicable law (such as deliberate and grossly
      negligent acts) or agreed to in writing, shall any Contributor be
      liable to You for damages, including any direct, indirect, special,
      incidental, or consequential damages of any character arising as a
      result of this License or out of the use or inability to use the
      Work (including but not limited to damages for loss of goodwill,
      work stoppage, computer failure or malfunction, or any and all
      other commercial damages or losses), even if such Contributor
      has been advised of the possibility of such damages.

   9. Accepting Warranty or Additional Liability. While redistributing
      the Work or Derivative Works thereof, You may choose to offer,
      and charge a fee for, acceptance of support, warranty, indemnity,
      or other liability obligations and/or rights consistent with this
      License. However, in accepting such obligations, You may act only
      on Your own behalf and on Your sole responsibility, not on behalf
      of any other Contributor, and only if You agree to indemnify,
      defend, and hold each Contributor harmless for any liability
      incurred by, or claims asserted against, such Contributor by reason
      of your accepting any such warranty or additional liability.

   END OF TERMS AND CONDITIONS

   APPENDIX: How to apply the Apache License to your work.

      To apply the Apache License to your work, attach the following
      boilerplate notice, with the fields enclosed by brackets "[]"
      replaced with your own identifying information. (Don't include
      the brackets!)  The text should be enclosed in the appropriate
      comment syntax for the file format. We also recommend that a
      file or class name and description of purpose be included on the
      same "printed page" as the copyright notice for easier
      identification within third-party archives.

   Copyright 2017-2021 https://github.com/quisquous/cactbot

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.


Bundled projects with different licenses:
    * FFXIVPluginHelper.cs (custom license allowing sublicensing)
        https://github.com/xtuaok/ACT_EnmityPlugin/
    * resources/ffxiv: art from Final Fantasy XIV (FINAL FANTASY® XIV Materials Usage License)
        reused non-commercially via https://support.na.square-enix.com/rule.php?id=5382
        FINAL FANTASY is a registered trademark of Square Enix Holdings Co., Ltd.
    * resources/sounds/BigWigs: sounds from BigWigs WoW mod (CreativeCommons)
        see: resources/sounds/BigWigs/LICENSE.txt
    * resources/sounds/freesound: sounds from freesound.org (Creative Commons)
        see: resources/sounds/freesound/LICENSE.txt
    * resources/sounds/Overwatch: sounds from OverWatch (Creative Commons)
        see: resources/sounds/Overwatch/LICENSE.txt

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

namespace MaggieScripts.Duties.Stormblood;
public sealed class UWU_Ifrit_Dash_Accessible : SplatoonScript
{
    // v5, 2026-10-06: arbitrary Awoken nail orders; non-Awoken live-layout route.
    // Evidence: Maggie's Network_30301_20261005.log (four complete dash sequences).
    // Geometry: awgil/ffxiv_bossmod, BossMod.Ultimate/Stormblood/Ultimate/UWU/
    // P2CrimsonCyclone.cs: main half-width 9, cross half-width 5.
    // Awoken cross axes are +/-45 degrees from the real Ifrit's dash.
    // Motion model: straight chord, Sprint 7.8y/s (BossMod FRUAI.cs).
    // 0.7s is a DESIGN allowance for response/acceleration, NOT a mechanic timer.
    // Forecast spacing 1.4s / cross +2.1s is checked against this log. The MOVE
    // cue NEVER uses that forecast: it requires the selected dash's action effect.
    // Validation: 120/120 order/status geometry cases and all four complete
    // October 5 sequences, including SW,N,E,SE and the non-Awoken layout.
    // Syntax parsed; full Dalamud/Splatoon compilation and live play not tested.
    public override HashSet<uint>? ValidTerritories { get; } = [777];
    public override Metadata Metadata => new(5, "Maggie");
    private const string Current = "IfritDash_Current", Next = "IfritDash_Next";
    private static readonly Vector3 Center = new(100, 0, 100);
    private readonly Dictionary<uint, Vector3> nailPositions = new();
    private readonly List<uint> deathOrder = new();
    private readonly HashSet<uint> resolvedDashes = new(), crossSources = new();
    private readonly Dictionary<uint, int> dashAxes = new();
    private readonly List<uint> casts = new();
    private bool armed, drawn, finished, moved, awoken, firstEffectSeen;
    private uint realIfrit, moveCaster;
    private int[] plannedAxes = [];
    private int moveIndex;
    private Vector3 destination;
    private long expires;
    private string diagnostic = "Waiting for Ifrit.";

    public override void OnSetup() { SetupMarkers(Current, Next); OnReset(); }
    public override void OnUpdate()
    {
        if (UpdateTest(Current, Next)) return;
        UpdateNotice();
        if (finished) return;
        foreach (var actor in Svc.Objects.Where(x => x.DataId == 0x221B))
            if (!deathOrder.Contains(actor.EntityId)) nailPositions[actor.EntityId] = actor.Position;
        // Retry actor/position availability. Do not require everything to exist on
        // the single frame of OnStartingCast. Never start a plan after dash 1 hit.
        if (armed && !drawn && !firstEffectSeen) TryInitialize();
        if (drawn && Environment.TickCount64 > expires)
            Stop("IFRIT: DISPLAY EXPIRED - WATCH DASHES");
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (finished) return;
        var action = set.Action?.RowId;
        if (action == 0x2B58 && set.Source is { DataId: 0x221B } nail)
        {
            if (!deathOrder.Contains(nail.EntityId))
            {
                nailPositions[nail.EntityId] = nail.Position;
                deathOrder.Add(nail.EntityId);
            }
        }
        if (action == 0x2B5D && set.Source is { DataId: 0x221A } boss)
        {
            // Flaming Crush is the phase gate; nail completeness is checked only
            // for Awoken prediction, not for the non-Awoken layout-based route.
            realIfrit = boss.EntityId;
            armed = true;
            diagnostic = "Flaming Crush seen; waiting for dash layout.";
        }
        if (!armed) return;
        if (action == 0x2B5F && set.Source is { DataId: 0x221A } caster)
        {
            if (!resolvedDashes.Add(caster.EntityId)) return;
            firstEffectSeen = true;
            if (!drawn) { Stop("IFRIT: DATA MISSING - WATCH DASHES"); return; }
            if (!dashAxes.ContainsKey(caster.EntityId))
            { Stop("IFRIT: DASH MISMATCH - WATCH DASHES"); return; }
            if (awoken && (resolvedDashes.Count > plannedAxes.Length ||
                dashAxes[caster.EntityId] != plannedAxes[resolvedDashes.Count - 1]))
            { Stop("IFRIT: ORDER CHANGED - WATCH DASHES"); return; }
            if (!moved && caster.EntityId == moveCaster)
            {
                moved = true;
                Place(Current, destination); Hide(Next);
                Say("SPRINT - STRAIGHT TO GREEN", 4000);
            }
        }
        if (drawn && action == 0x2B60 && set.Source is { DataId: 0x233C } helper)
        {
            if (!awoken) { Stop("IFRIT: UNEXPECTED CROSS - WATCH DASHES"); return; }
            crossSources.Add(helper.EntityId);
        }
        // Non-Awoken Ifrit has no cross to wait for. Awoken has two helpers.
        if (drawn && resolvedDashes.Count == 4 && (!awoken || crossSources.Count >= 2))
            Stop();
    }
    public override void OnStartingCast(uint source, uint castId)
    {
        var caster = Actor(source);
        if ((castId == 0x2CFD && caster?.DataId == 0x2217) || caster?.DataId == 0x221E)
        { Stop(); return; }
        if (finished || !armed || castId != 0x2B5F || caster?.DataId != 0x221A) return;
        if (casts.Contains(source)) return;
        casts.Add(source);
        if (!drawn) TryInitialize();
        if (!drawn) return;
        int axis = Direction(caster.Position) % 4;
        if (!dashAxes.TryGetValue(source, out var expected) || expected != axis ||
            (awoken && (casts.Count > 4 || axis != plannedAxes[casts.Count - 1])))
            Stop("IFRIT: CAST MISMATCH - WATCH DASHES");
    }
    private void TryInitialize()
    {
        if (Svc.Objects.LocalPlayer is not { } player || Actor(realIfrit) is not IBattleChara boss) return;
        var actors = Svc.Objects.OfType<IBattleChara>().Where(x => x.DataId == 0x221A).ToArray();
        if (actors.Length != 4 || actors.Any(x => Distance(x.Position, Center) < 18 || Distance(x.Position, Center) > 21)) return;
        var axes = actors.Select(x => Direction(x.Position) % 4).ToArray();
        if (axes.Distinct().Count() != 4) return;
        awoken = boss.StatusList.Any(s => s.StatusId == 1529);
        // Never infer non-Awoken from a missing boss object or an incomplete layout.
        if (actors.Any(x => x.EntityId != realIfrit && x.StatusList.Any(s => s.StatusId == 1529))) return;
        Route? route;
        if (awoken)
        {
            if (deathOrder.Count != 4 || deathOrder.Any(x => !nailPositions.ContainsKey(x)))
            { diagnostic = "Awoken: waiting for all four nail deaths."; return; }
            plannedAxes = deathOrder.Select(x => Direction(nailPositions[x]) % 4).ToArray();
            if (plannedAxes.Distinct().Count() != 4)
            { diagnostic = "Awoken: nail axes incomplete."; return; }
            if (casts.Count > 4 || casts.Where((id, i) => Actor(id) is not { } a || Direction(a.Position) % 4 != plannedAxes[i]).Any())
            { Stop("IFRIT: ORDER MISMATCH - WATCH DASHES"); return; }
            int wokenIndex = Array.IndexOf(plannedAxes, Direction(boss.Position) % 4);
            route = ChooseRoute(plannedAxes, wokenIndex, player.Position);
        }
        else
        {
            // In the supplied non-Awoken pull the dash order did NOT match nails.
            // Use dash 1's actual caster; check the route against all six possible
            // permutations of the other three axes, without predicting their order.
            if (casts.Count == 0 || Actor(casts[0]) is not { } first) return;
            plannedAxes = [];
            route = ChooseNonAwokenRoute(Direction(first.Position) % 4, player.Position);
        }
        if (route == null)
        { diagnostic = "No route passed the movement checks."; Say("IFRIT: NO VERIFIED PATH - WATCH DASHES"); return; }
        dashAxes.Clear();
        foreach (var actor in actors) dashAxes[actor.EntityId] = Direction(actor.Position) % 4;
        moveIndex = route.Cue;
        moveCaster = awoken ? actors.Single(x => dashAxes[x.EntityId] == plannedAxes[moveIndex]).EntityId : casts[0];
        destination = Point(route.End);
        Place(Current, Point(route.Start)); Place(Next, destination);
        drawn = true; moved = false;
        expires = Environment.TickCount64 + 15000; // display watchdog, not a mechanic trigger
        diagnostic = (awoken ? "Awoken" : "Non-Awoken") + $"; move after dash {moveIndex + 1}.";
        Say("SPRINT READY - WAIT AT GREEN", 5000);
    }

    private sealed class Route(float start, float end, int cue, float score)
    {
        public float Start = start, End = end;
        public int Cue = cue;
        public float Score = score;
    }
    private static Route? ChooseRoute(int[] order, int wokenIndex, Vector3 player)
    {
        Route? best = null;
        for (int cue = 0; cue < 4; cue++)
            for (int start = 0; start < 8; start++)
                for (int end = 0; end < 8; end++)
                {
                    int steps = Math.Min((start - end + 8) % 8, (end - start + 8) % 8);
                    if (steps == 0 || steps > 2 || !CheckRoute(order, wokenIndex, start, end, cue, 1.4f)) continue;
                    float score = Distance(player, Point(start)) + Distance(Point(start), Point(end)) * 0.1f;
                    if (best == null || score < best.Score) best = new(start, end, cue, score);
                }
        return best;
    }
    private static Route? ChooseNonAwokenRoute(int firstAxis, Vector3 player)
    {
        Route? best = null;
        // Put the holding point 10y laterally from dash 1's centre line:
        // verified 9y half-width + 1y positioning allowance, on the 19y circle.
        // This is calculated geometry, not a hardcoded world safe spot.
        float offset = MathF.Asin(10f / 19f) / (MathF.PI / 4);
        foreach (int end in new[] { firstAxis, firstAxis + 4 })
            foreach (float start in new[] { end - offset, end + offset })
            {
                bool valid = true;
                for (int a = 0; a < 4; a++)
                    for (int b = 0; b < 4; b++)
                        for (int c = 0; c < 4; c++)
                        {
                            int[] order = [firstAxis, a, b, c];
                            if (order.Distinct().Count() != 4) continue;
                            // Non-Awoken log: effect gaps 2.048, 2.010, 2.010s.
                            // Test ALL six orders at 2.0s plus timing cushions.
                            if (!CheckRoute(order, -1, start, end, 0, 2.0f)) valid = false;
                        }
                if (!valid) continue;
                float score = Distance(player, Point(start));
                if (best == null || score < best.Score) best = new(start, end, 0, score);
            }
        return best;
    }
    private static bool CheckRoute(int[] order, int wokenIndex, float start, float end, int cue, float spacing)
    {
        for (int i = 0; i < 4; i++)
            if (!ClearDuringEvent(start, end, (i - cue) * spacing, order[i], 9)) return false;
        if (wokenIndex >= 0)
        {
            float t = (wokenIndex - cue) * spacing + 2.1f;
            if (!ClearDuringEvent(start, end, t, (order[wokenIndex] + 1) % 4, 5) ||
                !ClearDuringEvent(start, end, t, (order[wokenIndex] + 3) % 4, 5)) return false;
        }
        return true;
    }
    private static bool ClearDuringEvent(float start, float end, float timeAfterCue, int axis, float halfWidth)
    {
        Vector3 a = Point(start) - Center, b = Point(end) - Center;
        float distance = Distance(a, b);
        // Check the entire possible segment at the hit, not just its endpoints.
        // Timing cushions cover observed cast/effect offsets and cadence variation.
        float lo = timeAfterCue <= 0 ? 0 : Math.Clamp((timeAfterCue - 0.35f - 0.7f) * 7.8f / distance, 0, 1);
        float hi = timeAfterCue <= 0 ? 0 : Math.Clamp((timeAfterCue + 0.2f) * 7.8f / distance, 0, 1);
        float angle = axis * MathF.PI / 4;
        Vector3 normal = new(MathF.Cos(angle), 0, MathF.Sin(angle));
        float d0 = Vector3.Dot(Vector3.Lerp(a, b, lo), normal);
        float d1 = Vector3.Dot(Vector3.Lerp(a, b, hi), normal);
        return d0 * d1 > 0 && MathF.Min(MathF.Abs(d0), MathF.Abs(d1)) >= halfWidth + 0.6f;
    }
    private static int Direction(Vector3 p) =>
        ((int)MathF.Round(MathF.Atan2(p.X - 100, 100 - p.Z) / (MathF.PI / 4)) + 8) % 8;
    private static Vector3 Point(float direction)
    {
        float angle = direction * MathF.PI / 4;
        return Center + new Vector3(MathF.Sin(angle), 0, -MathF.Cos(angle)) * 19;
    }
    private void Stop(string message = "")
    {
        Hide(Current); Hide(Next); Hide("PlayerNotice");
        armed = drawn = false; finished = true; noticeUntil = 0;
        if (message.Length > 0) { diagnostic = message; Say(message); }
    }
    public override void OnReset()
    {
        nailPositions.Clear(); deathOrder.Clear(); resolvedDashes.Clear(); crossSources.Clear();
        dashAxes.Clear(); casts.Clear(); plannedAxes = [];
        armed = drawn = finished = moved = awoken = firstEffectSeen = false;
        realIfrit = moveCaster = 0; moveIndex = 0;
        expires = noticeUntil = testUntil = 0; diagnostic = "Waiting for Ifrit.";
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
        ImGui.TextWrapped("v5: Green CURRENT, cyan NEXT. Have Sprint ready. Wait at green; when cyan turns green, sprint straight along the tether to it (do not run around the rim). Supports arbitrary Awoken nail orders and non-Awoken dashes. Display test checks visibility only.");
        ImGui.TextWrapped(diagnostic);
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

