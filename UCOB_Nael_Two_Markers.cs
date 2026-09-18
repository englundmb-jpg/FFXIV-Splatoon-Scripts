using Dalamud.Game.ClientState.Conditions;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UCOB_Nael_Two_Markers : SplatoonScript
{
    private const uint Quickmarch = 0x26E2;
    private const uint GrandOctet = 0x26E7;
    private static readonly Vector3 SouthNeurolink = new(0.0f, 0.0f, 8.75f);

    private bool phaseActive;
    private long grandOctetStarted;

    public override HashSet<uint>? ValidTerritories { get; } = [733];
    public override Metadata? Metadata => new(4, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Nael_Red_Line",
            """
            {
              "Name":"NAEL — RED LINE",
              "Enabled":false,
              "type":1,
              "radius":1.5,
              "color":4278190335,
              "thicc":8.0,
              "refActorNPCID":2617,
              "refActorComparisonType":4,
              "tether":true,
              "LegacyFill":false
            }
            """
        );

        Controller.RegisterElementFromCode(
            "South_Neurolink_Yellow",
            """
            {
              "Name":"SOUTH NEUROLINK — YELLOW",
              "Enabled":false,
              "radius":2.5,
              "Donut":0.35,
              "color":4278255615,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":false,
              "LegacyFill":true,
              "overlayText":"NEUROLINK",
              "overlayBGColor":4278190080,
              "overlayTextColor":4278255615,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId == Quickmarch)
        {
            phaseActive = true;
            grandOctetStarted = 0;
            ShowPhaseMarkers();
        }
        else if (castId == GrandOctet && phaseActive)
        {
            grandOctetStarted = Environment.TickCount64;
        }
    }

    public override void OnUpdate()
    {
        if (!Svc.Condition[ConditionFlag.InCombat])
        {
            if (phaseActive)
                OnReset();
            return;
        }

        if (!phaseActive)
            return;

        ShowPhaseMarkers();

        if (grandOctetStarted != 0 &&
            Environment.TickCount64 - grandOctetStarted > 30000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        phaseActive = false;
        grandOctetStarted = 0;
        SetEnabled("Nael_Red_Line", false);
        SetEnabled("South_Neurolink_Yellow", false);
    }

    private void ShowPhaseMarkers()
    {
        SetEnabled("Nael_Red_Line", true);

        if (Controller.TryGetElementByName(
                "South_Neurolink_Yellow",
                out var neurolink))
        {
            neurolink.SetOffPosition(SouthNeurolink);
            neurolink.Enabled = true;
        }
    }

    private void SetEnabled(string name, bool enabled)
    {
        if (Controller.TryGetElementByName(name, out var element))
            element.Enabled = enabled;
    }
}
