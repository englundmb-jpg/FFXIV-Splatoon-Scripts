// Adapted from PunishXIV/Splatoon; original authors retained in Metadata.
// NAUR review 2026-10-09. See TEA_NAUR_R1_README.md for coverage and validation.
using Dalamud.Game.ClientState.Objects.Types;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Hooks.ActionEffectTypes;
using ECommons.ImGuiMethods;
using ECommons.Logging;
using ECommons.Throttlers;
using Dalamud.Bindings.ImGui;
using Splatoon;
using Splatoon.SplatoonScripting;
using System.Collections.Generic;
using System.Linq;
using Vector3 = System.Numerics.Vector3;

using ECommons.DalamudServices.Legacy;

namespace MaggieSplatoon.TEA;

public class TEA_P4_Fate_Projection_α : SplatoonScript
{
    private readonly Dictionary<uint, uint> _futurePlayers = [];

    private FutureActionType[] _futureActionTypes =
    [
        FutureActionType.None,
        FutureActionType.None,
        FutureActionType.None
    ];

    private bool _isOpenSafeSpot;
    private bool _isStartFateProjectionCasting;
    private uint? _myFuturePlayer;

    private IBattleChara? SafeAlexander =>
        Svc.Objects.OfType<IBattleNpc>()
            .FirstOrDefault(x =>
                x is
                {
                    NameId: 0x2352,
                    IsCasting: true,
                    CastActionId: 18858
                });

    public override HashSet<uint>? ValidTerritories => [887];
    public override Metadata? Metadata => new(104, "Garume; Maggie accessibility repair");

    private string GetFutureActionText(FutureActionType type)
    {
        return type switch
        {
            FutureActionType.FirstMotion =>
                Loc(en: "FIRST: MOVE", jp: "最初は動け！"),

            FutureActionType.FirstStillness =>
                Loc(en: "FIRST: STOP ALL ACTIONS", jp: "最初は動くな"),

            FutureActionType.SecondMotion =>
                Loc(en: "SECOND: MOVE", jp: "最後は動け！"),

            FutureActionType.SecondStillness =>
                Loc(en: "SECOND: STOP ALL ACTIONS", jp: "最後は動くな！"),

            FutureActionType.Defamation =>
                Loc(en: "DEFAMATION: SAFE CLONE", jp: "名誉罰: 上へ"),

            FutureActionType.SharedSentence =>
                Loc(en: "STACK: LEFT", jp: "集団罰: 左下へ"),

            FutureActionType.Aggravated =>
                Loc(en: "ASSAULT: RIGHT", jp: "加重罰: 右下へ"),

            FutureActionType.Nothing =>
                Loc(en: "NO DEBUFF: LEFT", jp: "無職: 左下へ"),

            FutureActionType.UnKnown =>
                Loc(en: "UNKNOWN — CHECK CLONE", jp: "UnKnown: 左下へ？"),

            _ =>
                Loc(en: "WAIT FOR CLONE", jp: "None: 左下へ？")
        };
    }

    public override void OnStartingCast(uint source, uint castId)
    {
        if(castId == 18555)
        {
            Controller.CancelSchedulers();
            OnReset();
            _isStartFateProjectionCasting = true;

            Controller.Schedule(() =>
            {
                if(Controller.TryGetElementByName(
                    "FirstText",
                    out var firstTextElement))
                {
                    firstTextElement.overlayTextColor =
                        EColor.Red.ToUint();
                }
            }, 34 * 1000);

            Controller.Schedule(() =>
            {
                if(Controller.TryGetElementByName(
                    "FirstText",
                    out var firstTextElement))
                {
                    firstTextElement.overlayTextColor =
                        EColor.White.ToUint();
                }

                if(Controller.TryGetElementByName(
                    "ThirdText",
                    out var thirdTextElement))
                {
                    thirdTextElement.overlayTextColor =
                        EColor.Red.ToUint();
                }
            }, 39 * 1000);

            Controller.Schedule(() =>
            {
                if(Controller.TryGetElementByName(
                    "ThirdText",
                    out var thirdTextElement))
                {
                    thirdTextElement.overlayTextColor =
                        EColor.White.ToUint();
                }

                _isStartFateProjectionCasting = false;
            }, 44 * 1000);
        }
    }

