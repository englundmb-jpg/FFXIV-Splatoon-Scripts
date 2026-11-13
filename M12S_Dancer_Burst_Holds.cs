// Personal prog holds: Replication I and the Idyllic elemental tower sequence.
// Uses the existing M12_ID_R1 Triggernometry voice receiver.
using Dalamud.Bindings.ImGui;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.M12S;

public class M12S_Dancer_Burst_Holds : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 1327 };
    public override Metadata Metadata => new(1, "Maggie accessibility");
    private const uint TechnicalStep = 15998, Devilment = 16011;
    private enum Window { None, Replication, Towers }
    private Window window;
    private bool replicationSeen, opening, kickPassed, holdCalled, waitingForBoth;
    private long expires, nextCheck, captionUntil;
    private string caption = "";
    private volatile string voiceStatus = "Ready";
    private static readonly HttpClient Client = new(new HttpClientHandler { UseProxy = false })
        { Timeout = TimeSpan.FromSeconds(3) };

    public override void OnReset()
    {
        replicationSeen = opening = kickPassed = holdCalled = waitingForBoth = false;
        window = Window.None;
        expires = nextCheck = captionUntil = 0;
        caption = "";
    }
    public override void OnCombatEnd() => OnReset();
    public override void OnDisable() => OnReset();

    // Arcadia Aflame arms the first Replication only. Kick separates its two clone volleys.
    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (BasePlayer?.ClassJob.RowId != 38) return;
        switch (set.Action?.RowId)
        {
            case 0xB528 when !replicationSeen: opening = true; break;
            case 0xB4D8 when opening:
                replicationSeen = true;
                opening = false;
                kickPassed = false;
                Begin(Window.Replication);
                break;
            case 0xB527 when window == Window.Replication: kickPassed = true; break;
            case 0xB4E0 when window == Window.Replication && kickPassed: Finish(); break;
            // Arcanum finishes the meteor preview before stack/defamation and actual tower soaks.
            case 0xB9D9: Begin(Window.Towers); break;
            case 0xB4FA or 0xB4F8 when window == Window.Towers: Finish(); break;
        }
    }

    private void Begin(Window which)
    {
        window = which;
        waitingForBoth = holdCalled = false;
        expires = Environment.TickCount64 + (which == Window.Towers ? 65000 : 42000);
    }

    private static unsafe float Remaining(uint id)
    {
        var actions = ActionManager.Instance();
        if (actions == null) return float.PositiveInfinity;
        if (!actions->IsRecastTimerActive(ActionType.Action, id)) return 0;
        return Math.Max(0, actions->GetRecastTime(ActionType.Action, id)
            - actions->GetRecastTimeElapsed(ActionType.Action, id));
    }

    private void Finish()
    {
        window = Window.None;
        if (!holdCalled) return;
        waitingForBoth = true;
        expires = Environment.TickCount64 + 125000;
        nextCheck = 0;
    }

    public override void OnUpdate()
    {
        long now = Environment.TickCount64;
        if (now < captionUntil) Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), caption);
        if (window == Window.None && !waitingForBoth) return;
        if (now > expires) { window = Window.None; waitingForBoth = false; return; }
        if (now < nextCheck || BasePlayer?.ClassJob.RowId != 38 || BasePlayer.CurrentHp == 0) return;
        nextCheck = now + 150;
        float tech = Remaining(TechnicalStep), devil = Remaining(Devilment);
        if (waitingForBoth && tech < 0.15f && devil <= 5f)
        {
            waitingForBoth = false;
            Call("Burst now. Technical Step.");
        }
        else if (window != Window.None && !holdCalled && Math.Min(tech, devil) <= 5f)
        {
            holdCalled = true;
            Call(window == Window.Towers ? "Hold burst for towers." : "Hold burst for Rep one.");
        }
    }

    private void Call(string words)
    {
        caption = words;
        captionUntil = Environment.TickCount64 + 4500;
        _ = Speak(words);
    }

    private async Task Speak(string words)
    {
        try
        {
            using var body = new StringContent("M12_ID_R1|" + words, Encoding.UTF8, "text/plain");
            using var response = await Client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            voiceStatus = "Sent to Triggernometry";
        }
        catch (Exception) { voiceStatus = "Check Triggernometry endpoint on localhost:51423"; }
    }

    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Personal Dancer holds: Replication I and Idyllic elemental towers.");
        if (ImGui.Button("TEST BURST VOICE")) _ = Speak("Dancer burst voice ready.");
        ImGui.TextWrapped(voiceStatus);
    }
}
