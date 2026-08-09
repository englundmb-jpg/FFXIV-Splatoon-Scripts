using ECommons.ExcelServices.TerritoryEnumeration;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SplatoonScriptsOfficial.Duties.Stormblood;

public sealed class UWU_Ifrit_Dash_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        new() { Raids.the_Weapons_Refrain_Ultimate };

    public override Metadata? Metadata =>
        new(1, "Maggie Ifrit Dash accessibility build");

    private bool active;
    private long startedAt;
    private int stage;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "IfritDash_Current",
            """{"Name":"CURRENT","Enabled":false,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"tether":true}"""
        );

        Controller.RegisterElementFromCode(
            "IfritDash_Next",
            """{"Name":"NEXT","Enabled":false,"radius":2.2,"Donut":0.35,"color":4294967040,"thicc":8.0,"tether":true}"""
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != 0x2D4C)
            return;

        active = true;
        startedAt = Environment.TickCount64;
        stage = 0;
        DisableMarkers();
    }

    public override void OnUpdate()
    {
        if (!active)
            return;

        var elapsed = Environment.TickCount64 - startedAt;

        if (stage == 0 && elapsed >= 27500)
        {
            stage = 1;

            ShowRoute(
                new Vector3(100.138f, 0.0f, 81.841f),
                new Vector3(100.070f, 0.0f, 90.900f)
            );

            return;
        }

        if (elapsed >= 33000)
            Controller.Reset();
    }

    public override void OnReset()
    {
        active = false;
        startedAt = 0;
        stage = 0;
        DisableMarkers();
    }

    private void ShowRoute(Vector3 currentPosition, Vector3 nextPosition)
    {
        if (!Controller.TryGetElementByName(
                "IfritDash_Current",
                out var current) ||
            !Controller.TryGetElementByName(
                "IfritDash_Next",
                out var next))
            return;

        current.SetRefPosition(currentPosition);
        next.SetRefPosition(nextPosition);

        current.Enabled = true;
        next.Enabled = true;
    }

    private void DisableMarkers()
    {
        if (Controller.TryGetElementByName(
                "IfritDash_Current",
                out var current))
            current.Enabled = false;

        if (Controller.TryGetElementByName(
                "IfritDash_Next",
                out var next))
            next.Enabled = false;
    }
}
