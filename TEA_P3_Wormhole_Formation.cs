// NAUR personal number/soak helper, replacing the inherited unverified path table.
// Original helper: Garume, PunishXIV/Splatoon. See TEA_NAUR_R1_README.md.
using ECommons;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.ImGuiMethods;
using Splatoon;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ECommons.DalamudServices.Legacy;

namespace MaggieSplatoon.TEA;

public class TEA_P3_Wormhole_Formation : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories => [887];
    public override Metadata? Metadata => new(105, "Garume; Maggie accessibility repair");
    private bool active, chakramsDone;
    private int number, soaks;
    private long expires;

    public override void OnSetup()
    {
        Controller.RegisterElement("Soak", new Element(0)
        {
            Enabled = false, radius = 0.8f, thicc = 5f, overlayFScale = 1.7f,
            overlayVOffset = 2f, tether = false
        }, true);
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if(castId != 18542) return;
        OnReset();
        active = true;
        expires = Environment.TickCount64 + 65000;
    }

    public override void OnVFXSpawn(uint target, string path)
    {
        const string prefix = "vfx/lockon/eff/m0361trg_a";
        if(!active || BasePlayer == null || target != BasePlayer.EntityId
           || !path.StartsWith(prefix) || path.Length <= prefix.Length) return;
        if(int.TryParse(path.Substring(prefix.Length, 1), out var n) && n is >= 1 and <= 8)
            number = n;
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if(!active) return;
        // Absolute stages: duplicate packets cannot increment a stage or overrun an array.
        switch(set.Action?.RowId)
        {
            case 18517: chakramsDone = true; break;
            case 18537: soaks = Math.Max(soaks, 1); break;
            case 18536: soaks = Math.Max(soaks, 2); break;
            case 18535: OnReset(); break;
        }
    }

    private static int SoakWave(int n) => n switch
    {
        5 or 6 => 1,
        7 or 8 => 2,
        1 or 2 => 3,
        _ => 0
    };

    public override void OnUpdate()
    {
        Controller.GetRegisteredElements().Each(x => x.Value.Enabled = false);
        if(!active) return;
        if(BasePlayer == null || BasePlayer.CurrentHp == 0 || Environment.TickCount64 >= expires)
        {
            OnReset();
            return;
        }
        if(number == 0)
        {
            Controller.DisplayAttentionWindowLine(new Vector4(1, 1, 0, 1), "WORMHOLE: NUMBER UNKNOWN");
            return;
        }
        var wave = SoakWave(number);
        var due = wave != 0 && wave == soaks + 1;
        var side = number % 2 == 1 ? "WEST" : "EAST";
        var text = wave == 0 ? $"{number} — {side} — JUMP / RAY BAIT"
            : soaks >= wave ? $"{number} — SOAK DONE — CHECK LIMIT CUT"
            : $"{number} — {side} — SOAK {wave}";
        if(due && !chakramsDone) text += " — WAIT FOR CHAKRAMS";
        if(due && chakramsDone) text += wave == 3 ? " — WAIT FOR VULN TO CLEAR" : " — WALL ROUTE / OUTER EDGE";
        Controller.DisplayAttentionWindowLine(due && chakramsDone && wave != 3
            ? new Vector4(0, 1, 0, 1) : new Vector4(0, 1, 1, 1), text);
        if(!due) return;

        // Observe the actual wormhole position on this player's assigned side.
        // No fixed diagonal, invented movement route, or 'run through center' arrow.
        var candidates = Svc.Objects.Where(x => x.DataId is 2007519 or 2007520 or 2007521)
            .Where(x => number % 2 == 1 ? x.Position.X < 99 : x.Position.X > 101)
            .Select(x => x.Position).ToArray();
        if(candidates.Length == 0) return;
        var position = candidates[0];
        if(candidates.Any(x => Vector3.Distance(x, position) > 1f)) return; // ambiguous objects
        if(Controller.TryGetElementByName("Soak", out var marker))
        {
            marker.SetOffPosition(position);
            marker.color = chakramsDone && wave != 3 ? 0xFF00FF00u : 0xFFFFFF00u;
            marker.overlayText = wave == 3 ? "SOAK 3 AREA — CHECK VULN"
                : chakramsDone ? $"SOAK {wave} AREA — OUTER EDGE" : $"NEXT: SOAK {wave}";
            marker.Enabled = true;
        }
    }

    public override void OnReset()
    {
        active = chakramsDone = false;
        number = soaks = 0;
        expires = 0;
        Controller.GetRegisteredElements().Each(x => x.Value.Enabled = false);
    }
    public override void OnCombatEnd() => OnReset();
    public override void OnDisable() => OnReset();
    public override void OnSettingsDraw()
    {
        ImGuiEx.Text("NAUR: odd WEST / even EAST. Personal number and soak order only.");
        ImGuiEx.Text("Green soak AREA after Chakrams; stand near its wall edge. Final soak stays cyan: check vuln expiry.");
        ImGuiEx.Text("Does not solve jump/ray bait, Limit Cut facing, or the route around other players.");
        ImGuiEx.Text("Inherited fixed path arrows removed pending replay validation.");
    }
}
