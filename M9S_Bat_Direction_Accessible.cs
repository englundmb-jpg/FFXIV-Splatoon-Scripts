using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;

namespace MaggieScripts.Duties.Dawntrail;

public class M9S_Bat_Direction_Accessible : SplatoonScript
{
    public override Metadata Metadata => new(2, "Maggie accessibility");
    public override HashSet<uint>? ValidTerritories => [];

    private HttpClient? speechClient;
    private int lastSpokenDirection;
    private volatile string speechStatus = "No direction sent yet";

    private uint batId;
    private bool armed;
    private readonly M9SBatMotion motion = new();
    private long testUntil;
    private string testText = "";

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("Direction", """{"Name":"Bat direction","type":1,"Enabled":false,"radius":0.0,"thicc":0.0,"refActorType":1,"overlayText":"","overlayTextColor":4294967040,"overlayBGColor":4278190080,"overlayFScale":4.0,"overlayVOffset":2.0}""");
    }

    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Shows observed bat movement, not an advance prediction. CW/CCW is around the arena centre. No safe-spot guidance.");
        if (ImGui.Button("TEST CW (5 seconds)")) StartTest("TEST: CW");
        ImGui.SameLine();
        if (ImGui.Button("TEST CCW (5 seconds)")) StartTest("TEST: CCW");
        if (ImGui.Button("Clear test")) testUntil = 0;
        ImGui.TextWrapped("Test buttons work outside M9S only. Cyan text on black appears above your character.");
        ImGui.TextWrapped("Voice requires the Maggie M9S Bat Direction trigger folder in Triggernometry. Uses its local endpoint at localhost:51423. Test buttons also send voice cues.");
        ImGui.TextWrapped($"Voice connection: {speechStatus}");
        ImGui.TextUnformatted($"Bat: {(batId == 0 ? "not linked" : batId.ToString("X8"))}; movement tracking: {armed}");
    }

    private void StartTest(string text)
    {
        if (Svc.ClientState.TerritoryType == 1321) return;
        testText = text;
        testUntil = Environment.TickCount64 + 5000;
        _ = SendSpeechAsync(text == "TEST: CW" ? "CW" : "CCW");
    }

    public override void OnTetherCreate(uint source, uint target, uint data2, uint data3, uint data5)
    {
        if (Svc.ClientState.TerritoryType != 1321 || source != BasePlayer?.EntityId || (data3 != 353 && data3 != 354)) return;
        foreach (var actor in Svc.Objects)
        {
            if (actor.EntityId != target || actor.BaseId != 0x4C2F) continue;
            if (batId != target)
            {
                motion.Reset();
                lastSpokenDirection = 0;
            }
            batId = target;
            return;
        }
    }

    public override void OnTetherRemoval(uint source, uint data2, uint data3, uint data5)
    {
        if (source != BasePlayer?.EntityId || (data3 != 353 && data3 != 354)) return;
        batId = 0;
        motion.Reset();
        lastSpokenDirection = 0;
        Show("");
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (Svc.ClientState.TerritoryType != 1321) return;
        if (castId == 45984)
        {
            OnReset();
        }
        else if (castId == 45988)
        {
            armed = true;
            motion.Reset();
            lastSpokenDirection = 0;
            Show("");
        }
        else if (source == batId && castId is 45992 or 45993 or 45994 or 45995)
        {
            armed = false;
            motion.Reset();
            lastSpokenDirection = 0;
            Show("");
        }
    }

    public override void OnUpdate()
    {
        Show("");
        if (Svc.ClientState.TerritoryType != 1321)
        {
            batId = 0;
            armed = false;
            motion.Reset();
            lastSpokenDirection = 0;
            if (Environment.TickCount64 < testUntil) Show(testText);
            return;
        }
        testUntil = 0;
        if (!armed || batId == 0) return;
        if (BasePlayer == null || BasePlayer.CurrentHp == 0)
        {
            OnReset();
            return;
        }
        foreach (var actor in Svc.Objects)
        {
            if (actor.EntityId != batId || actor.BaseId != 0x4C2F) continue;
            var direction = motion.Sample(actor.Position.X, actor.Position.Z);
            Show(direction > 0 ? "CW" : direction < 0 ? "CCW" : "");
            if (direction != lastSpokenDirection)
            {
                lastSpokenDirection = direction;
                if (direction != 0) _ = SendSpeechAsync(direction > 0 ? "CW" : "CCW");
            }
            return;
        }
        batId = 0;
        motion.Reset();
        lastSpokenDirection = 0;
    }

    public override void OnReset()
    {
        batId = 0;
        armed = false;
        testUntil = 0;
        motion.Reset();
        lastSpokenDirection = 0;
        Show("");
    }

    private async Task SendSpeechAsync(string direction)
    {
        try
        {
            speechClient ??= new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                Timeout = TimeSpan.FromSeconds(1)
            };
            using var body = new StringContent("MAGGIE_M9S_BATS|" + direction, Encoding.UTF8, "text/plain");
            using var response = await speechClient.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            speechStatus = "Delivered to Triggernometry (" + direction + ")";
        }
        catch (Exception)
        {
            speechStatus = "Not connected - check Triggernometry endpoint localhost:51423";
        }
    }

    public override void OnDisable()
    {
        OnReset();
        speechClient?.Dispose();
        speechClient = null;
    }

    private void Show(string text)
    {
        if (!Controller.TryGetElementByName("Direction", out var element)) return;
        element.overlayText = text;
        element.Enabled = text.Length != 0;
    }
}

public sealed class M9SBatMotion
{
    private bool hasAnchor;
    private double anchor;
    public int Direction { get; private set; }

    public void Reset()
    {
        hasAnchor = false;
        Direction = 0;
    }

    public int Sample(float x, float z)
    {
        if (!float.IsFinite(x) || !float.IsFinite(z) || (x == 100f && z == 100f))
        {
            Reset();
            return 0;
        }
        double angle = Math.Atan2(x - 100.0, 100.0 - z);
        if (!hasAnchor)
        {
            anchor = angle;
            hasAnchor = true;
            return 0;
        }
        double delta = Math.Atan2(Math.Sin(angle - anchor), Math.Cos(angle - anchor));
        if (Math.Abs(delta) < Math.PI / 180.0) return Direction;
        anchor = angle;
        if (Math.Abs(delta) >= Math.PI / 2.0)
        {
            Direction = 0;
            return 0;
        }
        Direction = delta > 0 ? 1 : -1;
        return Direction;
    }
}
