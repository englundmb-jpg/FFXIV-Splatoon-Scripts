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
    public override Metadata? Metadata => new(2, "Maggie - blue circle movement cue");

    private static readonly Vector3 Center = new(100f, 0f, 100f);
    private readonly HashSet<ulong> selected = new();
    private Vector3 towardTitan;
    private uint titanId;
    private bool armed;
    private bool landslideResolved;
    private long armedAt;
    private long previewUntil;

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
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (Svc.ClientState.TerritoryType != 777 || castId != 0x2B67)
            return;

        OnReset();
        var titan = Svc.Objects.OfType<IBattleNpc>()
            .FirstOrDefault(x => x.EntityId == source && x.DataId == 8727);
        if (titan == null)
            return;

        var delta = titan.Position - Center;
        delta.Y = 0;
        if (delta.LengthSquared() < 1f)
            return;

        towardTitan = Vector3.Normalize(delta);
        titanId = source;
        armedAt = Environment.TickCount64;
        armed = true;
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (!armed || Svc.ClientState.TerritoryType != 777)
            return;

        var id = set.Action?.RowId ?? 0;
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
            Show(player.Position, "TEST BLUE CIRCLE",
                previewUntil - Environment.TickCount64 <= 2500);
            return;
        }

        if (!armed)
            return;

        if (Svc.ClientState.TerritoryType != 777 ||
            player.CurrentHp == 0 ||
            Environment.TickCount64 - armedAt > 20000 ||
            player.StatusList.Any(x => x.StatusId == 0x124))
        {
            OnReset();
            return;
        }

        if (!selected.Contains(player.EntityId))
            return;

        if (selected.Count != 3)
        {
            ShowText("WAIT FOR JAIL MARKER");
            return;
        }

        var markers = new ulong[]
        {
            Marking.GetMarker(0),
            Marking.GetMarker(1),
            Marking.GetMarker(2)
        };

        if (markers.Distinct().Count() != 3 ||
            markers.Any(x => !selected.Contains(x)))
        {
            ShowText("WAIT FOR JAIL MARKER");
            return;
        }

        var index = Array.IndexOf(markers, (ulong)player.EntityId);
        if (index < 0)
            return;

        var destination = Center + towardTitan * ((1 - index) * 6.5f);
        Show(destination, landslideResolved
            ? $"JAIL {index + 1} — IN"
            : $"JAIL {index + 1} — DODGE FIRST", landslideResolved);
    }

    private void Show(Vector3 position, string message, bool moveIn)
    {
        if (Controller.TryGetElementByName("JailSpot", out var spot))
        {
            spot.SetOffPosition(position);
            spot.Filled = moveIn;
            spot.fillIntensity = 0.65f;
            spot.thicc = moveIn ? 10f : 3f;
            spot.Enabled = true;
        }
        ShowText(message);
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
    }

    public override void OnReset()
    {
        armed = false;
        landslideResolved = false;
        titanId = 0;
        selected.Clear();
        previewUntil = 0;
        Hide();
    }

    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Version 2: outline before the first Landslide resolves; thick filled blue circle after it resolves. Fight timing has not been tested in game.");
        if (ImGui.Button("TEST BLUE CIRCLE (5 seconds)"))
        {
            OnReset();
            previewUntil = Environment.TickCount64 + 5000;
        }
        if (ImGui.Button("HIDE TEST"))
            OnReset();
        ImGui.TextWrapped("The test shows an outline for 2.5 seconds, then a filled circle for 2.5 seconds at your feet. In the fight, be beside your destination outside the first Landslide; the fill cues the final step in. This script does not guide the knockback or choose a Landslide dodge path.");
    }
}
