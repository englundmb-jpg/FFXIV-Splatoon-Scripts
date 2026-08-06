using ECommons.ExcelServices.TerritoryEnumeration;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SplatoonScriptsOfficial.Duties.Stormblood;

public sealed class UWU_Annihilation_D3_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        new() { Raids.the_Weapons_Refrain_Ultimate };

    public override Metadata? Metadata =>
        new(1, "Maggie D3 accessibility test build");

    private bool active;
    private long startedAt;
    private int stage;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Annihilation_Current",
            """{"Name":"CURRENT","Enabled":false,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"tether":true}"""
        );

        Controller.RegisterElementFromCode(
            "Annihilation_Next",
            """{"Name":"NEXT","Enabled":false,"radius":2.2,"Donut":0.35,"color":4294967040,"thicc":8.0,"tether":true}"""
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        // 0x2D4C = Ultimate Annihilation
        if (castId != 0x2D4C)
            return;

        active = true;
        startedAt = Environment.TickCount64;
        stage = 0;

        // Green: start southwest
        // Cyan: D3 Mesohigh position at C
        ShowRoute(
            new Vector3(94.000f, 0.0f, 94.000f),
            new Vector3(100.219f, 0.0f, 107.013f)
        );
    }

    public override void OnUpdate()
    {
        if (!active)
            return;

        var elapsed = Environment.TickCount64 - startedAt;

        // Move to D3 Mesohigh, prepare for north regroup.
        if (stage == 0 && elapsed >= 18500)
        {
            stage = 1;

            ShowRoute(
                new Vector3(100.219f, 0.0f, 107.013f),
                new Vector3(87.332f, 0.0f, 87.270f)
            );

            return;
        }

        // Regroup north at the verified 3-waymark.
        if (stage == 1 && elapsed >= 23000)
        {
            stage = 2;

            ShowRoute(
                new Vector3(87.332f, 0.0f, 87.270f),
                new Vector3(100.138f, 0.0f, 81.841f)
            );

            return;
        }

        // Dodge Crimson Cyclone at the verified 2-waymark.
        if (stage == 2 && elapsed >= 27500)
        {
            stage = 3;

            ShowRoute(
                new Vector3(100.138f, 0.0f, 81.841f),
                new Vector3(100.070f, 0.0f, 90.900f)
            );

            return;
        }

        // Move inward for Eye of the Storm.
        if (stage == 3 && elapsed >= 33000)
        {
            stage = 4;

            ShowCurrentOnly(
                new Vector3(100.070f, 0.0f, 90.900f)
            );

            return;
        }

        // Remove the markers after the useful portion of Annihilation.
        if (elapsed >= 44000)
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
                "Annihilation_Current",
                out var current) ||
            !Controller.TryGetElementByName(
                "Annihilation_Next",
                out var next))
            return;

        current.SetRefPosition(currentPosition);
        next.SetRefPosition(nextPosition);

        current.Enabled = true;
        next.Enabled = true;
    }

    private void ShowCurrentOnly(Vector3 currentPosition)
    {
        if (!Controller.TryGetElementByName(
                "Annihilation_Current",
                out var current) ||
            !Controller.TryGetElementByName(
                "Annihilation_Next",
                out var next))
            return;

        current.SetRefPosition(currentPosition);

        current.Enabled = true;
        next.Enabled = false;
    }

    private void DisableMarkers()
    {
        if (Controller.TryGetElementByName(
                "Annihilation_Current",
                out var current))
            current.Enabled = false;

        if (Controller.TryGetElementByName(
                "Annihilation_Next",
                out var next))
            next.Enabled = false;
    }
}
