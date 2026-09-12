using Dalamud.Game.ClientState.Objects.SubKinds;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using Splatoon.Data;
using Splatoon.Memory;
using Splatoon.SplatoonScripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MaggieScripts.Duties.Endwalker;

public sealed class P12S_R1_R2_Accessible_v1 : SplatoonScript
{
        public override HashSet<uint>? ValidTerritories { get; } = [1154];
        public override Metadata? Metadata => new(1, "Memoria — P12S ranged accessibility");

        private const uint Green = 4278255360;
        private const uint Cyan = 4294967040;

        private const uint Paradeigma = 33517;
        private const uint EngravementOfSouls = 33541;
        private const uint SuperchainTheory1 = 33498;
        private const uint Apodialogos = 33534;
        private const uint Peridialogos = 33535;

        private const uint Gaiaochos = 33574;
        private const uint SummonDarkness = 33583;
        private const uint GeocentrismVertical = 33577;
        private const uint GeocentrismCircle = 33578;
        private const uint GeocentrismHorizontal = 33579;
        private const uint ClassicalConcepts = 33585;
        private const uint CrushHelm = 33558;

        private const uint AlphaTarget = 3560;
        private const uint BetaTarget = 3561;

        private int paradeigmaCount;
        private int phase;
        private long hideAt;
        private long roleRefreshAt;
        private long roleHideAt;
        private string autoRole = "NOT FOUND";

        public override void OnSetup()
        {
            Controller.RegisterElementFromCode("CURRENT",
                "{\"Name\":\"CURRENT\",\"Enabled\":false,\"radius\":2.5,\"Donut\":0.35,\"color\":4278255360,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true,\"overlayBGColor\":4278190080,\"overlayTextColor\":4294967295,\"overlayFScale\":1.5,\"overlayText\":\"CURRENT\"}");
            Controller.RegisterElementFromCode("NEXT",
                "{\"Name\":\"NEXT\",\"Enabled\":false,\"radius\":2.2,\"Donut\":0.35,\"color\":4294967040,\"thicc\":8.0,\"FillStep\":1.0,\"tether\":true,\"LegacyFill\":true,\"overlayBGColor\":4278190080,\"overlayTextColor\":4294967295,\"overlayFScale\":1.5,\"overlayText\":\"NEXT\"}");
            Controller.RegisterElementFromCode("ROLE",
                "{\"Name\":\"ROLE\",\"Enabled\":false,\"radius\":0.0,\"thicc\":0.0,\"refActorType\":1,\"overlayBGColor\":4278190080,\"overlayTextColor\":4294967295,\"overlayVOffset\":2.7,\"overlayFScale\":1.5,\"overlayText\":\"AUTO ROLE\"}");

            OnReset();
        }

        public override void OnReset()
        {
            paradeigmaCount = 0;
            phase = 0;
            hideAt = 0;
            roleRefreshAt = 0;
            roleHideAt = 0;
            HideAll();
        }

        public override void OnUpdate()
        {
            if (Environment.TickCount64 >= roleRefreshAt)
            {
                roleRefreshAt = Environment.TickCount64 + 2000;
                var detected = DetectRole();
                if (detected != autoRole)
                {
                    autoRole = detected;
                    roleHideAt = Environment.TickCount64 + 8000;
                }
            }

            if (hideAt != 0 && Environment.TickCount64 >= hideAt)
            {
                hideAt = 0;
                HidePositions();
            }

            if (Controller.TryGetElementByName("ROLE", out var role))
            {
                role.Enabled = roleHideAt > Environment.TickCount64;
                role.overlayText = $"P12S: {autoRole}";
            }
        }

