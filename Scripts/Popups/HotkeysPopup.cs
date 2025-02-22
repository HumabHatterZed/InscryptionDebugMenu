﻿using DebugMenu.Scripts.Hotkeys;
using DebugMenu.Scripts.Utils;
using UnityEngine;

namespace DebugMenu.Scripts.Popups;

public class HotkeysPopup : BaseWindow
{
    public override string PopupName => "Hotkeys";
    public override Vector2 Size => new(600, 1000);

    private string filterText;
    private HotkeyController.Hotkey adjustingHotkeyShortcut = null;
    private HotkeyController.Hotkey adjustingHotkeyFunction = null;

    private string pressedKeysString = "";

    public HotkeysPopup()
    {
        ColumnWidth = 600;
        RowHeight = 50;
    }

    public override void OnGUI()
    {
        base.OnGUI();

        Label("Filter", new(0, RowHeight / 2));
        filterText = TextField(filterText, new(0, RowHeight / 2));

        Padding();

        List<HotkeyController.Hotkey> hotkeys = Plugin.Hotkeys.Hotkeys;

        using (HorizontalScope(2))
        {
            LabelHeader("\nCurrent Hotkeys", leftAligned: true);
            if (Button("New Hotkey"))
            {
                hotkeys.Add(Plugin.Hotkeys.CreateNewHotkey());
                Plugin.Hotkeys.SaveHotKeys();
            }
        }
        Padding();

        int row = 0;
        for (int i = 0; i < hotkeys.Count; i++)
        {
            HotkeyController.Hotkey hotkey = hotkeys[i];
            HotkeyController.FunctionData data = Plugin.Hotkeys.GetFunctionData(hotkey.FunctionID);

            using (HorizontalScope(3 + HotkeyController.MaxArgumentsInFunctions))
            {
                if (Button("Delete", new Vector2(50, 0)))
                {
                    hotkeys.RemoveAt(i--);
                    Plugin.Hotkeys.SaveHotKeys();
                    continue;
                }

                string codes = hotkey.KeyCodes.Serialize(" ", true);
                string changingKey = adjustingHotkeyShortcut == hotkey ? "Press any key\n" + pressedKeysString : codes;
                if (Button(changingKey))
                {
                    if (adjustingHotkeyShortcut == null)
                    {
                        HotkeyController.OnHotkeyPressed += HotKeyPressed;
                        pressedKeysString = "";
                        adjustingHotkeyShortcut = hotkey;
                    }
                }

                if (ButtonListPopup.OnGUI<ButtonListPopup>(this, hotkey.FunctionID, "Change Hotkey Function\nCurrent: " + hotkey.FunctionID, GetListsOfAllFunctions,
                        OnChoseButtonCallback, i.ToString()))
                {
                    adjustingHotkeyFunction = hotkey;
                }

                if (data != null)
                {
                    for (int j = 0; j < data.ArgumentNames.Length; j++)
                    {
                        string name = data.ArgumentNames[j];
                        object value = hotkey.Arguments[j];

                        Label(name);
                        object currentText = value;
                        object newText = InputField(currentText, data.Arguments[j]);
                        if (CompareValues(currentText, newText))
                        {
                            hotkey.Arguments[j] = newText;
                            Plugin.Hotkeys.SaveHotKeys();
                        }
                    }
                }
                else
                {
                    Label("Function not found!");
                }
            }


            if (row > 10)
            {
                StartNewColumn();
                row = 0;
            }

            row++;
        }
    }

    private static bool CompareValues(object currentText, object newText)
    {
        if (currentText == null && newText == null)
        {
            return false;
        }

        if (currentText == null || newText == null)
        {
            return true;
        }

        return !currentText.Equals(newText);
    }

    private void OnChoseButtonCallback(int chosenIndex, string chosenValue, List<string> inventoryIndex)
    {
        if (chosenIndex == -1)
        {
            return;
        }

        adjustingHotkeyFunction.FunctionID = chosenValue;

        HotkeyController.FunctionData newData = Plugin.Hotkeys.GetFunctionData(adjustingHotkeyFunction.FunctionID);
        adjustingHotkeyFunction.Arguments = newData.Arguments.Select(Activator.CreateInstance).ToArray();


        Plugin.Hotkeys.SaveHotKeys();
    }

    private Tuple<List<string>, List<string>> GetListsOfAllFunctions()
    {
        List<string> names = new(Plugin.Hotkeys.AllFunctionData.Count);
        List<string> ids = new(Plugin.Hotkeys.AllFunctionData.Count);
        foreach (HotkeyController.FunctionData pair in Plugin.Hotkeys.AllFunctionData)
        {
            names.Add(pair.ID);
            ids.Add(pair.ID);
        }

        return new Tuple<List<string>, List<string>>(names, ids);
    }

    public void HotKeyPressed(List<KeyCode> codes, KeyCode pressedKey)
    {
        pressedKeysString = codes.Serialize(" ");
        if (pressedKey == KeyCode.Escape)
        {
            // Cancel
            adjustingHotkeyShortcut = null;
            HotkeyController.OnHotkeyPressed -= HotKeyPressed;
        }
        else if (pressedKey is not (KeyCode.LeftShift or KeyCode.RightShift or
                KeyCode.LeftControl or KeyCode.RightControl or
                KeyCode.LeftAlt or KeyCode.RightAlt or
                KeyCode.LeftCommand or KeyCode.RightCommand))
        {
            adjustingHotkeyShortcut.KeyCodes = codes.ToArray();
            adjustingHotkeyShortcut = null;
            HotkeyController.OnHotkeyPressed -= HotKeyPressed;
            Plugin.Hotkeys.SaveHotKeys();
        }
    }
}

