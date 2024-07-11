using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Sequences;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public class ActGrimora : BaseAct
{
	public ActGrimora(DebugWindow window) : base(window)
	{
		m_mapSequence = new GrimoraMapSequence(this);
		m_cardBattleSequence = new GrimoraCardBattleSequence(window);
	}

	public override void Update()
	{
		
	}

	public override void OnGUI()
	{
		Window.LabelHeader("Grimora Act");
		if (RunState.Run.currentNodeId > 0 && Singleton<MapNodeManager>.m_Instance != null)
		{
			MapNode nodeWithId = Singleton<MapNodeManager>.Instance.GetNodeWithId(RunState.Run.currentNodeId);
			Window.Label("Current Node: " + RunState.Run.currentNodeId + " = " + nodeWithId, new(0, 120));
		}

        DrawCurrencyGUI();
        DrawItemsGUI();
		
		Window.StartNewColumn();
		OnGUICurrentNode();
	}

	public override void OnGUIMinimal()
	{
		OnGUICurrentNode();
	}

	private void OnGUICurrentNode()
	{
		GameFlowManager gameFlowManager = Singleton<GameFlowManager>.m_Instance;
		if (gameFlowManager == null)
			return;

        Window.LabelHeader(gameFlowManager.CurrentGameState.ToString());
        switch (gameFlowManager.CurrentGameState)
		{
			case GameState.CardBattle:
                m_cardBattleSequence.OnGUI();
				break;
			case GameState.Map:
                m_mapSequence.OnGUI();
				break;
			case GameState.FirstPerson3D:
                break;
			case GameState.SpecialCardSequence:
				Type nodeType = Helpers.LastSpecialNodeData.GetType();
				Window.Label($"<b>{nodeType.Name}</b>");
				if (nodeType == typeof(CardChoicesNodeData))
				{
                    OnGUICardChoiceNodeSequence();
					return;
                }
                if (GrimoraModHelper.Enabled)
                {
					switch (nodeType.Name.Replace("NodeData", ""))
					{
						case "BoneyardBurial":
                            GrimoraModHelper.OnGUIBoneyardBurial(this.Window);
							return;
						case "CardMerge":

							return;
                        case "ElectricChair":
                            GrimoraModHelper.OnGUIElectricChairNodeSequence(this.Window);
                            return;
                        case "GoatEye":
							GrimoraModHelper.OnGUIGoatEye(this.Window);
                            return;
                        case "GravebardCamp":
                            GrimoraModHelper.OnGUIGravebardCamp(this.Window);
                            return;
                    }
                }
                Window.Label($"Unhandled NodeData type: {nodeType.Name}");
                break;
			default:
				Window.Label($"Unhandled GameFlowState: {gameFlowManager.CurrentGameState}");
				break;
		}
	}

	private void OnGUICardChoiceNodeSequence()
	{
		CardSingleChoicesSequencer sequencer = Singleton<SpecialNodeHandler>.Instance.cardChoiceSequencer;
		Window.Label("Sequencer: " + sequencer, new(0, 80));
		if (Window.Button("Reroll choices"))
		{
			sequencer.OnRerollChoices();
		}
	}

	public override void Restart()
	{
		// TODO:
	}

	public override void Reload()
	{
		// TODO:
	}
}