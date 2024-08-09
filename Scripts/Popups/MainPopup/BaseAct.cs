using BepInEx.Logging;
using DebugMenu.Scripts.Grimora;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Sequences;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
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

    public virtual void OnGUIMinimal()
    {
        OnGUICurrentNode();
    }

    public virtual void Reload()
    {
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        MenuController.ReturnToStartScreen();
        MenuController.LoadGameFromMenu(newGameGBC: false);
    }
    public abstract void Restart();

    public void ReloadKaycees()
    {
        Log("Reloading Ascension...");
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        SceneLoader.Load("Ascension_Configure");
        FrameLoopManager.Instance.SetIterationDisabled(disabled: false);
        SaveManager.savingDisabled = false;
        MenuController.LoadGameFromMenu(newGameGBC: false);
    }

    public void Log(string log) => Logger.LogInfo($"[{GetType().Name}] {log}");
    public void Warning(string log) => Logger.LogWarning($"[{GetType().Name}] {log}");
    public void Error(string log) => Logger.LogError($"[{GetType().Name}] {log}");

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

    public static void OnChoseButtonCallback(int chosenIndex, string chosenValue, List<string> inventoryIndex)
    {
        List<string> currentItems = RunState.Run.consumables;
        foreach (string s in inventoryIndex)
        {
            int index = int.Parse(s);
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
        ButtonListPopup.OnGUI<SequenceListPopup>(Window, "Trigger Sequence", "Trigger Sequence", GetListsOfSequences, OnChoseSequenceButtonCallback, "Custom");
    }

    public static void OnChoseSequenceButtonCallback(int chosenIndex, string chosenValue, List<string> metaData)
    {
        if (chosenIndex < 0 || chosenIndex >= Helpers.ShownSequences.Count)
            return;

        Helpers.ShownSequences[chosenIndex].Sequence();
    }
    
    internal static Tuple<List<string>, List<string>> GetListsOfSequences()
    {
        List<BaseTriggerSequence> sequences = Helpers.ShownSequences;
        List<string> names = new(sequences.Count);
        List<string> values = new(sequences.Count);
        names.AddRange(sequences.ConvertAll(x => x.ButtonName));
        values.AddRange(sequences.ConvertAll(x => x.SequenceName));
        return new Tuple<List<string>, List<string>>(names, values);
    }

    public virtual void OnGUICurrentNode()
    {
        GameFlowManager instance = GameFlowManager.m_Instance;
        if (instance == null)
            return;

        GameState state = instance.CurrentGameState;
        //Window.LabelHeader(state.ToString());
        switch (state)
        {
            case GameState.CardBattle:
                m_cardBattleSequence.OnGUI();
                break;
            case GameState.Map:
                Window.LabelHeader("Map");
                m_mapSequence.OnGUI();
                break;
            case GameState.FirstPerson3D:
                Window.LabelHeader("FirstPerson3D");
                break;
            case GameState.SpecialCardSequence:
                if (Helpers.LastSpecialNodeData == null)
                {
                    Window.LabelHeader("Null NodeData");
                    DrawSequencesGUI();
                    return;
                }

                Type nodeType = Helpers.LastSpecialNodeData.GetType();
                string nodeDataName = nodeType.Name.Replace("NodeData", "");
                Window.LabelHeader(nodeDataName);
                
                if (nodeType == typeof(CardChoicesNodeData))
                {
                    OnGUICardChoiceNodeSequence();
                    return;
                }
                
                if (!OnSpecialCardSequence(nodeDataName))
                {
                    Window.Label($"Unhandled NodeData Type!");
                    DrawSequencesGUI();
                }
                break;
            default:
                Window.LabelHeader(instance.CurrentGameState.ToString());
                Window.Label($"Unhandled GameFlowState!");
                DrawSequencesGUI();
                break;
        }
    }

    public virtual bool OnSpecialCardSequence(string nodeDataName)
    {
        return false;
    }
    private void OnGUICardChoiceNodeSequence()
    {
        if (Window.Button("Reroll choices"))
        {
            Singleton<SpecialNodeHandler>.Instance.cardChoiceSequencer.OnRerollChoices();
        }
    }
}