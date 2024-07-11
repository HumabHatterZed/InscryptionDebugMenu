using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using BepInEx.Bootstrap;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.All;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GrimoraMod;
using HarmonyLib;
using InscryptionAPI.Helpers;
using InscryptionAPI.Regions;
using UnityEngine;

namespace DebugMenu.Scripts.Grimora;

public static partial class GrimoraModHelper
{
    internal static void OnGUIGravebardCamp(DebugWindow Window)
    {
        GravebardCampSequencer sequencer = GravebardCampSequencer.Instance;
        if (sequencer == null)
            return;

        FieldInfo priceInfo = typeof(GravebardCampSequencer).GetField("price");
        ConfirmStoneButton confirmStone = (ConfirmStoneButton)typeof(GravebardCampSequencer).GetField("confirmStone").GetValue(sequencer);
        Window.LabelBold($"Current price: {(int)priceInfo.GetValue(sequencer)}");

        if (Window.Button("Generate Item", disabled: () => new(() => addingMod || confirmStone.currentState != HighlightedInteractable.State.Interactable || RunState.Run.consumables.Count < 3)))
        {
            addingMod = true;
            Plugin.Instance.StartCoroutine(GenerateItem(sequencer));
        }
        if (Window.Button("Tell Random Story"))
        {
            Plugin.Instance.StartCoroutine(TellRandomStory(sequencer));
        }
    }
    private static IEnumerator GenerateItem(GravebardCampSequencer sequence)
    {
        yield return GravebardGenerateItem(sequence);
        addingMod = false;
    }
    private static IEnumerator TellRandomStory(GravebardCampSequencer sequence)
    {
        yield return GravebardTellStory(sequence);
    }

    [HarmonyReversePatch, HarmonyPatch(typeof(GravebardCampSequencer), "GenerateItem")]
    public static IEnumerator GravebardGenerateItem(GravebardCampSequencer instance) { yield break; }

    [HarmonyReversePatch, HarmonyPatch(typeof(GravebardCampSequencer), "TellRandomStory")]
    public static IEnumerator GravebardTellStory(GravebardCampSequencer instance) { yield break; }
}