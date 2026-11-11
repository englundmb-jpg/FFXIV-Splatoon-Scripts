// Ulti Strats FRU, Phase 1 slides 3-6. True north, R1 physical ranged.
using Dalamud.Bindings.ImGui;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.FRU;

public sealed class FRU_P1_R1_Cyclonic : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 1238 };
    public override Metadata Metadata => new(1, "Maggie accessibility");
    private readonly HttpClient client = new(new HttpClientHandler { UseProxy = false })
        { Timeout = TimeSpan.FromSeconds(1) };
    private Vector2 center;
    private bool fire, preview;
    private int wave;
    private long expires, lastRepeat;
    private string status = "";
    private static long Now => Environment.TickCount64;

    public override void OnSetup()
    {
        Controller.RegisterElement("Now", new Element(0) { radius = 0.9f, thicc = 6,
            color = 0xFF00FF00, tether = true, Enabled = false });
        Controller.RegisterElement("Next", new Element(0) { radius = 0.75f, thicc = 3,
            color = 0xFFFFFF00, Enabled = false });
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId is not (40144 or 40148 or 40329 or 40330)) return;
        if (!preview && expires > Now && wave == 0) return;
        var caster = Svc.Objects.SearchById(source);
        if (caster == null) return;
        center = new Vector2(caster.Position.X, caster.Position.Z);
        Begin(castId is 40144 or 40329, false);
    }

    private void Begin(bool isFire, bool isPreview)
    {
        fire = isFire;
        preview = isPreview;
        wave = 0;
        lastRepeat = 0;
        expires = Now + (preview ? 5000 : 16500);
        Draw();
        _ = Say(preview ? "TEST" : fire ? "CLOCK_FIRE" : "CLOCK_LIGHTNING");
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (expires <= Now || preview || set.Action is not { } action) return;
        if (action.RowId == 40145 && wave == 0) wave = 1;
        else if (action.RowId == 40146 && wave > 0 && Now - lastRepeat > 1100)
        {
            lastRepeat = Now;
            wave++;
        }
        else return;
        if (wave == 4) { OnReset(); return; }
        Draw();
        _ = Say(wave == 1 ? (fire ? "PAIR" : "SPREAD") : wave == 2 ? "BACK" : "STEP");
    }

    private void Draw()
    {
        // Staging radii are approximations of the slides. R1 and T1 meet at NNW.
        float radius = fire ? 7f : 15f;
        Vector2 bait = center + new Vector2(-0.707107f, -0.707107f) * radius;
        Vector2 dodge = center + new Vector2(-0.382683f, -0.923880f) * radius;
        bool atClock = wave == 0 || wave == 2;
        Mark("Now", atClock ? bait : dodge, atClock ? "NW" : "CLOCKWISE");
        Mark("Next", atClock ? dodge : bait, "NEXT");
        if (wave == 3 && Controller.TryGetElementByName("Next", out var next)) next.Enabled = false;
    }

    private void Mark(string key, Vector2 point, string label)
    {
        if (!Controller.TryGetElementByName(key, out var e)) return;
        e.refX = point.X; e.refY = point.Y;
        e.overlayText = preview ? "PREVIEW " + label : label;
        e.Enabled = true;
    }

    private async Task Say(string key)
    {
        try
        {
            using var body = new StringContent("MAGGIE_FRU_R1_CYCLONIC|" + key, Encoding.UTF8, "text/plain");
            using var reply = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
            status = reply.IsSuccessStatusCode ? "Sent to Triggernometry" : "Check Triggernometry endpoint";
        }
        catch (HttpRequestException) { status = "Check Triggernometry endpoint"; }
        catch (OperationCanceledException) { status = "Voice request timed out"; }
    }

    public override void OnUpdate()
    {
        if (expires != 0 && (Now >= expires || BasePlayer == null || BasePlayer.CurrentHp == 0)) OnReset();
    }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("R1 northwest. Green = current, cyan = next. Test in FRU before pulling.");
        if (expires == 0 && ImGui.Button("Test Cyclonic voice and markers"))
        {
            center = new Vector2(100, 100);
            Begin(true, true);
        }
        ImGui.TextUnformatted(status);
    }
    public override void OnReset()
    {
        expires = 0;
        foreach (var e in Controller.GetRegisteredElements().Values) e.Enabled = false;
    }
    public override void OnCombatEnd() => OnReset();
    public override void OnDisable() => OnReset();
}
