﻿using DebugMenu.Scripts.Acts;

namespace DebugMenu.Scripts.Act2;

public class Act2MapSequence : BaseMapSequence
{
    private readonly Act2 Act = null;
    private readonly DebugWindow Window = null;

    public Act2MapSequence(Act2 act)
    {
        this.Act = act;
        this.Window = act.Window;
    }

    public override void OnGUI()
    {

    }

    public override void ToggleSkipNextNode()
    {
    }

    public override void ToggleAllNodes()
    {
    }
}