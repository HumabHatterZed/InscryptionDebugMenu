﻿using DebugMenu.Scripts.Acts;
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
        base.OnGUI();
        if (GrimoraModHelper.Enabled)
            GrimoraModHelper.DisplayHammer(this.Window);
    }
}