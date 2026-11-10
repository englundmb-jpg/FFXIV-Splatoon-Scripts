// Tuufless D3: https://ffxiv.tuufless.com/elemental/tea/03_alex_prime/#inception-formation
using Dalamud.Game.ClientState.Objects.Types;
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

public sealed class TEA_P3_D3_Tuufless_Inception : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 887 };
    public override Metadata Metadata => new(2, "Maggie accessibility");
    private enum Step { Off, Drops, Gather, Sacrament, Swords, Escape }
    private Step step;
    private readonly Dictionary<uint, uint> tetherTargets = new();
    private readonly Dictionary<uint, Vector2> orbStarts = new();
    private static readonly Vector2 Center = new(100, 100);
    private long deadline, lastSword, lastFlare;
    private int flares;
    private int alexSide, justiceSide, swords;
    private uint debuff;
    private Vector2? heartSample, heartDestination;
    private int clearedSide;
    private bool mySwordTaken;
    private bool crystalIcon, crystalDropped, ownOrbDone;
    private string spoken = "";
    private HttpClient? voice;

    public override void OnSetup()
    {
        for (int i = 0; i < 5; i++)
            Controller.RegisterElement("Spot" + i, new Element(0)
            {
                Enabled = false, radius = 0.8f, thicc = 6,
                color = i == 0 ? 0xFF00FF00u : 0xFFFFFF00u,
                tether = i == 0, overlayFScale = 1.5f
            });
    }

    public override void OnEnable() =>
        voice = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(1) };

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == 18543)
        {
            OnReset();
            step = Step.Drops;
            deadline = Environment.TickCount64 + 90000;
        }
        if (step == Step.Off) return;
        if (castId == 18526 && (step is Step.Drops or Step.Gather)) step = Step.Sacrament;
        if (castId is 19072 or 18542) OnReset();
    }

    public override void OnTetherCreate(uint source, uint target, uint data2, uint data3, uint data5)
    {
        if (step != Step.Drops || data3 != 12) return;
        tetherTargets[source] = target;
        RememberOrb(source);
    }

    public override void OnActorControl(uint sourceId, uint command, uint p1, uint p2, uint p3,
        uint p4, uint p5, uint p6, uint p7, uint p8, ulong targetId, byte replaying)
    {
        if (step == Step.Drops && command == 502 && p1 == 96 && sourceId == BasePlayer?.EntityId)
            crystalIcon = true;
    }

    private void RememberOrb(uint id)
    {
        if (orbStarts.ContainsKey(id)) return;
        var orb = Svc.Objects.OfType<IBattleNpc>().FirstOrDefault(n => n.EntityId == id && n.DataId == 0x2C50);
        if (orb != null) orbStarts[id] = new(orb.Position.X, orb.Position.Z);
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (step == Step.Off) return;
        switch (set.Action?.RowId)
        {
            case 18524: crystalDropped = true; break;
            case 18546:
                if (set.Source != null && tetherTargets.TryGetValue(set.Source.EntityId, out var orbTargetId)
                    && orbTargetId == BasePlayer?.EntityId) ownOrbDone = true;
                break;
            case 18540 when step is Step.Gather or Step.Drops:
                if (Environment.TickCount64 - lastFlare > 500)
                {
                    lastFlare = Environment.TickCount64;
                    flares++;
                }
                break;
            case 18527: step = Step.Swords; break;
            case 18539 when step == Step.Swords:
                if (Environment.TickCount64 - lastSword < 400) break;
                lastSword = Environment.TickCount64;
                swords++;
                foreach (var hit in set.TargetEffects)
                {
                    if (hit.TargetID == BasePlayer?.EntityId) mySwordTaken = true;
                    var player = Svc.Objects.FirstOrDefault(o => o.EntityId == hit.TargetID);
                    if (player != null && Math.Abs(player.Position.X - 100) < 2 &&
                        Math.Abs(player.Position.Z - 100) > 1)
                        clearedSide = Math.Sign(player.Position.Z - 100);
                }
                if (swords >= 3 || (clearedSide != 0 && (debuff != 1124 || mySwordTaken)))
                    step = Step.Escape;
                break;
            case 18506: OnReset(); break;
        }
    }

    public override void OnUpdate()
    {
        if (step == Step.Off) return;
        Hide();
        var me = BasePlayer;
        if (me == null || me.CurrentHp == 0 || Environment.TickCount64 >= deadline) { OnReset(); return; }
        if (step is Step.Drops or Step.Gather or Step.Sacrament)
            foreach (var status in me.StatusList)
                if (status.StatusId is 1121 or 1122 or 1124) debuff = status.StatusId;

        if (step == Step.Drops)
        {
            foreach (var id in tetherTargets.Keys) RememberOrb(id);
            var own = tetherTargets.FirstOrDefault(t => t.Value == me.EntityId).Key;
            bool crystal = own == 0 && (crystalIcon ||
                (orbStarts.Count == 4 && tetherTargets.Values.Distinct().Count() == 4));
            if ((crystal && crystalDropped) || (own != 0 && ownOrbDone)) step = Step.Gather;
            ReadBossSides();
            if (step == Step.Drops && alexSide != 0)
            {
                if (own != 0) DrawOrb(own);
                else if (crystal) DrawCrystals();
            }
        }
        if (step == Step.Gather)
        {
            ReadBossSides();
            if (flares >= 3) Cue("HEART", "FIND THE GOLD HEART");
            else if (flares == 2) Cue("THIRD", "HOLD WITH GROUP - THIRD FLARE");
            else Cue("GATHER", "EDGE TO GROUP - STAY BEHIND TANK BAITS");
            if (flares < 3 && justiceSide != 0 && alexSide != 0)
                Spot(0, Center + new Vector2(justiceSide * 13, alexSide * 13), "GROUP");
        }
        if (step == Step.Sacrament)
        {
            TrackHeart();
            if (heartDestination is Vector2 end && debuff != 0)
            {
                var facing = Vector2.Normalize(end - Center);
                var right = new Vector2(-facing.Y, facing.X);
                Spot(0, Center + right * (debuff == 1122 ? -18f : 18f), debuff == 1122 ? "LEFT" : "RIGHT");
            }
            if (debuff == 1122) Cue("LEFT", "FACE HEART DESTINATION - LEFT WITH TANKS");
            if (debuff == 1124) Cue("FAR", "FACE HEART DESTINATION - RIGHT, STRETCH BLUE");
            if (debuff == 1121) Cue("RIGHT", "FACE HEART DESTINATION - RIGHT, AVOID DAMAGE");
        }
        if (step == Step.Swords && debuff != 0)
        {
            bool bait = debuff == 1124;
            Cue(bait ? "BAIT" : "EAST", bait ? "WEST OF CHASER - BAIT CLEAVE" : "EAST OF CHASER - GIVE HEALERS SPACE");
            Spot(0, Center + new Vector2(bait ? -2.5f : 7, 0), bait ? "BAIT" : "GROUP");
        }
        if (step == Step.Escape)
        {
            Cue("JUMP", "AWAY FROM EAST TANK - SUPER JUMP");
            if (clearedSide != 0) Spot(0, Center + new Vector2(0, 4 * clearedSide), "CLEARED SIDE");
        }
    }

    // During Inception the gold heart travels in a straight line to the new boss position.
    private void TrackHeart()
    {
        if (heartDestination.HasValue) return;
        var heart = Svc.Objects.OfType<IBattleNpc>().FirstOrDefault(n => n.DataId == 0x2C52);
        if (heart == null) return;
        var pos = new Vector2(heart.Position.X, heart.Position.Z);
        if (heartSample is not Vector2 previous) { heartSample = pos; return; }
        if (Vector2.Distance(previous, pos) < 0.8f) return;
        heartSample = pos;
        var dir = Vector2.Normalize(pos - previous);
        var offset = pos - Center;
        var projection = Vector2.Dot(offset, dir);
        var discriminant = projection * projection + 21.5f * 21.5f - offset.LengthSquared();
        if (discriminant <= 0) return;
        float distance = -projection + MathF.Sqrt(discriminant);
        if (distance > 0) heartDestination = pos + dir * distance;
    }

    private void ReadBossSides()
    {
        if ((tetherTargets.Count == 0 && !crystalIcon) || (alexSide != 0 && justiceSide != 0)) return;
        foreach (var boss in Svc.Objects.OfType<IBattleNpc>())
        {
            var x = boss.Position.X - 100;
            var z = boss.Position.Z - 100;
            if (alexSide == 0 && boss.DataId == 0x2C53 && Math.Abs(x) < 3 && Math.Abs(z) > 12)
                alexSide = Math.Sign(z);
            if (justiceSide == 0 && boss.DataId == 0x2C4C && Math.Abs(z) < 3 && Math.Abs(x) > 12)
                justiceSide = Math.Sign(x);
        }
    }

    private void DrawOrb(uint own)
    {
        if (!orbStarts.TryGetValue(own, out var orb)) return;
        int side = Math.Sign(100 - orb.X);
        var neighbors = orbStarts.Where(o => o.Key != own && Math.Sign(100 - o.Value.X) == side).ToArray();
        if (neighbors.Length != 1) return;
        bool cardinal = (orb.Y - neighbors[0].Value.Y) * alexSide > 0;
        var offset = cardinal ? new Vector2(side * 18.8f, 0) : new Vector2(side * 11.4f, alexSide * 14.9f);
        string direction = (cardinal ? "" : alexSide < 0 ? "NORTH" : "SOUTH") + (side < 0 ? "WEST" : "EAST");
        Cue("ORB_" + direction, "ORB - " + direction);
        Spot(0, Center + offset, direction);
    }

    private void DrawCrystals()
    {
        string side = alexSide < 0 ? "SOUTH" : "NORTH";
        Cue("CRYSTAL_" + side, "CRYSTAL " + side + " - TAKE AN OPEN CORNER");
        // Tuufless assigns the square to the four marked players; D3 has no fixed corner.
        int i = 1;
        foreach (float depth in new[] { 7.0f, 17.4f })
            foreach (float x in new[] { -5.2f, 5.2f })
                Spot(i++, Center + new Vector2(x, -alexSide * depth), "");
    }

    private void Spot(int i, Vector2 point, string label)
    {
        if (!Controller.TryGetElementByName("Spot" + i, out var e)) return;
        e.refX = point.X; e.refY = point.Y; e.overlayText = label; e.Enabled = true;
    }

    private void Cue(string code, string label)
    {
        Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), label);
        if (spoken == code) return;
        spoken = code;
        if (voice != null) _ = Speak(voice, code);
    }

    private static async Task Speak(HttpClient client, string code)
    {
        try
        {
            using var body = new StringContent("MAGGIE_TEA_INCEPTION|" + code, Encoding.UTF8, "text/plain");
            using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
        }
        catch (HttpRequestException) { }
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
    }

    private void Hide()
    {
        foreach (var e in Controller.GetRegisteredElements().Values) e.Enabled = false;
    }

    public override void OnReset()
    {
        step = Step.Off;
        tetherTargets.Clear(); orbStarts.Clear();
        alexSide = justiceSide = swords = flares = 0; lastFlare = 0;
        heartSample = heartDestination = null; clearedSide = 0; mySwordTaken = false;
        debuff = 0; lastSword = 0; spoken = "";
        crystalIcon = crystalDropped = ownOrbDone = false;
        Hide();
    }

    public override void OnDisable() { OnReset(); voice?.Dispose(); voice = null; }
}
