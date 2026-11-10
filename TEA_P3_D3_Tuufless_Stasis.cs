// D3 only. Tuufless, Alexander Prime: https://ffxiv.tuufless.com/elemental/tea/03_alex_prime/
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.Hooks.ActionEffectTypes;
using Splatoon;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MaggieSplatoon.TEA;

public sealed class TEA_P3_D3_Tuufless_Stasis : SplatoonScript
{
    public override HashSet<uint> ValidTerritories => new() { 887 };
    public override Metadata Metadata => new(1, "Maggie accessibility");
    private const uint Assault = 1121, Close = 1123, Far = 1124, Frozen = 1119;
    private long deadline;
    private string lastCall = "";
    private HttpClient? voice;

    public override void OnSetup() => Controller.RegisterElement("Stand", new Element(0)
    {
        Enabled = false, radius = 0.8f, thicc = 6f, color = 0xFF00FF00,
        tether = true, overlayFScale = 1.6f, overlayVOffset = 1.5f
    });

    public override void OnEnable()
    {
        voice = new HttpClient(new HttpClientHandler { UseProxy = false })
            { Timeout = TimeSpan.FromSeconds(1) };
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if (castId != 18522) return;
        OnReset();
        deadline = Environment.TickCount64 + 20000;
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if (deadline != 0 && set.Action is { RowId: 18531 }) OnReset();
    }

    public override void OnUpdate()
    {
        if (deadline == 0 || !Controller.TryGetElementByName("Stand", out var marker)) return;
        marker.Enabled = false;
        var me = BasePlayer;
        if (Environment.TickCount64 >= deadline || me == null || me.CurrentHp == 0 ||
            me.StatusList.Any(s => s.StatusId == Frozen))
        {
            OnReset();
            return;
        }

        // Wait for the adds to move from their previous phase positions into the Stasis pattern.
        var adds = Svc.Objects.OfType<IBattleNpc>().Where(n =>
            Math.Abs(Math.Abs(n.Position.X - 100f) - 6f) < 1.5f &&
            Math.Abs(n.Position.Z - 100f) < 1.5f).ToArray();
        var justice = adds.FirstOrDefault(n => n.DataId == 0x2C4C);
        var chaser = adds.FirstOrDefault(n => n.DataId == 0x2C4E);
        if (justice == null || chaser == null ||
            (justice.Position.X > 100f) == (chaser.Position.X > 100f)) return;

        bool chaserEast = chaser.Position.X > 100f;
        var assignment = me.StatusList.FirstOrDefault(s => IsAssignment(s.StatusId))?.StatusId ?? 0;
        if (assignment == 0)
        {
            // Six distinct players receive a status. Never announce "nothing" on partial data.
            var assigned = Svc.Objects.OfType<IPlayerCharacter>()
                .Count(p => p.StatusList.Any(s => IsAssignment(s.StatusId)));
            if (assigned != 6) return;
        }

        var (x, z, code, label) = assignment switch
        {
            Close => (94f, 102f, "CLOSE", "WEST BOSS - SOUTH"),
            Far when chaserEast => (114f, 100f, "BLUE_IN", "EAST - IN FROM WALL"),
            Far => (118.5f, 100f, "BLUE_EDGE", "EAST EDGE"),
            Assault when chaserEast => (81.5f, 100f, "ASSAULT_W", "FAR WEST"),
            Assault => (118.5f, 100f, "ASSAULT_E", "FAR EAST"),
            _ => (106f, 102f, "NONE", "EAST BOSS - SOUTH")
        };
        marker.refX = x;
        marker.refY = z;
        marker.overlayText = label;
        marker.Enabled = true;
        if (lastCall == code) return;
        lastCall = code;
        if (voice != null) _ = Speak(voice, code);
    }

    private static bool IsAssignment(uint status) => status is Assault or Close or Far;

    private static async Task Speak(HttpClient client, string code)
    {
        try
        {
            using var body = new StringContent("MAGGIE_TEA_STASIS|" + code, Encoding.UTF8, "text/plain");
            using var response = await client.PostAsync("http://localhost:51423/", body).ConfigureAwait(false);
        }
        catch (HttpRequestException) { } // The marker works without ACT.
        catch (OperationCanceledException) { }
        catch (ObjectDisposedException) { }
    }

    public override void OnReset()
    {
        deadline = 0;
        lastCall = "";
        if (Controller.TryGetElementByName("Stand", out var marker)) marker.Enabled = false;
    }

    public override void OnDisable()
    {
        OnReset();
        voice?.Dispose();
        voice = null;
    }
}
