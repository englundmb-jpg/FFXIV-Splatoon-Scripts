// Adapted from PunishXIV/Splatoon; original authors retained in Metadata.
// NAUR review 2026-10-09. See TEA_NAUR_R1_README.md for coverage and validation.
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.Configuration;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using Dalamud.Bindings.ImGui;
using Splatoon;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;
using System.Linq;

using ECommons.DalamudServices.Legacy;

namespace MaggieSplatoon.TEA;

public class TEA_P2_Nisi : SplatoonScript
{
    private const uint AlphaNisiId = 2222;
    private const uint AlphaNisiId2 = 2224;
    private const uint BetaNisiId = 2223;
    private const uint BetaNisiId2 = 2225;
    private const uint DeltaNisiId = 2138;
    private const uint DeltaNisiId2 = 2140;
    private const uint GammaNisiId = 2137;
    private const uint GammaNisiId2 = 2139;

    private const uint JudgmentNisiCastId = 18494;
    private const uint OpeningLastJudgmentCastId = 18491;

    private const uint JusticeId = 9216;

    private readonly Dictionary<uint, List<uint>> _firstNisiPlayerPairs = [];

    private bool _isJudgmentNisi;
    private bool _isOpeningLastJudgment;

    private uint _myFirstNisiId;

    private static IEnumerable<uint> NisiIds =>
        new[] { AlphaNisiId, BetaNisiId, GammaNisiId, DeltaNisiId };

    private static IEnumerable<uint> NisiIds2 =>
        new[] { AlphaNisiId2, BetaNisiId2, GammaNisiId2, DeltaNisiId2 };

    public override HashSet<uint> ValidTerritories => [887];
    public override Metadata Metadata => new(103, "Garume; Maggie accessibility repair");

    private IBattleNpc? Justice =>
        Svc.Objects
            .OfType<IBattleNpc>()
            .FirstOrDefault(x => x.NameId == JusticeId && x.IsTargetable);

    private Config C => Controller.GetConfig<Config>();

    public override void OnSetup()
    {
        foreach(var nisiId in NisiIds)
            _firstNisiPlayerPairs[nisiId] = [];

        var nisiPassElement = new Element(0)
        {
            overlayText = Loc(
                en: "NISI PARTNER",
                jp: "交換対象",
                de: "Nisi Pass"),
            tether = true,
            radius = 0.8f,
            color = 0xFF00FF00,
            thicc = 5f,
            overlayFScale = 1.7f
        };

        Controller.RegisterElement("NisiPass", nisiPassElement);
    }