        public override unsafe void OnStartingCast(
            uint sourceId,
            PacketActorCast* packet)
        {
            var castId = packet->ActionDescriptor;

            if (castId == Action(Paradeigma))
            {
                phase = 1;
                paradeigmaCount++;
                if (paradeigmaCount == 1)
                {
                    ShowAt(new Vector2(100, 100), "CURRENT: CENTER", RoleClock(), "NEXT: WING SAFE / OPPOSITE ADDS", 11000);
                }
                else if (paradeigmaCount == 2)
                {
                    ShowAt(RoleClock(), $"CURRENT: {ResolvedRole()} CLOCK", RolePair(), "NEXT: TETHER / TOWER", 14500);
                }
            }
            else if (castId == Action(EngravementOfSouls) && phase == 1 && paradeigmaCount == 2)
            {
                ShowAt(RoleClock(), $"CURRENT: {ResolvedRole()} START", RolePair(), "NEXT: CHECK YOUR DEBUFF", 13500);
            }
            else if (castId == Action(SuperchainTheory1))
            {
                phase = 1;
                ShowAt(RoleClock(), $"CURRENT: {ResolvedRole()} CLOCK", RolePair(), "NEXT: PAIR WITH HEALER", 19000);
            }
            else if (castId == Action(Apodialogos))
            {
                ShowAt(new Vector2(100, 100), "CURRENT: PARTY IN", new Vector2(100, 108), "NEXT: PARTY OUT", 7500);
            }
            else if (castId == Action(Peridialogos))
            {
                ShowAt(new Vector2(100, 108), "CURRENT: PARTY OUT", new Vector2(100, 100), "NEXT: PARTY IN", 7500);
            }
            else if (castId == Action(Gaiaochos))
            {
                phase = 2;
                ShowAt(new Vector2(100, 95), "CURRENT: STACK IN", new Vector2(100, 90), "NEXT: SMALL ARENA", 14500);
            }
            else if (castId == Action(SummonDarkness) && phase == 2)
            {
                ShowAt(new Vector2(100, 90), "CURRENT: WAIT FOR CHAIN", new Vector2(108, 90), "NEXT: DPS SAFE SIDE", 14500);
            }
            else if (castId == Action(GeocentrismVertical))
            {
                ShowAt(VerticalSpot(), $"CURRENT: {ResolvedRole()} VERTICAL", QSpreadSpot(), "NEXT: Q SPREAD", 10500);
            }
            else if (castId == Action(GeocentrismHorizontal))
            {
                ShowAt(HorizontalSpot(), $"CURRENT: {ResolvedRole()} HORIZONTAL", QSpreadSpot(), "NEXT: Q SPREAD", 10500);
            }
            else if (castId == Action(GeocentrismCircle))
            {
                ShowAt(QSpreadSpot(), $"CURRENT: {ResolvedRole()} TIGHT", QSpreadSpot(), "NEXT: HOLD", 10500);
            }
            else if (castId == Action(ClassicalConcepts))
            {
                ShowAt(new Vector2(100, 95), "CURRENT: FIND SYMBOL PARTNER", new Vector2(100, 92), ClassicalNextText(), 18000);
            }
            else if (castId == Action(CrushHelm))
            {
                HidePositions();
            }
        }

        private static ActionDescriptor Action(uint actionId) =>
            new(
                FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action,
                actionId);

        private string ClassicalNextText()
        {
            var player = Svc.ClientState.LocalPlayer;
            if (player == null) return "NEXT: CHECK ALPHA / BETA";
            if (player.StatusList.Any(x => x.StatusId == AlphaTarget)) return "NEXT: ALPHA -> RED";
            if (player.StatusList.Any(x => x.StatusId == BetaTarget)) return "NEXT: BETA -> YELLOW";
            return "NEXT: CHECK ALPHA / BETA";
        }

        private string DetectRole()
        {
            var player = Player.Object;
            if (player == null) return "NOT FOUND";

            uint[] rangedJobs = [23, 25, 27, 31, 35, 38, 42];
            var ranged = Svc.Party
                .Where(x => x.GameObject is IPlayerCharacter pc && rangedJobs.Contains(pc.ClassJob.RowId))
                .Select(x => (IPlayerCharacter)x.GameObject!)
                .ToList();

            var index = ranged.FindIndex(x => x.EntityId == player.EntityId);
            if (index == 0) return "R1";
            if (index == 1) return "R2";
            return "NOT RANGED";
        }

        private string ResolvedRole()
        {
            return autoRole;
        }

        private Vector2 RoleClock() => ResolvedRole() == "R2" ? new(106, 106) : new(94, 106);
        private Vector2 RolePair() => ResolvedRole() == "R2" ? new(104, 104) : new(96, 104);
        private Vector2 VerticalSpot() => ResolvedRole() == "R2" ? new(104, 94) : new(96, 94);
        private Vector2 HorizontalSpot() => ResolvedRole() == "R2" ? new(106, 92) : new(106, 88);
        private Vector2 QSpreadSpot() => ResolvedRole() == "R2" ? new(106, 96) : new(100, 90);

        private void ShowAt(Vector2 current, string currentText, Vector2 next, string nextText, int durationMs)
            => ShowAt(current.X, current.Y, currentText, next.X, next.Y, nextText, durationMs);

        private void ShowAt(float currentX, float currentY, string currentText,
                            float nextX, float nextY, string nextText, int durationMs)
        {
            if (Controller.TryGetElementByName("CURRENT", out var current))
            {
                current.refX = currentX;
                current.refY = currentY;
                current.color = Green;
                current.overlayText = currentText;
                current.Enabled = true;
            }
            if (Controller.TryGetElementByName("NEXT", out var next))
            {
                next.refX = nextX;
                next.refY = nextY;
                next.color = Cyan;
                next.overlayText = nextText;
                next.Enabled = true;
            }
            hideAt = Environment.TickCount64 + durationMs;
        }

        private void HidePositions()
        {
            if (Controller.TryGetElementByName("CURRENT", out var current)) current.Enabled = false;
            if (Controller.TryGetElementByName("NEXT", out var next)) next.Enabled = false;
        }

        private void HideAll()
        {
            HidePositions();
            if (Controller.TryGetElementByName("ROLE", out var role)) role.Enabled = false;
        }

}
