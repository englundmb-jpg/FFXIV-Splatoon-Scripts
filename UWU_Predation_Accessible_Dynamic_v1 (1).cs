using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.ExcelServices.TerritoryEnumeration;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SplatoonScriptsOfficial.Duties.Stormblood;

public sealed class UWU_Predation_Accessible_Dynamic : SplatoonScript
{
    public override HashSet<uint>? ValidTerritories { get; } =
        new() { Raids.the_Weapons_Refrain_Ultimate };

    public override Metadata? Metadata =>
        new(1, "OpenAI — accessibility test build");

    private const float CenterX = 100f;
    private const float CenterY = 100f;

    private bool waitingForSpawns;
    private bool markersDrawn;
    private long predationStartedAt;
    private uint ultimaSourceId;

    public override void OnSetup()
    {
        // Splatoon colors use ABGR:
        // 0xFF00FF00 = bright green
        // 0xFFFFFF00 = bright cyan
        Controller.RegisterElementFromCode(
            "Predation_Start",
            """{"Name":"START","radius":2.5,"Donut":0.35,"color":4278255360,"thicc":8.0,"tether":true}"""
        );

        Controller.RegisterElementFromCode(
            "Predation_Dodge",
            """{"Name":"DODGE","radius":2.2,"Donut":0.35,"color":4294967040,"thicc":8.0,"tether":true}"""
        );

        OnReset();
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        // 0x2B76 = Ultimate Predation
        if (castId != 0x2B76)
            return;

        ultimaSourceId = source;
        predationStartedAt = Environment.TickCount64;
        waitingForSpawns = true;
        markersDrawn = false;
        DisableMarkers();
    }

    public override void OnUpdate()
    {
        if (!waitingForSpawns || markersDrawn)
            return;

        // The primals finish moving into their Predation positions roughly
        // nine seconds after Ultimate Predation begins.
        var elapsed = Environment.TickCount64 - predationStartedAt;
        if (elapsed < 8500)
            return;

        // Stop trying after the useful window has passed.
        if (elapsed > 15000)
        {
            waitingForSpawns = false;
            return;
        }

        var ultima = ultimaSourceId.GetObject();
        var garuda = FindPredationActor("Garuda", 2f, 8f);
        var ifrit = FindPredationActor("Ifrit", 15f, 23f);
        var titan = FindPredationActor("Titan", 10f, 23f);

        if (ultima == null || garuda == null || ifrit == null || titan == null)
            return;

        var ultimaDistance = DistanceFromCenter(ultima.Position);
        if (ultimaDistance < 8f || ultimaDistance > 23f)
            return;

        if (!TryChooseRoute(
                garuda.Position,
                titan.Position,
                ultima.Position,
                ifrit.Position,
                out var startDirection,
                out var rotation))
            return;

        var start = CardinalStartPosition(startDirection);
        var dodge = DodgePosition(startDirection, rotation);

        if (!Controller.TryGetElementByName("Predation_Start", out var startElement) ||
            !Controller.TryGetElementByName("Predation_Dodge", out var dodgeElement))
            return;

        startElement.SetRefPosition(start);
        dodgeElement.SetRefPosition(dodge);
        startElement.Enabled = true;
        dodgeElement.Enabled = true;

        markersDrawn = true;
        waitingForSpawns = false;

        // Clear after Predation is over.
        Controller.ScheduleReset(12000);
    }

    public override void OnReset()
    {
        waitingForSpawns = false;
        markersDrawn = false;
        predationStartedAt = 0;
        ultimaSourceId = 0;
        DisableMarkers();
    }

    private void DisableMarkers()
    {
        if (Controller.TryGetElementByName("Predation_Start", out var start))
            start.Enabled = false;

        if (Controller.TryGetElementByName("Predation_Dodge", out var dodge))
            dodge.Enabled = false;
    }

    private static IGameObject? FindPredationActor(
        string name,
        float minimumDistance,
        float maximumDistance)
    {
        return Svc.Objects
            .Where(x => x != null && x.Name.TextValue.Equals(
                name,
                StringComparison.OrdinalIgnoreCase))
            .Where(x =>
            {
                var distance = DistanceFromCenter(x.Position);
                return distance >= minimumDistance && distance <= maximumDistance;
            })
            .OrderBy(x => MathF.Abs(
                DistanceFromCenter(x.Position) -
                ((minimumDistance + maximumDistance) / 2f)))
            .FirstOrDefault();
    }

