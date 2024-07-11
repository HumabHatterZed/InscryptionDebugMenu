using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMenu.Scripts.Popups;

public class SequenceListPopup : ButtonListPopup
{
    private bool modifyNames = false;
    public override void OnGUI()
    {
        modifyNames = false;
        base.OnGUI();
    }
    public override void DrawExtraTools()
	{
        bool oldValue = Configs.m_hideGuids.Value;
        if (oldValue != base.Toggle("Hide mod GUIDs", ref Configs.m_hideGuids))
            modifyNames = true;
    }
    public override string ModifyButtonName(string name, string value)
    {
        if (modifyNames && Configs.HideModGUIDs)
            return value;

        return name;
    }
    /*    public override bool IsFiltered(string buttonName, string buttonValue)
        {
            return !IsIncompatibleSequences(buttonName, buttonValue);
        }
        public bool IsIncompatibleSequences(string buttonName, string buttonValue)
        {
            switch (Helpers.GetCurrentSavedAct())
            {
                case Helpers.Acts.Act1:
                    break;
                case Helpers.Acts.Act2:
                    break;
                case Helpers.Acts.Act3:
                    break;
                case Helpers.Acts.GrimoraAct:
                    break;
            }
            return false;
        }*/
}