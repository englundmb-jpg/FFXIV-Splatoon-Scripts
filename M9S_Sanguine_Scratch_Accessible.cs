using ECommons.Hooks.ActionEffectTypes;
using ECommons.Logging;
using ECommons.MathHelpers;
using ECommons.Throttlers;
using Splatoon.Data;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using Splatoon.Utility;
using System;
using System.Collections.Generic;
using System.Numerics;

using ECommons.DalamudServices;
using ECommons.DalamudServices.Legacy;

namespace SplatoonScriptsOfficial.Duties.Dawntrail;

public class M9S_Sanguine_Scratch_Accessible : SplatoonScript
{
    public override Metadata Metadata { get; } = new(2, "Maggie - based on NightmareXIV Sanguine Scratch v2");
    public override HashSet<uint>? ValidTerritories { get; } = [1321];

    const float CenterX = 100f;
    const float CenterZ = 100f;
    const float Radius = 12f;

    int CastNum = 0;
    float BaseRotation = 0f;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode(
            "CURRENT",
            """{"Name":"CURRENT","Enabled":false,"refX":100.0,"refY":100.0,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"tether":true,"overlayText":"CURRENT","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}"""
        );

        Controller.RegisterElementFromCode(
            "NEXT",
            """{"Name":"NEXT","Enabled":false,"refX":100.0,"refY":100.0,"radius":2.2,"Donut":0.35,"color":4294967040,"thicc":8.0,"tether":true,"overlayText":"NEXT","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}"""
        );
    }

    public override unsafe void OnStartingCast(uint sourceId, PacketActorCast* packet)
    {
        if(packet->ActionDescriptor == new Splatoon.Data.ActionDescriptor(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, 45989))
        {
            CastNum = 0;
            BaseRotation = packet->Rotation;
            UpdateMarkers();
        }

        if(packet->ActionDescriptor == new ActionDescriptor(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, 45992) ||
           packet->ActionDescriptor == new ActionDescriptor(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, 45994))
        {
            this.Controller.Reset();
        }
    }

    public override void OnReset()
    {
        CastNum = 0;
        BaseRotation = 0f;

        if(Controller.TryGetElementByName("CURRENT", out var current))
            current.Enabled = false;

        if(Controller.TryGetElementByName("NEXT", out var next))
            next.Enabled = false;
    }

    public override void OnUpdate()
    {
        Controller.Hide();

        if(CastNum.InRange(0, 4))
            UpdateMarkers();
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if(set.Action?.RowId == 45989 || set.Action?.RowId == 45991)
        {
            if(EzThrottler.Throttle(this.InternalData.FullName + "Cast", 250))
            {
                CastNum++;

                if(CastNum >= 5)
                {
                    if(Controller.TryGetElementByName("CURRENT", out var current))
                        current.Enabled = false;

                    if(Controller.TryGetElementByName("NEXT", out var next))
                        next.Enabled = false;

                    return;
                }

                UpdateMarkers();
            }
        }
    }

    void UpdateMarkers()
    {
        var player = Svc.ClientState.LocalPlayer;
        if(player == null)
            return;

        // NightmareXIV v2 rotates the danger pattern by 22.5 degrees per hit.
        // The safe lane is halfway between the 30-degree danger cones.
        float dangerRotation = BaseRotation + (CastNum * 22.5f).DegToRad();
        float currentSafeRotation = dangerRotation + 22.5f.DegToRad();
        float nextSafeRotation = currentSafeRotation + 22.5f.DegToRad();

        var currentPos = NearestSafePoint(currentSafeRotation, player.Position);
        var nextPos = NearestSafePoint(nextSafeRotation, player.Position);

        if(Controller.TryGetElementByName("CURRENT", out var current))
        {
            current.SetRefPosition(currentPos);
            current.Enabled = true;
        }

        if(Controller.TryGetElementByName("NEXT", out var next))
        {
            next.SetRefPosition(nextPos);
            next.Enabled = CastNum < 4;
        }
    }

    Vector3 NearestSafePoint(float baseRotation, Vector3 playerPos)
    {
        Vector3 best = new(CenterX, playerPos.Y, CenterZ);
        float bestDistance = float.MaxValue;

        for(int i = 0; i < 8; i++)
        {
            float angle = baseRotation + (i * 45f).DegToRad();
            Vector3 candidate = new(
                CenterX + MathF.Sin(angle) * Radius,
                playerPos.Y,
                CenterZ + MathF.Cos(angle) * Radius
            );

            float dx = candidate.X - playerPos.X;
            float dz = candidate.Z - playerPos.Z;
            float distance = dx * dx + dz * dz;

            if(distance < bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }

        return best;
    }
}
