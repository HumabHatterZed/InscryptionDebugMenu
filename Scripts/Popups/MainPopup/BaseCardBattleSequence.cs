using System.Collections;
using System.Security.Cryptography;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Popups.DeckEditorPopup;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using InscryptionAPI.Helpers;
using UnityEngine;
using static DebugMenu.Scripts.DrawableGUI;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseCardBattleSequence
{
	public bool IsGBCBattle() => SceneLoader.ActiveSceneName == "GBC_CardBattle";

	public abstract int PlayerBones { get; }
	public abstract int ScalesBalance { get; }
	public abstract int PlayerEnergy { get; }
	public abstract int PlayerMaxEnergy { get; }
	public abstract CardDrawPiles CardDrawPiles { get; }
	public CardDrawPiles3D CardDrawPiles3D => CardDrawPiles as CardDrawPiles3D;

	protected readonly DebugWindow Window;

	protected bool hasSideDeck = true;
	protected bool hasBones = true;
	private bool drawingTutorCard = false;
    private bool dealingScaleDamage = false;
	public BaseCardBattleSequence(DebugWindow window)
	{
		this.Window = window;
	}

    private bool DisableCardDraw()
    {
        return (CardDrawPiles?.Deck?.CardsInDeck).GetValueOrDefault() == 0 | (SaveManager.SaveFile.IsPart2 && !IsGBCBattle());
    }
    private ButtonDisabledData DisableSideDraw() => new()
    {
        Disabled = !hasSideDeck || (CardDrawPiles3D?.SideDeck?.CardsInDeck).GetValueOrDefault() == 0
    };
    private ButtonDisabledData DisableTutorDraw() => new()
    {
        Disabled = drawingTutorCard || DisableCardDraw()
    };

    public virtual void OnGUI()
	{
        Window.LabelHeader("CardBattle");
        Opponent opp = TurnManager.m_Instance?.Opponent;
        if (opp == null)
            return;

        int difficulty = (Singleton<MapNodeManager>.m_Instance.GetNodeWithId(RunState.Run.currentNodeId).Data as CardBattleNodeData)?.difficulty ?? TurnManager.Instance.Opponent.Difficulty;
        Window.Label($"{TurnManager.Instance.Opponent.GetType()?.Name}\nBlueprint: {TurnManager.Instance.Opponent.Blueprint?.name}", new(0, 80f));
        Window.Label($"Difficulty: {difficulty + RunState.Run.DifficultyModifier} ({difficulty} + {RunState.Run.DifficultyModifier})" +
            $"\nTurn Number: {TurnManager.Instance.TurnNumber}");
        
        using (Window.HorizontalScope(4))
		{
			if (Window.Button("Draw Main", disabled: () => new(() => DisableCardDraw())))
				DrawCard();

			if (Window.Button("Draw Side", disabled: DisableSideDraw))
				DrawSideDeck();

			if (Window.Button("Draw Tutor", disabled: DisableTutorDraw))
				Plugin.Instance.StartCoroutine(DrawTutor());

			if (Window.Button("Draw New", disabled: () => new(() => SaveManager.SaveFile.IsPart2 && !IsGBCBattle())))
                Plugin.Instance.ToggleWindow(typeof(DrawCustomCardPopup));
        }

        if (!hasBones)
        {
            Window.Label("No Bones in this act!");
        }
        else
        {
            using (Window.HorizontalScope(4))
            {
                Window.Label("Bones:\n" + PlayerBones);
                if (Window.Button("+5"))
                    AddBones(5);

                if (Window.Button("-5"))
                    RemoveBones(5);

                if (Window.Button("Clear"))
                    ClearBones();
            }
        }

		using (Window.HorizontalScope(4))
		{
			Window.Label("Scales:\n" + ScalesBalance);

			if (Window.Button("+2"))
                Plugin.Instance.StartCoroutine(DealDamage(2));

            if (Window.Button("-2"))
                Plugin.Instance.StartCoroutine(TakeDamage(2));

            if (Window.Button("Reset", disabled: () => new(() => dealingScaleDamage)))
                Plugin.Instance.StartCoroutine(ResetScale());
		}

		using (Window.HorizontalScope(4))
		{
			Window.Label($"Energy: \n{PlayerEnergy}\\{PlayerMaxEnergy}");

			if (Window.Button("-1"))
				RemoveEnergy(1);

			if (Window.Button("+1"))
				AddEnergy(1);

			if (Window.Button("Fill"))
				FillEnergy();
		}

		using (Window.HorizontalScope(4))
		{
			Window.Label("Max Energy");

			if (Window.Button("-1"))
				RemoveMaxEnergy(1);

			if (Window.Button("+1"))
				AddMaxEnergy(1);

			if (Window.Button("MAX"))
				SetMaxEnergyToMax();
		}

		if (Window.Button("Show Game Board"))
			Plugin.Instance.ToggleWindow<GameBoardPopup>();

		using (Window.HorizontalScope(2))
		{
			if (Window.Button("Auto-win battle"))
				AutoWinBattle();

			if (Window.Button("Auto-lose battle"))
				AutoLoseBattle();
		}
	}

    private IEnumerator DrawFromMainDeck()
    {
        drawingTutorCard = true;
        CardDrawPiles3D?.pile.Draw();
        yield return CardDrawPiles.DrawCardFromDeck();
        drawingTutorCard = false;
    }
    private IEnumerator DrawFromSideDeck()
    {
        CardDrawPiles3D.SidePile.Draw();
        yield return CardDrawPiles3D.DrawFromSidePile();
    }
    public IEnumerator DrawTutor()
	{
        drawingTutorCard = true;
        yield return CardDrawPiles.Deck.Tutor();
        if (ViewManager.m_Instance != null)
            Singleton<ViewManager>.Instance.Controller.LockState = ViewLockState.Unlocked;

        drawingTutorCard = false;
    }

    private IEnumerator AddEnergyAndIncreaseLimit(int amount)
	{
		if (ResourcesManager.Instance.PlayerEnergy + amount > ResourcesManager.Instance.PlayerMaxEnergy)
			yield return ResourcesManager.Instance.AddMaxEnergy(amount);

		yield return ResourcesManager.Instance.AddEnergy(amount);
	}
	public virtual void DrawCard()
	{
        if (CardDrawPiles != null)
        {
            if (CardDrawPiles.Deck.CardsInDeck > 0)
                Plugin.Instance.StartCoroutine(DrawFromMainDeck());
        }
        else
        {
            Plugin.Log.LogError("Couldn't draw from deck; can't find CardDrawPiles!");
        }
    }

	public virtual void DrawSideDeck()
	{
        if (CardDrawPiles3D != null)
        {
            if (CardDrawPiles3D.SideDeck.Cards.Count > 0)
            {
                Plugin.Instance.StartCoroutine(DrawFromSideDeck());
            }
        }
        else
        {
            Plugin.Log.LogError("Couldn't draw from side deck; can't find CardDrawPiles3D!");
        }
    }
	public virtual void AddBones(int amount)
	{
        Plugin.Instance.StartCoroutine(ResourcesManager.Instance.AddBones(amount));
    }
	public virtual void RemoveBones(int amount)
	{
        int bones = Mathf.Min(PlayerBones, amount);
        Plugin.Instance.StartCoroutine(ResourcesManager.Instance.SpendBones(bones));
    }
    public virtual void ClearBones()
    {
        Plugin.Instance.StartCoroutine(ResourcesManager.Instance.SpendBones(PlayerBones));
    }
	public virtual void SetMaxEnergyToMax()
	{
		if (ResourceDrone.m_Instance != null)
		{
            for (int i = ResourcesManager.Instance.PlayerMaxEnergy; i < 6; i++)
                Singleton<ResourceDrone>.Instance.OpenCell(i);
        }

        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(6));
    }
	public virtual void AddMaxEnergy(int amount)
	{
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(amount));
    }
	public virtual void RemoveMaxEnergy(int amount)
	{
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.RemoveMaxEnergy(amount));
    }
	public virtual void FillEnergy()
	{
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.RefreshEnergy());
    }
	public virtual void AddEnergy(int amount)
	{
        ResourcesManager.Instance.StartCoroutine(AddEnergyAndIncreaseLimit(amount));
    }
    public virtual void RemoveEnergy(int amount)
	{
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.SpendEnergy(amount));
    }

    public virtual IEnumerator TakeDamage(int amount)
    {
        dealingScaleDamage = true;
        if (Configs.DisablePlayerDamage)
        {
            Singleton<LifeManager>.Instance.PlayerDamage += amount;
            yield return Singleton<LifeManager>.Instance.scales?.AddDamage(amount, Configs.InstantScales ? 1 : amount, true, null);
        }
        else
            yield return Singleton<LifeManager>.Instance.ShowDamageSequence(amount, Configs.InstantScales ? 1 : amount, true, 0.125f, null, 0f, false);
        dealingScaleDamage = false;
    }
    public virtual IEnumerator DealDamage(int amount)
    {
        dealingScaleDamage = true;
        if (Configs.DisableOpponentDamage)
        {
            Singleton<LifeManager>.Instance.OpponentDamage += amount;
            yield return Singleton<LifeManager>.Instance.scales?.AddDamage(amount, Configs.InstantScales ? 1 : amount, false, null);
        }
        else
            yield return Singleton<LifeManager>.Instance.ShowDamageSequence(amount, Configs.InstantScales ? 1 : amount, false, 0.125f, null, 0f, false);
        dealingScaleDamage = false;
    }
    public virtual IEnumerator ResetScale()
    {
        dealingScaleDamage = true;
        yield return LifeManager.Instance.ShowResetSequence();
        dealingScaleDamage = false;
    }
    public virtual void AutoLoseBattle()
	{
        LifeManager lifeManager = Singleton<LifeManager>.Instance;
        int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
        if (Configs.DisablePlayerDamage)
            lifeManager.PlayerDamage += lifeLeft;
        else
            Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, Configs.InstantScales ? 1 : lifeLeft, true, 0.125f, null, 0f, false));
    }
	public virtual void AutoWinBattle()
	{
        LifeManager lifeManager = Singleton<LifeManager>.Instance;
        int lifeLeft = Mathf.Abs(lifeManager.Balance - 5);
        if (Configs.DisableOpponentDamage)
            lifeManager.OpponentDamage += lifeLeft;
        else
            Plugin.Instance.StartCoroutine(lifeManager.ShowDamageSequence(lifeLeft, Configs.InstantScales ? 1 : lifeLeft, false, 0.125f, null, 0f, false));
    }
}
