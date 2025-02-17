using MagnificusMod;

namespace DebugMenu.Scripts.Magnificus;

public static partial class MagnificusModHelper
{
    internal static bool _enabled;
    public static bool Enabled => _enabled;
    internal static void PatchMagnificuMod()
    {
        Plugin.HarmonyInstance.PatchAll(typeof(MagnificusModHelper));
    }
}
