using BepInEx.Logging;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using DiskCardGame;

namespace DebugMenu.Scripts.Magnificus;

public class ActMagnificus : BaseAct
{
	public ActMagnificus(DebugWindow window) : base(window)
	{
		m_cardBattleSequence = new MagnificusCardBattleSequence(window);
	}

	public override void Update()
	{
	}
	
	public override void OnGUI()
	{
		Window.LabelHeader("Magnificus Act");
		Window.Label("Currently unsupported");
		OnGUICurrentNode();
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