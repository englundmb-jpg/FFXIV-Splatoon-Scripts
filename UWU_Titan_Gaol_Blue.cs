using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UWU_Titan_Gaol_Blue : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } = new();
    public override Metadata? Metadata => new(3, "Maggie - blue circle movement cue");

    private static readonly Vector3 Center = new(100f, 0f, 100f);
    private readonly HashSet<ulong> selected = new();
    private Vector3 towardTitan;
    private uint titanId;
    private bool armed;
    private bool landslideResolved;
    private long armedAt;
    private long previewUntil;
    private readonly ulong[] markersAtStart = new ulong[3];
    private int personalIndex = -1;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("JailSpot",
            """
            {"Name":"Your jail destination","Enabled":false,"type":0,"radius":0.6,"Filled":false,"color":4294940979,"thicc":3.0}
            """);
        Controller.RegisterElementFromCode("JailText",
            """
            {"Name":"Jail instruction","Enabled":false,"type":1,"refActorType":1,"radius":0.0,"thicc":0.0,"overlayVOffset":2.0,"overlayFScale":2.0,"overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayText":""}
            """);
        for (var i = 0; i < 3; i++)
            Controller.RegisterElementFromCode($"JailPreview{i}",
                """
                {"Name":"Jail position preview","Enabled":false,"type":0,"radius":0.6,"Filled":false,"color":4294940979,"thicc":5.0,"overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}
                """);
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (Svc.ClientState.TerritoryType != 777 || castId != 0x2B67)
            return;

        var titan = Svc.Objects.OfType<IBattleNpc>()
            .FirstOrDefault(x => x.EntityId == source && x.DataId == 8727);
        if (titan == null)
            return;

        Arm(titan);
    }

    private void Arm(IBattleNpc titan)
    {
        OnReset();
        var delta = titan.Position - Center;
        delta.Y = 0;
        if (delta.LengthSquared() < 1f)
            return;

        towardTitan = Vector3.Normalize(delta);
        titanId = titan.EntityId;
        armedAt = Environment.TickCount64;
        for (uint i = 0; i < 3; i++)
            markersAtStart[i] = Marking.GetMarker(i);
        armed = true;
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (Svc.ClientState.TerritoryType != 777)
            return;

        var id = set.Action?.RowId ?? 0;
        if (!armed && id == 0x2B67 && set.Source is IBattleNpc titan && titan.DataId == 8727)
            Arm(titan);
        if (!armed)
            return;
        if (id == 0x2B6B || id == 0x2B6C)
        {
            foreach (var target in set.TargetEffects)
                selected.Add(target.TargetID);
        }
        else if (id == 0x2B6F && set.Source?.EntityId == titanId)
        {
            landslideResolved = true;
        }
        else if (id == 0x2B6E)
        {
            OnReset();
        }
    }

    public override void OnUpdate()
    {
        Hide();
        var player = BasePlayer;
        if (player == null)
            return;

        if (previewUntil > Environment.TickCount64)
        {
            var green = previewUntil - Environment.TickCount64 <= 2500;
            Show(player.Position, green ? "TEST GREEN — MOVE" : "TEST BLUE — WAIT", green);
            return;
        }

        if (!armed)
            return;

        if (Svc.ClientState.TerritoryType != 777 ||
            player.CurrentHp == 0 ||
            Environment.TickCount64 - armedAt > 20000 ||
            Svc.Objects.OfType<IPlayerCharacter>().Any(x => x.StatusList.Any(s => s.StatusId == 0x124)))
        {
            OnReset();
            return;
        }

        var markers = new ulong[]
        {
            Marking.GetMarker(0),
            Marking.GetMarker(1),
            Marking.GetMarker(2)
        };

        personalIndex = FindPersonalIndex(markers, markersAtStart, player.EntityId,
            selected.Contains(player.EntityId));

        if (personalIndex < 0)
        {
            for (var i = 0; i < 3; i++)
                if (Controller.TryGetElementByName($"JailPreview{i}", out var preview))
                {
                    preview.SetRefPosition(Center + towardTitan * ((1 - i) * 6.5f));
                    preview.SetOffPosition(Vector3.Zero);
                    preview.overlayText = $"{i + 1}";
                    preview.Enabled = true;
                }
            if (selected.Contains(player.EntityId))
                ShowText("FOLLOW YOUR JAIL NUMBER");
            return;
        }

        var destination = Center + towardTitan * ((1 - personalIndex) * 6.5f);
        Show(destination, landslideResolved
            ? $"JAIL {personalIndex + 1} — MOVE"
            : $"JAIL {personalIndex + 1} — WAIT", landslideResolved);
    }

    private void Show(Vector3 position, string message, bool moveIn)
    {
        if (Controller.TryGetElementByName("JailSpot", out var spot))
        {
            spot.SetRefPosition(position);
            spot.SetOffPosition(Vector3.Zero);
            spot.color = moveIn ? 4278255360u : 4294940979u;
            spot.Filled = moveIn;
            spot.fillIntensity = 0.65f;
            spot.thicc = moveIn ? 10f : 3f;
            spot.Enabled = true;
        }
        ShowText(message);
    }

    private static int FindPersonalIndex(ulong[] markers, ulong[] initial, ulong player, bool selectedPlayer)
    {
        var index = Array.IndexOf(markers, player);
        return index >= 0 && markers.Count(x => x == player) == 1 &&
            (selectedPlayer || initial[index] != markers[index]) ? index : -1;
    }

    private void ShowText(string message)
    {
        if (Controller.TryGetElementByName("JailText", out var text))
        {
            text.overlayText = message;
            text.Enabled = true;
        }
    }

    private void Hide()
    {
        if (Controller.TryGetElementByName("JailSpot", out var spot))
            spot.Enabled = false;
        if (Controller.TryGetElementByName("JailText", out var text))
            text.Enabled = false;
        for (var i = 0; i < 3; i++)
            if (Controller.TryGetElementByName($"JailPreview{i}", out var preview))
                preview.Enabled = false;
    }

    public override void OnReset()
    {
        armed = false;
        landslideResolved = false;
        titanId = 0;
        selected.Clear();
        personalIndex = -1;
        Array.Clear(markersAtStart, 0, markersAtStart.Length);
        previewUntil = 0;
        Hide();
    }

    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Version 3: three blue numbered destinations appear at Upheaval. Your detected jail number narrows this to one blue circle. It turns green on the first Landslide hit event. Blue is a destination preview, not permission to move into Landslide.");
        ImGui.TextWrapped($"Active: {armed}. Jail targets seen: {selected.Count}. Your matched number: {(personalIndex < 0 ? "none" : (personalIndex + 1).ToString())}. Landslide resolved: {landslideResolved}.");
        if (Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
            return;
        if (ImGui.Button("TEST BLUE CIRCLE (5 seconds)"))
        {
            OnReset();
            previewUntil = Environment.TickCount64 + 5000;
        }
        if (ImGui.Button("HIDE TEST"))
            OnReset();
        ImGui.TextWrapped("The test shows blue for 2.5 seconds, then green for 2.5 seconds at your feet. This is a display test only. In the fight, be beside your destination outside the first Landslide before stepping in. This script does not guide the knockback or choose a Landslide dodge path. Live timing remains untested.");
    }

    public override void OnCombatEnd() => OnReset();
    public override void OnDisable() => OnReset();
}
