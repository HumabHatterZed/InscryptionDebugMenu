using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Utils;
using DiskCardGame;
using GBC;
using InscryptionAPI.Card;
using InscryptionAPI.Guid;
using InscryptionAPI.Helpers.Extensions;
using InscryptionAPI.Slots;
using System.Collections;
using UnityEngine;

namespace DebugMenu.Scripts.Popups.DeckEditorPopup;

public class ModifySlotPopup : BaseWindow
{
	public override string PopupName => "Slot Modification";
	public override Vector2 Size => new(240f, 480f);

	private string lastCardSearch = "";
	private List<SlotModificationManager.ModificationType> lastSearchedList = null;
	private Vector2 foundCardListScrollVector = Vector2.zero;

	public CardSlot currentSelection = null;
	List<SlotModificationManager.ModificationType> modificationTypes = null;
	private bool changingSlot = false;

    public override void OnGUI()
	{
        if (GameFlowManager.m_Instance?.CurrentGameState != GameState.CardBattle)
        {
            IsActive = false;
            return;
        }
        if (currentSelection == null)
			return;

		base.OnGUI();

		modificationTypes ??= GuidManager.GetValues<SlotModificationManager.ModificationType>();
		SlotModificationManager.ModificationType type = currentSelection.GetSlotModification();
		string header = currentSelection.IsPlayerSlot ? "Player" : "Opponent";

        LabelHeader($"{header} Slot ({currentSelection.Index})");
		Label("<b>Current Slot Modification:</b> " + type.ToString());
		if (Button("Reset Slot Behaviour", disabled: () => new(() => changingSlot || currentSelection.GetSlotModification() == SlotModificationManager.ModificationType.NoModification)))
			Plugin.Instance.StartCoroutine(ChangeSlotModification(SlotModificationManager.ModificationType.NoModification));

		GUILayout.BeginArea(new Rect(5f, Size.y / 3f, Size.x - 10f, Size.y / 3f * 2));
        OnGUISlotSearcher();
        GUILayout.EndArea();
	}

	private void OnGUISlotSearcher()
	{
		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();
		GUILayout.Label("Slot Mod Finder", Helpers.HeaderLabelStyle());
        GUILayout.EndHorizontal();
		lastCardSearch = GUILayout.TextField(lastCardSearch);
		lastSearchedList = new List<SlotModificationManager.ModificationType>();
		if (lastCardSearch != "")
		{
			if (GetCardByName(lastCardSearch, out var result))
			{
				lastSearchedList.Add(modificationTypes[result]);
			}
			else if (GetCardsThatContain(lastCardSearch, out List<int> results))
			{
				foreach (int item in results)
				{
					lastSearchedList.Add(modificationTypes[item]);
				}
			}
		}
		else
		{
			lastSearchedList = new(modificationTypes);
		}
		FoundCardList();
	}

	private void FoundCardList()
	{
		foundCardListScrollVector = GUILayout.BeginScrollView(foundCardListScrollVector);
		if (lastSearchedList.Count > 0)
		{
			foreach (SlotModificationManager.ModificationType lastSearched in lastSearchedList)
			{
				string display = lastSearched.ToString();

                if (changingSlot || currentSelection.GetSlotModification() == lastSearched)
				{
					GUILayout.Label(display, Helpers.DisabledButtonStyle());
				}
				else if (GUILayout.Button(display))
				{
                    Plugin.Instance.StartCoroutine(ChangeSlotModification(lastSearched));
                }
			}
		}
		else
		{
			GUILayout.Label("No ModificationTypes Found...");
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();
	}

	private IEnumerator ChangeSlotModification(SlotModificationManager.ModificationType modType)
	{
        changingSlot = true;
		yield return currentSelection.SetSlotModification(modType);
        changingSlot = false;
    }
	private bool GetCardsThatContain(string cardName, out List<int> results)
	{
		string lower = cardName.ToLower();
		bool result = false;
		results = new List<int>();
		for (int i = 0; i < modificationTypes.Count; i++)
		{
			SlotModificationManager.ModificationType allType = modificationTypes[i];
			string name = allType.ToString();
			if (name != null && name.ToLower().Contains(lower))
			{
				results.Add(i);
				result = true;
			}
		}

		return result;
	}

	private bool GetCardByName(string cardName, out int index)
	{
		bool exists = false;
		index = -1;
		
		List<SlotModificationManager.ModificationType> allModsCopy = modificationTypes;
		for (int i = 0; i < allModsCopy.Count; i++)
		{
			SlotModificationManager.ModificationType mod = allModsCopy[i];
			if (mod.ToString() == cardName)
			{
				index = i;
				exists = true;
				break;
			}
		}

		return exists;
	}
}