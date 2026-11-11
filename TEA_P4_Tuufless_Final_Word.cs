// Tuufless Final Word. The beacons determine march direction. This helper only guides the setup.
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
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

public sealed class TEA_P4_Tuufless_Final_Word : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 887 };
    public override Metadata Metadata => new(1, "Maggie accessibility");
    private const uint Light = 2152, LightBeacon = 2153, Dark = 2154, DarkBeacon = 2155;
    private long deadline;
    private bool assigned, spoken, paused;
    private HttpClient? voice;
    private static readonly string[] Directions = { "EAST", "SOUTHEAST", "SOUTH", "SOUTHWEST",
        "WEST", "NORTHWEST", "NORTH", "NORTHEAST" };

    public override void OnSetup() => Controller.RegisterElement("Stand", new Element(0)
    {
        Enabled = false, radius = 0.7f, thicc = 6, tether = true, color = 0xFF00FF00,
        overlayVOffset = 1.4f, overlayFScale = 1.5f
    });

    public override void OnEnable() => voice = NewClient();
    private static HttpClient NewClient() => new(new HttpClientHandler { UseProxy = false })
        { Timeout = TimeSpan.FromSeconds(1) };

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == 18557)
        {
            OnReset();
            deadline = Environment.TickCount64 + 25000;
        }
        if (deadline != 0 && castId == 18559) { paused = true; Hide(); }
    }
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (set.Action?.RowId is 19040 or 18561) paused = false;
    }

    public override void OnUpdate()
    {
        if (deadline == 0) return;
        var me = BasePlayer;
        if (Environment.TickCount64 >= deadline || me == null || me.CurrentHp == 0)
        { OnReset(); return; }
        var role = me.StatusList.FirstOrDefault(s => s.StatusId is Light or LightBeacon or Dark or DarkBeacon)?.StatusId ?? 0;
        if (role == 0) { if (assigned) OnReset(); return; }
        assigned = true;
        if (paused || !Controller.TryGetElementByName("Stand", out var marker)) return;

        var target = role switch
        {
            LightBeacon => new Vector2(86,100),
            DarkBeacon => new Vector2(112.5f,100),
            _ => new Vector2(110,100)
        };
        if (role is Light or Dark)
        {
            var beacon = Svc.Objects.OfType<IPlayerCharacter>().FirstOrDefault(p =>
                p.CurrentHp > 0 && p.StatusList.Any(s => s.StatusId == DarkBeacon));
            // Use the staging spot until the dark beacon arrives at the east station.
            if (beacon != null && beacon.Position.X >= 110 && Math.Abs(beacon.Position.Z - 100) <= 4)
                target = new Vector2(beacon.Position.X - 2.5f, beacon.Position.Z);
        }
        string assignment = role == LightBeacon ? "LIGHT BEACON" : role == DarkBeacon ? "DARK BEACON" : "WEST OF DARK BEACON";
        var delta = target - new Vector2(me.Position.X, me.Position.Z);
        string direction = delta.LengthSquared() <= 1 ? "HERE" :
            Directions[((int)Math.Round(Math.Atan2(delta.Y, delta.X) / (Math.PI / 4)) + 8) % 8];
        marker.refX = target.X;
        marker.refY = target.Y;
        marker.overlayText = assignment;
        marker.Enabled = true;
        string text = direction == "HERE" ? "IN POSITION" : "GO " + direction;
        Controller.DisplayAttentionWindowLine(new Vector4(0,1,0,1), text + " - " + assignment);
        if (spoken || voice == null) return;
        spoken = true;
        string group = role == LightBeacon ? "LIGHT" : role == DarkBeacon ? "DARK" : "GROUP";
        _ = Send(voice, group + "_" + direction);
    }

    private static async Task Send(HttpClient client, string code)
    {
        try
        {
            using var body = new StringContent("MAGGIE_TEA_FINAL_WORD|" + code, Encoding.UTF8, "text/plain");
            using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is HttpRequestException or OperationCanceledException or ObjectDisposedException) { }
    }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("True compass directions to the setup spot. Your existing core cues handle Stillness and Motion.");
        if (ImGui.Button("Test Final Word voice")) _ = Test();
    }
    private static async Task Test()
    {
        using var client = NewClient();
        await Send(client, "TEST");
    }
    private void Hide()
    {
        if (Controller.TryGetElementByName("Stand", out var marker)) marker.Enabled = false;
    }
    public override void OnReset()
    {
        deadline = 0;
        assigned = spoken = paused = false;
        Hide();
    }
    public override void OnCombatEnd() => OnReset();
    public override void OnDisable() { OnReset(); voice?.Dispose(); voice = null; }
}
