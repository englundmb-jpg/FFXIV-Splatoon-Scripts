using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Stormblood
{
    public sealed class UWU_Ifrit_Dash_Accessible : SplatoonScript
    {
        private const uint CrimsonCycloneCastId = 0x2B5F;

        private static readonly Vector3 Center =
            new(100f, 0f, 100f);

        private const float ArenaLimit = 30f;
        private const float NailMatchDistance = 7f;

        private sealed class NailInfo
        {
            public uint EntityId;
            public Vector3 Position;
            public bool DeathRecorded;
            public int MissingFrames;
        }

        private readonly Dictionary<uint, NailInfo> nails = new();
        private readonly List<NailInfo> deathOrder = new();

        private bool nailsCaptured;
        private bool nailOrderComplete;

        private bool dashSequenceActive;
        private int nextDashIndex;

        private bool markersVisible;
        private long markersShownAt;

        public override HashSet<uint> ValidTerritories => new() { 777 };

        public override Metadata? Metadata => new(2, "Maggie");

        public override void OnSetup()
        {
            Controller.RegisterElementFromCode(
                "IfritDash_Current",
                "{\"Name\":\"CURRENT\",\"Enabled\":false,\"radius\":2.5,\"Donut\":0.35,\"color\":4278255360,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true,\"overlayText\":\"CURRENT\",\"overlayBGColor\":4278190080,\"overlayTextColor\":4294967295,\"overlayFScale\":1.5}"
            );

            Controller.RegisterElementFromCode(
                "IfritDash_Next",
                "{\"Name\":\"NEXT\",\"Enabled\":false,\"radius\":2.2,\"Donut\":0.35,\"color\":4294967040,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true,\"overlayText\":\"NEXT\",\"overlayBGColor\":4278190080,\"overlayTextColor\":4294967295,\"overlayFScale\":1.5}"
            );

            OnReset();
        }

        public override void OnUpdate()
        {
            if (!nailsCaptured)
            {
                TryCaptureNails();
            }
            else if (!nailOrderComplete)
            {
                TrackNailDeaths();
            }

            if (markersVisible &&
                Environment.TickCount64 - markersShownAt > 4500)
            {
                HideMarkers();
                markersVisible = false;
            }
        }

        public override void OnStartingCast(uint source, uint castId)
        {
            if (castId != CrimsonCycloneCastId)
                return;

            if (!nailOrderComplete)
                return;

            // Do not react to the earlier Ifrit dash set.
            //
            // The nail-order dash set is only accepted once Ifrit/clones
            // have appeared back at the four actual nail locations.
            if (!NailDashLayoutPresent())
                return;

            var caster = Svc.Objects
                .OfType<IBattleNpc>()
                .FirstOrDefault(x => x.EntityId == source);

            if (caster == null)
                return;

            var nailIndex = FindMatchingNailIndex(caster.Position);

            if (nailIndex < 0)
                return;

            // When this second dash set first appears, begin with the
            // ACTUAL first nail that died on this pull.
            if (!dashSequenceActive)
            {
                if (nailIndex != 0)
                    return;

                dashSequenceActive = true;
                nextDashIndex = 0;
            }

            // Nail deaths determine Crimson Cyclone order.
            // Only accept the next dash if it matches the actual
            // recorded nail-death order from this pull.
            if (nailIndex != nextDashIndex)
                return;

            var start = caster.Position;
            var end = OppositePoint(start);

            ShowRoute(start, end);

            nextDashIndex++;

            if (nextDashIndex >= 4)
            {
                dashSequenceActive = false;
            }
        }

        public override void OnReset()
        {
            nails.Clear();
            deathOrder.Clear();

            nailsCaptured = false;
            nailOrderComplete = false;

            dashSequenceActive = false;
            nextDashIndex = 0;

            markersVisible = false;
            markersShownAt = 0;

            HideMarkers();
        }

        private void TryCaptureNails()
        {
            var liveNails = Svc.Objects
                .OfType<IBattleNpc>()
                .Where(x =>
                    x.Name.TextValue.Equals(
                        "Infernal Nail",
                        StringComparison.OrdinalIgnoreCase))
                .Where(x => HorizontalDistance(x.Position, Center) <= ArenaLimit)
                .Where(x => x.MaxHp > 0)
                .Where(x => x.CurrentHp > 0)
                .ToList();

            // There is one real set of four Infernal Nails in Ifrit.
            // Do not begin tracking until all four are visible together.
            if (liveNails.Count != 4)
                return;

            nails.Clear();
            deathOrder.Clear();

            foreach (var nail in liveNails)
            {
                nails[nail.EntityId] = new NailInfo
                {
                    EntityId = nail.EntityId,
                    Position = nail.Position,
                    DeathRecorded = false,
                    MissingFrames = 0
                };
            }

            nailsCaptured = true;
        }

        private void TrackNailDeaths()
        {
            foreach (var nail in nails.Values)
            {
                if (nail.DeathRecorded)
                    continue;

                var actor = Svc.Objects
                    .OfType<IBattleNpc>()
                    .FirstOrDefault(x => x.EntityId == nail.EntityId);

                if (actor != null)
                {
                    // Keep the last real position in case the object
                    // disappears immediately when it dies.
                    nail.Position = actor.Position;

                    if (actor.CurrentHp > 0)
                    {
                        nail.MissingFrames = 0;
                        continue;
                    }

                    RecordNailDeath(nail);
                    continue;
                }

                // A dead nail may disappear from the object table.
                // Require several missing frames so a one-frame object
                // table hiccup is not mistaken for a death.
                nail.MissingFrames++;

                if (nail.MissingFrames >= 3)
                {
                    RecordNailDeath(nail);
                }
            }
        }

        private void RecordNailDeath(NailInfo nail)
        {
            if (nail.DeathRecorded)
                return;

            nail.DeathRecorded = true;
            deathOrder.Add(nail);

            if (deathOrder.Count == 4)
            {
                nailOrderComplete = true;

                // deathOrder[3] is the ACTUAL last nail killed.
                // This remains correct even if PF did not follow Z order.
            }
        }

        private bool NailDashLayoutPresent()
        {
            if (deathOrder.Count != 4)
                return false;

            var ifritActors = Svc.Objects
                .OfType<IBattleNpc>()
                .Where(x =>
                    x.Name.TextValue.Contains(
                        "Ifrit",
                        StringComparison.OrdinalIgnoreCase))
                .Where(x => HorizontalDistance(x.Position, Center) <= ArenaLimit)
                .ToList();

            // For the nail-order dash set, there must be an Ifrit/clone
            // at every stored nail location.
            foreach (var nail in deathOrder)
            {
                var found = ifritActors.Any(ifrit =>
                    HorizontalDistance(
                        ifrit.Position,
                        nail.Position) <= NailMatchDistance);

                if (!found)
                    return false;
            }

            return true;
        }

        private int FindMatchingNailIndex(Vector3 casterPosition)
        {
            var bestIndex = -1;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < deathOrder.Count; i++)
            {
                var distance = HorizontalDistance(
                    casterPosition,
                    deathOrder[i].Position);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestIndex = i;
                }
            }

            if (bestDistance > NailMatchDistance)
                return -1;

            return bestIndex;
        }

        private static Vector3 OppositePoint(Vector3 start)
        {
            return new Vector3(
                Center.X * 2f - start.X,
                start.Y,
                Center.Z * 2f - start.Z
            );
        }

        private void ShowRoute(
            Vector3 currentPosition,
            Vector3 nextPosition)
        {
            if (Controller.TryGetElementByName(
                    "IfritDash_Current",
                    out var current))
            {
                current.SetOffPosition(currentPosition);
                current.Enabled = true;
            }

            if (Controller.TryGetElementByName(
                    "IfritDash_Next",
                    out var next))
            {
                next.SetOffPosition(nextPosition);
                next.Enabled = true;
            }

            markersVisible = true;
            markersShownAt = Environment.TickCount64;
        }

        private void HideMarkers()
        {
            if (Controller.TryGetElementByName(
                    "IfritDash_Current",
                    out var current))
            {
                current.Enabled = false;
            }

            if (Controller.TryGetElementByName(
                    "IfritDash_Next",
                    out var next))
            {
                next.Enabled = false;
            }
        }

        private static float HorizontalDistance(
            Vector3 a,
            Vector3 b)
        {
            var dx = a.X - b.X;
            var dz = a.Z - b.Z;

            return MathF.Sqrt(dx * dx + dz * dz);
        }
    }
}
