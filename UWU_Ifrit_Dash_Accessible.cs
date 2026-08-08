using ECommons.DalamudServices;
using ECommons.GameHelpers; // required for uint.GetObject()
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Numerics;
namespace MaggieScripts.Duties.Stormblood;
public sealed class UWU_Ifrit_Dash_Accessible : SplatoonScript
{
    // TODO: verify this is actually Ifrit's Crimson Cyclone cast in UWU (not the unrelated SMN pet action of the same name).
    // Use Splatoon's built-in Logs tool to confirm the real castId before relying on this.
    private const uint CrimsonCycloneCastId = 0x2B5F;

    private static readonly Vector3 Center = new(100f, 0f, 100f);

    // UWU territory ID, confirmed directly (777) rather than via an unverified enum member.
    public override HashSet<uint>? ValidTerritories { get; } = [777];

    public override Metadata? Metadata =>
        new(1, "Maggie UWU Ifrit Dash Accessible");

    private DateTime? _resetAt;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "IfritDash_Current",
            "{\"Name\":\"CURRENT\",\"Enabled\":false,\"radius\":2.5,\"Donut\":0.35,\"color\":4278255360,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true}"
        );
        Controller.RegisterElementFromCode(
            "IfritDash_Next",
            "{\"Name\":\"NEXT\",\"Enabled\":false,\"radius\":2.2,\"Donut\":0.35,\"color\":4294967040,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true}"
        );
        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != CrimsonCycloneCastId)
            return;

        var ifrit = source.GetObject();
        if (ifrit == null)
            return;

        var start = ifrit.Position;
        var end = new Vector3(
            Center.X * 2f - start.X,
            start.Y,
            Center.Z * 2f - start.Z
        );
        ShowRoute(start, end);
        _resetAt = DateTime.UtcNow.AddMilliseconds(4500);
    }

    public override void OnUpdate()
    {
        if (_resetAt.HasValue && DateTime.UtcNow >= _resetAt.Value)
        {
            DisableMarkers();
            _resetAt = null;
        }
    }

    public override void OnReset()
    {
        _resetAt = null;
        DisableMarkers();
    }

    private void ShowRoute(Vector3 currentPosition, Vector3 nextPosition)
    {
        if (!Controller.TryGetElementByName("IfritDash_Current", out var current) ||
            !Controller.TryGetElementByName("IfritDash_Next", out var next))
            return;

        // Splatoon axis convention: refX = world X, refY = world Z, refZ = world Y (height)
        current.refX = currentPosition.X;
        current.refY = currentPosition.Z;
        current.refZ = currentPosition.Y;

        next.refX = nextPosition.X;
        next.refY = nextPosition.Z;
        next.refZ = nextPosition.Y;

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
