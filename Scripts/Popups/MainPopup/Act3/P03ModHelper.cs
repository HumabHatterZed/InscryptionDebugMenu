using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using BepInEx.Bootstrap;

namespace DebugMenu.Scripts.Act3;

public static class P03ModHelper
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey("zorro.inscryption.infiniscryption.p03kayceerun");
    public static void PatchP03Mod() => Plugin.HarmonyInstance.PatchAll(typeof(P03ModHelper));

    internal static bool addingMod = false;
}
