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
using System.Text;

using ECommons.DalamudServices;
using ECommons.DalamudServices.Legacy;

namespace SplatoonScriptsOfficial.Duties.Dawntrail;

public class M9S_Sanguine_Scratch_Accessible : SplatoonScript
{
    public override Metadata Metadata { get; } = new(2, "Maggie - NightmareXIV v2 accessible");
    public override HashSet<uint>? ValidTerritories { get; } = [1321];

    int CastNum = 0;
    int ElementNum = 0;
    float BaseRotation = 0f;

    public override void OnSetup()
    {
        Controller.RegisterElementFromCode("CURRENT", """{"Name":"CURRENT","Enabled":false,"refX":100.0,"refY":100.0,"radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"tether":true,"overlayText":"CURRENT","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}""");
        Controller.RegisterElementFromCode("NEXT", """{"Name":"NEXT","Enabled":false,"refX":100.0,"refY":100.0,"radius":2.2,"Donut":0.35,"color":4294967040,"thicc":8.0,"tether":true,"overlayText":"NEXT","overlayBGColor":4278190080,"overlayTextColor":4294967295,"overlayFScale":1.5}""");
    }

    public override unsafe void OnStartingCast(uint sourceId, PacketActorCast* packet)
    {
        if(packet->ActionDescriptor == new Splatoon.Data.ActionDescriptor(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, 45989))
        {
            CastNum = 0;
            BaseRotation = packet->Rotation;
            ElementNum++;
            UpdateMarkers();
        }
        if(packet->ActionDescriptor == new ActionDescriptor(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, 45992) || packet->ActionDescriptor == new ActionDescriptor(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, 45994))
        {
            this.Controller.Reset();
        }
    }

    public override void OnReset()
    {
        CastNum = 0;
        ElementNum = 0;
        BaseRotation = 0f;
        if(Controller.TryGetElementByName("CURRENT", out var current)) current.Enabled = false;
        if(Controller.TryGetElementByName("NEXT", out var next)) next.Enabled = false;
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
                PluginLog.Information($"CastNum: {CastNum}");

                if(CastNum >= 5)
                {
                    if(Controller.TryGetElementByName("CURRENT", out var current)) current.Enabled = false;
                    if(Controller.TryGetElementByName("NEXT", out var next)) next.Enabled = false;
                    return;
                }

                UpdateMarkers();
            }
        }
    }

    void UpdateMarkers()
    {
        var player = Svc.ClientState.LocalPlayer;
        if(player == null) return;

        float dangerRotation = BaseRotation + (CastNum * 22.5f).DegToRad();
        float currentSafeRotation = dangerRotation + 22.5f.DegToRad();
        float nextSafeRotation = currentSafeRotation + 22.5f.DegToRad();

        SetNearestSafe("CURRENT", currentSafeRotation, true);
        SetNearestSafe("NEXT", nextSafeRotation, CastNum < 4);
    }

    void SetNearestSafe(string name, float baseRotation, bool enabled)
    {
        if(!Controller.TryGetElementByName(name, out var element)) return;

        if(!enabled)
        {
            element.Enabled = false;
            return;
        }

        var player = Svc.ClientState.LocalPlayer;
        if(player == null) return;

        float bestX = 100f;
        float bestZ = 100f;
        float bestDistance = float.MaxValue;
        const float radius = 12f;

        for(int i = 0; i < 8; i++)
        {
            float angle = baseRotation + (i * 45f).DegToRad();
            float x = 100f + MathF.Sin(angle) * radius;
            float z = 100f + MathF.Cos(angle) * radius;
            float dx = x - player.Position.X;
            float dz = z - player.Position.Z;
            float distance = dx * dx + dz * dz;

            if(distance < bestDistance)
            {
                bestDistance = distance;
                bestX = x;
                bestZ = z;
            }
        }

        element.refX = bestX;
        element.refY = bestZ;
        element.Enabled = true;
    }
}
