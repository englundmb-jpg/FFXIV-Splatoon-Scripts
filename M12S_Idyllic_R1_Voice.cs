// Companion to Maggie R1 DN Uptime — Green Guidance. North is slot 0.
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.DalamudServices.Legacy;
using ECommons.GameFunctions;
using ECommons.Hooks.ActionEffectTypes;
using FFXIVClientStructs.FFXIV.Client.Game;
using ECommons;
using Splatoon.Data;
using Splatoon.Utility;
using static Splatoon.Splatoon;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.M12S;

public unsafe class IdyllicR1Voice : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 1327 };
    public override Metadata Metadata => new(1, "Maggie accessibility");
    private int phase, cloneSlot = -1;
    private bool pickupCalled, dodgeCalled, towerCalled;
    private bool? safeNorth;
    private long scanAfter, captionUntil;
    private string caption = "";
    private readonly IdyllicSpeech speech = new();

    public override void OnReset()
    {
        phase = 0;
        cloneSlot = -1;
        safeNorth = null;
        pickupCalled = dodgeCalled = towerCalled = false;
        scanAfter = captionUntil = 0;
        caption = "";
    }

    public override void OnDisable() => OnReset();
    public override void OnCombatEnd() => OnReset();

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        switch (set.Action?.RowId)
        {
            case 46345:
                OnReset();
                phase = 1;
                break;
            case 48098 when phase > 0:
                phase++;
                break;
            case 46356 when phase == 7 && !towerCalled:
                towerCalled = true;
                Call("Your tower. West platform, ranged right.");
                break;
        }
    }

    public override void OnStartingCast(uint sourceId, PacketActorCast* packet)
    {
        if (phase == 0) return;
        if (packet->ActionDescriptor == new ActionDescriptor(ActionType.Action, 48098))
            phase++;
        if (phase is 3 or 4 && packet->ActionDescriptor == new ActionDescriptor(ActionType.Action, 46352))
            safeNorth = packet->Position.Z < 100;
    }

    private static int Slot(Vector3 p)
    {
        float x = p.X - 100, z = p.Z - 100;
        if (x * x + z * z < 16) return -1;
        return ((int)Math.Round(Math.Atan2(x, -z) * 4 / Math.PI) + 8) % 8;
    }

    private static int Quadrant(int slot) => slot / 2 + 1;

    public override void OnUpdate()
    {
        long now = Environment.TickCount64;
        if (now < captionUntil)
            Controller.DisplayAttentionWindowLine(new Vector4(0, 1, 0, 1), caption);
        if (phase == 0 || BasePlayer == null || now < scanAfter) return;
        scanAfter = now + 100;
        if (phase == 1 && cloneSlot < 0)
        {
            foreach (var clone in Svc.Objects.OfType<IBattleNpc>())
            {
                if (clone.DataId != 19210 || !clone.Struct()->Vfx.Tethers.ToArray()
                        .Any(t => t.TargetId.ObjectId == BasePlayer.ObjectId)) continue;
                cloneSlot = Slot(clone.Position);
                if (cloneSlot < 0) continue;
                string kind = cloneSlot < 4 ? "stack" : "defamation";
                Call($"Clone quadrant {Quadrant(cloneSlot)}. Get {kind} {cloneSlot % 4 + 1}.");
                break;
            }
        }
        if (phase is 5 or 6 && !pickupCalled && cloneSlot >= 0) ReadPickup();
        if (phase == 7 && !towerCalled && !dodgeCalled)
        {
            dodgeCalled = true;
            Call(safeNorth.HasValue
                ? (safeNorth.Value ? "Dodge northwest." : "Dodge southwest.")
                : "Dodge the cones.");
        }
    }

    private void ReadPickup()
    {
        // Require all eight distinct tether sources, so partial spawns cannot change the ordinal.
        var kinds = new uint[8];
        var party = Controller.GetPartyMembers().Select(p => p.ObjectId).ToHashSet();
        foreach (var boss in Svc.Objects.OfType<IBattleNpc>())
        {
            if (boss.NameId != 14380) continue;
            int slot = Slot(boss.Position);
            if (slot < 0) continue;
            foreach (var t in boss.Struct()->Vfx.Tethers.ToArray())
            {
                if (t.Id is not (368 or 369) || !party.Contains(t.TargetId.ObjectId)) continue;
                if (kinds[slot] != 0) return;
                kinds[slot] = t.Id;
            }
        }
        if (kinds.Count(k => k == 368) != 4 || kinds.Count(k => k == 369) != 4) return;
        bool stack = cloneSlot < 4;
        uint wanted = stack ? 369u : 368u;
        int target = Enumerable.Range(0, 8).Where(s => kinds[s] == wanted).ElementAt(cloneSlot % 4);
        string side = target < 4 ? "acting east" : "acting west";
        string[] directions = { "north", "northeast", "east", "southeast", "south", "southwest", "west", "northwest" };
        pickupCalled = true;
        Call($"Pick up {(stack ? "stack" : "defamation")}. Quadrant {Quadrant(target)}, {directions[target]}. {side}.");
    }

    private void Call(string words)
    {
        caption = words;
        captionUntil = Environment.TickCount64 + 5500;
        _ = speech.Send(words);
    }

    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("R1 DN Uptime. West platform, ranged right. Keep your green guidance preset enabled.");
        if (ImGui.Button("TEST ID VOICE")) _ = speech.Send("Idyllic voice ready.");
        ImGui.TextWrapped(speech.Status);
    }
}

internal sealed class IdyllicSpeech
{
    private static readonly HttpClient Client = new(new HttpClientHandler { UseProxy = false })
        { Timeout = TimeSpan.FromSeconds(3) };
    public volatile string Status = "Ready";
    public async Task Send(string words)
    {
        try
        {
            using var body = new StringContent("M12_ID_R1|" + words, Encoding.UTF8, "text/plain");
            using var response = await Client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            Status = "Sent to Triggernometry";
        }
        catch (Exception) { Status = "Check Triggernometry endpoint on localhost:51423"; }
    }
}
