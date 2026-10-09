// Adapted from PunishXIV/Splatoon; original authors retained in Metadata.
// NAUR review 2026-10-09. See TEA_NAUR_R1_README.md for coverage and validation.
using ECommons;
using ECommons.GameHelpers;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using Splatoon;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using ECommons.DalamudServices.Legacy;

namespace MaggieSplatoon.TEA;

public class TEA_P4_Fate_Projection_β : SplatoonScript
{
    public enum FutureActionType : byte
    {
        EastUpperLeft,
        EastCenterLeft,
        EastLowerLeft,
        EastCenter,
        North,
        Spread,
        Stack,
        None
    }

    private readonly Dictionary<uint, uint> _futurePlayers = [];

    private bool _canAddFuturePlayer = true;
    private bool _hasDonutPosition;
    private bool _isStartFateProjectionCasting;
    private bool _myDefuffIsYellow;
    private uint? _myFuturePlayer;

    public override Metadata? Metadata => new(102, "Garume; Maggie accessibility repair");
    public override HashSet<uint>? ValidTerritories => [887];

    public override void OnStartingCast(uint source, uint castId)
    {
        if(castId == 19219)
        {
            Controller.CancelSchedulers();
            OnReset();
            _isStartFateProjectionCasting = true;
            PluginLog.Warning("Start Fate Projection Casting");

            Controller.Schedule(() =>
            {
                if(Controller.TryGetElementByName("FirstBait", out var firstElement))
                    firstElement.Enabled = false;

                if(Controller.TryGetElementByName("FirstText", out var firstTextElement))
                    firstTextElement.Enabled = false;

                if(Controller.TryGetElementByName("SecondText", out var secondElement))
                    secondElement.overlayTextColor = EColor.Red.ToUint();
            }, 1000 * 55);

            Controller.Schedule(() =>
            {
                _isStartFateProjectionCasting = false;

                if(Controller.TryGetElementByName("SecondBait", out var secondElement))
                    secondElement.tether = false;
            }, 1000 * 70);
        }
    }

    public override void OnReset()
    {
        Controller.CancelSchedulers();
        _isStartFateProjectionCasting = false;
        _futurePlayers.Clear();
        _myFuturePlayer = null;
        _myDefuffIsYellow = false;
        _canAddFuturePlayer = true;
        _hasDonutPosition = false;

        Controller.GetRegisteredElements()
            .Each(x => x.Value.Enabled = false);
    }

    public override void OnSetup()
    {
        var element = new Element(0)
        {
            color = 0xFF00FF00,
            thicc = 5f
        };

        Controller.RegisterElement("FirstBait", element, true);

        var secondElement = new Element(0)
        {
            radius = 1f,
            color = 0xFF00FF00,
            overlayText = Loc(
                en: "Move beneath the enemy",
                jp: "足元へ！"),
            overlayFScale = 2f,
            overlayVOffset = 2f,
            thicc = 5f
        };

        Controller.RegisterElement("SecondBait", secondElement, true);

        var firstTextElement = new Element(0)
        {
            overlayText = "",
            overlayVOffset = 8f,
            overlayFScale = 2f,
            Filled = false,
            radius = 0f
        };

        firstTextElement.SetOffPosition(
            new Vector3(100f, 0, 100f));

        Controller.RegisterElement(
            "FirstText",
            firstTextElement,
            true);

        var secondTextElement = new Element(0)
        {
            overlayText = "",
            overlayVOffset = 5f,
            overlayFScale = 2f,
            Filled = false,
            radius = 0f
        };

        secondTextElement.SetOffPosition(
            new Vector3(100f, 0, 100f));

        Controller.RegisterElement(
            "SecondText",
            secondTextElement,
            true);
    }

    public override void OnUpdate()
    {
        if(_isStartFateProjectionCasting && (Player.Object == null || Player.Object.CurrentHp == 0))
        { OnReset(); return; }
        if(!_isStartFateProjectionCasting)
            Controller.GetRegisteredElements()
                .Each(x => x.Value.Enabled = false);
    }

    private string GetFutureActionText(FutureActionType type)
    {
        var stackDirection = _myDefuffIsYellow
            ? Loc(en: "NORTH GROUP", jp: "北に")
            : Loc(en: "CENTER / SOUTH GROUP", jp: "南に");

        return type switch
        {
            FutureActionType.EastCenter =>
                Loc(en: "Afterwards, move to the outer edge", jp: "終了後、外周に行け"),

            FutureActionType.EastCenterLeft =>
                Loc(en: "Afterwards, move to the outer edge", jp: "終了後、外周に行け"),

            FutureActionType.EastLowerLeft =>
                Loc(en: "Afterwards, move to the outer edge", jp: "終了後、外周に行け"),

            FutureActionType.EastUpperLeft =>
                Loc(en: "Afterwards, move to the outer edge", jp: "終了後、外周に行け"),

            FutureActionType.Spread =>
                Loc(en: "Spread out", jp: "散開"),

            FutureActionType.Stack =>
                Loc(
                    en: $"Stack {stackDirection}",
                    jp: $"頭割り {stackDirection}"),

            _ => Loc(en: "", jp: "")
        };
    }

