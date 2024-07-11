using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using DebugMenu.Scripts.Acts;
using DebugMenu.Scripts.Grimora;
using DebugMenu.Scripts.Sequences;
using DiskCardGame;
using InscryptionAPI;
using InscryptionAPI.Nodes;
using UnityEngine;

namespace DebugMenu.Scripts.Utils;
// card choices, card battles
public static partial class Helpers
{
	public static List<BaseTriggerSequence> CurrentSequences
	{
		get
		{
            c_sequences ??= GetCurrentSequences();
            return c_sequences;
		}
	}
    public static List<BaseTriggerSequence> AllSequences
    {
        get
        {
            m_sequences ??= GetAllSequences();
            return m_sequences;
        }
    }

    private static List<BaseTriggerSequence> c_sequences = null;
    private static List<BaseTriggerSequence> m_sequences = null;
	private static List<BaseTriggerSequence> GetAllSequences()
	{
		List<BaseTriggerSequence> list = new();

		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		Dictionary<Assembly, List<Type>> nodeTypes = new();
		Dictionary<Assembly, Type> assemblyToPluginType = new();
		foreach (Assembly a in assemblies)
		{
			foreach (Type type in a.GetTypes())
			{
				if (type.IsAbstract)
					continue;

				if (type.IsSubclassOf(typeof(BaseTriggerSequence)))
				{
					if (type != typeof(APIModdedSequence) && !type.IsAssignableFrom(typeof(SimpleStubSequence)))
					{
						BaseTriggerSequence sequence = (BaseTriggerSequence)Activator.CreateInstance(type);
						list.Add(sequence);
					}
				}
				else if (type.IsSubclassOf(typeof(BaseUnityPlugin)))
				{
					assemblyToPluginType[a] = type;
				}
				else if (type.IsSubclassOf(typeof(NodeData)))
				{
					if(nodeTypes.TryGetValue(a, out List<Type> types))
					{
						types.Add(type);
					}
					else
					{
						nodeTypes[a] = new List<Type>() { type };
					}
				}
			}
		}
		
		// get all types that override SpecialNodeData
		foreach (KeyValuePair<Assembly,List<Type>> pair in nodeTypes)
		{
			foreach (Type type in pair.Value)
			{
				bool hasOverride = false;
				foreach (BaseTriggerSequence sequence in list)
				{
					if (sequence is not SimpleTriggerSequences simpleTriggerSequences)
						continue;

					if (simpleTriggerSequences.NodeDataType == type)
					{
						hasOverride = true;
						break;
					}
				}

				if (hasOverride)
					continue;

				if (assemblyToPluginType.TryGetValue(pair.Key, out Type pluginType))
				{
					// A mod added this node type; get the plugin and find their GUID, then add it to the list
					BaseUnityPlugin plugin = (BaseUnityPlugin)Plugin.Instance.GetComponent(pluginType);
					SimpleStubSequence sequence = new()
					{
						ModGUID = plugin.Info.Metadata.GUID,
						type = type,
						gameState = GameState.SpecialCardSequence
					};
					list.Add(sequence);
				}
				else
				{
					// This is a vanilla node type
					SimpleStubSequence sequence = new()
					{
						ModGUID = null,
						type = type,
						gameState = type.IsAssignableFrom(typeof(CardBattleNodeData)) ? GameState.CardBattle : GameState.SpecialCardSequence
					};
					list.Add(sequence);
				}
			}
		}

		foreach (NewNodeManager.FullNode addedNode in NewNodeManager.NewNodes)
		{
			if (addedNode == null)
				continue;

			APIModdedSequence sequence = new()
			{
				ModGUID = addedNode.guid.IsNullOrWhiteSpace() ? "no guid" : addedNode.guid,
				CustomNodeData = addedNode
			};
			list.Add(sequence);
		}

		list.Sort(SortSequences);
		return list;
	}

	private static int SortSequences(BaseTriggerSequence a, BaseTriggerSequence b)
	{
		// Sort so we have this ordering
        // 1. Vanilla sequences
        // - a. sort by ButtonName
        // 2. Modded sigils
        // - a. sort by GUID
        // - b. sort by ButtonName
        if (!a.ModGUID.IsNullOrWhiteSpace())
		{
			if (!b.ModGUID.IsNullOrWhiteSpace())
			{
                int sortbyGUID = String.Compare(a.ModGUID, b.ModGUID, StringComparison.Ordinal);
				if (sortbyGUID != 0)
					return sortbyGUID;
                
                int sortByButtonName = String.Compare(a.SequenceName, b.SequenceName, StringComparison.Ordinal); // same GUID
                return sortByButtonName;
			}
			return 1;
		}

		if (!b.ModGUID.IsNullOrWhiteSpace())
			return -1;

        // Vanilla sequence - sort by ButtonName
        return String.Compare(a.SequenceName, b.SequenceName, StringComparison.Ordinal);
	}

	private static List<BaseTriggerSequence> GetCurrentSequences()
	{
		List<BaseTriggerSequence> sequences = new(AllSequences);
        if (SaveManager.SaveFile.IsGrimora && GrimoraModHelper.Enabled)
		{
            BaseTriggerSequence rand = sequences.Find(x => x.SequenceName == "3 Random Choice");
            BaseTriggerSequence card = sequences.Find(x => x.SequenceName == "Card Battle");
            BaseTriggerSequence boss = sequences.Find(x => x.SequenceName == "Boss Battle");
            BaseTriggerSequence rare = sequences.Find(x => x.SequenceName == "Choose Rare Card");
			List<BaseTriggerSequence> grimoraSeq = new()
			{
				rand, card, boss, rare
			};
			grimoraSeq.AddRange(sequences.FindAll(x => !string.IsNullOrEmpty(x.ModGUID)));
			return grimoraSeq;
        }
		return AllSequences;
	}
}