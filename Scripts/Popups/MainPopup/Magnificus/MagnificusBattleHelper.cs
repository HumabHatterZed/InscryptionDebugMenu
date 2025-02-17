﻿using DebugMenu.Scripts.Acts;
using DiskCardGame;
using HarmonyLib;
using MagnificusMod;
using System.Collections;
using UnityEngine;

namespace DebugMenu.Scripts.Magnificus;

public static partial class MagnificusModHelper
{
    public static SpellPile SpellPile => GetSpellPile();
    private static SpellPile _spellPile;

    public static List<CardInfo> SpellsInDeck => RunState.Run.playerDeck.Cards.FindAll(x => x.HasTrait(Trait.EatsWarrens));
    public static List<CardInfo> SpellsInDeckCache;
    internal static void RefreshSpells(DebugWindow window)
    {
        window.LabelHeader("Spell Pile");
        if (SpellPile == null)
        {
            window.Label("SpellPile not found!");
            return;
        }

        if (window.Button("Refresh Spells", disabled: () => new(() => SpellsInDeckCache.Count == 0)))
        {
            SpellPile.refreshSpellBookCards(SpellsInDeckCache);
        }

        if (window.Button("Add Spells Back to Deck", disabled: () => new(() => SpellsInDeckCache.Count == 0)))
        {
            foreach (CardInfo info in SpellsInDeckCache)
            {
                if (RunState.Run.playerDeck.Cards.Count(x => x.name == info.name) < SpellsInDeckCache.Count(x => x.name == info.name))
                    RunState.Run.playerDeck.AddCard(info);
            }
        }
    }

    private static SpellPile GetSpellPile()
    {
        if (_spellPile == null)
        {
            _spellPile = TurnManager.Instance.transform.parent.Find("SpellPile").GetComponent<SpellPile>();
        }
        return _spellPile;
    }

    [HarmonyPrefix]
    [HarmonyBefore("silenceman.inscryption.magnificusmod")]
    [HarmonyPatch(typeof(MagnificusCardDrawPiles), nameof(MagnificusCardDrawPiles.Awake))]
    private static bool CacheSpellsBeforeBattle()
    {
        SpellsInDeckCache = new(SpellsInDeck);
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MagnificusMod.Generation), nameof(MagnificusMod.Generation.LifeManagerStuff))]
    private static IEnumerator NegateDamageMagnificus(IEnumerator enumerator, bool toPlayer)
    {
        if (Configs.DisablePlayerDamage && toPlayer)
            yield break;

        if (Configs.DisableOpponentDamage && !toPlayer)
            yield break;

        yield return enumerator;
    }
}
