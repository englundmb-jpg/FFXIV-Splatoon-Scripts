using ECommons.ExcelServices.TerritoryEnumeration;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SplatoonScriptsOfficial.Duties.Stormblood;

public sealed class UWU_Suppression_D3_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        new() { Raids.the_Weapons_Refrain_Ultimate };

    public override Metadata? Metadata =>
        new(1, "Maggie D3 Suppression accessibility test build");

    private bool active;
    private long startedAt;
    private int stage;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Suppression_Current",
            """{"Name":"CURRENT","Enabled":false,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"tether":true}"""
        );

        Controller.RegisterElementFromCode(
            "Suppression_Next",
            """{"Name":"NEXT","Enabled":false,"radius":2.2,"Donut":0.35,"color":4294967040,"thicc":8.0,"tether":true}"""
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        // Ultimate Suppression
        if (castId != 0x2D4D)
            return;

        active = true;
        startedAt = Environment.TickCount64;
        stage = 0;

        // Green: eruption position southeast.
        // Cyan: behind the southwest tanks.
        ShowRoute(
            new Vector3(110.000f, 0.0f, 110.000f),
            new Vector3(92.000f, 0.0f, 106.000f)
        );
    }

    public override void OnUpdate()
    {
        if (!active)
            return;

        var elapsed = Environment.TickCount64 - startedAt;

        // Behind the southwest tanks for Mistral/Light Pillar.
        if (stage == 0 && elapsed >= 16000)
        {
            stage = 1;

            ShowRoute(
                new Vector3(92.000f, 0.0f, 106.000f),
                new Vector3(107.157f, 0.0f, 107.792f)
            );

            return;
        }

        // Verified 4-waymark for Gaol or Light Pillar.
        if (stage == 1 && elapsed >= 24500)
        {
            stage = 2;

            ShowRoute(
                new Vector3(107.157f, 0.0f, 107.792f),
                new Vector3(106.897f, 0.0f, 100.122f)
            );

            return;
        }

        // Verified B-waymark stack after Gaol.
        if (stage == 2 && elapsed >= 36500)
        {
            stage = 3;

            ShowCurrentOnly(
                new Vector3(106.897f, 0.0f, 100.122f)
            );

            return;
        }

        if (elapsed >= 50000)
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
                "Suppression_Current",
                out var current) ||
            !Controller.TryGetElementByName(
                "Suppression_Next",
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
                "Suppression_Current",
                out var current) ||
            !Controller.TryGetElementByName(
                "Suppression_Next",
                out var next))
            return;

        current.SetRefPosition(currentPosition);

        current.Enabled = true;
        next.Enabled = false;
    }

    private void DisableMarkers()
    {
        if (Controller.TryGetElementByName(
                "Suppression_Current",
                out var current))
            current.Enabled = false;

        if (Controller.TryGetElementByName(
                "Suppression_Next",
                out var next))
            next.Enabled = false;
    }
}
