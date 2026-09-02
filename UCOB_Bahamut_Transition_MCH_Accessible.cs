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

// Quackmarch pugs aether: during the Nael-to-Bahamut transition,
// the physical-ranged/MCH assignment stacks on the south 4-waymark.
public sealed class UCOB_Bahamut_Transition_MCH_Accessible : SplatoonScript
{
    private static readonly Vector3 SouthFour = new(0.0f, 0.0f, 8.75f);

    private bool transitionSeen;
    private bool active;
    private long activatedAt;

    public override HashSet<uint>? ValidTerritories { get; } = [733];

    public override Metadata? Metadata => new(1, "Maggie");

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "Bahamut_Transition_MCH",
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
              "overlayText":"CURRENT — MCH / 4",
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
        if (!Svc.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.InCombat])
        {
            if (transitionSeen || active)
                OnReset();
            return;
        }

        if (!transitionSeen)
        {
            var nael = Svc.Objects
                .OfType<IBattleNpc>()
                .FirstOrDefault(x => x.Name.TextValue.Contains(
                    "Nael", StringComparison.OrdinalIgnoreCase));

            // BossMod's verified P2 -> P3 transition condition.
            if (nael != null && !nael.IsTargetable && nael.CurrentHp <= 1)
                Activate();
        }

        // Seventh Umbral Era resolves about 5.3 seconds after transition.
        if (active && Environment.TickCount64 - activatedAt > 7000)
            HideMarker();
    }

    public override void OnReset()
    {
        transitionSeen = false;
        active = false;
        activatedAt = 0;

        if (Controller.TryGetElementByName(
                "Bahamut_Transition_MCH", out var marker))
            marker.Enabled = false;
    }

    private void Activate()
    {
        transitionSeen = true;
        active = true;
        activatedAt = Environment.TickCount64;

        if (!Controller.TryGetElementByName(
                "Bahamut_Transition_MCH", out var marker))
            return;

        marker.SetOffPosition(SouthFour);
        marker.Enabled = true;
    }

    private void HideMarker()
    {
        active = false;

        if (Controller.TryGetElementByName(
                "Bahamut_Transition_MCH", out var marker))
            marker.Enabled = false;
    }
}
