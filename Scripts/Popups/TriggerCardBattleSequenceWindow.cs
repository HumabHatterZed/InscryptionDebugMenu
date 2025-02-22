﻿using DebugMenu.Scripts.Utils;
using DiskCardGame;
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public class TriggerCardBattleSequenceWindow : BaseWindow
{
    public enum BattleType
    {
        CardBattle,
        TotemBattle,
        BossBattle,
    }

    public override string PopupName => "Trigger Card Battle";
    public override Vector2 Size => new(630, 600);

    public BattleType SelectedBattleType
    {
        get => m_selectedBattleType;
        set => m_selectedBattleType = value;
    }

    public static Tribe EncounterTotemTopOverride = Tribe.None;
    public static Ability EncounterTotemBottomOverride = Ability.None;

    private BattleType m_selectedBattleType = BattleType.CardBattle;
    private Tribe m_totemTop = Tribe.None;
    private Ability m_totemBottom = Ability.None;
    private int difficulty = 1;
    private EncounterBlueprintData m_blueprintData = null;
    private Opponent.Type m_opponent = Opponent.Type.ProspectorBoss;

    private ButtonDisabledData DisableCard() => new()
    {
        Disabled = m_selectedBattleType == BattleType.CardBattle
    };
    private ButtonDisabledData DisableTotem() => new()
    {
        Disabled = m_selectedBattleType == BattleType.TotemBattle
    };
    private ButtonDisabledData DisableBoss() => new()
    {
        Disabled = m_selectedBattleType == BattleType.BossBattle
    };

    public override void OnGUI()
    {
        base.OnGUI();
        LabelHeader(PopupName, leftAligned: true);
        DrawTools();
        StartNewColumn();
        DrawSequenceDetails();
        Padding();

        if (Button("Trigger Sequence", disabled: ValidateTriggerButton) && CanTriggerSequence())
        {
            TriggerSequence();
        }
    }

    private ButtonDisabledData ValidateTriggerButton()
    {
        if (m_selectedBattleType == BattleType.BossBattle)
        {
            if (m_opponent is Opponent.Type.Default or Opponent.Type.NUM_TYPES)
            {
                return new ButtonDisabledData("Select an opponent");
            }
        }
        else if (m_blueprintData == null)
        {
            return new ButtonDisabledData("Select a blueprint");
        }

        return default;
    }

    private void DrawTools()
    {
        Label("<b>Card Battle Types</b>");

        if (Button("Card Battle", disabled: DisableCard))
        {
            m_selectedBattleType = BattleType.CardBattle;
        }

        if (Button("Totem Battle", disabled: DisableTotem))
        {
            m_selectedBattleType = BattleType.TotemBattle;
        }

        if (Button("Boss Battle", disabled: DisableBoss))
        {
            m_selectedBattleType = BattleType.BossBattle;
        }

        Label("Difficulty");
        difficulty = IntField(difficulty);

        if (m_selectedBattleType == BattleType.BossBattle)
        {
            Label("Select Opponent");
            Helpers.DrawOpponentsGUI(this, (a) => m_opponent = a);
        }
        else
        {
            Label("Select Blueprint");
            Helpers.DrawBlueprintGUI(this, (a) => m_blueprintData = a);

            if (m_selectedBattleType == BattleType.TotemBattle)
            {
                Label("Totem");
                Helpers.DrawTribesGUI(this, (a) => m_totemTop = a);
                Helpers.DrawAbilitysGUI(this, (a) => m_totemBottom = a);
            }
        }
    }

    private void DrawSequenceDetails()
    {
        Label("<b>Type:</b> " + m_selectedBattleType);
        Label("<b>Difficulty:</b> " + difficulty);

        if (m_selectedBattleType == BattleType.BossBattle)
        {
            Label("<b>Opponent:</b> " + m_opponent);
        }
        else
        {
            Label("<b>Blueprint:</b> " + (m_blueprintData != null ? m_blueprintData.name : "null"));

            if (m_selectedBattleType == BattleType.TotemBattle)
            {
                Label("<b>Totem Top:</b> " + m_totemTop);
                Label("<b>Totem Bottom:</b> " + m_totemBottom);
            }
        }
    }

    private void TriggerSequence()
    {
        CardBattleNodeData bossBattleNodeData;
        switch (m_selectedBattleType)
        {
            case BattleType.CardBattle:
                bossBattleNodeData = new CardBattleNodeData
                {
                    blueprint = m_blueprintData
                };
                break;
            case BattleType.TotemBattle:
                TotemBattleNodeData data = new();
                bossBattleNodeData = data;
                data.blueprint = m_blueprintData;
                EncounterTotemTopOverride = m_totemTop;
                EncounterTotemBottomOverride = m_totemBottom;
                break;
            case BattleType.BossBattle:
                BossBattleNodeData battleNodeData = new();
                bossBattleNodeData = battleNodeData;
                battleNodeData.bossType = m_opponent;
                bossBattleNodeData.specialBattleId = BossBattleSequencer.GetSequencerIdForBoss(m_opponent);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        bossBattleNodeData.difficulty = difficulty;
        Singleton<GameFlowManager>.Instance.TransitionToGameState(GameState.CardBattle, bossBattleNodeData);

        Plugin.Instance.ToggleWindow<TriggerCardBattleSequenceWindow>();
    }

    private bool CanTriggerSequence()
    {
        return m_selectedBattleType switch
        {
            BattleType.BossBattle => true,
            _ => m_blueprintData != null,
        };
    }
}


[HarmonyPatch(typeof(EncounterBuilder), nameof(EncounterBuilder.Build))]
internal class TriggerCardBattleSequenceWindow_Patches
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        // === We want to turn this

        // encounterData.opponentTotem = BuildOpponentTotem(encounterData.Blueprint.dominantTribes[0], difficulty, encounterData.Blueprint.redundantAbilities);

        // === Into this

        // encounterData.opponentTotem = OverrideTotemData(BuildOpponentTotem(...));

        // ===

        List<CodeInstruction> codes = new(instructions);

        MethodInfo BuildOpponentTotem = AccessTools.Method(typeof(EncounterBuilder), nameof(EncounterBuilder.BuildOpponentTotem));
        MethodInfo OverrideTotemData = AccessTools.Method(typeof(TriggerCardBattleSequenceWindow_Patches), nameof(TriggerCardBattleSequenceWindow_Patches.OverrideTotemData));
        for (int i = 0; i < codes.Count; i++)
        {
            CodeInstruction codeInstruction = codes[i];
            if (codeInstruction.operand == BuildOpponentTotem)
            {
                codes.Insert(++i, new CodeInstruction(OpCodes.Call, OverrideTotemData));
                break;
            }
        }

        return codes;
    }

    public static TotemItemData OverrideTotemData(TotemItemData totemItemData)
    {
        if (TriggerCardBattleSequenceWindow.EncounterTotemTopOverride != Tribe.None)
        {
            totemItemData.top.prerequisites.tribe = TriggerCardBattleSequenceWindow.EncounterTotemTopOverride;
        }
        if (TriggerCardBattleSequenceWindow.EncounterTotemBottomOverride != Ability.None)
        {
            totemItemData.bottom.effectParams.ability = TriggerCardBattleSequenceWindow.EncounterTotemBottomOverride;
        }

        TriggerCardBattleSequenceWindow.EncounterTotemTopOverride = Tribe.None;
        TriggerCardBattleSequenceWindow.EncounterTotemBottomOverride = Ability.None;
        return totemItemData;
    }
}
