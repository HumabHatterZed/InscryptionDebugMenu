using DebugMenu.Scripts.Acts;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Magnificus;

public class ActMagnificus : BaseAct
{
    public static bool SkipNextNode = false;
    public static bool ActivateAllMapNodesActive = false;

    public ActMagnificus(DebugWindow window) : base(window)
    {
        m_mapSequence = new ActMagnificusMapSequence(this);
        m_cardBattleSequence = new MagnificusCardBattleSequence(window);
    }

    public override void Update()
    {
    }

    public override void OnGUI()
    {
        Window.LabelHeader("Magnificus' Act");
        
        DrawCurrencyGUI();

        Window.StartNewColumn();
        OnGUICurrentNode();
    }

    private void DrawCurrencyGUI()
    {
        Window.LabelHeader("Currency: " + RunState.Run.currency);
        using (Window.HorizontalScope(4))
        {
            if (Window.Button("+1"))
                RunState.Run.currency++;

            if (Window.Button("-1"))
                RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 1);

            if (Window.Button("+5"))
                RunState.Run.currency += 5;

            if (Window.Button("-5"))
                RunState.Run.currency = Mathf.Max(0, RunState.Run.currency - 5);
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