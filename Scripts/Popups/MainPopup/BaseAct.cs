using BepInEx.Logging;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Sequences;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using UnityEngine;

namespace DebugMenu.Scripts.Acts;

public abstract class BaseAct
{
    public BaseCardBattleSequence BattleSequence => m_cardBattleSequence;
    public BaseMapSequence MapSequence => m_mapSequence;

    public readonly ManualLogSource Logger;
    public readonly DebugWindow Window;

    protected BaseCardBattleSequence m_cardBattleSequence;
    protected BaseMapSequence m_mapSequence;

    public BaseAct(DebugWindow window)
    {
        Window = window;
        Logger = Plugin.Log;
    }

    public virtual void Update() { }

    public abstract void OnGUI();
    public abstract void OnGUIMinimal();

    public abstract void Reload();
    public abstract void Restart();

    public void Log(string log) => Logger.LogInfo($"[{GetType().Name}] {log}");
    public void Warning(string log) => Logger.LogWarning($"[{GetType().Name}] {log}");
    public void Error(string log) => Logger.LogError($"[{GetType().Name}] {log}");

    public void DrawCurrencyGUI()
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
    public void DrawItemsGUI()
    {
        Window.LabelHeader("Items");
        List<string> items = RunState.Run.consumables;

        using (Configs.VerticalItems ? Window.VerticalScope(RunState.Run.MaxConsumables) : Window.HorizontalScope(RunState.Run.MaxConsumables))
        {
            for (int i = 0; i < RunState.Run.MaxConsumables; i++)
            {
                string consumable = i >= items.Count ? null : items[i];
                string itemRulebookName = Helpers.GetConsumableByName(consumable);
                string itemName = itemRulebookName ?? (consumable ?? "None");
                ButtonListPopup.OnGUI<ButtonListPopup>(Window, itemName, "Change Item " + (i + 1), GetListsOfAllItems,
                    OnChoseButtonCallback, i.ToString());
            }
        }
    }

    public static void OnChoseButtonCallback(int chosenIndex, string chosenValue, string inventoryIndex)
    {
        List<string> currentItems = RunState.Run.consumables;
        int index = int.Parse(inventoryIndex);
        string selectedItem = index >= RunState.Run.consumables.Count ? null : RunState.Run.consumables[index];

        if (chosenValue == null)
        {
            ItemsManager.Instance.RemoveItemFromSaveData(selectedItem);
        }
        else
        {
            if (index >= currentItems.Count)
            {
                currentItems.Add(chosenValue);
            }
            else
            {
                currentItems[index] = chosenValue;
            }
        }

        foreach (ConsumableItemSlot slot in Singleton<ItemsManager>.Instance.consumableSlots)
        {
            if (slot.Item != null)
                slot.DestroyItem();
        }

        Singleton<ItemsManager>.Instance.UpdateItems(true);
    }

    private Tuple<List<string>, List<string>> GetListsOfAllItems()
    {
        List<ConsumableItemData> allConsumables = ItemsUtil.AllConsumables;

        List<string> names = new(allConsumables.Count);
        List<string> values = new(allConsumables.Count);

        names.Add("None"); // Option to set the item to null (Don't have an item in this slot)
        values.Add(null); // Option to set the item to null (Don't have an item in this slot) 
        for (int i = 0; i < allConsumables.Count; i++)
        {
            names.Add(allConsumables[i].rulebookName + "\n(" + allConsumables[i].name + ")");
            values.Add(allConsumables[i].name);
        }

        return new Tuple<List<string>, List<string>>(names, values);
    }

    public void DrawSequencesGUI()
    {
        ButtonListPopup.OnGUI<SequenceListPopup>(Window, "Trigger Sequence", "Trigger Sequence", GetListsOfSequences, OnChoseSequenceButtonCallback);
    }

    public static void OnChoseSequenceButtonCallback(int chosenIndex, string chosenValue, string metaData)
    {
        if (chosenIndex < 0 || chosenIndex >= Helpers.CurrentSequences.Count)
            return;

        Helpers.CurrentSequences[chosenIndex].Sequence();
    }

    private Tuple<List<string>, List<string>> GetListsOfSequences()
    {
        List<BaseTriggerSequence> sequences = Helpers.CurrentSequences;
        List<string> names = new(sequences.Count);
        List<string> values = new(sequences.Count);
        names.AddRange(sequences.ConvertAll(x => x.ButtonName));
        values.AddRange(sequences.ConvertAll(x => x.SequenceName));
        return new Tuple<List<string>, List<string>>(names, values);
    }
}