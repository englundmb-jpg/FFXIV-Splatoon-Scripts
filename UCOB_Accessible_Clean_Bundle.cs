using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using Splatoon.Data;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood;

public sealed class UCOB_Adds_D3_Hatch_Accessible : SplatoonScript
{
    // Verified UCOB Waymark 3 / D3 Neurolink.
    private static readonly Vector3 D3Neurolink =
        new(0.0f, 0.0f, 8.75f);

    // Twister bait position just outside the back Neurolink.
    private static readonly Vector3 TwisterBait =
        new(0.0f, 0.0f, 11.75f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "AddsHatch_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":2.5,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "AddsHatch_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":1.8,
              "Donut":0.35,
              "color":4294902015,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        // Never display positional guidance before the pull or after a wipe.
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var twin = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Twintania",
                    StringComparison.OrdinalIgnoreCase));

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        // Both bosses being present identifies Adds phase.
        if (twin == null ||
            nael == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active)
            DrawPositions();

        if (active &&
            Environment.TickCount64 - activatedAt > 30000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;

        HideMarkers();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }

    private void DrawPositions()
    {
        // D3 / R1:
        //
        // CURRENT:
        // Stay outside the back Neurolink
        // so Twister is not dropped inside it.
        //
        // NEXT:
        // Enter the bottom Neurolink
        // after Twister resolves to take Hatch.

        if (Controller.TryGetElementByName(
                "AddsHatch_Current",
                out var current))
        {
            current.SetOffPosition(TwisterBait);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "AddsHatch_Next",
                out var next))
        {
            next.SetOffPosition(D3Neurolink);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "AddsHatch_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "AddsHatch_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}

public sealed class UCOB_Blackfire_Accessible : SplatoonScript
{
    private static readonly Vector3 Center =
        new(100.0f, 0.0f, 100.0f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Blackfire_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":2.5,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "Blackfire_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":1.8,
              "Donut":0.35,
              "color":4294902015,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        // Never display positional guidance before the pull or after a wipe.
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        var bahamut = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Bahamut",
                    StringComparison.OrdinalIgnoreCase));

        if (nael == null ||
            bahamut == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active)
        {
            DrawPositions(
                nael.Position,
                bahamut.Position
            );
        }

        if (active &&
            Environment.TickCount64 - activatedAt > 15000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;

        HideMarkers();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }

    private void DrawPositions(
        Vector3 naelPosition,
        Vector3 bahamutPosition)
    {
        var direction =
            naelPosition - Center;

        direction.Y = 0.0f;

        if (direction.LengthSquared() < 0.001f)
            return;

        direction = Vector3.Normalize(direction);

        var left =
            new Vector3(
                direction.Z,
                0.0f,
                -direction.X
            );

        // D3 is on the DPS / CCW side.
        var currentPosition =
            Center +
            direction * 6.0f +
            left * 6.0f;

        var nextPosition =
            Center +
            direction * 3.0f +
            left * 3.0f;

        if (Controller.TryGetElementByName(
                "Blackfire_Current",
                out var current))
        {
            current.SetOffPosition(currentPosition);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "Blackfire_Next",
                out var next))
        {
            next.SetOffPosition(nextPosition);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "Blackfire_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "Blackfire_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}

public sealed class UCOB_D3_Liquid_Hell_Accessible : SplatoonScript
{
    private static readonly Vector3 Center =
        new(0.0f, 0.0f, 0.0f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "LiquidHell_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":2.5,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "LiquidHell_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":1.8,
              "Donut":0.35,
              "color":4294902015,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        // Never display positional guidance before the pull or after a wipe.
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var twin = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Twintania",
                    StringComparison.OrdinalIgnoreCase));

        if (twin == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active)
            DrawPositions(twin.Position);

        if (active &&
            Environment.TickCount64 - activatedAt > 30000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;

        HideMarkers();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }

    private void DrawPositions(Vector3 twinPosition)
    {
        var direction =
            twinPosition - Center;

        direction.Y = 0.0f;

        if (direction.LengthSquared() < 0.001f)
            return;

        direction = Vector3.Normalize(direction);

        // D3 Liquid Hell bait:
        // Stay well outside 15 yalms from Twintania.
        //
        // CURRENT = initial bait position.
        // NEXT = move farther along the same outside lane.

        var currentPosition =
            twinPosition +
            direction * 17.0f;

        var nextPosition =
            twinPosition +
            direction * 21.0f;

        if (Controller.TryGetElementByName(
                "LiquidHell_Current",
                out var current))
        {
            current.SetOffPosition(currentPosition);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "LiquidHell_Next",
                out var next))
        {
            next.SetOffPosition(nextPosition);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "LiquidHell_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "LiquidHell_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}

