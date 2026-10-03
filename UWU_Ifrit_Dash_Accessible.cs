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
    public override HashSet<uint>? ValidTerritories { get; } = [777];
    public override Metadata Metadata => new(4, "Maggie");
    private const string Current = "IfritDash_Current", Next = "IfritDash_Next";
    private static readonly Vector3 Center = new(100, 0, 100);
    private readonly Dictionary<uint, Vector3> nailPositions = new();
    private readonly List<uint> deathOrder = new();
    private readonly HashSet<uint> resolvedDashes = new();
    private bool armed, drawn, crossSeen, finished;
    private uint firstCaster;
    private Vector3 destination;
    private long expires;
    public override void OnSetup() { SetupMarkers(Current, Next); OnReset(); }
    public override void OnUpdate()
    {
        if (UpdateTest(Current, Next)) return;
        UpdateNotice();
        if (finished) return;
        foreach (var actor in Svc.Objects.Where(x => x.DataId == 0x221B))
            nailPositions[actor.EntityId] = actor.Position;
        if (drawn && Environment.TickCount64 > expires)
        {
            Hide(Current); Hide(Next); drawn = false; armed = false; finished = true;
        }
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (finished) return;
        var action = set.Action?.RowId;
        if (action == 0x2B58 && set.Source is { DataId: 0x221B } nail)
        {
            nailPositions[nail.EntityId] = nail.Position;
            if (!deathOrder.Contains(nail.EntityId)) deathOrder.Add(nail.EntityId);
        }
        // The four-dash jump follows Flaming Crush, not the earlier cardinal dashes.
        if (action == 0x2B5D && set.Source?.DataId == 0x221A && deathOrder.Count == 4)
            armed = true;
        if (!drawn) return;
        if (action == 0x2B5F && set.Source?.DataId == 0x221A)
        {
            if (!resolvedDashes.Add(set.Source.EntityId)) return;
            if (set.Source.EntityId == firstCaster)
            {
                Place(Current, destination); Hide(Next);
                Say("SPRINT — MOVE CURRENT", 4000);
            }
        }
        if (action == 0x2B60) crossSeen = true;
        if (resolvedDashes.Count == 4 && crossSeen)
        {
            Hide(Current); Hide(Next); drawn = false; armed = false; finished = true;
            noticeUntil = 0;
        }
    }
    public override void OnStartingCast(uint source, uint castId)
    {
        var caster = Actor(source);
        if ((castId == 0x2CFD && caster?.DataId == 0x2217) || caster?.DataId == 0x221E)
        {
            Hide(Current); Hide(Next); armed = drawn = false; finished = true; noticeUntil = 0;
            return;
        }
        if (finished || !armed || drawn || castId != 0x2B5F || caster?.DataId != 0x221A) return;
        if (!TryNailOrder(out var first, out var rotation))
        {
            Say("IFRIT: NAIL ORDER NOT CONFIRMED"); return;
        }
        var woken = Svc.Objects.OfType<IBattleChara>()
            .Where(x => x.DataId == 0x221A && x.StatusList.Any(s => s.StatusId == 1529)).ToArray();
        if (woken.Length != 1 || Distance(woken[0].Position, Center) < 18 ||
            Distance(woken[0].Position, Center) > 21 || Direction(caster.Position) % 4 != first % 4)
        {
            Say("IFRIT: DASH PATTERN NOT CONFIRMED"); return;
        }
        int start = (first - rotation + 8) % 8;
        int wokenAxis = Direction(woken[0].Position) % 4;
        int dash = 0;
        for (int i = 1; i <= 4; i++)
            if ((start + i * rotation + 16) % 4 == wokenAxis) { dash = i; break; }
        if (dash == 0) return;
        // Both sides are valid; retain the side nearest the player's party position.
        if (Svc.Objects.LocalPlayer is { } player &&
            Distance(Point((start + 4) % 8), player.Position) < Distance(Point(start), player.Position))
            start = (start + 4) % 8;
        int move = dash % 2 == 1 ? 1 : 2;
        destination = Point((start + rotation * move + 16) % 8);
        Place(Current, Point(start)); Place(Next, destination);
        firstCaster = source; drawn = true; crossSeen = false; resolvedDashes.Clear();
        expires = Environment.TickCount64 + 15000;
        Say((rotation == 1 ? "CLOCKWISE" : "COUNTERCLOCKWISE") +
            (move == 1 ? " 45°" : " 90°") + (dash <= 2 ? " FAST" : "") + " — WAIT FOR FIRST DASH", 5000);
    }
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
    public override void OnReset()
    {
        nailPositions.Clear(); deathOrder.Clear(); resolvedDashes.Clear();
        armed = drawn = crossSeen = finished = false; firstCaster = 0;
        expires = noticeUntil = testUntil = 0;
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
