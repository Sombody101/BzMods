using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;

namespace FasterPrawnDrill;

[BepInPlugin(GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string GUID = "com.Sombody101.FasterPrawnDrill";

    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded! ({DateTime.Now})");

        DrillConfig.Instance = new(Logger, Config);

        var harmony = new Harmony(GUID);
        harmony.PatchAll();
    }
}
