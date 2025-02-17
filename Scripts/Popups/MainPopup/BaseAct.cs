﻿using BepInEx.Logging;
using DebugMenu.Scripts.Popups;
using DebugMenu.Scripts.Sequences;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using System.Collections;
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
        GameFlowManager instance = GameFlowManager.Instance;
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

                if (nodeType == typeof(ChooseRareCardNodeData))
                {
                    OnGUIRareChoiceNodeSequence();
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
    private void OnGUIRareChoiceNodeSequence()
    {
        if (Window.Button("Reroll choices", disabled: () => new(() => rerollingRare)))
        {
            Plugin.Instance.StartCoroutine(RerollRareChoices());
        }
    }

    protected bool rerollingRare = false;

    private IEnumerator RerollRareChoices()
    {
        RareCardChoicesSequencer sequencer = SpecialNodeHandler.Instance.rareCardChoiceSequencer;
        List<CardChoice> list;
        int randSeed = SaveManager.SaveFile.GetCurrentRandomSeed() + UnityEngine.Random.Range(1, 99999);

        rerollingRare = true;

        sequencer.DisableViewDeck();
        sequencer.CleanupMushrooms();
        sequencer.box.GetComponentInChildren<Animator>().Play("close", 0, 0f);
        AudioController.Instance.PlaySound3D("woodbox_close", MixerGroup.TableObjectsSFX, sequencer.box.transform.position);
        yield return new WaitForSeconds(0.1f);
        sequencer.CleanUpCards();
        yield return new WaitForSeconds(0.3f);

        Singleton<ViewManager>.Instance.SwitchToView(sequencer.choicesView);
        sequencer.selectableCards = sequencer.SpawnCards(3, sequencer.box.transform, new Vector3(-1.55f, 0.2f, 0f));
        list = (!AscensionSaveData.Data.ChallengeIsActive(AscensionChallenge.NoBossRares)) ? sequencer.rareChoiceGenerator.GenerateChoices(randSeed) : sequencer.choiceGenerator.GenerateChoices(new CardChoicesNodeData(), randSeed);       
        
        for (int i = 0; i < sequencer.selectableCards.Count; i++)
        {
            sequencer.selectableCards[i].gameObject.SetActive(value: true);
            sequencer.selectableCards[i].ChoiceInfo = list[i];
            sequencer.selectableCards[i].Initialize(list[i].CardInfo, sequencer.OnRewardChosen, sequencer.OnCardFlipped, startFlipped: true, sequencer.OnCardInspected);
            sequencer.selectableCards[i].SetFaceDown(faceDown: true, immediate: true);
            SpecialCardBehaviour[] components = sequencer.selectableCards[i].GetComponents<SpecialCardBehaviour>();
            for (int j = 0; j < components.Length; j++)
            {
                components[j].OnShownForCardChoiceNode();
            }
        }

        sequencer.box.GetComponentInChildren<Animator>().Play("open", 0, 0f);
        AudioController.Instance.PlaySound3D("woodbox_open", MixerGroup.TableObjectsSFX, sequencer.box.transform.position);
        ChallengeActivationUI.TryShowActivation(AscensionChallenge.NoBossRares);
        sequencer.EnableViewDeck(sequencer.viewControlMode, sequencer.basePosition);

        rerollingRare = false;
    }
}