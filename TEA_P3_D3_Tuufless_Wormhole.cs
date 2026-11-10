// Tuufless 09STOP Wormhole; D3 takes the west Enumeration (melee swap).
using Dalamud.Bindings.ImGui;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.TEA;

public sealed class TEA_P3_D3_Tuufless_Wormhole : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 887 };
    public override Metadata Metadata => new(1, "Maggie accessibility");
    private static readonly Vector2 Center = new(100, 100);
    private readonly HashSet<string> announced = new();
    private HttpClient? voice;
    private volatile string voiceStatus = "";
    private long deadline;
    private int number, justiceSide, wave, dashes;
    private bool directed, spread, jumpCast, jump, ray, blades, heatCast, heatHit;
    private Vector2 south, east;

    public override void OnSetup() => Controller.RegisterElement("Stand", new Element(0)
    {
        Enabled = false, color = 0xFF00FF00, radius = 0.7f, thicc = 6, tether = true,
        overlayVOffset = 1.4f, overlayFScale = 1.6f
    });

    public override void OnEnable() => voice = NewClient();
    private static HttpClient NewClient() => new(new HttpClientHandler { UseProxy = false })
        { Timeout = TimeSpan.FromSeconds(1) };

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == 18542)
        {
            OnReset();
            deadline = Environment.TickCount64 + 80000;
            Call("CENTER");
        }
        if (deadline == 0) return;
        // Capture the orientation at Void of Repentance, after the bosses teleport.
        if (castId == 18534) directed = true;
        if (castId is 18517 or 18505) spread = true;
        if (castId == 18505) jumpCast = true;
        if (castId == 19025) heatCast = true;
        if (castId == 19075) OnReset();
    }

    private void Assign(uint target, int n)
    {
        if (deadline == 0 || number != 0 || target != BasePlayer?.EntityId || n < 1 || n > 8) return;
        number = n;
        Call("NUMBER_" + n);
    }
    public override void OnActorControl(uint id, uint cmd, uint p1, uint p2, uint p3,
        uint p4, uint p5, uint p6, uint p7, uint p8, ulong target, byte replaying)
    {
        if (cmd == 502 && p1 is >= 79 and <= 86) Assign(id, (int)p1 - 78);
    }
    public override void OnVFXSpawn(uint target, string path)
    {
        const string prefix = "vfx/lockon/eff/m0361trg_a";
        if (path.StartsWith(prefix, StringComparison.Ordinal) && path.Length > prefix.Length)
            Assign(target, path[prefix.Length] - '0');
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (deadline == 0) return;
        switch (set.Action?.RowId)
        {
            case 18517: blades = spread = true; break;
            case 18506: jump = true; break;
            case 18507: case 18508: ray = true; break;
            case 18537: wave = Math.Max(wave, 1); break;
            case 18536: wave = Math.Max(wave, 2); break;
            case 18535: wave = 3; break;
            case 18883:
                // The dash may hit multiple targets; count one event per dash burst.
                if (Environment.TickCount64 - lastDash > 800)
                {
                    lastDash = Environment.TickCount64;
                    dashes = Math.Min(4, dashes + 1);
                }
                break;
            case 19025: heatHit = true; break;
            case 18512: OnReset(); break;
        }
    }
    private long lastDash;

    private void ReadFrame()
    {
        if (!directed || (south != Vector2.Zero && justiceSide != 0)) return;
        if (south == Vector2.Zero)
        {
            var alex = Svc.Objects.FirstOrDefault(o => o.BaseId == 0x2C53);
            if (alex == null) return;
            var offset = new Vector2(alex.Position.X, alex.Position.Z) - Center;
            if (offset.Length() < 12) return;
            south = Vector2.Normalize(offset);
            east = new Vector2(south.Y, -south.X);
        }
        var justice = Svc.Objects.FirstOrDefault(o => o.BaseId == 0x2C4C);
        if (justice == null || jump) return;
        var bj = new Vector2(justice.Position.X, justice.Position.Z) - Center;
        float lateral = Vector2.Dot(bj, east);
        if (Vector2.Dot(bj, south) < -5 && Math.Abs(lateral) > 5) justiceSide = Math.Sign(lateral);
    }

    private int Side => number % 2 == 1 ? -1 : 1;
    private Vector2 Pos(float across, float down) => Center + east * across + south * down;
    private Vector2 Diagonal(float radius, bool lower) =>
        Pos(Side * radius * 0.707107f, (lower ? 1 : -1) * radius * 0.707107f);

    public override void OnUpdate()
    {
        if (deadline == 0) return;
        if (BasePlayer == null || BasePlayer.CurrentHp == 0 || Environment.TickCount64 >= deadline)
        {
            OnReset();
            return;
        }
        ReadFrame();
        if (heatHit) { Show("ENUM", "WEST ENUM - WITH H1", south == Vector2.Zero ? null : Pos(-5, 0)); return; }
        if (ReadyForCenter())
        {
            Show("HEAT", "STACK - JUST WEST OF CENTER", south == Vector2.Zero ? null : Pos(-1.6f, 0));
            return;
        }
        if (!spread) { Show("CENTER", number == 0 ? "STACK CENTER" : "HOLD CENTER", Center); return; }
        if (number == 0 || south == Vector2.Zero)
        {
            Show("CHECK", "CHECK NUMBER AND BOSSES", null);
            return;
        }
        switch (number)
        {
            case 1: case 2: FirstPair(); break;
            case 3: case 4: SecondPair(); break;
            case 5: case 6: ThirdPair(); break;
            default: FourthPair(); break;
        }
    }

    private bool ReadyForCenter()
    {
        if (number is 7 or 8) return dashes >= 4 || heatHit;
        if (number is 1 or 2) return wave >= 3;
        return wave >= 3 || heatCast;
    }

    private void FirstPair()
    {
        if (dashes >= 3 && wave >= 2) { Soak(3); return; }
        if (dashes >= 1 && ray)
        {
            Show("SIDE", "MOVE TO SIDE - WAIT FOR SOAK 3", Pos(Side * 18.6f, 0));
            return;
        }
        var northDot = Diagonal(17.8f, false);
        if (jump && justiceSide == Side && !ray)
            Show("RAY", "HOLD RED DOT - BAIT RAY", northDot);
        else if (blades && dashes >= 1 && justiceSide != 0 && justiceSide != Side)
            Show("SIDE", "MOVE TO SIDE - WAIT FOR SOAK 3", Pos(Side * 18.6f, 0));
        else
            Show(jumpCast || blades ? "IMMUNE_" + number : "NORTH_" + number,
                (jumpCast || blades ? "ARMS LENGTH - " : "") + (number == 1 ? "FACE OUT - CLEAVE" : "NORTH DOT - DASH"), northDot);
    }

    private void SecondPair()
    {
        if (dashes >= 2)
        {
            Show("NOTCH", "ONE NOTCH NORTH - DODGE SACRAMENT", Pos(Side * 16.8f, 8.8f));
            return;
        }
        if (jump)
        {
            Show("IMMUNE_" + number, number == 3 ? "ARMS LENGTH - FACE OUT" : "ARMS LENGTH - TAKE DASH",
                Diagonal(19.2f, true));
            return;
        }
        if (justiceSide == 0) { Show("CHECK_BJ", "SOUTH - CHECK BRUTE JUSTICE", null); return; }
        bool bait = Side != justiceSide;
        Show(bait ? "JUMP_BAIT" : "WALL", bait ? "BAIT JUMP - JUST INSIDE RED DOT" : "WALL - DODGE RAY",
            Diagonal(bait ? 16.6f : 19.4f, true));
    }

    private void ThirdPair()
    {
        if (wave == 0)
        {
            if (blades && dashes >= 1) Soak(1);
            else WaitAtSide();
            return;
        }
        if (dashes >= 3) { Show("CLEAR", "CLEAR THE NEXT DASH - WAIT AT SIDE", Pos(Side * 18.6f, 0)); return; }
        Show("IMMUNE_" + number, number == 5 ? "NORTH - ARMS LENGTH - FACE OUT" : "NORTH - ARMS LENGTH - DASH",
            Diagonal(17.8f, false));
    }

    private void FourthPair()
    {
        if (wave < 2)
        {
            if (wave == 1 && dashes >= 2) Soak(2);
            else if (blades) Show("WAIT2", "WAIT OUTSIDE - SECOND SOAK", Pos(Side * 19.3f, 0));
            else WaitAtSide();
            return;
        }
        Show("IMMUNE_" + number, number == 7 ? "SOUTH - ARMS LENGTH - FACE OUT" : "SOUTH - ARMS LENGTH - DASH",
            Diagonal(18.5f, true));
    }

    private void WaitAtSide() => Show("WAIT", "SIDE - WAIT OUTSIDE PUDDLE",
        Pos(Side * (number <= 6 ? 16.6f : 17.8f), number <= 6 ? -7.8f : -6.1f));

    private void Soak(int soak)
    {
        // Resolve the current wave's event object first. Earlier waves can persist at the same site.
        Vector2? spot = null;
        for (int stage = soak; stage >= 1 && spot == null; stage--)
        {
            uint id = 0x1EA1DFu + (uint)(stage - 1);
            foreach (var obj in Svc.Objects.Where(o => o.BaseId == id))
            {
                var p = new Vector2(obj.Position.X, obj.Position.Z);
                if (Vector2.Dot(p - Center, east) * Side < 4) continue;
                // Use the outer part of the circle to keep clear of central attacks.
                float margin = soak == 1 ? 5.5f : soak == 2 ? 4.2f : 1.0f;
                var candidate = p + east * (margin * Side);
                if (spot.HasValue && Vector2.Distance(spot.Value,candidate) > 1f)
                { Show("SOAK" + soak, "SOAK " + soak + " - FIND YOUR SIDE PUDDLE", null); return; }
                spot = candidate;
            }
        }
        Show("SOAK" + soak, "SOAK " + soak + " - YOUR SIDE PUDDLE", spot);
    }

    private void Show(string key, string label, Vector2? p)
    {
        Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), (number == 0 ? "" : number + "   ") + label);
        if (Controller.TryGetElementByName("Stand", out var e))
        {
            e.Enabled = p.HasValue;
            if (p is Vector2 pos) { e.refX = pos.X; e.refY = pos.Y; e.overlayText = label; }
        }
        Call(key);
    }

    private void Call(string key)
    {
        if (announced.Add(key) && voice != null) _ = Send(voice, key);
    }

    private async Task Send(HttpClient client, string key)
    {
        try
        {
            using var body = new StringContent("MAGGIE_TEA_WORMHOLE|" + key, Encoding.UTF8, "text/plain");
            using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            voiceStatus = "Sent";
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or ObjectDisposedException)
        { voiceStatus = "Check Triggernometry endpoint localhost:51423"; }
    }

    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Tuufless 09STOP. D3 west Enumeration (melee swap).");
        if (ImGui.Button("Test Wormhole voice")) _ = TestVoice();
        ImGui.TextUnformatted(voiceStatus);
    }
    private async Task TestVoice()
    {
        using var test = NewClient();
        await Send(test, "TEST");
    }

    public override void OnReset()
    {
        deadline = lastDash = 0;
        number = justiceSide = wave = dashes = 0;
        directed = spread = jumpCast = jump = ray = blades = heatCast = heatHit = false;
        south = east = Vector2.Zero;
        announced.Clear();
        if (Controller.TryGetElementByName("Stand", out var e)) e.Enabled = false;
    }
    public override void OnCombatEnd() => OnReset();
    public override void OnDisable() { OnReset(); voice?.Dispose(); voice = null; }
}