    private Vector2 GetFutureActionPosition(FutureActionType type)
    {
        return type switch
        {
            FutureActionType.EastUpperLeft =>
                new Vector2(112f, 98.5f),

            FutureActionType.EastCenterLeft =>
                new Vector2(112f, 100f),

            FutureActionType.EastLowerLeft =>
                new Vector2(112f, 101.5f),

            FutureActionType.EastCenter =>
                new Vector2(113.75f, 100),

            FutureActionType.North =>
                new Vector2(92, 84f),

            FutureActionType.None =>
                Vector2.Zero,

            _ => Vector2.Zero
        };
    }

    public override void OnActionEffectEvent(ActionEffectSet set)
    {
        if(!_isStartFateProjectionCasting)
            return;

        // Real stack/spread resolution releases the donut move; no guessed 63s timer.
        if(set.Action is { RowId: 18572 or 18573 })
        {
            if(Controller.TryGetElementByName("SecondText", out var text)) text.Enabled = false;
            if(_hasDonutPosition && Controller.TryGetElementByName("SecondBait", out var bait))
            {
                bait.color = 0xFF00FF00;
                bait.overlayText = "CURRENT";
                bait.Enabled = true;
                bait.tether = true;
            }
            return;
        }
        if(set.Action is { RowId: 18566 }) { OnReset(); return; }

        if(set.Source is not { DataId: 0x2C55 })
            return;

        switch(set.Action)
        {
            case { RowId: 18592 }:
            {
                var text =
                    GetFutureActionText(FutureActionType.Spread);

                if(Controller.TryGetElementByName(
                    "SecondText",
                    out var textElement))
                {
                    textElement.overlayText = text;
                    textElement.Enabled = true;
                }

                break;
            }

            case { RowId: 18593 }:
            {
                var text =
                    _canAddFuturePlayer ? "STACK: ASSIGNMENT UNKNOWN" : GetFutureActionText(FutureActionType.Stack);

                if(Controller.TryGetElementByName(
                    "SecondText",
                    out var textElement))
                {
                    textElement.overlayText = text;
                    textElement.Enabled = true;
                }

                break;
            }
        }

        if(set.Action is { RowId: 18590 })
        {
            if(Controller.TryGetElementByName(
                "SecondBait",
                out var element))
            {
                _hasDonutPosition = true;
                element.SetOffPosition(set.Source.Position);
                element.color = 0xFFFFFF00;
                element.overlayText = "NEXT";
                element.tether = false;
                element.Enabled = true;
            }
        }
    }

    // Require a complete one-to-one owner/clone map; packet arrival order is irrelevant.
    private static uint[] OrderedClones(Dictionary<uint, uint> owners)
    {
        return owners.Count == 8 && owners.Values.Distinct().Count() == 8
            && owners.Keys.All(x => x != 0) && owners.Values.All(x => x != 0)
            ? owners.Values.OrderByDescending(x => x).ToArray() : [];
    }

    public override void OnTetherCreate(
        uint source,
        uint target,
        uint data2,
        uint data3,
        uint data5)
    {
        if(!_isStartFateProjectionCasting || data3 != 98 || source == 0 || target == 0)
            return;

        if(_canAddFuturePlayer)
            _futurePlayers[source] = target;

        if(Player.Object != null && source == Player.Object.EntityId)
        {
            _myFuturePlayer = target;

        }
        Controller.Schedule(() =>
            {
                if(!_isStartFateProjectionCasting || !_canAddFuturePlayer || _futurePlayers.Count != 8
                   || _futurePlayers.Values.Distinct().Count() != 8 || _myFuturePlayer == null) return;
                _canAddFuturePlayer = false;
                var reversed = OrderedClones(_futurePlayers);
                if(reversed.Length != 8) return;

                var futureAction = FutureActionType.None;

                for(var i = 0; i < reversed.Length; i++)
                {
                    if(_myFuturePlayer == reversed[i])
                    {
                        futureAction = i switch
                        {
                            0 => FutureActionType.EastCenter,
                            1 => FutureActionType.North,
                            2 => FutureActionType.EastCenterLeft,
                            3 => FutureActionType.EastUpperLeft,
                            4 => FutureActionType.EastUpperLeft,
                            5 => FutureActionType.EastUpperLeft,
                            6 => FutureActionType.EastLowerLeft,
                            7 => FutureActionType.EastUpperLeft,
                            _ => FutureActionType.None
                        };

                        if(i is 1 or 3 or 5 or 7)
                            _myDefuffIsYellow = true;
                    }
                }

                if(Controller.TryGetElementByName(
                    "FirstBait",
                    out var element))
                {
                    var position =
                        GetFutureActionPosition(futureAction);

                    element.SetOffPosition(
                        new Vector3(
                            position.X,
                            0,
                            position.Y));

                    if(futureAction == FutureActionType.None) return;
                    element.tether = true;
                    element.overlayText = "CURRENT";
                    element.radius = 0.8f;
                    element.Enabled = true;
                }

                if(Controller.TryGetElementByName(
                    "FirstText",
                    out var textElement))
                {
                    var myIndex = System.Array.IndexOf(reversed, _myFuturePlayer.Value);
                    var text = myIndex switch
                    {
                        0 => "NEXT: EAST WALL — JUMP",
                        2 => "NEXT: WEST WALL — JUMP",
                        6 => "NEXT: SOUTH WALL — JUMP",
                        _ => "NEXT: NORTH STACK"
                    };

                    textElement.overlayText = text;
                    textElement.Enabled = true;
                }
            }, 2000);
    }
}

