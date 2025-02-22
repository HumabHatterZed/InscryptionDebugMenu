﻿using DebugMenu.Scripts.Acts;
using DiskCardGame;
using GBC;

namespace DebugMenu.Scripts.Act2;

public class Act2CardBattleSequence : BaseCardBattleSequence
{
    public override int PlayerBones => IsGBCBattle() ? PixelResourcesManager.Instance.PlayerBones : 0;
    public override int ScalesBalance => IsGBCBattle() ? PixelLifeManager.Instance.Balance : 0;
    public override int PlayerEnergy => IsGBCBattle() ? PixelResourcesManager.Instance.PlayerEnergy : 0;
    public override int PlayerMaxEnergy => IsGBCBattle() ? PixelResourcesManager.Instance.PlayerMaxEnergy : 0;
    public override CardDrawPiles CardDrawPiles => Singleton<PixelCardDrawPiles>.m_Instance;
    public Act2CardBattleSequence(DebugWindow window) : base(window)
    {
        hasSideDeck = false;
    }

    public override void OnGUI()
    {
        if (TurnManager.m_Instance == null)
            return;

        Window.Label("Turn Number: " + TurnManager.Instance.TurnNumber);
        base.OnGUI();
    }

    public override void DrawSideDeck() { }

    public override void SetMaxEnergyToMax()
    {
        ResourcesManager.Instance.StartCoroutine(ResourcesManager.Instance.AddMaxEnergy(6));
    }
}