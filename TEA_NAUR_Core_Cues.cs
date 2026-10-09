// Small NAUR reminders using the same local voice bridge as UCOB Blackfire.
// See TEA_NAUR_R1_README.md. Event IDs checked against BossMod and cactbot.
using Dalamud.Bindings.ImGui;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.TEA;

public sealed class TEA_NAUR_Core_Cues : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories => [887];
    public override Metadata Metadata => new(1, "Maggie accessibility repair");
    private HttpClient? voice;
    private volatile string voiceStatus = "Not tested";
    private bool enableVoice = true;
    private string current = "";
    private uint lastCast;
    private long until, lastAt;
    private bool motionCue;

    public override void OnEnable()
    {
        voice = new HttpClient(new HttpClientHandler { UseProxy = false })
        { Timeout = TimeSpan.FromMilliseconds(700) };
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        var now = Environment.TickCount64;
        if(castId == lastCast && now - lastAt < 1500) return;
        var cue = CueForCast(castId);
        if(cue.Code == "") return;
        lastCast = castId;
        lastAt = now;
        motionCue = castId is 18558 or 18559;
        Show(cue.Text, cue.Code, motionCue ? 6000 : 4500);
    }

    private static (string Text, string Code) CueForCast(uint id) => id switch
    {
        18558 => ("KEEP MOVING", "MOVE"),
        18559 => ("STOP MOVEMENT + ALL ACTIONS", "STOP"),
        18486 => ("NISI: CHECK PARTNER — WAIT FOR SAFE PASS", "NISI"),
        18491 => ("CHECK FINAL NISI SYMBOL", "VERDICT"),
        _ => ("", "")
    };

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        // Stop displaying ordinary Motion/Stillness after its success/failure packet.
        // Fate Calibration previews use separate scripts and are deliberately not voiced here.
        if(motionCue && set.Action is { RowId: 19039 or 19040 or 18560 or 18561 })
        {
            current = "";
            until = 0;
            motionCue = false;
        }
    }

    private void Show(string text, string code, int duration)
    {
        current = text;
        until = Environment.TickCount64 + duration;
        if(enableVoice) _ = SayAsync(code);
    }

    public override void OnUpdate()
    {
        if(BasePlayer == null || BasePlayer.CurrentHp == 0) { OnReset(); return; }
        if(current != "" && Environment.TickCount64 < until)
            Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), current);
    }

    private async Task SayAsync(string code)
    {
        var client = voice;
        if(client == null) return;
        try
        {
            using var body = new StringContent("MAGGIE_TEA|" + code, Encoding.UTF8, "text/plain");
            using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            voiceStatus = "Sent to Triggernometry";
        }
        catch(Exception) { voiceStatus = "No voice connection: check Triggernometry endpoint"; }
    }

    public override void OnReset()
    {
        current = "";
        until = lastAt = 0;
        lastCast = 0;
        motionCue = false;
    }
    public override void OnCombatEnd() => OnReset();
    public override void OnDisable()
    {
        OnReset();
        voice?.Dispose();
        voice = null;
    }
    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Four short reminders: ordinary Motion, ordinary Stillness, Photon Nisi partner check, and Verdict final symbol check. No automatic movement or action cancellation.");
        ImGui.TextWrapped("Photon is a CHECK reminder, not permission to pass. Second R1 pass waits for the CC tank's mines.");
        ImGui.TextWrapped("Voice uses Triggernometry_TEA_NAUR_Core_Cues.xml and http://localhost:51423/, as in UCOB Blackfire. Disable duplicate voice cues elsewhere.");
        ImGui.Checkbox("Voice (this session)", ref enableVoice);
        ImGui.TextWrapped("Voice: " + voiceStatus);
        if(ImGui.Button("TEST VOICE")) Show("TEST — TEA CUES", "TEST", 3000);
        if(ImGui.Button("HIDE / RESET")) OnReset();
    }
}
