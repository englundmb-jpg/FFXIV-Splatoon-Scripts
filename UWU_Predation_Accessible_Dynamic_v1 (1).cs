using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SplatoonScriptsOfficial.Duties.Stormblood;

// Normal UWU Ultimate Predation dodge, not the knockback/cheese strategy.
// Geometry reference: FFXIV-CombatReborn/BossmodReborn,
// BossMod/Modules/Stormblood/Ultimate/UWU/P4UltimatePredation.cs.
// Green CURRENT is the destination now; cyan NEXT is only a preview.
// Move along the arena edge when NEXT becomes green. STAY covers both waves,
// not the Feather Rain that follows. This build still needs an in-game test.
public sealed class UWU_Predation_Accessible_Dynamic : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = new() { 777 };
    public override Metadata? Metadata => new(2, "OpenAI — Predation accessibility");

    private const int Samples = 1440; // quarter-degree steps
    private const int Cushion = 10;  // 2.5 degrees on either side
    private const float Radius = 19f;
    private static readonly Vector2 Center = new(100, 100);
    private long started;
    private long stableSince;
    private uint ultimaId;
    private int phase; // 0 inactive, 1 finding positions, 2 first wave, 3 second
    private Snapshot? previous;
    private Vector3 firstPoint;
    private Vector3 secondPoint;
    private bool stay;

    private sealed record Snapshot(Vector2 Garuda, Vector2 Titan, float TitanRotation,
        Vector2 Ifrit, float IfritRotation, Vector2 Ultima);

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("Current",
            """{"Name":"CURRENT","type":0,"radius":0.4,"color":4278255360,"thicc":8.0,"tether":true,"overlayText":"CURRENT","overlayFScale":2.0,"overlayTextColor":4278255360,"overlayBGColor":4026531840}""");
        Controller.RegisterElementFromCode("Next",
            """{"Name":"NEXT","type":0,"radius":0.4,"color":4294967040,"thicc":8.0,"tether":false,"overlayText":"NEXT - WAIT","overlayFScale":1.6,"overlayTextColor":4294967040,"overlayBGColor":4026531840}""");
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != 11126) return; // Ultimate Predation
        OnReset();
        ultimaId = source;
        started = Environment.TickCount64;
        phase = 1;
    }

    public override void OnUpdate()
    {
        if (phase == 0) return;
        var now = Environment.TickCount64;
        if (now - started > 26000) { OnReset(); return; }
        if (phase != 1 || now - started < 8500) return;
        if (now - started > 15000) { OnReset(); return; }

        var snapshot = ReadSnapshot();
        if (snapshot == null) { previous = null; return; }
        // Do not freeze a route while actors are still relocating/turning.
        if (previous == null || !Stable(previous, snapshot))
        {
            previous = snapshot;
            stableSince = now;
            return;
        }
        if (now - stableSince < 250) return;
        if (!ChooseRoute(snapshot, out var a, out var b)) return;

        firstPoint = WorldPoint(a);
        secondPoint = WorldPoint(b);
        stay = a == b;
        phase = 2;
        Draw();
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        var action = set.Action?.RowId;
        if (action == 11103 && phase == 2) // first Crimson Cyclone resolves
        {
            phase = 3;
            Draw();
        }
        else if ((action == 11104 && phase >= 2) ||
                 (action == 11085 && phase >= 2)) // cross / Feather Rain
            OnReset();
    }

    private void Draw()
    {
        if (!Controller.TryGetElementByName("Current", out var current) ||
            !Controller.TryGetElementByName("Next", out var next)) return;
        current.SetRefPosition(phase == 3 ? secondPoint : firstPoint);
        current.SetOffPosition(Vector3.Zero);
        current.overlayText = stay ? "CURRENT - STAY" : phase == 3 ? "CURRENT - MOVE" : "CURRENT";
        current.Enabled = true;
        next.SetRefPosition(secondPoint);
        next.SetOffPosition(Vector3.Zero);
        next.Enabled = phase == 2 && !stay;
    }

    public override void OnReset()
    {
        phase = 0;
        previous = null;
        started = stableSince = 0;
        ultimaId = 0;
        if (Controller.TryGetElementByName("Current", out var current)) current.Enabled = false;
        if (Controller.TryGetElementByName("Next", out var next)) next.Enabled = false;
    }

    public override void OnDisable() => OnReset();

    private Snapshot? ReadSnapshot()
    {
        // Data IDs work on every game language. Refuse ambiguous actor matches.
        var garuda = Find(0x2212, 2, 8);
        var titan = Find(0x2217, 10, 23);
        var ifrit = Find(0x221A, 15, 23);
        var ultima = Svc.Objects.FirstOrDefault(x => x.EntityId == ultimaId);
        if (garuda == null || titan == null || ifrit == null || ultima == null) return null;
        var u = XZ(ultima.Position);
        var distance = Vector2.Distance(u, Center);
        if (distance < 8 || distance > 23) return null;
        return new(XZ(garuda.Position), XZ(titan.Position), titan.Rotation,
            XZ(ifrit.Position), ifrit.Rotation, u);
    }

    private static IGameObject? Find(uint dataId, float min, float max)
    {
        var matches = Svc.Objects.Where(x => x.DataId == dataId &&
            Vector2.Distance(XZ(x.Position), Center) >= min &&
            Vector2.Distance(XZ(x.Position), Center) <= max).Take(2).ToArray();
        return matches.Length == 1 ? matches[0] : null;
    }

    private static Vector2 XZ(Vector3 p) => new(p.X, p.Z);
    private static bool Stable(Snapshot a, Snapshot b) =>
        Vector2.DistanceSquared(a.Garuda, b.Garuda) < 0.0025f &&
        Vector2.DistanceSquared(a.Titan, b.Titan) < 0.0025f &&
        Vector2.DistanceSquared(a.Ifrit, b.Ifrit) < 0.0025f &&
        Vector2.DistanceSquared(a.Ultima, b.Ultima) < 0.0025f &&
        MathF.Abs(a.TitanRotation - b.TitanRotation) < 0.005f &&
        MathF.Abs(a.IfritRotation - b.IfritRotation) < 0.005f;

    private static int Wrap(int n) => (n % Samples + Samples) % Samples;
    private static Vector2 Point(int i)
    {
        var angle = i * (2 * MathF.PI / Samples);
        return Center + Radius * new Vector2(MathF.Sin(angle), MathF.Cos(angle));
    }
    private static Vector3 WorldPoint(int i)
    {
        var p = Point(i);
        return new(p.X, 0, p.Y);
    }
    private static bool InLine(Vector2 p, Vector2 origin, float rotation, float halfWidth)
    {
        var offset = p - origin;
        return MathF.Abs(offset.X * MathF.Cos(rotation) - offset.Y * MathF.Sin(rotation)) <= halfWidth;
    }
    private static bool Safe(Vector2 p, Snapshot s, bool second)
    {
        if (Vector2.DistanceSquared(p, s.Garuda) <= 400) return false;
        var r = s.TitanRotation;
        if (!second)
            return !InLine(p, s.Titan, r, 3) &&
                !InLine(p, s.Titan, r + MathF.PI / 4, 3) &&
                !InLine(p, s.Titan, r - MathF.PI / 4, 3) &&
                !InLine(p, s.Ifrit, s.IfritRotation, 9);
        return !InLine(p, s.Titan, r + MathF.PI / 8, 3) &&
            !InLine(p, s.Titan, r - MathF.PI / 8, 3) &&
            !InLine(p, s.Titan, r + MathF.PI / 2, 3) &&
            MathF.Abs(p.X - 100) > 5 && MathF.Abs(p.Y - 100) > 5 &&
            Vector2.DistanceSquared(p, s.Ultima) > 196;
    }

    private static bool[] Allowed(Snapshot s, bool second)
    {
        var raw = Enumerable.Range(0, Samples).Select(i => Safe(Point(i), s, second)).ToArray();
        var result = new bool[Samples];
        for (var i = 0; i < Samples; ++i)
        {
            result[i] = true;
            for (var j = -Cushion; j <= Cushion; ++j)
                if (!raw[Wrap(i + j)]) { result[i] = false; break; }
        }
        return result;
    }

    private static bool ChooseRoute(Snapshot s, out int first, out int second)
    {
        first = second = -1;
        var a = Allowed(s, false);
        var b = Allowed(s, true);
        // Pick the middle of the widest shared safe arc, including wraparound.
        var longest = 0;
        for (var i = 0; i < Samples; ++i)
        {
            if (!a[i] || !b[i] || (a[Wrap(i - 1)] && b[Wrap(i - 1)])) continue;
            var length = 0;
            while (length < Samples && a[Wrap(i + length)] && b[Wrap(i + length)]) ++length;
            if (length <= longest) continue;
            longest = length;
            first = second = Wrap(i + length / 2);
        }
        if (first >= 0) return true;
        // Otherwise take the shortest edge-to-edge move, with the cushion
        // already removed from both danger boundaries. Never invent a fallback.
        var best = Samples;
        for (var i = 0; i < Samples; ++i)
        {
            if (!a[i]) continue;
            for (var j = 0; j < Samples; ++j)
            {
                if (!b[j]) continue;
                var distance = Math.Abs(i - j);
                distance = Math.Min(distance, Samples - distance);
                if (distance >= best) continue;
                best = distance;
                first = i;
                second = j;
            }
        }
        return first >= 0;
    }
}
