using DebugMenu.Scripts.Acts;
using DiskCardGame;

namespace DebugMenu.Scripts.Grimora;

public class GrimoraCardBattleSequence : BaseCardBattleSequence
{
    public override int PlayerBones => ResourcesManager.Instance.PlayerBones;
    public override int ScalesBalance => LifeManager.Instance.Balance;
    public override int PlayerEnergy => ResourcesManager.Instance.PlayerEnergy;
    public override int PlayerMaxEnergy => ResourcesManager.Instance.PlayerMaxEnergy;
    public override CardDrawPiles CardDrawPiles => Singleton<GrimoraCardDrawPiles>.Instance;
    public GrimoraCardBattleSequence(DebugWindow window) : base(window)
    {
    }

    public override void OnGUI()
    {
        /*		TurnManager turnManager = Singleton<TurnManager>.Instance;
                Window.Label($"{TurnManager.Instance.Opponent.GetType()}\nBlueprint: {TurnManager.Instance.Opponent.Blueprint.name}");
                Window.Label($"Difficulty: {turnManager.Opponent.Difficulty + RunState.Run.DifficultyModifier} ({turnManager.Opponent.Difficulty} + {RunState.Run.DifficultyModifier})" +
                    $"\nTurn Number: {TurnManager.Instance.TurnNumber}");
        */
        base.OnGUI();

        if (GrimoraModHelper.Enabled) GrimoraModHelper.DisplayHammer(this.Window);
    }
}