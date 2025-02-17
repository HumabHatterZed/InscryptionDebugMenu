using DiskCardGame;
using HarmonyLib;
using MagnificusMod;
using System.Collections;
using System.Reflection;

namespace DebugMenu.Scripts.Magnificus;

public static partial class MagnificusModHelper
{
    [HarmonyPostfix, HarmonyPatch(typeof(NavigationZone3D), nameof(NavigationZone3D.ValidEvent))]
    private static void PreventEventTriggering(ref bool __result)
    {
        if (config.isometricMode && ActMagnificus.SkipNextNode)
            __result = false;
    }
}
