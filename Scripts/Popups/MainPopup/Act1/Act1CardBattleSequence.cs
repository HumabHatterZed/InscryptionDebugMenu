using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act1;

public class Act1CardBattleSequence : BaseCardBattleSequence
{
	public override int PlayerBones => ResourcesManager.Instance.PlayerBones;
	public override int ScalesBalance => LifeManager.Instance.Balance;
	public override int PlayerEnergy => ResourcesManager.Instance.PlayerEnergy;
	public override int PlayerMaxEnergy => ResourcesManager.Instance.PlayerMaxEnergy;
    public override CardDrawPiles CardDrawPiles => Singleton<Part1CardDrawPiles>.Instance;
    public Act1CardBattleSequence(DebugWindow window) : base(window)
	{
	}
	
/*	public override void OnGUI()
	{
		Opponent opp = TurnManager.m_Instance?.Opponent;
		if (opp == null)
			return;

        int difficulty = (Singleton<MapNodeManager>.m_Instance.GetNodeWithId(RunState.Run.currentNodeId).Data as CardBattleNodeData)?.difficulty ?? TurnManager.Instance.Opponent.Difficulty;
        Window.Label($"{TurnManager.Instance.Opponent.GetType()?.Name}\nBlueprint: {TurnManager.Instance.Opponent.Blueprint.name}");
        Window.Label($"Difficulty: {difficulty + RunState.Run.DifficultyModifier} ({difficulty} + {RunState.Run.DifficultyModifier})" +
            $"\nTurn Number: {TurnManager.Instance.TurnNumber}");

        base.OnGUI();
	}*/
}