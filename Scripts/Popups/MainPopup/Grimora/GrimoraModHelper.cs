using BepInEx.Bootstrap;

namespace DebugMenu.Scripts.Grimora;

public static partial class GrimoraModHelper
{
    public static bool Enabled => Chainloader.PluginInfos.ContainsKey("arackulele.inscryption.grimoramod");
    internal static void PatchGrimoraMod()
    {
        Plugin.HarmonyInstance.PatchAll(typeof(GrimoraModHelper));
        Plugin.HarmonyInstance.PatchAll(typeof(DetectBoon));
    }

    internal static bool addingMod = false;
}