    public override void OnReset()
    {
        Controller.CancelSchedulers();
        _isStartFateProjectionCasting = false;
        Controller.GetRegisteredElements().Each(x => { x.Value.Enabled = false; x.Value.overlayTextColor = 0xFFFFFFFF; });

        _futureActionTypes =
        [
            FutureActionType.None,
            FutureActionType.None,
            FutureActionType.None
        ];

        _futurePlayers.Clear();
        _myFuturePlayer = null;
        _isOpenSafeSpot = false;

        EzThrottler.Reset(
            "FateProjectionAlphaActionEffectDelay");
    }

    public override void OnSetup()
    {
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

        var thirdTextElement = new Element(0)
        {
            overlayText = "",
            overlayVOffset = 2f,
            overlayFScale = 2f,
            Filled = false,
            radius = 0f
        };

        thirdTextElement.SetOffPosition(
            new Vector3(100f, 0, 100f));

        Controller.RegisterElement(
            "ThirdText",
            thirdTextElement,
            true);

        Controller.RegisterElementFromCode(
            "NorthBait",
            "{\"Name\":\"北側サークル\",\"type\":1,\"offY\":7.0,\"radius\":3.0,\"color\":3372169472,\"fillIntensity\":0.0,\"thicc\":5.0,\"refActorNPCNameID\":9042,\"refActorRequireCast\":true,\"refActorCastId\":[18858],\"refActorUseCastTime\":true,\"refActorCastTimeMax\":30.0,\"refActorUseOvercast\":true,\"refActorComparisonType\":6,\"includeRotation\":true,\"onlyUnTargetable\":true,\"onlyVisible\":true,\"refActorTetherTimeMin\":0.0,\"refActorTetherTimeMax\":0.0}");

        Controller.RegisterElementFromCode(
            "SouthEastBait",
            "{\"Name\":\"南側サークル1\",\"type\":1,\"offX\":-2.5,\"offY\":41.5,\"radius\":1.0,\"color\":3372169472,\"fillIntensity\":0.0,\"thicc\":5.0,\"refActorNPCNameID\":9042,\"refActorRequireCast\":true,\"refActorCastId\":[18858],\"refActorUseCastTime\":true,\"refActorCastTimeMax\":30.0,\"refActorUseOvercast\":true,\"refActorComparisonType\":6,\"includeRotation\":true,\"onlyUnTargetable\":true,\"onlyVisible\":true,\"refActorTetherTimeMin\":0.0,\"refActorTetherTimeMax\":0.0}");

        Controller.RegisterElementFromCode(
            "SouthWestBait",
            "{\"Name\":\"南側サークル2\",\"type\":1,\"offX\":2.5,\"offY\":41.5,\"radius\":1.0,\"color\":3372169472,\"fillIntensity\":0.0,\"thicc\":5.0,\"refActorNPCNameID\":9042,\"refActorRequireCast\":true,\"refActorCastId\":[18858],\"refActorUseCastTime\":true,\"refActorCastTimeMax\":30.0,\"refActorUseOvercast\":true,\"refActorComparisonType\":6,\"includeRotation\":true,\"onlyUnTargetable\":true,\"onlyVisible\":true,\"refActorTetherTimeMin\":0.0,\"refActorTetherTimeMax\":0.0}");
    }