    public override void OnUpdate()
    {
        Controller
            .GetRegisteredElements()
            .Each(x => x.Value.Enabled = false);

        if(Player.Object == null || Player.Object.CurrentHp == 0) return;

        var justice = Justice;

        if(justice == null)
        {
            _isJudgmentNisi = false;
            _isOpeningLastJudgment = false;
            return;
        }

        if(justice is
        {
            IsCasting: true,
            CastActionId: JudgmentNisiCastId
        })
        {
            _isJudgmentNisi = true;
            _isOpeningLastJudgment = false;
        }

        if(justice is
        {
            IsCasting: true,
            CastActionId: OpeningLastJudgmentCastId
        })
        {
            _isJudgmentNisi = false;
            _isOpeningLastJudgment = true;
        }

        if(_isJudgmentNisi &&
           !_isOpeningLastJudgment &&
           !justice.IsCasting)
        {
            var nisiPlayers = Svc.Objects
                .OfType<IBattleChara>()
                .Where(x =>
                    x.StatusList.Any(y =>
                        NisiIds.Contains(y.StatusId)))
                .ToArray();

            if(_firstNisiPlayerPairs.First().Value.Count == 0)
            {
                if(nisiPlayers.Length == 8 && NisiIds.All(id => nisiPlayers.Count(p => p.StatusList.Any(s => s.StatusId == id)) == 2))
                {
                    foreach(var player in nisiPlayers)
                    {
                        var nisi = player.StatusList
                            .First(x =>
                                NisiIds.Contains(x.StatusId));

                        _firstNisiPlayerPairs[nisi.StatusId]
                            .Add(player.EntityId);

                        if(player == Player.Object)
                            _myFirstNisiId = nisi.StatusId;
                    }
                }
            }
            else
            {
                var myNisi =
                    Player.Object.StatusList
                        .FirstOrDefault(x =>
                            NisiIds.Contains(x.StatusId));

                if(!_firstNisiPlayerPairs.TryGetValue(_myFirstNisiId, out var pair)
                   || pair.Count != 2) return;
                var partnerId = pair.FirstOrDefault(x => x != Player.Object.EntityId);
                if(partnerId.GetObject() is not IBattleChara anotherPlayer || anotherPlayer.IsDead) return;

                var anotherPlayerNisi =
                    anotherPlayer.StatusList
                        .FirstOrDefault(x =>
                            NisiIds.Contains(x.StatusId));

                if(myNisi == null &&
                   anotherPlayerNisi != null &&
                   anotherPlayerNisi.RemainingTime < C.FirstNisiTime)
                {
                    SetPositionElement(
                        anotherPlayer,
                        "NisiPass");
                }

                if(anotherPlayerNisi == null &&
                   myNisi != null &&
                   myNisi.RemainingTime < C.FirstNisiTime)
                {
                    SetPositionElement(
                        anotherPlayer,
                        "NisiPass");
                }
            }
        }

        if(_isOpeningLastJudgment &&
           !justice.IsCasting)
        {
            var myMisi =
                Player.Object.StatusList
                    .FirstOrDefault(x =>
                        NisiIds.Contains(x.StatusId));

            var myNisi2 =
                Player.Object.StatusList
                    .FirstOrDefault(x =>
                        NisiIds2.Contains(x.StatusId));

            if(myMisi != null &&
               myMisi.RemainingTime < C.SecondNisiTime)
            {
                var matchingNisiId =
                    GetMatchingNisiId(myMisi.StatusId);

                var player = Svc.Objects
                    .OfType<IBattleChara>()
                    .Where(x =>
                        !x.StatusList.Any(y =>
                            NisiIds.Contains(y.StatusId)))
                    .FirstOrDefault(x =>
                        x.StatusList.Any(y =>
                            y.StatusId == matchingNisiId));

                SetPositionElement(
                    player,
                    "NisiPass");
            }

            if(myMisi == null &&
               myNisi2 != null)
            {
                var matchingNisiId =
                    GetMatchingNisiId(myNisi2.StatusId);

                var player = Svc.Objects
                    .OfType<IBattleChara>()
                    .FirstOrDefault(x =>
                        x.StatusList.Any(y =>
                            y.StatusId == matchingNisiId &&
                            y.RemainingTime < C.SecondNisiTime));

                SetPositionElement(
                    player,
                    "NisiPass");
            }

            if(myMisi != null &&
               myNisi2 != null &&
               myMisi.RemainingTime < C.SecondNisiTime)
            {
                var matchingNisiId =
                    GetMatchingNisiId(myMisi.StatusId);

                var player = Svc.Objects
                    .OfType<IBattleChara>()
                    .Where(x =>
                        !x.StatusList.Any(y =>
                            NisiIds.Contains(y.StatusId)))
                    .FirstOrDefault(x =>
                        x.StatusList.Any(y =>
                            y.StatusId == matchingNisiId));

                SetPositionElement(
                    player,
                    "NisiPass");
            }
        }
    }

    public override void OnReset()
    {
        _myFirstNisiId = 0;
        Controller.GetRegisteredElements().Each(x => x.Value.Enabled = false);
        _firstNisiPlayerPairs
            .Each(x => x.Value.Clear());

        _isJudgmentNisi = false;
        _isOpeningLastJudgment = false;
    }

    private uint GetMatchingNisiId(uint nisiId)
    {
        return nisiId switch
        {
            AlphaNisiId => AlphaNisiId2,
            BetaNisiId => BetaNisiId2,
            GammaNisiId => GammaNisiId2,
            DeltaNisiId => DeltaNisiId2,
            AlphaNisiId2 => AlphaNisiId,
            BetaNisiId2 => BetaNisiId,
            GammaNisiId2 => GammaNisiId,
            DeltaNisiId2 => DeltaNisiId,
            _ => 0
        };
    }

    private void SetPositionElement(
        IGameObject? player,
        string elementName)
    {
        if(player == null || player is IBattleChara { IsDead: true })
            return;

        if(Controller.TryGetElementByName(
            elementName,
            out var element))
        {
            element.Enabled = true;
            element.refX = player.Position.X;
            element.refY = player.Position.Z;
        }
    }

    public override void OnSettingsDraw()
    {
        ImGui.TextWrapped("Partner reminder only. First pass establishes the pair; it is not guided. For NAUR R1, wait until CC tank finishes mines before the second pass. A tether does not mean the direct path is safe.");
        ImGui.Text("1 ~ 2 Nisi");

        foreach(var pair in _firstNisiPlayerPairs)
        {
            ImGui.Text(
                $"{pair.Key.GetObject()}: {string.Join(", ", pair.Value)}");
        }

        ImGui.Spacing();

        ImGui.Text("Current Nisi");

        foreach(var nisi in
            Svc.Objects
                .OfType<IBattleChara>()
                .SelectMany(x => x.StatusList)
                .Where(x =>
                    NisiIds.Contains(x.StatusId)))
        {
            ImGui.Text(
                $"{nisi.StatusId}: {nisi.RemainingTime}");
        }
    }

    private class Config : IEzConfig
    {
        public float FirstNisiTime { get; } = 8f;
        public float SecondNisiTime { get; } = 15f;
    }
}

