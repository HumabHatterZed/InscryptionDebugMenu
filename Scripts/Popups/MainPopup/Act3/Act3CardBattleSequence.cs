using System.Collections;
using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Act3;

public class Act3CardBattleSequence : BaseCardBattleSequence
{
	public override int PlayerBones => ResourcesManager.Instance.PlayerBones;
	public override int ScalesBalance => LifeManager.Instance.Balance;
	public override int PlayerEnergy => ResourcesManager.Instance.PlayerEnergy;
	public override int PlayerMaxEnergy => ResourcesManager.Instance.PlayerMaxEnergy;
	public override CardDrawPiles CardDrawPiles => Singleton<Part3CardDrawPiles>.Instance;
	public Act3CardBattleSequence(DebugWindow window) : base(window)
	{
	}

	public override void OnGUI()
	{
		MapNodeManager mapNodeManager = Singleton<MapNodeManager>.m_Instance;
		if (mapNodeManager == null)
			return;
        Window.Label($"{TurnManager.Instance.Opponent.GetType()}\nBlueprint: {TurnManager.Instance.Opponent.Blueprint.name}");
        MapNode nodeWithId = mapNodeManager.GetNodeWithId(RunState.Run.currentNodeId);
        if (nodeWithId?.Data is CardBattleNodeData cardBattleNodeData)
		{
            Window.Label($"Difficulty: {cardBattleNodeData.difficulty + RunState.Run.DifficultyModifier} ({cardBattleNodeData.difficulty} + {RunState.Run.DifficultyModifier})" +
                $"\nTurn Number: {TurnManager.Instance.TurnNumber}");
        }

        base.OnGUI();
	}

	public override void AddBones(int amount)
	{
		if (P03ModHelper.Enabled)
			base.AddBones(amount);
	}
	public override void RemoveBones(int amount)
	{
        if (P03ModHelper.Enabled)
			base.RemoveBones(amount);
    }
}