public sealed class UCOB_Fellruin_Accessible : SplatoonScript
{
    private static readonly Vector3 Center =
        new(100.0f, 0.0f, 100.0f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Fellruin_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":2.5,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "Fellruin_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":1.8,
              "Donut":0.35,
              "color":4294902015,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        // Never display positional guidance before the pull or after a wipe.
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var bahamut = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Bahamut",
                    StringComparison.OrdinalIgnoreCase));

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        if (bahamut == null ||
            nael == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active)
        {
            DrawPositions(
                bahamut.Position,
                nael.Position
            );
        }

        if (active &&
            Environment.TickCount64 - activatedAt > 15000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;

        HideMarkers();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }

    private void DrawPositions(
        Vector3 bahamutPosition,
        Vector3 naelPosition)
    {
        var bahamutDirection =
            bahamutPosition - Center;

        bahamutDirection.Y = 0.0f;

        if (bahamutDirection.LengthSquared() < 0.001f)
            return;

        bahamutDirection =
            Vector3.Normalize(bahamutDirection);

        var right =
            new Vector3(
                -bahamutDirection.Z,
                0.0f,
                bahamutDirection.X
            );

        // D3 / R1 Quickmarch-style spread position
        // with Bahamut treated as relative north.
        var currentPosition =
            Center -
            bahamutDirection * 5.5f +
            right * 3.5f;

        // Party Neurolink is opposite Bahamut.
        var nextPosition =
            Center -
            bahamutDirection * 10.0f;

        if (Controller.TryGetElementByName(
                "Fellruin_Current",
                out var current))
        {
            current.SetOffPosition(currentPosition);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "Fellruin_Next",
                out var next))
        {
            next.SetOffPosition(nextPosition);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "Fellruin_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "Fellruin_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}

public sealed class UCOB_Heavensfall_Knockback_Accessible : SplatoonScript
{
    private static readonly Vector3 Center =
        new(0.0f, 0.0f, 0.0f);

    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "HeavensfallKB_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":2.5,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "HeavensfallKB_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "radius":1.8,
              "Donut":0.35,
              "color":4294902015,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        // Never display positional guidance before the pull or after a wipe.
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        if (nael == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active)
            DrawPositions(nael.Position);

        if (active &&
            Environment.TickCount64 - activatedAt > 15000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;

        HideMarkers();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }

    private void DrawPositions(Vector3 naelPosition)
    {
        var direction =
            naelPosition - Center;

        direction.Y = 0.0f;

        if (direction.LengthSquared() < 0.001f)
            return;

        direction = Vector3.Normalize(direction);

        var currentPosition =
            Center - direction * 6.0f;

        var nextPosition =
            Center - direction * 10.0f;

        if (Controller.TryGetElementByName(
                "HeavensfallKB_Current",
                out var current))
        {
            current.SetOffPosition(currentPosition);
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "HeavensfallKB_Next",
                out var next))
        {
            next.SetOffPosition(nextPosition);
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "HeavensfallKB_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "HeavensfallKB_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}

public sealed class UCOB_Heavensfall_R1_Accessible : SplatoonScript
{
    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Heavensfall_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "radius":3.0,
              "Donut":0.35,
              "color":4278255360,
              "thicc":8.0,
              "FillStep":1.0,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        // Never display positional guidance before the pull or after a wipe.
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        if (nael == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active)
        {
            if (Controller.TryGetElementByName(
                    "Heavensfall_Current",
                    out var current))
            {
                current.SetOffPosition(nael.Position);
                current.Enabled = true;
            }
        }

        if (active &&
            Environment.TickCount64 - activatedAt > 15000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;

        HideMarker();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;
    }

    private void HideMarker()
    {
        if (Controller.TryGetElementByName(
                "Heavensfall_Current",
                out var current))
        {
            current.Enabled = false;
        }
    }
}

public sealed class UCOB_Nael_Dive_Accessible : SplatoonScript
{
    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } =
        [733];

    public override Metadata? Metadata =>
        new(2, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "NaelDive_Current",
            """
            {
              "Name":"CURRENT",
              "Enabled":false,
              "type":3,
              "offX":15.0,
              "offY":15.0,
              "radius":2.5,
              "color":4278255360,
              "thicc":8.0,
              "refActorNPCID":2617,
              "refActorComparisonType":4,
              "includeRotation":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"CURRENT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        Controller.RegisterElementFromCode(
            "NaelDive_Next",
            """
            {
              "Name":"NEXT",
              "Enabled":false,
              "type":3,
              "offX":-15.0,
              "offY":15.0,
              "radius":2.2,
              "color":4294902015,
              "thicc":8.0,
              "refActorNPCID":2617,
              "refActorComparisonType":4,
              "includeRotation":true,
              "tether":true,
              "LegacyFill":true,
              "overlayText":"NEXT",
              "overlayBGColor":4278190080,
              "overlayTextColor":4294967295,
              "overlayFScale":1.5
            }
            """
        );

        OnReset();
    }

    public override void OnUpdate()
    {
        // Never display positional guidance before the pull or after a wipe.
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (active)
                OnReset();

            return;
        }

        var nael = Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x.Name.TextValue.Contains(
                    "Nael",
                    StringComparison.OrdinalIgnoreCase));

        if (nael == null)
        {
            if (active)
                OnReset();

            return;
        }

        if (!active)
            Activate();

        if (active &&
            Environment.TickCount64 - activatedAt > 10000)
        {
            OnReset();
        }
    }

    public override void OnReset()
    {
        active = false;
        activatedAt = 0;

        HideMarkers();
    }

    private void Activate()
    {
        active = true;
        activatedAt = Environment.TickCount64;

        if (Controller.TryGetElementByName(
                "NaelDive_Current",
                out var current))
        {
            current.Enabled = true;
        }

        if (Controller.TryGetElementByName(
                "NaelDive_Next",
                out var next))
        {
            next.Enabled = true;
        }
    }

    private void HideMarkers()
    {
        if (Controller.TryGetElementByName(
                "NaelDive_Current",
                out var current))
        {
            current.Enabled = false;
        }

        if (Controller.TryGetElementByName(
                "NaelDive_Next",
                out var next))
        {
            next.Enabled = false;
        }
    }
}