    public override void OnUpdate()
    {
        if(_isStartFateProjectionCasting && (Player.Object == null || Player.Object.CurrentHp == 0))
        { OnReset(); return; }
        if(!_isStartFateProjectionCasting)
        {
            Controller.GetRegisteredElements()
                .Each(x => x.Value.Enabled = false);

            return;
        }

        ApplyTextFromFutureAction(
            "FirstText",
            _futureActionTypes[0]);

        ApplyTextFromFutureAction(
            "SecondText",
            _futureActionTypes[1]);

        ApplyTextFromFutureAction(
            "ThirdText",
            _futureActionTypes[2]);

        var safeAlexander = SafeAlexander;

        if(safeAlexander == null || _isOpenSafeSpot || _futureActionTypes[1] == FutureActionType.None)
            return;

        _isOpenSafeSpot = true;

        switch(_futureActionTypes[1])
        {
            case FutureActionType.Defamation:
                ApplyBaitStyle("NorthBait");
                break;

            case FutureActionType.Aggravated:
                ApplyBaitStyle("SouthEastBait");
                break;

            case FutureActionType.SharedSentence:
            case FutureActionType.Nothing:
            case FutureActionType.None:
            case FutureActionType.FirstMotion:
            case FutureActionType.FirstStillness:
            case FutureActionType.SecondMotion:
            case FutureActionType.SecondStillness:
            case FutureActionType.UnKnown:
            default:
                ApplyBaitStyle("SouthWestBait");
                break;
        }
    }

    public override void OnSettingsDraw()
    {
        if(ImGuiEx.CollapsingHeader("Debug"))
        {
            ImGui.Text(
                $"_futureActionTypes[0]: {_futureActionTypes[0]}");

            ImGui.Text(
                $"_futureActionTypes[1]: {_futureActionTypes[1]}");

            ImGui.Text(
                $"_futureActionTypes[2]: {_futureActionTypes[2]}");
        }
    }

    private void ApplyTextFromFutureAction(
        string elementName,
        FutureActionType type)
    {
        if(type == FutureActionType.None)
            return;

        if(!Controller.TryGetElementByName(
            elementName,
            out var element))
            return;

        var text = GetFutureActionText(type);

        element.overlayText = text;
        element.Enabled = true;
    }

    private void ApplyBaitStyle(string elementName)
    {
        if(!Controller.TryGetElementByName(
            elementName,
            out var element))
            return;

        element.Enabled = true;
        element.tether = true;

        element.color = 0xFF00FF00;
        element.overlayText = "CURRENT";
        element.overlayFScale = 1.7f;

        element.thicc = 5f;
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

        _futurePlayers[source] = target;

        if(Player.Object != null && source == Player.Object.EntityId)
        {
            _myFuturePlayer = target;

        }
        Controller.Schedule(() =>
            {
                if(!_isStartFateProjectionCasting || _futurePlayers.Count != 8
                   || _futurePlayers.Values.Distinct().Count() != 8 || _myFuturePlayer == null) return;
                var reversed = OrderedClones(_futurePlayers);
                if(reversed.Length != 8) return;

                for(var i = 0; i < reversed.Length; i++)
                {
                    if(_myFuturePlayer == reversed[i])
                    {
                        switch(i)
                        {
                            case 0:
                                _futureActionTypes[1] =
                                    FutureActionType.SharedSentence;
                                break;

                            case 1:
                                _futureActionTypes[1] =
                                    FutureActionType.Defamation;
                                break;

                            case 2:
                            case 3:
                            case 4:
                                _futureActionTypes[1] =
                                    FutureActionType.Aggravated;
                                break;

                            case 5:
                            case 6:
                            case 7:
                                _futureActionTypes[1] =
                                    FutureActionType.Nothing;
                                break;
                        }
                    }
                }
            }, 1000);
    }

    public override void OnActionEffectEvent(
        ActionEffectSet set)
    {
        if(!_isStartFateProjectionCasting)
            return;

        if(set is
        {
            Action: not null,
            Source: not null,
            Target: not null
        })
        {
            switch(set.Action.Value.RowId)
            {
                case 19213: _futureActionTypes[0] = FutureActionType.FirstMotion; break;
                case 19214: _futureActionTypes[0] = FutureActionType.FirstStillness; break;
                case 18585: _futureActionTypes[2] = FutureActionType.SecondMotion; break;
                case 18586: _futureActionTypes[2] = FutureActionType.SecondStillness; break;
            }
        }
    }

    private enum FutureActionType : byte
    {
        None,
        FirstMotion,
        FirstStillness,
        SecondMotion,
        SecondStillness,
        Defamation,
        SharedSentence,
        Aggravated,
        Nothing,
        UnKnown
    }
}

