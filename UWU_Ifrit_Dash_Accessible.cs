using ECommons.ExcelServices.TerritoryEnumeration;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UWU_Ifrit_Dash_Accessible : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        [Raids.the_Weapons_Refrain_Ultimate];

    public override Metadata? Metadata =>
        new(1, "Maggie Ifrit Dash accessibility build");

    private bool active;
    private int cycloneCount;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "IfritDash_Current",
            "{\"Name\":\"CURRENT\",\"Enabled\":false,\"radius\":2.5,\"Donut\":0.35,\"color\":4278255360,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true}"
        );

        Controller.RegisterElementFromCode(
            "IfritDash_Next",
            "{\"Name\":\"NEXT\",\"Enabled\":false,\"radius\":2.2,\"Donut\":0.35,\"color\":4294967040,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true}"
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        // 2D4C = Ultimate Annihilation.
        // This is the proven phase gate used by the working
        // Annihilation accessibility script.
        if (castId == 0x2D4C)
        {
            active = true;
            cycloneCount = 0;
            return;
        }

        if (!active)
            return;

        // 2B5F = Crimson Cyclone.
        if (castId == 0x2B5F)
        {
            cycloneCount++;

            // First Crimson Cyclone during Annihilation.
            if (cycloneCount == 1)
            {
                ShowRoute(
                    new Vector3(100.138f, 0.0f, 81.841f),
                    new Vector3(100.070f, 0.0f, 90.900f)
                );
            }

            return;
        }

        // 2B52 = Eye of the Storm.
        // End this helper after the Cyclone/Landslide section.
        if (castId == 0x2B52 && cycloneCount > 0)
        {
            DisableMarkers();
            active = false;
        }
    }

    public override void OnReset()
    {
        active = false;
        cycloneCount = 0;
        DisableMarkers();
    }

    private void ShowRoute(Vector3 currentPosition, Vector3 nextPosition)
    {
        if (!Controller.TryGetElementByName("IfritDash_Current", out var current) ||
            !Controller.TryGetElementByName("IfritDash_Next", out var next))
            return;

        current.SetRefPosition(currentPosition);
        next.SetRefPosition(nextPosition);

        current.Enabled = true;
        next.Enabled = true;
    }

    private void DisableMarkers()
    {
        if (Controller.TryGetElementByName("IfritDash_Current", out var current))
            current.Enabled = false;

        if (Controller.TryGetElementByName("IfritDash_Next", out var next))
            next.Enabled = false;
    }
}