    private static float DistanceFromCenter(Vector3 position)
    {
        var dx = position.X - CenterX;
        var dy = position.Z - CenterY;
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    // Direction numbering matches cactbot:
    // 0=N, 1=NE, 2=E, 3=SE, 4=S, 5=SW, 6=W, 7=NW.
    private static int ToEightDirection(Vector3 position)
    {
        var dx = position.X - CenterX;
        var dy = position.Z - CenterY;
        var angle = MathF.Atan2(dx, -dy);
        var direction = (int)MathF.Round(angle / (MathF.PI / 4f));
        return (direction % 8 + 8) % 8;
    }

    private static bool TryChooseRoute(
        Vector3 garudaPosition,
        Vector3 titanPosition,
        Vector3 ultimaPosition,
        Vector3 ifritPosition,
        out int startDirection,
        out int rotation)
    {
        startDirection = 0;
        rotation = 0;

        var garudaDirection = ToEightDirection(garudaPosition);
        if (garudaDirection % 2 == 0)
            return false;

        // Example: Garuda NW (7) gives E (2) and S (4).
        var safeDirections = new List<int>
        {
            (garudaDirection + 3) % 8,
            (garudaDirection + 5) % 8,
        };

        // Never start toward Titan.
        var titanDirection = ToEightDirection(titanPosition);
        safeDirections.RemoveAll(x => x == titanDirection);
        if (safeDirections.Count == 0)
            return false;

        // Prefer a cardinal that is not beside Ultima.
        var ultimaDirection = ToEightDirection(ultimaPosition);
        var notAdjacentToUltima = safeDirections
            .Where(x =>
                x != (ultimaDirection + 1) % 8 &&
                ultimaDirection != (x + 1) % 8)
            .ToList();

        if (notAdjacentToUltima.Count > 0)
            safeDirections = notAdjacentToUltima;

        var ifritDirection = ToEightDirection(ifritPosition);

        // 1) Prefer an early-safe dodge that neither runs into Ultima
        // nor lies on Ifrit's dash line.
        foreach (var direction in safeDirections)
        {
            foreach (var run in new[] { -1, 1 })
            {
                var finalDirection = (direction + run + 8) % 8;
                if (finalDirection == ultimaDirection)
                    continue;
                if (finalDirection % 4 == ifritDirection % 4)
                    continue;

                startDirection = direction;
                rotation = run;
                return true;
            }
        }

        // 2) Otherwise prefer the intercardinal opposite Garuda.
        var oppositeGaruda = (garudaDirection + 4) % 8;
        foreach (var direction in safeDirections)
        {
            foreach (var run in new[] { -1, 1 })
            {
                var finalDirection = (direction + run + 8) % 8;
                if (finalDirection == ultimaDirection)
                    continue;
                if (finalDirection != oppositeGaruda)
                    continue;

                startDirection = direction;
                rotation = run;
                return true;
            }
        }

        // 3) Last fallback: any remaining direction away from Ultima.
        foreach (var direction in safeDirections)
        {
            foreach (var run in new[] { -1, 1 })
            {
                var finalDirection = (direction + run + 8) % 8;
                if (finalDirection == ultimaDirection)
                    continue;

                startDirection = direction;
                rotation = run;
                return true;
            }
        }

        return false;
    }

    private static Vector3 CardinalStartPosition(int direction)
    {
        return direction switch
        {
            0 => new Vector3(100.0f, 0.0f, 82.2f),   // North
            2 => new Vector3(117.8f, 0.0f, 100.0f),  // East
            4 => new Vector3(100.0f, 0.0f, 117.8f),  // South
            6 => new Vector3(82.2f, 0.0f, 100.0f),   // West
            _ => new Vector3(100.0f, 0.0f, 100.0f),
        };
    }

    private static Vector3 DodgePosition(int startDirection, int rotation)
    {
        // These are the exact eight coordinates from the user's working
        // static Predation preset.
        return (startDirection, rotation) switch
        {
            (0, -1) => new Vector3(93.5f, 0.0f, 82.2f),   // N -> NW
            (0,  1) => new Vector3(106.5f, 0.0f, 82.2f),  // N -> NE
            (2, -1) => new Vector3(117.8f, 0.0f, 93.5f),  // E -> NE
            (2,  1) => new Vector3(117.8f, 0.0f, 106.5f), // E -> SE
            (4, -1) => new Vector3(106.5f, 0.0f, 117.8f), // S -> SE
            (4,  1) => new Vector3(93.5f, 0.0f, 117.8f),  // S -> SW
            (6, -1) => new Vector3(82.2f, 0.0f, 106.5f),  // W -> SW
            (6,  1) => new Vector3(82.2f, 0.0f, 93.5f),   // W -> NW
            _ => new Vector3(100.0f, 0.0f, 100.0f),
        };
    }
}
