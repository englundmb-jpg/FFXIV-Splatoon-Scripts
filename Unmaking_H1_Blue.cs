// SPDX-License-Identifier: AGPL-3.0-or-later
// Orb ordering adapted from PunishXIV/Splatoon EX7_Gaze_of_the_Void
// (NightmareXIV, mirage). Fight IDs/geometry: awgil/ffxiv_bossmod Ex8Enuo.
// H1 strategy: Icy Veins, healers first clockwise from EACH tank orb.
// Development build: needs an in-game import and replay/live validation.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon.SplatoonScripting;

namespace MaggieScripts
{
    public class Unmaking_H1_Blue : SplatoonScript<Unmaking_H1_Blue.Config>
    {
        public override Metadata Metadata { get; } = new(1, "Maggie helper; orb logic credited to NightmareXIV, mirage");
        public override HashSet<uint> ValidTerritories { get; } = new HashSet<uint> { 1362 };
        public class Config
        {
            public bool H1WestSide = true;
            public bool EchoCallouts = true;
            public bool VoiceCallouts = true;
        }
        HttpClient voice;
        volatile string voiceStatus = "Not tested";
        public override void OnEnable()
        {
            voice = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromMilliseconds(700) };
        }
        public override void OnDisable()
        {
            voice?.Dispose(); voice = null;
        }
        async Task SayAsync(string message)
        {
            var client = voice;
            if(client == null) return;
            try
            {
                using var body = new StringContent("MAGGIE_UNMAKING_H1|" + message, Encoding.UTF8, "text/plain");
                using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                voiceStatus = "Sent to Triggernometry";
            }
            catch(Exception) { voiceStatus = "Check Triggernometry endpoint localhost:51423"; }
        }
        static readonly Vector2 Center = new Vector2(100, 100);
        const uint Blue = 0xFFFFA020;
        readonly HashSet<string> said = new HashSet<string>();
        readonly HashSet<uint> coneSeen = new HashSet<uint>();
        uint fast, slow;
        int soaked;
        bool orbReady, sawVuln, gazeActive, growsActive;
        long soakAt, gazeAt, growsAt;
        int stackMode; // 0 unknown, 1 light parties, 2 full party
        Vector2? conePoint;
        string lastConeDirection = "";
        static Vector2 Pos(IGameObject a) => new Vector2(a.Position.X, a.Position.Z);
        static float Angle(Vector2 p) => MathF.Atan2(p.X, -p.Y);
        static float Wrap(float a)
        {
            while(a < 0) a += MathF.PI * 2;
            while(a >= MathF.PI * 2) a -= MathF.PI * 2;
            return a;
        }
        static Vector2 Radial(float a, float r) => Center + new Vector2(MathF.Sin(a), -MathF.Cos(a)) * r;
        public override void OnSetup()
        {
            Controller.RegisterElementFromCode("Stand", "{\"type\":0,\"radius\":0.8,\"thicc\":6.0,\"fillIntensity\":0.1,\"tether\":true,\"overlayFScale\":1.7,\"overlayBGColor\":3758096384,\"overlayTextColor\":4294967295}");
            Controller.RegisterElementFromCode("Text", "{\"type\":1,\"radius\":0.0,\"thicc\":0.0,\"refActorComparisonType\":5,\"refActorPlaceholder\":[\"<1>\"],\"overlayFScale\":1.8,\"overlayBGColor\":3758096384,\"overlayTextColor\":4294967295}");
            Controller.Hide();
        }
        void Mark(Vector2 p, string label)
        {
            var e = Controller.GetElementByName("Stand");
            e.Enabled = true;
            e.refX = p.X; e.refY = p.Y; e.refZ = BasePlayer.Position.Y;
            e.color = Blue; e.overlayText = label;
        }
        void Banner(string message)
        {
            var e = Controller.GetElementByName("Text");
            e.Enabled = true; e.overlayText = message;
        }
        void Call(string key, string message)
        {
            if(!said.Add(key)) return;
            if(C.VoiceCallouts) _ = SayAsync(message);
            if(C.EchoCallouts) Svc.Chat.Print(new XivChatEntry { Type = XivChatType.Echo, Message = new SeStringBuilder().AddText("[Unmaking] " + message).Build() });
        }
        public override void OnReset()
        {
            Controller.Hide(); said.Clear(); coneSeen.Clear();
            fast = slow = 0; soaked = 0; stackMode = 0;
            orbReady = sawVuln = gazeActive = growsActive = false;
            conePoint = null; lastConeDirection = "";
        }
        public override void OnActorControl(uint sourceId, uint command, uint p1, uint p2, uint p3, uint p4, uint p5, uint p6, uint p7, uint p8, ulong targetId, byte replaying)
        {
            if(command == 502 && p1 == 701) stackMode = 1;
            if(command == 502 && p1 == 702) stackMode = 2;
        }
        public override void OnActionEffectEvent(ActionEffectSet set)
        {
            var id = set.Action?.RowId ?? 0;
            if(id == 49983 || id == 49984) { growsActive = false; stackMode = 0; }
            if(orbReady && (id == 50006 || id == 50007) && set.TargetEffects.Any(t => t.TargetID == BasePlayer.ObjectId))
            {
                soaked++;
                soakAt = Environment.TickCount64;
                sawVuln = false;
                if(soaked == 1) Call("orb-wait", "Wait. Let vulnerability expire.");
                if(soaked >= 2) { Call("orb-done", "Orbs done."); gazeActive = false; }
            }
        }
        public override void OnUpdate()
        {
            Controller.Hide();
            if(BasePlayer == null || BasePlayer.IsDead) return;
            var actors = Svc.Objects.OfType<IBattleNpc>().ToList();
            var casts = actors.Where(a => a.IsCasting).ToList();
            var now = Environment.TickCount64;
            if(casts.Any(a => a.CastActionId == 50002) && !gazeActive)
            {
                gazeActive = true; gazeAt = now;
                fast = slow = 0; soaked = 0; orbReady = sawVuln = false;
                said.Clear(); coneSeen.Clear(); conePoint = null; lastConeDirection = "";
                Call("gaze-start", "Gaze. Prepare to follow the cones.");
            }
            if(gazeActive && now - gazeAt > 65000) gazeActive = false;
            var grows = casts.Where(a => a.CastActionId >= 49977 && a.CastActionId <= 49980).ToList();
            if(grows.Count > 0)
            {
                if(!growsActive) { growsActive = true; growsAt = now; said.Clear(); }
                DrawGrows(grows, actors);
                return;
            }
            if(growsActive && now - growsAt > 11000) { growsActive = false; stackMode = 0; }
            if(!gazeActive) return;
            if(DrawOrbs(actors)) return;
            DrawCones(casts.Where(a => a.CastActionId == 50005).OrderBy(a => a.TotalCastTime - a.CurrentCastTime).ToList());
        }
        static bool SafeGrows(Vector2 p, List<IBattleNpc> casts, float margin)
        {
            if(Vector2.Distance(p, Center) > 19 - margin) return false;
            foreach(var a in casts)
            {
                var d = Vector2.Distance(p, Pos(a));
                if(a.CastActionId == 49977 && d < 40 + margin) return false;
                if(a.CastActionId == 49978 && d > 40 - margin && d < 60 + margin) return false;
                if(a.CastActionId == 49979 && d < 12 + margin) return false;
                if(a.CastActionId == 49980 && d > 6 - margin && d < 40 + margin) return false;
            }
            return true;
        }
        void DrawGrows(List<IBattleNpc> grows, List<IBattleNpc> actors)
        {
            // Wait a short interval for simultaneous helper casts to arrive.
            if(Environment.TickCount64 - growsAt < 300) return;
            var portal = grows.FirstOrDefault(a => a.CastActionId == 49977 || a.CastActionId == 49978);
            if(portal == null) { Banner("Reading portal..."); return; }
            if(actors.Any(a => a.DataId == 0x4BFA && a.GetTethers().Count > 0)) stackMode = 2;
            else if(actors.Any(a => a.DataId == 0x4BF7 && a.GetTethers().Count > 0)) stackMode = 1;
            var axis = Pos(portal) - Center;
            if(axis.LengthSquared() < 1) return;
            axis = Vector2.Normalize(axis);
            var right = new Vector2(-axis.Y, axis.X);
            // Choose the eastmost of the two symmetric safe lanes (true north).
            // Resolve exact north/south ties consistently: H1 north, H2 south.
            float eastSign = MathF.Abs(right.X) < 0.001f ? (right.Y >= 0 ? 1 : -1) : (right.X > 0 ? 1 : -1);
            float side = C.H1WestSide ? -eastSign : eastSign;
            Vector2? best = null;
            float bestScore = float.NegativeInfinity;
            // Symmetric light-party lanes; unknown stack mode uses the same AoE-safe
            // lane but explicitly requests the player to read the stack marker.
            float lateral = stackMode == 2 ? 0 : 3.5f;
            for(float along = -17.5f; along <= 17.5f; along += 0.25f)
            {
                var p = Center + axis * along + right * lateral * side;
                var other = Center + axis * along - right * lateral * side;
                if(!SafeGrows(p, grows, 1) || !SafeGrows(other, grows, 1)) continue;
                float clearance = 0;
                for(float m = 1.25f; m <= 3; m += 0.25f)
                    if(SafeGrows(p, grows, m) && SafeGrows(other, grows, m)) clearance = m;
                var score = clearance * 10 - MathF.Abs(along) * 0.1f;
                if(score > bestScore) { bestScore = score; best = p; }
            }
            if(best == null) { Banner("No confirmed spot - check portals"); return; }
            var bossIn = grows.Any(a => a.CastActionId == 49980);
            var bossOut = grows.Any(a => a.CastActionId == 49979);
            string move = bossIn ? "In, away from portal" : bossOut ? "Out, toward portal" : portal.CastActionId == 49978 ? "Toward portal" : "Away from portal";
            string stack = stackMode == 2 ? "Full party. Tanks in front." : stackMode == 1 ? "Light party. Tank in front." : "Check line-stack marker.";
            Mark(best.Value, "H1: " + move.ToUpperInvariant());
            Banner(stack);
            Call("grows-" + bossIn + bossOut + portal.CastActionId, move + ".");
            if(stackMode != 0) Call("stack-" + stackMode, stack);
        }
        bool DrawOrbs(List<IBattleNpc> actors)
        {
            if(soaked >= 2) return false;
            var balls = actors.Where(a => (a.DataId == 19909 || a.DataId == 19910) && a.GetTethers().Any(t => t.Id == 406 || t.Id == 407)).ToList();
            if(!orbReady)
            {
                var f = ChooseOrb(balls, 407);
                var s = ChooseOrb(balls, 406);
                if(f == 0 || s == 0) return false;
                fast = f; slow = s; orbReady = true;
            }
            var id = soaked == 0 ? fast : slow;
            var orb = balls.FirstOrDefault(a => a.ObjectId == id);
            if(orb == null) { Banner("Wait - checking your orb"); return true; }
            float remaining = BasePlayer.StatusList.FirstOrDefault(s => s.StatusId == 2941)?.RemainingTime ?? 0;
            if(soaked == 1 && remaining > 0) sawVuln = true;
            // No timer-based permission: wait until the actual debuff has been
            // observed and removed. A missed status fails closed.
            if(soaked == 1 && (!sawVuln || remaining > 0 || Environment.TickCount64 - soakAt < 500))
            {
                Banner(remaining > 0 ? "WAIT " + remaining.ToString("F1") + "s - DO NOT SOAK" : "WAIT - checking vulnerability");
                return true;
            }
            // Stand two yalms inward from your assigned moving orb, within soak
            // range. This is a destination marker, not an orb explosion outline.
            var radial = Pos(orb) - Center;
            if(radial.LengthSquared() < 1) return true;
            var p = Pos(orb) - Vector2.Normalize(radial) * 2;
            // Avoid offering a destination inside any other live orb's explosion.
            if(balls.Any(b => b.ObjectId != id && Vector2.Distance(Pos(b), p) < (b.DataId == 19910 ? 6.5f : 5.5f)))
            { Banner("Orbs overlap - wait / check spacing"); return true; }
            Mark(p, soaked == 0 ? "YELLOW - WITH H2" : "PURPLE - WITH H2");
            Call(soaked == 0 ? "yellow" : "purple", soaked == 0 ? "Yellow orb. Together with other healer." : "Purple orb now. Together.");
            return true;
        }
        static uint ChooseOrb(List<IBattleNpc> balls, uint tether)
        {
            var group = balls.Where(a => a.GetTethers().Any(t => t.Id == tether)).ToList();
            if(group.Count != 4 || group.Count(a => a.DataId == 19910) != 1) return 0;
            var tankAngle = Angle(Pos(group.First(a => a.DataId == 19910)) - Center);
            return group.Where(a => a.DataId == 19909).OrderBy(a => Wrap(Angle(Pos(a) - Center) - tankAngle)).First().ObjectId;
        }
        static bool ConeSafe(Vector2 p, List<IBattleNpc> cones)
        {
            foreach(var a in cones)
            {
                var delta = p - Pos(a);
                var distance = delta.Length();
                if(distance < 0.5f) return false;
                var forward = new Vector2(MathF.Sin(a.Rotation), MathF.Cos(a.Rotation));
                // 22.5 degree half-angle plus space for marker/player.
                var extra = MathF.Asin(MathF.Min(0.95f, 1.1f / distance));
                if(Vector2.Dot(delta / distance, forward) >= MathF.Cos(MathF.PI / 8 + extra)) return false;
            }
            return true;
        }
        void DrawCones(List<IBattleNpc> cones)
        {
            foreach(var a in cones) coneSeen.Add(a.ObjectId);
            if(cones.Count == 0) return;
            if(coneSeen.Count < 7) { Banner("Reading cone order..."); return; }
            // Same seven-imminent-cone window as upstream BossMod. The last
            // cones overlap the beginning, so treating all ten as simultaneous
            // would incorrectly leave no safe sector.
            var imminent = cones.Take(7).ToList();
            if(conePoint.HasValue && ConeSafe(conePoint.Value, imminent))
            { Mark(conePoint.Value, "GAZE - STAND HERE"); return; }
            var origin = conePoint ?? Pos(BasePlayer);
            Vector2? next = null; float best = float.MaxValue;
            for(int degrees = 0; degrees < 360; degrees++)
            {
                var p = Radial(degrees * MathF.PI / 180, 8);
                if(!ConeSafe(p, imminent)) continue;
                // Prefer the middle of a safe sector over its boundaries.
                if(!ConeSafe(Radial((degrees - 3) * MathF.PI / 180, 8), imminent) || !ConeSafe(Radial((degrees + 3) * MathF.PI / 180, 8), imminent)) continue;
                float score = Vector2.DistanceSquared(p, origin);
                if(score < best) { best = score; next = p; }
            }
            if(next == null) { Banner("Follow the cones - reading next gap"); return; }
            if(conePoint.HasValue)
            {
                var change = Wrap(Angle(next.Value - Center) - Angle(conePoint.Value - Center));
                string direction = change < MathF.PI ? "clockwise" : "counterclockwise";
                if(lastConeDirection != direction) { Call("turn-" + direction, "Move " + direction + ". Follow the blue circle."); lastConeDirection = direction; }
            }
            else Call("cone-first", "Go to blue. Follow the cones.");
            conePoint = next;
            Mark(next.Value, "GAZE - MOVE HERE");
        }
        public override void OnSettingsDraw()
        {
            ImGui.TextWrapped("H1 WEST helper: Naught Grows + Gaze only. Healer orbs are first clockwise from tank orbs. Development build: validate in game.");
            ImGui.Checkbox("H1 west-side stack (off = east)", ref C.H1WestSide);
            ImGui.Checkbox("Local Echo callouts", ref C.EchoCallouts);
            ImGui.Checkbox("Triggernometry voice callouts", ref C.VoiceCallouts);
            ImGui.TextWrapped("Import Triggernometry_Unmaking_H1_Blue.xml. Uses your local endpoint localhost:51423. Blue circles indicate destinations. WAIT hides the orb destination.");
            ImGui.TextWrapped("Voice: " + voiceStatus);
            if(ImGui.Button("Test callout")) { said.Remove("test"); Call("test", "Blue circle. Healer one."); }
        }
    }
}
